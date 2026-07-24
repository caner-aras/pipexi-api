using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMemberDayOffs;

public sealed record GetTeamMemberDayOffsQuery(
    Guid TeamMemberId,
    DateTimeOffset FromAt) : IQuery<Result<IReadOnlyCollection<TeamMemberDayOffDto>>>;

public sealed class Handler : IRequestHandler<GetTeamMemberDayOffsQuery, Result<IReadOnlyCollection<TeamMemberDayOffDto>>>
{
    private readonly ITeamMemberDayOffRepository _teamMemberDayOffRepository;

    public Handler(ITeamMemberDayOffRepository teamMemberDayOffRepository)
    {
        _teamMemberDayOffRepository = teamMemberDayOffRepository;
    }

    public async Task<Result<IReadOnlyCollection<TeamMemberDayOffDto>>> Handle(GetTeamMemberDayOffsQuery request, CancellationToken cancellationToken)
    {
        var items = await _teamMemberDayOffRepository.ListByTeamMemberIdAsync(request.TeamMemberId, cancellationToken);
        var windowStart = request.FromAt;
        var windowEnd = request.FromAt.AddDays(30);

        items = items
            .Where(x => x.EndAt > windowStart && x.StartAt < windowEnd)
            .ToList();

        var dtos = items.Select(x => x.ToDto()).ToList();
        return Result<IReadOnlyCollection<TeamMemberDayOffDto>>.Success(dtos);
    }
}