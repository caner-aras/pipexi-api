using FluentValidation;

namespace Workforce.Application.Features.RolePermissions.Commands.UpdateRolePermission;

public sealed class UpdateRolePermissionCommandValidator : AbstractValidator<UpdateRolePermissionCommand>
{
    public UpdateRolePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.Status is not null);
    }
}
