using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Queries.GetShiftBreakById;

public sealed record GetShiftBreakByIdQuery(Guid Id) : IQuery<Result<ShiftBreakDto>>
{
    public sealed class Handler : IRequestHandler<GetShiftBreakByIdQuery, Result<ShiftBreakDto>>
    {
        private readonly IShiftBreakRepository _shiftBreakRepository;

        public Handler(IShiftBreakRepository shiftBreakRepository)
        {
            _shiftBreakRepository = shiftBreakRepository;
        }

        public async Task<Result<ShiftBreakDto>> Handle(GetShiftBreakByIdQuery request, CancellationToken cancellationToken)
        {
            var shiftBreak = await _shiftBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shiftBreak is null)
            {
                return Result<ShiftBreakDto>.Failure(
                    new AppError("shift_breaks.not_found", "Shift break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<ShiftBreakDto>.Success(shiftBreak.ToDto());
        }
    }
}
