namespace BIReportingCopilot.Core.Interfaces.Repository;

/// <summary>
/// Repository interface for user feedback operations
/// </summary>
public interface IUserFeedbackRepository
{
    /// <summary>
    /// Get threshold feedback score for dynamic optimization
    /// </summary>
    Task<double> GetThresholdFeedbackScoreAsync(string feedbackKey);
    
    /// <summary>
    /// Record threshold feedback for dynamic optimization
    /// </summary>
    Task RecordThresholdFeedbackAsync(string feedbackKey, double score);
    
    /// <summary>
    /// Get validation accuracy for confidence validation
    /// </summary>
    Task<double> GetValidationAccuracyAsync(string validationKey);
    
    /// <summary>
    /// Store validation feedback for confidence validation
    /// </summary>
    Task StoreValidationFeedbackAsync(string validationKey, bool wasCorrect);
}
