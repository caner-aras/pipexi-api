using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid Id,
    Guid? ShiftId,
    Guid? LocationId,
    string? Title,
    string? Description,
    Guid? AssignedToTeamMemberId,
    Guid? AssignedToTeamId,
    DateTimeOffset? DueAt,
    string? Priority,
    string? Status) : ICommand<Result<TaskDto>>
{
    public sealed class Handler : IRequestHandler<UpdateTaskCommand, Result<TaskDto>>
    {
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ITaskCommentRepository _taskCommentRepository;

        public Handler(
            IWorkTaskRepository workTaskRepository,
            IShiftRepository shiftRepository,
            ILocationRepository locationRepository,
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            ITaskCommentRepository taskCommentRepository)
        {
            _workTaskRepository = workTaskRepository;
            _shiftRepository = shiftRepository;
            _locationRepository = locationRepository;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _taskCommentRepository = taskCommentRepository;
        }

        public async Task<Result<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _workTaskRepository.GetByIdAsync(request.Id, cancellationToken);
            if (task is null)
            {
                return Result<TaskDto>.Failure(
                    new AppError("tasks.not_found", "Task not found."),
                    (int)HttpStatusCode.NotFound);
            }

            if (request.ShiftId.HasValue)
            {
                var shift = await _shiftRepository.GetByIdAsync(request.ShiftId.Value, cancellationToken);
                if (shift is null || shift.OrganizationId != task.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_shift", "Shift not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.LocationId.HasValue)
            {
                var location = await _locationRepository.GetByIdAsync(request.LocationId.Value, cancellationToken);
                if (location is null || location.OrganizationId != task.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_location", "Location not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.AssignedToTeamMemberId.HasValue)
            {
                var teamMember = await _teamMemberRepository.GetByIdAsync(request.AssignedToTeamMemberId.Value, cancellationToken);
                if (teamMember is null)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_assigned_team_member", "Assigned team member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }

                var assigneeTeam = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
                if (assigneeTeam is null || assigneeTeam.OrganizationId != task.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_assigned_team_member", "Assigned team member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.AssignedToTeamId.HasValue)
            {
                var team = await _teamRepository.GetByIdAsync(request.AssignedToTeamId.Value, cancellationToken);
                if (team is null || team.OrganizationId != task.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_assigned_team", "Assigned team not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            task.UpdateDetails(
                request.ShiftId,
                request.LocationId,
                request.Title,
                request.Description,
                request.AssignedToTeamMemberId,
                request.AssignedToTeamId,
                request.DueAt,
                request.Priority,
                request.Status);

            await _workTaskRepository.UpdateAsync(task, cancellationToken);

            var comments = await _taskCommentRepository.ListByWorkTaskIdAsync(task.Id, cancellationToken);
            return Result<TaskDto>.Success(task.ToDto(comments.Select(x => x.ToDto()).ToList()), (int)HttpStatusCode.OK);
        }
    }
}
