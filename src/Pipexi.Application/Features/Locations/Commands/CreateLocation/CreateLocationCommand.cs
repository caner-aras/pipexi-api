using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Locations.Commands.CreateLocation;

public sealed record CreateLocationCommand(
    Guid OrganizationId,
    string Name,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    int GeofenceRadiusMeters,
    string? Timezone) : ICommand<Result<LocationDto>>
{
    public sealed class Handler : IRequestHandler<CreateLocationCommand, Result<LocationDto>>
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationWorkingHourRepository _locationWorkingHourRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public Handler(
            ILocationRepository locationRepository,
            ILocationWorkingHourRepository locationWorkingHourRepository,
            IOrganizationRepository organizationRepository)
        {
            _locationRepository = locationRepository;
            _locationWorkingHourRepository = locationWorkingHourRepository;
            _organizationRepository = organizationRepository;
        }

        public async Task<Result<LocationDto>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<LocationDto>.Failure(
                    new AppError("locations.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var exists = await _locationRepository.NameExistsAsync(
                request.OrganizationId,
                request.Name,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result<LocationDto>.Failure(
                    new AppError("locations.name_conflict", "Location name already exists in this organization."),
                    (int)HttpStatusCode.Conflict);
            }

            var location = Location.Create(
                request.OrganizationId,
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.GeofenceRadiusMeters,
                request.Timezone);

            await _locationRepository.AddAsync(location, cancellationToken);

            var workingHours = await _locationWorkingHourRepository.ListByLocationIdAsync(location.Id, cancellationToken);

            return Result<LocationDto>.Success(
                location.ToDto(workingHours.Select(x => x.ToDto()).ToList()),
                (int)HttpStatusCode.Created);
        }
    }
}
