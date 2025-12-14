using GoldPriceTracker.Models;

namespace GoldPriceTracker.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileAsync(int userId);
    Task<UserProfile> UpdateUserProfileAsync(int userId, UpdateProfileRequest request);
    Task<AccountSummary> GetAccountSummaryAsync(int userId);
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int userId);
}

