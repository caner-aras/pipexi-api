using FluentValidation;

namespace Workforce.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;

public sealed class UpdateLeaveRequestCommandValidator : AbstractValidator<UpdateLeaveRequestCommand>
{
    public UpdateLeaveRequestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.LeaveType)
            .MaximumLength(50)
            .When(x => x.LeaveType is not null);

        RuleFor(x => x.Reason)
            .MaximumLength(2000)
            .When(x => x.Reason is not null);

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.EndDate.Value >= x.StartDate.Value)
            .WithMessage("EndDate must be greater than or equal to StartDate when both dates are provided.");
    }
}
