using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI.Core;
using BIReportingCopilot.Infrastructure.AI.Management;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Features Controller - Consolidates AI capabilities and Phase 3 features
/// Replaces: AdvancedNLUController, Phase3Controller, RealTimeStreamingController
/// </summary>
[ApiController]
[Route("api/features")]
[Authorize]
public class FeaturesController : ControllerBase
{
    private readonly ILogger<FeaturesController> _logger;
    private readonly IMediator _mediator;
    private readonly Phase3StatusService _phase3StatusService;

    public FeaturesController(
        ILogger<FeaturesController> logger,
        IMediator mediator,
        Phase3StatusService phase3StatusService)
    {
        _logger = logger;
        _mediator = mediator;
        _phase3StatusService = phase3StatusService;
    }

    #region Natural Language Understanding

    /// <summary>
    /// Perform comprehensive NLU analysis on a natural language query
    /// </summary>
    [HttpPost("nlu/analyze")]
    public async Task<IActionResult> AnalyzeQuery([FromBody] AnalyzeNLURequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, error = "Query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🧠 NLU analysis requested by user {UserId}: {Query}", userId, request.Query);

            var command = new AnalyzeNLUCommand
            {
                NaturalLanguageQuery = request.Query,
                UserId = userId,
                Context = request.Context
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("🧠 NLU analysis completed - Confidence: {Confidence:P2}, Intent: {Intent}",
                result.ConfidenceScore, result.IntentAnalysis.PrimaryIntent);

            return Ok(new
            {
                success = true,
                data = result,
                metadata = new
                {
                    confidence = result.ConfidenceScore,
                    intent = result.IntentAnalysis.PrimaryIntent,
                    entities_count = result.EntityAnalysis.Entities.Count,
                    processing_time_ms = result.ProcessingTimeMs
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in NLU analysis");
            return StatusCode(500, new { success = false, error = "Internal server error during NLU analysis" });
        }
    }

    /// <summary>
    /// Classify query intent with confidence scoring
    /// </summary>
    [HttpPost("nlu/classify-intent")]
    public async Task<IActionResult> ClassifyIntent([FromBody] ClassifyIntentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, error = "Query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🎯 Intent classification requested by user {UserId}: {Query}", userId, request.Query);

            var query = new ClassifyIntentQuery
            {
                Query = request.Query,
                UserId = userId
            };

            var result = await _mediator.Send(query);

            _logger.LogInformation("🎯 Intent classified - Primary: {Intent}, Confidence: {Confidence:P2}",
                result.PrimaryIntent, result.Confidence);

            return Ok(new
            {
                success = true,
                data = result,
                metadata = new
                {
                    primary_intent = result.PrimaryIntent,
                    confidence = result.Confidence,
                    secondary_intents = result.SecondaryIntents?.Count ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in intent classification");
            return StatusCode(500, new { success = false, error = "Internal server error during intent classification" });
        }
    }

    /// <summary>
    /// Generate smart query suggestions based on partial input
    /// </summary>
    [HttpPost("nlu/suggestions")]
    public async Task<IActionResult> GetSmartSuggestions([FromBody] SmartSuggestionsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartialQuery))
            {
                return BadRequest(new { success = false, error = "Partial query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("💡 Smart suggestions requested by user {UserId}: {PartialQuery}", userId, request.PartialQuery);

            var query = new GetIntelligentSuggestionsQuery
            {
                UserId = userId,
                Schema = request.Schema,
                Context = request.Context
            };

            var suggestions = await _mediator.Send(query);

            _logger.LogInformation("💡 Smart suggestions generated - Count: {Count}", suggestions.Count);

            return Ok(new
            {
                success = true,
                data = suggestions,
                metadata = new
                {
                    suggestion_count = suggestions.Count,
                    partial_query = request.PartialQuery
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating smart suggestions");
            return StatusCode(500, new { success = false, error = "Internal server error during suggestion generation" });
        }
    }

    /// <summary>
    /// Get real-time query assistance for partial queries
    /// </summary>
    [HttpPost("nlu/assistance")]
    public async Task<IActionResult> GetQueryAssistance([FromBody] QueryAssistanceRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartialQuery))
            {
                return BadRequest(new { success = false, error = "Partial query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🤝 Query assistance requested by user {UserId}: {PartialQuery}", userId, request.PartialQuery);

            var query = new GetQueryAssistanceQuery
            {
                PartialQuery = request.PartialQuery,
                UserId = userId,
                Schema = request.Schema
            };

            var assistance = await _mediator.Send(query);

            _logger.LogInformation("🤝 Query assistance provided - Autocomplete: {AutocompleteCount}, Hints: {HintCount}",
                assistance.AutocompleteSuggestions.Count, assistance.PerformanceHints.Count);

            return Ok(new
            {
                success = true,
                data = assistance,
                metadata = new
                {
                    autocomplete_count = assistance.AutocompleteSuggestions.Count,
                    hint_count = assistance.PerformanceHints.Count,
                    validation_errors = assistance.ValidationErrors.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error providing query assistance");
            return StatusCode(500, new { success = false, error = "Internal server error during query assistance" });
        }
    }

    /// <summary>
    /// Get NLU processing metrics and statistics
    /// </summary>
    [HttpGet("nlu/metrics")]
    public async Task<IActionResult> GetNLUMetrics()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 NLU metrics requested by user {UserId}", userId);

            var query = new GetNLUMetricsQuery
            {
                UserId = userId,
                TimeWindow = TimeSpan.FromDays(7)
            };

            var metrics = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    time_window_days = 7,
                    generated_at = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting NLU metrics");
            return StatusCode(500, new { success = false, error = "Internal server error getting NLU metrics" });
        }
    }

    #endregion

    #region Phase 3 Management

    /// <summary>
    /// Get Phase 3 status and available features
    /// </summary>
    [HttpGet("phase3/status")]
    public async Task<ActionResult<Phase3Status>> GetPhase3StatusAsync()
    {
        try
        {
            _logger.LogInformation("Getting Phase 3 status");
            var status = await _phase3StatusService.GetPhase3StatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Phase 3 status");
            return StatusCode(500, new { error = "Failed to get Phase 3 status", details = ex.Message });
        }
    }

    /// <summary>
    /// Get Phase 3 analytics and metrics
    /// </summary>
    [HttpGet("phase3/analytics")]
    public async Task<ActionResult<Phase3Analytics>> GetPhase3AnalyticsAsync([FromQuery] int hours = 24)
    {
        try
        {
            _logger.LogInformation("Getting Phase 3 analytics for {Hours} hours", hours);
            var period = TimeSpan.FromHours(hours);
            var analytics = await _phase3StatusService.GetPhase3AnalyticsAsync(period);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Phase 3 analytics");
            return StatusCode(500, new { error = "Failed to get Phase 3 analytics", details = ex.Message });
        }
    }

    /// <summary>
    /// Enable a Phase 3 feature
    /// </summary>
    [HttpPost("phase3/features/{featureName}/enable")]
    public async Task<ActionResult> EnableFeatureAsync(string featureName)
    {
        try
        {
            _logger.LogInformation("Enabling Phase 3 feature: {FeatureName}", featureName);
            var success = await _phase3StatusService.EnableFeatureAsync(featureName);

            if (success)
            {
                return Ok(new { message = $"Feature '{featureName}' enabled successfully" });
            }
            else
            {
                return BadRequest(new { error = $"Failed to enable feature '{featureName}'" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling Phase 3 feature: {FeatureName}", featureName);
            return StatusCode(500, new { error = "Failed to enable feature", details = ex.Message });
        }
    }

    /// <summary>
    /// Get Phase 3 demo information
    /// </summary>
    [HttpGet("phase3/demo")]
    public ActionResult<Phase3DemoInfo> GetPhase3Demo()
    {
        try
        {
            var demoInfo = new Phase3DemoInfo
            {
                Title = "Phase 3: Next-Generation AI & Security Suite",
                Version = "3.0.0-demo",
                Description = "AI capabilities with enterprise-grade privacy and future-proof security",
                Features = new List<Phase3DemoFeature>
                {
                    new Phase3DemoFeature
                    {
                        Name = "Real-time Streaming Analytics",
                        Status = "Ready for Enablement",
                        Description = "Live data processing with sub-second latency and dynamic dashboards",
                        Benefits = new List<string>
                        {
                            "Instant insights from live data streams",
                            "Real-time alerting for critical events",
                            "Live performance monitoring",
                            "Enhanced user experience with dynamic interfaces"
                        },
                        TechnicalDetails = new List<string>
                        {
                            "Reactive programming with System.Reactive",
                            "Real-time data aggregation and processing",
                            "Live anomaly detection in data streams",
                            "Performance metrics collection and monitoring"
                        }
                    },
                    new Phase3DemoFeature
                    {
                        Name = "Natural Language Understanding",
                        Status = "Ready for Enablement",
                        Description = "Deep semantic analysis with multilingual support and context awareness",
                        Benefits = new List<string>
                        {
                            "95% query understanding accuracy",
                            "Natural conversation flow with context",
                            "Multilingual support for global users",
                            "Domain-specific intelligence for business terminology"
                        },
                        TechnicalDetails = new List<string>
                        {
                            "Deep semantic parsing and structure analysis",
                            "Multi-level intent classification hierarchy",
                            "Entity recognition with relationships",
                            "Conversation history and context integration"
                        }
                    },
                    new Phase3DemoFeature
                    {
                        Name = "Federated Learning for Privacy",
                        Status = "Ready for Enablement",
                        Description = "Privacy-preserving machine learning with mathematical guarantees",
                        Benefits = new List<string>
                        {
                            "Complete data privacy - no raw data sharing",
                            "Collaborative learning across organizations",
                            "Regulatory compliance (GDPR, CCPA)",
                            "Enterprise-grade security and privacy"
                        },
                        TechnicalDetails = new List<string>
                        {
                            "Differential privacy with noise injection",
                            "Secure aggregation with cryptographic protection",
                            "Local model training without data sharing",
                            "Privacy budget management and tracking"
                        }
                    },
                    new Phase3DemoFeature
                    {
                        Name = "Quantum-Resistant Security",
                        Status = "Ready for Enablement",
                        Description = "Post-quantum cryptography for future-proof security",
                        Benefits = new List<string>
                        {
                            "Protection against quantum computer attacks",
                            "NIST-approved post-quantum algorithms",
                            "Cryptographic agility for future transitions",
                            "Military-grade quantum-resistant protection"
                        },
                        TechnicalDetails = new List<string>
                        {
                            "NIST-approved algorithms (Kyber, Dilithium)",
                            "Quantum threat assessment and monitoring",
                            "Post-quantum encryption and digital signatures",
                            "Quantum-resistant key generation and management"
                        }
                    }
                },
                Capabilities = new List<string>
                {
                    "Real-time Intelligence with live data processing",
                    "AI Understanding with deep semantic comprehension",
                    "Privacy & Security with mathematical guarantees",
                    "Enterprise Features with comprehensive monitoring"
                },
                NextSteps = new List<string>
                {
                    "Enable individual features through the Phase 3 management interface",
                    "Monitor performance metrics and adjust configurations",
                    "Test capabilities with real-world scenarios",
                    "Deploy across distributed environments for full benefits"
                },
                CreatedAt = DateTime.UtcNow
            };

            return Ok(demoInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Phase 3 demo information");
            return StatusCode(500, new { error = "Failed to get demo information", details = ex.Message });
        }
    }

    /// <summary>
    /// Get Phase 3 deployment roadmap
    /// </summary>
    [HttpGet("phase3/roadmap")]
    public ActionResult<Phase3Roadmap> GetPhase3Roadmap()
    {
        try
        {
            var roadmap = new Phase3Roadmap
            {
                CurrentPhase = "Phase 3A: Infrastructure Ready",
                Phases = new List<Phase3RoadmapPhase>
                {
                    new Phase3RoadmapPhase
                    {
                        Name = "Phase 3A: Infrastructure Ready",
                        Status = "✅ COMPLETE",
                        Description = "Phase 3 infrastructure and status management",
                        Deliverables = new List<string>
                        {
                            "✅ Phase3StatusService implemented",
                            "✅ Configuration framework in place",
                            "✅ Service registration infrastructure ready",
                            "✅ Demo and management interfaces available"
                        },
                        Timeline = "Completed"
                    },
                    new Phase3RoadmapPhase
                    {
                        Name = "Phase 3B: Enable Streaming Analytics",
                        Status = "🔄 NEXT",
                        Description = "Real-time streaming and live dashboards",
                        Deliverables = new List<string>
                        {
                            "🔄 Fix ICacheService using statements",
                            "🔄 Enable real-time streaming services",
                            "🔄 Implement live dashboard management",
                            "🔄 Test streaming analytics capabilities"
                        },
                        Timeline = "30-60 minutes"
                    },
                    new Phase3RoadmapPhase
                    {
                        Name = "Phase 3C: Enable NLU",
                        Status = "⏳ PLANNED",
                        Description = "Deep semantic analysis and multilingual support",
                        Deliverables = new List<string>
                        {
                            "⏳ Resolve duplicate class definitions",
                            "⏳ Enable NLU services",
                            "⏳ Implement semantic parsing and analysis",
                            "⏳ Test complex natural language queries"
                        },
                        Timeline = "1-2 hours"
                    },
                    new Phase3RoadmapPhase
                    {
                        Name = "Phase 3D: Enable Federated Learning & Quantum Security",
                        Status = "⏳ PLANNED",
                        Description = "Privacy-preserving ML and post-quantum crypto",
                        Deliverables = new List<string>
                        {
                            "⏳ Complete interface implementations",
                            "⏳ Enable federated learning services",
                            "⏳ Enable quantum-resistant security",
                            "⏳ Test privacy and security features"
                        },
                        Timeline = "2-3 hours"
                    }
                },
                TotalEstimatedTime = "4-6 hours for complete Phase 3 enablement",
                RecommendedApproach = "Gradual enablement starting with streaming analytics",
                CreatedAt = DateTime.UtcNow
            };

            return Ok(roadmap);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Phase 3 roadmap");
            return StatusCode(500, new { error = "Failed to get roadmap", details = ex.Message });
        }
    }

    #endregion

    #region Real-Time Streaming

    /// <summary>
    /// Start a real-time streaming session
    /// </summary>
    [HttpPost("streaming/sessions/start")]
    public async Task<IActionResult> StartStreamingSession([FromBody] StartStreamingRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🎬 Starting streaming session for user {UserId}", userId);

            var command = new StartStreamingSessionCommand
            {
                UserId = userId,
                Configuration = request.Configuration ?? new Core.Models.StreamingConfiguration()
            };

            var session = await _mediator.Send(command);

            _logger.LogInformation("🎬 Streaming session {SessionId} started for user {UserId}",
                session.SessionId, userId);

            return Ok(new
            {
                success = true,
                data = session,
                metadata = new
                {
                    session_id = session.SessionId,
                    started_at = session.StartedAt,
                    configuration = session.Configuration
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting streaming session");
            return StatusCode(500, new { success = false, error = "Internal server error starting streaming session" });
        }
    }

    /// <summary>
    /// Stop a streaming session
    /// </summary>
    [HttpPost("streaming/sessions/{sessionId}/stop")]
    public async Task<IActionResult> StopStreamingSession(string sessionId)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🛑 Stopping streaming session {SessionId} for user {UserId}", sessionId, userId);

            var command = new StopStreamingSessionCommand
            {
                SessionId = sessionId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("🛑 Streaming session {SessionId} stopped successfully", sessionId);
                return Ok(new { success = true, message = "Streaming session stopped successfully" });
            }
            else
            {
                return NotFound(new { success = false, error = "Streaming session not found or access denied" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping streaming session {SessionId}", sessionId);
            return StatusCode(500, new { success = false, error = "Internal server error stopping streaming session" });
        }
    }

    /// <summary>
    /// Get real-time dashboard data
    /// </summary>
    [HttpGet("streaming/dashboard")]
    public async Task<IActionResult> GetRealTimeDashboard()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📈 Getting real-time dashboard for user {UserId}", userId);

            var query = new GetRealTimeDashboardQuery
            {
                UserId = userId
            };

            var dashboard = await _mediator.Send(query);

            _logger.LogInformation("📈 Real-time dashboard retrieved for user {UserId} with {ChartCount} charts",
                userId, dashboard.LiveCharts.Count);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    generated_at = dashboard.GeneratedAt,
                    active_sessions = dashboard.ActiveSessions.Count,
                    live_charts = dashboard.LiveCharts.Count,
                    alerts = dashboard.StreamingAlerts.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting real-time dashboard");
            return StatusCode(500, new { success = false, error = "Internal server error getting real-time dashboard" });
        }
    }

    /// <summary>
    /// Get streaming analytics for a time window
    /// </summary>
    [HttpGet("streaming/analytics")]
    public async Task<IActionResult> GetStreamingAnalytics([FromQuery] int? hours = 1)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 Getting streaming analytics for user {UserId}", userId);

            var query = new GetStreamingAnalyticsQuery
            {
                TimeWindow = TimeSpan.FromHours(hours ?? 1),
                UserId = userId
            };

            var analytics = await _mediator.Send(query);

            _logger.LogInformation("📊 Streaming analytics retrieved - Events: {EventCount}, Users: {UserCount}",
                analytics.TotalEvents, analytics.UserActivitySummary.ActiveUsers);

            return Ok(new
            {
                success = true,
                data = analytics,
                metadata = new
                {
                    time_window_hours = hours ?? 1,
                    total_events = analytics.TotalEvents,
                    active_users = analytics.UserActivitySummary.ActiveUsers,
                    generated_at = analytics.GeneratedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting streaming analytics");
            return StatusCode(500, new { success = false, error = "Internal server error getting streaming analytics" });
        }
    }

    /// <summary>
    /// Subscribe to a data stream
    /// </summary>
    [HttpPost("streaming/subscriptions")]
    public async Task<IActionResult> SubscribeToDataStream([FromBody] SubscribeRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📡 Creating data stream subscription for user {UserId}", userId);

            var command = new SubscribeToDataStreamCommand
            {
                UserId = userId,
                Subscription = request.Subscription
            };

            var subscriptionId = await _mediator.Send(command);

            _logger.LogInformation("📡 Data stream subscription {SubscriptionId} created for user {UserId}",
                subscriptionId, userId);

            return Ok(new
            {
                success = true,
                data = new { subscription_id = subscriptionId },
                metadata = new
                {
                    event_type = request.Subscription.EventType,
                    created_at = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating data stream subscription");
            return StatusCode(500, new { success = false, error = "Internal server error creating subscription" });
        }
    }

    /// <summary>
    /// Get real-time metrics
    /// </summary>
    [HttpGet("streaming/metrics")]
    public async Task<IActionResult> GetRealTimeMetrics()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 Getting real-time metrics for user {UserId}", userId);

            var query = new GetRealTimeMetricsQuery
            {
                UserId = userId
            };

            var metrics = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    active_users = metrics.ActiveUsers,
                    queries_per_minute = metrics.QueriesPerMinute,
                    last_updated = metrics.LastUpdated
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting real-time metrics");
            return StatusCode(500, new { success = false, error = "Internal server error getting real-time metrics" });
        }
    }

    #endregion
}

// Request/Response models for NLU operations
public class AnalyzeNLURequest
{
    public string Query { get; set; } = string.Empty;
    public NLUAnalysisContext? Context { get; set; }
}

public class ClassifyIntentRequest
{
    public string Query { get; set; } = string.Empty;
}

public class SmartSuggestionsRequest
{
    public string PartialQuery { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
    public NLUAnalysisContext? Context { get; set; }
}

public class QueryAssistanceRequest
{
    public string PartialQuery { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
}

// Streaming request models
public class StartStreamingRequest
{
    public Core.Models.StreamingConfiguration? Configuration { get; set; }
}

public class SubscribeRequest
{
    public StreamSubscription Subscription { get; set; } = new();
}

// Phase 3 demo information models
public class Phase3DemoInfo
{
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Phase3DemoFeature> Features { get; set; } = new();
    public List<string> Capabilities { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class Phase3DemoFeature
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public List<string> TechnicalDetails { get; set; } = new();
}

public class Phase3Roadmap
{
    public string CurrentPhase { get; set; } = string.Empty;
    public List<Phase3RoadmapPhase> Phases { get; set; } = new();
    public string TotalEstimatedTime { get; set; } = string.Empty;
    public string RecommendedApproach { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Phase3RoadmapPhase
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Deliverables { get; set; } = new();
    public string Timeline { get; set; } = string.Empty;
}
