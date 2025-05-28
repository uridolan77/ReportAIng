using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAILearningTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIGenerationAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GenerationTimeMs = table.Column<int>(type: "int", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    PromptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContextData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokensUsed = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    WasExecuted = table.Column<bool>(type: "bit", nullable: false),
                    WasModified = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIGenerationAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueryHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Query = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SchemaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TablesUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColumnsUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryComplexity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromCache = table.Column<bool>(type: "bit", nullable: false),
                    CacheKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SemanticCacheEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultMetadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessCount = table.Column<int>(type: "int", nullable: false),
                    SemanticSimilarityThreshold = table.Column<double>(type: "float", nullable: false),
                    SemanticVector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabaseContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CompressionType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemanticCacheEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIFeedbackEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QueryHistoryId = table.Column<int>(type: "int", nullable: true),
                    GenerationAttemptId = table.Column<int>(type: "int", nullable: true),
                    OriginalQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectedSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedbackType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIFeedbackEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIFeedbackEntries_AIGenerationAttempts_GenerationAttemptId",
                        column: x => x.GenerationAttemptId,
                        principalTable: "AIGenerationAttempts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AIFeedbackEntries_QueryHistories_QueryHistoryId",
                        column: x => x.QueryHistoryId,
                        principalTable: "QueryHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 28, 16, 12, 17, 313, DateTimeKind.Utc).AddTicks(9645));

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
                name: "IX_AIFeedbackEntries_CreatedAt",
                table: "AIFeedbackEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_GenerationAttemptId",
                table: "AIFeedbackEntries",
                column: "GenerationAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_QueryHistoryId",
                table: "AIFeedbackEntries",
                column: "QueryHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_UserId_CreatedAt",
                table: "AIFeedbackEntries",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_AttemptedAt",
                table: "AIGenerationAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_UserId",
                table: "AIGenerationAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistories_ExecutedAt",
                table: "QueryHistories",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistories_UserId",
                table: "QueryHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntries_ExpiresAt",
                table: "SemanticCacheEntries",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntries_LastAccessedAt",
                table: "SemanticCacheEntries",
                column: "LastAccessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntries_QueryHash",
                table: "SemanticCacheEntries",
                column: "QueryHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIFeedbackEntries");

            migrationBuilder.DropTable(
                name: "SemanticCacheEntries");

            migrationBuilder.DropTable(
                name: "AIGenerationAttempts");

            migrationBuilder.DropTable(
                name: "QueryHistories");

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
    }
}
