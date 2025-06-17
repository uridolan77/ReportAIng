using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "UnifiedAIGenerationAttempt",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AIGenerationAttempts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "AIGenerationAttempts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ModelVersion",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "AIGenerationAttempts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "AIGenerationAttempts",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AIProvider",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2764));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2636), new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2637) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2637), new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2638) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2634), new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2635) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2628), new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2631) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2632), new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2633) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 17, 13, 36, 37, 382, DateTimeKind.Utc).AddTicks(2789));

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_AttemptedAt",
                table: "AIGenerationAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_IsSuccessful_AttemptedAt",
                table: "AIGenerationAttempts",
                columns: new[] { "IsSuccessful", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_UserId",
                table: "AIGenerationAttempts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_AttemptedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_IsSuccessful_AttemptedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_UserId",
                table: "AIGenerationAttempts");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "UnifiedAIGenerationAttempt",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ModelVersion",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "AIGenerationAttempts",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AIProvider",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9565));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9384), new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9385) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9385), new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9386) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9382), new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9383) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9375), new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9379) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9380), new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9381) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9594));
        }
    }
}
