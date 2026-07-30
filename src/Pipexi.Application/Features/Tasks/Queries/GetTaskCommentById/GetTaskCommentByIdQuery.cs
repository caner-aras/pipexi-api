using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Queries.GetTaskCommentById;

public sealed record GetTaskCommentByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<TaskCommentDto>>
{
    public sealed class Handler : IRequestHandler<GetTaskCommentByIdQuery, Result<TaskCommentDto>>
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            ITaskCommentRepository taskCommentRepository,
            ITeamMemberRepository teamMemberRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            IWorkTaskRepository workTaskRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _workTaskRepository = workTaskRepository;
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

            var task = await _workTaskRepository.GetByIdAsync(comment.WorkTaskId, cancellationToken);
            if (task is null)
            {
                return Result<TaskCommentDto>.Failure(
                    new AppError("resource.not_found", "Parent resource not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TaskCommentDto>(
                task.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
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
