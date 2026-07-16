using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Identity;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Application.Features.OrganizationMembers;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Queries.GetFormSubmissions;

public sealed record GetFormSubmissionsQuery(
    Guid? OrganizationId,
    Guid? FormTemplateId) : IQuery<Result<IReadOnlyCollection<FormSubmissionDto>>>
{
    public sealed class Handler : IRequestHandler<GetFormSubmissionsQuery, Result<IReadOnlyCollection<FormSubmissionDto>>>
    {
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IFormAnswerRepository _formAnswerRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IFormSubmissionRepository formSubmissionRepository,
            IFormAnswerRepository formAnswerRepository,
            IFormFieldRepository formFieldRepository,
            IStoredFileRepository storedFileRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            ICurrentUserContext currentUserContext)
        {
            _formSubmissionRepository = formSubmissionRepository;
            _formAnswerRepository = formAnswerRepository;
            _formFieldRepository = formFieldRepository;
            _storedFileRepository = storedFileRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<FormSubmissionDto>>> Handle(GetFormSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<FormSubmissionDto>>.Failure(
                    new AppError("auth.unauthorized", "Unauthorized."),
                    (int)HttpStatusCode.Unauthorized);
            }

            IReadOnlyCollection<FormSubmission> submissions;
            if (request.FormTemplateId.HasValue)
            {
                submissions = await _formSubmissionRepository.ListByFormTemplateIdAsync(request.FormTemplateId.Value, cancellationToken);
                submissions = submissions.Where(x => x.OrganizationId == organizationId).ToList();
            }
            else
            {
                submissions = await _formSubmissionRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            }

            var formTemplateIds = submissions.Select(x => x.FormTemplateId).Distinct().ToList();
            var formFieldMap = new Dictionary<Guid, FormFieldDto>();
            if (formTemplateIds.Count > 0)
            {
                var fields = await _formFieldRepository.ListByFormTemplateIdsAsync(formTemplateIds, cancellationToken);
                formFieldMap = fields.ToDictionary(x => x.Id, x => x.ToDto());
            }

            var answersBySubmissionId = new Dictionary<Guid, IReadOnlyCollection<FormAnswer>>();
            var submissionIds = submissions.Select(x => x.Id).ToList();
            if (submissionIds.Count > 0)
            {
                var answers = await _formAnswerRepository.ListByFormSubmissionIdsAsync(submissionIds, cancellationToken);
                answersBySubmissionId = answers
                    .GroupBy(x => x.FormSubmissionId)
                    .ToDictionary(g => g.Key, g => (IReadOnlyCollection<FormAnswer>)g.OrderBy(x => x.CreatedAt).ToList());
            }

            var fileMap = new Dictionary<Guid, StoredFileDto>();
            var files = await _storedFileRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            fileMap = files.ToDictionary(x => x.Id, x => x.ToDto());

            var dtos = submissions
                .OrderByDescending(x => x.SubmittedAt)
                .Select(submission =>
                {
                    var answerDtos = (answersBySubmissionId.GetValueOrDefault(submission.Id) ?? Array.Empty<FormAnswer>())
                        .Select(answer => answer.ToDto(
                            file: answer.FileId.HasValue ? fileMap.GetValueOrDefault(answer.FileId.Value) : null,
                            formField: formFieldMap.GetValueOrDefault(answer.FormFieldId)))
                        .ToList();

                    return submission.ToDto(answers: answerDtos);
                })
                .ToList();

            var submittedByMemberIds = submissions
                .Select(x => x.SubmittedByMemberId)
                .Distinct()
                .ToList();

            var submittedByMemberMap = new Dictionary<Guid, OrganizationMemberDto>();
            if (submittedByMemberIds.Count > 0)
            {
                IReadOnlyCollection<Workforce.Domain.Entities.OrganizationMember> submittedByMembers;
                var allMembers = await _organizationMemberRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
                submittedByMembers = allMembers.Where(x => submittedByMemberIds.Contains(x.Id)).ToList();

                var userIds = submittedByMembers.Select(x => x.UserId).Distinct().ToList();
                var users = userIds.Count > 0
                    ? await _userRepository.ListByIdsAsync(userIds, cancellationToken)
                    : Array.Empty<Workforce.Domain.Entities.User>();
                var userMap = users.ToDictionary(x => x.Id, x => x.ToDto());

                submittedByMemberMap = submittedByMembers.ToDictionary(
                    x => x.Id,
                    x => x.ToDto(userMap.GetValueOrDefault(x.UserId)));
            }

            dtos = dtos
                .Select(x => x with { SubmittedByMember = submittedByMemberMap.GetValueOrDefault(x.SubmittedByMemberId) })
                .ToList();

            return Result<IReadOnlyCollection<FormSubmissionDto>>.Success(dtos);
        }
    }
}
