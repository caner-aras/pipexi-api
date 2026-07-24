using FluentValidation;

namespace Pipexi.Application.Features.Locations.Commands.UpdateLocation;

public sealed class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150)
            .When(x => x.Name is not null);

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => x.Address is not null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.GeofenceRadiusMeters)
            .GreaterThan(0)
            .LessThanOrEqualTo(10000)
            .When(x => x.GeofenceRadiusMeters.HasValue);

        RuleFor(x => x.Timezone)
            .MaximumLength(100)
            .When(x => x.Timezone is not null);

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.Status is not null);
    }
}
