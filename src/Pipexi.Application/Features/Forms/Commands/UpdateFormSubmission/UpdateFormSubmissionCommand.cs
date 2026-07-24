using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.UpdateFormSubmission;

public sealed record UpdateFormSubmissionAnswerInput(
    Guid FormFieldId,
    string? Value,
    Guid? FileId,
    string? Status);

public sealed record UpdateFormSubmissionCommand(
    Guid Id,
    Guid? TaskId,
    Guid? ShiftId,
    DateTimeOffset? SubmittedAt,
    string? Status,
    IReadOnlyCollection<UpdateFormSubmissionAnswerInput>? Answers) : ICommand<Result<FormSubmissionDto>>
{
    public sealed class Handler : IRequestHandler<UpdateFormSubmissionCommand, Result<FormSubmissionDto>>
    {
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IWorkTaskRepository _workTaskRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly IFormAnswerRepository _formAnswerRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IStoredFileRepository _storedFileRepository;

        public Handler(
            IFormSubmissionRepository formSubmissionRepository,
            IWorkTaskRepository workTaskRepository,
            IShiftRepository shiftRepository,
            IFormAnswerRepository formAnswerRepository,
            IFormFieldRepository formFieldRepository,
            IStoredFileRepository storedFileRepository)
        {
            _formSubmissionRepository = formSubmissionRepository;
            _workTaskRepository = workTaskRepository;
            _shiftRepository = shiftRepository;
            _formAnswerRepository = formAnswerRepository;
            _formFieldRepository = formFieldRepository;
            _storedFileRepository = storedFileRepository;
        }

        public async Task<Result<FormSubmissionDto>> Handle(UpdateFormSubmissionCommand request, CancellationToken cancellationToken)
        {
            var submission = await _formSubmissionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (submission is null)
            {
                return Result<FormSubmissionDto>.Failure(
                    new AppError("form_submissions.not_found", "Form submission not found."),
                    (int)HttpStatusCode.NotFound);
            }

            if (request.TaskId.HasValue)
            {
                var task = await _workTaskRepository.GetByIdAsync(request.TaskId.Value, cancellationToken);
                if (task is null || task.OrganizationId != submission.OrganizationId)
                {
                    return Result<FormSubmissionDto>.Failure(
                        new AppError("form_submissions.invalid_task", "Task not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.ShiftId.HasValue)
            {
                var shift = await _shiftRepository.GetByIdAsync(request.ShiftId.Value, cancellationToken);
                if (shift is null || shift.OrganizationId != submission.OrganizationId)
                {
                    return Result<FormSubmissionDto>.Failure(
                        new AppError("form_submissions.invalid_shift", "Shift not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            submission.UpdateDetails(request.TaskId, request.ShiftId, request.SubmittedAt, request.Status);
            await _formSubmissionRepository.UpdateAsync(submission, cancellationToken);

            if (request.Answers is not null)
            {
                var answerInputs = request.Answers;
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
                var inputFields = await _formFieldRepository.GetByIdsAsync(fieldIds, cancellationToken);
                if (inputFields.Count != fieldIds.Count || inputFields.Any(x => x.FormTemplateId != submission.FormTemplateId))
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
                var inputFiles = await _storedFileRepository.GetByIdsAsync(fileIds, cancellationToken);
                if (inputFiles.Any(x => x.OrganizationId != submission.OrganizationId) || inputFiles.Count != fileIds.Count)
                {
                    return Result<FormSubmissionDto>.Failure(
                        new AppError("form_answers.invalid_file", "One or more files are invalid for submission organization."),
                        (int)HttpStatusCode.BadRequest);
                }

                var existingAnswers = await _formAnswerRepository.ListByFormSubmissionIdAsync(submission.Id, cancellationToken);
                var existingAnswerMap = existingAnswers.ToDictionary(x => x.FormFieldId, x => x);

                foreach (var answerInput in answerInputs)
                {
                    if (existingAnswerMap.TryGetValue(answerInput.FormFieldId, out var existingAnswer))
                    {
                        existingAnswer.UpdateDetails(answerInput.Value, answerInput.FileId, answerInput.Status);
                        await _formAnswerRepository.UpdateAsync(existingAnswer, cancellationToken);
                        continue;
                    }

                    var createdAnswer = FormAnswer.Create(
                        submission.Id,
                        answerInput.FormFieldId,
                        answerInput.Value,
                        answerInput.FileId);
                    await _formAnswerRepository.AddAsync(createdAnswer, cancellationToken);
                }
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

            return Result<FormSubmissionDto>.Success(submission.ToDto(answers: answerDtos), (int)HttpStatusCode.OK);
        }
    }
}
