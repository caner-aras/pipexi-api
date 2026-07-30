using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Commands.DeleteTeamMember;

public sealed record DeleteTeamMemberCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteTeamMemberCommand, Result<object?>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
        }

        public async Task<Result<object?>> Handle(DeleteTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var teamMember = await _teamMemberRepository.GetByIdAsync(request.Id, cancellationToken);
            if (teamMember is null)
            {
                return Result<object?>.Failure(
                    new AppError("team_members.not_found", "Team member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var team = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
            if (team is null)
            {
                return Result<object?>.Failure(
                    new AppError("teams.not_found", "Team not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                team.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _teamMemberRepository.DeleteAsync(teamMember, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
