using AutoMapper;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Mapping;

/// <summary>
/// AutoMapper profile for mapping between UnifiedQueryHistoryEntity and QueryHistoryItem
/// Updated to use the consolidated unified models
/// </summary>
public class QueryHistoryMappingProfile : Profile
{
    public QueryHistoryMappingProfile()
    {
        CreateMap<UnifiedQueryHistoryEntity, QueryHistoryItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.Query))
            .ForMember(dest => dest.Sql, opt => opt.MapFrom(src => src.GeneratedSql))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.ExecutedAt))
            .ForMember(dest => dest.Successful, opt => opt.MapFrom(src => src.IsSuccessful))
            .ForMember(dest => dest.ExecutionTimeMs, opt => opt.MapFrom(src => src.ExecutionTimeMs))
            .ForMember(dest => dest.Confidence, opt => opt.MapFrom(src => src.ConfidenceScore))
            .ForMember(dest => dest.Error, opt => opt.MapFrom(src => src.ErrorMessage))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId));

        // Reverse mapping if needed
        CreateMap<QueryHistoryItem, UnifiedQueryHistoryEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ParseLongSafely(src.Id)))
            .ForMember(dest => dest.Query, opt => opt.MapFrom(src => src.Question))
            .ForMember(dest => dest.GeneratedSql, opt => opt.MapFrom(src => src.Sql))
            .ForMember(dest => dest.ExecutedAt, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.IsSuccessful, opt => opt.MapFrom(src => src.Successful))
            .ForMember(dest => dest.ExecutionTimeMs, opt => opt.MapFrom(src => src.ExecutionTimeMs))
            .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => src.Confidence))
            .ForMember(dest => dest.ErrorMessage, opt => opt.MapFrom(src => src.Error))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
            // Set default values for properties not in QueryHistoryItem
            .ForMember(dest => dest.RowCount, opt => opt.Ignore())
            .ForMember(dest => dest.UserFeedback, opt => opt.Ignore())
            .ForMember(dest => dest.QueryComplexity, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdated, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
    }

    private static long ParseLongSafely(string value)
    {
        return long.TryParse(value, out var result) ? result : 0;
    }
}
