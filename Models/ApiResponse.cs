using System.Text.Json.Serialization;

namespace GoldPriceTracker.Models;

public class NerkhApiResponse
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("change")]
    public decimal? Change { get; set; }
    
    [JsonPropertyName("changePercent")]
    public decimal? ChangePercent { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public class MetalsApiResponse
{
    public bool Success { get; set; }
    public long Timestamp { get; set; }
    public string Base { get; set; } = string.Empty;

    public Dictionary<string, decimal> Rates { get; set; }
        = new();
}

// مدل برای API navasan.tech
public class NavasanCurrencyItem
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonPropertyName("change")]
    public decimal Change { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}

public class NavasanApiResponse
{
    // دلار هرات نقدی خرید
    [JsonPropertyName("harat_naghdi_buy")]
    public NavasanCurrencyItem? HaratNaghdiBuy { get; set; }
    
    // دلار هرات نقدی فروش
    [JsonPropertyName("harat_naghdi_sell")]
    public NavasanCurrencyItem? HaratNaghdiSell { get; set; }
    
    // طلای 18 عیار
    [JsonPropertyName("geram18")]
    public NavasanCurrencyItem? Geram18 { get; set; }
    
    // طلای 24 عیار
    [JsonPropertyName("geram24")]
    public NavasanCurrencyItem? Geram24 { get; set; }
    
    // برای سایر ارزها
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}

// مدل برای API dailyCurrency
public class NavasanDailyCurrencyItem
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string? Value { get; set; }
    
    [JsonPropertyName("change")]
    public string? Change { get; set; }
}

// مدل برای API BrsApi.ir
public class BrsApiResponse
{
    [JsonPropertyName("gold")]
    public BrsGoldData? Gold { get; set; }
    
    [JsonPropertyName("currency")]
    public BrsCurrencyData? Currency { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class BrsGoldData
{
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    
    [JsonPropertyName("change")]
    public decimal? Change { get; set; }
    
    [JsonPropertyName("changePercent")]
    public decimal? ChangePercent { get; set; }
    
    [JsonPropertyName("lastUpdate")]
    public DateTime? LastUpdate { get; set; }
}

public class BrsCurrencyData
{
    [JsonPropertyName("usd")]
    public BrsCurrencyItem? Usd { get; set; }
}

public class BrsCurrencyItem
{
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    
    [JsonPropertyName("change")]
    public decimal? Change { get; set; }
    
    [JsonPropertyName("changePercent")]
    public decimal? ChangePercent { get; set; }
    
    [JsonPropertyName("lastUpdate")]
    public DateTime? LastUpdate { get; set; }
}

