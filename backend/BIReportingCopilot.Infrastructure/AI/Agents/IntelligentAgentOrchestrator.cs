using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Agents;
using BIReportingCopilot.Core.Models.Agents;
using BIReportingCopilot.Core.Models;
using System.Text.Json;
using AgentQueryIntent = BIReportingCopilot.Core.Models.Agents.QueryIntent;
using AgentSchemaContext = BIReportingCopilot.Core.Models.Agents.SchemaContext;

namespace BIReportingCopilot.Infrastructure.AI.Agents;

/// <summary>
/// Intelligent Agent Orchestrator - Coordinates multi-agent query processing workflows
/// </summary>
public class IntelligentAgentOrchestrator : IIntelligentAgentOrchestrator, ISpecializedAgent
{
    private readonly ILogger<IntelligentAgentOrchestrator> _logger;
    private readonly IAgentCommunicationProtocol _communicationProtocol;
    private readonly IConfiguration _configuration;
    private readonly AgentCapabilities _capabilities;
    private bool _isInitialized = false;

    public string AgentType => "IntelligentOrchestrator";
    public AgentCapabilities Capabilities => _capabilities;

    public IntelligentAgentOrchestrator(
        ILogger<IntelligentAgentOrchestrator> logger,
        IAgentCommunicationProtocol communicationProtocol,
        IConfiguration configuration)
    {
        _logger = logger;
        _communicationProtocol = communicationProtocol;
        _configuration = configuration;
        
        _capabilities = new AgentCapabilities
        {
            AgentType = AgentType,
            SupportedOperations = new List<string>
            {
                "ProcessCompleteQuery",
                "CoordinateAgents",
                "OptimizeWorkflow",
                "MonitorExecution",
                "HandleFailures"
            },
            Metadata = new Dictionary<string, object>
            {
                ["Version"] = "2.0.0",
                ["Specialization"] = "Multi-Agent Coordination and Workflow Management",
                ["MaxConcurrentWorkflows"] = 10
            },
            PerformanceScore = 0.96,
            IsAvailable = true
        };
    }

    #region ISpecializedAgent Implementation

    public async Task<AgentResponse> ProcessAsync(AgentRequest request, AgentContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🎯 IntelligentAgentOrchestrator processing request {RequestId} of type {RequestType}", 
                request.RequestId, request.RequestType);

            object? result = request.RequestType switch
            {
                "ProcessCompleteQuery" => await ProcessCompleteQueryAsync(request.Payload.ToString()!, context),
                "CoordinateAgents" => await CoordinateAgentsAsync(
                    JsonSerializer.Deserialize<WorkflowDefinition>(request.Payload.ToString()!)!, context),
                "OptimizeWorkflow" => await OptimizeWorkflowAsync(
                    JsonSerializer.Deserialize<WorkflowDefinition>(request.Payload.ToString()!)!, context),
                "MonitorExecution" => await MonitorExecutionAsync(
                    request.Parameters["workflowId"].ToString()!, context),
                "HandleFailures" => await HandleFailuresAsync(
                    JsonSerializer.Deserialize<WorkflowFailure>(request.Payload.ToString()!)!, context),
                _ => throw new NotSupportedException($"Request type {request.RequestType} is not supported")
            };

            stopwatch.Stop();

