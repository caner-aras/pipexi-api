using System.Security.Claims;
using Pipexi.Application.Abstractions.Identity;

namespace Pipexi.Api.Middleware;

public sealed class CurrentUserMembershipMiddleware
{
    public const string OrganizationIdHeader = "X-Organization-Id";

    private readonly RequestDelegate _next;

    public CurrentUserMembershipMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        CurrentUserMembershipState membershipState,
        ICurrentUserMembershipResolver membershipResolver)
    {
        membershipState.Clear();

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = ParseUserId(context.User);
            if (userId != Guid.Empty)
            {
                var requestedOrganizationId = ResolveRequestedOrganizationId(context.Request);
                var membership = await membershipResolver.ResolveAsync(
                    userId,
                    requestedOrganizationId,
                    context.RequestAborted);

                if (membership is not null)
                {
                    membershipState.Set(membership);
                }
            }
        }

        await _next(context);
    }

    private static Guid ParseUserId(ClaimsPrincipal user)
    {
        var value =
            user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty;
    }

    private static Guid? ResolveRequestedOrganizationId(HttpRequest request)
    {
        if (request.Headers.TryGetValue(OrganizationIdHeader, out var headerValues))
        {
            var headerValue = headerValues.FirstOrDefault();
            if (Guid.TryParse(headerValue, out var fromHeader) && fromHeader != Guid.Empty)
            {
                return fromHeader;
            }
        }

        if (request.Query.TryGetValue("organizationId", out var queryValues))
        {
            var queryValue = queryValues.FirstOrDefault();
            if (Guid.TryParse(queryValue, out var fromQuery) && fromQuery != Guid.Empty)
            {
                return fromQuery;
            }
        }

        return null;
    }
}
