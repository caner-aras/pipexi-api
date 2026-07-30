using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormAnswers;

public sealed record GetFormAnswersQuery(Guid FormSubmissionId, Guid? ScopedOrganizationId = null) : IQuery<Result<IReadOnlyCollection<FormAnswerDto>>>
{
    public sealed class Handler : IRequestHandler<GetFormAnswersQuery, Result<IReadOnlyCollection<FormAnswerDto>>>
    {
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IFormAnswerRepository _formAnswerRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IOrganizationAccessService _organizationAccess;

        public Handler(
            IFormSubmissionRepository formSubmissionRepository,
            IFormAnswerRepository formAnswerRepository,
            IFormFieldRepository formFieldRepository,
            IStoredFileRepository storedFileRepository,
            IOrganizationAccessService organizationAccess)
        {
            _formSubmissionRepository = formSubmissionRepository;
            _formAnswerRepository = formAnswerRepository;
            _formFieldRepository = formFieldRepository;
            _storedFileRepository = storedFileRepository;
            _organizationAccess = organizationAccess;
        }

        public async Task<Result<IReadOnlyCollection<FormAnswerDto>>> Handle(GetFormAnswersQuery request, CancellationToken cancellationToken)
        {
            var submission = await _formSubmissionRepository.GetByIdAsync(request.FormSubmissionId, cancellationToken);
            if (submission is null)
            {
                return Result<IReadOnlyCollection<FormAnswerDto>>.Failure(
                    new AppError("form_submissions.not_found", "Form submission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var accessDenied = await _organizationAccess.ValidateResourceAccessAsync<IReadOnlyCollection<FormAnswerDto>>(
                submission.OrganizationId, request.ScopedOrganizationId, cancellationToken);
            if (accessDenied is not null) return accessDenied;

            var answers = await _formAnswerRepository.ListByFormSubmissionIdAsync(request.FormSubmissionId, cancellationToken);
            var fields = await _formFieldRepository.ListByFormTemplateIdAsync(submission.FormTemplateId, cancellationToken);
            var files = await _storedFileRepository.ListByOrganizationIdAsync(submission.OrganizationId, cancellationToken);
            var formFieldMap = fields.ToDictionary(x => x.Id, x => x.ToDto());
            var fileMap = files.ToDictionary(x => x.Id, x => x.ToDto());

            var dtos = answers
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.ToDto(
                    file: x.FileId.HasValue ? fileMap.GetValueOrDefault(x.FileId.Value) : null,
                    formField: formFieldMap.GetValueOrDefault(x.FormFieldId)))
                .ToList();

            return Result<IReadOnlyCollection<FormAnswerDto>>.Success(dtos);
        }
    }
}
