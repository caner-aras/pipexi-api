using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Identity;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.AuditLogs.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    Guid? OrganizationId,
    string? EntityName = null,
    Guid? EntityId = null) : IQuery<Result<IReadOnlyCollection<AuditLogDto>>>
{
    public sealed class Handler : IRequestHandler<GetAuditLogsQuery, Result<IReadOnlyCollection<AuditLogDto>>>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ICurrentUserContext _currentUserContext;

        public Handler(IAuditLogRepository auditLogRepository, ICurrentUserContext currentUserContext)
        {
            _auditLogRepository = auditLogRepository;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<IReadOnlyCollection<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var organizationId = request.OrganizationId ?? _currentUserContext.OrganizationId;
            if (organizationId == Guid.Empty)
            {
                return Result<IReadOnlyCollection<AuditLogDto>>.Failure(
                    new AppError("auth.organization_required", "Organization is required."),
                    (int)HttpStatusCode.Forbidden);
            }

            var auditLogs = await _auditLogRepository.ListByOrganizationIdAsync(organizationId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.EntityName))
            {
                auditLogs = auditLogs
                    .Where(x => x.EntityName.Equals(request.EntityName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (request.EntityId.HasValue)
            {
                auditLogs = auditLogs
                    .Where(x => x.EntityId == request.EntityId.Value)
                    .ToList();
            }

            var dtos = auditLogs.Select(x => x.ToDto()).ToList();
            return Result<IReadOnlyCollection<AuditLogDto>>.Success(dtos);
        }
    }
}
