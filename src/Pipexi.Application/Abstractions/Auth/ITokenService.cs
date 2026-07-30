namespace Pipexi.Application.Abstractions.Auth;

public interface ITokenService
{
    Task<Pipexi.Shared.Results.Result<TokenResponse>> ExchangePasswordForTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Pipexi.Shared.Results.Result<TokenResponse>> ExchangeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<Pipexi.Shared.Results.Result<RegisterResponse>> RegisterWithEmailPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Pipexi.Shared.Results.Result<object?>> SendPasswordRecoveryEmailAsync(
        string email,
        CancellationToken cancellationToken);
}

public sealed record TokenResponse(
    int StatusCode,
    string access_token,
    string refresh_token,
    int expires_in);

public sealed record RegisterResponse(
    int StatusCode,
    string user_id,
    string email,
    string? access_token,
    string? refresh_token,
    int? expires_in);
