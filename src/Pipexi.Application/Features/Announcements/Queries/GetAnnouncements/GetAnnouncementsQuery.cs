using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Announcements.Dtos;
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

        public Handler(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        public async Task<Result<IReadOnlyCollection<AnnouncementDto>>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
        {
            var announcements = request.OrganizationId.HasValue
                ? await _announcementRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _announcementRepository.GetAllAsync(cancellationToken);

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
