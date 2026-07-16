using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Locations.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Locations.Commands.SetLocationWorkingHours;

public sealed record SetLocationWorkingHourInput(
    int DayOfWeek,
    bool IsClosed,
    TimeOnly? OpensAt,
    TimeOnly? ClosesAt);

public sealed record SetLocationWorkingHoursCommand(
    Guid OrganizationId,
    Guid LocationId,
    IReadOnlyCollection<SetLocationWorkingHourInput> WorkingHours)
    : ICommand<Result<IReadOnlyCollection<LocationWorkingHourDto>>>;

public sealed class Handler : IRequestHandler<SetLocationWorkingHoursCommand, Result<IReadOnlyCollection<LocationWorkingHourDto>>>
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

    public async Task<Result<IReadOnlyCollection<LocationWorkingHourDto>>> Handle(
        SetLocationWorkingHoursCommand request,
        CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
        if (location is null || location.OrganizationId != request.OrganizationId)
        {
            return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Failure(
                new AppError("locations.not_found", "Location not found for organization."),
                (int)HttpStatusCode.NotFound);
        }

        var duplicatedDays = request.WorkingHours
            .GroupBy(x => x.DayOfWeek)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatedDays.Count > 0)
        {
            return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Failure(
                new AppError("location_working_hours.duplicate_day", "Each day can be provided at most once."),
                (int)HttpStatusCode.BadRequest);
        }

        foreach (var input in request.WorkingHours)
        {
            if (input.DayOfWeek < 0 || input.DayOfWeek > 6)
            {
                return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Failure(
                    new AppError("location_working_hours.invalid_day", "DayOfWeek must be between 0 and 6."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (input.IsClosed)
            {
                continue;
            }

            if (!input.OpensAt.HasValue || !input.ClosesAt.HasValue)
            {
                return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Failure(
                    new AppError("location_working_hours.invalid_time", "Open and close times are required when day is not closed."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (input.OpensAt.Value >= input.ClosesAt.Value)
            {
                return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Failure(
                    new AppError("location_working_hours.invalid_range", "Close time must be after open time."),
                    (int)HttpStatusCode.BadRequest);
            }
        }

        await _locationWorkingHourRepository.HardDeleteByLocationIdAsync(request.LocationId, cancellationToken);

        var newItems = request.WorkingHours
            .Select(x => LocationWorkingHour.Create(request.LocationId, x.DayOfWeek, x.IsClosed, x.OpensAt, x.ClosesAt))
            .ToList();

        if (newItems.Count > 0)
        {
            await _locationWorkingHourRepository.AddRangeAsync(newItems, cancellationToken);
        }

        var saved = await _locationWorkingHourRepository.ListByLocationIdAsync(request.LocationId, cancellationToken);
        return Result<IReadOnlyCollection<LocationWorkingHourDto>>.Success(saved.Select(x => x.ToDto()).ToList(), (int)HttpStatusCode.OK);
    }
}
