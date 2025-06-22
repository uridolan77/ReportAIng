using AutoMapper;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Core.Interfaces.Analytics;
using System.Text.Json;
using InterfaceTemplatePerformanceMetrics = BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics;
using InterfacePerformanceDataPoint = BIReportingCopilot.Core.Interfaces.Analytics.PerformanceDataPoint;
using InterfaceComparisonResult = BIReportingCopilot.Core.Interfaces.Analytics.ComparisonResult;
using InterfacePerformanceAlert = BIReportingCopilot.Core.Interfaces.Analytics.PerformanceAlert;
using InterfaceAlertSeverity = BIReportingCopilot.Core.Interfaces.Analytics.AlertSeverity;

namespace BIReportingCopilot.Infrastructure.Mapping;

/// <summary>
/// AutoMapper profile for template analytics entities and models
/// </summary>
public class TemplateAnalyticsProfile : Profile
{
    public TemplateAnalyticsProfile()
    {
        // Entity to Analytics Model mappings (Core entities to Analytics models)

        // Analytics model mappings
        CreateMap<BIReportingCopilot.Core.Models.TemplatePerformanceMetricsEntity, InterfaceTemplatePerformanceMetrics>()
            .ForMember(dest => dest.TemplateKey, opt => opt.MapFrom(src => src.TemplateKey))
            .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.Template != null ? src.Template.Name : string.Empty))
            .ForMember(dest => dest.IntentType, opt => opt.MapFrom(src => src.IntentType))
            .ForMember(dest => dest.TotalUsages, opt => opt.MapFrom(src => src.TotalUsages))
            .ForMember(dest => dest.SuccessfulUsages, opt => opt.MapFrom(src => src.SuccessfulUsages))
            .ForMember(dest => dest.SuccessRate, opt => opt.MapFrom(src => src.SuccessRate))
            .ForMember(dest => dest.AverageConfidenceScore, opt => opt.MapFrom(src => src.AverageConfidenceScore))
            .ForMember(dest => dest.AverageProcessingTimeMs, opt => opt.MapFrom(src => src.AverageProcessingTimeMs))
            .ForMember(dest => dest.AverageUserRating, opt => opt.MapFrom(src => src.AverageUserRating))
            .ForMember(dest => dest.LastUsedDate, opt => opt.MapFrom(src => src.LastUsedDate))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
            .ForMember(dest => dest.AdditionalMetrics, opt => opt.MapFrom(src =>
                src.AdditionalMetrics ?? new Dictionary<string, object>()));

        // A/B Test mappings
        CreateMap<BIReportingCopilot.Core.Models.TemplateABTestEntity, ABTestDetails>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TestName, opt => opt.MapFrom(src => src.TestName))
            .ForMember(dest => dest.OriginalTemplateKey, opt => opt.MapFrom(src => src.OriginalTemplate != null ? src.OriginalTemplate.TemplateKey : string.Empty))
            .ForMember(dest => dest.VariantTemplateKey, opt => opt.MapFrom(src => src.VariantTemplate != null ? src.VariantTemplate.TemplateKey : string.Empty))
            .ForMember(dest => dest.TrafficSplitPercent, opt => opt.MapFrom(src => src.TrafficSplitPercent))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ABTestStatus>(src.Status, true)))
            .ForMember(dest => dest.OriginalSuccessRate, opt => opt.MapFrom(src => src.OriginalSuccessRate))
            .ForMember(dest => dest.VariantSuccessRate, opt => opt.MapFrom(src => src.VariantSuccessRate))
            .ForMember(dest => dest.StatisticalSignificance, opt => opt.MapFrom(src => src.StatisticalSignificance))
            .ForMember(dest => dest.WinnerTemplateKey, opt => opt.MapFrom(src => src.WinnerTemplate != null ? src.WinnerTemplate.TemplateKey : null))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy ?? string.Empty))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));

        // Improvement suggestion mappings
        CreateMap<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, TemplateImprovementSuggestion>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TemplateKey, opt => opt.MapFrom(src => src.Template != null ? src.Template.TemplateKey : string.Empty))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.SuggestionType))
            .ForMember(dest => dest.CurrentVersion, opt => opt.MapFrom(src => src.CurrentVersion))
            .ForMember(dest => dest.ReasoningExplanation, opt => opt.MapFrom(src => src.ReasoningExplanation))
            .ForMember(dest => dest.SuggestedChanges, opt => opt.MapFrom(src => src.SuggestedChanges))
            .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => src.ConfidenceScore ?? 0))
            .ForMember(dest => dest.ExpectedImprovementPercent, opt => opt.MapFrom(src => src.ExpectedImprovementPercent ?? 0))
            .ForMember(dest => dest.BasedOnDataPoints, opt => opt.MapFrom(src => src.BasedOnDataPoints))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.ReviewedBy, opt => opt.MapFrom(src => src.ReviewedBy))
            .ForMember(dest => dest.ReviewedDate, opt => opt.MapFrom(src => src.ReviewedDate))
            .ForMember(dest => dest.ReviewComments, opt => opt.MapFrom(src => src.ReviewComments))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));

        // Performance Alert mappings
        CreateMap<BIReportingCopilot.Core.Models.TemplatePerformanceMetricsEntity, InterfacePerformanceAlert>()
            .ForMember(dest => dest.TemplateKey, opt => opt.MapFrom(src => src.TemplateKey))
            .ForMember(dest => dest.AlertType, opt => opt.MapFrom(src => 
                src.SuccessRate < 0.5m ? "LowSuccessRate" : 
                src.AverageProcessingTimeMs > 5000 ? "SlowResponse" : 
                src.TotalUsages == 0 ? "NoUsage" : "General"))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => 
                src.SuccessRate < 0.5m ? $"Low success rate: {src.SuccessRate:P}" : 
                src.AverageProcessingTimeMs > 5000 ? $"Slow response time: {src.AverageProcessingTimeMs}ms" : 
                src.TotalUsages == 0 ? "Template has no usage" : "Performance issue detected"))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => 
                src.SuccessRate < 0.3m ? InterfaceAlertSeverity.Critical :
                src.SuccessRate < 0.5m ? InterfaceAlertSeverity.High :
                src.AverageProcessingTimeMs > 10000 ? InterfaceAlertSeverity.High :
                src.AverageProcessingTimeMs > 5000 ? InterfaceAlertSeverity.Medium : InterfaceAlertSeverity.Low))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.UpdatedDate));

        // Request/Response mappings
        CreateMap<ABTestRequest, BIReportingCopilot.Core.Models.TemplateABTestEntity>()
            .ForMember(dest => dest.TestName, opt => opt.MapFrom(src => src.TestName))
            .ForMember(dest => dest.TrafficSplitPercent, opt => opt.MapFrom(src => src.TrafficSplitPercent))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "active"))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OriginalTemplateId, opt => opt.Ignore())
            .ForMember(dest => dest.VariantTemplateId, opt => opt.Ignore());
    }
}
