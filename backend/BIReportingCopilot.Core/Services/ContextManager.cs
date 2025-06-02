using BIReportingCopilot.Core.Models.ML;

namespace BIReportingCopilot.Core.Services;

/// <summary>
/// Manages query execution context and user session information
/// </summary>
public class ContextManager
{
    private readonly Dictionary<string, QueryExecutionContext> _activeContexts = new();
    private readonly Dictionary<string, List<QueryExecutionContext>> _userHistory = new();
    private readonly object _lock = new();

    /// <summary>
    /// Create a new query execution context
    /// </summary>
    public QueryExecutionContext CreateContext(string userId, string sessionId, string databaseName)
    {
        var context = new QueryExecutionContext
        {
            QueryId = Guid.NewGuid().ToString(),
            UserId = userId,
            SessionId = sessionId,
            StartTime = DateTime.UtcNow,
            DatabaseName = databaseName
        };

        lock (_lock)
        {
            _activeContexts[context.QueryId] = context;
            
            if (!_userHistory.ContainsKey(userId))
            {
                _userHistory[userId] = new List<QueryExecutionContext>();
            }
            _userHistory[userId].Add(context);
        }

        return context;
    }

    /// <summary>
    /// Get an active context by query ID
    /// </summary>
    public QueryExecutionContext? GetContext(string queryId)
    {
        lock (_lock)
        {
            return _activeContexts.TryGetValue(queryId, out var context) ? context : null;
        }
    }

    /// <summary>
    /// Update context with execution results
    /// </summary>
    public void UpdateContext(string queryId, Action<QueryExecutionContext> updateAction)
    {
        lock (_lock)
        {
            if (_activeContexts.TryGetValue(queryId, out var context))
            {
                updateAction(context);
            }
        }
    }

    /// <summary>
    /// Complete a query execution context
    /// </summary>
    public void CompleteContext(string queryId)
    {
        lock (_lock)
        {
            if (_activeContexts.TryGetValue(queryId, out var context))
            {
                context.EndTime = DateTime.UtcNow;
                _activeContexts.Remove(queryId);
            }
        }
    }

    /// <summary>
    /// Get user's query history
    /// </summary>
    public List<QueryExecutionContext> GetUserHistory(string userId, int limit = 50)
    {
        lock (_lock)
        {
            if (_userHistory.TryGetValue(userId, out var history))
            {
                return history.OrderByDescending(h => h.StartTime).Take(limit).ToList();
            }
            return new List<QueryExecutionContext>();
        }
    }

    /// <summary>
    /// Get active contexts for a user
    /// </summary>
    public List<QueryExecutionContext> GetActiveContexts(string userId)
    {
        lock (_lock)
        {
            return _activeContexts.Values
                .Where(c => c.UserId == userId)
                .ToList();
        }
    }

    /// <summary>
    /// Clean up old contexts
    /// </summary>
    public void CleanupOldContexts(TimeSpan maxAge)
    {
        var cutoffTime = DateTime.UtcNow - maxAge;
        
        lock (_lock)
        {
            var expiredContexts = _activeContexts.Values
                .Where(c => c.StartTime < cutoffTime)
                .ToList();

            foreach (var context in expiredContexts)
            {
                _activeContexts.Remove(context.QueryId);
            }

            // Also clean up user history
            foreach (var userHistory in _userHistory.Values)
            {
                userHistory.RemoveAll(c => c.StartTime < cutoffTime);
            }
        }
    }

    /// <summary>
    /// Get context statistics
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        lock (_lock)
        {
            return new Dictionary<string, object>
            {
                ["active_contexts"] = _activeContexts.Count,
                ["total_users"] = _userHistory.Count,
                ["total_history_entries"] = _userHistory.Values.Sum(h => h.Count)
            };
        }
    }
}
