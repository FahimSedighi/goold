using System.Security.Cryptography;
using System.Text;
using GoldPriceTracker.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;

namespace GoldPriceTracker.Services;

public class CustomPasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // Use PBKDF2 with HMAC-SHA256 (same as ASP.NET Identity)
        byte[] salt = new byte[16]; // 128 bits = 16 bytes
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 20); // 160 bits = 20 bytes (SHA-1 size for compatibility)

        // Combine salt and hash: 16 bytes salt + 20 bytes hash = 36 bytes total
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        return Convert.ToBase64String(hashBytes);
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentNullException(nameof(hashedPassword));
        if (string.IsNullOrEmpty(providedPassword))
            return PasswordVerificationResult.Failed;

        try
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            
            if (hashBytes.Length != 36)
                return PasswordVerificationResult.Failed;

            // Extract salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Hash the provided password with the extracted salt
            byte[] hash = KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 20);

            // Compare
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return PasswordVerificationResult.Failed;
            }

            return PasswordVerificationResult.Success;
        }
        catch
        {
            return PasswordVerificationResult.Failed;
        }
    }
}

