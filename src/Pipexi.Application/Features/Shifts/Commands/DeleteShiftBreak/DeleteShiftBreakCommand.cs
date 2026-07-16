using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Shifts.Commands.DeleteShiftBreak;

public sealed record DeleteShiftBreakCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteShiftBreakCommand, Result<object?>>
    {
        private readonly IShiftBreakRepository _shiftBreakRepository;

        public Handler(IShiftBreakRepository shiftBreakRepository)
        {
            _shiftBreakRepository = shiftBreakRepository;
        }

        public async Task<Result<object?>> Handle(DeleteShiftBreakCommand request, CancellationToken cancellationToken)
        {
            var shiftBreak = await _shiftBreakRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shiftBreak is null)
            {
                return Result<object?>.Failure(
                    new AppError("shift_breaks.not_found", "Shift break not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _shiftBreakRepository.DeleteAsync(shiftBreak, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
