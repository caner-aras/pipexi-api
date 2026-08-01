using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pipexi.Persistence.Migrations;

/// <inheritdoc />
public partial class ConversationMessage_ReactionsJson : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "reactions_json",
            table: "conversation_messages",
            type: "jsonb",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "reactions_json",
            table: "conversation_messages");
    }
}
