namespace GoldPriceTracker.Infrastructure.Security.Interfaces;

/// <summary>
/// Interface for password hashing operations.
/// Implemented in Infrastructure layer.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

