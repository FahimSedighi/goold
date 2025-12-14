using GoldPriceTracker.Models;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Services;

/// <summary>
/// Service for managing user dashboard data.
/// This service follows clean architecture principles and is designed to be easily extensible.
/// </summary>
public class UserDashboardService : IUserDashboardService
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserDashboardService> _logger;

    public UserDashboardService(
        IUserProfileService userProfileService,
        ILogger<UserDashboardService> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    /// <summary>
    /// Gets complete dashboard data for a user.
    /// This method aggregates data from multiple sources to provide a complete dashboard view.
    /// </summary>
    public async Task<UserDashboardViewModel> GetDashboardDataAsync(int userId)
    {
        try
        {
            _logger.LogInformation($"Getting dashboard data for userId: {userId}");

            var profile = await GetUserProfileAsync(userId);
            if (profile == null)
            {
                _logger.LogWarning($"Profile not found for userId: {userId}");
                throw new InvalidOperationException($"User profile not found for userId: {userId}");
            }

            var summary = await GetAccountSummaryAsync(userId);
            var activities = await GetRecentActivitiesAsync(userId);

            return new UserDashboardViewModel
            {
                Profile = profile,
                Summary = summary,
                RecentActivities = activities
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting dashboard data for userId: {userId}");
            throw;
        }
    }

    /// <summary>
    /// Gets user profile information.
    /// Delegates to UserProfileService to maintain separation of concerns.
    /// </summary>
    public Task<UserProfile?> GetUserProfileAsync(int userId)
    {
        return _userProfileService.GetUserProfileAsync(userId);
    }

    /// <summary>
    /// Gets account summary statistics.
    /// Delegates to UserProfileService to maintain separation of concerns.
    /// </summary>
    public Task<AccountSummary> GetAccountSummaryAsync(int userId)
    {
        return _userProfileService.GetAccountSummaryAsync(userId);
    }

    /// <summary>
    /// Gets recent user activities.
    /// Delegates to UserProfileService to maintain separation of concerns.
    /// </summary>
    public Task<List<RecentActivity>> GetRecentActivitiesAsync(int userId)
    {
        return _userProfileService.GetRecentActivitiesAsync(userId);
    }

    /// <summary>
    /// Updates user profile information.
    /// Delegates to UserProfileService to maintain separation of concerns.
    /// </summary>
    public Task<UserProfile> UpdateUserProfileAsync(int userId, UpdateProfileRequest request)
    {
        return _userProfileService.UpdateUserProfileAsync(userId, request);
    }

    // Future extensibility methods can be added here:
    // - GetNotificationsAsync
    // - GetUserPreferencesAsync
    // - GetUserRolesAsync
    // - GetDashboardWidgetsAsync
    // etc.
}

