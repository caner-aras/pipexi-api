using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Commands.DeleteFormSubmission;

public sealed record DeleteFormSubmissionCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteFormSubmissionCommand, Result<object?>>
    {
        private readonly IFormSubmissionRepository _formSubmissionRepository;

        public Handler(IFormSubmissionRepository formSubmissionRepository)
        {
            _formSubmissionRepository = formSubmissionRepository;
        }

        public async Task<Result<object?>> Handle(DeleteFormSubmissionCommand request, CancellationToken cancellationToken)
        {
            var submission = await _formSubmissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (submission is null)
            {
                return Result<object?>.Failure(
                    new AppError("form_submissions.not_found", "Form submission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _formSubmissionRepository.DeleteAsync(submission, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
