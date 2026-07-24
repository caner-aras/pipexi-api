using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormTemplateById;

public sealed record GetFormTemplateByIdQuery(Guid Id) : IQuery<Result<FormTemplateDto>>
{
    public sealed class Handler : IRequestHandler<GetFormTemplateByIdQuery, Result<FormTemplateDto>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IFormFieldRepository _formFieldRepository;

        public Handler(IFormTemplateRepository formTemplateRepository, IFormFieldRepository formFieldRepository)
        {
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

            var fields = await _formFieldRepository.ListByFormTemplateIdAsync(template.Id, cancellationToken);
            return Result<FormTemplateDto>.Success(template.ToDto(fields.Select(x => x.ToDto()).ToList()));
        }
    }
}
