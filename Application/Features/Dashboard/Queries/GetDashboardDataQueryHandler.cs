using GoldPriceTracker.Application.Common.Interfaces;
using GoldPriceTracker.Application.Interfaces.Repositories;
using GoldPriceTracker.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GoldPriceTracker.Application.Features.Dashboard.Queries;

/// <summary>
/// Handler for GetDashboardDataQuery.
/// Implements CQRS pattern - handles the query logic.
/// </summary>
public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, UserDashboardViewModel>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetDashboardDataQueryHandler> _logger;

    public GetDashboardDataQueryHandler(
        IUserRepository userRepository,
        ILogger<GetDashboardDataQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDashboardViewModel> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting dashboard data for userId: {request.UserId}");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User not found for userId: {request.UserId}");
        }

        // Build dashboard view model
        // Note: This is simplified - in a real app, you'd have separate services for profile, summary, activities
        var profile = new UserProfile
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.Username,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        };

        var summary = new AccountSummary
        {
            TotalLogins = 0, // TODO: Implement activity tracking
            LastLoginDate = user.LastLoginAt,
            MemberSince = user.CreatedAt.ToString("yyyy/MM/dd"),
            AccountStatus = user.IsActive ? "فعال" : "غیرفعال"
        };

        return new UserDashboardViewModel
        {
            Profile = profile,
            Summary = summary,
            RecentActivities = new List<RecentActivity>()
        };
    }
}

