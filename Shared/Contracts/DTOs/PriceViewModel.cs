using GoldPriceTracker.Domain.Entities;

namespace GoldPriceTracker.Shared.Contracts.DTOs;

public class PriceViewModel
{
    public PriceData GoldPrice { get; set; } = new();
    public PriceData CoinPrice { get; set; } = new();
    public PriceData DollarPrice { get; set; } = new();
    public LoginViewModel LoginModel { get; set; } = new();
}

public class LoginViewModel
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

