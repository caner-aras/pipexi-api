using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.OrganizationMembers.Dtos;
using Pipexi.Application.Features.Shifts;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams.Dtos;
using Pipexi.Application.Features.TimeEntries;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Teams.Queries.GetTeamMemberDetailsById;

public sealed record GetTeamMemberDetailsByIdQuery(Guid Id, DateTimeOffset? FromDate = null) : IQuery<Result<TeamMemberDetailsDto>>
{
    public sealed class Handler : IRequestHandler<GetTeamMemberDetailsByIdQuery, Result<TeamMemberDetailsDto>>
    {
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftBreakRepository _shiftBreakRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IWorkTaskRepository _workTaskRepository;

        public Handler(
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            IOrganizationRepository organizationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            IShiftRepository shiftRepository,
            IShiftBreakRepository shiftBreakRepository,
            ILocationRepository locationRepository,
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            IWorkTaskRepository workTaskRepository)
        {
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _organizationRepository = organizationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
            _locationRepository = locationRepository;
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _workTaskRepository = workTaskRepository;
        }

        public async Task<Result<TeamMemberDetailsDto>> Handle(GetTeamMemberDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            var teamMember = await _teamMemberRepository.GetByIdAsync(request.Id, cancellationToken);
            if (teamMember is null)
            {
                return Result<TeamMemberDetailsDto>.Failure(
                    new AppError("team_members.not_found", "Team member not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var team = await _teamRepository.GetByIdAsync(teamMember.TeamId, cancellationToken);
            var organizationMember = await _organizationMemberRepository.GetByIdAsync(
                teamMember.OrganizationMemberId,
                cancellationToken);
            var user = organizationMember is null
                ? null
                : await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);

            if (organizationMember is null)
            {
                return Result<TeamMemberDetailsDto>.Failure(
                    new AppError("team_members.invalid_organization_member", "Organization member not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var windowStart = request.FromDate ?? DateTimeOffset.UtcNow;
            var windowEnd = windowStart.AddDays(7);

            var organization = await _organizationRepository.GetByIdAsync(organizationMember.OrganizationId, cancellationToken);

            var shifts = await _shiftRepository.ListByOrganizationMemberIdAsync(organizationMember.Id, cancellationToken);
            shifts = shifts
                .Where(x => x.StartAt >= windowStart && x.StartAt < windowEnd)
                .ToList();
            var teamIds = shifts
                .Where(x => x.TeamId.HasValue)
                .Select(x => x.TeamId!.Value)
                .Distinct()
                .ToList();
            var shiftTeams = teamIds.Count > 0
                ? await _teamRepository.GetByIdsAsync(teamIds, cancellationToken)
                : Array.Empty<Pipexi.Domain.Entities.Team>();
            var shiftTeamMap = shiftTeams.ToDictionary(x => x.Id, x => x.ToDto());

            var locationIds = shifts.Select(x => x.LocationId).Distinct().ToList();
            var shiftLocations = locationIds.Count > 0
                ? await _locationRepository.GetByIdsAsync(locationIds, cancellationToken)
                : Array.Empty<Pipexi.Domain.Entities.Location>();
            var shiftLocationMap = shiftLocations.ToDictionary(x => x.Id, x => x.ToDto());

            var shiftIds = shifts.Select(x => x.Id).ToList();
            var shiftBreakMap = new Dictionary<Guid, IReadOnlyCollection<ShiftBreakDto>>();
            if (shiftIds.Count > 0)
            {
                var shiftBreaks = await _shiftBreakRepository.ListByShiftIdsAsync(shiftIds, cancellationToken);
                shiftBreakMap = shiftBreaks
                    .GroupBy(x => x.ShiftId)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyCollection<ShiftBreakDto>)g.OrderBy(x => x.StartAt).Select(x => x.ToDto()).ToList());
            }

            var shiftOrganizationMemberIds = shifts
                .Where(x => x.OrganizationMemberId.HasValue)
                .Select(x => x.OrganizationMemberId!.Value)
                .Distinct()
                .ToList();

            var shiftOrganizationMemberMap = new Dictionary<Guid, OrganizationMemberDto>();
            if (shiftOrganizationMemberIds.Count > 0)
            {
                var shiftOrganizationMembers = await _organizationMemberRepository.GetByIdsAsync(shiftOrganizationMemberIds, cancellationToken);
                var shiftUserIds = shiftOrganizationMembers.Select(x => x.UserId).Distinct().ToList();
                var shiftUsers = shiftUserIds.Count > 0
                    ? await _userRepository.ListByIdsAsync(shiftUserIds, cancellationToken)
                    : Array.Empty<Pipexi.Domain.Entities.User>();
                var shiftUserMap = shiftUsers.ToDictionary(x => x.Id, x => x.ToDto());

                shiftOrganizationMemberMap = shiftOrganizationMembers.ToDictionary(
                    x => x.Id,
                    x => x.ToDto(shiftUserMap.GetValueOrDefault(x.UserId)));
            }

            var shiftDtos = shifts
                .Select(x => x.ToDto(
                    x.TeamId.HasValue ? shiftTeamMap.GetValueOrDefault(x.TeamId.Value) : null,
                    x.OrganizationMemberId.HasValue
                        ? shiftOrganizationMemberMap.GetValueOrDefault(x.OrganizationMemberId.Value)
                        : null,
                    shiftLocationMap.GetValueOrDefault(x.LocationId),
                    shiftBreakMap.GetValueOrDefault(x.Id) ?? Array.Empty<ShiftBreakDto>()))
                .ToList();

            var timeEntries = await _timeEntryRepository.ListByOrganizationMemberIdAsync(organizationMember.Id, cancellationToken);
            timeEntries = timeEntries
                .Where(x => x.ClockInAt >= windowStart && x.ClockInAt < windowEnd)
                .ToList();
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

            var timeEntryDtos = timeEntries
                .Select(x => x.ToDto(timeEntryBreakMap.GetValueOrDefault(x.Id) ?? Array.Empty<TimeEntryBreakDto>()))
                .ToList();

            var totalTaskCount = (await _workTaskRepository
                .ListByAssignedTeamMemberIdAsync(teamMember.Id, cancellationToken))
                .Count;

            var teamMemberDto = teamMember.ToDto(team?.ToDto(), organizationMember.ToDto(user?.ToDto()));

            return Result<TeamMemberDetailsDto>.Success(
                new TeamMemberDetailsDto(teamMemberDto, organizationMember.OrganizationId, organization?.Name, shiftDtos, timeEntryDtos, totalTaskCount));
        }
    }
}
