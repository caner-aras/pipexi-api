using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.DeleteTeam;

public sealed record DeleteTeamCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTeamCommand, Result<object?>>
    {
        private readonly ITeamRepository _teamRepository;

        public Handler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<Result<object?>> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
        {
            var team = await _teamRepository.GetByIdAsync(request.Id, cancellationToken);
            if (team is null)
            {
                return Result<object?>.Failure(
                    new AppError("teams.not_found", "Team not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _teamRepository.DeleteAsync(team, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
