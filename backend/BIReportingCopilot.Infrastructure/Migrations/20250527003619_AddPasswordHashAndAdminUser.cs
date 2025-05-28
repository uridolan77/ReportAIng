using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHashAndAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1936));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1813), new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1814) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1815), new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1816) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1812), new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1813) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1806), new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1809) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1810), new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1811) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "DisplayName", "Email", "IsActive", "LastLoginDate", "PasswordHash", "Roles", "UpdatedBy", "UpdatedDate", "Username" },
                values: new object[] { "admin-user-001", "System", new DateTime(2025, 5, 27, 0, 36, 19, 152, DateTimeKind.Utc).AddTicks(1952), "System Administrator", "admin@bireporting.local", true, null, "", "Admin", null, null, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001");

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
    }
}
