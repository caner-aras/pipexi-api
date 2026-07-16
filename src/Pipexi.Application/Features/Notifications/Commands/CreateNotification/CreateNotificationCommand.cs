using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.Notifications.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.Notifications.Commands.CreateNotification;

public sealed record CreateNotificationCommand(
    Guid OrganizationId,
    Guid OrganizationMemberId,
    string Type,
    string Title,
    string Body,
    bool IsRead,
    DateTimeOffset? ScheduledTime) : ICommand<Result<NotificationDto>>
{
    public sealed class Handler : IRequestHandler<CreateNotificationCommand, Result<NotificationDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly INotificationRepository _notificationRepository;

        public Handler(
            IOrganizationRepository organizationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            INotificationRepository notificationRepository)
        {
            _organizationRepository = organizationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<Result<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<NotificationDto>.Failure(
                    new AppError("notifications.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            var member = await _organizationMemberRepository.GetByIdAsync(request.OrganizationMemberId, cancellationToken);
            if (member is null || member.OrganizationId != request.OrganizationId)
            {
                return Result<NotificationDto>.Failure(
                    new AppError("notifications.invalid_member", "Organization member not found for organization."),
                    (int)HttpStatusCode.BadRequest);
            }

            var notification = Notification.Create(
                request.OrganizationId,
                request.OrganizationMemberId,
                request.Type,
                request.Title,
                request.Body,
                request.IsRead,
                request.ScheduledTime);

            await _notificationRepository.AddAsync(notification, cancellationToken);
            return Result<NotificationDto>.Success(notification.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
