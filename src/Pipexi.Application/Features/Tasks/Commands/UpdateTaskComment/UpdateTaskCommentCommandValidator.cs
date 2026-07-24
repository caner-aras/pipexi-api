using FluentValidation;

namespace Pipexi.Application.Features.Tasks.Commands.UpdateTaskComment;

public sealed class UpdateTaskCommentCommandValidator : AbstractValidator<UpdateTaskCommentCommand>
{
    public UpdateTaskCommentCommandValidator()
    {
        RuleFor(x => x.Message)
            .MaximumLength(4000)
            .When(x => x.Message is not null);
    }
}
