using GoldPriceTracker.Infrastructure.ExternalServices.Interfaces;
using GoldPriceTracker.Shared.Contracts.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace GoldPriceTracker.Infrastructure.ExternalServices;

/// <summary>
/// HTTP client for communicating with AuthService microservice.
/// </summary>
public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthServiceClient> _logger;

    public AuthServiceClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AuthServiceClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["AuthService:BaseUrl"] ?? "https://localhost:5001";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Sending login request to AuthService for: {request.EmailOrUsername}");

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
                return authResponse ?? new AuthResponse { Success = false, Message = "Invalid response from AuthService" };
            }

            // Try to read error response
            var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            return errorResponse ?? new AuthResponse 
            { 
                Success = false, 
                Message = $"AuthService returned status: {response.StatusCode}" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calling AuthService login endpoint");
            return new AuthResponse 
            { 
                Success = false, 
                Message = "خطا در ارتباط با سرویس احراز هویت" 
            };
        }
    }
}

