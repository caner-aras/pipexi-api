using Microsoft.Extensions.Caching.Memory;
using Pipexi.Application.Abstractions.Identity;

namespace Pipexi.Api.Identity;

public sealed class CachingCurrentUserMembershipResolver : ICurrentUserMembershipResolver
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ICurrentUserMembershipResolver _inner;
    private readonly IMemoryCache _cache;

    public CachingCurrentUserMembershipResolver(
        ICurrentUserMembershipResolver inner,
        IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<CurrentUserMembership?> ResolveAsync(
        Guid userId,
        Guid? requestedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var cacheKey = BuildCacheKey(userId, requestedOrganizationId);
        if (_cache.TryGetValue(cacheKey, out CachedMembership? cached) && cached is not null)
        {
            return cached.Membership;
        }

        var membership = await _inner.ResolveAsync(userId, requestedOrganizationId, cancellationToken);

        _cache.Set(
            cacheKey,
            new CachedMembership(membership),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            });

        return membership;
    }

    private static string BuildCacheKey(Guid userId, Guid? requestedOrganizationId)
    {
        var orgPart = requestedOrganizationId is { } orgId && orgId != Guid.Empty
            ? orgId.ToString("N")
            : "default";

        return $"current-user-membership:{userId:N}:{orgPart}";
    }

    private sealed record CachedMembership(CurrentUserMembership? Membership);
}
