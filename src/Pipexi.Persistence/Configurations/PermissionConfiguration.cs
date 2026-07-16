using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workforce.Domain.Entities;

namespace Workforce.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    private static readonly DateTimeOffset SeedCreatedAt = new(2026, 7, 9, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.Key).HasColumnName("key").HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique();

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

        builder.HasData(
            new { Id = Guid.Parse("f8dfb2dd-70d3-4d59-a0c0-5f4f65ea6e01"), Key = "employee.read", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("7b6206f8-08f6-4833-b0c9-95e4a4f50f02"), Key = "employee.create", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("15f8c50d-0ff1-4f79-a1fb-8f11e9624603"), Key = "employee.update", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("f0dfb31e-810e-4d83-a08e-10f1b9444104"), Key = "employee.delete", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("7fc6b247-0809-4249-94ba-ff6260f2de05"), Key = "shift.read", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("a5ed558e-09a0-4c2c-91d6-f6ef1c8cab06"), Key = "shift.create", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("89bdd130-7a13-4a99-98cf-d59d7029e607"), Key = "shift.update", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("80cc9d06-bba0-4e6c-a02f-b1596f94f408"), Key = "shift.publish", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("13a2a3f6-f992-4f5f-8e4d-c67c2c0ba809"), Key = "task.read", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("4bff31fb-a5fd-4f7c-a8db-cec95f90d810"), Key = "task.create", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("968f0c96-c153-4f95-a6ef-6f4558fd0411"), Key = "task.update", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("c9ce2ea5-f8f1-4f1f-9a74-2b2512f40b12"), Key = "task.complete", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("8fcf3d67-20fb-4225-9e88-6d117f299313"), Key = "report.read", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("e9f4dedf-78ea-4bce-afd5-425ff89f5514"), Key = "report.export", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("4b3330ce-d2bb-442d-bfe6-43dc16ca3c15"), Key = "time.clockin", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null },
            new { Id = Guid.Parse("806fdd2b-b9a4-4fda-8e28-e72ee0a2f116"), Key = "time.clockout", Status = "active", CreatedAt = SeedCreatedAt, UpdatedAt = (DateTimeOffset?)null });
    }
}
