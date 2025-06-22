namespace BIReportingCopilot.Core.Interfaces.BusinessContext;

/// <summary>
/// Repository interface for user feedback data
/// </summary>
public interface IUserFeedbackRepository
{
    Task<double> GetValidationAccuracyAsync(string validationKey);
    Task StoreValidationFeedbackAsync(string validationKey, bool wasCorrect);
    Task<double> GetThresholdFeedbackScoreAsync(string feedbackKey);
    Task RecordThresholdFeedbackAsync(string feedbackKey, double score);
}
