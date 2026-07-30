using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormTemplates;

public sealed record GetFormTemplatesQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<FormTemplateDto>>>
{
    public sealed class Handler : IRequestHandler<GetFormTemplatesQuery, Result<IReadOnlyCollection<FormTemplateDto>>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(IFormTemplateRepository formTemplateRepository, ICurrentUserContext currentUserContext)
        {
            _formTemplateRepository = formTemplateRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<FormTemplateDto>>> Handle(GetFormTemplatesQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<FormTemplateDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var templates = await _formTemplateRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);

            var dtos = templates
                .Select(x => x.ToDto())
                .ToList();

            return Result<IReadOnlyCollection<FormTemplateDto>>.Success(dtos);
        }
    }
}
