namespace GoldPriceTracker.Application.Services.Common;

/// <summary>
/// Service for date/time operations.
/// Part of Application layer - Common services for cross-cutting concerns.
/// </summary>
public interface IDateTimeService
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

/// <summary>
/// Implementation of date/time service.
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}

