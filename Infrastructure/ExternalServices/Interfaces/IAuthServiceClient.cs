using GoldPriceTracker.Shared.Contracts.DTOs;

namespace GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;

/// <summary>
/// Client interface for communicating with AuthService microservice.
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>
    /// Login request - proxies to AuthService.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

