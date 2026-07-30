using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamDayOffs;

public sealed record GetTeamDayOffsQuery(
    Guid TeamId,
    DateTimeOffset FromAt,
    Guid? ScopedOrganizationId = null) : IQuery<Result<IReadOnlyCollection<TeamMemberDayOffDto>>>;

public sealed class Handler : IRequestHandler<GetTeamDayOffsQuery, Result<IReadOnlyCollection<TeamMemberDayOffDto>>>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;
    private readonly IOrganizationAccessService _organizationAccess;

    public Handler(
        ITeamRepository teamRepository,
        ITeamMemberRepository teamMemberRepository,
        ITeamMemberDayOffRepository teamMemberDayOffRepository,
        IOrganizationAccessService organizationAccess)
    {
        _teamRepository = teamRepository;
        _teamMemberRepository = teamMemberRepository;
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
        _organizationAccess = organizationAccess;
    }

    public async Task<Result<IReadOnlyCollection<TeamMemberDayOffDto>>> Handle(GetTeamDayOffsQuery request, CancellationToken cancellationToken)
    {
        var team = await _teamRepository.GetByIdAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            return Result<IReadOnlyCollection<TeamMemberDayOffDto>>.Failure(
                new AppError("teams.not_found", "Team not found."),
                (int)HttpStatusCode.NotFound);
        }

        var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<IReadOnlyCollection<TeamMemberDayOffDto>>(
            team.OrganizationId, request.ScopedOrganizationId, cancellationToken);
        if (accessDenied is not null) return accessDenied;

        var teamMembers = await _teamMemberRepository.ListByTeamIdAsync(request.TeamId, cancellationToken);
        var teamMemberIds = teamMembers.Select(x => x.Id).Distinct().ToList();

        var items = await _teamMemberDayOffRepository.ListByTeamMemberIdsAsync(teamMemberIds, cancellationToken);

        var windowStart = request.FromAt;
        var windowEnd = request.FromAt.AddDays(30);

        var dtos = items
            .Where(x => x.EndAt > windowStart && x.StartAt < windowEnd)
            .Select(x => x.ToDto())
            .ToList();

        return Result<IReadOnlyCollection<TeamMemberDayOffDto>>.Success(dtos);
    }
}
