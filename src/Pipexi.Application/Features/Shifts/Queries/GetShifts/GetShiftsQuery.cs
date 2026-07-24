using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Application.Features.TimeEntries;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Queries.GetShifts;

public sealed record GetShiftsQuery(Guid? OrganizationId, Guid? OrganizationMemberId = null) : IQuery<Result<IReadOnlyCollection<ShiftDto>>>
{
    public sealed class Handler : IRequestHandler<GetShiftsQuery, Result<IReadOnlyCollection<ShiftDto>>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftBreakRepository _shiftBreakRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationWorkingHourRepository _locationWorkingHourRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IShiftRepository shiftRepository,
            IShiftBreakRepository shiftBreakRepository,
            ITeamRepository teamRepository,
            ILocationRepository locationRepository,
            ILocationWorkingHourRepository locationWorkingHourRepository,
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            ICurrentUserContext currentUserContext)
        {
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
            _teamRepository = teamRepository;
            _locationRepository = locationRepository;
            _locationWorkingHourRepository = locationWorkingHourRepository;
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<ShiftDto>>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<ShiftDto>>.Failure(
                    new AppError("auth.unauthorized", "Unauthorized."),
                    (int)HttpStatusCode.Unauthorized);
            }

            IReadOnlyCollection<Pipexi.Domain.Entities.Shift> shifts;
            if (request.OrganizationMemberId.HasValue)
            {
                shifts = await _shiftRepository.ListByOrganizationMemberIdAsync(request.OrganizationMemberId.Value, cancellationToken);
                shifts = shifts.Where(x => x.OrganizationId == organizationId).ToList();
            }
            else
            {
                shifts = await _shiftRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            }

            var breakMap = new Dictionary<Guid, IReadOnlyCollection<ShiftBreakDto>>();
            var shiftIds = shifts.Select(x => x.Id).ToList();
            if (shiftIds.Count > 0)
            {
                var shiftBreaks = await _shiftBreakRepository.ListByShiftIdsAsync(shiftIds, cancellationToken);
                breakMap = shiftBreaks
                    .GroupBy(x => x.ShiftId)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyCollection<ShiftBreakDto>)g.OrderBy(x => x.StartAt).Select(x => x.ToDto()).ToList());
            }

            var timeEntriesByShiftId = new Dictionary<Guid, IReadOnlyCollection<TimeEntryDto>>();
            if (shiftIds.Count > 0)
            {
                var timeEntries = await _timeEntryRepository.ListByShiftIdsAsync(shiftIds, cancellationToken);
                var timeEntryIds = timeEntries.Select(x => x.Id).ToList();
                var timeEntryBreakMap = new Dictionary<Guid, IReadOnlyCollection<TimeEntryBreakDto>>();

                if (timeEntryIds.Count > 0)
                {
                    var timeEntryBreaks = await _timeEntryBreakRepository.ListByTimeEntryIdsAsync(timeEntryIds, cancellationToken);
                    timeEntryBreakMap = timeEntryBreaks
                        .GroupBy(x => x.TimeEntryId)
                        .ToDictionary(
                            g => g.Key,
                            g => (IReadOnlyCollection<TimeEntryBreakDto>)g.OrderBy(x => x.StartAt).Select(x => x.ToDto()).ToList());
                }

                timeEntriesByShiftId = timeEntries
                    .GroupBy(x => x.ShiftId)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyCollection<TimeEntryDto>)g
                            .OrderBy(x => x.ClockInAt)
                            .Select(x => x.ToDto(timeEntryBreakMap.GetValueOrDefault(x.Id) ?? Array.Empty<TimeEntryBreakDto>()))
                            .ToList());
            }

            var teamMap = new Dictionary<Guid, TeamDto>();
            var teamIds = shifts
                .Where(x => x.TeamId.HasValue)
                .Select(x => x.TeamId!.Value)
                .Distinct()
                .ToList();

            if (teamIds.Count > 0)
            {
                var teams = await _teamRepository.GetByIdsAsync(teamIds, cancellationToken);
                teamMap = teams.ToDictionary(x => x.Id, x => x.ToDto());
            }

            var locationMap = new Dictionary<Guid, LocationDto>();
            var locationIds = shifts
                .Select(x => x.LocationId)
                .Distinct()
                .ToList();

            if (locationIds.Count > 0)
            {
                var locations = await _locationRepository.GetByIdsAsync(locationIds, cancellationToken);
                var workingHours = await _locationWorkingHourRepository.ListByLocationIdsAsync(locationIds, cancellationToken);
                var workingHoursMap = workingHours
                    .GroupBy(x => x.LocationId)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyCollection<LocationWorkingHourDto>)g
                            .OrderBy(x => x.DayOfWeek)
                            .Select(x => x.ToDto())
                            .ToList());

                locationMap = locations.ToDictionary(
                    x => x.Id,
                    x => x.ToDto(workingHoursMap.GetValueOrDefault(x.Id)));
            }

            var organizationMemberMap = new Dictionary<Guid, OrganizationMemberDto>();
            var organizationMemberIds = shifts
                .Where(x => x.OrganizationMemberId.HasValue)
                .Select(x => x.OrganizationMemberId!.Value)
                .Distinct()
                .ToList();

            if (organizationMemberIds.Count > 0)
            {
                var organizationMembers = await _organizationMemberRepository.GetByIdsAsync(organizationMemberIds, cancellationToken);
                var userIds = organizationMembers.Select(x => x.UserId).Distinct().ToList();
                var users = await _userRepository.ListByIdsAsync(userIds, cancellationToken);
                var userMap = users.ToDictionary(x => x.Id, x => x.ToDto());

                organizationMemberMap = organizationMembers.ToDictionary(
                    x => x.Id,
                    x => x.ToDto(userMap.GetValueOrDefault(x.UserId)));
            }

            var dtos = shifts
                .Select(x => x.ToDto(
                    x.TeamId.HasValue ? teamMap.GetValueOrDefault(x.TeamId.Value) : null,
                    x.OrganizationMemberId.HasValue
                        ? organizationMemberMap.GetValueOrDefault(x.OrganizationMemberId.Value)
                        : null,
                    locationMap.GetValueOrDefault(x.LocationId),
                    breakMap.GetValueOrDefault(x.Id) ?? Array.Empty<ShiftBreakDto>(),
                    timeEntriesByShiftId.GetValueOrDefault(x.Id) ?? Array.Empty<TimeEntryDto>()))
                .ToList();

            return Result<IReadOnlyCollection<ShiftDto>>.Success(dtos);
        }
    }
}
