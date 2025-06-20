using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Models;
using System.Collections.Concurrent;
using ReviewNotificationSettings = BIReportingCopilot.Core.Interfaces.Review.NotificationSettings;

namespace BIReportingCopilot.Infrastructure.Review;

/// <summary>
/// Phase 4: Review Notification Service Implementation
/// Manages notifications for human review workflows
/// </summary>
public class ReviewNotificationService : IReviewNotificationService
{
    private readonly ILogger<ReviewNotificationService> _logger;
    
    // In-memory storage for demo (would be database in production)
    private readonly ConcurrentDictionary<string, List<ReviewNotification>> _notificationsByUser = new();
    private readonly ConcurrentDictionary<string, ReviewNotificationSettings> _userSettings = new();

    public ReviewNotificationService(ILogger<ReviewNotificationService> logger)
    {
        _logger = logger;
        InitializeDefaultSettings();
    }

    /// <summary>
    /// Send review assignment notification
    /// </summary>
    public async Task<bool> SendReviewAssignmentNotificationAsync(
        string reviewId,
        string assignedTo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📧 Sending review assignment notification: {ReviewId} to {AssignedTo}", 
                reviewId, assignedTo);

            var notification = new ReviewNotification
            {
                ReviewRequestId = reviewId,
                RecipientId = assignedTo,
                Type = NotificationType.ReviewAssigned,
                Title = "New Review Assignment",
                Message = $"You have been assigned a new review request: {reviewId}",
                Priority = NotificationPriority.Normal,
                Data = new Dictionary<string, object>
                {
                    ["reviewId"] = reviewId,
                    ["assignedAt"] = DateTime.UtcNow
                }
            };

            await StoreNotificationAsync(notification, cancellationToken);
            await SendNotificationAsync(notification, cancellationToken);

