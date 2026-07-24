using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Commands.DeleteShift;

public sealed record DeleteShiftCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteShiftCommand, Result<object?>>
    {
        private readonly IShiftRepository _shiftRepository;

        public Handler(IShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<Result<object?>> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shift is null)
            {
                return Result<object?>.Failure(
                    new AppError("shifts.not_found", "Shift not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _shiftRepository.DeleteAsync(shift, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
