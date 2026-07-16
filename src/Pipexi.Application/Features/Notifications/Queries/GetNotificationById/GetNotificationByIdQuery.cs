using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Notifications.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Notifications.Queries.GetNotificationById;

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
