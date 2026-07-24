using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pipexi.Persistence.Context;

#nullable disable

namespace Pipexi.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260712220000_TaskReporterUser")]
public partial class TaskReporterUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "reporter_user_id",
            table: "tasks",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_tasks_reporter_user_id",
            table: "tasks",
            column: "reporter_user_id");

        migrationBuilder.AddForeignKey(
            name: "FK_tasks_users_reporter_user_id",
            table: "tasks",
            column: "reporter_user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_tasks_users_reporter_user_id",
            table: "tasks");

        migrationBuilder.DropIndex(
            name: "IX_tasks_reporter_user_id",
            table: "tasks");

        migrationBuilder.DropColumn(
            name: "reporter_user_id",
            table: "tasks");
    }
}
