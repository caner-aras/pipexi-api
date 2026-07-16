namespace Workforce.Contracts.V1.Auth;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
