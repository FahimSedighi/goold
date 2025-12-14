using GoldPriceTracker.Application.Interfaces.Repositories;
using GoldPriceTracker.Shared.Contracts.DTOs;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Application.Services.User.Profile;

/// <summary>
/// Service for managing user profiles.
/// Part of Application layer - feature-based organization (User/Profile feature).
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserProfileService> _logger;
    
    // In-memory storage for activities (replace with database in production)
    private static readonly Dictionary<int, List<RecentActivity>> _activities = new();

    public UserProfileService(IUserRepository userRepository, ILogger<UserProfileService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserProfile?> GetUserProfileAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            return new UserProfile
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.Username, // Default to username, can be extended
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user profile for userId: {userId}");
            return null;
        }
    }

    public async Task<UserProfile> UpdateUserProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // In a real application, update the database
        // For now, we'll just log the update
        _logger.LogInformation($"Updating profile for user {userId}: FullName={request.FullName}, PhoneNumber={request.PhoneNumber}");

        // Add activity
        AddActivity(userId, "profile_update", "پروفایل به‌روزرسانی شد", "bi-person-check");

        var profile = await GetUserProfileAsync(userId);
        if (profile == null)
            throw new InvalidOperationException("Failed to retrieve updated profile");

        // Update profile fields (in real app, these would be saved to database)
        if (!string.IsNullOrWhiteSpace(request.FullName))
            profile.FullName = request.FullName;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            profile.PhoneNumber = request.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(request.Email))
            profile.Email = request.Email;

        return profile;
    }

    public async Task<AccountSummary> GetAccountSummaryAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var activities = await GetRecentActivitiesAsync(userId);
        var loginCount = activities.Count(a => a.Action == "login");

        return new AccountSummary
        {
            TotalLogins = loginCount,
            LastLoginDate = user.LastLoginAt,
            MemberSince = user.CreatedAt.ToString("yyyy/MM/dd"),
            AccountStatus = user.IsActive ? "فعال" : "غیرفعال"
        };
    }

    public Task<List<RecentActivity>> GetRecentActivitiesAsync(int userId)
    {
        if (!_activities.ContainsKey(userId))
        {
            _activities[userId] = new List<RecentActivity>();
        }

        var activities = _activities[userId]
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();

        return Task.FromResult(activities);
    }

    public void AddActivity(int userId, string action, string description, string icon)
    {
        if (!_activities.ContainsKey(userId))
        {
            _activities[userId] = new List<RecentActivity>();
        }

        _activities[userId].Add(new RecentActivity
        {
            Action = action,
            Description = description,
            Timestamp = DateTime.UtcNow,
            Icon = icon
        });

        // Keep only last 50 activities per user
        if (_activities[userId].Count > 50)
        {
            _activities[userId] = _activities[userId]
                .OrderByDescending(a => a.Timestamp)
                .Take(50)
                .ToList();
        }
    }
}

