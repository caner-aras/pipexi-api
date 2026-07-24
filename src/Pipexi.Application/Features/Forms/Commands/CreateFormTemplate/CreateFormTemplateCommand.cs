using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.CreateFormTemplate;

public sealed record CreateFormTemplateCommand(
    Guid OrganizationId,
    string Name,
    string? Description) : ICommand<Result<FormTemplateDto>>
{
    public sealed class Handler : IRequestHandler<CreateFormTemplateCommand, Result<FormTemplateDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;

        public Handler(
            IOrganizationRepository organizationRepository,
            IFormTemplateRepository formTemplateRepository)
        {
            _organizationRepository = organizationRepository;
            _formTemplateRepository = formTemplateRepository;
        }

        public async Task<Result<FormTemplateDto>> Handle(CreateFormTemplateCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<FormTemplateDto>.Failure(
                    new AppError("form_templates.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var template = FormTemplate.Create(request.OrganizationId, request.Name, request.Description);
            await _formTemplateRepository.AddAsync(template, cancellationToken);

            return Result<FormTemplateDto>.Success(template.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
