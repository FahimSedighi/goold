using System.Text.Json;
using System.Text.Json.Serialization;
using GoldPriceTracker.Domain.Entities;
using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Infrastructure.ExternalServices;

/// <summary>
/// Service for fetching price data from external APIs.
/// Part of Infrastructure layer - handles external service integration.
/// </summary>
public class PriceService : IPriceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PriceService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PriceService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PriceService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GoldPriceTracker/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<PriceData> GetGoldPriceAsync()
    {
        // Implementation from existing PriceService
        // This will be refactored to use proper API integration
        try
        {
            var brsApiKey = _configuration["ApiSettings:BrsApiKey"];
            var brsApiUrl = _configuration["ApiSettings:BrsApiUrl"] ?? "https://BrsApi.ir/Api/Market/Gold_Currency.php";
            
            if (string.IsNullOrEmpty(brsApiKey) || brsApiKey == "YOUR_API_KEY_HERE" || brsApiKey == "YourApiKey")
            {
                _logger.LogError("❌ API Key برای BrsApi تنظیم نشده است!");
                return GetFallbackGoldPrice();
            }
            
            var apiUrl = $"{brsApiUrl}?key={brsApiKey}";
            _logger.LogInformation($"🔍 در حال دریافت قیمت طلا از BrsApi.ir");
            
            var response = await _httpClient.GetAsync(apiUrl);
            var jsonString = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;
                    
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
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "خطا در پردازش JSON BrsApi");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ خطا در دریافت قیمت طلا");
        }

        return GetFallbackGoldPrice();
    }

    public async Task<PriceData> GetDollarPriceAsync()
    {
        // Similar implementation for dollar price
        try
        {
            var brsApiKey = _configuration["ApiSettings:BrsApiKey"];
            var brsApiUrl = _configuration["ApiSettings:BrsApiUrl"] ?? "https://BrsApi.ir/Api/Market/Gold_Currency.php";
            
            if (string.IsNullOrEmpty(brsApiKey) || brsApiKey == "YOUR_API_KEY_HERE" || brsApiKey == "YourApiKey")
            {
                return GetFallbackDollarPrice();
            }
            
            var apiUrl = $"{brsApiUrl}?key={brsApiKey}";
            var response = await _httpClient.GetAsync(apiUrl);
            var jsonString = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var apiData = JsonSerializer.Deserialize<BrsApiResponse>(jsonString, _jsonOptions);
                    
                    if (apiData?.Currency != null && apiData.Currency.Price.HasValue && apiData.Currency.Price.Value > 0)
                    {
                        return new PriceData
                        {
                            Name = "دلار",
                            Price = apiData.Currency.Price.Value,
                            Change = apiData.Currency.Change ?? 0,
                            ChangePercent = apiData.Currency.ChangePercent ?? 0,
                            LastUpdate = apiData.Currency.LastUpdate ?? DateTime.Now
                        };
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "خطا در پردازش JSON برای دلار");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ خطا در دریافت قیمت دلار");
        }

        return GetFallbackDollarPrice();
    }

    private PriceData GetFallbackGoldPrice()
    {
        _logger.LogWarning("⚠️ استفاده از داده‌های نمونه برای قیمت طلا");
        return new PriceData
        {
            Name = "طلای 18 عیار",
            Price = 2500000,
            Change = 50000,
            ChangePercent = 2.04m,
            LastUpdate = DateTime.Now
        };
    }

    private PriceData GetFallbackDollarPrice()
    {
        _logger.LogWarning("⚠️ استفاده از داده‌های نمونه برای قیمت دلار");
        return new PriceData
        {
            Name = "دلار آمریکا",
            Price = 42000,
            Change = 500,
            ChangePercent = 1.20m,
            LastUpdate = DateTime.Now
        };
    }
}

// API Response Models
internal class BrsApiResponse
{
    public BrsGoldItem? Gold { get; set; }
    public BrsCurrencyItem? Currency { get; set; }
}

internal class BrsGoldItem
{
    public decimal? Price { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePercent { get; set; }
    public DateTime? LastUpdate { get; set; }
}

internal class BrsCurrencyItem
{
    public decimal? Price { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePercent { get; set; }
    public DateTime? LastUpdate { get; set; }
}

