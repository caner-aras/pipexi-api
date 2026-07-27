using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Shifts;

internal static class ShiftTeamMemberLookup
{
    public static async Task<IReadOnlyDictionary<(Guid TeamId, Guid OrganizationMemberId), Guid>> CreateAsync(
        ITeamMemberRepository teamMemberRepository,
        IEnumerable<Shift> shifts,
        CancellationToken cancellationToken)
    {
        var organizationMemberIds = shifts
            .Where(x => x.TeamId.HasValue && x.OrganizationMemberId.HasValue)
            .Select(x => x.OrganizationMemberId!.Value)
            .Distinct()
            .ToList();

        if (organizationMemberIds.Count == 0)
        {
            return new Dictionary<(Guid, Guid), Guid>();
        }

        var teamMembers = await teamMemberRepository.ListByOrganizationMemberIdsAsync(
            organizationMemberIds,
            cancellationToken);

        return teamMembers
            .GroupBy(x => (x.TeamId, x.OrganizationMemberId))
            .ToDictionary(g => g.Key, g => g.First().Id);
    }

    public static Guid? Resolve(
        Guid? teamId,
        Guid? organizationMemberId,
        IReadOnlyDictionary<(Guid TeamId, Guid OrganizationMemberId), Guid> lookup)
    {
        if (!teamId.HasValue || !organizationMemberId.HasValue)
        {
            return null;
        }

        return lookup.TryGetValue((teamId.Value, organizationMemberId.Value), out var teamMemberId)
            ? teamMemberId
            : null;
    }
}
