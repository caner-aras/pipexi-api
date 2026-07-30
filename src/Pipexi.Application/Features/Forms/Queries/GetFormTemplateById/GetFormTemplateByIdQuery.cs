using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormTemplateById;

public sealed record GetFormTemplateByIdQuery(Guid Id, Guid? ScopedOrganizationId = null) : IQuery<Result<FormTemplateDto>>
{
    public sealed class Handler : IRequestHandler<GetFormTemplateByIdQuery, Result<FormTemplateDto>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IFormTemplateRepository formTemplateRepository, IFormFieldRepository formFieldRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _formTemplateRepository = formTemplateRepository;
            _formFieldRepository = formFieldRepository;
        }

        public async Task<Result<FormTemplateDto>> Handle(GetFormTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var template = await _formTemplateRepository.GetByIdAsync(request.Id, cancellationToken);
            if (template is null)
            {
                return Result<FormTemplateDto>.Failure(
                    new AppError("form_templates.not_found", "Form template not found."),
                    (int)HttpStatusCode.NotFound);
            }


            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<FormTemplateDto>(
                template.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            var fields = await _formFieldRepository.ListByFormTemplateIdAsync(template.Id, cancellationToken);
            return Result<FormTemplateDto>.Success(template.ToDto(fields.Select(x => x.ToDto()).ToList()));
        }
    }
}
