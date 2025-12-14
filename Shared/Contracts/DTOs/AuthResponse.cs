namespace GoldPriceTracker.Shared.Contracts.DTOs;

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public string? Message { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

