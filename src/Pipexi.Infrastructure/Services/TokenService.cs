using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Pipexi.Application.Abstractions.Auth;

namespace Pipexi.Infrastructure.Services;

public sealed class TokenService : ITokenService
{

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Pipexi.Shared.Results.Result<TokenResponse>> ExchangePasswordForTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var authBaseUrl = _configuration["Supabase:Auth:Authority"];
        var anonApiKey = _configuration["Supabase:Auth:AnonApiKey"];

        if (string.IsNullOrWhiteSpace(authBaseUrl) || string.IsNullOrWhiteSpace(anonApiKey))
        {

            return Pipexi.Shared.Results.Result<TokenResponse>.Failure(new Pipexi.Shared.Errors.AppError(
                "SupabaseAuthConfigurationMissing",
                "Supabase auth configuration is missing."));
        }

        var client = _httpClientFactory.CreateClient();
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{authBaseUrl.TrimEnd('/')}/token?grant_type=password");

        message.Headers.Add("apikey", anonApiKey);
        message.Content = JsonContent.Create(new
        {
            email,
            password
        });

        using var response = await client.SendAsync(message, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

        if (!response.IsSuccessStatusCode)
        {
            return Pipexi.Shared.Results.Result<TokenResponse>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "AuthFailed",
                    responseBody));
        }

        using var document = JsonDocument.Parse(responseBody);

        var accessToken = document.RootElement.GetProperty("access_token").GetString();
        var refreshToken = document.RootElement.GetProperty("refresh_token").GetString();

        var result = new TokenResponse(
            (int)response.StatusCode,
            accessToken ?? string.Empty,
            refreshToken ?? string.Empty);

        return Pipexi.Shared.Results.Result<TokenResponse>.Success(result);

    }

    public async Task<Pipexi.Shared.Results.Result<RegisterResponse>> RegisterWithEmailPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var authBaseUrl = _configuration["Supabase:Auth:Authority"];
        var anonApiKey = _configuration["Supabase:Auth:AnonApiKey"];

        if (string.IsNullOrWhiteSpace(authBaseUrl) || string.IsNullOrWhiteSpace(anonApiKey))
        {
            return Pipexi.Shared.Results.Result<RegisterResponse>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "SupabaseAuthConfigurationMissing",
                    "Supabase auth configuration is missing."));
        }

        var client = _httpClientFactory.CreateClient();
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{authBaseUrl.TrimEnd('/')}/signup");

        message.Headers.Add("apikey", anonApiKey);
        message.Content = JsonContent.Create(new
        {
            email,
            password
        });

        using var response = await client.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Pipexi.Shared.Results.Result<RegisterResponse>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "AuthRegisterFailed",
                    responseBody),
                (int)response.StatusCode);
        }

        using var document = JsonDocument.Parse(responseBody);
        if (!document.RootElement.TryGetProperty("user", out var userElement))
        {
            return Pipexi.Shared.Results.Result<RegisterResponse>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "AuthRegisterFailed",
                    "Supabase register response does not include user payload."),
                (int)response.StatusCode);
        }

        var userId = userElement.TryGetProperty("id", out var userIdElement)
            ? userIdElement.GetString()
            : null;

        var userEmail = userElement.TryGetProperty("email", out var userEmailElement)
            ? userEmailElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userEmail))
        {
            return Pipexi.Shared.Results.Result<RegisterResponse>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "AuthRegisterFailed",
                    "Supabase register response is missing user id or email."),
                (int)response.StatusCode);
        }

        var accessToken = document.RootElement.TryGetProperty("access_token", out var accessTokenElement)
            ? accessTokenElement.GetString()
            : null;

        var refreshToken = document.RootElement.TryGetProperty("refresh_token", out var refreshTokenElement)
            ? refreshTokenElement.GetString()
            : null;

        var result = new RegisterResponse(
            (int)response.StatusCode,
            userId,
            userEmail,
            accessToken,
            refreshToken);

        return Pipexi.Shared.Results.Result<RegisterResponse>.Success(result, (int)response.StatusCode);
    }
}
