using GoldPriceTracker.Application.Interfaces.Repositories;
using GoldPriceTracker.Domain.Entities;
using GoldPriceTracker.Infrastructure.Security.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoldPriceTracker.Infrastructure.Persistence.Repositories;

/// <summary>
/// User repository implementation.
/// Part of Infrastructure layer - handles data access.
/// In a real application, this would use Entity Framework or another ORM.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    
    // In-memory storage for demo (replace with database in production)
    private static readonly List<User> _users = new();
    private static int _nextId = 1;
    private static bool _demoUserInitialized = false;

    public UserRepository(ILogger<UserRepository> logger, IPasswordHasher passwordHasher)
    {
        _logger = logger;
        
        // Initialize demo user
        InitializeDemoUser(passwordHasher);
    }

    private void InitializeDemoUser(IPasswordHasher passwordHasher)
    {
        if (!_demoUserInitialized)
        {
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
            demoUser.PasswordHash = passwordHasher.HashPassword("Admin123!");
            _users.Add(demoUser);
            _nextId = 2;
            _demoUserInitialized = true;
            _logger.LogInformation("Demo user created: admin@example.com / Admin123!");
        }
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => 
            (u.Email.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase) ||
             u.Username.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase)) &&
            u.IsActive);
        
        return Task.FromResult(user);
    }

    public Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.Id = _nextId++;
        _users.Add(user);
        _logger.LogInformation($"User created: {user.Email}");
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.IsActive = user.IsActive;
            existingUser.LastLoginAt = user.LastLoginAt;
        }
        return Task.CompletedTask;
    }

    public Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var exists = _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}

