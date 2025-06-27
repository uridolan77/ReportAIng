using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced time context analyzer that extracts temporal information from queries
/// </summary>
public class TimeContextAnalyzer : ITimeContextAnalyzer
{
    private readonly IAIService _aiService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<TimeContextAnalyzer> _logger;

    // Enhanced time expression patterns with comprehensive relative time support
    private static readonly Dictionary<string, Func<DateTime>> RelativeTimeExpressions = new()
    {
        // Basic relative expressions
        { @"\btoday\b", () => DateTime.Today },
        { @"\byesterday\b", () => DateTime.Today.AddDays(-1) },
        { @"\btomorrow\b", () => DateTime.Today.AddDays(1) },

        // Week expressions
        { @"\bthis week\b", () => GetStartOfWeek(DateTime.Today) },
        { @"\blast week\b", () => GetStartOfWeek(DateTime.Today.AddDays(-7)) },
        { @"\bnext week\b", () => GetStartOfWeek(DateTime.Today.AddDays(7)) },
        { @"\bprevious week\b", () => GetStartOfWeek(DateTime.Today.AddDays(-7)) },

        // Month expressions
        { @"\bthis month\b", () => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) },
        { @"\blast month\b", () => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1) },
        { @"\bnext month\b", () => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1) },
        { @"\bprevious month\b", () => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1) },

        // Quarter expressions
        { @"\bthis quarter\b", () => GetStartOfQuarter(DateTime.Today) },
        { @"\blast quarter\b", () => GetStartOfQuarter(DateTime.Today).AddMonths(-3) },
        { @"\bnext quarter\b", () => GetStartOfQuarter(DateTime.Today).AddMonths(3) },
        { @"\bprevious quarter\b", () => GetStartOfQuarter(DateTime.Today).AddMonths(-3) },
        { @"\bq1\b", () => new DateTime(DateTime.Today.Year, 1, 1) },
        { @"\bq2\b", () => new DateTime(DateTime.Today.Year, 4, 1) },
        { @"\bq3\b", () => new DateTime(DateTime.Today.Year, 7, 1) },
        { @"\bq4\b", () => new DateTime(DateTime.Today.Year, 10, 1) },

        // Year expressions
        { @"\bthis year\b", () => new DateTime(DateTime.Today.Year, 1, 1) },
        { @"\blast year\b", () => new DateTime(DateTime.Today.Year - 1, 1, 1) },
        { @"\bnext year\b", () => new DateTime(DateTime.Today.Year + 1, 1, 1) },
        { @"\bprevious year\b", () => new DateTime(DateTime.Today.Year - 1, 1, 1) },

        // Year-to-date expressions
        { @"\bytd\b", () => new DateTime(DateTime.Today.Year, 1, 1) },
        { @"\byear to date\b", () => new DateTime(DateTime.Today.Year, 1, 1) },
        { @"\bmtd\b", () => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) },
        { @"\bmonth to date\b", () => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) },

        // Recent time expressions
        { @"\blast 7 days\b", () => DateTime.Today.AddDays(-7) },
        { @"\blast 30 days\b", () => DateTime.Today.AddDays(-30) },
        { @"\blast 90 days\b", () => DateTime.Today.AddDays(-90) },
        { @"\bpast week\b", () => DateTime.Today.AddDays(-7) },
        { @"\bpast month\b", () => DateTime.Today.AddDays(-30) },
        { @"\brecent\b", () => DateTime.Today.AddDays(-7) }
    };

    public TimeContextAnalyzer(
        IAIService aiService,
        ICacheService cacheService,
        ILogger<TimeContextAnalyzer> logger)
    {
        _aiService = aiService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TimeRange?> ExtractTimeContextAsync(string userQuestion)
    {
        try
        {
            var cacheKey = $"time_context:{userQuestion.GetHashCode()}";
            var cached = await _cacheService.GetAsync<TimeRange?>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var timeRange = await AnalyzeTimeContextAsync(userQuestion);
            
            await _cacheService.SetAsync(cacheKey, timeRange, TimeSpan.FromMinutes(15));
            
            if (timeRange != null)
            {
                _logger.LogDebug("Extracted time context: {StartDate} to {EndDate}, Granularity: {Granularity}", 
                    timeRange.StartDate, timeRange.EndDate, timeRange.Granularity);
            }

            return timeRange;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting time context from question: {Question}", userQuestion);
            return null;
        }
    }

    private async Task<TimeRange?> AnalyzeTimeContextAsync(string userQuestion)
    {
        // 1. Try pattern-based extraction first
        var patternResult = ExtractPatternBasedTimeContext(userQuestion);
        if (patternResult != null)
        {
            return patternResult;
        }

        // 2. Try AI-based extraction for complex expressions
        var aiResult = await ExtractAIBasedTimeContextAsync(userQuestion);
        if (aiResult != null)
        {
            return aiResult;
        }

        // 3. Check for implicit time context
        var implicitResult = ExtractImplicitTimeContext(userQuestion);
        return implicitResult;
    }

    private TimeRange? ExtractPatternBasedTimeContext(string userQuestion)
    {
        var lowerQuestion = userQuestion.ToLower();

        // 1. Check for numeric relative expressions first (e.g., "last 3 days", "past 2 weeks")
        var numericResult = ExtractNumericTimeExpressions(lowerQuestion);
        if (numericResult != null)
        {
            return numericResult;
        }

        // 2. Check for standard relative time expressions
        foreach (var (pattern, dateFunc) in RelativeTimeExpressions)
        {
            if (Regex.IsMatch(lowerQuestion, pattern, RegexOptions.IgnoreCase))
            {
                var startDate = dateFunc();
                var granularity = DetermineGranularity(pattern);
                var endDate = CalculateEndDate(startDate, granularity);

                _logger.LogDebug("Matched time pattern '{Pattern}': {StartDate} to {EndDate}",
                    pattern, startDate, endDate);

                return new TimeRange
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    RelativeExpression = Regex.Match(lowerQuestion, pattern, RegexOptions.IgnoreCase).Value,
                    Granularity = granularity
                };
            }
        }

        // 3. Check for specific date patterns
        var specificDateResult = ExtractSpecificDatePatterns(userQuestion);
        if (specificDateResult != null)
        {
            return specificDateResult;
        }

        // 4. Check for month/year patterns (e.g., "January 2024", "2023")
        var monthYearResult = ExtractMonthYearPatterns(lowerQuestion);
        if (monthYearResult != null)
        {
            return monthYearResult;
        }

        return null;
    }

    private TimeRange? ExtractNumericTimeExpressions(string lowerQuestion)
    {
        // Pattern for "last/past N days/weeks/months/years"
        var numericPatterns = new (string pattern, Func<int, DateTime> dateFunc, TimeGranularity granularity)[]
        {
            (@"\b(?:last|past)\s+(\d+)\s+days?\b", (int n) => DateTime.Today.AddDays(-n), TimeGranularity.Day),
            (@"\b(?:last|past)\s+(\d+)\s+weeks?\b", (int n) => DateTime.Today.AddDays(-n * 7), TimeGranularity.Week),
            (@"\b(?:last|past)\s+(\d+)\s+months?\b", (int n) => DateTime.Today.AddMonths(-n), TimeGranularity.Month),
            (@"\b(?:last|past)\s+(\d+)\s+years?\b", (int n) => DateTime.Today.AddYears(-n), TimeGranularity.Year),
            (@"\b(?:next)\s+(\d+)\s+days?\b", (int n) => DateTime.Today, TimeGranularity.Day),
            (@"\b(?:next)\s+(\d+)\s+weeks?\b", (int n) => DateTime.Today, TimeGranularity.Week),
            (@"\b(?:next)\s+(\d+)\s+months?\b", (int n) => DateTime.Today, TimeGranularity.Month),
            (@"\b(?:next)\s+(\d+)\s+years?\b", (int n) => DateTime.Today, TimeGranularity.Year)
        };

        foreach (var (pattern, dateFunc, granularity) in numericPatterns)
        {
            var match = Regex.Match(lowerQuestion, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var number))
            {
                var startDate = dateFunc(number);
                var endDate = pattern.Contains("next") ?
                    CalculateEndDateForNext(startDate, granularity, number) :
                    DateTime.Today;

                _logger.LogDebug("Matched numeric pattern '{Pattern}' with number {Number}: {StartDate} to {EndDate}",
                    pattern, number, startDate, endDate);

                return new TimeRange
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    RelativeExpression = match.Value,
                    Granularity = granularity
                };
            }
        }

        return null;
    }

    private TimeRange? ExtractSpecificDatePatterns(string userQuestion)
    {
        var datePatterns = new[]
        {
            @"\b(\d{4}-\d{1,2}-\d{1,2})\b", // ISO format: 2024-01-15
            @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{4})\b", // US format: 1/15/2024 or 01-15-2024
            @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2})\b", // Short year: 1/15/24
            @"\b(\d{4}[/-]\d{1,2}[/-]\d{1,2})\b" // Alternative ISO: 2024/01/15
        };

        var allDates = new List<DateTime>();

        foreach (var pattern in datePatterns)
        {
            var matches = Regex.Matches(userQuestion, pattern);
            foreach (Match match in matches)
            {
                if (DateTime.TryParse(match.Value, out var date))
                {
                    allDates.Add(date);
                }
            }
        }

        if (allDates.Count > 0)
        {
            allDates.Sort();
            var startDate = allDates.First();
            var endDate = allDates.Count > 1 ? allDates.Last() : startDate.AddDays(1).AddSeconds(-1);

            _logger.LogDebug("Extracted specific dates: {StartDate} to {EndDate}", startDate, endDate);

            return new TimeRange
            {
                StartDate = startDate,
                EndDate = endDate,
                Granularity = TimeGranularity.Day
            };
        }

        return null;
    }

    private TimeRange? ExtractMonthYearPatterns(string lowerQuestion)
    {
        // Pattern for "January 2024", "Jan 2024", "2024"
        var monthYearPattern = @"\b(january|february|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)\s+(\d{4})\b";
        var yearOnlyPattern = @"\b(\d{4})\b";

        var monthYearMatch = Regex.Match(lowerQuestion, monthYearPattern, RegexOptions.IgnoreCase);
        if (monthYearMatch.Success)
        {
            var monthStr = monthYearMatch.Groups[1].Value;
            var yearStr = monthYearMatch.Groups[2].Value;

            if (int.TryParse(yearStr, out var year) && TryParseMonth(monthStr, out var month))
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                return new TimeRange
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    RelativeExpression = monthYearMatch.Value,
                    Granularity = TimeGranularity.Month
                };
            }
        }

        var yearMatch = Regex.Match(lowerQuestion, yearOnlyPattern);
        if (yearMatch.Success && int.TryParse(yearMatch.Value, out var yearOnly) && yearOnly >= 2000 && yearOnly <= 2030)
        {
            var startDate = new DateTime(yearOnly, 1, 1);
            var endDate = new DateTime(yearOnly, 12, 31);

            return new TimeRange
            {
                StartDate = startDate,
                EndDate = endDate,
                RelativeExpression = yearMatch.Value,
                Granularity = TimeGranularity.Year
            };
        }

        return null;
    }

    private async Task<TimeRange?> ExtractAIBasedTimeContextAsync(string userQuestion)
    {
        try
        {
            var prompt = $@"
Extract time context from this question. Look for:
- Specific dates
- Relative time expressions (last month, year to date, etc.)
- Time comparisons
- Time granularity

Question: {userQuestion}

If no time context found, return null.
Otherwise return JSON:
{{
    ""startDate"": ""<ISO date or null>"",
    ""endDate"": ""<ISO date or null>"",
    ""relativeExpression"": ""<expression or empty>"",
    ""granularity"": ""<Hour|Day|Week|Month|Quarter|Year|Unknown>""
}}";

            var response = await _aiService.GenerateSQLAsync(prompt);
            
            if (string.IsNullOrWhiteSpace(response) || response.Trim().ToLower() == "null")
                return null;

            return ParseTimeRangeFromJson(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI-based time context extraction failed");
            return null;
        }
    }

    private TimeRange? ExtractImplicitTimeContext(string userQuestion)
    {
        var lowerQuestion = userQuestion.ToLower();

        // Check for words that imply recent time context
        if (Regex.IsMatch(lowerQuestion, @"\b(recent|latest|current|now|trending)\b"))
        {
            return new TimeRange
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today,
                RelativeExpression = "recent",
                Granularity = TimeGranularity.Day
            };
        }

        // Check for business-specific time contexts
        if (Regex.IsMatch(lowerQuestion, @"\b(daily|day|per day)\b"))
        {
            return new TimeRange
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1).AddSeconds(-1),
                RelativeExpression = "daily",
                Granularity = TimeGranularity.Day
            };
        }

        if (Regex.IsMatch(lowerQuestion, @"\b(weekly|week|per week)\b"))
        {
            return new TimeRange
            {
                StartDate = GetStartOfWeek(DateTime.Today),
                EndDate = GetStartOfWeek(DateTime.Today).AddDays(6),
                RelativeExpression = "weekly",
                Granularity = TimeGranularity.Week
            };
        }

        if (Regex.IsMatch(lowerQuestion, @"\b(monthly|month|per month)\b"))
        {
            return new TimeRange
            {
                StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                EndDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1),
                RelativeExpression = "monthly",
                Granularity = TimeGranularity.Month
            };
        }

        // Check for performance/analytics contexts that typically need recent data
        if (Regex.IsMatch(lowerQuestion, @"\b(performance|analytics|metrics|kpi|dashboard)\b"))
        {
            return new TimeRange
            {
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today,
                RelativeExpression = "performance_context",
                Granularity = TimeGranularity.Day
            };
        }

        return null;
    }

    private TimeRange? ParseTimeRangeFromJson(string jsonResponse)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<JsonTimeRange>(jsonResponse, options);
            
            if (result == null) return null;

            var timeRange = new TimeRange
            {
                RelativeExpression = result.RelativeExpression ?? string.Empty,
                Granularity = Enum.TryParse<TimeGranularity>(result.Granularity, true, out var granularity) 
                    ? granularity : TimeGranularity.Unknown
            };

            if (DateTime.TryParse(result.StartDate, out var startDate))
                timeRange.StartDate = startDate;

            if (DateTime.TryParse(result.EndDate, out var endDate))
                timeRange.EndDate = endDate;

            return timeRange;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse time range JSON: {Json}", jsonResponse);
            return null;
        }
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private static DateTime GetStartOfQuarter(DateTime date)
    {
        var quarterNumber = (date.Month - 1) / 3 + 1;
        return new DateTime(date.Year, (quarterNumber - 1) * 3 + 1, 1);
    }

    private static TimeGranularity DetermineGranularity(string pattern)
    {
        return pattern.ToLower() switch
        {
            var p when p.Contains("year") => TimeGranularity.Year,
            var p when p.Contains("quarter") => TimeGranularity.Quarter,
            var p when p.Contains("month") => TimeGranularity.Month,
            var p when p.Contains("week") => TimeGranularity.Week,
            var p when p.Contains("day") || p.Contains("today") || p.Contains("yesterday") => TimeGranularity.Day,
            _ => TimeGranularity.Day
        };
    }

    private static DateTime CalculateEndDate(DateTime startDate, TimeGranularity granularity)
    {
        return granularity switch
        {
            TimeGranularity.Year => startDate.AddYears(1).AddDays(-1),
            TimeGranularity.Quarter => startDate.AddMonths(3).AddDays(-1),
            TimeGranularity.Month => startDate.AddMonths(1).AddDays(-1),
            TimeGranularity.Week => startDate.AddDays(6),
            TimeGranularity.Day => startDate.AddDays(1).AddSeconds(-1),
            _ => startDate.AddDays(1).AddSeconds(-1)
        };
    }

    private static DateTime CalculateEndDateForNext(DateTime startDate, TimeGranularity granularity, int number)
    {
        return granularity switch
        {
            TimeGranularity.Year => startDate.AddYears(number),
            TimeGranularity.Quarter => startDate.AddMonths(number * 3),
            TimeGranularity.Month => startDate.AddMonths(number),
            TimeGranularity.Week => startDate.AddDays(number * 7),
            TimeGranularity.Day => startDate.AddDays(number),
            _ => startDate.AddDays(number)
        };
    }

    private static bool TryParseMonth(string monthStr, out int month)
    {
        var monthMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "january", 1 }, { "jan", 1 },
            { "february", 2 }, { "feb", 2 },
            { "march", 3 }, { "mar", 3 },
            { "april", 4 }, { "apr", 4 },
            { "may", 5 },
            { "june", 6 }, { "jun", 6 },
            { "july", 7 }, { "jul", 7 },
            { "august", 8 }, { "aug", 8 },
            { "september", 9 }, { "sep", 9 },
            { "october", 10 }, { "oct", 10 },
            { "november", 11 }, { "nov", 11 },
            { "december", 12 }, { "dec", 12 }
        };

        return monthMap.TryGetValue(monthStr, out month);
    }

    private record JsonTimeRange(string? StartDate, string? EndDate, string? RelativeExpression, string? Granularity);
}
