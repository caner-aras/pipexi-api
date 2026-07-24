using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Commands.UpdateTimeEntry;

public sealed record UpdateTimeEntryCommand(
    Guid Id,
    Guid? ShiftId,
    Guid? OrganizationMemberId,
    Guid? LocationId,
    DateTimeOffset? ClockInAt,
    DateTimeOffset? ClockOutAt,
    string? EmployeeNote,
    string? ManagerNote,
    string? Status) : ICommand<Result<TimeEntryDto>>
{
    public sealed class Handler : IRequestHandler<UpdateTimeEntryCommand, Result<TimeEntryDto>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
        private readonly IFormSubmissionRepository _formSubmissionRepository;

        public Handler(
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            IShiftRepository shiftRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ILocationRepository locationRepository,
            IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
            IFormSubmissionRepository formSubmissionRepository)
        {
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _shiftRepository = shiftRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _locationRepository = locationRepository;
            _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
            _formSubmissionRepository = formSubmissionRepository;
        }

        public async Task<Result<TimeEntryDto>> Handle(UpdateTimeEntryCommand request, CancellationToken cancellationToken)
        {
            var timeEntry = await _timeEntryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (timeEntry is null)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.not_found", "Time entry not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var candidateClockInAt = request.ClockInAt ?? timeEntry.ClockInAt;
            var candidateClockOutAt = request.ClockOutAt ?? timeEntry.ClockOutAt;
            if (candidateClockOutAt.HasValue && candidateClockOutAt.Value <= candidateClockInAt)
            {
                return Result<TimeEntryDto>.Failure(
                    new AppError("time_entries.invalid_range", "ClockOutAt must be after ClockInAt."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (request.ShiftId.HasValue)
            {
                var shift = await _shiftRepository.GetByIdAsync(request.ShiftId.Value, cancellationToken);
                if (shift is null || shift.OrganizationId != timeEntry.OrganizationId)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entries.invalid_shift", "Shift not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.OrganizationMemberId.HasValue)
            {
                var member = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId.Value, cancellationToken);
                if (member is null || member.OrganizationId != timeEntry.OrganizationId)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entries.invalid_organization_member", "Organization member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.LocationId.HasValue)
            {
                var location = await _locationRepository.GetByIdAsync(request.LocationId.Value, cancellationToken);
                if (location is null || location.OrganizationId != timeEntry.OrganizationId)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entries.invalid_location", "Location not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (candidateClockOutAt.HasValue)
            {
                var breaks = await _timeEntryBreakRepository.ListByTimeEntryIdAsync(timeEntry.Id, cancellationToken);
                var hasOutOfRangeBreak = breaks.Any(x => x.EndAt > candidateClockOutAt.Value || x.StartAt < candidateClockInAt);
                if (hasOutOfRangeBreak)
                {
                    return Result<TimeEntryDto>.Failure(
                        new AppError("time_entry_breaks.out_of_entry", "Existing breaks are out of the updated time entry range."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            timeEntry.UpdateDetails(
                request.ShiftId,
                request.OrganizationMemberId,
                request.LocationId,
                request.ClockInAt,
                request.ClockOutAt,
                request.EmployeeNote,
                request.ManagerNote,
                request.Status);

            await _timeEntryRepository.UpdateAsync(timeEntry, cancellationToken);

            await TryFinalizeShiftStatusAsync(timeEntry, cancellationToken);

            var existingBreaks = await _timeEntryBreakRepository.ListByTimeEntryIdAsync(timeEntry.Id, cancellationToken);
            return Result<TimeEntryDto>.Success(timeEntry.ToDto(existingBreaks.Select(x => x.ToDto()).ToList()), (int)HttpStatusCode.OK);
        }

        private async Task TryFinalizeShiftStatusAsync(TimeEntry timeEntry, CancellationToken cancellationToken)
        {
            if (!timeEntry.ClockOutAt.HasValue)
            {
                return;
            }

            var shift = await _shiftRepository.GetByIdAsync(timeEntry.ShiftId, cancellationToken);
            if (shift is null)
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
