using GoldPriceTracker.Application.Common.Interfaces;
using GoldPriceTracker.Application.Services.Dashboard.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Application.Features.Dashboard.Queries;

/// <summary>
/// Handler for GetDashboardDataQuery.
/// Implements CQRS pattern - handles the query logic.
/// Uses feature-based services from Application/Services.
/// </summary>
public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, UserDashboardViewModel>
{
    private readonly IUserDashboardService _dashboardService;
    private readonly ILogger<GetDashboardDataQueryHandler> _logger;

    public GetDashboardDataQueryHandler(
        IUserDashboardService dashboardService,
        ILogger<GetDashboardDataQueryHandler> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<UserDashboardViewModel> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting dashboard data for userId: {request.UserId}");

        // Use feature-based dashboard service
        return await _dashboardService.GetDashboardDataAsync(request.UserId);
    }
}

