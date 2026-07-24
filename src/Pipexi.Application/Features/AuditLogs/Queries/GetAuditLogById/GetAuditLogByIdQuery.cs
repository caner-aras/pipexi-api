using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common.Models;
using Pipexi.Application.Features.AuditLogs.Dtos;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;

namespace Pipexi.Application.Features.AuditLogs.Queries.GetAuditLogById;

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
