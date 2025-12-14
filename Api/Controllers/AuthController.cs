using GoldPriceTracker.Application.Features.Auth.Login;
using GoldPriceTracker.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Api.Controllers;

/// <summary>
/// Authentication controller - thin API layer.
/// Uses CQRS pattern via MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint - uses LoginCommand via CQRS pattern.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation($"Login request received - EmailOrUsername: {request?.EmailOrUsername}");
        
        if (request == null)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "درخواست نامعتبر است"
            });
        }

        // Create command from request
        var command = new LoginCommand
        {
            EmailOrUsername = request.EmailOrUsername,
            Password = request.Password,
            RememberMe = request.RememberMe
        };

        // Execute command via MediatR (CQRS pattern)
        var response = await _mediator.Send(command);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        // Set cookies
        if (!string.IsNullOrEmpty(response.Token))
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = response.ExpiresAt
            };

            Response.Cookies.Append("AuthToken", response.Token, cookieOptions);

            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                Response.Cookies.Append("RefreshToken", response.RefreshToken, cookieOptions);
            }
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("RefreshToken");
        
        return Ok(new { Success = true, Message = "خروج با موفقیت انجام شد" });
    }
}

