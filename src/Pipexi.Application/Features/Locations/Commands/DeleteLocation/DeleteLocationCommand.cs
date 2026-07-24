using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Locations.Commands.DeleteLocation;

public sealed record DeleteLocationCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteLocationCommand, Result<object?>>
    {
        private readonly ILocationRepository _locationRepository;

        public Handler(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<Result<object?>> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
        {
            var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (location is null)
            {
                return Result<object?>.Failure(
                    new AppError("locations.not_found", "Location not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _locationRepository.DeleteAsync(location, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
