using System.Net;
using System.Text.Json;
using FluentValidation;
using Pipexi.Application.Common.Exceptions;

namespace Pipexi.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ForbiddenException exception)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.Forbidden,
                "Forbidden",
                exception.Message);
        }
        catch (UnauthorizedException exception)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                exception.Message);
        }
        catch (ValidationException exception)
        {
            var message = string.Join(
                "; ",
                exception.Errors.Select(error => error.ErrorMessage));

            await WriteProblemAsync(
                context,
                HttpStatusCode.BadRequest,
                "Validation failed",
                string.IsNullOrWhiteSpace(message) ? "Validation failed." : message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);

            await WriteProblemAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var payload = JsonSerializer.Serialize(new
        {
            type = "about:blank",
            title,
            status = (int)statusCode,
            detail,
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    }
}
