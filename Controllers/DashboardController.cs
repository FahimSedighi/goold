 using System.Security.Claims;
using GoldPriceTracker.Models;
using GoldPriceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IUserProfileService _userProfileService;
    private readonly IAuthService _authService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IUserProfileService userProfileService,
        IAuthService authService,
        ILogger<DashboardController> logger)
    {
        _userProfileService = userProfileService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var profile = await _userProfileService.GetUserProfileAsync(userId.Value);
            if (profile == null)
            {
                _logger.LogWarning($"Profile not found for userId: {userId}");
                return RedirectToAction("Index", "Home");
            }

            var summary = await _userProfileService.GetAccountSummaryAsync(userId.Value);
            var activities = await _userProfileService.GetRecentActivitiesAsync(userId.Value);

            var viewModel = new UserDashboardViewModel
            {
                Profile = profile,
                Summary = summary,
                RecentActivities = activities
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var updatedProfile = await _userProfileService.UpdateUserProfileAsync(userId.Value, request);
            
            return Json(new { success = true, message = "پروفایل با موفقیت به‌روزرسانی شد", profile = updatedProfile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return Json(new { success = false, message = "خطا در به‌روزرسانی پروفایل" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("RefreshToken");
        
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            // Add logout activity
            if (_userProfileService is UserProfileService profileService)
            {
                profileService.AddActivity(userId.Value, "logout", "خروج از سیستم", "bi-box-arrow-right");
            }
        }

        return RedirectToAction("Index", "Home");
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}

