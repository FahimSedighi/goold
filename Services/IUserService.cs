using GoldPriceTracker.Models;

namespace GoldPriceTracker.Services;

public interface IUserService
{
    Task<User?> GetUserByEmailOrUsernameAsync(string emailOrUsername);
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(string username, string email, string password);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task UpdateLastLoginAsync(int userId);
}

