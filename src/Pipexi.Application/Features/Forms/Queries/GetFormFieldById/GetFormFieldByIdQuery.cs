using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormFieldById;

public sealed record GetFormFieldByIdQuery(Guid Id) : IQuery<Result<FormFieldDto>>
{
    public sealed class Handler : IRequestHandler<GetFormFieldByIdQuery, Result<FormFieldDto>>
    {
        private readonly IFormFieldRepository _formFieldRepository;

        public Handler(IFormFieldRepository formFieldRepository)
        {
            _formFieldRepository = formFieldRepository;
        }

        public async Task<Result<FormFieldDto>> Handle(GetFormFieldByIdQuery request, CancellationToken cancellationToken)
        {
            var field = await _formFieldRepository.GetByIdAsync(request.Id, cancellationToken);
            if (field is null)
            {
                return Result<FormFieldDto>.Failure(
                    new AppError("form_fields.not_found", "Form field not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<FormFieldDto>.Success(field.ToDto());
        }
    }
}
