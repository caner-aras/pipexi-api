using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMemberPayments.Queries.GetOrganizationMemberPayments;

public sealed class GetOrganizationMemberPaymentsQueryValidator
    : AbstractValidator<GetOrganizationMemberPaymentsQuery>
{
    public GetOrganizationMemberPaymentsQueryValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("fromDate must be on or before toDate.");
    }
}
