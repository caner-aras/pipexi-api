using FluentValidation;

namespace Workforce.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .Must(x => x is null || x is "low" or "medium" or "high" or "urgent")
            .WithMessage("Priority must be one of: low, medium, high, urgent.");
    }
}
