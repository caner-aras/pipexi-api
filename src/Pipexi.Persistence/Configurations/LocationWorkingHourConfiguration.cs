using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Configurations;

public sealed class LocationWorkingHourConfiguration : IEntityTypeConfiguration<LocationWorkingHour>
{
    public void Configure(EntityTypeBuilder<LocationWorkingHour> builder)
    {
        builder.ToTable("location_working_hours");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(x => x.DayOfWeek)
            .HasColumnName("day_of_week")
            .IsRequired();

        builder.Property(x => x.IsClosed)
            .HasColumnName("is_closed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.OpensAt)
            .HasColumnName("opens_at")
            .HasColumnType("time without time zone");

        builder.Property(x => x.ClosesAt)
            .HasColumnName("closes_at")
            .HasColumnType("time without time zone");

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

        builder.HasIndex(x => new { x.LocationId, x.DayOfWeek })
            .IsUnique();

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}