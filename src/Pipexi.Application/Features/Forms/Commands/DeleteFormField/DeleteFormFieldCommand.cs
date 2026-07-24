using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.DeleteFormField;

public sealed record DeleteFormFieldCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteFormFieldCommand, Result<object?>>
    {
        private readonly IFormFieldRepository _formFieldRepository;

        public Handler(IFormFieldRepository formFieldRepository)
        {
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

            await _formFieldRepository.DeleteAsync(field, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
