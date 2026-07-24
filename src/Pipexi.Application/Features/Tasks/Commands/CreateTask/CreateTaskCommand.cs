using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    Guid OrganizationId,
    Guid? ShiftId,
    Guid? LocationId,
    string Title,
    string? Description,
    Guid? AssignedToTeamMemberId,
    Guid? AssignedToTeamId,
    DateTimeOffset? DueAt,
    string? Priority) : ICommand<Result<TaskDto>>
{
    public sealed class Handler : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IOrganizationRepository organizationRepository,
            IShiftRepository shiftRepository,
            ILocationRepository locationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IWorkTaskRepository workTaskRepository,
            ICurrentUserContext currentUserContext)
        {
            _organizationRepository = organizationRepository;
            _shiftRepository = shiftRepository;
            _locationRepository = locationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _workTaskRepository = workTaskRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<TaskDto>.Failure(
                    new AppError("tasks.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var reporterUserId = _currentUserContext.UserId;
            if (reporterUserId == Guid.Empty)
            {
                return Result<TaskDto>.Failure(
                    new AppError("tasks.unauthorized", "Current user could not be resolved from token."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var reporterMembership = await _organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                request.OrganizationId,
                reporterUserId,
                cancellationToken);
            if (reporterMembership is null)
            {
                return Result<TaskDto>.Failure(
                    new AppError("tasks.invalid_reporter", "Current user is not a member of this organization."),
                    (int)HttpStatusCode.Forbidden);
            }

            if (request.ShiftId.HasValue)
            {
                var shift = await _shiftRepository.GetByIdAsync(request.ShiftId.Value, cancellationToken);
                if (shift is null || shift.OrganizationId != request.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_shift", "Shift not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.LocationId.HasValue)
            {
                var location = await _locationRepository.GetByIdAsync(request.LocationId.Value, cancellationToken);
                if (location is null || location.OrganizationId != request.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_location", "Location not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            Guid? assignedToTeamMemberId = null;

            if (request.AssignedToTeamMemberId.HasValue)
            {
                var assignedTeamMember = await _teamMemberRepository.GetByIdAsync(
                    request.AssignedToTeamMemberId.Value,
                    cancellationToken);
                if (assignedTeamMember is null)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_assigned_team_member", "Assigned team member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }

                var assignedTeam = await _teamRepository.GetByIdAsync(assignedTeamMember.TeamId, cancellationToken);
                if (assignedTeam is null || assignedTeam.OrganizationId != request.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_assigned_team_member", "Assigned team member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }

                if (request.AssignedToTeamId.HasValue && assignedTeamMember.TeamId != request.AssignedToTeamId.Value)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_assigned_team_member", "Assigned team member is not in assigned team."),
                        (int)HttpStatusCode.BadRequest);
                }

                assignedToTeamMemberId = assignedTeamMember.Id;
            }

            if (request.AssignedToTeamId.HasValue)
            {
                var team = await _teamRepository.GetByIdAsync(request.AssignedToTeamId.Value, cancellationToken);
                if (team is null || team.OrganizationId != request.OrganizationId)
                {
                    return Result<TaskDto>.Failure(
                        new AppError("tasks.invalid_assigned_team", "Assigned team not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var task = WorkTask.Create(
                request.OrganizationId,
                reporterUserId,
                request.ShiftId,
                request.LocationId,
                request.Title,
                request.Description,
                assignedToTeamMemberId,
                request.AssignedToTeamId,
                request.DueAt,
                request.Priority ?? "medium");

            await _workTaskRepository.AddAsync(task, cancellationToken);

            return Result<TaskDto>.Success(task.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
