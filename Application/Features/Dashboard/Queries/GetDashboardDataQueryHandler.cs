using GoldPriceTracker.Application.Common.Interfaces;
using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Application.Features.Dashboard.Queries;

/// <summary>
/// Handler for GetDashboardDataQuery.
/// Implements CQRS pattern - handles the query logic.
/// Aggregates data from external microservices (UserService).
/// </summary>
public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, UserDashboardViewModel>
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<GetDashboardDataQueryHandler> _logger;

    public GetDashboardDataQueryHandler(
        IUserServiceClient userServiceClient,
        ILogger<GetDashboardDataQueryHandler> logger)
    {
        _userServiceClient = userServiceClient;
        _logger = logger;
    }

    public async Task<UserDashboardViewModel> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting dashboard data for userId: {request.UserId}");

        // Get user profile from UserService
        var userProfile = await _userServiceClient.GetUserByIdAsync(request.UserId);
        
        if (userProfile == null)
        {
            _logger.LogWarning($"User not found for userId: {request.UserId}");
            throw new InvalidOperationException($"User not found for userId: {request.UserId}");
        }

        // Build dashboard view model
        // Note: Activities and summary would come from UserService in a real implementation
        return new UserDashboardViewModel
        {
            Profile = new UserProfile
            {
                Id = (int)userProfile.Id.GetHashCode(), // Temporary conversion - should be updated when UserProfile uses Guid
                Username = userProfile.Username,
                Email = userProfile.Email ?? string.Empty,
                FullName = userProfile.Username,
                CreatedAt = userProfile.CreatedAt,
                LastLoginAt = null,
                IsActive = userProfile.IsActive
            },
            Summary = new AccountSummary
            {
                TotalLogins = 0,
                LastLoginDate = null,
                MemberSince = userProfile.CreatedAt.ToString("yyyy/MM/dd"),
                AccountStatus = userProfile.IsActive ? "فعال" : "غیرفعال"
            },
            RecentActivities = new List<RecentActivity>()
        };
    }
}
