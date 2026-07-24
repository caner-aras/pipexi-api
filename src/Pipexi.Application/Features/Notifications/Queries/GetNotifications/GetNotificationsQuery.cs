using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.Notifications.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid? OrganizationId,
    Guid? OrganizationMemberId = null,
    bool? IsRead = null) : IQuery<Result<IReadOnlyCollection<NotificationDto>>>
{
    public sealed class Handler : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyCollection<NotificationDto>>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(
            INotificationRepository notificationRepository,
            ICurrentUserContext currentUserContext)
        {
            _notificationRepository = notificationRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<NotificationDto>>.Failure(
                    new AppError("auth.unauthorized", "Unauthorized."),
                    (int)HttpStatusCode.Unauthorized);
            }

            IReadOnlyCollection<Domain.Entities.Notification> notifications;
            if (request.OrganizationMemberId.HasValue)
            {
                notifications = await _notificationRepository.ListByOrganizationMemberIdAsync(request.OrganizationMemberId.Value, cancellationToken);
                notifications = notifications.Where(x => x.OrganizationId == organizationId).ToList();
            }
            else
            {
                notifications = await _notificationRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);
            }

            if (request.IsRead.HasValue)
            {
                notifications = notifications.Where(x => x.IsRead == request.IsRead.Value).ToList();
            }

            var dtos = notifications.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<NotificationDto>>.Success(dtos);
        }
    }
}
