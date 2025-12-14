using GoldPriceTracker.Domain.Entities;

namespace GoldPriceTracker.Infrastructure.Security.Interfaces;

/// <summary>
/// Interface for JWT token operations.
/// Implemented in Infrastructure layer.
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(User user, bool rememberMe = false);
    bool ValidateToken(string token, out System.Security.Claims.ClaimsPrincipal? principal);
    string GenerateRefreshToken();
}

