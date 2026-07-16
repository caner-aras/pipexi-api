using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workforce.Domain.Entities;

namespace Workforce.Persistence.Configurations;

public sealed class FormAnswerConfiguration : IEntityTypeConfiguration<FormAnswer>
{
    public void Configure(EntityTypeBuilder<FormAnswer> builder)
    {
        builder.ToTable("form_answers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.FormSubmissionId)
            .HasColumnName("form_submission_id")
            .IsRequired();

        builder.Property(x => x.FormFieldId)
            .HasColumnName("form_field_id")
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasColumnType("text");

        builder.Property(x => x.FileId)
            .HasColumnName("file_id");

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

        builder.HasIndex(x => new { x.FormSubmissionId, x.FormFieldId });

        builder.HasOne<FormSubmission>()
            .WithMany()
            .HasForeignKey(x => x.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FormField>()
            .WithMany()
            .HasForeignKey(x => x.FormFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StoredFile>()
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
