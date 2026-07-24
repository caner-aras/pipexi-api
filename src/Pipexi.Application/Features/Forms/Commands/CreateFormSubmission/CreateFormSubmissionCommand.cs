using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.CreateFormSubmission;

public sealed record CreateFormSubmissionAnswerInput(
    Guid FormFieldId,
    string? Value,
    Guid? FileId);

public sealed record CreateFormSubmissionCommand(
    Guid OrganizationId,
    Guid FormTemplateId,
    Guid SubmittedByMemberId,
    Guid? TaskId,
    Guid? ShiftId,
    DateTimeOffset SubmittedAt,
    IReadOnlyCollection<CreateFormSubmissionAnswerInput>? Answers) : ICommand<Result<FormSubmissionDto>>
{
    public sealed class Handler : IRequestHandler<CreateFormSubmissionCommand, Result<FormSubmissionDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IFormAnswerRepository _formAnswerRepository;

        public Handler(
            IOrganizationRepository organizationRepository,
            IFormTemplateRepository formTemplateRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IWorkTaskRepository workTaskRepository,
            IShiftRepository shiftRepository,
            IFormSubmissionRepository formSubmissionRepository,
            IFormFieldRepository formFieldRepository,
            IStoredFileRepository storedFileRepository,
            IFormAnswerRepository formAnswerRepository)
        {
            _organizationRepository = organizationRepository;
            _formTemplateRepository = formTemplateRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _workTaskRepository = workTaskRepository;
            _shiftRepository = shiftRepository;
            _formSubmissionRepository = formSubmissionRepository;
            _formFieldRepository = formFieldRepository;
            _storedFileRepository = storedFileRepository;
            _formAnswerRepository = formAnswerRepository;
        }

        public async Task<Result<FormSubmissionDto>> Handle(CreateFormSubmissionCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_submissions.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var template = await _formTemplateRepository.GetByIdAsync(request.FormTemplateId, cancellationToken);
            if (template is null || template.OrganizationId != request.OrganizationId)
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_submissions.invalid_template", "Form template not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var member = await _organizationMemberRepository.GetByIdAsync(request.SubmittedByMemberId, cancellationToken);
            if (member is null || member.OrganizationId != request.OrganizationId)
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_submissions.invalid_member", "Submitted member not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (request.TaskId.HasValue)
            {
                var task = await _workTaskRepository.GetByIdAsync(request.TaskId.Value, cancellationToken);
                if (task is null || task.OrganizationId != request.OrganizationId)
                {
                    return Result<FormSubmissionDto>.Failure(
                        new AppError("form_submissions.invalid_task", "Task not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.ShiftId.HasValue)
            {
                var shift = await _shiftRepository.GetByIdAsync(request.ShiftId.Value, cancellationToken);
                if (shift is null || shift.OrganizationId != request.OrganizationId)
                {
                    return Result<FormSubmissionDto>.Failure(
                        new AppError("form_submissions.invalid_shift", "Shift not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var submission = FormSubmission.Create(
                request.OrganizationId,
                request.FormTemplateId,
                request.SubmittedByMemberId,
                request.TaskId,
                request.ShiftId,
                request.SubmittedAt);

            await _formSubmissionRepository.AddAsync(submission, cancellationToken);

            var answerInputs = request.Answers ?? Array.Empty<CreateFormSubmissionAnswerInput>();
            var duplicateFieldIds = answerInputs
                .GroupBy(x => x.FormFieldId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateFieldIds.Count > 0)
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_answers.duplicate_field", "Duplicate answers for the same field are not allowed."),
                    (int)HttpStatusCode.BadRequest);
            }

            var fieldIds = answerInputs.Select(x => x.FormFieldId).Distinct().ToList();
            var fields = await _formFieldRepository.GetByIdsAsync(fieldIds, cancellationToken);
            if (fields.Count != fieldIds.Count || fields.Any(x => x.FormTemplateId != request.FormTemplateId))
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_answers.invalid_field", "One or more form fields are invalid for template."),
                    (int)HttpStatusCode.BadRequest);
            }

            var fileIds = answerInputs
                .Where(x => x.FileId.HasValue)
                .Select(x => x.FileId!.Value)
                .Distinct()
                .ToList();
            var files = await _storedFileRepository.GetByIdsAsync(fileIds, cancellationToken);
            if (files.Any(x => x.OrganizationId != request.OrganizationId) || files.Count != fileIds.Count)
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_answers.invalid_file", "One or more files are invalid for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var formFieldMap = fields.ToDictionary(x => x.Id, x => x.ToDto());
            var fileMap = files.ToDictionary(x => x.Id, x => x.ToDto());

            var createdAnswers = new List<FormAnswerDto>();
            foreach (var answerInput in answerInputs)
            {
                var answer = FormAnswer.Create(
                    submission.Id,
                    answerInput.FormFieldId,
                    answerInput.Value,
                    answerInput.FileId);

                await _formAnswerRepository.AddAsync(answer, cancellationToken);
                createdAnswers.Add(answer.ToDto(
                    file: answerInput.FileId.HasValue ? fileMap.GetValueOrDefault(answerInput.FileId.Value) : null,
                    formField: formFieldMap.GetValueOrDefault(answerInput.FormFieldId)));
            }

            return Result<FormSubmissionDto>.Success(
                submission.ToDto(answers: createdAnswers),
                (int)HttpStatusCode.Created);
        }
    }
}
