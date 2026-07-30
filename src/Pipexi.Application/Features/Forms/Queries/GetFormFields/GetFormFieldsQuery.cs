using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormFields;

public sealed record GetFormFieldsQuery(Guid FormTemplateId, Guid? ScopedOrganizationId = null) : IQuery<Result<IReadOnlyCollection<FormFieldDto>>>
{
    public sealed class Handler : IRequestHandler<GetFormFieldsQuery, Result<IReadOnlyCollection<FormFieldDto>>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IFormTemplateRepository formTemplateRepository,
            IFormFieldRepository formFieldRepository,
            IOrganizationAccessService organizationAccess)
        {
            _formTemplateRepository = formTemplateRepository;
            _formFieldRepository = formFieldRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<IReadOnlyCollection<FormFieldDto>>> Handle(GetFormFieldsQuery request, CancellationToken cancellationToken)
        {
            var formTemplate = await _formTemplateRepository.GetByIdAsync(request.FormTemplateId, cancellationToken);
            if (formTemplate is null)
            {
                return Result<IReadOnlyCollection<FormFieldDto>>.Failure(
                    new AppError("form_templates.not_found", "Form template not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<IReadOnlyCollection<FormFieldDto>>(
                formTemplate.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            var fields = await _formFieldRepository.ListByFormTemplateIdAsync(request.FormTemplateId, cancellationToken);
            var dtos = fields.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<FormFieldDto>>.Success(dtos);
        }
    }
}
