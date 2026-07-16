using FluentValidation;

namespace Workforce.Application.Features.Tasks.Commands.CreateTaskComment;

public sealed class CreateTaskCommentCommandValidator : AbstractValidator<CreateTaskCommentCommand>
{
    public CreateTaskCommentCommandValidator()
    {
        RuleFor(x => x.WorkTaskId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}
