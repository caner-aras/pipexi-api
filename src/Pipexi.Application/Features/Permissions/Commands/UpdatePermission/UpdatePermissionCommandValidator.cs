using FluentValidation;

namespace Workforce.Application.Features.Permissions.Commands.UpdatePermission;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Key).NotEmpty().MaximumLength(100).When(x => x.Key is not null);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50).When(x => x.Status is not null);
    }
}
