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

    public UserService(ILogger<UserService> logger, IPasswordHasher<User> passwordHasher)
    {
        _logger = logger;
        _passwordHasher = passwordHasher;
        
        // Create a demo user for testing (remove in production)
        InitializeDemoUser();
    }

    private void InitializeDemoUser()
    {
        if (!_users.Any())
        {
            var demoUser = new User
            {
                Id = _nextId++,
                Username = "admin",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            demoUser.PasswordHash = _passwordHasher.HashPassword(demoUser, "Admin123!");
            _users.Add(demoUser);
            _logger.LogInformation("Demo user created: admin@example.com / Admin123!");
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
            return Task.FromResult(false);

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
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

