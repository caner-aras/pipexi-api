using FluentValidation;

namespace Pipexi.Application.Features.Forms.Commands.UpdateStoredFile;

public sealed class UpdateStoredFileCommandValidator : AbstractValidator<UpdateStoredFileCommand>
{
    public UpdateStoredFileCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.FileName)
            .MaximumLength(255)
            .When(x => x.FileName is not null);

        RuleFor(x => x.ContentType)
            .MaximumLength(120)
            .When(x => x.ContentType is not null);

        RuleFor(x => x.StoragePath)
            .MaximumLength(1000)
            .When(x => x.StoragePath is not null);

        RuleFor(x => x.SizeBytes)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SizeBytes.HasValue);

        RuleFor(x => x.Status)
            .MaximumLength(30)
            .When(x => x.Status is not null);
    }
}
