using GoldPriceTracker.Shared.Contracts.DTOs;

namespace GoldPriceTracker.Application.Services.Dashboard.Interfaces;

/// <summary>
/// Service interface for user dashboard operations.
/// This service is designed to be modular and easily extensible for future features.
/// Part of Application layer - feature-based organization.
/// </summary>
public interface IUserDashboardService
{
    /// <summary>
    /// Gets complete dashboard data for a user including profile, summary, and activities.
    /// </summary>
    Task<UserDashboardViewModel> GetDashboardDataAsync(int userId);

    /// <summary>
    /// Gets user profile information for the dashboard.
    /// </summary>
    Task<UserProfile?> GetUserProfileAsync(int userId);

    /// <summary>
    /// Gets account summary statistics.
    /// </summary>
    Task<AccountSummary> GetAccountSummaryAsync(int userId);

    /// <summary>
    /// Gets recent user activities.
    /// </summary>
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int userId);

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    Task<UserProfile> UpdateUserProfileAsync(int userId, UpdateProfileRequest request);
}

