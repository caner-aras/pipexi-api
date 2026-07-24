using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Queries.GetFormSubmissionById;

public sealed record GetFormSubmissionByIdQuery(Guid Id) : IQuery<Result<FormSubmissionDto>>
{
    public sealed class Handler : IRequestHandler<GetFormSubmissionByIdQuery, Result<FormSubmissionDto>>
    {
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IFormAnswerRepository _formAnswerRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;

        public Handler(
            IFormSubmissionRepository formSubmissionRepository,
            IFormAnswerRepository formAnswerRepository,
            IFormFieldRepository formFieldRepository,
            IStoredFileRepository storedFileRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository)
        {
            _formSubmissionRepository = formSubmissionRepository;
            _formAnswerRepository = formAnswerRepository;
            _formFieldRepository = formFieldRepository;
            _storedFileRepository = storedFileRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<FormSubmissionDto>> Handle(GetFormSubmissionByIdQuery request, CancellationToken cancellationToken)
        {
            var submission = await _formSubmissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (submission is null)
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_submissions.not_found", "Form submission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var answers = await _formAnswerRepository.ListByFormSubmissionIdAsync(submission.Id, cancellationToken);
            var fields = await _formFieldRepository.ListByFormTemplateIdAsync(submission.FormTemplateId, cancellationToken);
            var files = await _storedFileRepository.ListByOrganizationIdAsync(submission.OrganizationId, cancellationToken);
            var formFieldMap = fields.ToDictionary(x => x.Id, x => x.ToDto());
            var fileMap = files.ToDictionary(x => x.Id, x => x.ToDto());

            var answerDtos = answers
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.ToDto(
                    file: x.FileId.HasValue ? fileMap.GetValueOrDefault(x.FileId.Value) : null,
                    formField: formFieldMap.GetValueOrDefault(x.FormFieldId)))
                .ToList();

            var submittedByMember = await _organizationMemberRepository.GetByIdAsync(submission.SubmittedByMemberId, cancellationToken);
            var submittedByUser = submittedByMember is null
                ? null
                : await _userRepository.GetByIdAsync(submittedByMember.UserId, cancellationToken);

            return Result<FormSubmissionDto>.Success(
                submission.ToDto(submittedByMember?.ToDto(submittedByUser?.ToDto()), answerDtos));
        }
    }
}