            _logger.LogInformation("✅ Review assignment notification sent successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending review assignment notification");
            return false;
        }
    }

    /// <summary>
    /// Send review reminder notification
    /// </summary>
    public async Task<bool> SendReviewReminderAsync(
        string reviewId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("⏰ Sending review reminder: {ReviewId}", reviewId);

            // In production, would get review details and assigned user
            var assignedTo = "user@company.com"; // Placeholder

            var notification = new ReviewNotification
            {
                ReviewRequestId = reviewId,
                RecipientId = assignedTo,
                Type = NotificationType.ReviewReminder,
                Title = "Review Reminder",
                Message = $"Reminder: Review request {reviewId} is still pending your attention",
                Priority = NotificationPriority.High,
                Data = new Dictionary<string, object>
                {
                    ["reviewId"] = reviewId,
                    ["reminderSentAt"] = DateTime.UtcNow
                }
            };

            await StoreNotificationAsync(notification, cancellationToken);
            await SendNotificationAsync(notification, cancellationToken);

            _logger.LogInformation("✅ Review reminder sent successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending review reminder");
            return false;
        }
    }

    /// <summary>
    /// Send escalation notification
    /// </summary>
    public async Task<bool> SendEscalationNotificationAsync(
        string reviewId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("⬆️ Sending escalation notification: {ReviewId} - {Reason}", reviewId, reason);

            // In production, would determine escalation recipients based on org structure
            var escalationRecipients = new[] { "manager@company.com", "admin@company.com" };

            foreach (var recipient in escalationRecipients)
            {
                var notification = new ReviewNotification
                {
                    ReviewRequestId = reviewId,
                    RecipientId = recipient,
                    Type = NotificationType.ReviewEscalated,
                    Title = "Review Escalated",
                    Message = $"Review request {reviewId} has been escalated. Reason: {reason}",
                    Priority = NotificationPriority.Urgent,
                    Data = new Dictionary<string, object>
                    {
                        ["reviewId"] = reviewId,
                        ["reason"] = reason,
                        ["escalatedAt"] = DateTime.UtcNow
                    }
                };

                await StoreNotificationAsync(notification, cancellationToken);
                await SendNotificationAsync(notification, cancellationToken);
            }

            _logger.LogInformation("✅ Escalation notifications sent successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending escalation notification");
            return false;
        }
    }

    /// <summary>
    /// Get notifications for a user
    /// </summary>
    public async Task<List<ReviewNotification>> GetNotificationsAsync(
        string userId,
        bool unreadOnly = false,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            if (!_notificationsByUser.TryGetValue(userId, out var notifications))
            {
                return new List<ReviewNotification>();
            }

            var filteredNotifications = notifications.AsEnumerable();

            if (unreadOnly)
            {
                filteredNotifications = filteredNotifications.Where(n => !n.IsRead);
            }

            return filteredNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting notifications for user: {UserId}", userId);
            return new List<ReviewNotification>();
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    public async Task<bool> MarkNotificationAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            foreach (var userNotifications in _notificationsByUser.Values)
            {
                var notification = userNotifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    
                    _logger.LogDebug("✅ Notification marked as read: {NotificationId}", notificationId);
                    return true;
                }
            }

            _logger.LogWarning("⚠️ Notification not found: {NotificationId}", notificationId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error marking notification as read");
            return false;
        }
    }

    /// <summary>
    /// Get notification settings for a user
    /// </summary>
    public async Task<ReviewNotificationSettings> GetNotificationSettingsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            return _userSettings.TryGetValue(userId, out var settings) 
                ? settings 
                : GetDefaultNotificationSettings(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting notification settings for user: {UserId}", userId);
            return GetDefaultNotificationSettings(userId);
        }
    }

    /// <summary>
    /// Update notification settings
    /// </summary>
    public async Task<bool> UpdateNotificationSettingsAsync(
        string userId,
        ReviewNotificationSettings settings,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            _userSettings[userId] = settings;
            
            _logger.LogInformation("✅ Notification settings updated for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating notification settings for user: {UserId}", userId);
            return false;
        }
    }

    // Private helper methods
    private async Task StoreNotificationAsync(ReviewNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            if (!_notificationsByUser.ContainsKey(notification.RecipientId))
            {
                _notificationsByUser[notification.RecipientId] = new List<ReviewNotification>();
            }

            _notificationsByUser[notification.RecipientId].Add(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error storing notification");
        }
    }

    private async Task SendNotificationAsync(ReviewNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            var settings = await GetNotificationSettingsAsync(notification.RecipientId, cancellationToken);

            // Check if notifications are enabled for this type
            if (!settings.NotificationTypes.Contains(ReviewType.SqlValidation)) // Simplified check
            {
                _logger.LogDebug("🔇 Notifications disabled for user: {UserId}", notification.RecipientId);
                return;
            }

            // Check quiet hours
            var now = DateTime.Now.TimeOfDay;
            if (now >= settings.QuietHoursStart && now <= settings.QuietHoursEnd)
            {
                _logger.LogDebug("🔇 Quiet hours active for user: {UserId}", notification.RecipientId);
                return;
            }

            // Send notifications based on user preferences
            if (settings.EmailNotifications)
            {
                await SendEmailNotificationAsync(notification, cancellationToken);
            }

            if (settings.InAppNotifications)
            {
                await SendInAppNotificationAsync(notification, cancellationToken);
            }

            if (settings.SlackNotifications)
            {
                await SendSlackNotificationAsync(notification, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending notification");
        }
    }

    private async Task SendEmailNotificationAsync(ReviewNotification notification, CancellationToken cancellationToken)
    {
        // In production, would integrate with email service
        await Task.Delay(1, cancellationToken);
        _logger.LogDebug("📧 Email notification sent to: {RecipientId}", notification.RecipientId);
    }

    private async Task SendInAppNotificationAsync(ReviewNotification notification, CancellationToken cancellationToken)
    {
        // In production, would send via SignalR or WebSocket
        await Task.Delay(1, cancellationToken);
        _logger.LogDebug("🔔 In-app notification sent to: {RecipientId}", notification.RecipientId);
    }

    private async Task SendSlackNotificationAsync(ReviewNotification notification, CancellationToken cancellationToken)
    {
        // In production, would integrate with Slack API
        await Task.Delay(1, cancellationToken);
        _logger.LogDebug("💬 Slack notification sent to: {RecipientId}", notification.RecipientId);
    }

    private ReviewNotificationSettings GetDefaultNotificationSettings(string userId)
    {
        return new ReviewNotificationSettings
        {
            UserId = userId,
            EmailNotifications = true,
            InAppNotifications = true,
            SlackNotifications = false,
            ReminderInterval = TimeSpan.FromHours(4),
            NotificationTypes = Enum.GetValues<ReviewType>().ToList(),
            NotificationPriorities = Enum.GetValues<ReviewPriority>().ToList(),
            WeekendNotifications = false,
            QuietHoursStart = TimeSpan.FromHours(18),
            QuietHoursEnd = TimeSpan.FromHours(8)
        };
    }

    private void InitializeDefaultSettings()
    {
        // Initialize some default user settings
        var defaultUsers = new[] { "admin@company.com", "user@company.com", "manager@company.com" };
        
        foreach (var user in defaultUsers)
        {
            _userSettings[user] = GetDefaultNotificationSettings(user);
        }

        _logger.LogInformation("✅ Initialized notification settings for {Count} users", defaultUsers.Length);
    }
}
