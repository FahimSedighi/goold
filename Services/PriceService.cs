using System.Net.Http.Json;
using System.Text.Json;
using GoldPriceTracker.Models;
using Microsoft.Extensions.Configuration;

namespace GoldPriceTracker.Services;

public class PriceService : IPriceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PriceService> _logger;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public PriceService(HttpClient httpClient, ILogger<PriceService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        var timeoutSeconds = _configuration.GetValue<int>("ApiSettings:TimeoutSeconds", 10);
        _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        
        // اضافه کردن header برای API
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<PriceData> GetGoldPriceAsync()
    {
        // استفاده از API BrsApi.ir
        try
        {
            var brsApiKey = _configuration["ApiSettings:BrsApiKey"];
            var brsApiUrl = _configuration["ApiSettings:BrsApiUrl"] ?? "https://BrsApi.ir/Api/Market/Gold_Currency.php";
            
            if (string.IsNullOrEmpty(brsApiKey) || brsApiKey == "YOUR_API_KEY_HERE" || brsApiKey == "YourApiKey")
            {
                _logger.LogError("❌ API Key برای BrsApi تنظیم نشده است! لطفاً در appsettings.json تنظیم کنید.");
                return GetFallbackGoldPrice();
            }
            
            // ساخت URL - فقط یک بار key را اضافه می‌کنیم
            var apiUrl = $"{brsApiUrl}?key={brsApiKey}";
            _logger.LogInformation($"🔍 در حال دریافت قیمت طلا از BrsApi.ir");
            _logger.LogInformation($"📡 URL: {brsApiUrl}?key=***");
            
            var response = await _httpClient.GetAsync(apiUrl);
            var jsonString = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"📡 Status Code: {response.StatusCode}");
            _logger.LogInformation($"📄 پاسخ (اول 500 کاراکتر): {jsonString.Substring(0, Math.Min(500, jsonString.Length))}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError($"❌ خطای 401 Unauthorized - API Key نامعتبر است یا منقضی شده است");
                _logger.LogError($"❌ محتوای پاسخ: {jsonString}");
                return GetFallbackGoldPrice();
            }
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // پردازش JSON - ابتدا ساختار واقعی را بررسی می‌کنیم
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;
                    
                    _logger.LogInformation($"🔑 کلیدهای موجود در JSON: {string.Join(", ", root.EnumerateObject().Select(p => p.Name))}");
                    
                    // تلاش برای deserialize با مدل
                    var apiData = JsonSerializer.Deserialize<BrsApiResponse>(jsonString, _jsonOptions);
                    
                    if (apiData?.Gold != null && apiData.Gold.Price.HasValue && apiData.Gold.Price.Value > 0)
                    {
                        _logger.LogInformation($"✅ قیمت طلا از BrsApi دریافت شد: {apiData.Gold.Price}");
                        
                        return new PriceData
                        {
                            Name = "طلا",
                            Price = apiData.Gold.Price.Value,
                            Change = apiData.Gold.Change ?? 0,
                            ChangePercent = apiData.Gold.ChangePercent ?? 0,
                            LastUpdate = apiData.Gold.LastUpdate ?? DateTime.Now
                        };
                    }
                    
                    // اگر ساختار متفاوت است، به صورت دستی پردازش می‌کنیم
                    if (root.TryGetProperty("gold", out var goldElement))
                    {
                        decimal price = 0;
                        decimal change = 0;
                        decimal changePercent = 0;
                        
                        if (goldElement.TryGetProperty("price", out var priceElement))
                        {
                            if (priceElement.ValueKind == JsonValueKind.Number)
                                price = priceElement.GetDecimal();
                            else if (priceElement.ValueKind == JsonValueKind.String && decimal.TryParse(priceElement.GetString(), out var parsedPrice))
                                price = parsedPrice;
                        }
                        
                        if (price > 0)
                        {
                            if (goldElement.TryGetProperty("change", out var changeElement))
                            {
                                if (changeElement.ValueKind == JsonValueKind.Number)
                                    change = changeElement.GetDecimal();
                                else if (changeElement.ValueKind == JsonValueKind.String && decimal.TryParse(changeElement.GetString(), out var parsedChange))
                                    change = parsedChange;
                            }
                            
                            if (goldElement.TryGetProperty("changePercent", out var changePercentElement))
                            {
                                if (changePercentElement.ValueKind == JsonValueKind.Number)
                                    changePercent = changePercentElement.GetDecimal();
                                else if (changePercentElement.ValueKind == JsonValueKind.String && decimal.TryParse(changePercentElement.GetString(), out var parsedChangePercent))
                                    changePercent = parsedChangePercent;
                            }
                            
                            _logger.LogInformation($"✅ قیمت طلا از BrsApi دریافت شد (دستی): {price}");
                            
                            return new PriceData
                            {
                                Name = "طلا",
                                Price = price,
                                Change = change,
                                ChangePercent = changePercent,
                                LastUpdate = DateTime.Now
                            };
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "❌ خطا در پردازش JSON BrsApi");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ خطا در دریافت از BrsApi");
        }
        

        _logger.LogWarning("⚠️ بازگشت به داده‌های نمونه");
        return GetFallbackGoldPrice();
    }

    public async Task<PriceData> GetDollarPriceAsync()
    {
        // استفاده از API BrsApi.ir
        try
        {
            var brsApiKey = _configuration["ApiSettings:BrsApiKey"];
            var brsApiUrl = _configuration["ApiSettings:BrsApiUrl"] ?? "https://BrsApi.ir/Api/Market/Gold_Currency.php";
            
            if (string.IsNullOrEmpty(brsApiKey) || brsApiKey == "YOUR_API_KEY_HERE" || brsApiKey == "YourApiKey")
            {
                _logger.LogError("❌ API Key برای BrsApi تنظیم نشده است! لطفاً در appsettings.json تنظیم کنید.");
                return GetFallbackDollarPrice();
            }
            
            // ساخت URL - فقط یک بار key را اضافه می‌کنیم
            var apiUrl = $"{brsApiUrl}?key={brsApiKey}";
            _logger.LogInformation($"🔍 در حال دریافت قیمت دلار از BrsApi.ir");
            _logger.LogInformation($"📡 URL: {brsApiUrl}?key=***");
            
            var response = await _httpClient.GetAsync(apiUrl);
            var jsonString = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"📡 Status Code: {response.StatusCode}");
            _logger.LogInformation($"📄 پاسخ (اول 500 کاراکتر): {jsonString.Substring(0, Math.Min(500, jsonString.Length))}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError($"❌ خطای 401 Unauthorized - API Key نامعتبر است یا منقضی شده است");
                _logger.LogError($"❌ محتوای پاسخ: {jsonString}");
                return GetFallbackDollarPrice();
            }
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // پردازش JSON - ابتدا ساختار واقعی را بررسی می‌کنیم
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;
                    
                    _logger.LogInformation($"🔑 کلیدهای موجود در JSON: {string.Join(", ", root.EnumerateObject().Select(p => p.Name))}");
                    
                    // تلاش برای deserialize با مدل
                    var apiData = JsonSerializer.Deserialize<BrsApiResponse>(jsonString, _jsonOptions);
                    
                    if (apiData?.Currency?.Usd != null && apiData.Currency.Usd.Price.HasValue && apiData.Currency.Usd.Price.Value > 0)
                    {
                        _logger.LogInformation($"✅ قیمت دلار از BrsApi دریافت شد: {apiData.Currency.Usd.Price}");
                        
                        return new PriceData
                        {
                            Name = "دلار",
                            Price = apiData.Currency.Usd.Price.Value,
                            Change = apiData.Currency.Usd.Change ?? 0,
                            ChangePercent = apiData.Currency.Usd.ChangePercent ?? 0,
                            LastUpdate = apiData.Currency.Usd.LastUpdate ?? DateTime.Now
                        };
                    }
                    
                    // اگر ساختار متفاوت است، به صورت دستی پردازش می‌کنیم
                    if (root.TryGetProperty("currency", out var currencyElement))
                    {
                        if (currencyElement.TryGetProperty("usd", out var usdElement))
                        {
                            decimal price = 0;
                            decimal change = 0;
                            decimal changePercent = 0;
                            
                            if (usdElement.TryGetProperty("price", out var priceElement))
                            {
                                if (priceElement.ValueKind == JsonValueKind.Number)
                                    price = priceElement.GetDecimal();
                                else if (priceElement.ValueKind == JsonValueKind.String && decimal.TryParse(priceElement.GetString(), out var parsedPrice))
                                    price = parsedPrice;
                            }
                            
                            if (price > 0)
                            {
                                if (usdElement.TryGetProperty("change", out var changeElement))
                                {
                                    if (changeElement.ValueKind == JsonValueKind.Number)
                                        change = changeElement.GetDecimal();
                                    else if (changeElement.ValueKind == JsonValueKind.String && decimal.TryParse(changeElement.GetString(), out var parsedChange))
                                        change = parsedChange;
                                }
                                
                                if (usdElement.TryGetProperty("changePercent", out var changePercentElement))
                                {
                                    if (changePercentElement.ValueKind == JsonValueKind.Number)
                                        changePercent = changePercentElement.GetDecimal();
                                    else if (changePercentElement.ValueKind == JsonValueKind.String && decimal.TryParse(changePercentElement.GetString(), out var parsedChangePercent))
                                        changePercent = parsedChangePercent;
                                }
                                
                                _logger.LogInformation($"✅ قیمت دلار از BrsApi دریافت شد (دستی): {price}");
                                
                                return new PriceData
                                {
                                    Name = "دلار",
                                    Price = price,
                                    Change = change,
                                    ChangePercent = changePercent,
                                    LastUpdate = DateTime.Now
                                };
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "❌ خطا در پردازش JSON BrsApi");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ خطا در دریافت از BrsApi");
        }
        

        _logger.LogWarning("⚠️ بازگشت به داده‌های نمونه");
        return GetFallbackDollarPrice();
    }

    private PriceData GetFallbackGoldPrice()
    {
        _logger.LogWarning("استفاده از داده‌های نمونه برای قیمت طلا");
        return new PriceData
        {
            Name = "طلای 18 عیار",
            Price = 2500000,
            Change = 50000,
            ChangePercent = (decimal)2.04,
            LastUpdate = DateTime.Now
        };
    }

    private PriceData GetFallbackDollarPrice()
    {
        _logger.LogWarning("استفاده از داده‌های نمونه برای قیمت دلار");
        return new PriceData
        {
            Name = "دلار",
            Price = 42000,
            Change = -500,
            ChangePercent = (decimal)-1.18,
            LastUpdate = DateTime.Now
        };
    }

}
