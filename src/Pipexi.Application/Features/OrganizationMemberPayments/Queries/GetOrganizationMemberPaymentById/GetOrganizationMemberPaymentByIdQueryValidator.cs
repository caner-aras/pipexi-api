using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Queries.GetOrganizationMemberPaymentById;

public sealed class GetOrganizationMemberPaymentByIdQueryValidator
    : AbstractValidator<GetOrganizationMemberPaymentByIdQuery>
{
    public GetOrganizationMemberPaymentByIdQueryValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
