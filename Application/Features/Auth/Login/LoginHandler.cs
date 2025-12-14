using GoldPriceTracker.Application.Common.Interfaces;
using GoldPriceTracker.Application.Interfaces.Repositories;
using GoldPriceTracker.Application.Services.User.Profile;
using GoldPriceTracker.Domain.Entities;
using GoldPriceTracker.Infrastructure.Security.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Application.Features.Auth.Login;

/// <summary>
/// Handler for LoginCommand.
/// Implements CQRS pattern - handles the login business logic.
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUserProfileService userProfileService,
        ILogger<LoginHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _userProfileService = userProfileService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Login attempt for: {request.EmailOrUsername}");

            // Find user
            var user = await _userRepository.GetByEmailOrUsernameAsync(request.EmailOrUsername);
            if (user == null)
            {
                _logger.LogWarning($"Login attempt with invalid email/username: {request.EmailOrUsername}");
                return new AuthResponse
                {
                    Success = false,
                    Message = "ایمیل یا نام کاربری یا رمز عبور اشتباه است"
                };
            }

            // Validate password
            var isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning($"Invalid password attempt for user: {user.Email}");
                return new AuthResponse
                {
                    Success = false,
                    Message = "ایمیل یا نام کاربری یا رمز عبور اشتباه است"
                };
            }

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
            var token = _jwtTokenService.GenerateToken(user, request.RememberMe);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Update last login
            await _userRepository.UpdateLastLoginAsync(user.Id);

            // Add login activity
            _userProfileService.AddActivity(user.Id, "login", "ورود به سیستم", "bi-box-arrow-in-right");

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
}

