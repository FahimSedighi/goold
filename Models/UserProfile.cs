using System.ComponentModel.DataAnnotations;

namespace GoldPriceTracker.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateProfileRequest
{
    [MaxLength(100)]
    public string? FullName { get; set; }
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
}

public class UserDashboardViewModel
{
    public UserProfile Profile { get; set; } = new();
    public AccountSummary Summary { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
}

public class AccountSummary
{
    public int TotalLogins { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string MemberSince { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = "فعال";
}

public class RecentActivity
{
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
}

