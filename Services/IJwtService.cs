using GoldPriceTracker.Models;
using System.Security.Claims;

namespace GoldPriceTracker.Services;

public interface IJwtService
{
    string GenerateToken(User user, bool rememberMe = false);
    ClaimsPrincipal? ValidateToken(string token);
    string GenerateRefreshToken();
}

