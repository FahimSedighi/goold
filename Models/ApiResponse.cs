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
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("change")]
    public decimal? Change { get; set; }
    
    [JsonPropertyName("changePercent")]
    public decimal? ChangePercent { get; set; }
}



