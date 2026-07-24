using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.AuditLogs.Dtos;
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

        public Handler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<Result<IReadOnlyCollection<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var auditLogs = request.OrganizationId.HasValue
                ? await _auditLogRepository.ListByOrganizationIdAsync(request.OrganizationId.Value, cancellationToken)
                : await _auditLogRepository.GetAllAsync(cancellationToken);

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
