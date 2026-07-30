using FluentValidation;

namespace Pipexi.Application.Features.OrganizationMemberProfiles.Queries.GetOrganizationMemberProfile;

public sealed class GetOrganizationMemberProfileQueryValidator
    : AbstractValidator<GetOrganizationMemberProfileQuery>
{
    public GetOrganizationMemberProfileQueryValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
    }
}
