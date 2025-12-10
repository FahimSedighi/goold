namespace GoldPriceTracker.Models;

public class PriceViewModel
{
    public PriceData GoldPrice { get; set; } = new();
    public PriceData DollarPrice { get; set; } = new();
}

