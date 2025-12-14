using GoldPriceTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;
    
    // In-memory storage for demo (replace with database in production)
    private static readonly List<User> _users = new();
    private static int _nextId = 1;
    private static bool _demoUserInitialized = false;

    public UserService(ILogger<UserService> logger, IPasswordHasher<User> passwordHasher)
    {
        _logger = logger;
        _passwordHasher = passwordHasher;
        
        // Create a demo user for testing (remove in production)
        InitializeDemoUser();
    }

    private void InitializeDemoUser()
    {
        if (!_demoUserInitialized)
        {
            // Remove existing demo user if exists
            var existingUser = _users.FirstOrDefault(u => u.Username == "admin" || u.Email == "admin@example.com");
            if (existingUser != null)
            {
                _users.Remove(existingUser);
            }

            var demoUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            demoUser.PasswordHash = _passwordHasher.HashPassword(demoUser, "Admin123!");
            _users.Add(demoUser);
            _nextId = 2;
            _demoUserInitialized = true;
            _logger.LogInformation("Demo user created: admin@example.com / Admin123!");
            _logger.LogInformation($"Password hash length: {demoUser.PasswordHash.Length}");
            _logger.LogInformation($"Password hash (first 50 chars): {demoUser.PasswordHash.Substring(0, Math.Min(50, demoUser.PasswordHash.Length))}");
            
            // Test the password immediately
            var testResult = _passwordHasher.VerifyHashedPassword(demoUser, demoUser.PasswordHash, "Admin123!");
            _logger.LogInformation($"Password test result after creation: {testResult}");
            if (testResult != PasswordVerificationResult.Success)
            {
                _logger.LogError("ERROR: Password verification failed immediately after creation!");
            }
        }
    }

    public Task<User?> GetUserByEmailOrUsernameAsync(string emailOrUsername)
    {
        var user = _users.FirstOrDefault(u => 
            (u.Email.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase) ||
             u.Username.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase)) &&
            u.IsActive);
        
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(string username, string email, string password)
    {
        var user = new User
        {
            Id = _nextId++,
            Username = username,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _users.Add(user);
        
        _logger.LogInformation($"User created: {email}");
        return Task.FromResult(user);
    }

    public Task<bool> ValidatePasswordAsync(User user, string password)
    {
        if (user == null || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("ValidatePasswordAsync: user is null or password is empty");
            return Task.FromResult(false);
        }

        _logger.LogInformation($"Validating password for user: {user.Email}");
        _logger.LogInformation($"Stored hash length: {user.PasswordHash?.Length}");
        _logger.LogInformation($"Stored hash (first 30 chars): {user.PasswordHash?.Substring(0, Math.Min(30, user.PasswordHash?.Length ?? 0))}");
        _logger.LogInformation($"Provided password length: {password.Length}");
        _logger.LogInformation($"Provided password: {password}");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        _logger.LogInformation($"Password verification result: {result}");
        
        return Task.FromResult(result == PasswordVerificationResult.Success);
    }

    public Task UpdateLastLoginAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }
}

