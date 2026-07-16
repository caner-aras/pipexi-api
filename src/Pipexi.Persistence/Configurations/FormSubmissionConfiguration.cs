using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workforce.Domain.Entities;

namespace Workforce.Persistence.Configurations;

public sealed class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("form_submissions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(x => x.FormTemplateId)
            .HasColumnName("form_template_id")
            .IsRequired();

        builder.Property(x => x.SubmittedByMemberId)
            .HasColumnName("submitted_by_member_id")
            .IsRequired();

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id");

        builder.Property(x => x.ShiftId)
            .HasColumnName("shift_id");

        builder.Property(x => x.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasDefaultValue("submitted")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.OrganizationId, x.SubmittedAt });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FormTemplate>()
            .WithMany()
            .HasForeignKey(x => x.FormTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<OrganizationMember>()
            .WithMany()
            .HasForeignKey(x => x.SubmittedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkTask>()
            .WithMany()
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Shift>()
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
