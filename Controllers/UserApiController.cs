using System.Security.Claims;
using GoldPriceTracker.Models;
using GoldPriceTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserApiController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserApiController> _logger;

    public UserApiController(IUserProfileService userProfileService, ILogger<UserApiController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var profile = await _userProfileService.GetUserProfileAsync(userId.Value);
            if (profile == null)
            {
                return NotFound(new { success = false, message = "Profile not found" });
            }

            return Ok(new { success = true, profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { success = false, message = "خطا در دریافت اطلاعات کاربر" });
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "اطلاعات نامعتبر", errors = ModelState });
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var updatedProfile = await _userProfileService.UpdateUserProfileAsync(userId.Value, request);
            
            return Ok(new { success = true, message = "پروفایل با موفقیت به‌روزرسانی شد", profile = updatedProfile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new { success = false, message = "خطا در به‌روزرسانی پروفایل" });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetAccountSummary()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var summary = await _userProfileService.GetAccountSummaryAsync(userId.Value);
            return Ok(new { success = true, summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account summary");
            return StatusCode(500, new { success = false, message = "خطا در دریافت خلاصه حساب" });
        }
    }

    [HttpGet("activities")]
    public async Task<IActionResult> GetRecentActivities()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var activities = await _userProfileService.GetRecentActivitiesAsync(userId.Value);
            return Ok(new { success = true, activities });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activities");
            return StatusCode(500, new { success = false, message = "خطا در دریافت فعالیت‌های اخیر" });
        }
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

