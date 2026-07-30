using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.DeleteFormAnswer;

public sealed record DeleteFormAnswerCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteFormAnswerCommand, Result<object?>>
    {
        private readonly IFormAnswerRepository _formAnswerRepository;
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IFormAnswerRepository formAnswerRepository,
            IFormSubmissionRepository formSubmissionRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
            _formSubmissionRepository = formSubmissionRepository;
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

            var formSubmission = await _formSubmissionRepository.GetByIdAsync(answer.FormSubmissionId, cancellationToken);
            if (formSubmission is null)
            {
                return Result<object?>.Failure(
                    new AppError("form_answers.invalid_submission", "Form submission not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                formSubmission.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _formAnswerRepository.DeleteAsync(answer, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
