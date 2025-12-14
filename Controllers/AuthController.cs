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
        _logger.LogInformation($"Login request received - EmailOrUsername: {request?.EmailOrUsername}, RememberMe: {request?.RememberMe}");
        
        if (request == null)
        {
            _logger.LogWarning("Login request is null");
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "درخواست نامعتبر است"
            });
        }

        // Log ModelState errors if any
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning($"ModelState is invalid. Errors: {string.Join(", ", errors)}");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state?.Errors != null && state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        _logger.LogWarning($"ModelState error for {key}: {error.ErrorMessage}");
                    }
                }
            }
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = $"اطلاعات ورودی نامعتبر است: {string.Join(", ", errors)}"
            });
        }
        
        // Additional validation (ModelState might pass but we want to be sure)
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
        {
            _logger.LogWarning("EmailOrUsername is null or whitespace");
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "ایمیل یا نام کاربری الزامی است"
            });
        }
        
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Password is null or whitespace");
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "رمز عبور الزامی است"
            });
        }

        _logger.LogInformation($"Calling AuthService.LoginAsync with EmailOrUsername: {request.EmailOrUsername}, RememberMe: {request.RememberMe}");
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

