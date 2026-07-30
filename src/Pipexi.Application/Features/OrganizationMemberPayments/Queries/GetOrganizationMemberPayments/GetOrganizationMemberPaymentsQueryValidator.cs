using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Queries.GetOrganizationMemberPayments;

public sealed class GetOrganizationMemberPaymentsQueryValidator
    : AbstractValidator<GetOrganizationMemberPaymentsQuery>
{
    public GetOrganizationMemberPaymentsQueryValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
    }
}
