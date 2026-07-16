using MediatR;
using Workforce.Application.Abstractions.Identity;
using Workforce.Application.Features.Forms.Commands.CreateFormAnswer;
using Workforce.Application.Features.Forms.Commands.CreateFormField;
using Workforce.Application.Features.Forms.Commands.CreateFormSubmission;
using Workforce.Application.Features.Forms.Commands.CreateFormTemplate;
using Workforce.Application.Features.Forms.Commands.CreateStoredFile;
using Workforce.Application.Features.Forms.Commands.DeleteFormAnswer;
using Workforce.Application.Features.Forms.Commands.DeleteFormField;
using Workforce.Application.Features.Forms.Commands.DeleteFormSubmission;
using Workforce.Application.Features.Forms.Commands.DeleteFormTemplate;
using Workforce.Application.Features.Forms.Commands.DeleteStoredFile;
using Workforce.Application.Features.Forms.Commands.UpdateFormAnswer;
using Workforce.Application.Features.Forms.Commands.UpdateFormField;
using Workforce.Application.Features.Forms.Commands.UpdateFormSubmission;
using Workforce.Application.Features.Forms.Commands.UpdateFormTemplate;
using Workforce.Application.Features.Forms.Commands.UpdateStoredFile;
using Workforce.Application.Features.Forms.Queries.GetFormAnswerById;
using Workforce.Application.Features.Forms.Queries.GetFormAnswers;
using Workforce.Application.Features.Forms.Queries.GetFormFieldById;
using Workforce.Application.Features.Forms.Queries.GetFormFields;
using Workforce.Application.Features.Forms.Queries.GetFormSubmissionById;
using Workforce.Application.Features.Forms.Queries.GetFormSubmissions;
using Workforce.Application.Features.Forms.Queries.GetFormTemplateById;
using Workforce.Application.Features.Forms.Queries.GetFormTemplates;
using Workforce.Application.Features.Forms.Queries.GetShiftFormTemplates;
using Workforce.Application.Features.Forms.Queries.GetStoredFileById;
using Workforce.Application.Features.Forms.Queries.GetStoredFiles;
using Workforce.Application.Features.Locations.Commands.CreateLocation;
using Workforce.Application.Features.Locations.Commands.SetLocationWorkingHours;
using Workforce.Application.Features.Locations.Queries.GetLocations;
using Workforce.Application.Features.Locations.Queries.GetLocationWorkingHours;
using Workforce.Application.Features.OrganizationMembers.Commands.CreateOrganizationMember;
using Workforce.Application.Features.OrganizationMembers.Queries.GetOrganizationMembers;
using Workforce.Application.Features.Organizations.Commands.CreateOrganization;
using Workforce.Application.Features.Organizations.Commands.DeleteOrganization;
using Workforce.Application.Features.Organizations.Commands.UpdateOrganization;
using Workforce.Application.Features.Organizations.Dtos;
using Workforce.Application.Features.Organizations.Queries.GetOrganizationById;
using Workforce.Application.Features.Organizations.Queries.GetOrganizations;
using Workforce.Application.Features.Roles.Commands.CreateRole;
using Workforce.Application.Features.Roles.Queries.GetRoles;
using Workforce.Application.Features.Shifts.Commands.CreateShift;
using Workforce.Application.Features.Shifts.Commands.CreateShiftBreak;
using Workforce.Application.Features.Shifts.Commands.DeleteShift;
using Workforce.Application.Features.Shifts.Commands.DeleteShiftBreak;
using Workforce.Application.Features.Shifts.Commands.UpdateShift;
using Workforce.Application.Features.Shifts.Commands.UpdateShiftBreak;
using Workforce.Application.Features.Shifts.Queries.GetOrganizationShifts;
using Workforce.Application.Features.Shifts.Queries.GetShiftBreakById;
using Workforce.Application.Features.Shifts.Queries.GetShiftBreaks;
using Workforce.Application.Features.Shifts.Queries.GetShiftById;
using Workforce.Application.Features.Shifts.Queries.GetShifts;
using Workforce.Application.Features.Tasks.Commands.CreateTask;
using Workforce.Application.Features.Tasks.Commands.CreateTaskComment;
using Workforce.Application.Features.Tasks.Commands.DeleteTask;
using Workforce.Application.Features.Tasks.Commands.DeleteTaskComment;
using Workforce.Application.Features.Tasks.Commands.UpdateTask;
using Workforce.Application.Features.Tasks.Commands.UpdateTaskComment;
using Workforce.Application.Features.Tasks.Queries.GetTaskById;
using Workforce.Application.Features.Tasks.Queries.GetTaskCommentById;
using Workforce.Application.Features.Tasks.Queries.GetTaskComments;
using Workforce.Application.Features.Tasks.Queries.GetTasks;
using Workforce.Contracts.V1.Organizations;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Api.Endpoints.V1;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/organizations")
            .WithTags("organizations")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        group.MapGet("/{organizationId:guid}/members", ListMembersAsync);
        group.MapPost("/{organizationId:guid}/members", CreateMemberAsync);

        group.MapGet("/{organizationId:guid}/roles", ListRolesAsync);
        group.MapPost("/{organizationId:guid}/roles", CreateRoleAsync);

        group.MapGet("/{organizationId:guid}/locations", ListLocationsAsync);
        group.MapPost("/{organizationId:guid}/locations", CreateLocationAsync);
        group.MapGet("/{organizationId:guid}/locations/{locationId:guid}/working-hours", ListLocationWorkingHoursAsync);
        group.MapPost("/{organizationId:guid}/locations/{locationId:guid}/working-hours", CreateLocationWorkingHourAsync);
        group.MapPut("/{organizationId:guid}/locations/{locationId:guid}/working-hours", SetLocationWorkingHoursAsync);

        group.MapGet("/{organizationId:guid}/shifts", ListShiftsAsync);
        group.MapPost("/{organizationId:guid}/shifts", CreateShiftAsync);
        group.MapGet("/{organizationId:guid}/shifts/{shiftId:guid}", GetShiftByIdAsync);
        group.MapGet("/{organizationId:guid}/shifts/{shiftId:guid}/form-templates", ListShiftFormTemplatesAsync);
        group.MapPut("/{organizationId:guid}/shifts/{shiftId:guid}", UpdateShiftAsync);
        group.MapDelete("/{organizationId:guid}/shifts/{shiftId:guid}", DeleteShiftAsync);

        group.MapGet("/{organizationId:guid}/shifts/{shiftId:guid}/breaks", ListShiftBreaksAsync);
        group.MapPost("/{organizationId:guid}/shifts/{shiftId:guid}/breaks", CreateShiftBreakAsync);
        group.MapGet("/{organizationId:guid}/shifts/{shiftId:guid}/breaks/{shiftBreakId:guid}", GetShiftBreakByIdAsync);
        group.MapPut("/{organizationId:guid}/shifts/{shiftId:guid}/breaks/{shiftBreakId:guid}", UpdateShiftBreakAsync);
        group.MapDelete("/{organizationId:guid}/shifts/{shiftId:guid}/breaks/{shiftBreakId:guid}", DeleteShiftBreakAsync);

        group.MapGet("/{organizationId:guid}/tasks", ListTasksAsync);
        group.MapPost("/{organizationId:guid}/tasks", CreateTaskAsync);
        group.MapGet("/{organizationId:guid}/tasks/{taskId:guid}", GetTaskByIdAsync);
        group.MapPut("/{organizationId:guid}/tasks/{taskId:guid}", UpdateTaskAsync);
        group.MapDelete("/{organizationId:guid}/tasks/{taskId:guid}", DeleteTaskAsync);

        group.MapGet("/{organizationId:guid}/tasks/{taskId:guid}/comments", ListTaskCommentsAsync);
        group.MapPost("/{organizationId:guid}/tasks/{taskId:guid}/comments", CreateTaskCommentAsync);
        group.MapGet("/{organizationId:guid}/tasks/{taskId:guid}/comments/{taskCommentId:guid}", GetTaskCommentByIdAsync);
        group.MapPut("/{organizationId:guid}/tasks/{taskId:guid}/comments/{taskCommentId:guid}", UpdateTaskCommentAsync);
        group.MapDelete("/{organizationId:guid}/tasks/{taskId:guid}/comments/{taskCommentId:guid}", DeleteTaskCommentAsync);

        group.MapGet("/{organizationId:guid}/form-templates", ListFormTemplatesAsync);
        group.MapPost("/{organizationId:guid}/form-templates", CreateFormTemplateAsync);
        group.MapGet("/{organizationId:guid}/form-templates/{formTemplateId:guid}", GetFormTemplateByIdAsync);
        group.MapPut("/{organizationId:guid}/form-templates/{formTemplateId:guid}", UpdateFormTemplateAsync);
        group.MapDelete("/{organizationId:guid}/form-templates/{formTemplateId:guid}", DeleteFormTemplateAsync);

        group.MapGet("/{organizationId:guid}/form-templates/{formTemplateId:guid}/fields", ListFormFieldsAsync);
        group.MapPost("/{organizationId:guid}/form-templates/{formTemplateId:guid}/fields", CreateFormFieldAsync);
        group.MapGet("/{organizationId:guid}/form-templates/{formTemplateId:guid}/fields/{formFieldId:guid}", GetFormFieldByIdAsync);
        group.MapPut("/{organizationId:guid}/form-templates/{formTemplateId:guid}/fields/{formFieldId:guid}", UpdateFormFieldAsync);
        group.MapDelete("/{organizationId:guid}/form-templates/{formTemplateId:guid}/fields/{formFieldId:guid}", DeleteFormFieldAsync);

        group.MapGet("/{organizationId:guid}/form-submissions", ListFormSubmissionsAsync);
        group.MapPost("/{organizationId:guid}/form-submissions", CreateFormSubmissionAsync);
        group.MapGet("/{organizationId:guid}/form-submissions/{formSubmissionId:guid}", GetFormSubmissionByIdAsync);
        group.MapPut("/{organizationId:guid}/form-submissions/{formSubmissionId:guid}", UpdateFormSubmissionAsync);
        group.MapDelete("/{organizationId:guid}/form-submissions/{formSubmissionId:guid}", DeleteFormSubmissionAsync);

        group.MapGet("/{organizationId:guid}/form-submissions/{formSubmissionId:guid}/answers", ListFormAnswersAsync);
        group.MapGet("/{organizationId:guid}/form-submissions/{formSubmissionId:guid}/answers/{formAnswerId:guid}", GetFormAnswerByIdAsync);
        group.MapDelete("/{organizationId:guid}/form-submissions/{formSubmissionId:guid}/answers/{formAnswerId:guid}", DeleteFormAnswerAsync);

        group.MapGet("/{organizationId:guid}/files", ListStoredFilesAsync);
        group.MapPost("/{organizationId:guid}/files", CreateStoredFileAsync);
        group.MapGet("/{organizationId:guid}/files/{fileId:guid}", GetStoredFileByIdAsync);
        group.MapPut("/{organizationId:guid}/files/{fileId:guid}", UpdateStoredFileAsync);
        group.MapDelete("/{organizationId:guid}/files/{fileId:guid}", DeleteStoredFileAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrganizationsQuery(), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrganizationByIdQuery(id), cancellationToken);
        if (result is null)
        {
            return Results.NotFound(Result<OrganizationDto>.Failure(
                new AppError("organizations.not_found", "Organization not found.")));
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateAsync(
        CreateOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationCommand(request.Name, request.Slug, request.Timezone);
        var result = await sender.Send(command, cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationCommand(
            id,
            request.Name,
            request.Slug,
            request.Timezone,
            request.Status);

        var result = await sender.Send(command, cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOrganizationCommand(id), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListMembersAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrganizationMembersQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateMemberAsync(
        Guid organizationId,
        CreateOrganizationMemberInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationMemberCommand(
            organizationId,
            request.UserId,
            request.RoleId,
            request.JobTitle);

        var result = await sender.Send(command, cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListRolesAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRolesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateRoleAsync(
        Guid organizationId,
        CreateRoleInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRoleCommand(organizationId, request.Name), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListLocationsAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLocationsQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateLocationAsync(
        Guid organizationId,
        CreateLocationInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateLocationCommand(
                organizationId,
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.GeofenceRadiusMeters,
                request.Timezone),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListLocationWorkingHoursAsync(
        Guid organizationId,
        Guid locationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLocationWorkingHoursQuery(organizationId, locationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> SetLocationWorkingHoursAsync(
        Guid organizationId,
        Guid locationId,
        SetLocationWorkingHoursInLocationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SetLocationWorkingHoursCommand(
            organizationId,
            locationId,
            request.WorkingHours.Select(x => new SetLocationWorkingHourInput(
                x.DayOfWeek,
                x.IsClosed,
                x.OpensAt,
                x.ClosesAt)).ToList());

        var result = await sender.Send(command, cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateLocationWorkingHourAsync(
        Guid organizationId,
        Guid locationId,
        SetLocationWorkingHoursInLocationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SetLocationWorkingHoursCommand(
            organizationId,
            locationId,
            request.WorkingHours.Select(x => new SetLocationWorkingHourInput(
                x.DayOfWeek,
                x.IsClosed,
                x.OpensAt,
                x.ClosesAt)).ToList());

        var result = await sender.Send(command, cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListShiftsAsync(
        Guid organizationId,
        DateTimeOffset? fromDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrganizationShiftsQuery(organizationId, fromDate), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateShiftAsync(
        Guid organizationId,
        CreateShiftInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateShiftCommand(
                organizationId,
                request.TeamId,
                request.OrganizationMemberId,
                request.LocationId,
                request.Title,
                request.StartAt,
                request.EndAt,
                request.Notes,
                request.Breaks?.Select(x =>
                    new CreateShiftBreakInput(x.StartAt, x.EndAt, x.IsPaid)).ToList(),
                request.RequiredFormTemplateIds,
                request.Repeat,
                request.RepeatTimes,
                request.RepeatOn,
                request.DayOfMonth),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetShiftByIdAsync(
        Guid organizationId,
        Guid shiftId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetShiftByIdQuery(shiftId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListShiftFormTemplatesAsync(
        Guid organizationId,
        Guid shiftId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftFormTemplatesQuery(organizationId, shiftId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateShiftAsync(
        Guid organizationId,
        Guid shiftId,
        UpdateShiftInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new UpdateShiftCommand(
                shiftId,
                request.TeamId,
                request.OrganizationMemberId,
                request.LocationId,
                request.Title,
                request.StartAt,
                request.EndAt,
                request.Notes,
                request.Status,
                request.RequiredFormTemplateIds),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteShiftAsync(
        Guid organizationId,
        Guid shiftId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new DeleteShiftCommand(shiftId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListShiftBreaksAsync(
        Guid organizationId,
        Guid shiftId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetShiftBreaksQuery(shiftId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateShiftBreakAsync(
        Guid organizationId,
        Guid shiftId,
        CreateShiftBreakInShiftRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new CreateShiftBreakCommand(shiftId, request.StartAt, request.EndAt, request.IsPaid),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetShiftBreakByIdAsync(
        Guid organizationId,
        Guid shiftId,
        Guid shiftBreakId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = shiftId;

        var result = await sender.Send(new GetShiftBreakByIdQuery(shiftBreakId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateShiftBreakAsync(
        Guid organizationId,
        Guid shiftId,
        Guid shiftBreakId,
        UpdateShiftBreakInShiftRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = shiftId;

        var result = await sender.Send(
            new UpdateShiftBreakCommand(shiftBreakId, request.StartAt, request.EndAt, request.IsPaid, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteShiftBreakAsync(
        Guid organizationId,
        Guid shiftId,
        Guid shiftBreakId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = shiftId;

        var result = await sender.Send(new DeleteShiftBreakCommand(shiftBreakId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTasksAsync(
        Guid organizationId,
        Guid? userId,
        ICurrentUserContext currentUserContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var effectiveUserId = userId ?? currentUserContext.UserId;
        var result = await sender.Send(new GetTasksQuery(OrganizationId: organizationId, UserId: effectiveUserId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTaskAsync(
        Guid organizationId,
        CreateTaskInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTaskCommand(
                organizationId,
                request.ShiftId,
                request.LocationId,
                request.Title,
                request.Description,
                request.AssignedToTeamMemberId,
                request.AssignedToTeamId,
                request.DueAt,
                request.Priority),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTaskByIdAsync(
        Guid organizationId,
        Guid taskId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetTaskByIdQuery(taskId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTaskAsync(
        Guid organizationId,
        Guid taskId,
        UpdateTaskInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new UpdateTaskCommand(
                taskId,
                request.ShiftId,
                request.LocationId,
                request.Title,
                request.Description,
                request.AssignedToTeamMemberId,
                request.AssignedToTeamId,
                request.DueAt,
                request.Priority,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTaskAsync(
        Guid organizationId,
        Guid taskId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new DeleteTaskCommand(taskId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListTaskCommentsAsync(
        Guid organizationId,
        Guid taskId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetTaskCommentsQuery(taskId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateTaskCommentAsync(
        Guid organizationId,
        Guid taskId,
        CreateTaskCommentInTaskRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new CreateTaskCommentCommand(taskId, request.UserId, request.Message),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetTaskCommentByIdAsync(
        Guid organizationId,
        Guid taskId,
        Guid taskCommentId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = taskId;

        var result = await sender.Send(new GetTaskCommentByIdQuery(taskCommentId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateTaskCommentAsync(
        Guid organizationId,
        Guid taskId,
        Guid taskCommentId,
        UpdateTaskCommentInTaskRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = taskId;

        var result = await sender.Send(new UpdateTaskCommentCommand(taskCommentId, request.Message, request.Status), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteTaskCommentAsync(
        Guid organizationId,
        Guid taskId,
        Guid taskCommentId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = taskId;

        var result = await sender.Send(new DeleteTaskCommentCommand(taskCommentId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListFormTemplatesAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormTemplatesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateFormTemplateAsync(
        Guid organizationId,
        CreateFormTemplateInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateFormTemplateCommand(organizationId, request.Name, request.Description),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetFormTemplateByIdAsync(
        Guid organizationId,
        Guid formTemplateId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetFormTemplateByIdQuery(formTemplateId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateFormTemplateAsync(
        Guid organizationId,
        Guid formTemplateId,
        UpdateFormTemplateInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new UpdateFormTemplateCommand(formTemplateId, request.Name, request.Description, request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteFormTemplateAsync(
        Guid organizationId,
        Guid formTemplateId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new DeleteFormTemplateCommand(formTemplateId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListFormFieldsAsync(
        Guid organizationId,
        Guid formTemplateId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetFormFieldsQuery(formTemplateId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateFormFieldAsync(
        Guid organizationId,
        Guid formTemplateId,
        CreateFormFieldInFormTemplateRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new CreateFormFieldCommand(
                formTemplateId,
                request.Type,
                request.Label,
                request.IsRequired,
                request.SortOrder,
                request.OptionsJson),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetFormFieldByIdAsync(
        Guid organizationId,
        Guid formTemplateId,
        Guid formFieldId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = formTemplateId;

        var result = await sender.Send(new GetFormFieldByIdQuery(formFieldId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateFormFieldAsync(
        Guid organizationId,
        Guid formTemplateId,
        Guid formFieldId,
        UpdateFormFieldInFormTemplateRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = formTemplateId;

        var result = await sender.Send(
            new UpdateFormFieldCommand(
                formFieldId,
                request.Type,
                request.Label,
                request.IsRequired,
                request.SortOrder,
                request.OptionsJson,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteFormFieldAsync(
        Guid organizationId,
        Guid formTemplateId,
        Guid formFieldId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = formTemplateId;

        var result = await sender.Send(new DeleteFormFieldCommand(formFieldId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListFormSubmissionsAsync(
        Guid organizationId,
        Guid? formTemplateId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormSubmissionsQuery(organizationId, formTemplateId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateFormSubmissionAsync(
        Guid organizationId,
        CreateFormSubmissionInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateFormSubmissionCommand(
                organizationId,
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

    private static async Task<IResult> GetFormSubmissionByIdAsync(
        Guid organizationId,
        Guid formSubmissionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetFormSubmissionByIdQuery(formSubmissionId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateFormSubmissionAsync(
        Guid organizationId,
        Guid formSubmissionId,
        UpdateFormSubmissionInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new UpdateFormSubmissionCommand(
                formSubmissionId,
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

    private static async Task<IResult> DeleteFormSubmissionAsync(
        Guid organizationId,
        Guid formSubmissionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new DeleteFormSubmissionCommand(formSubmissionId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListFormAnswersAsync(
        Guid organizationId,
        Guid formSubmissionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetFormAnswersQuery(formSubmissionId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetFormAnswerByIdAsync(
        Guid organizationId,
        Guid formSubmissionId,
        Guid formAnswerId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = formSubmissionId;

        var result = await sender.Send(new GetFormAnswerByIdQuery(formAnswerId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteFormAnswerAsync(
        Guid organizationId,
        Guid formSubmissionId,
        Guid formAnswerId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;
        _ = formSubmissionId;

        var result = await sender.Send(new DeleteFormAnswerCommand(formAnswerId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> ListStoredFilesAsync(
        Guid organizationId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoredFilesQuery(organizationId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> CreateStoredFileAsync(
        Guid organizationId,
        CreateStoredFileInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateStoredFileCommand(
                organizationId,
                request.FileName,
                request.ContentType,
                request.StoragePath,
                request.SizeBytes),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetStoredFileByIdAsync(
        Guid organizationId,
        Guid fileId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new GetStoredFileByIdQuery(fileId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> UpdateStoredFileAsync(
        Guid organizationId,
        Guid fileId,
        UpdateStoredFileInOrganizationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(
            new UpdateStoredFileCommand(
                fileId,
                request.FileName,
                request.ContentType,
                request.StoragePath,
                request.SizeBytes,
                request.Status),
            cancellationToken);

        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> DeleteStoredFileAsync(
        Guid organizationId,
        Guid fileId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        _ = organizationId;

        var result = await sender.Send(new DeleteStoredFileCommand(fileId), cancellationToken);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private sealed record CreateOrganizationMemberInOrganizationRequest(Guid UserId, Guid RoleId, string? JobTitle);
    private sealed record CreateRoleInOrganizationRequest(string Name);
    private sealed record CreateLocationInOrganizationRequest(
        string Name,
        string? Address,
        decimal? Latitude,
        decimal? Longitude,
        int GeofenceRadiusMeters,
        string? Timezone);
    private sealed record SetLocationWorkingHoursInLocationRequest(
        IReadOnlyCollection<SetLocationWorkingHourInLocationRequest> WorkingHours);
    private sealed record SetLocationWorkingHourInLocationRequest(
        int DayOfWeek,
        bool IsClosed,
        TimeOnly? OpensAt,
        TimeOnly? ClosesAt);
    private sealed record CreateShiftInOrganizationRequest(
        Guid? TeamId,
        Guid? OrganizationMemberId,
        Guid LocationId,
        string? Title,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        string? Notes,
        IReadOnlyCollection<CreateShiftBreakInShiftRequest>? Breaks,
        IReadOnlyCollection<Guid>? RequiredFormTemplateIds,
        string? Repeat,
        int? RepeatTimes,
        IReadOnlyCollection<int>? RepeatOn,
        int? DayOfMonth);
    private sealed record CreateShiftBreakInShiftRequest(
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        bool IsPaid);
    private sealed record UpdateShiftInOrganizationRequest(
        Guid? TeamId,
        Guid? OrganizationMemberId,
        Guid? LocationId,
        string? Title,
        DateTimeOffset? StartAt,
        DateTimeOffset? EndAt,
        string? Notes,
        string? Status,
        IReadOnlyCollection<Guid>? RequiredFormTemplateIds);
    private sealed record UpdateShiftBreakInShiftRequest(
        DateTimeOffset? StartAt,
        DateTimeOffset? EndAt,
        bool? IsPaid,
        string? Status);
    private sealed record CreateTaskInOrganizationRequest(
        Guid? ShiftId,
        Guid? LocationId,
        string Title,
        string? Description,
        Guid? AssignedToTeamMemberId,
        Guid? AssignedToTeamId,
        DateTimeOffset? DueAt,
        string? Priority);
    private sealed record UpdateTaskInOrganizationRequest(
        Guid? ShiftId,
        Guid? LocationId,
        string? Title,
        string? Description,
        Guid? AssignedToTeamMemberId,
        Guid? AssignedToTeamId,
        DateTimeOffset? DueAt,
        string? Priority,
        string? Status);
    private sealed record CreateTaskCommentInTaskRequest(Guid UserId, string Message);
    private sealed record UpdateTaskCommentInTaskRequest(string? Message, string? Status);
    private sealed record CreateFormTemplateInOrganizationRequest(string Name, string? Description);
    private sealed record UpdateFormTemplateInOrganizationRequest(string? Name, string? Description, string? Status);
    private sealed record CreateFormFieldInFormTemplateRequest(
        string Type,
        string Label,
        bool IsRequired,
        int SortOrder,
        string? OptionsJson);
    private sealed record UpdateFormFieldInFormTemplateRequest(
        string? Type,
        string? Label,
        bool? IsRequired,
        int? SortOrder,
        string? OptionsJson,
        string? Status);
    private sealed record CreateFormSubmissionInOrganizationRequest(
        Guid FormTemplateId,
        Guid SubmittedByMemberId,
        Guid? TaskId,
        Guid? ShiftId,
        DateTimeOffset SubmittedAt,
        IReadOnlyCollection<CreateFormSubmissionAnswerInOrganizationRequest>? Answers);
    private sealed record CreateFormSubmissionAnswerInOrganizationRequest(
        Guid FormFieldId,
        string? Value,
        Guid? FileId);
    private sealed record UpdateFormSubmissionInOrganizationRequest(
        Guid? TaskId,
        Guid? ShiftId,
        DateTimeOffset? SubmittedAt,
        string? Status,
        IReadOnlyCollection<UpdateFormSubmissionAnswerInOrganizationRequest>? Answers);
    private sealed record UpdateFormSubmissionAnswerInOrganizationRequest(
        Guid FormFieldId,
        string? Value,
        Guid? FileId,
        string? Status);
    private sealed record CreateStoredFileInOrganizationRequest(
        string FileName,
        string ContentType,
        string StoragePath,
        long SizeBytes);
    private sealed record UpdateStoredFileInOrganizationRequest(
        string? FileName,
        string? ContentType,
        string? StoragePath,
        long? SizeBytes,
        string? Status);
}
