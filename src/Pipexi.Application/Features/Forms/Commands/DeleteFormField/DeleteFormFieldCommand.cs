using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.DeleteFormField;

public sealed record DeleteFormFieldCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteFormFieldCommand, Result<object?>>
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

        public async Task<Result<object?>> Handle(DeleteFormFieldCommand request, CancellationToken cancellationToken)
        {
            var field = await _formFieldRepository.GetByIdAsync(request.Id, cancellationToken);
            if (field is null)
            {
                return Result<object?>.Failure(
                    new AppError("form_fields.not_found", "Form field not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var formTemplate = await _formTemplateRepository.GetByIdAsync(field.FormTemplateId, cancellationToken);
            if (formTemplate is null)
            {
                return Result<object?>.Failure(
                    new AppError("form_fields.invalid_template", "Form template not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                formTemplate.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _formFieldRepository.DeleteAsync(field, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
