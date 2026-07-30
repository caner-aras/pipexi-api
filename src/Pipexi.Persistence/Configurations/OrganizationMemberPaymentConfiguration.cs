using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Configurations;

public sealed class OrganizationMemberPaymentConfiguration : IEntityTypeConfiguration<OrganizationMemberPayment>
{
    public void Configure(EntityTypeBuilder<OrganizationMemberPayment> builder)
    {
        builder.ToTable("organization_member_payments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationMemberId)
            .HasColumnName("organization_member_id")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.PaidAt)
            .HasColumnName("paid_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Method)
            .HasColumnName("method")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Reference)
            .HasColumnName("reference")
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(x => x.PeriodStart)
            .HasColumnName("period_start")
            .HasColumnType("date");

        builder.Property(x => x.PeriodEnd)
            .HasColumnName("period_end")
            .HasColumnType("date");

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

        builder.HasIndex(x => new { x.OrganizationMemberId, x.PaidAt });

        builder.HasOne<OrganizationMember>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
