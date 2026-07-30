using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMemberProfiles.Commands.UpsertOrganizationMemberProfile;

public sealed class UpsertOrganizationMemberProfileCommandValidator
    : AbstractValidator<UpsertOrganizationMemberProfileCommand>
{
    private static readonly string[] AllowedGenders =
    [
        "male",
        "female",
        "other",
        "prefer_not_to_say"
    ];

    public UpsertOrganizationMemberProfileCommandValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();

        RuleFor(x => x.Gender)
            .Must(gender => gender is null || AllowedGenders.Contains(gender.Trim().ToLowerInvariant()))
            .WithMessage("Gender must be one of: male, female, other, prefer_not_to_say.");

        RuleFor(x => x.AddressLine1).MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(30);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.EmergencyContactName).MaximumLength(150);
        RuleFor(x => x.EmergencyContactPhone).MaximumLength(50);
        RuleFor(x => x.NationalId).MaximumLength(50);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth cannot be in the future.");
    }
}
