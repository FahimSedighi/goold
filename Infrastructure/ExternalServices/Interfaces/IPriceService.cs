using GoldPriceTracker.Domain.Entities;

namespace GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;

/// <summary>
/// Interface for price service.
/// Part of Infrastructure layer - handles external API calls.
/// </summary>
public interface IPriceService
{
    Task<PriceData> GetGoldPriceAsync();
    Task<PriceData> GetDollarPriceAsync();
}

