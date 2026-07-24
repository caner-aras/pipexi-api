using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Announcements.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Announcements.Queries.GetAnnouncementById;

public sealed record GetAnnouncementByIdQuery(Guid Id) : IQuery<Result<AnnouncementDto>>
{
    public sealed class Handler : IRequestHandler<GetAnnouncementByIdQuery, Result<AnnouncementDto>>
    {
        private readonly IAnnouncementRepository _announcementRepository;

        public Handler(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        public async Task<Result<AnnouncementDto>> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
        {
            var announcement = await _announcementRepository.GetByIdAsync(request.Id, cancellationToken);
            if (announcement is null)
            {
                return Result<AnnouncementDto>.Failure(
                    new AppError("announcements.not_found", "Announcement not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<AnnouncementDto>.Success(announcement.ToDto());
        }
    }
}
