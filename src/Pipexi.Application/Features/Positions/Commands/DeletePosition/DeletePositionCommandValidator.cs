using FluentValidation;

namespace Pipexi.Application.Features.Positions.Commands.DeletePosition;

public sealed class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
