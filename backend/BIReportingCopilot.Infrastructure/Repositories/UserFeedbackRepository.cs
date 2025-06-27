using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for user feedback data
/// </summary>
public class UserFeedbackRepository : BIReportingCopilot.Core.Interfaces.BusinessContext.IUserFeedbackRepository
{
    private readonly BICopilotContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserFeedbackRepository> _logger;

    public UserFeedbackRepository(
        BICopilotContext context,
        ICacheService cacheService,
        ILogger<UserFeedbackRepository> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<double> GetValidationAccuracyAsync(string validationKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(validationKey))
                return 0.5; // Default neutral accuracy

            var cacheKey = $"validation_accuracy:{validationKey}";
            var (found, cached) = await _cacheService.TryGetValueAsync<double>(cacheKey);
            if (found)
            {
                return cached;
            }

            // Calculate accuracy from stored feedback
            var feedbackRecords = await GetValidationFeedbackRecords(validationKey);
            
            if (!feedbackRecords.Any())
            {
                return 0.5; // Default neutral accuracy when no feedback exists
            }

            var accuracy = (double)feedbackRecords.Count(f => f.WasCorrect) / feedbackRecords.Count;
            
            // Cache the result for 1 hour
            await _cacheService.SetValueAsync(cacheKey, accuracy, TimeSpan.FromHours(1));
            
            _logger.LogDebug("Calculated validation accuracy for {ValidationKey}: {Accuracy:P2}", 
                validationKey, accuracy);

            return accuracy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting validation accuracy for key: {ValidationKey}", validationKey);
            return 0.5; // Return neutral accuracy on error
        }
    }

    public async Task StoreValidationFeedbackAsync(string validationKey, bool wasCorrect)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(validationKey))
                return;

            // Store in memory cache as a simple implementation
            // In a real implementation, this would be stored in a database table
            var feedbackRecords = await GetValidationFeedbackRecords(validationKey);
            
            var newRecord = new ValidationFeedbackRecord
            {
                ValidationKey = validationKey,
                WasCorrect = wasCorrect,
                Timestamp = DateTime.UtcNow
            };
            
            feedbackRecords.Add(newRecord);
            
            // Keep only the last 100 records per validation key
            if (feedbackRecords.Count > 100)
            {
                feedbackRecords = feedbackRecords
                    .OrderByDescending(r => r.Timestamp)
                    .Take(100)
                    .ToList();
            }
            
            var cacheKey = $"validation_feedback:{validationKey}";
            await _cacheService.SetAsync(cacheKey, feedbackRecords, TimeSpan.FromDays(30));
            
            // Invalidate accuracy cache
            var accuracyCacheKey = $"validation_accuracy:{validationKey}";
            await _cacheService.RemoveAsync(accuracyCacheKey);
            
            _logger.LogDebug("Stored validation feedback for {ValidationKey}: {WasCorrect}", 
                validationKey, wasCorrect);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing validation feedback for key: {ValidationKey}", validationKey);
        }
    }

    public async Task<double> GetThresholdFeedbackScoreAsync(string feedbackKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(feedbackKey))
                return 0.5; // Default neutral score

            var cacheKey = $"threshold_score:{feedbackKey}";
            var (found, cached) = await _cacheService.TryGetValueAsync<double>(cacheKey);
            if (found)
            {
                return cached;
            }

            // Calculate average score from stored feedback
            var feedbackRecords = await GetThresholdFeedbackRecords(feedbackKey);
            
            if (!feedbackRecords.Any())
            {
                return 0.5; // Default neutral score when no feedback exists
            }

            var averageScore = feedbackRecords.Average(f => f.Score);
            
            // Cache the result for 1 hour
            await _cacheService.SetValueAsync(cacheKey, averageScore, TimeSpan.FromHours(1));
            
            _logger.LogDebug("Calculated threshold feedback score for {FeedbackKey}: {Score:F3}", 
                feedbackKey, averageScore);

            return averageScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting threshold feedback score for key: {FeedbackKey}", feedbackKey);
            return 0.5; // Return neutral score on error
        }
    }

    public async Task RecordThresholdFeedbackAsync(string feedbackKey, double score)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(feedbackKey))
                return;

            // Validate score range
            score = Math.Max(0.0, Math.Min(1.0, score));

            // Store in memory cache as a simple implementation
            var feedbackRecords = await GetThresholdFeedbackRecords(feedbackKey);
            
            var newRecord = new ThresholdFeedbackRecord
            {
                FeedbackKey = feedbackKey,
                Score = score,
                Timestamp = DateTime.UtcNow
            };
            
            feedbackRecords.Add(newRecord);
            
            // Keep only the last 100 records per feedback key
            if (feedbackRecords.Count > 100)
            {
                feedbackRecords = feedbackRecords
                    .OrderByDescending(r => r.Timestamp)
                    .Take(100)
                    .ToList();
            }
            
            var cacheKey = $"threshold_feedback:{feedbackKey}";
            await _cacheService.SetAsync(cacheKey, feedbackRecords, TimeSpan.FromDays(30));
            
            // Invalidate score cache
            var scoreCacheKey = $"threshold_score:{feedbackKey}";
            await _cacheService.RemoveAsync(scoreCacheKey);
            
            _logger.LogDebug("Recorded threshold feedback for {FeedbackKey}: {Score:F3}", 
                feedbackKey, score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording threshold feedback for key: {FeedbackKey}", feedbackKey);
        }
    }

    private async Task<List<ValidationFeedbackRecord>> GetValidationFeedbackRecords(string validationKey)
    {
        var cacheKey = $"validation_feedback:{validationKey}";
        var cached = await _cacheService.GetAsync<List<ValidationFeedbackRecord>>(cacheKey);
        return cached ?? new List<ValidationFeedbackRecord>();
    }

    private async Task<List<ThresholdFeedbackRecord>> GetThresholdFeedbackRecords(string feedbackKey)
    {
        var cacheKey = $"threshold_feedback:{feedbackKey}";
        var cached = await _cacheService.GetAsync<List<ThresholdFeedbackRecord>>(cacheKey);
        return cached ?? new List<ThresholdFeedbackRecord>();
    }

    private record ValidationFeedbackRecord
    {
        public string ValidationKey { get; init; } = string.Empty;
        public bool WasCorrect { get; init; }
        public DateTime Timestamp { get; init; }
    }

    private record ThresholdFeedbackRecord
    {
        public string FeedbackKey { get; init; } = string.Empty;
        public double Score { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
