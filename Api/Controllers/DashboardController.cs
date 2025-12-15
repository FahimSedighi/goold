using GoldPriceTracker.Application.Features.Dashboard.Queries;
using GoldPriceTracker.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Api.Controllers;

/// <summary>
/// Dashboard controller - thin API layer.
/// Uses CQRS pattern via MediatR for queries.
/// Aggregates data from external microservices.
/// Note: Authentication is handled by AuthService. This controller is open for gateway-level protection.
/// </summary>
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
    /// Main dashboard page.
    /// Note: User ID should be passed via query parameter or header from gateway.
    /// </summary>
    public async Task<IActionResult> Index([FromQuery] Guid? userId)
    {
        try
        {
            if (userId == null)
            {
                _logger.LogWarning("Dashboard accessed without userId");
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
    public async Task<IActionResult> UpdateProfile([FromQuery] Guid userId, UpdateProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
}
