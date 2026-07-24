using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Notifications.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Notifications.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid Id) : IQuery<Result<NotificationDto>>
{
    public sealed class Handler : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
    {
        private readonly INotificationRepository _notificationRepository;

        public Handler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Result<NotificationDto>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (notification is null)
            {
                return Result<NotificationDto>.Failure(
                    new AppError("notifications.not_found", "Notification not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<NotificationDto>.Success(notification.ToDto());
        }
    }
}
