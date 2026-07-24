using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Configurations;

public sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(x => x.ReporterUserId)
            .HasColumnName("reporter_user_id");

        builder.Property(x => x.ShiftId)
            .HasColumnName("shift_id");

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id");

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(x => x.AssignedToTeamMemberId)
            .HasColumnName("assigned_to_team_member_id");

        builder.Property(x => x.AssignedToTeamId)
            .HasColumnName("assigned_to_team_id");

        builder.Property(x => x.DueAt)
            .HasColumnName("due_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasDefaultValue("medium")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasDefaultValue("open")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.OrganizationId, x.DueAt });
        builder.HasIndex(x => x.ReporterUserId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ReporterUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Shift>()
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(x => x.AssignedToTeamMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(x => x.AssignedToTeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
