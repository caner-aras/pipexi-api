using FluentValidation;

namespace Workforce.Application.Features.RolePermissions.Commands.CreateRolePermission;

public sealed class CreateRolePermissionCommandValidator : AbstractValidator<CreateRolePermissionCommand>
{
    public CreateRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}
