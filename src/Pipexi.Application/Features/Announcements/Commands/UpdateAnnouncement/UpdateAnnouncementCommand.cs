using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Announcements.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Announcements.Commands.UpdateAnnouncement;

public sealed record UpdateAnnouncementCommand(
    Guid Id,
    string? Title,
    string? Body,
    string? AudienceType,
    Guid? AudienceId,
    DateTimeOffset? PublishedAt,
    string? Status) : ICommand<Result<AnnouncementDto>>
{
    public sealed class Handler : IRequestHandler<UpdateAnnouncementCommand, Result<AnnouncementDto>>
    {
        private readonly IAnnouncementRepository _announcementRepository;

        public Handler(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        public async Task<Result<AnnouncementDto>> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var announcement = await _announcementRepository.GetByIdAsync(request.Id, cancellationToken);
            if (announcement is null)
            {
                return Result<AnnouncementDto>.Failure(
                    new AppError("announcements.not_found", "Announcement not found."),
                    (int)HttpStatusCode.NotFound);
            }

            announcement.UpdateDetails(
                request.Title,
                request.Body,
                request.AudienceType,
                request.AudienceId,
                request.PublishedAt,
                request.Status);

            await _announcementRepository.UpdateAsync(announcement, cancellationToken);
            return Result<AnnouncementDto>.Success(announcement.ToDto());
        }
    }
}
