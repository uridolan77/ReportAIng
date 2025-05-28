using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHashToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3135));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3021), new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3022) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3022), new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3023) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3019), new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3020) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3015), new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3017) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3018), new DateTime(2025, 5, 27, 0, 33, 16, 65, DateTimeKind.Utc).AddTicks(3019) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7306));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7137), new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7138) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7139), new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7139) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7136), new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7137) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7131), new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7133) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7134), new DateTime(2025, 5, 26, 19, 27, 10, 999, DateTimeKind.Utc).AddTicks(7135) });
        }
    }
}
