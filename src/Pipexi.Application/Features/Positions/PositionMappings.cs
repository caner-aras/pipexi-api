using Pipexi.Application.Features.Positions.Dtos;
using Pipexi.Domain.Entities;

namespace Pipexi.Application.Features.Positions;

public static class PositionMappings
{
    public static PositionDto ToDto(this Position entity)
    {
        return new PositionDto(
            entity.Id,
            entity.OrganizationId,
            entity.Title,
            entity.Description,
            entity.DefaultHourlyRate,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
