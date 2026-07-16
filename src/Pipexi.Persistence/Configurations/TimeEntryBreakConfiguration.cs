using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workforce.Domain.Entities;

namespace Workforce.Persistence.Configurations;

public sealed class TimeEntryBreakConfiguration : IEntityTypeConfiguration<TimeEntryBreak>
{
    public void Configure(EntityTypeBuilder<TimeEntryBreak> builder)
    {
        builder.ToTable("time_entry_breaks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.TimeEntryId)
            .HasColumnName("time_entry_id")
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

        builder.HasIndex(x => new { x.TimeEntryId, x.StartAt });

        builder.HasOne<TimeEntry>()
            .WithMany()
            .HasForeignKey(x => x.TimeEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
