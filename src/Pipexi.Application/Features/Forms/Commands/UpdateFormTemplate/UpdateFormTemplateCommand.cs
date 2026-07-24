using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.UpdateFormTemplate;

public sealed record UpdateFormTemplateCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? Status) : ICommand<Result<FormTemplateDto>>
{
    public sealed class Handler : IRequestHandler<UpdateFormTemplateCommand, Result<FormTemplateDto>>
    {
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IFormFieldRepository _formFieldRepository;

        public Handler(IFormTemplateRepository formTemplateRepository, IFormFieldRepository formFieldRepository)
        {
            _formTemplateRepository = formTemplateRepository;
            _formFieldRepository = formFieldRepository;
        }

        public async Task<Result<FormTemplateDto>> Handle(UpdateFormTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _formTemplateRepository.GetByIdAsync(request.Id, cancellationToken);
            if (template is null)
            {
                return Result<FormTemplateDto>.Failure(
                    new AppError("form_templates.not_found", "Form template not found."),
                    (int)HttpStatusCode.NotFound);
            }

            template.UpdateDetails(request.Name, request.Description, request.Status);
            await _formTemplateRepository.UpdateAsync(template, cancellationToken);

            var fields = await _formFieldRepository.ListByFormTemplateIdAsync(template.Id, cancellationToken);
            return Result<FormTemplateDto>.Success(template.ToDto(fields.Select(x => x.ToDto()).ToList()));
        }
    }
}
