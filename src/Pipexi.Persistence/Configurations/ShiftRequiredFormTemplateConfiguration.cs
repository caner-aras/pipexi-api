using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workforce.Domain.Entities;

namespace Workforce.Persistence.Configurations;

public sealed class ShiftRequiredFormTemplateConfiguration : IEntityTypeConfiguration<ShiftRequiredFormTemplate>
{
    public void Configure(EntityTypeBuilder<ShiftRequiredFormTemplate> builder)
    {
        builder.ToTable("shift_required_form_templates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ShiftId)
            .HasColumnName("shift_id")
            .IsRequired();

        builder.Property(x => x.FormTemplateId)
            .HasColumnName("form_template_id")
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

        builder.HasIndex(x => new { x.ShiftId, x.FormTemplateId })
            .IsUnique();

        builder.HasOne<Shift>()
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FormTemplate>()
            .WithMany()
            .HasForeignKey(x => x.FormTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}