using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// Business glossary DTO
/// </summary>
public class BusinessGlossaryDto
{
    public long Id { get; set; }
    public string TermId { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public List<string> RelatedTerms { get; set; } = new();
    public List<string> Examples { get; set; } = new();

    // Enhanced semantic metadata fields to match frontend interface
    public string Domain { get; set; } = string.Empty;
    public string MappedTables { get; set; } = string.Empty;
    public string MappedColumns { get; set; } = string.Empty;
    public string HierarchicalRelations { get; set; } = string.Empty;
    public string PreferredCalculation { get; set; } = string.Empty;
    public string DisambiguationRules { get; set; } = string.Empty;
    public string BusinessOwner { get; set; } = string.Empty;
    public string RegulationReferences { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; } = 1.0;
    public double AmbiguityScore { get; set; } = 0.0;
    public double RelevanceScore { get; set; } = 0.5;
    public string ContextualVariations { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsedDate { get; set; }
    public string? LastUsed { get; set; }
    public string? LastValidated { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Create business glossary request DTO
/// </summary>
public class CreateBusinessGlossaryRequest
{
    [Required]
    [StringLength(100)]
    public string Term { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Definition { get; set; } = string.Empty;

    [StringLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public List<string> Synonyms { get; set; } = new();

    public List<string> RelatedTerms { get; set; } = new();

    public List<string> Examples { get; set; } = new();

    public bool IsActive { get; set; } = true;
}
