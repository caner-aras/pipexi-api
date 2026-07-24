using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations;
using Pipexi.Application.Features.Shifts;
using Pipexi.Application.Features.Tasks;
using Pipexi.Application.Features.Tasks.Dtos;
using Pipexi.Application.Features.Teams;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMemberTasksById;

public sealed record GetTeamMemberTasksByIdQuery(Guid TeamMemberId) : IQuery<Result<IReadOnlyCollection<TaskDto>>>
{
    public sealed class Handler : IRequestHandler<GetTeamMemberTasksByIdQuery, Result<IReadOnlyCollection<TaskDto>>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IShiftRepository _shiftRepository;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            IWorkTaskRepository workTaskRepository,
            ILocationRepository locationRepository,
            IShiftRepository shiftRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _workTaskRepository = workTaskRepository;
            _locationRepository = locationRepository;
            _shiftRepository = shiftRepository;
        }

        public async Task<Result<IReadOnlyCollection<TaskDto>>> Handle(GetTeamMemberTasksByIdQuery request, CancellationToken cancellationToken)
        {
            var teamMember = await _teamMemberRepository.GetByIdAsync(request.TeamMemberId, cancellationToken);
            if (teamMember is null)
            {
                return Result<IReadOnlyCollection<TaskDto>>.Failure(
                    new AppError("team_members.not_found", "Team member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var tasks = await _workTaskRepository
                .ListByAssignedTeamMemberIdAsync(teamMember.Id, cancellationToken);

            var assignedTeamMemberIds = tasks
                .Where(x => x.AssignedToTeamMemberId.HasValue)
                .Select(x => x.AssignedToTeamMemberId!.Value)
                .Distinct()
                .ToList();
            var locationIds = tasks
                .Where(x => x.LocationId.HasValue)
                .Select(x => x.LocationId!.Value)
                .Distinct()
                .ToList();
            var shiftIds = tasks
                .Where(x => x.ShiftId.HasValue)
                .Select(x => x.ShiftId!.Value)
                .Distinct()
                .ToList();

            var assignedTeamMembers = assignedTeamMemberIds.Count == 0
                ? []
                : await _teamMemberRepository.GetByIdsAsync(assignedTeamMemberIds, cancellationToken);
            var locations = locationIds.Count == 0
                ? []
                : await _locationRepository.GetByIdsAsync(locationIds, cancellationToken);
            var shifts = shiftIds.Count == 0
                ? []
                : await _shiftRepository.GetByIdsAsync(shiftIds, cancellationToken);

            var assignedTeamMemberMap = assignedTeamMembers.ToDictionary(x => x.Id, x => x.ToDto());
            var locationMap = locations.ToDictionary(x => x.Id, x => x.ToDto());
            var shiftMap = shifts.ToDictionary(x => x.Id, x => x.ToDto());

            var dtos = tasks
                .Select(x => x.ToDto(Array.Empty<TaskCommentDto>()) with
                {
                    AssignedToTeamMember = x.AssignedToTeamMemberId.HasValue
                        ? assignedTeamMemberMap.GetValueOrDefault(x.AssignedToTeamMemberId.Value)
                        : null,
                    Location = x.LocationId.HasValue
                        ? locationMap.GetValueOrDefault(x.LocationId.Value)
                        : null,
                    Shift = x.ShiftId.HasValue
                        ? shiftMap.GetValueOrDefault(x.ShiftId.Value)
                        : null
                })
                .ToList();

            return Result<IReadOnlyCollection<TaskDto>>.Success(dtos);
        }
    }
}
