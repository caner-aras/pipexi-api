using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pipexi.Persistence.Migrations;

/// <inheritdoc />
public partial class ConversationMessage_EditedAt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "edited_at",
            table: "conversation_messages",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "edited_at",
            table: "conversation_messages");
    }
}
