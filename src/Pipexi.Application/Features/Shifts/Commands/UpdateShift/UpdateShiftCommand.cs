using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Locations;
using Pipexi.Application.Features.OrganizationMembers;
using Pipexi.Application.Features.Shifts;
using Pipexi.Application.Features.Shifts.Dtos;
using Pipexi.Application.Features.Teams;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Shifts.Commands.UpdateShift;

public sealed record UpdateShiftCommand(
    Guid Id,
    Guid? TeamId,
    Guid? OrganizationMemberId,
    Guid? LocationId,
    string? Title,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    string? Notes,
    string? Status,
    IReadOnlyCollection<Guid>? RequiredFormTemplateIds) : ICommand<Result<ShiftDto>>
{
    public sealed class Handler : IRequestHandler<UpdateShiftCommand, Result<ShiftDto>>
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;

        public Handler(
            IShiftRepository shiftRepository,
            ITeamRepository teamRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IUserRepository userRepository,
            ILocationRepository locationRepository,
            IFormTemplateRepository formTemplateRepository,
            IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
            ITeamMemberRepository teamMemberRepository)
        {
            _shiftRepository = shiftRepository;
            _teamRepository = teamRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _userRepository = userRepository;
            _locationRepository = locationRepository;
            _formTemplateRepository = formTemplateRepository;
            _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        public async Task<Result<ShiftDto>> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
        {
            var shift = await _shiftRepository.GetByIdAsync(request.Id, cancellationToken);
            if (shift is null)
            {
                return Result<ShiftDto>.Failure(
                    new AppError("shifts.not_found", "Shift not found."),
                    (int)HttpStatusCode.NotFound);
            }

            var candidateStart = request.StartAt ?? shift.StartAt;
            var candidateEnd = request.EndAt ?? shift.EndAt;
            if (candidateStart >= candidateEnd)
            {
                return Result<ShiftDto>.Failure(
                    new AppError("shifts.invalid_range", "Shift end time must be after start time."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (request.LocationId.HasValue)
            {
                var location = await _locationRepository.GetByIdAsync(request.LocationId.Value, cancellationToken);
                if (location is null || location.OrganizationId != shift.OrganizationId)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shifts.invalid_location", "Location not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.TeamId.HasValue)
            {
                var team = await _teamRepository.GetByIdAsync(request.TeamId.Value, cancellationToken);
                if (team is null || team.OrganizationId != shift.OrganizationId)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shifts.invalid_team", "Team not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            if (request.OrganizationMemberId.HasValue)
            {
                var member = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId.Value, cancellationToken);
                if (member is null || member.OrganizationId != shift.OrganizationId)
                {
                    return Result<ShiftDto>.Failure(
                        new AppError("shifts.invalid_organization_member", "Organization member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            List<Guid>? requiredFormTemplateIds = null;
            if (request.RequiredFormTemplateIds is not null)
            {
                requiredFormTemplateIds = request.RequiredFormTemplateIds
                    .Distinct()
                    .ToList();

                if (requiredFormTemplateIds.Count > 0)
                {
                    var templates = await _formTemplateRepository.GetByIdsAsync(requiredFormTemplateIds, cancellationToken);
                    if (templates.Count != requiredFormTemplateIds.Count || templates.Any(x => x.OrganizationId != shift.OrganizationId))
                    {
                        return Result<ShiftDto>.Failure(
                            new AppError("shifts.invalid_required_form_templates", "One or more required form templates are invalid for organization."),
                            (int)HttpStatusCode.BadRequest);
                    }
                }
            }

            shift.UpdateDetails(
                request.TeamId,
                request.OrganizationMemberId,
                request.LocationId,
                request.Title,
                request.StartAt,
                request.EndAt,
                request.Notes,
                request.Status);

            await _shiftRepository.UpdateAsync(shift, cancellationToken);

            if (requiredFormTemplateIds is not null)
            {
                var existingRequiredTemplates = await _shiftRequiredFormTemplateRepository.ListByShiftIdAsync(shift.Id, cancellationToken);
                if (existingRequiredTemplates.Count > 0)
                {
                    await _shiftRequiredFormTemplateRepository.DeleteRangeAsync(existingRequiredTemplates, cancellationToken);
                }

                if (requiredFormTemplateIds.Count > 0)
                {
                    var replacements = requiredFormTemplateIds
                        .Select(x => ShiftRequiredFormTemplate.Create(shift.Id, x))
                        .ToList();

                    await _shiftRequiredFormTemplateRepository.AddRangeAsync(replacements, cancellationToken);
                }
            }

            var resolvedTeam = shift.TeamId.HasValue
                ? await _teamRepository.GetByIdAsync(shift.TeamId.Value, cancellationToken)
                : null;
            var resolvedOrganizationMember = shift.OrganizationMemberId.HasValue
                ? await _organizationMemberRepository.GetByIdAsync(shift.OrganizationMemberId.Value, cancellationToken)
                : null;
            var resolvedUser = resolvedOrganizationMember is null
                ? null
                : await _userRepository.GetByIdAsync(resolvedOrganizationMember.UserId, cancellationToken);
            var resolvedLocation = await _locationRepository.GetByIdAsync(shift.LocationId, cancellationToken);

            var teamMemberLookup = await ShiftTeamMemberLookup.CreateAsync(
                _teamMemberRepository,
                new[] { shift },
                cancellationToken);

            return Result<ShiftDto>.Success(
                shift.ToDto(
                    resolvedTeam?.ToDto(),
                    resolvedOrganizationMember?.ToDto(resolvedUser?.ToDto()),
                    resolvedLocation?.ToDto(),
                    teamMemberId: ShiftTeamMemberLookup.Resolve(
                        shift.TeamId,
                        shift.OrganizationMemberId,
                        teamMemberLookup)),
                (int)HttpStatusCode.OK);
        }
    }
}
