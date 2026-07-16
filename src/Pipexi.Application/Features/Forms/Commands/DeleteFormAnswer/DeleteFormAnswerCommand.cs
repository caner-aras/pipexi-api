using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Commands.DeleteFormAnswer;

public sealed record DeleteFormAnswerCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteFormAnswerCommand, Result<object?>>
    {
        private readonly IFormAnswerRepository _formAnswerRepository;

        public Handler(IFormAnswerRepository formAnswerRepository)
        {
            _formAnswerRepository = formAnswerRepository;
        }

        public async Task<Result<object?>> Handle(DeleteFormAnswerCommand request, CancellationToken cancellationToken)
        {
            var answer = await _formAnswerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (answer is null)
            {
                return Result<object?>.Failure(
                    new AppError("form_answers.not_found", "Form answer not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _formAnswerRepository.DeleteAsync(answer, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
