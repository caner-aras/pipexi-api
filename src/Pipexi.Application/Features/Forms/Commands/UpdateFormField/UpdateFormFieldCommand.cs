using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.UpdateFormField;

public sealed record UpdateFormFieldCommand(
    Guid Id,
    string? Type,
    string? Label,
    bool? IsRequired,
    int? SortOrder,
    string? OptionsJson,
    string? Status) : ICommand<Result<FormFieldDto>>
{
    public sealed class Handler : IRequestHandler<UpdateFormFieldCommand, Result<FormFieldDto>>
    {
        private readonly IFormFieldRepository _formFieldRepository;

        public Handler(IFormFieldRepository formFieldRepository)
        {
            _formFieldRepository = formFieldRepository;
        }

        public async Task<Result<FormFieldDto>> Handle(UpdateFormFieldCommand request, CancellationToken cancellationToken)
        {
            var field = await _formFieldRepository.GetByIdAsync(request.Id, cancellationToken);
            if (field is null)
            {
                return Result<FormFieldDto>.Failure(
                    new AppError("form_fields.not_found", "Form field not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var candidateSortOrder = request.SortOrder ?? field.SortOrder;
            var siblingFields = await _formFieldRepository.ListByFormTemplateIdAsync(field.FormTemplateId, cancellationToken);
            if (siblingFields.Any(x => x.Id != field.Id && x.SortOrder == candidateSortOrder))
            {
                return Result<FormFieldDto>.Failure(
                    new AppError("form_fields.duplicate_sort_order", "Sort order is already used in this form template."),
                    (int)HttpStatusCode.BadRequest);
            }

            field.UpdateDetails(
                request.Type,
                request.Label,
                request.IsRequired,
                request.SortOrder,
                request.OptionsJson,
                request.Status);

            await _formFieldRepository.UpdateAsync(field, cancellationToken);
            return Result<FormFieldDto>.Success(field.ToDto(), (int)HttpStatusCode.OK);
        }
    }
}
