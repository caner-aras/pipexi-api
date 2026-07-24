using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pipexi.Persistence.Context;

#nullable disable

namespace Pipexi.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260712203000_TaskCommentAuthorTeamMember")]
public partial class TaskCommentAuthorTeamMember : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_task_comments_organization_members_organization_member_id",
            table: "task_comments");

        migrationBuilder.DropIndex(
            name: "IX_task_comments_organization_member_id",
            table: "task_comments");

        migrationBuilder.RenameColumn(
            name: "organization_member_id",
            table: "task_comments",
            newName: "team_member_id");

        migrationBuilder.Sql(
            """
            WITH mapped AS (
                SELECT DISTINCT ON (tm.organization_member_id)
                    tm.organization_member_id,
                    tm.id
                FROM team_members tm
                ORDER BY tm.organization_member_id, tm.created_at
            )
            UPDATE task_comments tc
            SET team_member_id = mapped.id
            FROM mapped
            WHERE mapped.organization_member_id = tc.team_member_id;
            """);

        migrationBuilder.Sql(
            """
            DELETE FROM task_comments tc
            WHERE NOT EXISTS (
                SELECT 1
                FROM team_members tm
                WHERE tm.id = tc.team_member_id
            );
            """);

        migrationBuilder.CreateIndex(
            name: "IX_task_comments_team_member_id",
            table: "task_comments",
            column: "team_member_id");

        migrationBuilder.AddForeignKey(
            name: "FK_task_comments_team_members_team_member_id",
            table: "task_comments",
            column: "team_member_id",
            principalTable: "team_members",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_task_comments_team_members_team_member_id",
            table: "task_comments");

        migrationBuilder.DropIndex(
            name: "IX_task_comments_team_member_id",
            table: "task_comments");

        migrationBuilder.RenameColumn(
            name: "team_member_id",
            table: "task_comments",
            newName: "organization_member_id");

        migrationBuilder.Sql(
            """
            UPDATE task_comments tc
            SET organization_member_id = tm.organization_member_id
            FROM team_members tm
            WHERE tm.id = tc.organization_member_id;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_task_comments_organization_member_id",
            table: "task_comments",
            column: "organization_member_id");

        migrationBuilder.AddForeignKey(
            name: "FK_task_comments_organization_members_organization_member_id",
            table: "task_comments",
            column: "organization_member_id",
            principalTable: "organization_members",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }
}
