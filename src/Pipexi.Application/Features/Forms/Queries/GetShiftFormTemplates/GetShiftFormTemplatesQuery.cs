using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Forms.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Forms.Queries.GetShiftFormTemplates;

public sealed record GetShiftFormTemplatesQuery(Guid OrganizationId, Guid ShiftId)
    : IQuery<Result<IReadOnlyCollection<ShiftFormTemplateDto>>>;

public sealed class Handler : IRequestHandler<GetShiftFormTemplatesQuery, Result<IReadOnlyCollection<ShiftFormTemplateDto>>>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
    private readonly IFormTemplateRepository _formTemplateRepository;
    private readonly IFormSubmissionRepository _formSubmissionRepository;

    public Handler(
        IShiftRepository shiftRepository,
        IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
        IFormTemplateRepository formTemplateRepository,
        IFormSubmissionRepository formSubmissionRepository)
    {
        _shiftRepository = shiftRepository;
        _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
        _formTemplateRepository = formTemplateRepository;
        _formSubmissionRepository = formSubmissionRepository;
    }

    public async Task<Result<IReadOnlyCollection<ShiftFormTemplateDto>>> Handle(
        GetShiftFormTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var shift = await _shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift is null || shift.OrganizationId != request.OrganizationId)
        {
            return Result<IReadOnlyCollection<ShiftFormTemplateDto>>.Failure(
                new AppError("shifts.not_found", "Shift not found for organization."),
                (int)HttpStatusCode.NotFound);
        }

        var requiredTemplateIds = await _shiftRequiredFormTemplateRepository.ListRequiredTemplateIdsByShiftIdAsync(
            request.ShiftId,
            cancellationToken);

        if (requiredTemplateIds.Count == 0)
        {
            return Result<IReadOnlyCollection<ShiftFormTemplateDto>>.Success(Array.Empty<ShiftFormTemplateDto>());
        }

        var templates = await _formTemplateRepository.GetByIdsAsync(requiredTemplateIds, cancellationToken);
        var submittedTemplateIds = await _formSubmissionRepository.ListSubmittedTemplateIdsByShiftAsync(
            request.ShiftId,
            cancellationToken);
        var submittedTemplateIdSet = submittedTemplateIds.ToHashSet();

        var dtos = templates
            .OrderBy(x => x.Name)
            .Select(x => new ShiftFormTemplateDto(
                x.Id,
                x.OrganizationId,
                x.Name,
                x.Description,
                x.Status,
                x.CreatedAt,
                x.UpdatedAt,
                submittedTemplateIdSet.Contains(x.Id)))
            .ToList();

        return Result<IReadOnlyCollection<ShiftFormTemplateDto>>.Success(dtos);
    }
}