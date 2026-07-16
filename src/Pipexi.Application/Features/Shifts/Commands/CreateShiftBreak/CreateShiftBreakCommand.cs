using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Shifts.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Shifts.Commands.CreateShiftBreak;

public sealed record CreateShiftBreakCommand(
    Guid ShiftId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid) : ICommand<Result<ShiftBreakDto>>
{
    public sealed class Handler : IRequestHandler<CreateShiftBreakCommand, Result<ShiftBreakDto>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftBreakRepository _shiftBreakRepository;

        public Handler(IShiftRepository shiftRepository, IShiftBreakRepository shiftBreakRepository)
        {
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
        }

        public async Task<Result<ShiftBreakDto>> Handle(CreateShiftBreakCommand request, CancellationToken cancellationToken)
        {
            if (request.StartAt >= request.EndAt)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.invalid_range", "Break end time must be after start time."),
                    (int)HttpStatusCode.BadRequest);
            }

            var shift = await _shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken);
            if (shift is null)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.invalid_shift", "Shift not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (request.StartAt < shift.StartAt || request.EndAt > shift.EndAt)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.out_of_shift", "Break must be within shift time range."),
                    (int)HttpStatusCode.BadRequest);
            }

            var overlaps = await _shiftBreakRepository.OverlapsAsync(
                request.ShiftId,
                request.StartAt,
                request.EndAt,
                cancellationToken: cancellationToken);

            if (overlaps)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.overlap", "Break overlaps with another break."),
                    (int)HttpStatusCode.Conflict);
            }

            var shiftBreak = ShiftBreak.Create(request.ShiftId, request.StartAt, request.EndAt, request.IsPaid);
            await _shiftBreakRepository.AddAsync(shiftBreak, cancellationToken);

            return Result<ShiftBreakDto>.Success(shiftBreak.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
