using System.Security.Claims;
using Pipexi.Application.Abstractions.Identity;

namespace Pipexi.Api.Identity;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CurrentUserMembershipState _membershipState;

    public HttpCurrentUserContext(
        IHttpContextAccessor httpContextAccessor,
        CurrentUserMembershipState membershipState)
    {
        _httpContextAccessor = httpContextAccessor;
        _membershipState = membershipState;
    }

    public Guid UserId => ParseGuidClaim(ClaimTypes.NameIdentifier, "sub");

    public Guid OrganizationId => _membershipState.OrganizationId;

    public string Role => _membershipState.Role;

    private Guid ParseGuidClaim(params string[] claimTypes)
    {
        var value = GetClaimValue(claimTypes);
        return Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty;
    }

    private string? GetClaimValue(params string[] claimTypes)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return null;
        }

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
}
