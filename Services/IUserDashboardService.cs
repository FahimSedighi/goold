using GoldPriceTracker.Models;

namespace GoldPriceTracker.Services;

/// <summary>
/// Service interface for user dashboard operations.
/// This service is designed to be modular and easily extensible for future features.
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

    // Future extensibility methods (commented for reference):
    // Task<List<Notification>> GetNotificationsAsync(int userId);
    // Task<UserPreferences> GetUserPreferencesAsync(int userId);
    // Task<List<UserRole>> GetUserRolesAsync(int userId);
}

