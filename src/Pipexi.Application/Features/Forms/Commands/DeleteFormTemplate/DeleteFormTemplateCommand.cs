using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.DeleteFormTemplate;

public sealed record DeleteFormTemplateCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteFormTemplateCommand, Result<object?>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IFormTemplateRepository formTemplateRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _formTemplateRepository = formTemplateRepository;
        }

        public async Task<Result<object?>> Handle(DeleteFormTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _formTemplateRepository.GetByIdAsync(request.Id, cancellationToken);
            if (template is null)
            {
                return Result<object?>.Failure(
                    new AppError("form_templates.not_found", "Form template not found."),
                    (int)HttpStatusCode.NotFound);
            }


            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                template.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _formTemplateRepository.DeleteAsync(template, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
