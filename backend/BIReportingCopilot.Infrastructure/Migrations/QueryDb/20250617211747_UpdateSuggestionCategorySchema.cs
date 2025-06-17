using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations.QueryDb
{
    /// <inheritdoc />
    public partial class UpdateSuggestionCategorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueryCache",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NormalizedQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultMetadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CacheTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HitCount = table.Column<int>(type: "int", nullable: false),
                    LastAccessedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueryFeedback",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryFeedback", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueryHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutionTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Query = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    UserFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryComplexity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SchemaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueryPerformance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: false),
                    LogicalReads = table.Column<long>(type: "bigint", nullable: false),
                    PhysicalReads = table.Column<long>(type: "bigint", nullable: false),
                    RowsAffected = table.Column<long>(type: "bigint", nullable: false),
                    CpuTime = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryPerformance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SemanticCacheEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingVector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SimilarityThreshold = table.Column<double>(type: "float", nullable: false),
                    AccessCount = table.Column<int>(type: "int", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QueryHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemanticCacheEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SemanticCacheEntry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SerializedResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueryHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalQuery = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    NormalizedQuery = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SqlQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingModel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessCount = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    SimilarityScore = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemanticCacheEntry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuggestionCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
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
                    Query = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredTables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0.8m),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "manual"),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_QueryCache_ExpiryTimestamp",
                table: "QueryCache",
                column: "ExpiryTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_QueryCache_ExpiryTimestamp_LastAccessedDate",
                table: "QueryCache",
                columns: new[] { "ExpiryTimestamp", "LastAccessedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryCache_LastAccessedDate",
                table: "QueryCache",
                column: "LastAccessedDate");

            migrationBuilder.CreateIndex(
                name: "IX_QueryCache_QueryHash",
                table: "QueryCache",
                column: "QueryHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_ExecutedAt",
                table: "QueryHistory",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_IsSuccessful_ExecutedAt",
                table: "QueryHistory",
                columns: new[] { "IsSuccessful", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_SessionId",
                table: "QueryHistory",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryHistory_UserId_ExecutedAt",
                table: "QueryHistory",
                columns: new[] { "UserId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryPerformance_QueryHash",
                table: "QueryPerformance",
                column: "QueryHash");

            migrationBuilder.CreateIndex(
                name: "IX_QueryPerformance_QueryHash_Timestamp",
                table: "QueryPerformance",
                columns: new[] { "QueryHash", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryPerformance_Timestamp",
                table: "QueryPerformance",
                column: "Timestamp");

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
                name: "IX_SemanticCacheEntry_CreatedAt",
                table: "SemanticCacheEntry",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntry_ExpiresAt",
                table: "SemanticCacheEntry",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntry_ExpiresAt_LastAccessedAt",
                table: "SemanticCacheEntry",
                columns: new[] { "ExpiresAt", "LastAccessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntry_LastAccessedAt",
                table: "SemanticCacheEntry",
                column: "LastAccessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SemanticCacheEntry_QueryHash",
                table: "SemanticCacheEntry",
                column: "QueryHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionCategories_CategoryKey",
                table: "SuggestionCategories",
                column: "CategoryKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionCategories_IsActive_DisplayOrder",
                table: "SuggestionCategories",
                columns: new[] { "IsActive", "DisplayOrder" });

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
                name: "IX_TempFiles_CreatedAt",
                table: "TempFiles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TempFiles_ExpiresAt",
                table: "TempFiles",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TempFiles_ExpiresAt_CreatedAt",
                table: "TempFiles",
                columns: new[] { "ExpiresAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeFrameDefinitions_IsActive_SortOrder",
                table: "TimeFrameDefinitions",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeFrameDefinitions_TimeFrameKey",
                table: "TimeFrameDefinitions",
                column: "TimeFrameKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueryCache");

            migrationBuilder.DropTable(
                name: "QueryFeedback");

            migrationBuilder.DropTable(
                name: "QueryHistory");

            migrationBuilder.DropTable(
                name: "QueryPerformance");

            migrationBuilder.DropTable(
                name: "SemanticCacheEntries");

            migrationBuilder.DropTable(
                name: "SemanticCacheEntry");

            migrationBuilder.DropTable(
                name: "SuggestionUsageAnalytics");

            migrationBuilder.DropTable(
                name: "TempFiles");

            migrationBuilder.DropTable(
                name: "TimeFrameDefinitions");

            migrationBuilder.DropTable(
                name: "QuerySuggestions");

            migrationBuilder.DropTable(
                name: "SuggestionCategories");
        }
    }
}
