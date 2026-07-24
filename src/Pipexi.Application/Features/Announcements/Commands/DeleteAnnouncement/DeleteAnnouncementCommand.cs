using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Announcements.Commands.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(Guid Id) : ICommand<Result<object?>>
{
    public sealed class Handler : IRequestHandler<DeleteAnnouncementCommand, Result<object?>>
    {
        private readonly IAnnouncementRepository _announcementRepository;

        public Handler(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        public async Task<Result<object?>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var announcement = await _announcementRepository.GetByIdAsync(request.Id, cancellationToken);
            if (announcement is null)
            {
                return Result<object?>.Failure(
                    new AppError("announcements.not_found", "Announcement not found."),
                    (int)HttpStatusCode.NotFound);
            }

            await _announcementRepository.DeleteAsync(announcement, cancellationToken);
            return Result<object?>.Success(null, (int)HttpStatusCode.OK);
        }
    }
}
