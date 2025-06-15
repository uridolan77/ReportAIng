namespace BIReportingCopilot.Infrastructure.Data.Entities;

/// <summary>
/// Entity for storing query feedback from users
/// </summary>
public class QueryFeedbackEntity : BaseEntity
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string QueryId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
