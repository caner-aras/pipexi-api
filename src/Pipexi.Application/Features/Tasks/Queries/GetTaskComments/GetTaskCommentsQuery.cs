using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Queries.GetTaskComments;

public sealed record GetTaskCommentsQuery(Guid WorkTaskId) : IQuery<Result<IReadOnlyCollection<TaskCommentDto>>>
{
    public sealed class Handler : IRequestHandler<GetTaskCommentsQuery, Result<IReadOnlyCollection<TaskCommentDto>>>
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

        public async Task<Result<IReadOnlyCollection<TaskCommentDto>>> Handle(GetTaskCommentsQuery request, CancellationToken cancellationToken)
        {
            var comments = await _taskCommentRepository.ListByWorkTaskIdAsync(request.WorkTaskId, cancellationToken);
            var memberMap = await BuildMemberMapAsync(comments, cancellationToken);
            var dtos = comments.Select(x => x.ToDto(memberMap.GetValueOrDefault(x.TeamMemberId))).ToList();
            return Result<IReadOnlyCollection<TaskCommentDto>>.Success(dtos);
        }

        private async Task<Dictionary<Guid, TaskCommentMemberDto>> BuildMemberMapAsync(
            IReadOnlyCollection<Pipexi.Domain.Entities.TaskComment> comments,
            CancellationToken cancellationToken)
        {
            var teamMemberIds = comments.Select(x => x.TeamMemberId).Distinct().ToList();
            if (teamMemberIds.Count == 0)
            {
                return [];
            }

            var teamMembers = await _teamMemberRepository.GetByIdsAsync(teamMemberIds, cancellationToken);
            var organizationMemberIds = teamMembers.Select(x => x.OrganizationMemberId).Distinct().ToList();
            var organizationMembers = await _organizationMemberRepository.GetByIdsAsync(organizationMemberIds, cancellationToken);
            var userIds = organizationMembers.Select(x => x.UserId).Distinct().ToList();
            var users = await _userRepository.ListByIdsAsync(userIds, cancellationToken);

            var organizationMemberMap = organizationMembers.ToDictionary(x => x.Id);
            var userMap = users.ToDictionary(x => x.Id);
            var result = new Dictionary<Guid, TaskCommentMemberDto>();

            foreach (var teamMember in teamMembers)
            {
                if (!organizationMemberMap.TryGetValue(teamMember.OrganizationMemberId, out var organizationMember))
                {
                    continue;
                }

                userMap.TryGetValue(organizationMember.UserId, out var user);
                result[teamMember.Id] = teamMember.ToCommentMemberDto(organizationMember, user);
            }

            return result;
        }
    }
}
