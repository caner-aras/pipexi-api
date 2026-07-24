using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Queries.GetTaskCommentsByOrganizationMemberId;

public sealed record GetTaskCommentsByOrganizationMemberIdQuery(Guid OrganizationMemberId)
    : IQuery<Result<IReadOnlyCollection<TaskCommentDto>>>
{
    public sealed class Handler : IRequestHandler<GetTaskCommentsByOrganizationMemberIdQuery, Result<IReadOnlyCollection<TaskCommentDto>>>
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            ITaskCommentRepository taskCommentRepository,
            ITeamMemberRepository teamMemberRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository)
        {
            _taskCommentRepository = taskCommentRepository;
            _teamMemberRepository = teamMemberRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<IReadOnlyCollection<TaskCommentDto>>> Handle(
            GetTaskCommentsByOrganizationMemberIdQuery request,
            CancellationToken cancellationToken)
        {
            var teamMembers = await _teamMemberRepository.ListByOrganizationMemberIdAsync(request.OrganizationMemberId, cancellationToken);
            var teamMemberIds = teamMembers.Select(x => x.Id).Distinct().ToList();
            var comments = await _taskCommentRepository.ListByTeamMemberIdsAsync(teamMemberIds, cancellationToken);

            var organizationMember = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId, cancellationToken);
            var user = organizationMember is null
                ? null
                : await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            var memberMap = teamMembers.ToDictionary(
                x => x.Id,
                x => organizationMember is null
                    ? null
                    : x.ToCommentMemberDto(organizationMember, user));

            var dtos = comments.Select(x => x.ToDto(memberMap.GetValueOrDefault(x.TeamMemberId))).ToList();
            return Result<IReadOnlyCollection<TaskCommentDto>>.Success(dtos);
        }
    }
}
