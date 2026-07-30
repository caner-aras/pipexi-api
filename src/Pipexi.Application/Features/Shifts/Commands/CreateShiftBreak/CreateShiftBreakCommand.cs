using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Commands.CreateShiftBreak;

public sealed record CreateShiftBreakCommand(
    Guid ShiftId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsPaid, Guid? ScopedOrganizationId = null) : ICommand<Result<ShiftBreakDto>>
{
    public sealed class Handler : IRequestHandler<CreateShiftBreakCommand, Result<ShiftBreakDto>>
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

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<ShiftBreakDto>(
                shift.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

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
