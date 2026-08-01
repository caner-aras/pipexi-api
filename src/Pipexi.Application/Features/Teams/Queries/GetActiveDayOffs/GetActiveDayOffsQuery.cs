using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetActiveDayOffs;

public sealed record GetActiveDayOffsQuery(Guid OrganizationId)
    : IQuery<Result<IReadOnlyCollection<ActiveDayOffDto>>>;

public sealed class Handler : IRequestHandler<GetActiveDayOffsQuery, Result<IReadOnlyCollection<ActiveDayOffDto>>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserRepository _userRepository;

    public Handler(
        IOrganizationRepository organizationRepository,
        ITeamRepository teamRepository,
        ITeamMemberRepository teamMemberRepository,
        ITeamMemberDayOffRepository teamMemberDayOffRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository)
    {
        _organizationRepository = organizationRepository;
        _teamRepository = teamRepository;
        _teamMemberRepository = teamMemberRepository;
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyCollection<ActiveDayOffDto>>> Handle(
        GetActiveDayOffsQuery request,
        CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return Result<IReadOnlyCollection<ActiveDayOffDto>>.Failure(
                new AppError("organizations.not_found", "Organization not found."),
                (int)HttpStatusCode.NotFound);
        }

        var teams = await _teamRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        if (teams.Count == 0)
        {
            return Result<IReadOnlyCollection<ActiveDayOffDto>>.Success([]);
        }

        var teamIds = teams.Select(t => t.Id).ToList();
        var teamNameById = teams.ToDictionary(t => t.Id, t => t.Name);

        var allTeamMembers = new List<Domain.Entities.TeamMember>();
        foreach (var teamId in teamIds)
        {
            var members = await _teamMemberRepository.ListByTeamIdAsync(teamId, cancellationToken);
            allTeamMembers.AddRange(members);
        }

        if (allTeamMembers.Count == 0)
        {
            return Result<IReadOnlyCollection<ActiveDayOffDto>>.Success([]);
        }

        var teamMemberIds = allTeamMembers.Select(m => m.Id).Distinct().ToList();
        var now = DateTimeOffset.UtcNow;
        var activeDayOffs = await _teamMemberDayOffRepository.ListActiveByTeamMemberIdsAsync(
            teamMemberIds, now, cancellationToken);

        if (activeDayOffs.Count == 0)
        {
            return Result<IReadOnlyCollection<ActiveDayOffDto>>.Success([]);
        }

        var teamMemberById = allTeamMembers.ToDictionary(m => m.Id);

        var orgMembers = await _organizationMemberRepository
            .ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var orgMemberById = orgMembers.ToDictionary(m => m.Id);

        var userIds = orgMembers.Select(m => m.UserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? Array.Empty<Domain.Entities.User>()
            : await _userRepository.ListByIdsAsync(userIds, cancellationToken);
        var userById = users.ToDictionary(u => u.Id);

        var dtos = activeDayOffs
            .Select(dayOff =>
            {
                var teamMember = teamMemberById.GetValueOrDefault(dayOff.TeamMemberId);
                var teamName = teamMember is not null
                    ? teamNameById.GetValueOrDefault(teamMember.TeamId, "Unknown")
                    : "Unknown";

                string memberName = "Unknown";
                string? avatarUrl = null;

                if (teamMember is not null &&
                    orgMemberById.TryGetValue(teamMember.OrganizationMemberId, out var orgMember) &&
                    userById.TryGetValue(orgMember.UserId, out var user))
                {
                    var fullName = $"{user.FirstName} {user.LastName}".Trim();
                    memberName = string.IsNullOrWhiteSpace(fullName) ? user.Email : fullName;
                    avatarUrl = AvatarUrls.Resolve(user.Id, user.AvatarUrl);
                }

                return new ActiveDayOffDto(
                    dayOff.Id,
                    dayOff.TeamMemberId,
                    memberName,
                    avatarUrl,
                    teamName,
                    dayOff.StartAt,
                    dayOff.EndAt,
                    dayOff.Reason);
            })
            .ToList();

        return Result<IReadOnlyCollection<ActiveDayOffDto>>.Success(dtos);
    }
}
