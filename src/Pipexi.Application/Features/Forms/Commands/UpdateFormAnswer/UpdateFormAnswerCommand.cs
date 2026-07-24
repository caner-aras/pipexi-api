using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Forms.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Forms.Commands.UpdateFormAnswer;

public sealed record UpdateFormAnswerCommand(
    Guid Id,
    string? Value,
    Guid? FileId,
    string? Status) : ICommand<Result<FormAnswerDto>>
{
    public sealed class Handler : IRequestHandler<UpdateFormAnswerCommand, Result<FormAnswerDto>>
    {
        private readonly IFormAnswerRepository _formAnswerRepository;
        private readonly IFormSubmissionRepository _formSubmissionRepository;
        private readonly IFormFieldRepository _formFieldRepository;
        private readonly IStoredFileRepository _storedFileRepository;

        public Handler(
            IFormAnswerRepository formAnswerRepository,
            IFormSubmissionRepository formSubmissionRepository,
            IFormFieldRepository formFieldRepository,
            IStoredFileRepository storedFileRepository)
        {
            _formAnswerRepository = formAnswerRepository;
            _formSubmissionRepository = formSubmissionRepository;
            _formFieldRepository = formFieldRepository;
            _storedFileRepository = storedFileRepository;
        }

        public async Task<Result<FormAnswerDto>> Handle(UpdateFormAnswerCommand request, CancellationToken cancellationToken)
        {
            var answer = await _formAnswerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (answer is null)
            {
                return Result<FormAnswerDto>.Failure(
                    new AppError("form_answers.not_found", "Form answer not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var submission = await _formSubmissionRepository.GetByIdAsync(answer.FormSubmissionId, cancellationToken);
            if (submission is null)
            {
                return Result<FormAnswerDto>.Failure(
                    new AppError("form_answers.invalid_submission", "Form submission not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            StoredFileDto? fileDto = null;
            if (request.FileId.HasValue)
            {
                var file = await _storedFileRepository.GetByIdAsync(request.FileId.Value, cancellationToken);
                if (file is null || file.OrganizationId != submission.OrganizationId)
                {
                    return Result<FormAnswerDto>.Failure(
                        new AppError("form_answers.invalid_file", "File not found for submission organization."),
                        (int)HttpStatusCode.BadRequest);
                }

                fileDto = file.ToDto();
            }

            answer.UpdateDetails(request.Value, request.FileId, request.Status);
            await _formAnswerRepository.UpdateAsync(answer, cancellationToken);

            if (fileDto is null && answer.FileId.HasValue)
            {
                var existingFile = await _storedFileRepository.GetByIdAsync(answer.FileId.Value, cancellationToken);
                if (existingFile is not null)
                {
                    fileDto = existingFile.ToDto();
                }
            }

            var field = await _formFieldRepository.GetByIdAsync(answer.FormFieldId, cancellationToken);
            var fieldDto = field?.ToDto();

            return Result<FormAnswerDto>.Success(answer.ToDto(file: fileDto, formField: fieldDto), (int)HttpStatusCode.OK);
        }
    }
}
