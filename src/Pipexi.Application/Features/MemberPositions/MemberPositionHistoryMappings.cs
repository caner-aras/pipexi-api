using Pipexi.Application.Features.MemberPositions.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.MemberPositions;

public static class MemberPositionHistoryMappings
{
    public static MemberPositionHistoryDto ToDto(this MemberPositionHistory entity)
    {
        return new MemberPositionHistoryDto(
            entity.Id,
            entity.OrganizationMemberId,
            entity.PositionId,
            entity.HourlyRate,
            entity.StartDate,
            entity.EndDate,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
