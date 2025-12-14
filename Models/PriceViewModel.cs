namespace GoldPriceTracker.Models;

public class PriceViewModel
{
    public PriceData GoldPrice { get; set; } = new();
    public PriceData CoinPrice { get; set; } = new();
    public PriceData DollarPrice { get; set; } = new();
    public LoginViewModel LoginModel { get; set; } = new();
}





