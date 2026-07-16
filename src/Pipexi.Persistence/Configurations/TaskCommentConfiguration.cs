using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workforce.Domain.Entities;

namespace Workforce.Persistence.Configurations;

public sealed class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("task_comments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.WorkTaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.TeamMemberId)
            .HasColumnName("team_member_id")
            .IsRequired();

        builder.Property(x => x.Message)
            .HasColumnName("message")
            .HasMaxLength(4000)
            .IsRequired();

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

        builder.HasIndex(x => new { x.WorkTaskId, x.CreatedAt });

        builder.HasIndex(x => x.TeamMemberId);

        builder.HasOne<WorkTask>()
            .WithMany()
            .HasForeignKey(x => x.WorkTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(x => x.TeamMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
