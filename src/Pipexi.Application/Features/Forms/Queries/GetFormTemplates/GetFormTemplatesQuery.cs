using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormTemplates;

public sealed record GetFormTemplatesQuery(Guid? OrganizationId) : IQuery<Result<IReadOnlyCollection<FormTemplateDto>>>
{
    public sealed class Handler : IRequestHandler<GetFormTemplatesQuery, Result<IReadOnlyCollection<FormTemplateDto>>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;

        public Handler(IFormTemplateRepository formTemplateRepository)
        {
            _formTemplateRepository = formTemplateRepository;
        }

        public async Task<Result<IReadOnlyCollection<FormTemplateDto>>> Handle(GetFormTemplatesQuery request, CancellationToken cancellationToken)
        {
            var templates = request.OrganizationId.HasValue
                ? await _formTemplateRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _formTemplateRepository.GetAllAsync(cancellationToken);

            var dtos = templates
                .Select(x => x.ToDto())
                .ToList();

            return Result<IReadOnlyCollection<FormTemplateDto>>.Success(dtos);
        }
    }
}
