using GoldPriceTracker.Models;
using GoldPriceTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GoldPriceTracker.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<TestController> _logger;

    public TestController(
        IUserService userService,
        IPasswordHasher<User> passwordHasher,
        ILogger<TestController> logger)
    {
        _userService = userService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpGet("password")]
    public IActionResult TestPassword()
    {
        try
        {
            var testUser = new User
            {
                Id = 999,
                Username = "test",
                Email = "test@test.com"
            };

            var password = "Admin123!";
            var hash = _passwordHasher.HashPassword(testUser, password);
            var verifyResult = _passwordHasher.VerifyHashedPassword(testUser, hash, password);

            return Ok(new
            {
                success = true,
                password = password,
                hash = hash,
                hashLength = hash.Length,
                verifyResult = verifyResult.ToString(),
                isValid = verifyResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpGet("user")]
    public async Task<IActionResult> TestUser()
    {
        try
        {
            var user = await _userService.GetUserByEmailOrUsernameAsync("admin");
            if (user == null)
            {
                return Ok(new { success = false, message = "User not found" });
            }

            var testPassword = "Admin123!";
            var isValid = await _userService.ValidatePasswordAsync(user, testPassword);

            return Ok(new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    passwordHash = user.PasswordHash,
                    passwordHashLength = user.PasswordHash?.Length,
                    isActive = user.IsActive
                },
                testPassword = testPassword,
                isValid = isValid
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}

