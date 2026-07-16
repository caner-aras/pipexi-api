using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Queries.GetFormFields;

public sealed record GetFormFieldsQuery(Guid FormTemplateId) : IQuery<Result<IReadOnlyCollection<FormFieldDto>>>
{
    public sealed class Handler : IRequestHandler<GetFormFieldsQuery, Result<IReadOnlyCollection<FormFieldDto>>>
    {
        private readonly IFormFieldRepository _formFieldRepository;

        public Handler(IFormFieldRepository formFieldRepository)
        {
            _formFieldRepository = formFieldRepository;
        }

        public async Task<Result<IReadOnlyCollection<FormFieldDto>>> Handle(GetFormFieldsQuery request, CancellationToken cancellationToken)
        {
            var fields = await _formFieldRepository.ListByFormTemplateIdAsync(request.FormTemplateId, cancellationToken);
            var dtos = fields.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<FormFieldDto>>.Success(dtos);
        }
    }
}
