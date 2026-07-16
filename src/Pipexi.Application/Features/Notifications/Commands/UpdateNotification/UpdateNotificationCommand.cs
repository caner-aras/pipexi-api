using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Notifications.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Notifications.Commands.UpdateNotification;

public sealed record UpdateNotificationCommand(
    Guid Id,
    string? Type,
    string? Title,
    string? Body,
    bool? IsRead,
    DateTimeOffset? ScheduledTime,
    string? Status) : ICommand<Result<NotificationDto>>
{
    public sealed class Handler : IRequestHandler<UpdateNotificationCommand, Result<NotificationDto>>
    {
        private readonly INotificationRepository _notificationRepository;

        public Handler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Result<NotificationDto>> Handle(UpdateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (notification is null)
            {
                return Result<NotificationDto>.Failure(
                    new AppError("notifications.not_found", "Notification not found."),
                    (int)HttpStatusCode.NotFound);
            }

            notification.UpdateDetails(request.Type, request.Title, request.Body, request.IsRead, request.ScheduledTime, request.Status);
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
            return Result<NotificationDto>.Success(notification.ToDto());
        }
    }
}
