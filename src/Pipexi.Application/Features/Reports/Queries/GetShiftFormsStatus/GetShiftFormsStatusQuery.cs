using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Reports.Dtos;
using Pipexi.Shared.Results;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Reports.Queries.GetShiftFormsStatus;

public sealed record GetShiftFormsStatusQuery(Guid OrganizationId, int TrendDays = 30, int FutureDays = 7)
    : IQuery<Result<IReadOnlyCollection<ShiftFormsStatusDto>>>;

public sealed class Handler : IRequestHandler<GetShiftFormsStatusQuery, Result<IReadOnlyCollection<ShiftFormsStatusDto>>>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUserRepository _userRepository;
    private readonly IShiftRequiredFormTemplateRepository _shiftRequiredFormTemplateRepository;
    private readonly IFormSubmissionRepository _formSubmissionRepository;

    public Handler(
        IShiftRepository shiftRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        ITeamRepository teamRepository,
        IUserRepository userRepository,
        IShiftRequiredFormTemplateRepository shiftRequiredFormTemplateRepository,
        IFormSubmissionRepository formSubmissionRepository)
    {
        _shiftRepository = shiftRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _teamRepository = teamRepository;
        _userRepository = userRepository;
        _shiftRequiredFormTemplateRepository = shiftRequiredFormTemplateRepository;
        _formSubmissionRepository = formSubmissionRepository;
    }

    public async Task<Result<IReadOnlyCollection<ShiftFormsStatusDto>>> Handle(GetShiftFormsStatusQuery request, CancellationToken cancellationToken)
    {
        var shifts = await _shiftRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        
        var today = DateTimeOffset.UtcNow.Date;
        var trendStart = today.AddDays(-request.TrendDays);
        var futureEnd = today.AddDays(request.FutureDays);
        
        var relevantShifts = shifts.Where(x => x.StartAt < futureEnd && x.EndAt > trendStart).ToList();
        
        if (relevantShifts.Count == 0)
        {
            return Result<IReadOnlyCollection<ShiftFormsStatusDto>>.Success(Array.Empty<ShiftFormsStatusDto>());
        }

        var shiftIds = relevantShifts.Select(x => x.Id).ToList();
        
        var requiredByShift = await _shiftRequiredFormTemplateRepository
            .ListRequiredTemplateIdsByShiftIdsAsync(shiftIds, cancellationToken);
            
        var submittedByShift = await _formSubmissionRepository
            .ListSubmittedTemplateIdsByShiftIdsAsync(shiftIds, cancellationToken);

        var result = new List<ShiftFormsStatusDto>();
        
        if (requiredByShift.Count == 0)
        {
            return Result<IReadOnlyCollection<ShiftFormsStatusDto>>.Success(result);
        }

        var members = await _organizationMemberRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var teams = await _teamRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        
        var userIds = members.Select(x => x.UserId).Distinct().ToList();
        var users = await _userRepository.ListByIdsAsync(userIds, cancellationToken);
        
        var userNameById = users.ToDictionary(
            x => x.Id,
            x => string.IsNullOrWhiteSpace($"{x.FirstName} {x.LastName}")
                ? x.Email
                : $"{x.FirstName} {x.LastName}".Trim());

        var userAvatarById = users.ToDictionary(
            x => x.Id,
            x => AvatarUrls.Resolve(x.Id, x.AvatarUrl));

        foreach (var shift in relevantShifts)
        {
            if (!requiredByShift.TryGetValue(shift.Id, out var requiredTemplateIds) || requiredTemplateIds.Count == 0)
            {
                continue;
            }

            submittedByShift.TryGetValue(shift.Id, out var submittedTemplateIds);
            submittedTemplateIds ??= Array.Empty<Guid>();

            bool isMissing = requiredTemplateIds.Except(submittedTemplateIds).Any();

            var member = members.FirstOrDefault(x => x.Id == shift.OrganizationMemberId);
            var team = teams.FirstOrDefault(x => x.Id == shift.TeamId);
            
            var memberName = member != null && userNameById.TryGetValue(member.UserId, out var name) ? name : "Unknown";
            var avatarUrl = member != null && userAvatarById.TryGetValue(member.UserId, out var avatar) ? avatar : null;
            var teamName = team?.Name ?? "Unknown Team";

            result.Add(new ShiftFormsStatusDto(
                shift.Id,
                shift.OrganizationMemberId,
                memberName,
                avatarUrl,
                teamName,
                shift.StartAt,
                shift.EndAt,
                isMissing
            ));
        }

        // Sort by StartAt descending (most recent first)
        result.Sort((a, b) => b.StartAt.CompareTo(a.StartAt));

        return Result<IReadOnlyCollection<ShiftFormsStatusDto>>.Success(result);
    }
}
