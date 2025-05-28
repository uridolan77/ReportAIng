using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Tests.Infrastructure;

/// <summary>
/// Captures log messages during test execution for verification
/// </summary>
public class TestLogCapture : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Type _categoryType;
    private readonly TestLoggerProvider _provider;
    private readonly ConcurrentQueue<LogEntry> _logEntries;

    public TestLogCapture(ILoggerFactory loggerFactory, Type categoryType)
    {
        _loggerFactory = loggerFactory;
        _categoryType = categoryType;
        _logEntries = new ConcurrentQueue<LogEntry>();
        _provider = new TestLoggerProvider(_logEntries);
        
        _loggerFactory.AddProvider(_provider);
    }

    /// <summary>
    /// Get all captured log entries
    /// </summary>
    public IReadOnlyList<LogEntry> LogEntries => _logEntries.ToList();

    /// <summary>
    /// Get log entries for a specific log level
    /// </summary>
    public IReadOnlyList<LogEntry> GetLogEntries(LogLevel logLevel)
    {
        return _logEntries.Where(e => e.LogLevel == logLevel).ToList();
    }

    /// <summary>
    /// Get log entries containing specific text
    /// </summary>
    public IReadOnlyList<LogEntry> GetLogEntries(string messageContains)
    {
        return _logEntries.Where(e => e.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Get log entries for a specific category
    /// </summary>
    public IReadOnlyList<LogEntry> GetLogEntries(Type categoryType)
    {
        return _logEntries.Where(e => e.CategoryName == categoryType.FullName).ToList();
    }

    /// <summary>
    /// Check if any log entry matches the criteria
    /// </summary>
    public bool HasLogEntry(LogLevel logLevel, string messageContains)
    {
        return _logEntries.Any(e => e.LogLevel == logLevel && 
                                   e.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if any error log entries exist
    /// </summary>
    public bool HasErrors => _logEntries.Any(e => e.LogLevel == LogLevel.Error);

    /// <summary>
    /// Check if any warning log entries exist
    /// </summary>
    public bool HasWarnings => _logEntries.Any(e => e.LogLevel == LogLevel.Warning);

    /// <summary>
    /// Get the count of log entries for a specific level
    /// </summary>
    public int GetLogCount(LogLevel logLevel)
    {
        return _logEntries.Count(e => e.LogLevel == logLevel);
    }

    /// <summary>
    /// Clear all captured log entries
    /// </summary>
    public void Clear()
    {
        while (_logEntries.TryDequeue(out _)) { }
    }

    /// <summary>
    /// Wait for a specific log entry to appear
    /// </summary>
    public async Task<bool> WaitForLogEntryAsync(LogLevel logLevel, string messageContains, TimeSpan timeout)
    {
        var endTime = DateTime.UtcNow.Add(timeout);
        
        while (DateTime.UtcNow < endTime)
        {
            if (HasLogEntry(logLevel, messageContains))
                return true;
                
            await Task.Delay(50);
        }
        
        return false;
    }

    /// <summary>
    /// Assert that a specific log entry exists
    /// </summary>
    public void AssertLogEntry(LogLevel logLevel, string messageContains)
    {
        if (!HasLogEntry(logLevel, messageContains))
        {
            var allMessages = string.Join("\n", _logEntries.Select(e => $"[{e.LogLevel}] {e.Message}"));
            throw new Xunit.Sdk.XunitException(
                $"Expected log entry with level {logLevel} containing '{messageContains}' was not found.\n" +
                $"Actual log entries:\n{allMessages}");
        }
    }

    /// <summary>
    /// Assert that no error log entries exist
    /// </summary>
    public void AssertNoErrors()
    {
        var errors = GetLogEntries(LogLevel.Error);
        if (errors.Any())
        {
            var errorMessages = string.Join("\n", errors.Select(e => e.Message));
            throw new Xunit.Sdk.XunitException($"Expected no errors, but found:\n{errorMessages}");
        }
    }

    /// <summary>
    /// Assert that no warning log entries exist
    /// </summary>
    public void AssertNoWarnings()
    {
        var warnings = GetLogEntries(LogLevel.Warning);
        if (warnings.Any())
        {
            var warningMessages = string.Join("\n", warnings.Select(e => e.Message));
            throw new Xunit.Sdk.XunitException($"Expected no warnings, but found:\n{warningMessages}");
        }
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
        _provider.Dispose();
    }
}

/// <summary>
/// Represents a captured log entry
/// </summary>
public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Scopes { get; set; } = new();
}

/// <summary>
/// Test logger provider that captures log entries
/// </summary>
internal class TestLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> _logEntries;
    private readonly ConcurrentDictionary<string, TestLogger> _loggers;

    public TestLoggerProvider(ConcurrentQueue<LogEntry> logEntries)
    {
        _logEntries = logEntries;
        _loggers = new ConcurrentDictionary<string, TestLogger>();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new TestLogger(name, _logEntries));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

/// <summary>
/// Test logger that captures log entries
/// </summary>
internal class TestLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ConcurrentQueue<LogEntry> _logEntries;

    public TestLogger(string categoryName, ConcurrentQueue<LogEntry> logEntries)
    {
        _categoryName = categoryName;
        _logEntries = logEntries;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return new TestLogScope();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true; // Capture all log levels for testing
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        
        var logEntry = new LogEntry
        {
            LogLevel = logLevel,
            EventId = eventId,
            CategoryName = _categoryName,
            Message = message,
            Exception = exception,
            Timestamp = DateTime.UtcNow
        };

        // Capture structured logging data if available
        if (state is IEnumerable<KeyValuePair<string, object>> structuredState)
        {
            foreach (var kvp in structuredState)
            {
                if (kvp.Key != "{OriginalFormat}")
                {
                    logEntry.Scopes[kvp.Key] = kvp.Value;
                }
            }
        }

        _logEntries.Enqueue(logEntry);
    }
}

/// <summary>
/// Simple log scope implementation for testing
/// </summary>
internal class TestLogScope : IDisposable
{
    public void Dispose()
    {
        // No-op for testing
    }
}

/// <summary>
/// Extension methods for easier log verification in tests
/// </summary>
public static class TestLogCaptureExtensions
{
    /// <summary>
    /// Assert that a specific number of log entries exist for a level
    /// </summary>
    public static void AssertLogCount(this TestLogCapture capture, LogLevel logLevel, int expectedCount)
    {
        var actualCount = capture.GetLogCount(logLevel);
        if (actualCount != expectedCount)
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected {expectedCount} log entries with level {logLevel}, but found {actualCount}");
        }
    }

    /// <summary>
    /// Assert that log entries exist in a specific order
    /// </summary>
    public static void AssertLogOrder(this TestLogCapture capture, params (LogLevel level, string messageContains)[] expectedOrder)
    {
        var allEntries = capture.LogEntries.ToList();
        var matchingEntries = new List<LogEntry>();

        foreach (var (level, messageContains) in expectedOrder)
        {
            var entry = allEntries.FirstOrDefault(e => e.LogLevel == level && 
                                                      e.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Expected log entry with level {level} containing '{messageContains}' was not found");
            }
            matchingEntries.Add(entry);
        }

        // Verify order
        for (int i = 1; i < matchingEntries.Count; i++)
        {
            if (matchingEntries[i].Timestamp < matchingEntries[i - 1].Timestamp)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Log entries are not in expected order. Entry '{matchingEntries[i].Message}' " +
                    $"should come after '{matchingEntries[i - 1].Message}'");
            }
        }
    }

    /// <summary>
    /// Get log entries within a specific time range
    /// </summary>
    public static IReadOnlyList<LogEntry> GetLogEntriesInRange(this TestLogCapture capture, DateTime start, DateTime end)
    {
        return capture.LogEntries.Where(e => e.Timestamp >= start && e.Timestamp <= end).ToList();
    }

    /// <summary>
    /// Get the most recent log entry
    /// </summary>
    public static LogEntry? GetMostRecentLogEntry(this TestLogCapture capture)
    {
        return capture.LogEntries.OrderByDescending(e => e.Timestamp).FirstOrDefault();
    }

    /// <summary>
    /// Get the most recent log entry for a specific level
    /// </summary>
    public static LogEntry? GetMostRecentLogEntry(this TestLogCapture capture, LogLevel logLevel)
    {
        return capture.LogEntries
            .Where(e => e.LogLevel == logLevel)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefault();
    }
}
