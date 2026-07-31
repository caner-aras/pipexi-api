using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pipexi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Team_LocationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "location_id",
                table: "teams",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_teams_location_id",
                table: "teams",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_locations_location_id",
                table: "teams",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_locations_location_id",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_location_id",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "location_id",
                table: "teams");
        }
    }
}
