using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Api.Controllers;

/// <summary>
/// Authentication controller - proxies requests to AuthService microservice.
/// This project does NOT handle authentication logic, only routes requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthServiceClient authServiceClient, ILogger<AuthController> logger)
    {
        _authServiceClient = authServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint - proxies to AuthService.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation($"Login request received - EmailOrUsername: {request?.EmailOrUsername}");
        
        if (request == null)
        {
            return BadRequest(new { Success = false, Message = "درخواست نامعتبر است" });
        }

        // Proxy to AuthService
        var response = await _authServiceClient.LoginAsync(request);

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

    /// <summary>
    /// Logout endpoint - clears cookies.
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("RefreshToken");
        
        return Ok(new { Success = true, Message = "خروج با موفقیت انجام شد" });
    }
}
