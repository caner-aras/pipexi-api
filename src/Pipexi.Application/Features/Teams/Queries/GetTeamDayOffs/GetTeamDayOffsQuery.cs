using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamDayOffs;

public sealed record GetTeamDayOffsQuery(
    Guid TeamId,
    DateTimeOffset FromAt) : IQuery<Result<IReadOnlyCollection<TeamMemberDayOffDto>>>;

public sealed class Handler : IRequestHandler<GetTeamDayOffsQuery, Result<IReadOnlyCollection<TeamMemberDayOffDto>>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;

    public Handler(
        ITeamMemberRepository teamMemberRepository,
        ITeamMemberDayOffRepository teamMemberDayOffRepository)
    {
        _teamMemberRepository = teamMemberRepository;
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
    }

    public async Task<Result<IReadOnlyCollection<TeamMemberDayOffDto>>> Handle(GetTeamDayOffsQuery request, CancellationToken cancellationToken)
    {
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
