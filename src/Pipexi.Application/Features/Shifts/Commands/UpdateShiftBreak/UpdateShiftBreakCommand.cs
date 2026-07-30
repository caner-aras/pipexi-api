using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Commands.UpdateShiftBreak;

public sealed record UpdateShiftBreakCommand(
    Guid Id,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    bool? IsPaid,
    string? Status, Guid? ScopedOrganizationId = null) : ICommand<Result<ShiftBreakDto>>
{
    public sealed class Handler : IRequestHandler<UpdateShiftBreakCommand, Result<ShiftBreakDto>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IShiftBreakRepository _shiftBreakRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IShiftRepository shiftRepository, IShiftBreakRepository shiftBreakRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _shiftRepository = shiftRepository;
            _shiftBreakRepository = shiftBreakRepository;
        }

        public async Task<Result<ShiftBreakDto>> Handle(UpdateShiftBreakCommand request, CancellationToken cancellationToken)
        {
            var shiftBreak = await _shiftBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shiftBreak is null)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.not_found", "Shift break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var shift = await _shiftRepository.GetByIdAsync(shiftBreak.ShiftId, cancellationToken);
            if (shift is null)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.invalid_shift", "Shift not found."),
                    (int)HttpStatusCode.BadRequest);
            }


            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<ShiftBreakDto>(
                shift.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            var candidateStart = request.StartAt ?? shiftBreak.StartAt;
            var candidateEnd = request.EndAt ?? shiftBreak.EndAt;

            if (candidateStart >= candidateEnd)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.invalid_range", "Break end time must be after start time."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (candidateStart < shift.StartAt || candidateEnd > shift.EndAt)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.out_of_shift", "Break must be within shift time range."),
                    (int)HttpStatusCode.BadRequest);
            }

            var overlaps = await _shiftBreakRepository.OverlapsAsync(
                shiftBreak.ShiftId,
                candidateStart,
                candidateEnd,
                shiftBreak.Id,
                cancellationToken);

            if (overlaps)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.overlap", "Break overlaps with another break."),
                    (int)HttpStatusCode.Conflict);
            }

            shiftBreak.UpdateDetails(request.StartAt, request.EndAt, request.IsPaid, request.Status);
            await _shiftBreakRepository.UpdateAsync(shiftBreak, cancellationToken);

            return Result<ShiftBreakDto>.Success(shiftBreak.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
