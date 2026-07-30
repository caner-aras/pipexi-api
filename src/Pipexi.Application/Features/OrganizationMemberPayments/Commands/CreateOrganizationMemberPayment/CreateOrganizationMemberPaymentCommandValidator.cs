using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Commands.CreateOrganizationMemberPayment;

public sealed class CreateOrganizationMemberPaymentCommandValidator
    : AbstractValidator<CreateOrganizationMemberPaymentCommand>
{
    private static readonly string[] AllowedMethods =
    [
        "cash",
        "bank_transfer",
        "check",
        "card",
        "other"
    ];

    public CreateOrganizationMemberPaymentCommandValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaidAt).NotEmpty();

        RuleFor(x => x.Method)
            .NotEmpty()
            .Must(method => AllowedMethods.Contains(method.Trim().ToLowerInvariant()))
            .WithMessage("Method must be one of: cash, bank_transfer, check, card, other.");

        RuleFor(x => x.Currency)
            .Length(3)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.Reference).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);

        RuleFor(x => x.PeriodEnd)
            .GreaterThanOrEqualTo(x => x.PeriodStart!.Value)
            .When(x => x.PeriodStart.HasValue && x.PeriodEnd.HasValue);
    }
}
