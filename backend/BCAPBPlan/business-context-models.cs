// Domain/Models/BusinessContext/BusinessContextProfile.cs
namespace ReportAIng.Domain.Models.BusinessContext
{
    public class BusinessContextProfile
    {
        public string OriginalQuestion { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public QueryIntent Intent { get; set; }
        public BusinessDomain Domain { get; set; }
        public List<BusinessEntity> Entities { get; set; } = new();
        public List<string> BusinessTerms { get; set; } = new();
        public Dictionary<string, double> TermRelevanceScores { get; set; } = new();
        public double ConfidenceScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        // Additional context for enhanced analysis
        public List<string> IdentifiedMetrics { get; set; } = new();
        public List<string> IdentifiedDimensions { get; set; } = new();
        public TimeRange? TimeContext { get; set; }
        public List<string> ComparisonTerms { get; set; } = new();
    }

    public class QueryIntent
    {
        public IntentType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public List<string> SubIntents { get; set; } = new();
    }

    public enum IntentType
    {
        Analytical,      // Complex analysis queries
        Operational,     // Transactional/operational queries
        Exploratory,     // Discovery queries
        Comparison,      // Comparative analysis
        Aggregation,     // Summary/rollup queries
        Trend,          // Time-series analysis
        Detail,         // Drill-down queries
        Unknown
    }

    public class BusinessDomain
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> RelatedTables { get; set; } = new();
        public List<string> KeyConcepts { get; set; } = new();
        public double RelevanceScore { get; set; }
    }

    public class BusinessEntity
    {
        public string Name { get; set; } = string.Empty;
        public EntityType Type { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string MappedTableName { get; set; } = string.Empty;
        public string MappedColumnName { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public enum EntityType
    {
        Table,
        Column,
        Metric,
        Dimension,
        GlossaryTerm,
        TimeReference,
        ComparisonValue,
        Unknown
    }

    public class TimeRange
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RelativeExpression { get; set; } = string.Empty; // e.g., "last month", "year to date"
        public TimeGranularity Granularity { get; set; }
    }

    public enum TimeGranularity
    {
        Hour,
        Day,
        Week,
        Month,
        Quarter,
        Year,
        Unknown
    }
}

// Domain/Models/BusinessContext/ContextualBusinessSchema.cs
namespace ReportAIng.Domain.Models.BusinessContext
{
    public class ContextualBusinessSchema
    {
        public List<BusinessTableInfoDto> RelevantTables { get; set; } = new();
        public Dictionary<long, List<BusinessColumnInfoDto>> TableColumns { get; set; } = new();
        public List<BusinessGlossaryDto> RelevantGlossaryTerms { get; set; } = new();
        public List<BusinessRule> BusinessRules { get; set; } = new();
        public List<TableRelationship> Relationships { get; set; } = new();
        public Dictionary<string, string> SemanticMappings { get; set; } = new();
        public double RelevanceScore { get; set; }
        public SchemaComplexity Complexity { get; set; }
        
        // Performance hints
        public List<string> SuggestedIndexes { get; set; } = new();
        public List<string> PartitioningHints { get; set; } = new();
    }

    public class BusinessRule
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RuleType Type { get; set; }
        public string SqlExpression { get; set; } = string.Empty;
        public List<string> AffectedColumns { get; set; } = new();
        public int Priority { get; set; }
    }

    public enum RuleType
    {
        Validation,
        Calculation,
        Filter,
        Aggregation,
        Security
    }

    public class TableRelationship
    {
        public string FromTable { get; set; } = string.Empty;
        public string ToTable { get; set; } = string.Empty;
        public string FromColumn { get; set; } = string.Empty;
        public string ToColumn { get; set; } = string.Empty;
        public RelationshipType Type { get; set; }
        public string BusinessMeaning { get; set; } = string.Empty;
    }

    public enum RelationshipType
    {
        OneToOne,
        OneToMany,
        ManyToMany
    }

    public enum SchemaComplexity
    {
        Simple,      // Single table
        Moderate,    // 2-3 tables with simple joins
        Complex,     // Multiple tables with complex relationships
        VeryComplex  // Many tables with nested relationships
    }
}

// Domain/Models/PromptGeneration/PromptGenerationContext.cs
namespace ReportAIng.Domain.Models.PromptGeneration
{
    public class PromptGenerationContext
    {
        public string UserQuestion { get; set; } = string.Empty;
        public BusinessContextProfile BusinessContext { get; set; } = new();
        public ContextualBusinessSchema Schema { get; set; } = new();
        public PromptTemplate SelectedTemplate { get; set; } = new();
        public Dictionary<string, string> PlaceholderValues { get; set; } = new();
        public List<QueryExample> RelevantExamples { get; set; } = new();
        public PromptComplexityLevel ComplexityLevel { get; set; }
        public List<string> OptimizationHints { get; set; } = new();
    }

    public class QueryExample
    {
        public string NaturalLanguageQuery { get; set; } = string.Empty;
        public string GeneratedSql { get; set; } = string.Empty;
        public string BusinessContext { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public List<string> UsedTables { get; set; } = new();
    }

    public enum PromptComplexityLevel
    {
        Basic,
        Standard,
        Advanced,
        Expert
    }
}