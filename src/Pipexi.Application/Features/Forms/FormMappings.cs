using Workforce.Application.Features.Forms.Dtos;
using Workforce.Application.Features.OrganizationMembers.Dtos;
using Workforce.Domain.Entities;

namespace Workforce.Application.Features.Forms;

internal static class FormMappings
{
    public static FormTemplateDto ToDto(this FormTemplate formTemplate, IReadOnlyCollection<FormFieldDto>? fields = null)
    {
        return new FormTemplateDto(
            formTemplate.Id,
            formTemplate.OrganizationId,
            formTemplate.Name,
            formTemplate.Description,
            formTemplate.Status,
            formTemplate.CreatedAt,
            formTemplate.UpdatedAt,
            fields ?? Array.Empty<FormFieldDto>());
    }

    public static FormFieldDto ToDto(this FormField formField)
    {
        return new FormFieldDto(
            formField.Id,
            formField.FormTemplateId,
            formField.Type,
            formField.Label,
            formField.IsRequired,
            formField.SortOrder,
            formField.OptionsJson,
            formField.Status,
            formField.CreatedAt,
            formField.UpdatedAt);
    }

    public static FormSubmissionDto ToDto(
        this FormSubmission formSubmission,
        OrganizationMemberDto? submittedByMember = null,
        IReadOnlyCollection<FormAnswerDto>? answers = null)
    {
        return new FormSubmissionDto(
            formSubmission.Id,
            formSubmission.OrganizationId,
            formSubmission.FormTemplateId,
            formSubmission.SubmittedByMemberId,
            submittedByMember,
            formSubmission.TaskId,
            formSubmission.ShiftId,
            formSubmission.SubmittedAt,
            formSubmission.Status,
            formSubmission.CreatedAt,
            formSubmission.UpdatedAt,
            answers ?? Array.Empty<FormAnswerDto>());
    }

    public static FormAnswerDto ToDto(
        this FormAnswer formAnswer,
        StoredFileDto? file = null,
        FormFieldDto? formField = null)
    {
        return new FormAnswerDto(
            formAnswer.Id,
            formAnswer.FormSubmissionId,
            formAnswer.FormFieldId,
            formField,
            formAnswer.Value,
            formAnswer.FileId,
            formAnswer.Status,
            formAnswer.CreatedAt,
            formAnswer.UpdatedAt,
            file);
    }

    public static StoredFileDto ToDto(this StoredFile storedFile)
    {
        return new StoredFileDto(
            storedFile.Id,
            storedFile.OrganizationId,
            storedFile.FileName,
            storedFile.ContentType,
            storedFile.StoragePath,
            storedFile.SizeBytes,
            storedFile.Status,
            storedFile.CreatedAt,
            storedFile.UpdatedAt);
    }
}
