using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaColumnsToUsersManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add MFA-related columns to Users table
            migrationBuilder.AddColumn<bool>(
                name: "IsMfaEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MfaSecret",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MfaMethod",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneNumberVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMfaValidationDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackupCodes",
                table: "Users",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9128));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9009), new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9009) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9010), new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9011) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9007), new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9008) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9002), new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9004) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9006), new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9006) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 3, 22, 31, 53, DateTimeKind.Utc).AddTicks(9150));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop MFA-related columns from Users table
            migrationBuilder.DropColumn(
                name: "IsMfaEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MfaSecret",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MfaMethod",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPhoneNumberVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastMfaValidationDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BackupCodes",
                table: "Users");

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
    }
}
