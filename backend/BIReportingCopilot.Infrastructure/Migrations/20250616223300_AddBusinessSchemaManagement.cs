using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessSchemaManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIFeedbackEntries_AIGenerationAttempts_GenerationAttemptId",
                table: "AIFeedbackEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_AIFeedbackEntries_QueryHistories_QueryHistoryId",
                table: "AIFeedbackEntries");

            migrationBuilder.DropTable(
                name: "QueryHistories");

            migrationBuilder.DropIndex(
                name: "IX_QueryHistory_QueryTimestamp",
                table: "QueryHistory");

            migrationBuilder.DropIndex(
                name: "IX_QueryHistory_UserId_QueryTimestamp",
                table: "QueryHistory");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_AttemptedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_UserId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AIFeedbackEntries_GenerationAttemptId",
                table: "AIFeedbackEntries");

            migrationBuilder.DropIndex(
                name: "IX_AIFeedbackEntries_QueryHistoryId",
                table: "AIFeedbackEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemMetrics",
                table: "SystemMetrics");

            migrationBuilder.DropColumn(
                name: "CompressionType",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "DatabaseContext",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "ResultData",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "ResultMetadata",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "SemanticVector",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "ResultRowCount",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "GenerationTimeMs",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "CorrectedSql",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "GeneratedSql",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "QueryHistoryId",
                table: "AIFeedbackEntries");

            migrationBuilder.RenameTable(
                name: "SystemMetrics",
                newName: "SystemMetricsEntity");

            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "SemanticCacheEntries",
                newName: "EmbeddingVector");

            migrationBuilder.RenameColumn(
                name: "SemanticSimilarityThreshold",
                table: "SemanticCacheEntries",
                newName: "SimilarityThreshold");

            migrationBuilder.RenameColumn(
                name: "GeneratedSQL",
                table: "QueryHistory",
                newName: "GeneratedSql");

            migrationBuilder.RenameColumn(
                name: "QueryTimestamp",
                table: "QueryHistory",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "NaturalLanguageQuery",
                table: "QueryHistory",
                newName: "Query");

            migrationBuilder.RenameColumn(
                name: "UserQuery",
                table: "AIGenerationAttempts",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "GeneratedSql",
                table: "AIGenerationAttempts",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "AIFeedbackEntries",
                newName: "Sentiment");

            migrationBuilder.RenameColumn(
                name: "ProcessingResult",
                table: "AIFeedbackEntries",
                newName: "CorrectedOutput");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMetrics_Timestamp",
                table: "SystemMetricsEntity",
                newName: "IX_SystemMetricsEntity_Timestamp");

            migrationBuilder.AlterColumn<string>(
                name: "QueryHash",
                table: "SemanticCacheEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalQuery",
                table: "SemanticCacheEntries",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedQuery",
                table: "SemanticCacheEntries",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "SemanticCacheEntries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SemanticCacheEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryType",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Query",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SemanticCacheEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SemanticCacheEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "QueryHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserFeedback",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "QueryHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "QueryHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "QueryComplexity",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ExecutionTimeMs",
                table: "QueryHistory",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "QueryHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "ConfidenceScore",
                table: "QueryHistory",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "QueryHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DatabaseName",
                table: "QueryHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExecutedAt",
                table: "QueryHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "QueryHistory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "QueryHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalQuery",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultData",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowCount",
                table: "QueryHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SchemaName",
                table: "QueryHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "SuccessRate",
                table: "PromptTemplates",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromptHash",
                table: "PromptLogs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TemplateId",
                table: "PromptLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "ModelVersion",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "ConfidenceScore",
                table: "AIGenerationAttempts",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "AIProvider",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "AIGenerationAttempts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "AIGenerationAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AIGenerationAttempts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedContent",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GenerationAttemptId",
                table: "AIGenerationAttempts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProcessingTimeMs",
                table: "AIGenerationAttempts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "QueryHistoryId",
                table: "AIGenerationAttempts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryId",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestParameters",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AIGenerationAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "AIFeedbackEntries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "GenerationAttemptId",
                table: "AIFeedbackEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AIFeedbackEntries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AIFeedbackEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AIFeedbackEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AIFeedbackEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHelpful",
                table: "AIFeedbackEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "AIFeedbackEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "QueryId",
                table: "AIFeedbackEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AIFeedbackEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AIFeedbackEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetricCategory",
                table: "SystemMetricsEntity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionDate",
                table: "SystemMetricsEntity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemMetricsEntity",
                table: "SystemMetricsEntity",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BusinessSchemas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessSchemas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachePerformanceMetrics",
                columns: table => new
                {
                    HitRate = table.Column<double>(type: "float", nullable: false),
                    MissRate = table.Column<double>(type: "float", nullable: false),
                    TotalRequests = table.Column<long>(type: "bigint", nullable: false),
                    AverageResponseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ThroughputPerSecond = table.Column<double>(type: "float", nullable: false),
                    MemoryUsage = table.Column<long>(type: "bigint", nullable: false),
                    EvictionRate = table.Column<double>(type: "float", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AverageHitTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    AverageMissTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    HitRatio = table.Column<double>(type: "float", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "LLMModelConfigs",
                columns: table => new
                {
                    ModelId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: false),
                    MaxTokens = table.Column<int>(type: "int", nullable: false),
                    TopP = table.Column<float>(type: "real", nullable: false),
                    FrequencyPenalty = table.Column<float>(type: "real", nullable: false),
                    PresencePenalty = table.Column<float>(type: "real", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UseCase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CostPerToken = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Capabilities = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LLMModelConfigs", x => x.ModelId);
                });

            migrationBuilder.CreateTable(
                name: "LLMProviderConfigs",
                columns: table => new
                {
                    ProviderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Organization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LLMProviderConfigs", x => x.ProviderId);
                });

            migrationBuilder.CreateTable(
                name: "LLMUsageLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LLMUsageLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuggestionCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeFrameDefinitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeFrameKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SqlExpression = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeFrameDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnifiedAIGenerationAttempt",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GenerationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualityScore = table.Column<double>(type: "float", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AIProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnifiedAIGenerationAttempt", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessSchemaVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    SchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    VersionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ChangeLog = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessSchemaVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessSchemaVersions_BusinessSchemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "BusinessSchemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuerySuggestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    QueryText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultTimeFrame = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TargetTables = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Complexity = table.Column<byte>(type: "tinyint", nullable: false),
                    RequiredPermissions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Relevance = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0.8m),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredTables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0.8m),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuerySuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuerySuggestions_SuggestionCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SuggestionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaGlossaryTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    SchemaVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessContext = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Synonyms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceTables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    IsAutoGenerated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaGlossaryTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaGlossaryTerms_BusinessSchemaVersions_SchemaVersionId",
                        column: x => x.SchemaVersionId,
                        principalTable: "BusinessSchemaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    SchemaVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromTable = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ToTable = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FromColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    IsAutoGenerated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceTableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetTableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaRelationships_BusinessSchemaVersions_SchemaVersionId",
                        column: x => x.SchemaVersionId,
                        principalTable: "BusinessSchemaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaTableContexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    SchemaVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, defaultValue: "dbo"),
                    BusinessPurpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BusinessContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryUseCase = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KeyBusinessMetrics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommonQueryPatterns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    IsAutoGenerated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaTableContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaTableContexts_BusinessSchemaVersions_SchemaVersionId",
                        column: x => x.SchemaVersionId,
                        principalTable: "BusinessSchemaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSchemaPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SchemaVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSchemaPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSchemaPreferences_BusinessSchemaVersions_SchemaVersionId",
                        column: x => x.SchemaVersionId,
                        principalTable: "BusinessSchemaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuggestionUsageAnalytics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SuggestionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeFrameUsed = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResultCount = table.Column<int>(type: "int", nullable: true),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: true),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    UserFeedback = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestionUsageAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuggestionUsageAnalytics_QuerySuggestions_SuggestionId",
                        column: x => x.SuggestionId,
                        principalTable: "QuerySuggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaColumnContexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    TableContextId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BusinessDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BusinessDataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataExamples = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommonUseCases = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsKeyColumn = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsPrimaryKey = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsForeignKey = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    IsAutoGenerated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaColumnContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaColumnContexts_SchemaTableContexts_TableContextId",
                        column: x => x.TableContextId,
                        principalTable: "SchemaTableContexts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Content", "CreatedDate" },
                values: new object[] { "You are a SQL Server expert helping generate business intelligence reports.\n\nDatabase schema:\n{schema}\n\nWhen the user asks: '{question}'\n- Write a T-SQL SELECT query to answer the question.\n- Only use SELECT statements, never write/alter data.\n- Use proper table and column names from the schema.\n- Include appropriate WHERE clauses for filtering.\n- Use JOINs when data from multiple tables is needed.\n- Format the query for readability.\n- Always add WITH (NOLOCK) hint to all table references for better read performance.\n- Format table hints as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints.\n\nReturn only the SQL query without any explanation or markdown formatting.", new DateTime(2025, 6, 16, 22, 32, 59, 999, DateTimeKind.Utc).AddTicks(9565) });

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

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntries_CreatedAt",
                table: "SemanticCacheEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_ExecutedAt",
                table: "QueryHistory",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_UserId",
                table: "QueryHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_GenerationAttemptId",
                table: "AIGenerationAttempts",
                column: "GenerationAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_QueryHistoryId",
                table: "AIGenerationAttempts",
                column: "QueryHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSchemas_IsActive",
                table: "BusinessSchemas",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSchemas_IsDefault",
                table: "BusinessSchemas",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "UK_BusinessSchemas_Name",
                table: "BusinessSchemas",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSchemaVersions_SchemaId_IsCurrent",
                table: "BusinessSchemaVersions",
                columns: new[] { "SchemaId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "UK_BusinessSchemaVersions_SchemaVersion",
                table: "BusinessSchemaVersions",
                columns: new[] { "SchemaId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LLMModelConfigs_IsEnabled_UseCase",
                table: "LLMModelConfigs",
                columns: new[] { "IsEnabled", "UseCase" });

            migrationBuilder.CreateIndex(
                name: "IX_LLMModelConfigs_ProviderId",
                table: "LLMModelConfigs",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_LLMProviderConfigs_IsEnabled_IsDefault",
                table: "LLMProviderConfigs",
                columns: new[] { "IsEnabled", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_LLMProviderConfigs_Name",
                table: "LLMProviderConfigs",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LLMUsageLogs_ModelId_Timestamp",
                table: "LLMUsageLogs",
                columns: new[] { "ModelId", "Timestamp" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_LLMUsageLogs_ProviderId_Timestamp",
                table: "LLMUsageLogs",
                columns: new[] { "ProviderId", "Timestamp" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_LLMUsageLogs_RequestId",
                table: "LLMUsageLogs",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LLMUsageLogs_RequestType_Timestamp",
                table: "LLMUsageLogs",
                columns: new[] { "RequestType", "Timestamp" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_LLMUsageLogs_UserId_Timestamp",
                table: "LLMUsageLogs",
                columns: new[] { "UserId", "Timestamp" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_QuerySuggestions_CategoryId_IsActive_SortOrder",
                table: "QuerySuggestions",
                columns: new[] { "CategoryId", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_QuerySuggestions_UsageCount_LastUsed",
                table: "QuerySuggestions",
                columns: new[] { "UsageCount", "LastUsed" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SchemaColumnContexts_TableContextId",
                table: "SchemaColumnContexts",
                column: "TableContextId");

            migrationBuilder.CreateIndex(
                name: "UK_SchemaColumnContexts_TableColumn",
                table: "SchemaColumnContexts",
                columns: new[] { "TableContextId", "ColumnName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchemaGlossaryTerms_SchemaVersionId",
                table: "SchemaGlossaryTerms",
                column: "SchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "UK_SchemaGlossaryTerms_VersionTerm",
                table: "SchemaGlossaryTerms",
                columns: new[] { "SchemaVersionId", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchemaRelationships_SchemaVersionId",
                table: "SchemaRelationships",
                column: "SchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaTableContexts_SchemaVersionId",
                table: "SchemaTableContexts",
                column: "SchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "UK_SchemaTableContexts_VersionTable",
                table: "SchemaTableContexts",
                columns: new[] { "SchemaVersionId", "SchemaName", "TableName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionCategories_CategoryKey",
                table: "SuggestionCategories",
                column: "CategoryKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionCategories_IsActive_SortOrder",
                table: "SuggestionCategories",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionUsageAnalytics_SuggestionId_UsedAt",
                table: "SuggestionUsageAnalytics",
                columns: new[] { "SuggestionId", "UsedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionUsageAnalytics_UserId_UsedAt",
                table: "SuggestionUsageAnalytics",
                columns: new[] { "UserId", "UsedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_TimeFrameDefinitions_IsActive_SortOrder",
                table: "TimeFrameDefinitions",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeFrameDefinitions_TimeFrameKey",
                table: "TimeFrameDefinitions",
                column: "TimeFrameKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnifiedAIGenerationAttempt_AttemptedAt",
                table: "UnifiedAIGenerationAttempt",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UnifiedAIGenerationAttempt_UserId",
                table: "UnifiedAIGenerationAttempt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSchemaPreferences_SchemaVersionId",
                table: "UserSchemaPreferences",
                column: "SchemaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSchemaPreferences_UserId",
                table: "UserSchemaPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UK_UserSchemaPreferences_User",
                table: "UserSchemaPreferences",
                columns: new[] { "UserId", "SchemaVersionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AIGenerationAttempts_AIGenerationAttempts_GenerationAttemptId",
                table: "AIGenerationAttempts",
                column: "GenerationAttemptId",
                principalTable: "AIGenerationAttempts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIGenerationAttempts_QueryHistory_QueryHistoryId",
                table: "AIGenerationAttempts",
                column: "QueryHistoryId",
                principalTable: "QueryHistory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIGenerationAttempts_AIGenerationAttempts_GenerationAttemptId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_AIGenerationAttempts_QueryHistory_QueryHistoryId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropTable(
                name: "CachePerformanceMetrics");

            migrationBuilder.DropTable(
                name: "LLMModelConfigs");

            migrationBuilder.DropTable(
                name: "LLMProviderConfigs");

            migrationBuilder.DropTable(
                name: "LLMUsageLogs");

            migrationBuilder.DropTable(
                name: "SchemaColumnContexts");

            migrationBuilder.DropTable(
                name: "SchemaGlossaryTerms");

            migrationBuilder.DropTable(
                name: "SchemaRelationships");

            migrationBuilder.DropTable(
                name: "SuggestionUsageAnalytics");

            migrationBuilder.DropTable(
                name: "TempFiles");

            migrationBuilder.DropTable(
                name: "TimeFrameDefinitions");

            migrationBuilder.DropTable(
                name: "UnifiedAIGenerationAttempt");

            migrationBuilder.DropTable(
                name: "UserSchemaPreferences");

            migrationBuilder.DropTable(
                name: "SchemaTableContexts");

            migrationBuilder.DropTable(
                name: "QuerySuggestions");

            migrationBuilder.DropTable(
                name: "BusinessSchemaVersions");

            migrationBuilder.DropTable(
                name: "SuggestionCategories");

            migrationBuilder.DropTable(
                name: "BusinessSchemas");

            migrationBuilder.DropIndex(
                name: "IX_SemanticCacheEntries_CreatedAt",
                table: "SemanticCacheEntries");

            migrationBuilder.DropIndex(
                name: "IX_QueryHistory_ExecutedAt",
                table: "QueryHistory");

            migrationBuilder.DropIndex(
                name: "IX_QueryHistory_UserId",
                table: "QueryHistory");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_GenerationAttemptId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_QueryHistoryId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemMetricsEntity",
                table: "SystemMetricsEntity");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "EntryType",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "Query",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "SemanticCacheEntries");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "DatabaseName",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "ExecutedAt",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "OriginalQuery",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "ResultData",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "RowCount",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "SchemaName",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "QueryHistory");

            migrationBuilder.DropColumn(
                name: "PromptHash",
                table: "PromptLogs");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "PromptLogs");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "GeneratedContent",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "GenerationAttemptId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ProcessingTimeMs",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "QueryHistoryId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "QueryId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "RequestParameters",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "IsHelpful",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "QueryId",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AIFeedbackEntries");

            migrationBuilder.DropColumn(
                name: "MetricCategory",
                table: "SystemMetricsEntity");

            migrationBuilder.DropColumn(
                name: "RetentionDate",
                table: "SystemMetricsEntity");

            migrationBuilder.RenameTable(
                name: "SystemMetricsEntity",
                newName: "SystemMetrics");

            migrationBuilder.RenameColumn(
                name: "SimilarityThreshold",
                table: "SemanticCacheEntries",
                newName: "SemanticSimilarityThreshold");

            migrationBuilder.RenameColumn(
                name: "EmbeddingVector",
                table: "SemanticCacheEntries",
                newName: "Tags");

            migrationBuilder.RenameColumn(
                name: "GeneratedSql",
                table: "QueryHistory",
                newName: "GeneratedSQL");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "QueryHistory",
                newName: "QueryTimestamp");

            migrationBuilder.RenameColumn(
                name: "Query",
                table: "QueryHistory",
                newName: "NaturalLanguageQuery");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AIGenerationAttempts",
                newName: "GeneratedSql");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "AIGenerationAttempts",
                newName: "UserQuery");

            migrationBuilder.RenameColumn(
                name: "Sentiment",
                table: "AIFeedbackEntries",
                newName: "Tags");

            migrationBuilder.RenameColumn(
                name: "CorrectedOutput",
                table: "AIFeedbackEntries",
                newName: "ProcessingResult");

            migrationBuilder.RenameIndex(
                name: "IX_SystemMetricsEntity_Timestamp",
                table: "SystemMetrics",
                newName: "IX_SystemMetrics_Timestamp");

            migrationBuilder.AlterColumn<string>(
                name: "QueryHash",
                table: "SemanticCacheEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalQuery",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedQuery",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SemanticCacheEntries",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CompressionType",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatabaseContext",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SemanticCacheEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResultData",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultMetadata",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SemanticVector",
                table: "SemanticCacheEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "SemanticCacheEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "QueryHistory",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<byte>(
                name: "UserFeedback",
                table: "QueryHistory",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "QueryHistory",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<byte>(
                name: "QueryComplexity",
                table: "QueryHistory",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ExecutionTimeMs",
                table: "QueryHistory",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "QueryHistory",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "QueryHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ConfidenceScore",
                table: "QueryHistory",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<int>(
                name: "ResultRowCount",
                table: "QueryHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "QueryHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SuccessRate",
                table: "PromptTemplates",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AIGenerationAttempts",
                type: "nvarchar(500)",
                maxLength: 500,
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

            migrationBuilder.AlterColumn<double>(
                name: "ConfidenceScore",
                table: "AIGenerationAttempts",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
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

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AIGenerationAttempts",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "GenerationTimeMs",
                table: "AIGenerationAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "AIFeedbackEntries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GenerationAttemptId",
                table: "AIFeedbackEntries",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AIFeedbackEntries",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CorrectedSql",
                table: "AIFeedbackEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedSql",
                table: "AIFeedbackEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "AIFeedbackEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueryHistoryId",
                table: "AIFeedbackEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemMetrics",
                table: "SystemMetrics",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "QueryHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CacheKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColumnsUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: false),
                    FromCache = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Query = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    QueryComplexity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TablesUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryHistories", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Content", "CreatedDate" },
                values: new object[] { "You are a SQL Server expert helping generate business intelligence reports.\n\nDatabase schema:\n{schema}\n\nWhen the user asks: '{question}'\n- Write a T-SQL SELECT query to answer the question.\n- Only use SELECT statements, never write/alter data.\n- Use proper table and column names from the schema.\n- Include appropriate WHERE clauses for filtering.\n- Use JOINs when data from multiple tables is needed.\n- Format the query for readability.\n\nReturn only the SQL query without any explanation or markdown formatting.", new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9645) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9542), new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9543) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9543), new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9544) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9541), new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9541) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9536), new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9538) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9539), new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9540) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001",
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9664));

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_QueryTimestamp",
                table: "QueryHistory",
                column: "QueryTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_UserId_QueryTimestamp",
                table: "QueryHistory",
                columns: new[] { "UserId", "QueryTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_AttemptedAt",
                table: "AIGenerationAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_UserId",
                table: "AIGenerationAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_GenerationAttemptId",
                table: "AIFeedbackEntries",
                column: "GenerationAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_QueryHistoryId",
                table: "AIFeedbackEntries",
                column: "QueryHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistories_ExecutedAt",
                table: "QueryHistories",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistories_UserId",
                table: "QueryHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIFeedbackEntries_AIGenerationAttempts_GenerationAttemptId",
                table: "AIFeedbackEntries",
                column: "GenerationAttemptId",
                principalTable: "AIGenerationAttempts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIFeedbackEntries_QueryHistories_QueryHistoryId",
                table: "AIFeedbackEntries",
                column: "QueryHistoryId",
                principalTable: "QueryHistories",
                principalColumn: "Id");
        }
    }
}
