using GoldPriceTracker.Domain.Entities;

namespace GoldPriceTracker.Shared.Contracts.DTOs;

public class PriceViewModel
{
    public PriceData GoldPrice { get; set; } = new();
    public PriceData CoinPrice { get; set; } = new();
    public PriceData DollarPrice { get; set; } = new();
}

