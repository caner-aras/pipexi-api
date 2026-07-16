using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.TimeEntries.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.TimeEntries.Commands.CreateTimeEntry;

public sealed record CreateTimeEntryBreakInput(DateTimeOffset StartAt, DateTimeOffset EndAt, bool IsPaid);

public sealed record CreateTimeEntryCommand(
    Guid OrganizationId,
    Guid ShiftId,
    Guid OrganizationMemberId,
    Guid LocationId,
    DateTimeOffset ClockInAt,
    DateTimeOffset? ClockOutAt,
    string? EmployeeNote,
    string? ManagerNote,
    IReadOnlyCollection<CreateTimeEntryBreakInput>? Breaks) : ICommand<Result<TimeEntryDto>>
{
    public sealed class Handler : IRequestHandler<CreateTimeEntryCommand, Result<TimeEntryDto>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
        private readonly IFormSubmissionRepository _formSubmissionRepository;

        public Handler(
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            IOrganizationRepository organizationRepository,
            IShiftRepository shiftRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ILocationRepository locationRepository,
            IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
            IFormSubmissionRepository formSubmissionRepository)
        {
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _organizationRepository = organizationRepository;
            _shiftRepository = shiftRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _locationRepository = locationRepository;
            _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
            _formSubmissionRepository = formSubmissionRepository;
        }

        public async Task<Result<TimeEntryDto>> Handle(CreateTimeEntryCommand request, CancellationToken cancellationToken)
        {
            if (request.ClockOutAt.HasValue && request.ClockOutAt.Value <= request.ClockInAt)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.invalid_range", "ClockOutAt must be after ClockInAt."),
                    (int)HttpStatusCode.BadRequest);
            }

            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var shift = await _shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken);
            if (shift is null || shift.OrganizationId != request.OrganizationId)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.invalid_shift", "Shift not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var member = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId, cancellationToken);
            if (member is null || member.OrganizationId != request.OrganizationId)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.invalid_organization_member", "Organization member not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var requiredTemplateIds = await _shiftRequiredFormTemplateRepository.ListRequiredTemplateIdsByShiftIdAsync(
                request.ShiftId,
                cancellationToken);

            if (requiredTemplateIds.Count > 0)
            {
                var submittedTemplateIds = await _formSubmissionRepository.ListSubmittedTemplateIdsByShiftAndMemberAsync(
                    request.ShiftId,
                    request.OrganizationMemberId,
                    cancellationToken);

                var missingRequiredTemplateIds = requiredTemplateIds.Except(submittedTemplateIds).ToList();
                if (missingRequiredTemplateIds.Count > 0)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError(
                            "time_entries.missing_required_forms",
                            $"Required shift forms must be completed before time entry. Missing formTemplateIds: {string.Join(",", missingRequiredTemplateIds)}"),
                        (int)HttpStatusCode.Conflict);
                }
            }

            var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
            if (location is null || location.OrganizationId != request.OrganizationId)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.invalid_location", "Location not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var timeEntry = TimeEntry.Create(
                request.OrganizationId,
                request.ShiftId,
                request.OrganizationMemberId,
                request.LocationId,
                request.ClockInAt,
                request.ClockOutAt,
                request.EmployeeNote,
                request.ManagerNote);

            await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);

            var createdBreaks = new List<TimeEntryBreakDto>();
            var breakInputs = request.Breaks ?? Array.Empty<CreateTimeEntryBreakInput>();
            var orderedBreaks = breakInputs.OrderBy(x => x.StartAt).ToList();

            DateTimeOffset? previousBreakEnd = null;
            foreach (var breakInput in orderedBreaks)
            {
                if (breakInput.StartAt >= breakInput.EndAt)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entry_breaks.invalid_range", "Break end time must be after start time."),
                        (int)HttpStatusCode.BadRequest);
                }

                if (breakInput.StartAt < timeEntry.ClockInAt)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entry_breaks.out_of_entry", "Break must be within time entry range."),
                        (int)HttpStatusCode.BadRequest);
                }

                if (timeEntry.ClockOutAt.HasValue && breakInput.EndAt > timeEntry.ClockOutAt.Value)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entry_breaks.out_of_entry", "Break must be within time entry range."),
                        (int)HttpStatusCode.BadRequest);
                }

                if (previousBreakEnd.HasValue && breakInput.StartAt < previousBreakEnd.Value)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entry_breaks.overlap", "Break overlaps with another break."),
                        (int)HttpStatusCode.Conflict);
                }

                var timeEntryBreak = TimeEntryBreak.Create(timeEntry.Id, breakInput.StartAt, breakInput.EndAt, breakInput.IsPaid);
                await _timeEntryBreakRepository.AddAsync(timeEntryBreak, cancellationToken);
                createdBreaks.Add(timeEntryBreak.ToDto());
                previousBreakEnd = breakInput.EndAt;
            }

            await TryFinalizeShiftStatusAsync(timeEntry, shift, cancellationToken);

            return Result<TimeEntryDto>.Success(timeEntry.ToDto(createdBreaks), (int)HttpStatusCode.Created);
        }

        private async Task TryFinalizeShiftStatusAsync(
            TimeEntry timeEntry,
            Shift shift,
            CancellationToken cancellationToken)
        {
            if (!timeEntry.ClockOutAt.HasValue)
            {
                return;
            }

            var requiredTemplateIds = await _shiftRequiredFormTemplateRepository
                .ListRequiredTemplateIdsByShiftIdAsync(shift.Id, cancellationToken);

            if (requiredTemplateIds.Count > 0)
            {
                var submittedTemplateIds = await _formSubmissionRepository
                    .ListSubmittedTemplateIdsByShiftAndMemberAsync(
                        shift.Id,
                        timeEntry.OrganizationMemberId,
                        cancellationToken);

                var missingRequiredTemplateIds = requiredTemplateIds.Except(submittedTemplateIds).ToList();
                if (missingRequiredTemplateIds.Count > 0)
                {
                    return;
                }
            }

            var breaks = await _timeEntryBreakRepository.ListByTimeEntryIdAsync(timeEntry.Id, cancellationToken);
            var hasBreak = breaks.Count > 0;
            var coversAllShiftHours = timeEntry.ClockInAt <= shift.StartAt && timeEntry.ClockOutAt.Value >= shift.EndAt;
            var nextStatus = coversAllShiftHours && hasBreak ? "completed" : "ongoing";

            shift.UpdateDetails(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                nextStatus);

            await _shiftRepository.UpdateAsync(shift, cancellationToken);
        }
    }
}
