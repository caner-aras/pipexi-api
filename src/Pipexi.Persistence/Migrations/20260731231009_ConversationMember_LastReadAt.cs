using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pipexi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConversationMember_LastReadAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_read_at",
                table: "conversation_members",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_read_at",
                table: "conversation_members");
        }
    }
}
