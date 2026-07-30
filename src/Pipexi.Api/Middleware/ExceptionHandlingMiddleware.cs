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
            await WriteResultFailureAsync(
                context,
                HttpStatusCode.Forbidden,
                "forbidden",
                exception.Message);
        }
        catch (UnauthorizedException exception)
        {
            await WriteResultFailureAsync(
                context,
                HttpStatusCode.Unauthorized,
                "unauthorized",
                exception.Message);
        }
        catch (ValidationException exception)
        {
            var message = string.Join(
                "; ",
                exception.Errors.Select(error => error.ErrorMessage));

            await WriteResultFailureAsync(
                context,
                HttpStatusCode.BadRequest,
                "validation.failed",
                string.IsNullOrWhiteSpace(message) ? "Validation failed." : message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);

            await WriteResultFailureAsync(
                context,
                HttpStatusCode.InternalServerError,
                "internal_server_error",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteResultFailureAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string code,
        string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            isSuccess = false,
            statusCode = (int)statusCode,
            data = (object?)null,
            error = new
            {
                code,
                message
            }
        });

        await context.Response.WriteAsync(payload);
    }
}
