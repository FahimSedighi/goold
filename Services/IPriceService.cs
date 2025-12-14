using GoldPriceTracker.Models;

namespace GoldPriceTracker.Services;

public interface IPriceService
{
    Task<PriceData> GetGoldPriceAsync();
    Task<PriceData> GetDollarPriceAsync();
}





