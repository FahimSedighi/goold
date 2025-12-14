using GoldPriceTracker.Application.Common.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;

namespace GoldPriceTracker.Application.Features.Auth.Login;

/// <summary>
/// Command for user login operation.
/// Implements CQRS pattern for write operations.
/// </summary>
public class LoginCommand : ICommand<AuthResponse>
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}

