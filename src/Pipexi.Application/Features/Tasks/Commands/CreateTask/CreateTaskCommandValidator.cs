using FluentValidation;

namespace Pipexi.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .Must(x => x is null || x is "low" or "medium" or "high" or "urgent")
            .WithMessage("Priority must be one of: low, medium, high, urgent.");
    }
}
