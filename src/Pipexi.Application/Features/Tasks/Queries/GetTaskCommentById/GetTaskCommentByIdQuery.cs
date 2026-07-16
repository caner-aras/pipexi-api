using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Tasks.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Tasks.Queries.GetTaskCommentById;

public sealed record GetTaskCommentByIdQuery(Guid Id) : IQuery<Result<TaskCommentDto>>
{
    public sealed class Handler : IRequestHandler<GetTaskCommentByIdQuery, Result<TaskCommentDto>>
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

        public async Task<Result<TaskCommentDto>> Handle(GetTaskCommentByIdQuery request, CancellationToken cancellationToken)
        {
            var comment = await _taskCommentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (comment is null)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("task_comments.not_found", "Task comment not found."),
                    (int)HttpStatusCode.NotFound);
            }

            TaskCommentMemberDto? member = null;
            var teamMember = await _teamMemberRepository.GetByIdAsync(comment.TeamMemberId, cancellationToken);
            if (teamMember is not null)
            {
                var organizationMember = await _organizationMemberRepository.GetByIdAsync(teamMember.OrganizationMemberId, cancellationToken);
                if (organizationMember is not null)
                {
                    var user = await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);
                    member = teamMember.ToCommentMemberDto(organizationMember, user);
                }
            }

            return Result<TaskCommentDto>.Success(comment.ToDto(member));
        }
    }
}
