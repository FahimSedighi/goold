using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace GoldPriceTracker.Infrastructure.ExternalServices;

/// <summary>
/// HTTP client for communicating with UserService microservice.
/// </summary>
public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["UserService:BaseUrl"] ?? "https://localhost:5002";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Fetching user {userId} from UserService");

            var response = await _httpClient.GetAsync($"/api/users/{userId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: cancellationToken);
                return user;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {userId} not found in UserService");
                return null;
            }

            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching user {userId} from UserService");
            throw;
        }
    }
}

