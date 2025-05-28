using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaSupportClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9976));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9771), new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9771) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9772), new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9773) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9769), new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9770) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9764), new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9766) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9767), new DateTime(2025, 5, 28, 1, 51, 48, 264, DateTimeKind.Utc).AddTicks(9768) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 1, 51, 48, 265, DateTimeKind.Utc).AddTicks(2));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2505));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2293), new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2294) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2295), new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2295) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2292), new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2293) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2286), new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2289) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2290), new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2291) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 1, 45, 36, 471, DateTimeKind.Utc).AddTicks(2532));
        }
    }
}
