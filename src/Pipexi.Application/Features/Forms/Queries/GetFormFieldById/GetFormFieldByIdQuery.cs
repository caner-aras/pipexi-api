using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormFieldById;

public sealed record GetFormFieldByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<FormFieldDto>>
{
    public sealed class Handler : IRequestHandler<GetFormFieldByIdQuery, Result<FormFieldDto>>
    {
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IFormFieldRepository formFieldRepository,
            IFormTemplateRepository formTemplateRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _formTemplateRepository = formTemplateRepository;
            _formFieldRepository = formFieldRepository;
        }

        public async Task<Result<FormFieldDto>> Handle(GetFormFieldByIdQuery request, CancellationToken cancellationToken)
        {
            var field = await _formFieldRepository.GetByIdAsync(request.Id, cancellationToken);
            if (field is null)
            {
                return Result<FormFieldDto>.Failure(
                    new AppError("form_fields.not_found", "Form field not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var formTemplate = await _formTemplateRepository.GetByIdAsync(field.FormTemplateId, cancellationToken);
            if (formTemplate is null)
            {
                return Result<FormFieldDto>.Failure(
                    new AppError("form_fields.invalid_template", "Form template not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<FormFieldDto>(
                formTemplate.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            return Result<FormFieldDto>.Success(field.ToDto());
        }
    }
}
