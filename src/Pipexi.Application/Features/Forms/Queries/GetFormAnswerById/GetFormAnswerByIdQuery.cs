using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Queries.GetFormAnswerById;

public sealed record GetFormAnswerByIdQuery(Guid Id) : IQuery<Result<FormAnswerDto>>
{
    public sealed class Handler : IRequestHandler<GetFormAnswerByIdQuery, Result<FormAnswerDto>>
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

        public async Task<Result<FormAnswerDto>> Handle(GetFormAnswerByIdQuery request, CancellationToken cancellationToken)
        {
            var answer = await _formAnswerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (answer is null)
            {
                return Result<FormAnswerDto>.Failure(
                    new AppError("form_answers.not_found", "Form answer not found."),
                    (int)HttpStatusCode.NotFound);
            }

            StoredFileDto? fileDto = null;
            if (answer.FileId.HasValue)
            {
                var submission = await _formSubmissionRepository.GetByIdAsync(answer.FormSubmissionId, cancellationToken);
                if (submission is not null)
                {
                    var files = await _storedFileRepository.ListByOrganizationIdAsync(submission.OrganizationId, cancellationToken);
                    var file = files.FirstOrDefault(x => x.Id == answer.FileId.Value);
                    fileDto = file?.ToDto();
                }
            }

            var field = await _formFieldRepository.GetByIdAsync(answer.FormFieldId, cancellationToken);
            var fieldDto = field?.ToDto();

            return Result<FormAnswerDto>.Success(answer.ToDto(file: fileDto, formField: fieldDto));
        }
    }
}
