using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Locations.Commands.UpdateLocation;

public sealed record UpdateLocationCommand(
    Guid Id,
    string? Name,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    int? GeofenceRadiusMeters,
    string? Timezone,
    string? Status) : ICommand<Result<LocationDto>>
{
    public sealed class Handler : IRequestHandler<UpdateLocationCommand, Result<LocationDto>>
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationWorkingHourRepository _locationWorkingHourRepository;

        public Handler(
            ILocationRepository locationRepository,
            ILocationWorkingHourRepository locationWorkingHourRepository)
        {
            _locationRepository = locationRepository;
            _locationWorkingHourRepository = locationWorkingHourRepository;
        }

        public async Task<Result<LocationDto>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
        {
            var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (location is null)
            {
                return Result<LocationDto>.Failure(
                    new AppError("locations.not_found", "Location not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var candidateName = request.Name ?? location.Name;
            var exists = await _locationRepository.NameExistsAsync(
                location.OrganizationId,
                candidateName,
                location.Id,
                cancellationToken);

            if (exists)
            {
                return Result<LocationDto>.Failure(
                    new AppError("locations.name_conflict", "Location name already exists in this organization."),
                    (int)HttpStatusCode.Conflict);
            }

            location.UpdateDetails(
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.GeofenceRadiusMeters,
                request.Timezone,
                request.Status);

            await _locationRepository.UpdateAsync(location, cancellationToken);
            var workingHours = await _locationWorkingHourRepository.ListByLocationIdAsync(location.Id, cancellationToken);
            return Result<LocationDto>.Success(
                location.ToDto(workingHours.Select(x => x.ToDto()).ToList()),
                (int)HttpStatusCode.OK);
        }
    }
}
