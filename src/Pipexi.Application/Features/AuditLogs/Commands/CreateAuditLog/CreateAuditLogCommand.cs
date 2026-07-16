using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.AuditLogs.Dtos;
using Workforce.Domain.Entities;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.AuditLogs.Commands.CreateAuditLog;

public sealed record CreateAuditLogCommand(
    Guid OrganizationId,
    Guid? ActorMemberId,
    string EntityName,
    Guid EntityId,
    string Action,
    string? BeforeJson,
    string? AfterJson,
    DateTimeOffset? CreatedAt) : ICommand<Result<AuditLogDto>>
{
    public sealed class Handler : IRequestHandler<CreateAuditLogCommand, Result<AuditLogDto>>
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationMemberRepository _organizationMemberRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public Handler(
            IOrganizationRepository organizationRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IAuditLogRepository auditLogRepository)
        {
            _organizationRepository = organizationRepository;
            _organizationMemberRepository = organizationMemberRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<Result<AuditLogDto>> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
            {
                return Result<AuditLogDto>.Failure(
                    new AppError("audit_logs.invalid_organization", "Organization not found."),
                    (int)HttpStatusCode.BadRequest);
            }

            if (request.ActorMemberId.HasValue)
            {
                var member = await _organizationMemberRepository.GetByIdAsync(request.ActorMemberId.Value, cancellationToken);
                if (member is null || member.OrganizationId != request.OrganizationId)
                {
                    return Result<AuditLogDto>.Failure(
                        new AppError("audit_logs.invalid_actor", "Actor member not found for organization."),
                        (int)HttpStatusCode.BadRequest);
                }
            }

            var auditLog = AuditLog.Create(
                request.OrganizationId,
                request.ActorMemberId,
                request.EntityName,
                request.EntityId,
                request.Action,
                request.BeforeJson,
                request.AfterJson,
                request.CreatedAt);

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            return Result<AuditLogDto>.Success(auditLog.ToDto(), (int)HttpStatusCode.Created);
        }
    }
}
