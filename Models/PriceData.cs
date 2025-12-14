namespace GoldPriceTracker.Models;

public class PriceData
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public DateTime LastUpdate { get; set; }
}





