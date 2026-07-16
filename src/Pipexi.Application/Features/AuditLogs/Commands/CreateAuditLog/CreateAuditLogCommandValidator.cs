using FluentValidation;

namespace Workforce.Application.Features.AuditLogs.Commands.CreateAuditLog;

public sealed class CreateAuditLogCommandValidator : AbstractValidator<CreateAuditLogCommand>
{
    public CreateAuditLogCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.EntityId).NotEmpty();

        RuleFor(x => x.EntityName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(50);
    }
}
