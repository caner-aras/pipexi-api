using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Application.Features.Locations;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Shifts;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams;
using Pipexi.Application.Features.TimeEntries;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Queries.GetShiftById;

public sealed record GetShiftByIdQuery(Guid Id) : IQuery<Result<ShiftDto>>
{
    public sealed class Handler : IRequestHandler<GetShiftByIdQuery, Result<ShiftDto>>
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
        private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IFormSubmissionRepository _formSubmissionRepository;
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
            IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
            IFormTemplateRepository formTemplateRepository,
            IFormSubmissionRepository formSubmissionRepository,
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
            _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
            _formTemplateRepository = formTemplateRepository;
            _formSubmissionRepository = formSubmissionRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        public async Task<Result<ShiftDto>> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shift is null)
            {
                return Result<ShiftDto>.Failure(
                    new AppError("shifts.not_found", "Shift not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var breaks = await _shiftBreakRepository.ListByShiftIdAsync(shift.Id, cancellationToken);
            var team = shift.TeamId.HasValue
                ? await _teamRepository.GetByIdAsync(shift.TeamId.Value, cancellationToken)
                : null;
            var organizationMember = shift.OrganizationMemberId.HasValue
                ? await _organizationMemberRepository.GetByIdAsync(shift.OrganizationMemberId.Value, cancellationToken)
                : null;
            var user = organizationMember is null
                ? null
                : await _userRepository.GetByIdAsync(organizationMember.UserId, cancellationToken);
            var location = await _locationRepository.GetByIdAsync(shift.LocationId, cancellationToken);
            IReadOnlyCollection<Pipexi.Application.Features.Locations.Dtos.LocationWorkingHourDto> locationWorkingHours = location is null
                ? Array.Empty<Pipexi.Application.Features.Locations.Dtos.LocationWorkingHourDto>()
                : (await _locationWorkingHourRepository.ListByLocationIdAsync(location.Id, cancellationToken))
                    .OrderBy(x => x.DayOfWeek)
                    .Select(x => x.ToDto())
                    .ToList();

            var timeEntries = await _timeEntryRepository.ListByShiftIdsAsync(new[] { shift.Id }, cancellationToken);
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
                .OrderBy(x => x.ClockInAt)
                .Select(x => x.ToDto(timeEntryBreakMap.GetValueOrDefault(x.Id) ?? Array.Empty<TimeEntryBreakDto>()))
                .ToList();

            var requiredTemplateIds = await _shiftRequiredFormTemplateRepository
                .ListRequiredTemplateIdsByShiftIdAsync(shift.Id, cancellationToken);
            var shiftFormTemplates = new List<ShiftFormTemplateDto>();

            if (requiredTemplateIds.Count > 0)
            {
                var templates = await _formTemplateRepository.GetByIdsAsync(requiredTemplateIds, cancellationToken);
                var submittedTemplateIds = await _formSubmissionRepository.ListSubmittedTemplateIdsByShiftAsync(
                    shift.Id,
                    cancellationToken);
                var submittedTemplateIdSet = submittedTemplateIds.ToHashSet();

                shiftFormTemplates = templates
                    .OrderBy(x => x.Name)
                    .Select(x => new ShiftFormTemplateDto(
                        x.Id,
                        x.OrganizationId,
                        x.Name,
                        x.Description,
                        x.Status,
                        x.CreatedAt,
                        x.UpdatedAt,
                        submittedTemplateIdSet.Contains(x.Id)))
                    .ToList();
            }

            var teamMemberLookup = await ShiftTeamMemberLookup.CreateAsync(
                _teamMemberRepository,
                new[] { shift },
                cancellationToken);
            var teamMemberId = ShiftTeamMemberLookup.Resolve(
                shift.TeamId,
                shift.OrganizationMemberId,
                teamMemberLookup);

            return Result<ShiftDto>.Success(
                shift.ToDto(
                    team?.ToDto(),
                    organizationMember?.ToDto(user?.ToDto()),
                    location?.ToDto(locationWorkingHours),
                    breaks.Select(x => x.ToDto()).ToList(),
                    timeEntryDtos,
                    shiftFormTemplates,
                    teamMemberId));
        }
    }
}
