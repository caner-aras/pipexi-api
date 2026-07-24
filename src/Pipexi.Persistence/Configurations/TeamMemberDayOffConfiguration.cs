using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Configurations;

public sealed class TeamMemberDayOffConfiguration : IEntityTypeConfiguration<TeamMemberDayOff>
{
    public void Configure(EntityTypeBuilder<TeamMemberDayOff> builder)
    {
        builder.ToTable("team_member_day_offs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.TeamMemberId)
            .HasColumnName("team_member_id")
            .IsRequired();

        builder.Property(x => x.StartAt)
            .HasColumnName("start_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.EndAt)
            .HasColumnName("end_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);

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

        builder.HasIndex(x => new { x.TeamMemberId, x.StartAt, x.EndAt });

        builder.HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(x => x.TeamMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}