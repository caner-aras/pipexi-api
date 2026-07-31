using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Pipexi.Application.Abstractions.Auth;

namespace Pipexi.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private const int DefaultAccessTokenExpiresInSeconds = 3600;

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public Task<Pipexi.Shared.Results.Result<TokenResponse>> ExchangePasswordForTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        return ExchangeTokenAsync(
            "password",
            new { email, password },
            cancellationToken);
    }

    public Task<Pipexi.Shared.Results.Result<TokenResponse>> ExchangeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        return ExchangeTokenAsync(
            "refresh_token",
            new { refresh_token = refreshToken },
            cancellationToken);
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
                    ParseSupabaseAuthErrorMessage(responseBody)),
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

        var expiresIn = ReadExpiresIn(document.RootElement);

        var result = new RegisterResponse(
            (int)response.StatusCode,
            userId,
            userEmail,
            accessToken,
            refreshToken,
            expiresIn);

        return Pipexi.Shared.Results.Result<RegisterResponse>.Success(result, (int)response.StatusCode);
    }

    public async Task<Pipexi.Shared.Results.Result<object?>> SendPasswordRecoveryEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var authBaseUrl = _configuration["Supabase:Auth:Authority"];
        var anonApiKey = _configuration["Supabase:Auth:AnonApiKey"];

        if (string.IsNullOrWhiteSpace(authBaseUrl) || string.IsNullOrWhiteSpace(anonApiKey))
        {
            return Pipexi.Shared.Results.Result<object?>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "SupabaseAuthConfigurationMissing",
                    "Supabase auth configuration is missing."));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Pipexi.Shared.Results.Result<object?>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "AuthRecoverInvalidEmail",
                    "Email is required."));
        }

        var client = _httpClientFactory.CreateClient();
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{authBaseUrl.TrimEnd('/')}/recover");

        message.Headers.Add("apikey", anonApiKey);

        var redirectBaseUrl = _configuration["Supabase:Auth:EmailRedirectBaseUrl"]
            ?? _configuration["Web:PublicBaseUrl"];
        var redirectTo = string.IsNullOrWhiteSpace(redirectBaseUrl)
            ? null
            : $"{redirectBaseUrl.TrimEnd('/')}/auth/callback";

        message.Content = JsonContent.Create(
            string.IsNullOrWhiteSpace(redirectTo)
                ? (object)new { email = email.Trim().ToLowerInvariant() }
                : new
                {
                    email = email.Trim().ToLowerInvariant(),
                    redirect_to = redirectTo
                });

        using var response = await client.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Pipexi.Shared.Results.Result<object?>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    "AuthRecoverFailed",
                    ParseSupabaseAuthErrorMessage(responseBody)),
                (int)response.StatusCode);
        }

        return Pipexi.Shared.Results.Result<object?>.Success(null, (int)response.StatusCode);
    }

    private async Task<Pipexi.Shared.Results.Result<TokenResponse>> ExchangeTokenAsync(
        string grantType,
        object body,
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
            $"{authBaseUrl.TrimEnd('/')}/token?grant_type={grantType}");

        message.Headers.Add("apikey", anonApiKey);
        message.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Pipexi.Shared.Results.Result<TokenResponse>.Failure(
                new Pipexi.Shared.Errors.AppError(
                    grantType == "refresh_token" ? "AuthRefreshFailed" : "AuthFailed",
                    ParseSupabaseAuthErrorMessage(responseBody)),
                (int)response.StatusCode);
        }

        using var document = JsonDocument.Parse(responseBody);

        var accessToken = document.RootElement.GetProperty("access_token").GetString();
        var refreshToken = document.RootElement.GetProperty("refresh_token").GetString();
        var expiresIn = ReadExpiresIn(document.RootElement);

        var result = new TokenResponse(
            (int)response.StatusCode,
            accessToken ?? string.Empty,
            refreshToken ?? string.Empty,
            expiresIn);

        return Pipexi.Shared.Results.Result<TokenResponse>.Success(result);
    }

    private static int ReadExpiresIn(JsonElement root)
    {
        if (root.TryGetProperty("expires_in", out var expiresElement)
            && expiresElement.TryGetInt32(out var expiresIn)
            && expiresIn > 0)
        {
            return expiresIn;
        }

        return DefaultAccessTokenExpiresInSeconds;
    }

    /// <summary>
    /// Supabase auth errors are JSON like
    /// {"code":400,"error_code":"invalid_credentials","msg":"Invalid login credentials"}.
    /// Prefer a human-readable msg / error_description over the raw body.
    /// </summary>
    private static string ParseSupabaseAuthErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return "Authentication failed.";
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (TryReadStringProperty(root, "msg", out var msg))
            {
                return msg;
            }

            if (TryReadStringProperty(root, "error_description", out var errorDescription))
            {
                return errorDescription;
            }

            if (TryReadStringProperty(root, "message", out var message))
            {
                return message;
            }

            if (TryReadStringProperty(root, "error", out var error) &&
                !string.Equals(error, "invalid_grant", StringComparison.OrdinalIgnoreCase))
            {
                return error;
            }
        }
        catch (JsonException)
        {
            // Fall through to trimmed raw body for non-JSON responses.
        }

        var trimmed = responseBody.Trim();
        return trimmed.Length > 300 ? trimmed[..300] : trimmed;
    }

    private static bool TryReadStringProperty(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var element) ||
            element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var text = element.GetString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        value = text.Trim();
        return true;
    }
}
