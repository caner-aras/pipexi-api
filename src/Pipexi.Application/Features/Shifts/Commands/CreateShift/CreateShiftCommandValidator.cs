using FluentValidation;

namespace Workforce.Application.Features.Shifts.Commands.CreateShift;

public sealed class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .WithMessage("EndAt must be greater than StartAt.");

        RuleForEach(x => x.Breaks).ChildRules(breakRules =>
        {
            breakRules.RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage("Break EndAt must be greater than StartAt.");
        });

        RuleFor(x => x.Repeat)
            .Must(x => string.IsNullOrWhiteSpace(x) || IsAllowedRepeatType(x))
            .WithMessage("Repeat must be daily, weekly or monthly.");

        RuleFor(x => x.RepeatTimes)
            .GreaterThan(0)
            .When(x => !string.IsNullOrWhiteSpace(x.Repeat))
            .WithMessage("RepeatTimes must be greater than 0 when Repeat is provided.");

        RuleFor(x => x.RepeatOn)
            .NotNull()
            .Must(x => x is { Count: > 0 })
            .When(x => IsWeeklyRepeat(x.Repeat))
            .WithMessage("RepeatOn must be provided for weekly repeat.");

        RuleForEach(x => x.RepeatOn)
            .InclusiveBetween(0, 6)
            .When(x => x.RepeatOn is not null)
            .WithMessage("RepeatOn values must be between 0 and 6.");

        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(1, 31)
            .When(x => IsMonthlyRepeat(x.Repeat))
            .WithMessage("DayOfMonth must be between 1 and 31 for monthly repeat.");
    }

    private static bool IsAllowedRepeatType(string repeat)
    {
        var normalized = repeat.Trim().ToLowerInvariant();
        if (normalized == "montly")
        {
            normalized = "monthly";
        }

        return normalized is "daily" or "weekly" or "monthly";
    }

    private static bool IsWeeklyRepeat(string? repeat)
    {
        return string.Equals(repeat?.Trim(), "weekly", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMonthlyRepeat(string? repeat)
    {
        var normalized = repeat?.Trim().ToLowerInvariant();
        return normalized is "monthly" or "montly";
    }
}
