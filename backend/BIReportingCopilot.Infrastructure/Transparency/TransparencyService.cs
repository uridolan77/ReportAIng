using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Transparency;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Infrastructure.Data.Entities;
using System.Text.Json;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Transparency
{
    /// <summary>
    /// Service implementation for AI transparency business logic and data aggregation
    /// </summary>
    public class TransparencyService : ITransparencyService
    {
        private readonly ITransparencyRepository _transparencyRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransparencyService> _logger;

        public TransparencyService(
            ITransparencyRepository transparencyRepository,
            IConfiguration configuration,
            ILogger<TransparencyService> logger)
        {
            _transparencyRepository = transparencyRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ConfidenceBreakdown> GetConfidenceBreakdownAsync(string analysisId)
        {
            try
            {
                _logger.LogInformation("Getting confidence breakdown for analysis: {AnalysisId}", analysisId);

                // Try to get trace by ID (analysisId could be traceId)
                var trace = await _transparencyRepository.GetPromptTraceAsync(analysisId);
                if (trace == null)
                {
                    // If not found as trace, try to get business context profile
                    var profile = await _transparencyRepository.GetBusinessContextAsync(analysisId);
                    if (profile == null)
                    {
                        throw new InvalidOperationException($"Analysis {analysisId} not found");
                    }

                    return CreateConfidenceBreakdownFromProfile(profile);
                }

                return await CreateConfidenceBreakdownFromTrace(trace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confidence breakdown for analysis {AnalysisId}", analysisId);
                throw;
            }
        }

        public async Task<List<AlternativeOptionDto>> GetAlternativeOptionsAsync(string traceId)
        {
            try
            {
                _logger.LogInformation("Getting alternative options for trace: {TraceId}", traceId);

                var trace = await _transparencyRepository.GetPromptTraceAsync(traceId);
                if (trace == null)
                {
                    return new List<AlternativeOptionDto>();
                }

                var alternatives = new List<AlternativeOptionDto>();

                // Generate alternatives based on trace data
                if (trace.OverallConfidence < 0.8m)
                {
                    alternatives.Add(new AlternativeOptionDto
                    {
                        OptionId = Guid.NewGuid().ToString(),
                        Type = "Template",
                        Description = "Use more specific prompt template for better accuracy",
                        Score = 0.85,
                        Rationale = "Current confidence is below optimal threshold",
                        EstimatedImprovement = (double)(0.85m - trace.OverallConfidence) * 100
                    });
                }

                if (trace.TotalTokens > 3000)
                {
                    alternatives.Add(new AlternativeOptionDto
                    {
                        OptionId = Guid.NewGuid().ToString(),
                        Type = "Context",
                        Description = "Reduce context size to optimize token usage",
                        Score = 0.78,
                        Rationale = "High token usage detected, optimization possible",
                        EstimatedImprovement = 15.0
                    });
                }

                // Add intent-specific alternatives
                alternatives.AddRange(await GetIntentSpecificAlternatives(trace));

                return alternatives;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alternative options for trace {TraceId}", traceId);
                throw;
            }
        }

        public async Task<List<OptimizationSuggestionDto>> GetOptimizationSuggestionsAsync(string userQuery, string? traceId = null)
        {
            try
            {
                _logger.LogInformation("Getting optimization suggestions for query: {Query}", userQuery.Substring(0, Math.Min(100, userQuery.Length)));

                var suggestions = new List<OptimizationSuggestionDto>();

                if (!string.IsNullOrEmpty(traceId))
                {
                    var trace = await _transparencyRepository.GetPromptTraceAsync(traceId);
                    if (trace != null)
                    {
                        suggestions.AddRange(await GenerateTraceBasedSuggestions(trace));
                    }
                }

                // Add general query-based suggestions
                suggestions.AddRange(GenerateQueryBasedSuggestions(userQuery));

                // Add performance-based suggestions from historical data
                suggestions.AddRange(await GeneratePerformanceBasedSuggestions());

                return suggestions.OrderByDescending(s => s.EstimatedPerformanceGain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimization suggestions");
                throw;
            }
        }

        public async Task<TransparencyMetricsDto> GetTransparencyMetricsAsync(string? userId = null, int days = 7)
        {
            try
            {
                _logger.LogInformation("Getting transparency metrics for user: {UserId}, days: {Days}", userId, days);

                var startDate = DateTime.UtcNow.AddDays(-days);
                var traces = await GetTracesForPeriod(userId, startDate);

                var metrics = new TransparencyMetricsDto
                {
                    TotalAnalyses = traces.Count,
                    AverageConfidence = traces.Any() ? (double)traces.Average(t => t.OverallConfidence) : 0.0,
                    TimeRange = new { from = startDate, to = DateTime.UtcNow }
                };

                // Calculate confidence distribution
                metrics.ConfidenceDistribution = CalculateConfidenceDistribution(traces);

                // Calculate top intent types
                metrics.TopIntentTypes = traces
                    .GroupBy(t => t.IntentType)
                    .ToDictionary(g => g.Key, g => g.Count())
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(5)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Calculate optimization impact (mock for now, would need historical comparison)
                metrics.OptimizationImpact = new Dictionary<string, double>
                {
                    ["Token Savings"] = CalculateTokenSavings(traces),
                    ["Performance Improvement"] = CalculatePerformanceImprovement(traces),
                    ["Accuracy Gain"] = CalculateAccuracyGain(traces)
                };

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transparency metrics");
                throw;
            }
        }

        public async Task<TransparencyDashboardMetricsDto> GetDashboardMetricsAsync(int days = 30)
        {
            try
            {
                _logger.LogInformation("Getting dashboard metrics for {Days} days", days);

                var startDate = DateTime.UtcNow.AddDays(-days);
                var traces = await GetTracesForPeriod(null, startDate);

                var metrics = new TransparencyDashboardMetricsDto
                {
                    TotalTraces = traces.Count,
                    AverageConfidence = traces.Any() ? (double)traces.Average(t => t.OverallConfidence) : 0.0
                };

                // Get top optimizations (would be from actual optimization results)
                metrics.TopOptimizations = await GetTopOptimizations(5);

                // Calculate confidence trends
                metrics.ConfidenceTrends = CalculateConfidenceTrends(traces, days);

                // Calculate usage by model (from metadata)
                metrics.UsageByModel = CalculateModelUsage(traces);

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard metrics");
                throw;
            }
        }

        public async Task<TransparencySettingsDto> GetTransparencySettingsAsync(string userId)
        {
            try
            {
                // For now, return default settings
                // In a full implementation, this would be stored per user
                return new TransparencySettingsDto
                {
                    EnableDetailedLogging = true,
                    ConfidenceThreshold = 0.7,
                    RetentionDays = 30,
                    EnableOptimizationSuggestions = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transparency settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateTransparencySettingsAsync(string userId, TransparencySettingsDto settings)
        {
            try
            {
                _logger.LogInformation("Updating transparency settings for user {UserId}", userId);
                // In a full implementation, this would save to database
                // For now, just log the update
                _logger.LogInformation("Settings updated: DetailedLogging={DetailedLogging}, Threshold={Threshold}", 
                    settings.EnableDetailedLogging, settings.ConfidenceThreshold);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transparency settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<byte[]> ExportTransparencyDataAsync(ExportTransparencyRequest request)
        {
            try
            {
                _logger.LogInformation("Exporting transparency data in format: {Format}", request.Format);

                var traces = await GetTracesForExport(request);
                
                return request.Format.ToLower() switch
                {
                    "json" => ExportAsJson(traces),
                    "csv" => ExportAsCsv(traces),
                    "excel" => ExportAsExcel(traces),
                    _ => throw new ArgumentException($"Unsupported export format: {request.Format}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting transparency data");
                throw;
            }
        }

        public async Task<List<TransparencyTraceDto>> GetRecentTracesAsync(string? userId = null, int limit = 10)
        {
            try
            {
                var traces = userId != null 
                    ? await _transparencyRepository.GetPromptTracesByUserAsync(userId, limit)
                    : await _transparencyRepository.GetRecentTracesAsync(limit);

                return traces.Select(MapToTraceDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent traces");
                throw;
            }
        }

        public async Task<TransparencyTraceDetailDto> GetTraceDetailAsync(string traceId)
        {
            try
            {
                var trace = await _transparencyRepository.GetPromptTraceAsync(traceId);
                if (trace == null)
                {
                    throw new InvalidOperationException($"Trace {traceId} not found");
                }

                var steps = await _transparencyRepository.GetPromptStepsByTraceAsync(traceId);

                return MapToTraceDetailDto(trace, steps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trace detail for {TraceId}", traceId);
                throw;
            }
        }

        public async Task<List<ConfidenceTrendDto>> GetConfidenceTrendsAsync(string? userId = null, int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                var traces = await GetTracesForPeriod(userId, startDate);

                return CalculateConfidenceTrends(traces, days);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confidence trends");
                throw;
            }
        }

        public async Task<TokenUsageAnalyticsDto> GetTokenUsageAnalyticsAsync(string? userId = null, int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                var traces = await GetTracesForPeriod(userId, startDate);

                return new TokenUsageAnalyticsDto
                {
                    TotalTokensUsed = traces.Sum(t => t.TotalTokens),
                    AverageTokensPerQuery = traces.Any() ? traces.Average(t => t.TotalTokens) : 0,
                    TokensByIntentType = traces
                        .GroupBy(t => t.IntentType)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalTokens)),
                    TokenTrends = CalculateTokenTrends(traces, days)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token usage analytics");
                throw;
            }
        }

        public async Task<TransparencyPerformanceDto> GetPerformanceMetricsAsync(int days = 7)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                var traces = await GetTracesForPeriod(null, startDate);

                return new TransparencyPerformanceDto
                {
                    TotalQueries = traces.Count,
                    SuccessRate = traces.Any() ? (double)traces.Count(t => t.Success) / traces.Count : 0.0,
                    AverageProcessingTime = CalculateAverageProcessingTime(traces),
                    PerformanceByIntentType = CalculatePerformanceByIntentType(traces)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<List<PromptConstructionTraceEntity>> GetTracesForPeriod(string? userId, DateTime startDate)
        {
            // This would need to be implemented in the repository
            // For now, get recent traces and filter by date
            var allTraces = userId != null 
                ? await _transparencyRepository.GetPromptTracesByUserAsync(userId, 1000)
                : await _transparencyRepository.GetRecentTracesAsync(1000);

            return allTraces.Where(t => t.CreatedAt >= startDate).ToList();
        }

        private ConfidenceBreakdown CreateConfidenceBreakdownFromProfile(BusinessContextProfileEntity profile)
        {
            return new ConfidenceBreakdown
            {
                AnalysisId = profile.Id,
                OverallConfidence = (double)profile.OverallConfidence,
                FactorBreakdown = new Dictionary<string, double>
                {
                    ["Intent Classification"] = (double)profile.IntentConfidence,
                    ["Domain Understanding"] = (double)profile.DomainConfidence,
                    ["Overall Analysis"] = (double)profile.OverallConfidence
                },
                ConfidenceFactors = new List<ConfidenceFactor>
                {
                    new ConfidenceFactor
                    {
                        FactorName = "Intent Classification",
                        Score = (double)profile.IntentConfidence,
                        Weight = 0.4,
                        Description = $"Confidence in understanding {profile.IntentType} intent"
                    },
                    new ConfidenceFactor
                    {
                        FactorName = "Domain Understanding",
                        Score = (double)profile.DomainConfidence,
                        Weight = 0.3,
                        Description = $"Understanding of {profile.DomainName} domain"
                    }
                },
                Timestamp = profile.CreatedAt
            };
        }

        private async Task<ConfidenceBreakdown> CreateConfidenceBreakdownFromTrace(PromptConstructionTraceEntity trace)
        {
            var steps = await _transparencyRepository.GetPromptStepsByTraceAsync(trace.TraceId);
            
            var factorBreakdown = new Dictionary<string, double>();
            var confidenceFactors = new List<ConfidenceFactor>();

            foreach (var step in steps)
            {
                factorBreakdown[step.StepName] = (double)step.Confidence;
                confidenceFactors.Add(new ConfidenceFactor
                {
                    FactorName = step.StepName,
                    Score = (double)step.Confidence,
                    Weight = 1.0 / steps.Count, // Equal weight for now
                    Description = $"Confidence in {step.StepName} step"
                });
            }

            return new ConfidenceBreakdown
            {
                AnalysisId = trace.TraceId,
                OverallConfidence = (double)trace.OverallConfidence,
                FactorBreakdown = factorBreakdown,
                ConfidenceFactors = confidenceFactors,
                Timestamp = trace.CreatedAt
            };
        }

        private TransparencyTraceDto MapToTraceDto(PromptConstructionTraceEntity trace)
        {
            return new TransparencyTraceDto
            {
                TraceId = trace.TraceId,
                UserId = trace.UserId,
                UserQuestion = trace.UserQuestion,
                IntentType = trace.IntentType,
                OverallConfidence = (double)trace.OverallConfidence,
                TotalTokens = trace.TotalTokens,
                Success = trace.Success,
                CreatedAt = trace.CreatedAt
            };
        }

        private TransparencyTraceDetailDto MapToTraceDetailDto(PromptConstructionTraceEntity trace, List<PromptConstructionStepEntity> steps)
        {
            var detail = new TransparencyTraceDetailDto
            {
                TraceId = trace.TraceId,
                UserId = trace.UserId,
                UserQuestion = trace.UserQuestion,
                IntentType = trace.IntentType,
                OverallConfidence = (double)trace.OverallConfidence,
                TotalTokens = trace.TotalTokens,
                Success = trace.Success,
                CreatedAt = trace.CreatedAt,
                FinalPrompt = trace.FinalPrompt,
                ErrorMessage = trace.ErrorMessage
            };

            detail.Steps = steps.Select(s => new PromptConstructionStepDto
            {
                Id = s.Id,
                StepName = s.StepName,
                StepOrder = s.StepOrder,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Success = s.Success,
                TokensAdded = s.TokensAdded,
                Confidence = (double)s.Confidence,
                Content = s.Content,
                Details = ParseJsonToDictionary(s.Details)
            }).ToList();

            detail.Metadata = ParseJsonToDictionary(trace.Metadata);

            return detail;
        }

        private Dictionary<string, object> ParseJsonToDictionary(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<string, object>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private async Task<List<AlternativeOptionDto>> GetIntentSpecificAlternatives(PromptConstructionTraceEntity trace)
        {
            var alternatives = new List<AlternativeOptionDto>();

            switch (trace.IntentType.ToLower())
            {
                case "analytical":
                    alternatives.Add(new AlternativeOptionDto
                    {
                        OptionId = Guid.NewGuid().ToString(),
                        Type = "Template",
                        Description = "Use statistical analysis template for deeper insights",
                        Score = 0.82,
                        Rationale = "Analytical queries benefit from statistical context",
                        EstimatedImprovement = 12.0
                    });
                    break;

                case "reporting":
                    alternatives.Add(new AlternativeOptionDto
                    {
                        OptionId = Guid.NewGuid().ToString(),
                        Type = "Format",
                        Description = "Use structured reporting template for better formatting",
                        Score = 0.79,
                        Rationale = "Reporting queries need consistent structure",
                        EstimatedImprovement = 8.0
                    });
                    break;
            }

            return alternatives;
        }

        private async Task<List<OptimizationSuggestionDto>> GenerateTraceBasedSuggestions(PromptConstructionTraceEntity trace)
        {
            var suggestions = new List<OptimizationSuggestionDto>();

            if (trace.TotalTokens > 3000)
            {
                suggestions.Add(new OptimizationSuggestionDto
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    Type = "Token Optimization",
                    Title = "Reduce context verbosity",
                    Description = "Remove redundant schema information to save tokens",
                    Priority = "Medium",
                    EstimatedTokenSaving = Math.Max(150, trace.TotalTokens - 2500),
                    EstimatedPerformanceGain = 12.0,
                    Implementation = "Filter out less relevant table columns"
                });
            }

            if (trace.OverallConfidence < 0.8m)
            {
                suggestions.Add(new OptimizationSuggestionDto
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    Type = "Accuracy Improvement",
                    Title = "Add domain-specific examples",
                    Description = "Include more relevant query examples for better context",
                    Priority = "High",
                    EstimatedTokenSaving = -50, // Uses more tokens
                    EstimatedPerformanceGain = 25.0,
                    Implementation = "Add 2-3 similar query examples to the prompt"
                });
            }

            return suggestions;
        }

        private List<OptimizationSuggestionDto> GenerateQueryBasedSuggestions(string userQuery)
        {
            var suggestions = new List<OptimizationSuggestionDto>();

            if (userQuery.Length > 500)
            {
                suggestions.Add(new OptimizationSuggestionDto
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    Type = "Query Simplification",
                    Title = "Simplify query structure",
                    Description = "Break down complex query into simpler components",
                    Priority = "Medium",
                    EstimatedTokenSaving = 100,
                    EstimatedPerformanceGain = 15.0,
                    Implementation = "Split into multiple focused queries"
                });
            }

            return suggestions;
        }

        private async Task<List<OptimizationSuggestionDto>> GeneratePerformanceBasedSuggestions()
        {
            var suggestions = new List<OptimizationSuggestionDto>();

            // Get recent performance data to generate suggestions
            var recentTraces = await _transparencyRepository.GetRecentTracesAsync(100);
            var avgTokens = recentTraces.Any() ? recentTraces.Average(t => t.TotalTokens) : 0;

            if (avgTokens > 2500)
            {
                suggestions.Add(new OptimizationSuggestionDto
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    Type = "System Optimization",
                    Title = "Optimize default token allocation",
                    Description = "System-wide token usage is above optimal threshold",
                    Priority = "High",
                    EstimatedTokenSaving = (int)(avgTokens - 2000),
                    EstimatedPerformanceGain = 20.0,
                    Implementation = "Review and optimize default context templates"
                });
            }

            return suggestions;
        }

        private Dictionary<string, int> CalculateConfidenceDistribution(List<PromptConstructionTraceEntity> traces)
        {
            var distribution = new Dictionary<string, int>
            {
                ["High (>0.8)"] = 0,
                ["Medium (0.6-0.8)"] = 0,
                ["Low (<0.6)"] = 0
            };

            foreach (var trace in traces)
            {
                var confidence = (double)trace.OverallConfidence;
                if (confidence > 0.8)
                    distribution["High (>0.8)"]++;
                else if (confidence >= 0.6)
                    distribution["Medium (0.6-0.8)"]++;
                else
                    distribution["Low (<0.6)"]++;
            }

            return distribution;
        }

        private double CalculateTokenSavings(List<PromptConstructionTraceEntity> traces)
        {
            // Mock calculation - in real implementation, compare with baseline
            return traces.Any() ? Math.Max(0, 3000 - traces.Average(t => t.TotalTokens)) / 30.0 : 0.0;
        }

        private double CalculatePerformanceImprovement(List<PromptConstructionTraceEntity> traces)
        {
            // Mock calculation - in real implementation, compare processing times
            var successRate = traces.Any() ? (double)traces.Count(t => t.Success) / traces.Count : 0.0;
            return successRate * 20.0; // Convert to percentage improvement
        }

        private double CalculateAccuracyGain(List<PromptConstructionTraceEntity> traces)
        {
            // Mock calculation - in real implementation, compare with user feedback
            return traces.Any() ? (double)traces.Average(t => t.OverallConfidence) * 15.0 : 0.0;
        }

        private async Task<List<OptimizationSuggestionDto>> GetTopOptimizations(int count)
        {
            // In a real implementation, this would get actual optimization results
            return new List<OptimizationSuggestionDto>
            {
                new OptimizationSuggestionDto
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    Type = "Token Optimization",
                    Title = "Reduce schema verbosity",
                    Description = "Optimize schema context for better performance",
                    Priority = "High",
                    EstimatedTokenSaving = 200,
                    EstimatedPerformanceGain = 18.5,
                    Implementation = "Use selective schema inclusion"
                }
            }.Take(count).ToList();
        }

        private List<ConfidenceTrendDto> CalculateConfidenceTrends(List<PromptConstructionTraceEntity> traces, int days)
        {
            var trends = new List<ConfidenceTrendDto>();
            var startDate = DateTime.UtcNow.AddDays(-days);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayTraces = traces.Where(t => t.CreatedAt.Date == date.Date).ToList();

                trends.Add(new ConfidenceTrendDto
                {
                    Date = date,
                    Confidence = dayTraces.Any() ? (double)dayTraces.Average(t => t.OverallConfidence) : 0.0,
                    TraceCount = dayTraces.Count
                });
            }

            return trends;
        }

        private List<ModelUsageDto> CalculateModelUsage(List<PromptConstructionTraceEntity> traces)
        {
            // Extract model info from metadata
            var modelUsage = new Dictionary<string, List<PromptConstructionTraceEntity>>();

            foreach (var trace in traces)
            {
                var model = "gpt-4"; // Default, would parse from metadata
                if (!string.IsNullOrEmpty(trace.Metadata))
                {
                    try
                    {
                        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(trace.Metadata);
                        if (metadata?.ContainsKey("model") == true)
                        {
                            model = metadata["model"].ToString() ?? "gpt-4";
                        }
                    }
                    catch
                    {
                        // Use default
                    }
                }

                if (!modelUsage.ContainsKey(model))
                    modelUsage[model] = new List<PromptConstructionTraceEntity>();

                modelUsage[model].Add(trace);
            }

            return modelUsage.Select(kvp => new ModelUsageDto
            {
                Model = kvp.Key,
                Count = kvp.Value.Count,
                AverageConfidence = kvp.Value.Any() ? (double)kvp.Value.Average(t => t.OverallConfidence) : 0.0,
                TotalTokens = kvp.Value.Sum(t => t.TotalTokens)
            }).ToList();
        }

        private async Task<List<PromptConstructionTraceEntity>> GetTracesForExport(ExportTransparencyRequest request)
        {
            var traces = new List<PromptConstructionTraceEntity>();

            if (request.TraceIds?.Any() == true)
            {
                foreach (var traceId in request.TraceIds)
                {
                    var trace = await _transparencyRepository.GetPromptTraceAsync(traceId);
                    if (trace != null)
                        traces.Add(trace);
                }
            }
            else
            {
                // Get traces by date range and user
                var allTraces = request.UserId != null
                    ? await _transparencyRepository.GetPromptTracesByUserAsync(request.UserId, 1000)
                    : await _transparencyRepository.GetRecentTracesAsync(1000);

                traces = allTraces.Where(t =>
                    (request.StartDate == null || t.CreatedAt >= request.StartDate) &&
                    (request.EndDate == null || t.CreatedAt <= request.EndDate)
                ).ToList();
            }

            return traces;
        }

        private byte[] ExportAsJson(List<PromptConstructionTraceEntity> traces)
        {
            var data = traces.Select(MapToTraceDto).ToList();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            return Encoding.UTF8.GetBytes(json);
        }

        private byte[] ExportAsCsv(List<PromptConstructionTraceEntity> traces)
        {
            var csv = new StringBuilder();
            csv.AppendLine("TraceId,UserId,UserQuestion,IntentType,OverallConfidence,TotalTokens,Success,CreatedAt");

            foreach (var trace in traces)
            {
                csv.AppendLine($"{trace.TraceId},{trace.UserId},\"{trace.UserQuestion.Replace("\"", "\"\"")}\",{trace.IntentType},{trace.OverallConfidence},{trace.TotalTokens},{trace.Success},{trace.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private byte[] ExportAsExcel(List<PromptConstructionTraceEntity> traces)
        {
            // For now, return CSV format
            // In a full implementation, would use a library like EPPlus to create Excel files
            return ExportAsCsv(traces);
        }

        private List<TokenTrendDto> CalculateTokenTrends(List<PromptConstructionTraceEntity> traces, int days)
        {
            var trends = new List<TokenTrendDto>();
            var startDate = DateTime.UtcNow.AddDays(-days);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayTraces = traces.Where(t => t.CreatedAt.Date == date.Date).ToList();

                trends.Add(new TokenTrendDto
                {
                    Date = date,
                    TokensUsed = dayTraces.Sum(t => t.TotalTokens),
                    QueryCount = dayTraces.Count
                });
            }

            return trends;
        }

        private double CalculateAverageProcessingTime(List<PromptConstructionTraceEntity> traces)
        {
            // Calculate from start/end times
            var processingTimes = traces
                .Where(t => t.EndTime.HasValue)
                .Select(t => (t.EndTime!.Value - t.StartTime).TotalMilliseconds)
                .ToList();

            return processingTimes.Any() ? processingTimes.Average() : 0.0;
        }

        private Dictionary<string, double> CalculatePerformanceByIntentType(List<PromptConstructionTraceEntity> traces)
        {
            return traces
                .Where(t => t.EndTime.HasValue)
                .GroupBy(t => t.IntentType)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(t => (t.EndTime!.Value - t.StartTime).TotalMilliseconds)
                );
        }

        #endregion
    }
}
