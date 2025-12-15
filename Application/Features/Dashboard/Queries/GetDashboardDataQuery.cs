using GoldPriceTracker.Application.Common.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;

namespace GoldPriceTracker.Application.Features.Dashboard.Queries;

/// <summary>
/// Query for getting dashboard data.
/// Implements CQRS pattern for read operations.
/// </summary>
public class GetDashboardDataQuery : IQuery<UserDashboardViewModel>
{
    public Guid UserId { get; set; }
}
