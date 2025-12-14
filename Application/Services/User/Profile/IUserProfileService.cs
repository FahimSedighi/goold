using GoldPriceTracker.Shared.Contracts.DTOs;

namespace GoldPriceTracker.Application.Services.User.Profile;

/// <summary>
/// Service interface for user profile operations.
/// Part of Application layer - feature-based organization (User/Profile feature).
/// </summary>
public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileAsync(int userId);
    Task<UserProfile> UpdateUserProfileAsync(int userId, UpdateProfileRequest request);
    Task<AccountSummary> GetAccountSummaryAsync(int userId);
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int userId);
    void AddActivity(int userId, string action, string description, string icon);
}

