using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Shifts;

internal sealed record ShiftTeamMemberMaps(
    IReadOnlyDictionary<(Guid TeamId, Guid OrganizationMemberId), Guid> ByPair,
    IReadOnlyDictionary<Guid, Guid> ByOrganizationMemberId);

internal static class ShiftTeamMemberLookup
{
    public static async Task<ShiftTeamMemberMaps> CreateAsync(
        ITeamMemberRepository teamMemberRepository,
        IEnumerable<Shift> shifts,
        CancellationToken cancellationToken)
    {
        var organizationMemberIds = shifts
            .Where(x => x.OrganizationMemberId.HasValue)
            .Select(x => x.OrganizationMemberId!.Value)
            .Distinct()
            .ToList();

        if (organizationMemberIds.Count == 0)
        {
            return new ShiftTeamMemberMaps(
                new Dictionary<(Guid, Guid), Guid>(),
                new Dictionary<Guid, Guid>());
        }

        var teamMembers = await teamMemberRepository.ListByOrganizationMemberIdsAsync(
            organizationMemberIds,
            cancellationToken);

        var byPair = teamMembers
            .GroupBy(x => (x.TeamId, x.OrganizationMemberId))
            .ToDictionary(g => g.Key, g => g.First().Id);

        // Used when shift has organizationMemberId but no teamId.
        // Prefer the earliest membership (same behavior as the old web lookup).
        var byOrganizationMemberId = teamMembers
            .GroupBy(x => x.OrganizationMemberId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CreatedAt).First().Id);

        return new ShiftTeamMemberMaps(byPair, byOrganizationMemberId);
    }

    public static Guid? Resolve(
        Guid? teamId,
        Guid? organizationMemberId,
        ShiftTeamMemberMaps maps)
    {
        if (!organizationMemberId.HasValue)
        {
            return null;
        }

        if (teamId.HasValue &&
            maps.ByPair.TryGetValue((teamId.Value, organizationMemberId.Value), out var scopedId))
        {
            return scopedId;
        }

        if (!teamId.HasValue &&
            maps.ByOrganizationMemberId.TryGetValue(organizationMemberId.Value, out var fallbackId))
        {
            return fallbackId;
        }

        return null;
    }
}
