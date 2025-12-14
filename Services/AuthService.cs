using System.Security.Claims;
using GoldPriceTracker.Models;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"AuthService.LoginAsync called - EmailOrUsername: {request?.EmailOrUsername}, RememberMe: {request?.RememberMe}");
            
            // Validation
            if (request == null)
            {
                _logger.LogWarning("LoginRequest is null");
                return new AuthResponse
                {
                    Success = false,
                    Message = "درخواست نامعتبر است"
                };
            }

            if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
            {
                _logger.LogWarning("EmailOrUsername is empty");
                return new AuthResponse
                {
                    Success = false,
                    Message = "ایمیل یا نام کاربری الزامی است"
                };
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Password is empty");
                return new AuthResponse
                {
                    Success = false,
                    Message = "رمز عبور الزامی است"
                };
            }

            // Find user
            _logger.LogInformation($"Looking for user with email/username: {request.EmailOrUsername}");
            var user = await _userService.GetUserByEmailOrUsernameAsync(request.EmailOrUsername);
            if (user == null)
            {
                _logger.LogWarning($"Login attempt with invalid email/username: {request.EmailOrUsername}");
                _logger.LogWarning("User not found in database");
                return new AuthResponse
                {
                    Success = false,
                    Message = "ایمیل یا نام کاربری یا رمز عبور اشتباه است"
                };
            }

            _logger.LogInformation($"User found: {user.Email}, Username: {user.Username}");
            _logger.LogInformation($"User password hash length: {user.PasswordHash?.Length}");

            // Validate password
            _logger.LogInformation($"Validating password for user: {user.Email}");
            var isPasswordValid = await _userService.ValidatePasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning($"Invalid password attempt for user: {user.Email}");
                _logger.LogWarning($"Password hash stored (first 50 chars): {user.PasswordHash?.Substring(0, Math.Min(50, user.PasswordHash?.Length ?? 0))}");
                _logger.LogWarning($"Password provided: {request.Password}");
                _logger.LogWarning($"Password provided length: {request.Password?.Length}");
                return new AuthResponse
                {
                    Success = false,
                    Message = "ایمیل یا نام کاربری یا رمز عبور اشتباه است"
                };
            }

            _logger.LogInformation($"Password validated successfully for user: {user.Email}");

            // Check if user is active
            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "حساب کاربری شما غیرفعال است"
                };
            }

            // Generate token
            var token = _jwtService.GenerateToken(user, request.RememberMe);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update last login
            await _userService.UpdateLastLoginAsync(user.Id);

            _logger.LogInformation($"User logged in successfully: {user.Email}");

            // Calculate expiration
            var expiresInMinutes = request.RememberMe ? 7 * 24 * 60 : 60;
            var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                },
                ExpiresAt = expiresAt,
                Message = "ورود با موفقیت انجام شد"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new AuthResponse
            {
                Success = false,
                Message = "خطا در ورود به سیستم. لطفاً دوباره تلاش کنید."
            };
        }
    }

    public async Task<AuthResponse> ValidateTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Token is required"
                };
            }

            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Token is invalid or expired"
                };
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid token claims"
                };
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User not found or inactive"
                };
            }

            return new AuthResponse
            {
                Success = true,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return new AuthResponse
            {
                Success = false,
                Message = "Error validating token"
            };
        }
    }
}

