using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pipexi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "key", "status", "updated_at" },
                values: new object[,]
                {
                    { new Guid("13a2a3f6-f992-4f5f-8e4d-c67c2c0ba809"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "task.read", "active", null },
                    { new Guid("15f8c50d-0ff1-4f79-a1fb-8f11e9624603"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee.update", "active", null },
                    { new Guid("4b3330ce-d2bb-442d-bfe6-43dc16ca3c15"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "time.clockin", "active", null },
                    { new Guid("4bff31fb-a5fd-4f7c-a8db-cec95f90d810"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "task.create", "active", null },
                    { new Guid("7b6206f8-08f6-4833-b0c9-95e4a4f50f02"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee.create", "active", null },
                    { new Guid("7fc6b247-0809-4249-94ba-ff6260f2de05"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "shift.read", "active", null },
                    { new Guid("806fdd2b-b9a4-4fda-8e28-e72ee0a2f116"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "time.clockout", "active", null },
                    { new Guid("80cc9d06-bba0-4e6c-a02f-b1596f94f408"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "shift.publish", "active", null },
                    { new Guid("89bdd130-7a13-4a99-98cf-d59d7029e607"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "shift.update", "active", null },
                    { new Guid("8fcf3d67-20fb-4225-9e88-6d117f299313"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "report.read", "active", null },
                    { new Guid("968f0c96-c153-4f95-a6ef-6f4558fd0411"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "task.update", "active", null },
                    { new Guid("a5ed558e-09a0-4c2c-91d6-f6ef1c8cab06"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "shift.create", "active", null },
                    { new Guid("c9ce2ea5-f8f1-4f1f-9a74-2b2512f40b12"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "task.complete", "active", null },
                    { new Guid("e9f4dedf-78ea-4bce-afd5-425ff89f5514"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "report.export", "active", null },
                    { new Guid("f0dfb31e-810e-4d83-a08e-10f1b9444104"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee.delete", "active", null },
                    { new Guid("f8dfb2dd-70d3-4d59-a0c0-5f4f65ea6e01"), new DateTimeOffset(new DateTime(2026, 7, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee.read", "active", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("13a2a3f6-f992-4f5f-8e4d-c67c2c0ba809"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("15f8c50d-0ff1-4f79-a1fb-8f11e9624603"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("4b3330ce-d2bb-442d-bfe6-43dc16ca3c15"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("4bff31fb-a5fd-4f7c-a8db-cec95f90d810"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("7b6206f8-08f6-4833-b0c9-95e4a4f50f02"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("7fc6b247-0809-4249-94ba-ff6260f2de05"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("806fdd2b-b9a4-4fda-8e28-e72ee0a2f116"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("80cc9d06-bba0-4e6c-a02f-b1596f94f408"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("89bdd130-7a13-4a99-98cf-d59d7029e607"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("8fcf3d67-20fb-4225-9e88-6d117f299313"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("968f0c96-c153-4f95-a6ef-6f4558fd0411"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("a5ed558e-09a0-4c2c-91d6-f6ef1c8cab06"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("c9ce2ea5-f8f1-4f1f-9a74-2b2512f40b12"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e9f4dedf-78ea-4bce-afd5-425ff89f5514"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("f0dfb31e-810e-4d83-a08e-10f1b9444104"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("f8dfb2dd-70d3-4d59-a0c0-5f4f65ea6e01"));
        }
    }
}
