using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

/// <summary>
/// Entity for tracking daily token usage analytics and trends
/// </summary>
[Table("TokenUsageAnalytics")]
public class TokenUsageAnalyticsEntity
{
    /// <summary>
    /// Unique identifier for the analytics record
    /// </summary>
    [Key]
    [MaxLength(450)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// User ID associated with the token usage
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Date for the analytics aggregation
    /// </summary>
    [Required]
    public DateTime Date { get; set; }

    /// <summary>
    /// Type of request (e.g., enhanced_query, basic_query, analysis)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string RequestType { get; set; } = string.Empty;

    /// <summary>
    /// Intent type classification (e.g., Reporting, Analytical, Comparative)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string IntentType { get; set; } = string.Empty;

    /// <summary>
    /// Total number of requests for this day/user/type combination
    /// </summary>
    public int TotalRequests { get; set; } = 0;

    /// <summary>
    /// Total tokens used for this aggregation
    /// </summary>
    public int TotalTokensUsed { get; set; } = 0;

    /// <summary>
    /// Total cost for this aggregation
    /// </summary>
    [Column(TypeName = "decimal(10,6)")]
    public decimal TotalCost { get; set; } = 0.0m;

    /// <summary>
    /// Average tokens per request
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal AverageTokensPerRequest { get; set; } = 0.0m;

    /// <summary>
    /// Average cost per request
    /// </summary>
    [Column(TypeName = "decimal(10,6)")]
    public decimal AverageCostPerRequest { get; set; } = 0.0m;

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Natural language description for business context
    /// </summary>
    [MaxLength(2000)]
    public string? NaturalLanguageDescription { get; set; }

    /// <summary>
    /// Business rules applied during processing
    /// </summary>
    [MaxLength(2000)]
    public string? BusinessRules { get; set; }

    /// <summary>
    /// Relationship context information
    /// </summary>
    [MaxLength(2000)]
    public string? RelationshipContext { get; set; }

    /// <summary>
    /// Data governance level applied
    /// </summary>
    [MaxLength(100)]
    public string? DataGovernanceLevel { get; set; }

    /// <summary>
    /// Last business review timestamp
    /// </summary>
    public DateTime? LastBusinessReview { get; set; }

    /// <summary>
    /// Importance score for this usage pattern
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal ImportanceScore { get; set; } = 0.5m;

    /// <summary>
    /// Usage frequency score
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal UsageFrequency { get; set; } = 0.0m;

    /// <summary>
    /// Calculate averages based on totals
    /// </summary>
    public void CalculateAverages()
    {
        if (TotalRequests > 0)
        {
            AverageTokensPerRequest = (decimal)TotalTokensUsed / TotalRequests;
            AverageCostPerRequest = TotalCost / TotalRequests;
        }
        else
        {
            AverageTokensPerRequest = 0;
            AverageCostPerRequest = 0;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add usage data to this analytics record
    /// </summary>
    public void AddUsage(int tokens, decimal cost)
    {
        TotalRequests++;
        TotalTokensUsed += tokens;
        TotalCost += cost;
        CalculateAverages();
    }
}
