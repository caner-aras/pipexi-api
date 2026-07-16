using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workforce.Domain.Entities;

namespace Workforce.Persistence.Configurations;

public sealed class ShiftBreakConfiguration : IEntityTypeConfiguration<ShiftBreak>
{
    public void Configure(EntityTypeBuilder<ShiftBreak> builder)
    {
        builder.ToTable("shift_breaks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ShiftId)
            .HasColumnName("shift_id")
            .IsRequired();

        builder.Property(x => x.StartAt)
            .HasColumnName("start_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.EndAt)
            .HasColumnName("end_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.IsPaid)
            .HasColumnName("is_paid")
            .HasDefaultValue(true)
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

        builder.HasIndex(x => new { x.ShiftId, x.StartAt });

        builder.HasOne<Shift>()
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
