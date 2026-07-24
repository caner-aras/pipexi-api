using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Announcements.Dtos;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(
    Guid OrganizationId,
    string Title,
    string Body,
    string AudienceType,
    Guid? AudienceId,
    DateTimeOffset? PublishedAt) : ICommand<Result<AnnouncementDto>>
{
    public sealed class Handler : IRequestHandler<CreateAnnouncementCommand, Result<AnnouncementDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IAnnouncementRepository _announcementRepository;

        public Handler(IOrganizationRepository organizationRepository, IAnnouncementRepository announcementRepository)
        {
            _organizationRepository = organizationRepository;
            _announcementRepository = announcementRepository;
        }

        public async Task<Result<AnnouncementDto>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<AnnouncementDto>.Failure(
                    new AppError("announcements.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var announcement = Announcement.Create(
                request.OrganizationId,
                request.Title,
                request.Body,
                request.AudienceType,
                request.AudienceId,
                request.PublishedAt);

            await _announcementRepository.AddAsync(announcement, cancellationToken);
            return Result<AnnouncementDto>.Success(announcement.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
