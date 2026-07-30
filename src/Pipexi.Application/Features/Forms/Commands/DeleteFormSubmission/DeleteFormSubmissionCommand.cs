using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.DeleteFormSubmission;

public sealed record DeleteFormSubmissionCommand(Guid Id, Guid? ScopedOrganizationId = null) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteFormSubmissionCommand, Result<object?>>
    {
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(IFormSubmissionRepository formSubmissionRepository,
            IOrganizationAccessService organizationAccess)
        {
            _organizationAccess = organizationAccess;
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


            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<object?>(
                submission.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;
            await _formSubmissionRepository.DeleteAsync(submission, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
