using MediatR;
using Pipexi.Application.Features.Forms.Commands.CreateFormAnswer;
using Pipexi.Application.Features.Forms.Commands.CreateFormField;
using Pipexi.Application.Features.Forms.Commands.CreateFormSubmission;
using Pipexi.Application.Features.Forms.Commands.CreateFormTemplate;
using Pipexi.Application.Features.Forms.Commands.CreateStoredFile;
using Pipexi.Application.Features.Forms.Commands.DeleteFormAnswer;
using Pipexi.Application.Features.Forms.Commands.DeleteFormField;
using Pipexi.Application.Features.Forms.Commands.DeleteFormSubmission;
using Pipexi.Application.Features.Forms.Commands.DeleteFormTemplate;
using Pipexi.Application.Features.Forms.Commands.DeleteStoredFile;
using Pipexi.Application.Features.Forms.Commands.UpdateFormField;
using Pipexi.Application.Features.Forms.Commands.UpdateFormSubmission;
using Pipexi.Application.Features.Forms.Commands.UpdateFormTemplate;
using Pipexi.Application.Features.Forms.Commands.UpdateStoredFile;
using Pipexi.Application.Features.Forms.Queries.GetFormAnswerById;
using Pipexi.Application.Features.Forms.Queries.GetFormAnswers;
using Pipexi.Application.Features.Forms.Queries.GetFormFieldById;
using Pipexi.Application.Features.Forms.Queries.GetFormFields;
using Pipexi.Application.Features.Forms.Queries.GetFormSubmissionById;
using Pipexi.Application.Features.Forms.Queries.GetFormSubmissions;
using Pipexi.Application.Features.Forms.Queries.GetFormTemplateById;
using Pipexi.Application.Features.Forms.Queries.GetFormTemplates;
using Pipexi.Application.Features.Forms.Queries.GetStoredFileById;
using Pipexi.Application.Features.Forms.Queries.GetStoredFiles;
using Pipexi.Contracts.V1.Forms;

namespace Pipexi.Api.Endpoints.V1;

public static class FormEndpoints
{
    public static IEndpointRouteBuilder MapFormEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/forms")
            .WithTags("forms")
            .RequireAuthorization();

        group.MapGet("/templates", ListFormTemplatesAsync);
        group.MapGet("/templates/{id:guid}", GetFormTemplateByIdAsync);
        group.MapPost("/templates", CreateFormTemplateAsync);
        group.MapPut("/templates/{id:guid}", UpdateFormTemplateAsync);
        group.MapDelete("/templates/{id:guid}", DeleteFormTemplateAsync);

        group.MapGet("/templates/{formTemplateId:guid}/fields", ListFormFieldsAsync);
        group.MapGet("/fields/{id:guid}", GetFormFieldByIdAsync);
        group.MapPost("/fields", CreateFormFieldAsync);
        group.MapPut("/fields/{id:guid}", UpdateFormFieldAsync);
        group.MapDelete("/fields/{id:guid}", DeleteFormFieldAsync);

        group.MapGet("/submissions", ListFormSubmissionsAsync);
        group.MapGet("/submissions/{id:guid}", GetFormSubmissionByIdAsync);
        group.MapPost("/submissions", CreateFormSubmissionAsync);
        group.MapPut("/submissions/{id:guid}", UpdateFormSubmissionAsync);
        group.MapDelete("/submissions/{id:guid}", DeleteFormSubmissionAsync);

        group.MapGet("/submissions/{formSubmissionId:guid}/answers", ListFormAnswersAsync);
        group.MapGet("/answers/{id:guid}", GetFormAnswerByIdAsync);
        group.MapDelete("/answers/{id:guid}", DeleteFormAnswerAsync);

        group.MapGet("/files", ListStoredFilesAsync);
        group.MapGet("/files/{id:guid}", GetStoredFileByIdAsync);
        group.MapPost("/files", CreateStoredFileAsync);
        group.MapPut("/files/{id:guid}", UpdateStoredFileAsync);
        group.MapDelete("/files/{id:guid}", DeleteStoredFileAsync);

        return app;
    }

    private static async Task<IResult> ListFormTemplatesAsync(Guid? organizationId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormTemplatesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetFormTemplateByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormTemplateByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateFormTemplateAsync(
        CreateFormTemplateRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateFormTemplateCommand(request.OrganizationId, request.Name, request.Description),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateFormTemplateAsync(
        Guid id,
        UpdateFormTemplateRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateFormTemplateCommand(id, request.Name, request.Description, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteFormTemplateAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteFormTemplateCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListFormFieldsAsync(Guid formTemplateId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormFieldsQuery(formTemplateId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetFormFieldByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormFieldByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateFormFieldAsync(
        CreateFormFieldRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateFormFieldCommand(
                request.FormTemplateId,
                request.Type,
                request.Label,
                request.IsRequired,
                request.SortOrder,
                request.OptionsJson),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateFormFieldAsync(
        Guid id,
        UpdateFormFieldRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateFormFieldCommand(
                id,
                request.Type,
                request.Label,
                request.IsRequired,
                request.SortOrder,
                request.OptionsJson,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteFormFieldAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteFormFieldCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListFormSubmissionsAsync(
        Guid? organizationId,
        Guid? formTemplateId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormSubmissionsQuery(organizationId, formTemplateId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetFormSubmissionByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormSubmissionByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateFormSubmissionAsync(
        CreateFormSubmissionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateFormSubmissionCommand(
                request.OrganizationId,
                request.FormTemplateId,
                request.SubmittedByMemberId,
                request.TaskId,
                request.ShiftId,
                request.SubmittedAt,
                request.Answers?.Select(x => new CreateFormSubmissionAnswerInput(
                    x.FormFieldId,
                    x.Value,
                    x.FileId)).ToList()),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateFormSubmissionAsync(
        Guid id,
        UpdateFormSubmissionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateFormSubmissionCommand(
                id,
                request.TaskId,
                request.ShiftId,
                request.SubmittedAt,
                request.Status,
                request.Answers?.Select(x => new UpdateFormSubmissionAnswerInput(
                    x.FormFieldId,
                    x.Value,
                    x.FileId,
                    x.Status)).ToList()),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteFormSubmissionAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteFormSubmissionCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListFormAnswersAsync(Guid formSubmissionId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormAnswersQuery(formSubmissionId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetFormAnswerByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormAnswerByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteFormAnswerAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteFormAnswerCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListStoredFilesAsync(Guid? organizationId, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoredFilesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetStoredFileByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoredFileByIdQuery(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateStoredFileAsync(
        CreateStoredFileRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateStoredFileCommand(
                request.OrganizationId,
                request.FileName,
                request.ContentType,
                request.StoragePath,
                request.SizeBytes),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateStoredFileAsync(
        Guid id,
        UpdateStoredFileRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateStoredFileCommand(
                id,
                request.FileName,
                request.ContentType,
                request.StoragePath,
                request.SizeBytes,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteStoredFileAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteStoredFileCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
