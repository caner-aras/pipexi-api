using FluentValidation;

namespace Workforce.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public sealed class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.OrganizationMemberId).NotEmpty();

        RuleFor(x => x.LeaveType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate);
    }
}
