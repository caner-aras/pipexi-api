using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Shifts;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Application.Features.TimeEntries;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Queries.GetOrganizationShifts;

public sealed record GetOrganizationShiftsQuery(Guid OrganizationId, DateTimeOffset? FromDate = null, DateTimeOffset? ToDate = null)
    : IRequest<Result<OrganizationShiftsDto>>;

public sealed class Handler : IRequestHandler<GetOrganizationShiftsQuery, Result<OrganizationShiftsDto>>
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
    private readonly ITeamMemberRepository _teamMemberRepository;

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
        ITeamMemberRepository teamMemberRepository)
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
        _teamMemberRepository = teamMemberRepository;
    }

    public async Task<Result<OrganizationShiftsDto>> Handle(GetOrganizationShiftsQuery request, CancellationToken cancellationToken)
    {
        var windowStart = request.FromDate ?? DateTimeOffset.UtcNow;
        var windowEnd = request.ToDate ?? windowStart.AddDays(7);

        var shifts = await _shiftRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        shifts = shifts
            .Where(x => x.StartAt >= windowStart && x.StartAt < windowEnd)
            .ToList();

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

        var locations = await _locationRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var locationIds = locations.Select(x => x.Id).ToList();
        var locationWorkingHours = await _locationWorkingHourRepository.ListByLocationIdsAsync(locationIds, cancellationToken);
        var locationWorkingHoursMap = locationWorkingHours
            .GroupBy(x => x.LocationId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<LocationWorkingHourDto>)g
                    .OrderBy(x => x.DayOfWeek)
                    .Select(x => x.ToDto())
                    .ToList());

        var locationDtos = locations
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto(locationWorkingHoursMap.GetValueOrDefault(x.Id)))
            .ToList();

        var teamMemberLookup = await ShiftTeamMemberLookup.CreateAsync(
            _teamMemberRepository,
            shifts,
            cancellationToken);

        var shiftDtos = shifts
            .Select(x =>
            {
                var resolved = ShiftTeamMemberLookup.ResolveInfo(x.TeamId, x.OrganizationMemberId, teamMemberLookup);
                var effectiveTeamId = resolved.ResolvedTeamId;
                var teamDto = effectiveTeamId.HasValue ? teamMap.GetValueOrDefault(effectiveTeamId.Value) : null;

                return x.ToOrganizationShiftDto(
                    teamDto,
                    x.OrganizationMemberId.HasValue ? organizationMemberMap.GetValueOrDefault(x.OrganizationMemberId.Value) : null,
                    breakMap.GetValueOrDefault(x.Id) ?? Array.Empty<ShiftBreakDto>(),
                    timeEntriesByShiftId.GetValueOrDefault(x.Id) ?? Array.Empty<TimeEntryDto>(),
                    resolved.TeamMemberId);
            })
            .ToList();

        return Result<OrganizationShiftsDto>.Success(new OrganizationShiftsDto(
            request.OrganizationId,
            locationDtos,
            shiftDtos));
    }
}