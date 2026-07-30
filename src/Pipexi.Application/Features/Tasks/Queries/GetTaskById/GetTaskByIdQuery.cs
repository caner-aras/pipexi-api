using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<TaskDto>>
{
    public sealed class Handler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
    {
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IWorkTaskRepository workTaskRepository,
            ITaskCommentRepository taskCommentRepository,
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _workTaskRepository = workTaskRepository;
            _taskCommentRepository = taskCommentRepository;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _workTaskRepository.GetByIdAsync(request.Id, cancellationToken);
            if (task is null)
            {
                return Result<TaskDto>.Failure(
                    new AppError("tasks.not_found", "Task not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<TaskDto>(
                task.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            var dtos = await TaskHydration.BuildDtosAsync(
                [task],
                includeComments: true,
                taskCommentRepository: _taskCommentRepository,
                teamMemberRepository: _teamMemberRepository,
                teamRepository: _teamRepository,
                organizationMemberRepository: _organizationMemberRepository,
                userRepository: _userRepository,
                cancellationToken);

            return Result<TaskDto>.Success(dtos.Single());
        }
    }
}
