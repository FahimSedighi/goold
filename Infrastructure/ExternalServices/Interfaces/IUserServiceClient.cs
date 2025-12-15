using GoldPriceTracker.Shared.Contracts.DTOs;

namespace GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;

/// <summary>
/// Client interface for communicating with UserService microservice.
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Gets user by ID from UserService.
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

