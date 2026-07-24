using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pipexi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TaskAssigneeTeamMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_organization_members_assigned_to_member_id",
                table: "tasks");

            migrationBuilder.RenameColumn(
                name: "assigned_to_member_id",
                table: "tasks",
                newName: "assigned_to_team_member_id");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_assigned_to_member_id",
                table: "tasks",
                newName: "IX_tasks_assigned_to_team_member_id");

            migrationBuilder.Sql(
                """
                UPDATE tasks t
                SET assigned_to_team_member_id = NULL
                WHERE assigned_to_team_member_id IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM team_members tm
                      WHERE tm.id = t.assigned_to_team_member_id
                  )
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_team_members_assigned_to_team_member_id",
                table: "tasks",
                column: "assigned_to_team_member_id",
                principalTable: "team_members",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_team_members_assigned_to_team_member_id",
                table: "tasks");

            migrationBuilder.RenameColumn(
                name: "assigned_to_team_member_id",
                table: "tasks",
                newName: "assigned_to_member_id");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_assigned_to_team_member_id",
                table: "tasks",
                newName: "IX_tasks_assigned_to_member_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_organization_members_assigned_to_member_id",
                table: "tasks",
                column: "assigned_to_member_id",
                principalTable: "organization_members",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
