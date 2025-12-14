using GoldPriceTracker.Models;

namespace GoldPriceTracker.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ValidateTokenAsync(string token);
}

