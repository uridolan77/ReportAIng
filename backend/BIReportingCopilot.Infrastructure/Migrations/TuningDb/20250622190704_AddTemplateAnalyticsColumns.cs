using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BIReportingCopilot.Infrastructure.Migrations.TuningDb
{
    /// <inheritdoc />
    public partial class AddTemplateAnalyticsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIFeedbackEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GenerationAttemptId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeedbackType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectedOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sentiment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHelpful = table.Column<bool>(type: "bit", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QueryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIFeedbackEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIGenerationAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
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
                    table.PrimaryKey("PK_AIGenerationAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AITuningSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AITuningSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessDomain",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomainName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RelatedTables = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    KeyConcepts = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CommonQueries = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BusinessOwner = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RelatedDomains = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImportanceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessDomain", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessGlossary",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BusinessContext = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Synonyms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RelatedTerms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Examples = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MappedTables = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MappedColumns = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HierarchicalRelations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PreferredCalculation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisambiguationRules = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BusinessOwner = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RegulationReferences = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmbiguityScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContextualVariations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SemanticEmbedding = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    QueryPatterns = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LLMPromptTemplates = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DisambiguationContext = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SemanticRelationships = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConceptualLevel = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CrossDomainMappings = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SemanticStability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InferenceRules = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastValidated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessGlossary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessTableInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BusinessPurpose = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BusinessContext = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PrimaryUseCase = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CommonQueryPatterns = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    BusinessRules = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DomainClassification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NaturalLanguageAliases = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UsagePatterns = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DataQualityIndicators = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RelationshipSemantics = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ImportanceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsageFrequency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastAnalyzed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessOwner = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataGovernancePolicies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SemanticDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    BusinessProcesses = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AnalyticalUseCases = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ReportingCategories = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SemanticRelationships = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QueryComplexityHints = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BusinessGlossaryTerms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SemanticCoverageScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LLMContextHints = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VectorSearchKeywords = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessTableInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromptType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserQuery = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FullPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedSQL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromptLength = table.Column<int>(type: "int", nullable: false),
                    ResponseLength = table.Column<int>(type: "int", nullable: true),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TemplateId = table.Column<long>(type: "bigint", nullable: true),
                    PromptHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SuccessRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastBusinessReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessPurpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedBusinessTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessFriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NaturalLanguageDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationshipContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataGovernanceLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportanceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UsageFrequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessMetadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueryPatterns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatternName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NaturalLanguagePattern = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SqlTemplate = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BusinessContext = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequiredTables = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryPatterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SemanticSchemaMapping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryIntent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RelevantTables = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RelevantColumns = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    BusinessTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    QueryCategory = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SemanticVector = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemanticSchemaMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfiguration",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfiguration", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "BusinessColumnInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableInfoId = table.Column<long>(type: "bigint", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BusinessMeaning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BusinessContext = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataExamples = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ValidationRules = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NaturalLanguageAliases = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SemanticContext = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ConceptualRelationships = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DomainSpecificTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QueryIntentMapping = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BusinessQuestionTypes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SemanticSynonyms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AnalyticalContext = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BusinessMetrics = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SemanticRelevanceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LLMPromptHints = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    VectorSearchTags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ValueExamples = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DataLineage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CalculationRules = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SemanticTags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BusinessDataType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConstraintsAndRules = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataQualityScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsageFrequency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreferredAggregation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RelatedBusinessTerms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsKeyColumn = table.Column<bool>(type: "bit", nullable: false),
                    IsSensitiveData = table.Column<bool>(type: "bit", nullable: false),
                    IsCalculatedField = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessColumnInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessColumnInfo_BusinessTableInfo_TableInfoId",
                        column: x => x.TableInfoId,
                        principalTable: "BusinessTableInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PromptTemplates",
                columns: new[] { "Id", "BusinessFriendlyName", "BusinessMetadata", "BusinessPurpose", "BusinessRules", "Content", "CreatedBy", "CreatedDate", "DataGovernanceLevel", "Description", "ImportanceScore", "IntentType", "IsActive", "LastBusinessReviewDate", "LastUsedDate", "Name", "NaturalLanguageDescription", "Parameters", "Priority", "RelatedBusinessTerms", "RelationshipContext", "SuccessRate", "Tags", "TemplateKey", "UpdatedDate", "UsageCount", "UsageFrequency", "Version" },
                values: new object[] { 1L, null, null, null, null, "You are a SQL Server expert helping generate business intelligence reports.\n\nDatabase schema:\n{schema}\n\nWhen the user asks: '{question}'\n- Write a T-SQL SELECT query to answer the question.\n- Only use SELECT statements, never write/alter data.\n- Use proper table and column names from the schema.\n- Include appropriate WHERE clauses for filtering.\n- Use JOINs when data from multiple tables is needed.\n- Format the query for readability.\n- Always add WITH (NOLOCK) hint to all table references for better read performance.\n- Format table hints as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints.\n\nReturn only the SQL query without any explanation or markdown formatting.", "System", new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9886), null, "Basic template for generating SQL queries from natural language", null, "", true, null, null, "BasicQueryGeneration", null, null, 1, null, null, null, null, "", null, 0, null, "1.0" });

            migrationBuilder.InsertData(
                table: "SystemConfiguration",
                columns: new[] { "Key", "CreatedBy", "CreatedDate", "DataType", "Description", "IsEncrypted", "LastUpdated", "UpdatedBy", "UpdatedDate", "Value" },
                values: new object[,]
                {
                    { "CacheExpiryHours", null, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9672), "int", "Default cache expiry time in hours", false, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9673), "System", null, "24" },
                    { "EnableAuditLogging", null, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9674), "bool", "Enable comprehensive audit logging", false, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9674), "System", null, "true" },
                    { "EnableQueryCaching", null, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9670), "bool", "Enable caching of query results", false, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9671), "System", null, "true" },
                    { "MaxQueryExecutionTimeSeconds", null, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9666), "int", "Maximum time allowed for query execution", false, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9667), "System", null, "30" },
                    { "MaxResultRows", null, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9668), "int", "Maximum number of rows returned per query", false, new DateTime(2025, 6, 22, 19, 7, 4, 39, DateTimeKind.Utc).AddTicks(9669), "System", null, "10000" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_CreatedAt",
                table: "AIFeedbackEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_FeedbackType_CreatedAt",
                table: "AIFeedbackEntries",
                columns: new[] { "FeedbackType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackEntries_UserId_CreatedAt",
                table: "AIFeedbackEntries",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_AttemptedAt",
                table: "AIGenerationAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_Status",
                table: "AIGenerationAttempts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_UserId",
                table: "AIGenerationAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AITuningSettings_Category",
                table: "AITuningSettings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AITuningSettings_SettingKey",
                table: "AITuningSettings",
                column: "SettingKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessColumnInfo_IsActive_TableInfoId",
                table: "BusinessColumnInfo",
                columns: new[] { "IsActive", "TableInfoId" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessColumnInfo_IsKeyColumn",
                table: "BusinessColumnInfo",
                column: "IsKeyColumn");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessColumnInfo_Table_Column",
                table: "BusinessColumnInfo",
                columns: new[] { "TableInfoId", "ColumnName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessGlossary_Category",
                table: "BusinessGlossary",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessGlossary_IsActive_Category",
                table: "BusinessGlossary",
                columns: new[] { "IsActive", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessGlossary_Term",
                table: "BusinessGlossary",
                column: "Term",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessTableInfo_CreatedDate_IsActive",
                table: "BusinessTableInfo",
                columns: new[] { "CreatedDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessTableInfo_IsActive",
                table: "BusinessTableInfo",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessTableInfo_Schema_Table",
                table: "BusinessTableInfo",
                columns: new[] { "SchemaName", "TableName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptLogs_TemplateId_Timestamp",
                table: "PromptLogs",
                columns: new[] { "TemplateId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptLogs_Timestamp",
                table: "PromptLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PromptLogs_UserId",
                table: "PromptLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_IsActive_Name",
                table: "PromptTemplates",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Name_Version",
                table: "PromptTemplates",
                columns: new[] { "Name", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_UsageCount",
                table: "PromptTemplates",
                column: "UsageCount");

            migrationBuilder.CreateIndex(
                name: "IX_QueryPatterns_IsActive_LastUsedDate",
                table: "QueryPatterns",
                columns: new[] { "IsActive", "LastUsedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryPatterns_PatternName",
                table: "QueryPatterns",
                column: "PatternName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueryPatterns_Priority_Active",
                table: "QueryPatterns",
                columns: new[] { "Priority", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryPatterns_UsageCount",
                table: "QueryPatterns",
                column: "UsageCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIFeedbackEntries");

            migrationBuilder.DropTable(
                name: "AIGenerationAttempts");

            migrationBuilder.DropTable(
                name: "AITuningSettings");

            migrationBuilder.DropTable(
                name: "BusinessColumnInfo");

            migrationBuilder.DropTable(
                name: "BusinessDomain");

            migrationBuilder.DropTable(
                name: "BusinessGlossary");

            migrationBuilder.DropTable(
                name: "PromptLogs");

            migrationBuilder.DropTable(
                name: "PromptTemplates");

            migrationBuilder.DropTable(
                name: "QueryPatterns");

            migrationBuilder.DropTable(
                name: "SemanticSchemaMapping");

            migrationBuilder.DropTable(
                name: "SystemConfiguration");

            migrationBuilder.DropTable(
                name: "BusinessTableInfo");
        }
    }
}
