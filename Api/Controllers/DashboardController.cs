using System.Security.Claims;
using GoldPriceTracker.Application.Features.Dashboard.Queries;
using GoldPriceTracker.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Api.Controllers;

/// <summary>
/// Dashboard controller - thin API layer.
/// Uses CQRS pattern via MediatR for queries.
/// Aggregates data from external microservices.
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Main dashboard page - requires authentication via JWT.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthenticated user attempted to access dashboard");
                return RedirectToAction("Index", "Home");
            }

            // Use CQRS query to get dashboard data
            var query = new GetDashboardDataQuery { UserId = userId.Value };
            var viewModel = await _mediator.Send(query);

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

            // TODO: Implement UpdateProfileCommand - would call UserService
            return Json(new { success = false, message = "Not implemented yet" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return Json(new { success = false, message = "خطا در به‌روزرسانی پروفایل" });
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("RefreshToken");
        return RedirectToAction("Index", "Home");
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
