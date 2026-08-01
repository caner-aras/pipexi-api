using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Configurations;

public sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.ConversationId)
            .HasColumnName("conversation_id")
            .IsRequired();

        builder.Property(x => x.SenderOrganizationMemberId)
            .HasColumnName("sender_organization_member_id")
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasMaxLength(8000)
            .IsRequired();

        builder.Property(x => x.ReactionsJson)
            .HasColumnName("reactions_json")
            .HasColumnType("jsonb");

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

        builder.HasIndex(x => new { x.ConversationId, x.CreatedAt });

        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<OrganizationMember>()
            .WithMany()
            .HasForeignKey(x => x.SenderOrganizationMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
