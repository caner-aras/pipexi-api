using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Application.Features.Teams.Queries.GetTeamMemberDetailsById;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMemberDetailsByOrganizationMember;

public sealed record GetTeamMemberDetailsByOrganizationMemberQuery(
    Guid OrganizationId,
    Guid OrganizationMemberId,
    DateTimeOffset? FromDate,
    Guid? TeamId) : IQuery<Result<TeamMemberDetailsDto>>
{
    public sealed class Handler : IRequestHandler<GetTeamMemberDetailsByOrganizationMemberQuery, Result<TeamMemberDetailsDto>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ISender _sender;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ISender sender)
        {
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _sender = sender;
        }

        public async Task<Result<TeamMemberDetailsDto>> Handle(
            GetTeamMemberDetailsByOrganizationMemberQuery request,
            CancellationToken cancellationToken)
        {
            var organizationMember = await _organizationMemberRepository.GetByIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            if (organizationMember is null || organizationMember.OrganizationId != request.OrganizationId)
            {
                return Result<TeamMemberDetailsDto>.Failure(
                    new AppError("organization_members.not_found", "Organization member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var teamMembers = await _teamMemberRepository.ListByOrganizationMemberIdAsync(
                request.OrganizationMemberId,
                cancellationToken);

            var candidates = new List<TeamMember>();

            foreach (var teamMember in teamMembers)
            {
                var team = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
                if (team is null || team.OrganizationId != request.OrganizationId)
                {
                    continue;
                }

                if (request.TeamId.HasValue && teamMember.TeamId != request.TeamId.Value)
                {
                    continue;
                }

                candidates.Add(teamMember);
            }

            if (candidates.Count == 0)
            {
                return Result<TeamMemberDetailsDto>.Failure(
                    new AppError("team_members.not_found", "Team member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            if (candidates.Count > 1 && !request.TeamId.HasValue)
            {
                return Result<TeamMemberDetailsDto>.Failure(
                    new AppError(
                        "team_members.ambiguous",
                        "Member belongs to multiple teams. Specify teamId."),
                    (int)HttpStatusCode.Conflict);
            }

            var selected = candidates[0];
            return await _sender.Send(
                new GetTeamMemberDetailsByIdQuery(selected.Id, request.FromDate),
                cancellationToken);
        }
    }
}
