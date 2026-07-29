using System.Net.Mail;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pipexi.Application.Abstractions.Auth;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common;
using Pipexi.Application.Features.Users.Commands.CreateUser;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Api.Endpoints.V1;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("auth");
        //.ExcludeFromDescription();

        group.MapPost("/token", HandleSupabaseTokenAsync);
        group.MapPost("/refresh", RefreshTokenAsync);
        group.MapPost("/register", RegisterAsync);
        group.MapPost("/sync", SyncProfileAsync)
            .RequireAuthorization();
        group.MapGet("/me", GetMeAsync)
            .RequireAuthorization();
        //.ExcludeFromDescription();

        return app;
    }

    private static async Task<IResult> HandleSupabaseTokenAsync(
        ITokenService tokenService,
        TokenRequest request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await tokenService.ExchangePasswordForTokenAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return Results.Json(tokenResponse, statusCode: tokenResponse.StatusCode);
    }

    private static async Task<IResult> RefreshTokenAsync(
        ITokenService tokenService,
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.refresh_token))
        {
            var invalid = Result<TokenResponse>.Failure(
                new AppError("auth.invalid_refresh_token", "Refresh token is required."),
                StatusCodes.Status400BadRequest);

            return Results.Json(invalid, statusCode: invalid.StatusCode);
        }

        var tokenResponse = await tokenService.ExchangeRefreshTokenAsync(
            request.refresh_token,
            cancellationToken);

        return Results.Json(tokenResponse, statusCode: tokenResponse.StatusCode);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ITokenService tokenService,
        IUserRepository userRepository,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        if (!IsValidEmail(email))
        {
            var invalidEmail = Result<RegisterResponse>.Failure(
                new AppError("auth.invalid_email", "A valid email address is required."),
                StatusCodes.Status400BadRequest);

            return Results.Json(invalidEmail, statusCode: invalidEmail.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Trim().Length > 100)
        {
            var invalidFirstName = Result<RegisterResponse>.Failure(
                new AppError("auth.invalid_first_name", "FirstName is required and must be 100 characters or less."),
                StatusCodes.Status400BadRequest);

            return Results.Json(invalidFirstName, statusCode: invalidFirstName.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Trim().Length > 100)
        {
            var invalidLastName = Result<RegisterResponse>.Failure(
                new AppError("auth.invalid_last_name", "LastName is required and must be 100 characters or less."),
                StatusCodes.Status400BadRequest);

            return Results.Json(invalidLastName, statusCode: invalidLastName.StatusCode);
        }

        if (!IsStrongPassword(request.Password))
        {
            var invalidPassword = Result<RegisterResponse>.Failure(
                new AppError("auth.invalid_password", "Password must be at least 6 characters."),
                StatusCodes.Status400BadRequest);

            return Results.Json(invalidPassword, statusCode: invalidPassword.StatusCode);
        }

        var existingUser = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            var duplicateEmail = Result<RegisterResponse>.Failure(
                new AppError("auth.email_already_registered", "This email is already registered."),
                StatusCodes.Status409Conflict);

            return Results.Json(duplicateEmail, statusCode: duplicateEmail.StatusCode);
        }

        var registerResult = await tokenService.RegisterWithEmailPasswordAsync(email, request.Password, cancellationToken);
        if (!registerResult.IsSuccess || registerResult.Data is null)
        {
            var failed = Result<RegisterResponse>.Failure(
                registerResult.Error ?? new AppError("auth.register_failed", "Registration failed."),
                registerResult.StatusCode);

            return Results.Json(failed, statusCode: failed.StatusCode);
        }

        var createUserResult = await sender.Send(
            new CreateUserCommand(
                registerResult.Data.user_id,
                registerResult.Data.email,
                request.FirstName,
                request.LastName,
                null,
                null),
            cancellationToken);

        if (!createUserResult.IsSuccess || createUserResult.Data is null)
        {
            var failed = Result<RegisterResponse>.Failure(
                createUserResult.Error ?? new AppError("auth.register_failed", "User profile could not be created."),
                createUserResult.StatusCode);

            return Results.Json(failed, statusCode: failed.StatusCode);
        }

        var response = Result<RegisterResponse>.Success(
            new RegisterResponse(
                StatusCodes.Status201Created,
                registerResult.Data.user_id,
                registerResult.Data.email,
                registerResult.Data.access_token,
                registerResult.Data.refresh_token,
                registerResult.Data.expires_in),
            StatusCodes.Status201Created);

        return Results.Json(response, statusCode: response.StatusCode);
    }

    private static async Task<IResult> SyncProfileAsync(
        HttpContext httpContext,
        IUserRepository userRepository,
        ISender sender,
        SyncProfileRequest request,
        CancellationToken cancellationToken)
    {
        var sub = GetClaimValue(httpContext.User, ClaimTypes.NameIdentifier, "sub");
        if (string.IsNullOrWhiteSpace(sub))
        {
            var unauthorized = Result<SyncProfileResponse>.Failure(
                new AppError("auth.unauthorized", "Unauthorized."),
                StatusCodes.Status401Unauthorized);

            return Results.Json(unauthorized, statusCode: unauthorized.StatusCode);
        }

        var user = await userRepository.GetByAuthProviderIdAsync(sub, cancellationToken);

        if (user is null)
        {
            var email = (request.Email ?? GetClaimValue(httpContext.User, "email"))?.Trim() ?? string.Empty;
            if (!IsValidEmail(email))
            {
                var invalidEmail = Result<SyncProfileResponse>.Failure(
                    new AppError("auth.invalid_email", "A valid email address is required for first sync."),
                    StatusCodes.Status400BadRequest);

                return Results.Json(invalidEmail, statusCode: invalidEmail.StatusCode);
            }

            var firstName = NormalizeOrFallback(
                request.FirstName,
                GetClaimValue(httpContext.User, "given_name"),
                GetClaimValue(httpContext.User, "name"),
                "User");

            var lastName = NormalizeOrFallback(
                request.LastName,
                GetClaimValue(httpContext.User, "family_name"),
                null,
                "");

            var createResult = await sender.Send(
                new CreateUserCommand(
                    sub,
                    email,
                    firstName,
                    string.IsNullOrWhiteSpace(lastName) ? "-" : lastName,
                    request.Phone,
                    request.AvatarUrl ?? GetClaimValue(httpContext.User, "picture")),
                cancellationToken);

            if (!createResult.IsSuccess || createResult.Data is null)
            {
                var failed = Result<SyncProfileResponse>.Failure(
                    createResult.Error ?? new AppError("auth.sync_failed", "Profile sync failed."),
                    createResult.StatusCode);

                return Results.Json(failed, statusCode: failed.StatusCode);
            }

            var created = Result<SyncProfileResponse>.Success(
                new SyncProfileResponse(
                    createResult.Data.Id,
                    createResult.Data.Email,
                    createResult.Data.FirstName,
                    createResult.Data.LastName,
                    createResult.Data.Phone,
                    AvatarUrls.Resolve(createResult.Data.Id, createResult.Data.AvatarUrl),
                    true));

            return Results.Json(created, statusCode: created.StatusCode);
        }

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.AvatarUrl ?? GetClaimValue(httpContext.User, "picture"));

        await userRepository.UpdateAsync(user, cancellationToken);

        var updated = Result<SyncProfileResponse>.Success(
            new SyncProfileResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Phone,
                AvatarUrls.Resolve(user.Id, user.AvatarUrl),
                false));

        return Results.Json(updated, statusCode: updated.StatusCode);
    }

    private static async Task<IResult> GetMeAsync(
        HttpContext httpContext,
        ICurrentUserContext currentUserContext,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (currentUserContext.UserId == Guid.Empty)
        {
            var unauthorized = Result<MeResponse>.Failure(
                new AppError("auth.unauthorized", "Unauthorized."),
                StatusCodes.Status401Unauthorized);

            return Results.Json(unauthorized, statusCode: unauthorized.StatusCode);
        }

        var user = await userRepository.GetByIdAsync(currentUserContext.UserId, cancellationToken);
        if (user is null)
        {
            var notFound = Result<MeResponse>.Failure(
                new AppError("auth.user_not_found", "User profile not found."),
                StatusCodes.Status404NotFound);

            return Results.Json(notFound, statusCode: notFound.StatusCode);
        }

        var email = string.IsNullOrWhiteSpace(user.Email)
            ? httpContext.User.FindFirst("email")?.Value
            : user.Email;

        var response = Result<MeResponse>.Success(new MeResponse(
            currentUserContext.UserId,
            currentUserContext.OrganizationId,
            currentUserContext.Role,
            email,
            user.FirstName,
            user.LastName,
            user.Phone,
            AvatarUrls.Resolve(user.Id, user.AvatarUrl)));

        return Results.Json(response, statusCode: response.StatusCode);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 200)
        {
            return false;
        }

        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsStrongPassword(string? password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= 6 && password.Length <= 128;
    }

    private static string? GetClaimValue(ClaimsPrincipal user, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var claim = user.FindFirst(claimType);
            if (!string.IsNullOrWhiteSpace(claim?.Value))
            {
                return claim.Value;
            }
        }

        return null;
    }

    private static string NormalizeOrFallback(
        string? requestValue,
        string? primaryClaim,
        string? secondaryClaim,
        string fallback)
    {
        if (!string.IsNullOrWhiteSpace(requestValue))
        {
            return requestValue.Trim();
        }

        if (!string.IsNullOrWhiteSpace(primaryClaim))
        {
            return primaryClaim.Trim();
        }

        if (!string.IsNullOrWhiteSpace(secondaryClaim))
        {
            return secondaryClaim.Trim();
        }

        return fallback;
    }
}

public sealed record TokenRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string refresh_token);
public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public sealed record SyncProfileRequest(
    string? Email,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? AvatarUrl);
public sealed record SyncProfileResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? AvatarUrl,
    bool Created);
public sealed record MeResponse(
    Guid UserId,
    Guid OrganizationId,
    string Role,
    string? Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? AvatarUrl);
