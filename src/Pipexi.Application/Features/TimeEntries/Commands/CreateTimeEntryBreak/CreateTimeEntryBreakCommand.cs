using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.TimeEntries.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.TimeEntries.Commands.CreateTimeEntryBreak;

public sealed record CreateTimeEntryBreakCommand(
    Guid TimeEntryId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid) : ICommand<Result<TimeEntryBreakDto>>
{
    public sealed class Handler : IRequestHandler<CreateTimeEntryBreakCommand, Result<TimeEntryBreakDto>>
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
        private readonly IFormSubmissionRepository _formSubmissionRepository;

        public Handler(
            ITimeEntryRepository timeEntryRepository,
            ITimeEntryBreakRepository timeEntryBreakRepository,
            IShiftRepository shiftRepository,
            IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
            IFormSubmissionRepository formSubmissionRepository)
        {
            _timeEntryRepository = timeEntryRepository;
            _timeEntryBreakRepository = timeEntryBreakRepository;
            _shiftRepository = shiftRepository;
            _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
            _formSubmissionRepository = formSubmissionRepository;
        }

        public async Task<Result<TimeEntryBreakDto>> Handle(CreateTimeEntryBreakCommand request, CancellationToken cancellationToken)
        {
            if (request.StartAt >= request.EndAt)
            {
                return Result<TimeEntryBreakDto>.Failure(
                    new AppError("time_entry_breaks.invalid_range", "Break end time must be after start time."),
                    (int)HttpStatusCode.BadRequest);
            }

            var timeEntry = await _timeEntryRepository.GetByIdAsync(request.TimeEntryId, cancellationToken);
            if (timeEntry is null)
            {
                return Result<TimeEntryBreakDto>.Failure(
                    new AppError("time_entry_breaks.invalid_time_entry", "Time entry not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (request.StartAt < timeEntry.ClockInAt || (timeEntry.ClockOutAt.HasValue && request.EndAt > timeEntry.ClockOutAt.Value))
            {
                return Result<TimeEntryBreakDto>.Failure(
                    new AppError("time_entry_breaks.out_of_entry", "Break must be within time entry range."),
                    (int)HttpStatusCode.BadRequest);
            }

            var overlaps = await _timeEntryBreakRepository.OverlapsAsync(
                request.TimeEntryId,
                request.StartAt,
                request.EndAt,
                cancellationToken: cancellationToken);

            if (overlaps)
            {
                return Result<TimeEntryBreakDto>.Failure(
                    new AppError("time_entry_breaks.overlap", "Break overlaps with another break."),
                    (int)HttpStatusCode.Conflict);
            }

            var timeEntryBreak = TimeEntryBreak.Create(request.TimeEntryId, request.StartAt, request.EndAt, request.IsPaid);
            await _timeEntryBreakRepository.AddAsync(timeEntryBreak, cancellationToken);

            await TryFinalizeShiftStatusAsync(timeEntry, cancellationToken);

            return Result<TimeEntryBreakDto>.Success(timeEntryBreak.ToDto(), (int)HttpStatusCode.Created);
        }

        private async Task TryFinalizeShiftStatusAsync(TimeEntry timeEntry, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepository.GetByIdAsync(timeEntry.ShiftId, cancellationToken);
            if (shift is null)
            {
                return;
            }

            if (timeEntry.ClockOutAt.HasValue)
            {
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
                        shift.UpdateDetails(null, null, null, null, null, null, null, "ongoing");
                        await _shiftRepository.UpdateAsync(shift, cancellationToken);
                        return;
                    }
                }

                var breaks = await _timeEntryBreakRepository.ListByTimeEntryIdAsync(timeEntry.Id, cancellationToken);
                var hasBreak = breaks.Count > 0;
                var coversAllShiftHours = timeEntry.ClockInAt <= shift.StartAt && timeEntry.ClockOutAt.Value >= shift.EndAt;
                var nextStatus = coversAllShiftHours && hasBreak ? "completed" : "ongoing";

                shift.UpdateDetails(null, null, null, null, null, null, null, nextStatus);
                await _shiftRepository.UpdateAsync(shift, cancellationToken);
                return;
            }

            shift.UpdateDetails(null, null, null, null, null, null, null, "ongoing");
            await _shiftRepository.UpdateAsync(shift, cancellationToken);
        }
    }
}
