using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Commands.CreateFormField;

public sealed record CreateFormFieldCommand(
    Guid FormTemplateId,
    string Type,
    string Label,
    bool IsRequired,
    int SortOrder,
    string? OptionsJson) : ICommand<Result<FormFieldDto>>
{
    public sealed class Handler : IRequestHandler<CreateFormFieldCommand, Result<FormFieldDto>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IFormFieldRepository _formFieldRepository;

        public Handler(IFormTemplateRepository formTemplateRepository, IFormFieldRepository formFieldRepository)
        {
            _formTemplateRepository = formTemplateRepository;
            _formFieldRepository = formFieldRepository;
        }

        public async Task<Result<FormFieldDto>> Handle(CreateFormFieldCommand request, CancellationToken cancellationToken)
        {
            var template = await _formTemplateRepository.GetByIdAsync(request.FormTemplateId, cancellationToken);
            if (template is null)
            {
                return Result<FormFieldDto>.Failure(
                    new AppError("form_fields.invalid_template", "Form template not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var existingFields = await _formFieldRepository.ListByFormTemplateIdAsync(request.FormTemplateId, cancellationToken);
            if (existingFields.Any(x => x.SortOrder == request.SortOrder))
            {
                return Result<FormFieldDto>.Failure(
                    new AppError("form_fields.duplicate_sort_order", "Sort order is already used in this form template."),
                    (int)HttpStatusCode.BadRequest);
            }

            var field = FormField.Create(
                request.FormTemplateId,
                request.Type,
                request.Label,
                request.IsRequired,
                request.SortOrder,
                request.OptionsJson);

            await _formFieldRepository.AddAsync(field, cancellationToken);

            return Result<FormFieldDto>.Success(field.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
