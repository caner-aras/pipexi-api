using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Announcements.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Announcements.Queries.GetMyAnnouncements;

public sealed record GetMyAnnouncementsQuery(Guid? OrganizationId = null)
    : IQuery<Result<IReadOnlyCollection<AnnouncementDto>>>
{
    public sealed class Handler : IRequestHandler<GetMyAnnouncementsQuery, Result<IReadOnlyCollection<AnnouncementDto>>>
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            IAnnouncementRepository announcementRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            ITeamMemberRepository teamMemberRepository,
            ITeamRepository teamRepository,
            ICurrentUserContext currentUserContext)
        {
            _announcementRepository = announcementRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _teamMemberRepository = teamMemberRepository;
            _teamRepository = teamRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<AnnouncementDto>>> Handle(
            GetMyAnnouncementsQuery request,
            CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<AnnouncementDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            if (_currentUserContext.UserId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<AnnouncementDto>>.Failure(
                    new AppError("auth.unauthorized", "User is required."),
                    (int)HttpStatusCode.Unauthorized);
            }

            var member = await _organizationMemberRepository.GetByOrganizationIdAndUserIdAsync(
                organizationId,
                _currentUserContext.UserId,
                cancellationToken);

            if (member is null)
            {
                return Result<IReadOnlyCollection<AnnouncementDto>>.Failure(
                    new AppError("announcements.member_not_found", "Organization membership not found."),
                    (int)HttpStatusCode.Forbidden);
            }

            var now = DateTimeOffset.UtcNow;
            var oldestAllowed = now.AddDays(-3);
            var announcements = await _announcementRepository.ListByOrganizationIdAsync(
                organizationId,
                cancellationToken);

            var teamMembers = await _teamMemberRepository.ListByOrganizationMemberIdAsync(
                member.Id,
                cancellationToken);
            var teamIds = teamMembers
                .Where(x => string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.TeamId)
                .Distinct()
                .ToHashSet();

            var teams = await _teamRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            var memberTeams = teams
                .Where(x => teamIds.Contains(x.Id))
                .ToList();
            var locationIds = memberTeams
                .Where(x => x.LocationId.HasValue)
                .Select(x => x.LocationId!.Value)
                .ToHashSet();

            var visible = announcements
                .Where(x => string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.PublishedAt.HasValue || x.PublishedAt.Value <= now)
                .Where(x => (x.PublishedAt ?? x.CreatedAt) >= oldestAllowed)
                .Where(x => IsVisibleToMember(
                    x.AudienceType,
                    x.AudienceId,
                    member.Id,
                    member.RoleId,
                    teamIds,
                    locationIds))
                .Select(x => x.ToDto())
                .ToList();

            return Result<IReadOnlyCollection<AnnouncementDto>>.Success(visible);
        }

        private static bool IsVisibleToMember(
            string audienceType,
            Guid? audienceId,
            Guid organizationMemberId,
            Guid roleId,
            HashSet<Guid> teamIds,
            HashSet<Guid> locationIds)
        {
            var normalized = AnnouncementAudience.Normalize(audienceType);

            return normalized switch
            {
                AnnouncementAudience.All => true,
                AnnouncementAudience.Location =>
                    audienceId.HasValue && locationIds.Contains(audienceId.Value),
                AnnouncementAudience.Role =>
                    audienceId.HasValue && audienceId.Value == roleId,
                AnnouncementAudience.Member =>
                    audienceId.HasValue && audienceId.Value == organizationMemberId,
                AnnouncementAudience.Team =>
                    audienceId.HasValue && teamIds.Contains(audienceId.Value),
                _ => false
            };
        }
    }
}
