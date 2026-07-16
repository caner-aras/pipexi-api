using FluentValidation;

namespace Workforce.Application.Features.Permissions.Commands.CreatePermission;

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
    }
}
