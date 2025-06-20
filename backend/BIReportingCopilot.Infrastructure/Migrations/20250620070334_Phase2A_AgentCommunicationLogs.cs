using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIReportingCopilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2A_AgentCommunicationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIGenerationAttempts_AIGenerationAttempts_GenerationAttemptId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_AIGenerationAttempts_QueryHistory_QueryHistoryId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropTable(
                name: "UnifiedAIGenerationAttempt");

            migrationBuilder.DropIndex(
                name: "IX_SuggestionCategories_CategoryKey",
                table: "SuggestionCategories");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_GenerationAttemptId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_IsSuccessful_AttemptedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_QueryHistoryId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "SuggestionCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SuggestionCategories");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ContextData",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "GeneratedContent",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "GenerationAttemptId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ModifiedSql",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "PromptTemplate",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "QueryHistoryId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "WasExecuted",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "WasModified",
                table: "AIGenerationAttempts");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "SuggestionCategories",
                newName: "DisplayOrder");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "SuggestionCategories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "SuggestionCategories",
                newName: "UpdatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_SuggestionCategories_IsActive_SortOrder",
                table: "SuggestionCategories",
                newName: "IX_SuggestionCategories_IsActive_DisplayOrder");

            migrationBuilder.RenameColumn(
                name: "TokensUsed",
                table: "AIGenerationAttempts",
                newName: "TotalTokens");

            migrationBuilder.RenameColumn(
                name: "RequestParameters",
                table: "AIGenerationAttempts",
                newName: "GeneratedOutput");

            migrationBuilder.RenameColumn(
                name: "QueryId",
                table: "AIGenerationAttempts",
                newName: "Provider");

            migrationBuilder.RenameColumn(
                name: "Prompt",
                table: "AIGenerationAttempts",
                newName: "Model");

            migrationBuilder.RenameColumn(
                name: "ProcessingTimeMs",
                table: "AIGenerationAttempts",
                newName: "QualityScore");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SuggestionCategories",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SuggestionCategories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "SuggestionCategories",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "SuggestionCategories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SuggestionCategories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "BusinessContext",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessDomain",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessFriendlyName",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BusinessImportance",
                table: "SchemaMetadata",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPurpose",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRules",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataGovernanceLevel",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImportanceScore",
                table: "SchemaMetadata",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastBusinessReview",
                table: "SchemaMetadata",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NaturalLanguageDescription",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryIntents",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedBusinessTerms",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipContext",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SemanticSynonyms",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageExamples",
                table: "SchemaMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UsageFrequency",
                table: "SchemaMetadata",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AnalyticalUseCases",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessGlossaryTerms",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessOwner",
                table: "BusinessTableInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessProcesses",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataGovernancePolicies",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataQualityIndicators",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DomainClassification",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ImportanceScore",
                table: "BusinessTableInfo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LLMContextHints",
                table: "BusinessTableInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAnalyzed",
                table: "BusinessTableInfo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NaturalLanguageAliases",
                table: "BusinessTableInfo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QueryComplexityHints",
                table: "BusinessTableInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelationshipSemantics",
                table: "BusinessTableInfo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportingCategories",
                table: "BusinessTableInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SemanticCoverageScore",
                table: "BusinessTableInfo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SemanticDescription",
                table: "BusinessTableInfo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SemanticRelationships",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UsageFrequency",
                table: "BusinessTableInfo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UsagePatterns",
                table: "BusinessTableInfo",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VectorSearchKeywords",
                table: "BusinessTableInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AmbiguityScore",
                table: "BusinessGlossary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BusinessOwner",
                table: "BusinessGlossary",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConceptualLevel",
                table: "BusinessGlossary",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ConfidenceScore",
                table: "BusinessGlossary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ContextualVariations",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CrossDomainMappings",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisambiguationContext",
                table: "BusinessGlossary",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisambiguationRules",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "BusinessGlossary",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Examples",
                table: "BusinessGlossary",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HierarchicalRelations",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InferenceRules",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LLMPromptTemplates",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastValidated",
                table: "BusinessGlossary",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MappedColumns",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MappedTables",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredCalculation",
                table: "BusinessGlossary",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QueryPatterns",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegulationReferences",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SemanticEmbedding",
                table: "BusinessGlossary",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SemanticRelationships",
                table: "BusinessGlossary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SemanticStability",
                table: "BusinessGlossary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AnalyticalContext",
                table: "BusinessColumnInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessDataType",
                table: "BusinessColumnInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessMetrics",
                table: "BusinessColumnInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessQuestionTypes",
                table: "BusinessColumnInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CalculationRules",
                table: "BusinessColumnInfo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConceptualRelationships",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConstraintsAndRules",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataLineage",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DataQualityScore",
                table: "BusinessColumnInfo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DomainSpecificTerms",
                table: "BusinessColumnInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCalculatedField",
                table: "BusinessColumnInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSensitiveData",
                table: "BusinessColumnInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LLMPromptHints",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NaturalLanguageAliases",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreferredAggregation",
                table: "BusinessColumnInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QueryIntentMapping",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedBusinessTerms",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SemanticContext",
                table: "BusinessColumnInfo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SemanticRelevanceScore",
                table: "BusinessColumnInfo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SemanticSynonyms",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SemanticTags",
                table: "BusinessColumnInfo",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UsageFrequency",
                table: "BusinessColumnInfo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ValueExamples",
                table: "BusinessColumnInfo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VectorSearchTags",
                table: "BusinessColumnInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AIGenerationAttempts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AIGenerationAttempts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModelVersion",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "AIGenerationAttempts",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AIProvider",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            // Skip IDENTITY column change - handled manually
            // migrationBuilder.AlterColumn<string>(
            //     name: "Id",
            //     table: "AIGenerationAttempts",
            //     type: "nvarchar(450)",
            //     nullable: false,
            //     oldClrType: typeof(long),
            //     oldType: "bigint")
            //     .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "GenerationType",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InputPrompt",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InputTokens",
                table: "AIGenerationAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OutputTokens",
                table: "AIGenerationAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ResponseTimeMs",
                table: "AIGenerationAttempts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AIGenerationAttempts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AgentCommunicationLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ExecutionTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommunicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlertThreshold = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    BlockThreshold = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetManagement", x => x.Id);
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
                name: "CacheConfigurations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CacheType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultTtlSeconds = table.Column<long>(type: "bigint", nullable: false),
                    MaxSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxEntries = table.Column<int>(type: "int", nullable: false),
                    EvictionPolicy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnableCompression = table.Column<bool>(type: "bit", nullable: false),
                    EnableEncryption = table.Column<bool>(type: "bit", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CacheStatistics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CacheType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostOptimizationRecommendations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PotentialSavings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImpactScore = table.Column<double>(type: "float", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Implementation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Benefits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Risks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsImplemented = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostOptimizationRecommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostPredictions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstimatedTokens = table.Column<int>(type: "int", nullable: false),
                    EstimatedDurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Factors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostPredictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostTracking",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    CostPerToken = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    QueryId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Project = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostTracking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetricsEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OperationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetentionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetricsEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceQuotas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxQuantity = table.Column<int>(type: "int", nullable: false),
                    PeriodSeconds = table.Column<long>(type: "bigint", nullable: false),
                    CurrentUsage = table.Column<int>(type: "int", nullable: false),
                    ResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceQuotas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceUsage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResourceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RetentionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceUsage", x => x.Id);
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

            migrationBuilder.UpdateData(
                table: "PromptTemplates",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedDate",
                value: new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(664));

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "CacheExpiryHours",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(497), new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(498) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableAuditLogging",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(499), new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(500) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "EnableQueryCaching",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(496), new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(497) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxQueryExecutionTimeSeconds",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(491), new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(493) });

            migrationBuilder.UpdateData(
                table: "SystemConfiguration",
                keyColumn: "Key",
                keyValue: "MaxResultRows",
                columns: new[] { "CreatedDate", "LastUpdated" },
                values: new object[] { new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(494), new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(495) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-user-001",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 20, 7, 3, 33, 994, DateTimeKind.Utc).AddTicks(693));

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_Status",
                table: "AIGenerationAttempts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommunicationLogs_CorrelationId",
                table: "AgentCommunicationLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommunicationLogs_CreatedAt",
                table: "AgentCommunicationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommunicationLogs_SourceAgent_TargetAgent",
                table: "AgentCommunicationLogs",
                columns: new[] { "SourceAgent", "TargetAgent" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommunicationLogs_Success_CreatedAt",
                table: "AgentCommunicationLogs",
                columns: new[] { "Success", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentCommunicationLogs");

            migrationBuilder.DropTable(
                name: "BudgetManagement");

            migrationBuilder.DropTable(
                name: "BusinessDomain");

            migrationBuilder.DropTable(
                name: "CacheConfigurations");

            migrationBuilder.DropTable(
                name: "CacheStatistics");

            migrationBuilder.DropTable(
                name: "CostOptimizationRecommendations");

            migrationBuilder.DropTable(
                name: "CostPredictions");

            migrationBuilder.DropTable(
                name: "CostTracking");

            migrationBuilder.DropTable(
                name: "PerformanceMetricsEntries");

            migrationBuilder.DropTable(
                name: "ResourceQuotas");

            migrationBuilder.DropTable(
                name: "ResourceUsage");

            migrationBuilder.DropTable(
                name: "SemanticSchemaMapping");

            migrationBuilder.DropIndex(
                name: "IX_AIGenerationAttempts_Status",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SuggestionCategories");

            migrationBuilder.DropColumn(
                name: "BusinessContext",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "BusinessDomain",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "BusinessFriendlyName",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "BusinessImportance",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "BusinessPurpose",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "BusinessRules",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "DataGovernanceLevel",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "ImportanceScore",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "LastBusinessReview",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "NaturalLanguageDescription",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "QueryIntents",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "RelatedBusinessTerms",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "RelationshipContext",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "SemanticSynonyms",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "UsageExamples",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "UsageFrequency",
                table: "SchemaMetadata");

            migrationBuilder.DropColumn(
                name: "AnalyticalUseCases",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "BusinessGlossaryTerms",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "BusinessOwner",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "BusinessProcesses",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "DataGovernancePolicies",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "DataQualityIndicators",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "DomainClassification",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "ImportanceScore",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "LLMContextHints",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "LastAnalyzed",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "NaturalLanguageAliases",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "QueryComplexityHints",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "RelationshipSemantics",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "ReportingCategories",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "SemanticCoverageScore",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "SemanticDescription",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "SemanticRelationships",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "UsageFrequency",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "UsagePatterns",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "VectorSearchKeywords",
                table: "BusinessTableInfo");

            migrationBuilder.DropColumn(
                name: "AmbiguityScore",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "BusinessOwner",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "ConceptualLevel",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "ContextualVariations",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "CrossDomainMappings",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "DisambiguationContext",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "DisambiguationRules",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "Examples",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "HierarchicalRelations",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "InferenceRules",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "LLMPromptTemplates",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "LastValidated",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "MappedColumns",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "MappedTables",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "PreferredCalculation",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "QueryPatterns",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "RegulationReferences",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "SemanticEmbedding",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "SemanticRelationships",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "SemanticStability",
                table: "BusinessGlossary");

            migrationBuilder.DropColumn(
                name: "AnalyticalContext",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "BusinessDataType",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "BusinessMetrics",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "BusinessQuestionTypes",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "CalculationRules",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "ConceptualRelationships",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "ConstraintsAndRules",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "DataLineage",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "DataQualityScore",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "DomainSpecificTerms",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "IsCalculatedField",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "IsSensitiveData",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "LLMPromptHints",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "NaturalLanguageAliases",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "PreferredAggregation",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "QueryIntentMapping",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "RelatedBusinessTerms",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "SemanticContext",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "SemanticRelevanceScore",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "SemanticSynonyms",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "SemanticTags",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "UsageFrequency",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "ValueExamples",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "VectorSearchTags",
                table: "BusinessColumnInfo");

            migrationBuilder.DropColumn(
                name: "GenerationType",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "InputPrompt",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "InputTokens",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "OutputTokens",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                table: "AIGenerationAttempts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AIGenerationAttempts");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "SuggestionCategories",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "SuggestionCategories",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SuggestionCategories",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_SuggestionCategories_IsActive_DisplayOrder",
                table: "SuggestionCategories",
                newName: "IX_SuggestionCategories_IsActive_SortOrder");

            migrationBuilder.RenameColumn(
                name: "TotalTokens",
                table: "AIGenerationAttempts",
                newName: "TokensUsed");

            migrationBuilder.RenameColumn(
                name: "QualityScore",
                table: "AIGenerationAttempts",
                newName: "ProcessingTimeMs");

            migrationBuilder.RenameColumn(
                name: "Provider",
                table: "AIGenerationAttempts",
                newName: "QueryId");

            migrationBuilder.RenameColumn(
                name: "Model",
                table: "AIGenerationAttempts",
                newName: "Prompt");

            migrationBuilder.RenameColumn(
                name: "GeneratedOutput",
                table: "AIGenerationAttempts",
                newName: "RequestParameters");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SuggestionCategories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SuggestionCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "SuggestionCategories",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "SuggestionCategories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "SuggestionCategories",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "SuggestionCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AIGenerationAttempts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AIGenerationAttempts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ModelVersion",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "AIGenerationAttempts",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AlterColumn<string>(
                name: "AIProvider",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            // Skip IDENTITY column change - handled manually
            // migrationBuilder.AlterColumn<long>(
            //     name: "Id",
            //     table: "AIGenerationAttempts",
            //     type: "bigint",
            //     nullable: false,
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(450)")
            //     .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "AIGenerationAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ConfidenceScore",
                table: "AIGenerationAttempts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextData",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
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

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                table: "AIGenerationAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedSql",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromptTemplate",
                table: "AIGenerationAttempts",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "AIGenerationAttempts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "AIGenerationAttempts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "WasExecuted",
                table: "AIGenerationAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WasModified",
                table: "AIGenerationAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UnifiedAIGenerationAttempt",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AIProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenerationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualityScore = table.Column<double>(type: "float", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnifiedAIGenerationAttempt", x => x.Id);
                });

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
                name: "IX_SuggestionCategories_CategoryKey",
                table: "SuggestionCategories",
                column: "CategoryKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_GenerationAttemptId",
                table: "AIGenerationAttempts",
                column: "GenerationAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_IsSuccessful_AttemptedAt",
                table: "AIGenerationAttempts",
                columns: new[] { "IsSuccessful", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIGenerationAttempts_QueryHistoryId",
                table: "AIGenerationAttempts",
                column: "QueryHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UnifiedAIGenerationAttempt_AttemptedAt",
                table: "UnifiedAIGenerationAttempt",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UnifiedAIGenerationAttempt_UserId",
                table: "UnifiedAIGenerationAttempt",
                column: "UserId");

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
    }
}
