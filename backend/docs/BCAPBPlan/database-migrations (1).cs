// Infrastructure/Migrations/AddPromptTemplatesTable.cs
namespace ReportAIng.Infrastructure.Migrations
{
    public partial class AddPromptTemplatesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IntentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_TemplateKey",
                table: "PromptTemplates",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_IntentType_IsActive",
                table: "PromptTemplates",
                columns: new[] { "IntentType", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PromptTemplates");
        }
    }
}

// Infrastructure/Migrations/AddQueryExamplesTable.cs
namespace ReportAIng.Infrastructure.Migrations
{
    public partial class AddQueryExamplesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueryExamples",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NaturalLanguageQuery = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    GeneratedSql = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsedTables = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BusinessContext = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SuccessRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    UsageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryExamples", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueryExamples_IntentType_Domain",
                table: "QueryExamples",
                columns: new[] { "IntentType", "Domain" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryExamples_SuccessRate",
                table: "QueryExamples",
                column: "SuccessRate",
                descending: new[] { true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "QueryExamples");
        }
    }
}

// Infrastructure/Migrations/AddVectorEmbeddingsTable.cs
namespace ReportAIng.Infrastructure.Migrations
{
    public partial class AddVectorEmbeddingsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VectorEmbeddings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    EmbeddingVector = table.Column<string>(type: "nvarchar(max)", nullable: false), // JSON array of floats
                    ModelVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VectorEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VectorEmbeddings_EntityType_EntityId",
                table: "VectorEmbeddings",
                columns: new[] { "EntityType", "EntityId" },
                unique: true);

            // Add vector search support (SQL Server 2022+)
            migrationBuilder.Sql(@"
                -- Enable columnstore for efficient vector operations
                CREATE NONCLUSTERED COLUMNSTORE INDEX IX_VectorEmbeddings_ColumnStore
                ON VectorEmbeddings (EntityType, EntityId, EmbeddingVector)
                WHERE EntityType IS NOT NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "VectorEmbeddings");
        }
    }
}

// Infrastructure/Migrations/AddPromptGenerationLogsTable.cs
namespace ReportAIng.Infrastructure.Migrations
{
    public partial class AddPromptGenerationLogsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromptGenerationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserQuestion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    GeneratedPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptLength = table.Column<int>(type: "int", nullable: false),
                    IntentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    TablesUsed = table.Column<int>(type: "int", nullable: false),
                    GenerationTimeMs = table.Column<int>(type: "int", nullable: false),
                    TemplateUsed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptGenerationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptGenerationLogs_UserId_Timestamp",
                table: "PromptGenerationLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptGenerationLogs_IntentType_Domain",
                table: "PromptGenerationLogs",
                columns: new[] { "IntentType", "Domain" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PromptGenerationLogs");
        }
    }
}

// Infrastructure/Migrations/ExtendBusinessTablesForContext.cs
namespace ReportAIng.Infrastructure.Migrations
{
    public partial class ExtendBusinessTablesForContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to BusinessTableInfo if they don't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') 
                    AND name = 'QueryComplexityScore')
                BEGIN
                    ALTER TABLE [dbo].[BusinessTableInfo]
                    ADD [QueryComplexityScore] decimal(3,2) NULL DEFAULT 0.5
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTableInfo]') 
                    AND name = 'PreferredJoinOrder')
                BEGIN
                    ALTER TABLE [dbo].[BusinessTableInfo]
                    ADD [PreferredJoinOrder] int NULL DEFAULT 100
                END
            ");

            // Add indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_BusinessTableInfo_DomainClassification_IsActive",
                table: "BusinessTableInfo",
                columns: new[] { "DomainClassification", "IsActive" })
                .Annotation("SqlServer:Include", new[] { "ImportanceScore", "UsageFrequency" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessColumnInfo_ImportanceScore_IsActive",
                table: "BusinessColumnInfo",
                columns: new[] { "ImportanceScore", "IsActive" })
                .Annotation("SqlServer:Include", new[] { "BusinessMeaning", "SemanticTags" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessTableInfo_DomainClassification_IsActive",
                table: "BusinessTableInfo");

            migrationBuilder.DropIndex(
                name: "IX_BusinessColumnInfo_ImportanceScore_IsActive",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "QueryComplexityScore",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "PreferredJoinOrder",
                table: "BusinessTableInfo");
        }
    }
}