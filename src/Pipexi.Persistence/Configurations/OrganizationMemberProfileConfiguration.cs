using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Configurations;

public sealed class OrganizationMemberProfileConfiguration : IEntityTypeConfiguration<OrganizationMemberProfile>
{
    public void Configure(EntityTypeBuilder<OrganizationMemberProfile> builder)
    {
        builder.ToTable("organization_member_profiles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationMemberId)
            .HasColumnName("organization_member_id")
            .IsRequired();

        builder.Property(x => x.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date");

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasMaxLength(30);

        builder.Property(x => x.AddressLine1)
            .HasColumnName("address_line1")
            .HasMaxLength(200);

        builder.Property(x => x.AddressLine2)
            .HasColumnName("address_line2")
            .HasMaxLength(200);

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasColumnName("state")
            .HasMaxLength(100);

        builder.Property(x => x.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(30);

        builder.Property(x => x.Country)
            .HasColumnName("country")
            .HasMaxLength(100);

        builder.Property(x => x.EmergencyContactName)
            .HasColumnName("emergency_contact_name")
            .HasMaxLength(150);

        builder.Property(x => x.EmergencyContactPhone)
            .HasColumnName("emergency_contact_phone")
            .HasMaxLength(50);

        builder.Property(x => x.NationalId)
            .HasColumnName("national_id")
            .HasMaxLength(50);

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

        builder.HasIndex(x => x.OrganizationMemberId)
            .IsUnique();

        builder.HasOne<OrganizationMember>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
