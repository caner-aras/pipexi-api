using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Announcements.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed record GetAnnouncementsQuery(
    Guid? OrganizationId,
    string? AudienceType = null,
    Guid? AudienceId = null) : IQuery<Result<IReadOnlyCollection<AnnouncementDto>>>
{
    public sealed class Handler : IRequestHandler<GetAnnouncementsQuery, Result<IReadOnlyCollection<AnnouncementDto>>>
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(IAnnouncementRepository announcementRepository, ICurrentUserContext currentUserContext)
        {
            _announcementRepository = announcementRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<AnnouncementDto>>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<AnnouncementDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var announcements = await _announcementRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.AudienceType))
            {
                announcements = announcements
                    .Where(x => x.AudienceType.Equals(request.AudienceType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (request.AudienceId.HasValue)
            {
                announcements = announcements
                    .Where(x => x.AudienceId == request.AudienceId)
                    .ToList();
            }

            var dtos = announcements.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<AnnouncementDto>>.Success(dtos);
        }
    }
}
