using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.CreateFormAnswer;

public sealed record CreateFormAnswerCommand(
    Guid FormSubmissionId,
    Guid FormFieldId,
    string? Value,
    Guid? FileId) : ICommand<Result<FormAnswerDto>>
{
    public sealed class Handler : IRequestHandler<CreateFormAnswerCommand, Result<FormAnswerDto>>
    {
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IStoredFileRepository _storedFileRepository;
        private readonly IFormAnswerRepository _formAnswerRepository;

        public Handler(
            IFormSubmissionRepository formSubmissionRepository,
            IFormFieldRepository formFieldRepository,
            IStoredFileRepository storedFileRepository,
            IFormAnswerRepository formAnswerRepository)
        {
            _formSubmissionRepository = formSubmissionRepository;
            _formFieldRepository = formFieldRepository;
            _storedFileRepository = storedFileRepository;
            _formAnswerRepository = formAnswerRepository;
        }

        public async Task<Result<FormAnswerDto>> Handle(CreateFormAnswerCommand request, CancellationToken cancellationToken)
        {
            var submission = await _formSubmissionRepository.GetByIdAsync(request.FormSubmissionId, cancellationToken);
            if (submission is null)
            {
                return Result<FormAnswerDto>.Failure(
                    new AppError("form_answers.invalid_submission", "Form submission not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var field = await _formFieldRepository.GetByIdAsync(request.FormFieldId, cancellationToken);
            if (field is null || field.FormTemplateId != submission.FormTemplateId)
            {
                return Result<FormAnswerDto>.Failure(
                    new AppError("form_answers.invalid_field", "Form field not found for submission template."),
                    (int)HttpStatusCode.BadRequest);
            }

            var existingAnswers = await _formAnswerRepository.ListByFormSubmissionIdAsync(submission.Id, cancellationToken);
            if (existingAnswers.Any(x => x.FormFieldId == request.FormFieldId))
            {
                return Result<FormAnswerDto>.Failure(
                    new AppError("form_answers.duplicate_field", "This field already has an answer in the submission."),
                    (int)HttpStatusCode.BadRequest);
            }

            StoredFile? file = null;
            if (request.FileId.HasValue)
            {
                file = await _storedFileRepository.GetByIdAsync(request.FileId.Value, cancellationToken);
                if (file is null || file.OrganizationId != submission.OrganizationId)
                {
                    return Result<FormAnswerDto>.Failure(
                        new AppError("form_answers.invalid_file", "File not found for submission organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var answer = FormAnswer.Create(
                request.FormSubmissionId,
                request.FormFieldId,
                request.Value,
                request.FileId);

            await _formAnswerRepository.AddAsync(answer, cancellationToken);

            return Result<FormAnswerDto>.Success(answer.ToDto(file: file?.ToDto(), formField: field.ToDto()), (int)HttpStatusCode.Created);
        }
    }
}
