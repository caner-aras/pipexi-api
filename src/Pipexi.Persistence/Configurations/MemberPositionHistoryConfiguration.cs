using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Configurations;

public sealed class MemberPositionHistoryConfiguration : IEntityTypeConfiguration<MemberPositionHistory>
{
    public void Configure(EntityTypeBuilder<MemberPositionHistory> builder)
    {
        builder.ToTable("member_position_histories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationMemberId)
            .HasColumnName("organization_member_id")
            .IsRequired();

        builder.Property(x => x.PositionId)
            .HasColumnName("position_id")
            .IsRequired();

        builder.Property(x => x.HourlyRate)
            .HasColumnName("hourly_rate")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasDefaultValue("active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.OrganizationMemberId, x.EndDate });

        builder.HasOne<OrganizationMember>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Position>()
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
