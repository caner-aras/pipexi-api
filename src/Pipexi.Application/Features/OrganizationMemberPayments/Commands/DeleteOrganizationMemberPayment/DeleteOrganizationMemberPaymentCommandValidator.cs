using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Commands.DeleteOrganizationMemberPayment;

public sealed class DeleteOrganizationMemberPaymentCommandValidator
    : AbstractValidator<DeleteOrganizationMemberPaymentCommand>
{
    public DeleteOrganizationMemberPaymentCommandValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
