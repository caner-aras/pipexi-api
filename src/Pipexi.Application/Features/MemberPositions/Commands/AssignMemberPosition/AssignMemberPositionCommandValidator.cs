using FluentValidation;

namespace Pipexi.Application.Features.MemberPositions.Commands.AssignMemberPosition;

public sealed class AssignMemberPositionCommandValidator : AbstractValidator<AssignMemberPositionCommand>
{
    public AssignMemberPositionCommandValidator()
    {
        RuleFor(x => x.OrganizationMemberId).NotEmpty();
        RuleFor(x => x.PositionId).NotEmpty();
        RuleFor(x => x.HourlyRate).GreaterThanOrEqualTo(0);
    }
}
