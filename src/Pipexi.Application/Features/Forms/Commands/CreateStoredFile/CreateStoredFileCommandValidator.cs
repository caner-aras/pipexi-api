using FluentValidation;

namespace Pipexi.Application.Features.Forms.Commands.CreateStoredFile;

public sealed class CreateStoredFileCommandValidator : AbstractValidator<CreateStoredFileCommand>
{
    public CreateStoredFileCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.StoragePath)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.SizeBytes)
            .GreaterThanOrEqualTo(0);
    }
}
