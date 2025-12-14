using Microsoft.AspNetCore.Mvc;
using GoldPriceTracker.Models;
using GoldPriceTracker.Services;

namespace GoldPriceTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "اطلاعات ورودی نامعتبر است"
            });
        }

        var response = await _authService.LoginAsync(request);

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

    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken()
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Token not found"
            });
        }

        var response = await _authService.ValidateTokenAsync(token);
        
        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }
}

