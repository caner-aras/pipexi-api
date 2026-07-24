namespace Pipexi.Api.Middleware;

public sealed class RequestContextMiddleware
{
    private const string RequestIdHeader = "X-Request-Id";
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers[RequestIdHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(requestId))
        {
            requestId = Guid.NewGuid().ToString("N");
        }

        context.TraceIdentifier = requestId;
        context.Response.Headers[RequestIdHeader] = requestId;

        await _next(context);
    }
}