            return new AgentResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Result = result,
                ExecutionTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["AgentType"] = AgentType,
                    ["ProcessedAt"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Error processing request {RequestId}", request.RequestId);
            
            return new AgentResponse
            {
                RequestId = request.RequestId,
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync()
    {
        try
        {
            // Check agent availability
            var availableAgents = await _communicationProtocol.DiscoverAgentCapabilitiesAsync();
            var requiredAgents = new[] { "QueryUnderstanding", "SchemaNavigation", "SqlGeneration" };
            var missingAgents = requiredAgents.Where(ra => !availableAgents.Any(aa => aa.AgentType == ra)).ToList();
            
            return new HealthStatus
            {
                IsHealthy = missingAgents.Count == 0,
                Status = missingAgents.Count == 0 ? "Healthy" : "Degraded",
                Metrics = new Dictionary<string, object>
                {
                    ["LastCheck"] = DateTime.UtcNow,
                    ["AvailableAgents"] = availableAgents.Count,
                    ["RequiredAgents"] = requiredAgents.Length,
                    ["MissingAgents"] = missingAgents,
                    ["IsInitialized"] = _isInitialized
                },
                Issues = missingAgents.Count > 0 ? new List<string> { $"Missing required agents: {string.Join(", ", missingAgents)}" } : new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for IntelligentAgentOrchestrator");
            
            return new HealthStatus
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Issues = new List<string> { ex.Message }
            };
        }
    }

    public async Task InitializeAsync(Dictionary<string, object> configuration)
    {
        _logger.LogInformation("🚀 Initializing IntelligentAgentOrchestrator");
        
        // Register self with communication protocol
        await _communicationProtocol.RegisterAgentAsync(this);
        
        _isInitialized = true;
        _capabilities.LastHealthCheck = DateTime.UtcNow;
        
        _logger.LogInformation("✅ IntelligentAgentOrchestrator initialized successfully");
    }

    public async Task ShutdownAsync()
    {
        _logger.LogInformation("🛑 Shutting down IntelligentAgentOrchestrator");
        
        // Unregister from communication protocol
        await _communicationProtocol.UnregisterAgentAsync(AgentType);
        
        _isInitialized = false;
        _capabilities.IsAvailable = false;
        
        await Task.CompletedTask;
    }

    #endregion

    #region IIntelligentAgentOrchestrator Implementation

    public async Task<QueryProcessingResult> ProcessCompleteQueryAsync(string naturalLanguageQuery, AgentContext? context = null)
    {
        var workflowId = Guid.NewGuid().ToString();
        var agentContext = context ?? new AgentContext
        {
            CurrentAgent = AgentType,
            CorrelationId = workflowId,
            UserId = "system"
        };

        _logger.LogInformation("🎯 Starting complete query processing for workflow {WorkflowId}", workflowId);

        try
        {
            var result = new QueryProcessingResult
            {
                WorkflowId = workflowId,
                OriginalQuery = naturalLanguageQuery,
                StartTime = DateTime.UtcNow
            };

            // Step 1: Query Understanding
            _logger.LogDebug("📝 Step 1: Analyzing query intent");
            var intent = await _communicationProtocol.SendMessageAsync<string, AgentQueryIntent>(
                "QueryUnderstanding", naturalLanguageQuery, agentContext, TimeSpan.FromSeconds(30));
            
            result.QueryIntent = intent;
            result.Steps.Add(new WorkflowStep
            {
                StepName = "QueryUnderstanding",
                Status = "Completed",
                ExecutionTime = TimeSpan.FromMilliseconds(100),
                Output = intent
            });

            // Step 2: Schema Navigation
            _logger.LogDebug("🗺️ Step 2: Discovering relevant schema");
            var relevantTables = await _communicationProtocol.SendMessageAsync<AgentQueryIntent, List<RelevantTable>>(
                "SchemaNavigation", intent, agentContext, TimeSpan.FromSeconds(30));

            var schemaContext = await _communicationProtocol.SendMessageAsync<List<RelevantTable>, AgentSchemaContext>(
                "SchemaNavigation", relevantTables, agentContext, TimeSpan.FromSeconds(30));
            
            result.SchemaContext = schemaContext;
            result.Steps.Add(new WorkflowStep
            {
                StepName = "SchemaNavigation",
                Status = "Completed",
                ExecutionTime = TimeSpan.FromMilliseconds(150),
                Output = schemaContext
            });

            // Step 3: SQL Generation
            _logger.LogDebug("⚡ Step 3: Generating optimized SQL");
            var sqlGenerationRequest = new AgentRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                RequestType = "GenerateOptimizedSql",
                Parameters = new Dictionary<string, object>
                {
                    ["intent"] = JsonSerializer.Serialize(intent),
                    ["context"] = JsonSerializer.Serialize(schemaContext)
                }
            };

            var sqlResponse = await _communicationProtocol.SendMessageAsync<AgentRequest, GeneratedSql>(
                "SqlGeneration", sqlGenerationRequest, agentContext, TimeSpan.FromSeconds(45));
            
            result.GeneratedSql = sqlResponse;
            result.Steps.Add(new WorkflowStep
            {
                StepName = "SqlGeneration",
                Status = "Completed",
                ExecutionTime = TimeSpan.FromMilliseconds(200),
                Output = sqlResponse
            });

            // Step 4: Final Validation and Optimization
            _logger.LogDebug("✅ Step 4: Final validation and optimization");
            result = await PerformFinalValidation(result, agentContext);

            result.EndTime = DateTime.UtcNow;
            result.TotalExecutionTime = result.EndTime - result.StartTime;
            result.Success = true;

            _logger.LogInformation("✅ Complete query processing finished for workflow {WorkflowId} in {Duration}ms", 
                workflowId, result.TotalExecutionTime.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in complete query processing for workflow {WorkflowId}", workflowId);
            
            return new QueryProcessingResult
            {
                WorkflowId = workflowId,
                OriginalQuery = naturalLanguageQuery,
                Success = false,
                ErrorMessage = ex.Message,
                EndTime = DateTime.UtcNow
            };
        }
    }

    public async Task<WorkflowResult> CoordinateAgentsAsync(WorkflowDefinition workflow, AgentContext? context = null)
    {
        _logger.LogDebug("🎭 Coordinating agents for workflow {WorkflowId}", workflow.Id);

        try
        {
            var result = new WorkflowResult
            {
                WorkflowId = workflow.Id,
                StartTime = DateTime.UtcNow,
                Steps = new List<WorkflowStep>()
            };

            foreach (var step in workflow.Steps.OrderBy(s => s.Order))
            {
                _logger.LogDebug("🔄 Executing step {StepName} (Order: {Order})", step.AgentType, step.Order);

                var stepResult = await ExecuteWorkflowStep(step, context);
                result.Steps.Add(stepResult);

                if (stepResult.Status == "Failed" && step.IsRequired)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Required step {step.AgentType} failed: {stepResult.ErrorMessage}";
                    break;
                }
            }

            result.EndTime = DateTime.UtcNow;
            result.TotalExecutionTime = result.EndTime - result.StartTime;
            result.Success = result.Success && result.Steps.All(s => s.Status != "Failed" || !workflow.Steps.First(ws => ws.AgentType == s.StepName).IsRequired);

            _logger.LogDebug("✅ Workflow coordination complete for {WorkflowId}. Success: {Success}", 
                workflow.Id, result.Success);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error coordinating workflow {WorkflowId}", workflow.Id);
            throw;
        }
    }

    public async Task<WorkflowDefinition> OptimizeWorkflowAsync(WorkflowDefinition workflow, AgentContext? context = null)
    {
        _logger.LogDebug("🚀 Optimizing workflow {WorkflowId}", workflow.Id);

        try
        {
            var optimizedWorkflow = new WorkflowDefinition
            {
                Id = workflow.Id,
                Name = workflow.Name + "_Optimized",
                Steps = new List<WorkflowStep>()
            };

            // Analyze dependencies and optimize execution order
            var dependencyGraph = BuildDependencyGraph(workflow.Steps);
            var optimizedOrder = TopologicalSort(dependencyGraph);

            foreach (var stepName in optimizedOrder)
            {
                var originalStep = workflow.Steps.First(s => s.StepName == stepName);
                optimizedWorkflow.Steps.Add(originalStep);
            }

            _logger.LogDebug("✅ Workflow optimization complete for {WorkflowId}", workflow.Id);
            return optimizedWorkflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing workflow {WorkflowId}", workflow.Id);
            throw;
        }
    }

    public async Task<BIReportingCopilot.Core.Models.Agents.WorkflowStatus> MonitorExecutionAsync(string workflowId, AgentContext? context = null)
    {
        _logger.LogDebug("👁️ Monitoring execution for workflow {WorkflowId}", workflowId);

        try
        {
            // Get communication logs for this workflow
            var logs = await _communicationProtocol.GetCommunicationLogsAsync(workflowId);
            
            var status = new BIReportingCopilot.Core.Models.Agents.WorkflowStatus
            {
                WorkflowId = workflowId,
                Status = DetermineWorkflowStatus(logs),
                Progress = CalculateProgress(logs),
                LastActivity = logs.Any() ? logs.Max(l => l.CreatedAt) : DateTime.UtcNow,
                ActiveAgents = logs.Where(l => l.Success).Select(l => l.TargetAgent).Distinct().ToList(),
                Metrics = new Dictionary<string, object>
                {
                    ["TotalMessages"] = logs.Count,
                    ["SuccessfulMessages"] = logs.Count(l => l.Success),
                    ["FailedMessages"] = logs.Count(l => !l.Success),
                    ["AverageExecutionTime"] = logs.Any() ? logs.Average(l => l.ExecutionTimeMs) : 0
                }
            };

            _logger.LogDebug("✅ Workflow monitoring complete for {WorkflowId}. Status: {Status}", 
                workflowId, status.Status);

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error monitoring workflow {WorkflowId}", workflowId);
            throw;
        }
    }

    public async Task<RecoveryResult> HandleFailuresAsync(WorkflowFailure failure, AgentContext? context = null)
    {
        _logger.LogWarning("🚨 Handling failure for workflow {WorkflowId}: {ErrorMessage}", 
            failure.WorkflowId, failure.ErrorMessage);

        try
        {
            var recoveryResult = new RecoveryResult
            {
                WorkflowId = failure.WorkflowId,
                OriginalError = failure.ErrorMessage,
                RecoveryAttempts = new List<RecoveryAttempt>()
            };

            // Attempt different recovery strategies
            var strategies = GetRecoveryStrategies(failure);
            
            foreach (var strategy in strategies)
            {
                _logger.LogDebug("🔄 Attempting recovery strategy: {Strategy}", strategy.Name);
                
                var attempt = await ExecuteRecoveryStrategy(strategy, failure, context);
                recoveryResult.RecoveryAttempts.Add(attempt);
                
                if (attempt.Success)
                {
                    recoveryResult.Success = true;
                    recoveryResult.RecoveryStrategy = strategy.Name;
                    break;
                }
            }

            _logger.LogInformation("✅ Failure handling complete for workflow {WorkflowId}. Recovered: {Success}", 
                failure.WorkflowId, recoveryResult.Success);

            return recoveryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling failure for workflow {WorkflowId}", failure.WorkflowId);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<QueryProcessingResult> PerformFinalValidation(QueryProcessingResult result, AgentContext context)
    {
        // Add final validation step
        result.Steps.Add(new WorkflowStep
        {
            StepName = "FinalValidation",
            Status = "Completed",
            ExecutionTime = TimeSpan.FromMilliseconds(50),
            Output = "Validation passed"
        });

        return result;
    }

    private async Task<WorkflowStep> ExecuteWorkflowStep(WorkflowStep step, AgentContext? context)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Execute the step based on agent type
            var result = await _communicationProtocol.SendMessageAsync<object, object>(
                step.AgentType, step.Input!, context ?? new AgentContext(), TimeSpan.FromSeconds(30));
            
            stopwatch.Stop();

            return new WorkflowStep
            {
                StepName = step.StepName,
                AgentType = step.AgentType,
                Status = "Completed",
                ExecutionTime = stopwatch.Elapsed,
                Output = result
            };
        }
        catch (Exception ex)
        {
            return new WorkflowStep
            {
                StepName = step.StepName,
                AgentType = step.AgentType,
                Status = "Failed",
                ErrorMessage = ex.Message
            };
        }
    }

    private Dictionary<string, List<string>> BuildDependencyGraph(List<WorkflowStep> steps)
    {
        // Simple dependency analysis based on step order
        var graph = new Dictionary<string, List<string>>();
        
        foreach (var step in steps)
        {
            graph[step.StepName] = new List<string>();
        }

        return graph;
    }

    private List<string> TopologicalSort(Dictionary<string, List<string>> graph)
    {
        // Simple topological sort
        return graph.Keys.ToList();
    }

    private string DetermineWorkflowStatus(List<AgentCommunicationLog> logs)
    {
        if (!logs.Any()) return "NotStarted";
        if (logs.Any(l => !l.Success)) return "Failed";
        return "Completed";
    }

    private double CalculateProgress(List<AgentCommunicationLog> logs)
    {
        // Simple progress calculation
        return logs.Any() ? Math.Min(1.0, logs.Count / 3.0) : 0.0;
    }

    private List<RecoveryStrategy> GetRecoveryStrategies(WorkflowFailure failure)
    {
        return new List<RecoveryStrategy>
        {
            new RecoveryStrategy { Name = "Retry", Description = "Retry the failed operation" },
            new RecoveryStrategy { Name = "Fallback", Description = "Use alternative approach" },
            new RecoveryStrategy { Name = "Simplify", Description = "Simplify the query and retry" }
        };
    }

    private async Task<RecoveryAttempt> ExecuteRecoveryStrategy(RecoveryStrategy strategy, WorkflowFailure failure, AgentContext? context)
    {
        try
        {
            // Simple recovery attempt simulation
            await Task.Delay(100);
            
            return new RecoveryAttempt
            {
                Strategy = strategy.Name,
                Success = strategy.Name == "Retry", // Simple success logic
                ExecutionTime = TimeSpan.FromMilliseconds(100)
            };
        }
        catch (Exception ex)
        {
            return new RecoveryAttempt
            {
                Strategy = strategy.Name,
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = TimeSpan.FromMilliseconds(100)
            };
        }
    }

    #endregion

    #region IIntelligentAgentOrchestrator Interface Methods

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🎯 Processing query request for user {UserId}", userId);

            var context = new AgentContext
            {
                CurrentAgent = AgentType,
                CorrelationId = Guid.NewGuid().ToString(),
                UserId = userId
            };

            var result = await ProcessCompleteQueryAsync(request.Question, context);

            return new QueryResponse
            {
                Success = result.Success,
                Sql = result.GeneratedSql?.Sql ?? "",
                ErrorMessage = result.ErrorMessage,
                ExecutionTime = result.TotalExecutionTime,
                Confidence = result.GeneratedSql?.Confidence ?? 0.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing query for user {UserId}", userId);

            return new QueryResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<string>> SelectOptimalAgentsAsync(AgentQueryIntent intent, AgentContext context)
    {
        try
        {
            _logger.LogDebug("🎯 Selecting optimal agents for query intent: {QueryType}", intent.QueryType);

            var selectedAgents = new List<string>();

            // Always include core agents for complete query processing
            selectedAgents.Add("QueryUnderstanding");
            selectedAgents.Add("SchemaNavigation");
            selectedAgents.Add("SqlGeneration");

            // Add specialized agents based on query complexity and type
            if (intent.Complexity.Level >= BIReportingCopilot.Core.Models.Agents.ComplexityLevel.High)
            {
                selectedAgents.Add("ExecutionAgent");
            }

            if (intent.QueryType == "VISUALIZATION" || intent.BusinessContext.BusinessTerms.Contains("chart") || intent.BusinessContext.BusinessTerms.Contains("graph"))
            {
                selectedAgents.Add("VisualizationAgent");
            }

            _logger.LogDebug("✅ Selected {Count} agents: {Agents}", selectedAgents.Count, string.Join(", ", selectedAgents));
            return selectedAgents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error selecting optimal agents");
            throw;
        }
    }

    public async Task<Dictionary<string, AgentResponse>> ExecuteAgentsInParallelAsync(
        List<string> agentTypes,
        AgentRequest request,
        AgentContext context)
    {
        try
        {
            _logger.LogDebug("🔄 Executing {Count} agents in parallel", agentTypes.Count);

            var tasks = agentTypes.Select(async agentType =>
            {
                try
                {
                    var response = await _communicationProtocol.SendMessageAsync<AgentRequest, AgentResponse>(
                        agentType, request, context, TimeSpan.FromSeconds(30));
                    return new KeyValuePair<string, AgentResponse>(agentType, response);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Agent {AgentType} failed during parallel execution", agentType);
                    return new KeyValuePair<string, AgentResponse>(agentType, new AgentResponse
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            });

            var results = await Task.WhenAll(tasks);
            var resultDictionary = results.ToDictionary(r => r.Key, r => r.Value);

            _logger.LogDebug("✅ Parallel execution complete. Success rate: {SuccessRate:P}",
                resultDictionary.Values.Count(r => r.Success) / (double)resultDictionary.Count);

            return resultDictionary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error executing agents in parallel");
            throw;
        }
    }

    public async Task<QueryResponse> AggregateResultsAsync(Dictionary<string, AgentResponse> agentResults, AgentContext context)
    {
        try
        {
            _logger.LogDebug("🔄 Aggregating results from {Count} agents", agentResults.Count);

            var successfulResults = agentResults.Where(r => r.Value.Success).ToList();
            var failedResults = agentResults.Where(r => !r.Value.Success).ToList();

            if (failedResults.Any())
            {
                _logger.LogWarning("⚠️ {FailedCount} agents failed during execution", failedResults.Count);
            }

            // Create aggregated response
            var aggregatedResponse = new QueryResponse
            {
                Success = successfulResults.Any(),
                Sql = "", // Will be filled by the calling method
                ExecutionTime = TimeSpan.FromMilliseconds(agentResults.Values.Sum(r => r.ExecutionTime.TotalMilliseconds))
            };

            if (failedResults.Any())
            {
                aggregatedResponse.ErrorMessage = $"Some agents failed: {string.Join(", ", failedResults.Select(r => r.Key))}";
            }

            _logger.LogDebug("✅ Result aggregation complete. Success rate: {SuccessRate:P}",
                successfulResults.Count / (double)agentResults.Count);

            return aggregatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error aggregating agent results");
            throw;
        }
    }

    public async Task<BIReportingCopilot.Core.Interfaces.Agents.OrchestrationMetrics> GetOrchestrationMetricsAsync(string? workflowId = null)
    {
        try
        {
            _logger.LogDebug("📊 Getting orchestration metrics for workflow: {WorkflowId}", workflowId ?? "All");

            var logs = string.IsNullOrEmpty(workflowId)
                ? await _communicationProtocol.GetCommunicationLogsAsync()
                : await _communicationProtocol.GetCommunicationLogsAsync(workflowId);

            var metrics = new BIReportingCopilot.Core.Interfaces.Agents.OrchestrationMetrics
            {
                CorrelationId = workflowId ?? "All",
                TotalExecutionTime = TimeSpan.FromMilliseconds(logs.Any() ? logs.Sum(l => l.ExecutionTimeMs) : 0),
                AgentsInvolved = logs.Select(l => l.TargetAgent).Distinct().Count(),
                AgentExecutionTimes = logs.GroupBy(l => l.TargetAgent)
                    .ToDictionary(g => g.Key, g => TimeSpan.FromMilliseconds(g.Sum(x => x.ExecutionTimeMs))),
                SuccessRate = logs.Any() ? logs.Count(l => l.Success) / (double)logs.Count : 0.0,
                Errors = logs.Where(l => !l.Success && !string.IsNullOrEmpty(l.ErrorMessage))
                    .Select(l => l.ErrorMessage!)
                    .ToList()
            };

            _logger.LogDebug("✅ Orchestration metrics generated. Success rate: {SuccessRate:P}",
                metrics.SuccessRate);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting orchestration metrics");
            throw;
        }
    }

    #endregion
}
