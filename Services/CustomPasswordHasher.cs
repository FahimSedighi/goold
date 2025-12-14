using GoldPriceTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace GoldPriceTracker.Services;

public class CustomPasswordHasher : IPasswordHasher<User>
{
    private readonly PasswordHasher<User> _passwordHasher;

    public CustomPasswordHasher()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(User user, string password)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        return _passwordHasher.HashPassword(user, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentNullException(nameof(hashedPassword));
        if (string.IsNullOrEmpty(providedPassword))
            return PasswordVerificationResult.Failed;

        return _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }
}

