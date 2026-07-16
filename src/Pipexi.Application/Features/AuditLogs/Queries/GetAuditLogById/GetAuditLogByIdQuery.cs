using System.Net;
using MediatR;
using Workforce.Application.Abstractions.Persistence;
using Workforce.Application.Common.Models;
using Workforce.Application.Features.AuditLogs.Dtos;
using Workforce.Shared.Errors;
using Workforce.Shared.Results;

namespace Workforce.Application.Features.AuditLogs.Queries.GetAuditLogById;

public sealed record GetAuditLogByIdQuery(Guid Id) : IQuery<Result<AuditLogDto>>
{
    public sealed class Handler : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public Handler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
        {
            var auditLog = await _auditLogRepository.GetByIdAsync(request.Id, cancellationToken);
            if (auditLog is null)
            {
                return Result<AuditLogDto>.Failure(
                    new AppError("audit_logs.not_found", "Audit log not found."),
                    (int)HttpStatusCode.NotFound);
            }

            return Result<AuditLogDto>.Success(auditLog.ToDto());
        }
    }
}
