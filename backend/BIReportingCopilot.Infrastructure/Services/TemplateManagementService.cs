using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Models;
using ABTestStatus = BIReportingCopilot.Core.Models.Analytics.ABTestStatus;
using SuggestionStatus = BIReportingCopilot.Core.Models.Analytics.SuggestionStatus;
using TemplateSearchCriteria = BIReportingCopilot.Core.Interfaces.Analytics.TemplateSearchCriteria;
using TemplatePerformanceMetrics = BIReportingCopilot.Core.Interfaces.Analytics.TemplatePerformanceMetrics;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Service implementation for comprehensive template management and lifecycle operations
/// </summary>
public class TemplateManagementService : ITemplateManagementService
{
    private readonly IEnhancedPromptTemplateRepository _templateRepository;
    private readonly ITemplatePerformanceRepository _performanceRepository;
    private readonly ITemplateABTestRepository _abTestRepository;
    private readonly ITemplateImprovementRepository _improvementRepository;
    private readonly ITemplatePerformanceService _performanceService;
    private readonly ILogger<TemplateManagementService> _logger;

    public TemplateManagementService(
        IEnhancedPromptTemplateRepository templateRepository,
        ITemplatePerformanceRepository performanceRepository,
        ITemplateABTestRepository abTestRepository,
        ITemplateImprovementRepository improvementRepository,
        ITemplatePerformanceService performanceService,
        ILogger<TemplateManagementService> logger)
    {
        _templateRepository = templateRepository;
        _performanceRepository = performanceRepository;
        _abTestRepository = abTestRepository;
        _improvementRepository = improvementRepository;
        _performanceService = performanceService;
        _logger = logger;
    }

    public async Task<TemplateWithMetrics?> GetTemplateAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return null;
            }

            var performanceMetrics = await _performanceService.GetTemplatePerformanceAsync(templateKey, cancellationToken);
            var activeTests = await _abTestRepository.GetTestsByTemplateIdAsync(template.Id, cancellationToken);
            var pendingSuggestions = await _improvementRepository.GetByTemplateIdAsync(template.Id, cancellationToken);

            return new TemplateWithMetrics
            {
                Template = template,
                PerformanceMetrics = performanceMetrics,
                ActiveTests = activeTests.Select(MapToABTestDetails).ToList(),
                PendingSuggestions = pendingSuggestions.Select(MapToImprovementSuggestion).ToList(),
                HealthStatus = CalculateHealthStatus(performanceMetrics),
                LastAnalyzed = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<List<TemplateWithMetrics>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = await _templateRepository.GetActiveTemplatesAsync(cancellationToken);
            var results = new List<TemplateWithMetrics>();

            foreach (var template in templates)
            {
                var templateWithMetrics = await GetTemplateAsync(template.TemplateKey ?? string.Empty, cancellationToken);
                if (templateWithMetrics != null)
                {
                    results.Add(templateWithMetrics);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active templates");
            throw;
        }
    }

    public async Task<List<TemplateWithMetrics>> GetTemplatesByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = await _templateRepository.GetByIntentTypeAsync(intentType, cancellationToken);
            var results = new List<TemplateWithMetrics>();

            foreach (var template in templates)
            {
                var templateWithMetrics = await GetTemplateAsync(template.TemplateKey ?? string.Empty, cancellationToken);
                if (templateWithMetrics != null)
                {
                    results.Add(templateWithMetrics);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates by intent type: {IntentType}", intentType);
            throw;
        }
    }

    public async Task<TemplateCreationResult> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating template: {TemplateName}", request.Name);

            // Validate template
            var validation = await ValidateTemplateAsync(request.Content, request.IntentType, cancellationToken);
            if (!validation.IsValid)
            {
                return new TemplateCreationResult
                {
                    Success = false,
                    Message = "Template validation failed",
                    ValidationWarnings = validation.Errors.Select(e => e.Message).ToList()
                };
            }

            // Check if template key already exists
            var existingTemplate = await _templateRepository.GetByKeyAsync(request.TemplateKey, cancellationToken);
            if (existingTemplate != null)
            {
                return new TemplateCreationResult
                {
                    Success = false,
                    Message = $"Template with key '{request.TemplateKey}' already exists"
                };
            }

            // Create template entity
            var templateEntity = new PromptTemplateEntity
            {
                Name = request.Name,
                TemplateKey = request.TemplateKey,
                Content = request.Content,
                Description = request.Description,
                IntentType = request.IntentType,
                Priority = request.Priority,
                Tags = string.Join(",", request.Tags),
                IsActive = true,
                Version = "1.0",
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var createdTemplate = await _templateRepository.CreateAsync(templateEntity, cancellationToken);

            // Initialize performance metrics
            var performanceEntity = new TemplatePerformanceMetricsEntity
            {
                TemplateId = createdTemplate.Id,
                TemplateKey = request.TemplateKey,
                IntentType = request.IntentType,
                TotalUsages = 0,
                SuccessfulUsages = 0,
                SuccessRate = 0,
                AverageConfidenceScore = 0,
                AverageProcessingTimeMs = 0,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _performanceRepository.CreateAsync(performanceEntity, cancellationToken);

            _logger.LogInformation("Successfully created template {TemplateKey} with ID {TemplateId}",
                request.TemplateKey, createdTemplate.Id);

            return new TemplateCreationResult
            {
                Success = true,
                Message = "Template created successfully",
                TemplateKey = request.TemplateKey,
                TemplateId = createdTemplate.Id,
                Version = "1.0",
                ValidationWarnings = validation.Warnings.Select(w => w.Message).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template: {TemplateName}", request.Name);
            throw;
        }
    }

    public async Task<TemplateUpdateResult> UpdateTemplateAsync(string templateKey, UpdateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating template: {TemplateKey}", templateKey);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return new TemplateUpdateResult
                {
                    Success = false,
                    Message = $"Template not found: {templateKey}"
                };
            }

            var previousVersion = template.Version;
            var changes = new List<string>();

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name) && request.Name != template.Name)
            {
                template.Name = request.Name;
                changes.Add($"Name changed from '{template.Name}' to '{request.Name}'");
            }

            if (!string.IsNullOrEmpty(request.Content) && request.Content != template.Content)
            {
                // Validate new content
                var validation = await ValidateTemplateAsync(request.Content, template.IntentType, cancellationToken);
                if (!validation.IsValid)
                {
                    return new TemplateUpdateResult
                    {
                        Success = false,
                        Message = "Template content validation failed",
                        ValidationWarnings = validation.Errors.Select(e => e.Message).ToList()
                    };
                }

                template.Content = request.Content;
                changes.Add("Content updated");
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                template.Description = request.Description;
                changes.Add("Description updated");
            }

            if (request.Priority.HasValue)
            {
                template.Priority = request.Priority.Value;
                changes.Add($"Priority changed to {request.Priority.Value}");
            }

            if (request.Tags != null)
            {
                template.Tags = string.Join(",", request.Tags);
                changes.Add("Tags updated");
            }

            // Create new version if requested
            if (request.CreateNewVersion)
            {
                var versionParts = template.Version.Split('.');
                if (versionParts.Length >= 2 && int.TryParse(versionParts[1], out var minorVersion))
                {
                    template.Version = $"{versionParts[0]}.{minorVersion + 1}";
                }
                else
                {
                    template.Version = "1.1";
                }
            }

            template.UpdatedDate = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template, cancellationToken);

            _logger.LogInformation("Successfully updated template {TemplateKey}", templateKey);

            return new TemplateUpdateResult
            {
                Success = true,
                Message = "Template updated successfully",
                NewVersion = template.Version,
                PreviousVersion = previousVersion,
                ChangesApplied = changes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<bool> ActivateTemplateAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return false;
            }

            template.IsActive = true;
            template.UpdatedDate = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template, cancellationToken);

            _logger.LogInformation("Activated template: {TemplateKey}", templateKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating template: {TemplateKey}", templateKey);
            return false;
        }
    }

    public async Task<bool> DeactivateTemplateAsync(string templateKey, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return false;
            }

            template.IsActive = false;
            template.UpdatedDate = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template, cancellationToken);

            _logger.LogInformation("Deactivated template {TemplateKey}: {Reason}", templateKey, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating template: {TemplateKey}", templateKey);
            return false;
        }
    }

    public async Task<bool> DeleteTemplateAsync(string templateKey, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return false;
            }

            // Soft delete - just deactivate
            template.IsActive = false;
            template.UpdatedDate = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template, cancellationToken);

            _logger.LogInformation("Deleted (soft) template {TemplateKey}: {Reason}", templateKey, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template: {TemplateKey}", templateKey);
            return false;
        }
    }

    public async Task<List<TemplateVersion>> GetTemplateVersionsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would require version history tracking - simplified implementation
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return new List<TemplateVersion>();
            }

            var performance = await _performanceService.GetTemplatePerformanceAsync(templateKey, cancellationToken);

            return new List<TemplateVersion>
            {
                new TemplateVersion
                {
                    Version = template.Version,
                    Content = template.Content,
                    CreatedDate = template.UpdatedDate ?? DateTime.UtcNow,
                    CreatedBy = template.CreatedBy ?? "System",
                    IsActive = template.IsActive,
                    PerformanceMetrics = performance
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template versions: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<TemplateRollbackResult> RollbackTemplateAsync(string templateKey, string targetVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simplified rollback - in a real implementation, you'd have version history
            _logger.LogInformation("Rollback requested for template {TemplateKey} to version {Version}", templateKey, targetVersion);

            return new TemplateRollbackResult
            {
                Success = false,
                Message = "Version history not implemented - rollback not available",
                RollbackDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back template: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<TemplateSearchResult> SearchTemplatesAsync(BIReportingCopilot.Core.Interfaces.Analytics.TemplateSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert interface criteria to model criteria
            var modelCriteria = new BIReportingCopilot.Core.Models.TemplateSearchCriteria
            {
                SearchTerm = criteria.SearchTerm,
                IntentType = criteria.IntentTypes?.FirstOrDefault(),
                IsActive = criteria.IsActive,
                Tags = criteria.Tags,
                CreatedAfter = criteria.CreatedAfter,
                CreatedBefore = criteria.CreatedBefore,
                CreatedBy = criteria.CreatedBy,
                SortBy = criteria.SortBy,
                SortDescending = criteria.SortDescending,
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };

            var templates = await _templateRepository.SearchTemplatesAsync(modelCriteria, cancellationToken);
            var totalCount = await _templateRepository.GetSearchCountAsync(modelCriteria, cancellationToken);

            var results = new List<TemplateWithMetrics>();
            foreach (var template in templates)
            {
                var templateWithMetrics = await GetTemplateAsync(template.TemplateKey ?? string.Empty, cancellationToken);
                if (templateWithMetrics != null)
                {
                    results.Add(templateWithMetrics);
                }
            }

            return new TemplateSearchResult
            {
                Templates = results,
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching templates");
            throw;
        }
    }

    public async Task<List<TemplateWithMetrics>> GetTemplatesNeedingReviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var underperforming = await _performanceService.GetUnderperformingTemplatesAsync(cancellationToken: cancellationToken);
            var results = new List<TemplateWithMetrics>();

            foreach (var performance in underperforming.Take(10))
            {
                var templateWithMetrics = await GetTemplateAsync(performance.TemplateKey, cancellationToken);
                if (templateWithMetrics != null)
                {
                    results.Add(templateWithMetrics);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates needing review");
            throw;
        }
    }

    public async Task<bool> UpdateBusinessMetadataAsync(string templateKey, TemplateBusinessMetadata metadata, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return false;
            }

            // Update business metadata fields directly
            template.BusinessPurpose = metadata.BusinessPurpose;
            template.RelatedBusinessTerms = metadata.RelatedBusinessTerms != null ?
                string.Join(",", metadata.RelatedBusinessTerms) : null;
            template.BusinessFriendlyName = metadata.BusinessFriendlyName;
            template.NaturalLanguageDescription = metadata.NaturalLanguageDescription;
            template.BusinessRules = metadata.BusinessRules;
            template.RelationshipContext = metadata.RelationshipContext;
            template.DataGovernanceLevel = metadata.DataGovernanceLevel;
            template.UpdatedDate = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template, cancellationToken);

            _logger.LogInformation("Updated business metadata for template: {TemplateKey}", templateKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business metadata for template: {TemplateKey}", templateKey);
            return false;
        }
    }

    public async Task<TemplateUsageAnalytics> GetUsageAnalyticsAsync(string templateKey, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var window = timeWindow ?? TimeSpan.FromDays(30);
            var insights = await _performanceService.GetUsageInsightsAsync(templateKey, cancellationToken);

            return new TemplateUsageAnalytics
            {
                TemplateKey = templateKey,
                AnalysisWindow = window,
                TotalUsages = insights.UsagePatterns.Sum(p => (int)p.Frequency),
                UsageByHour = insights.UsageByTimeOfDay,
                UsageByDay = insights.UsageByDayOfWeek,
                Patterns = insights.UsagePatterns,
                AnalysisDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage analytics for template: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<TemplateCreationResult> CloneTemplateAsync(string sourceTemplateKey, CloneTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var sourceTemplate = await _templateRepository.GetByKeyAsync(sourceTemplateKey, cancellationToken);
            if (sourceTemplate == null)
            {
                return new TemplateCreationResult
                {
                    Success = false,
                    Message = $"Source template not found: {sourceTemplateKey}"
                };
            }

            var content = !string.IsNullOrEmpty(request.ContentModifications) ?
                request.ContentModifications : sourceTemplate.Content;

            var createRequest = new CreateTemplateRequest
            {
                Name = request.NewName,
                TemplateKey = request.NewTemplateKey,
                Content = content,
                Description = $"Cloned from {sourceTemplate.Name}. {request.CloneReason}",
                IntentType = request.NewIntentType ?? sourceTemplate.IntentType,
                Tags = request.NewTags ?? sourceTemplate.Tags?.Split(',').ToList() ?? new List<string>(),
                CreatedBy = request.CreatedBy
            };

            return await CreateTemplateAsync(createRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning template: {SourceTemplateKey}", sourceTemplateKey);
            throw;
        }
    }

    public async Task<TemplateValidationResult> ValidateTemplateAsync(string templateContent, string intentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new TemplateValidationResult { IsValid = true };

            // Basic validation rules
            if (string.IsNullOrWhiteSpace(templateContent))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    ErrorType = "Content",
                    Message = "Template content cannot be empty"
                });
            }

            if (templateContent.Length < 10)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    WarningType = "Content",
                    Message = "Template content is very short"
                });
            }

            if (templateContent.Length > 10000)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    WarningType = "Content",
                    Message = "Template content is very long - consider breaking it down"
                });
            }

            // Check for required placeholders based on intent type
            if (intentType == "sql_generation" && !templateContent.Contains("{schema}"))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    WarningType = "Structure",
                    Message = "SQL generation templates should include {schema} placeholder"
                });
            }

            // Calculate quality score
            result.QualityScore = CalculateQualityScore(templateContent, result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template");
            throw;
        }
    }

    public async Task<List<TemplateRecommendation>> GetTemplateRecommendationsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = new List<TemplateRecommendation>();
            var performance = await _performanceService.GetTemplatePerformanceAsync(templateKey, cancellationToken);

            if (performance != null)
            {
                if (performance.SuccessRate < 0.8m)
                {
                    recommendations.Add(new TemplateRecommendation
                    {
                        RecommendationType = "Performance Improvement",
                        Title = "Low Success Rate",
                        Description = $"Template has a success rate of {performance.SuccessRate:P2}",
                        Priority = 1 - performance.SuccessRate,
                        ExpectedImpact = 0.3m,
                        ActionItems = new List<string>
                        {
                            "Review template content for clarity",
                            "Consider A/B testing with variations",
                            "Analyze failed usage patterns"
                        }
                    });
                }

                if (performance.AverageProcessingTimeMs > 5000)
                {
                    recommendations.Add(new TemplateRecommendation
                    {
                        RecommendationType = "Performance Optimization",
                        Title = "Slow Processing Time",
                        Description = $"Template takes {performance.AverageProcessingTimeMs}ms on average",
                        Priority = 0.7m,
                        ExpectedImpact = 0.2m,
                        ActionItems = new List<string>
                        {
                            "Simplify template structure",
                            "Reduce template length",
                            "Optimize placeholder usage"
                        }
                    });
                }
            }

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template recommendations: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<byte[]> ExportTemplatesAsync(List<string> templateKeys, BIReportingCopilot.Core.Models.Analytics.ExportFormat format = BIReportingCopilot.Core.Models.Analytics.ExportFormat.JSON, CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = new List<BIReportingCopilot.Core.Models.PromptTemplateEntity>();
            foreach (var key in templateKeys)
            {
                var template = await _templateRepository.GetByKeyAsync(key, cancellationToken);
                if (template != null)
                {
                    templates.Add(template);
                }
            }

            if (format == BIReportingCopilot.Core.Models.Analytics.ExportFormat.JSON)
            {
                var json = JsonSerializer.Serialize(templates, new JsonSerializerOptions { WriteIndented = true });
                return System.Text.Encoding.UTF8.GetBytes(json);
            }

            if (format == BIReportingCopilot.Core.Models.Analytics.ExportFormat.CSV)
            {
                var csv = "TemplateKey,Name,IntentType,Content,Description,IsActive,Version,CreatedBy,CreatedDate\n";
                foreach (var template in templates)
                {
                    csv += $"{template.TemplateKey},{template.Name},{template.IntentType},\"{template.Content.Replace("\"", "\"\"")}\",\"{template.Description}\",{template.IsActive},{template.Version},{template.CreatedBy},{template.CreatedDate}\n";
                }
                return System.Text.Encoding.UTF8.GetBytes(csv);
            }

            throw new NotImplementedException($"Export format {format} not implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting templates");
            throw;
        }
    }

    public async Task<TemplateImportResult> ImportTemplatesAsync(byte[] templateData, ImportOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new TemplateImportResult { Success = true };
            var json = System.Text.Encoding.UTF8.GetString(templateData);
            var templates = JsonSerializer.Deserialize<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(json);

            if (templates == null)
            {
                return new TemplateImportResult
                {
                    Success = false,
                    Message = "Invalid template data format"
                };
            }

            foreach (var template in templates)
            {
                try
                {
                    var existing = await _templateRepository.GetByKeyAsync(template.TemplateKey ?? string.Empty, cancellationToken);

                    if (existing != null && !options.OverwriteExisting)
                    {
                        result.TemplatesSkipped++;
                        continue;
                    }

                    if (options.ValidateBeforeImport)
                    {
                        var validation = await ValidateTemplateAsync(template.Content, template.IntentType, cancellationToken);
                        if (!validation.IsValid)
                        {
                            result.ImportErrors.Add($"Template {template.TemplateKey}: {string.Join(", ", validation.Errors.Select(e => e.Message))}");
                            continue;
                        }
                    }

                    if (existing != null)
                    {
                        // Update existing
                        existing.Content = template.Content;
                        existing.Description = template.Description;
                        existing.UpdatedDate = DateTime.UtcNow;
                        await _templateRepository.UpdateAsync(existing, cancellationToken);
                        result.TemplatesUpdated++;
                    }
                    else
                    {
                        // Create new
                        template.Id = 0; // Reset ID for new entity
                        template.CreatedDate = DateTime.UtcNow;
                        template.UpdatedDate = DateTime.UtcNow;
                        await _templateRepository.CreateAsync(template, cancellationToken);
                        result.TemplatesImported++;
                    }
                }
                catch (Exception ex)
                {
                    result.ImportErrors.Add($"Template {template.TemplateKey}: {ex.Message}");
                }
            }

            result.Message = $"Import completed: {result.TemplatesImported} imported, {result.TemplatesUpdated} updated, {result.TemplatesSkipped} skipped";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing templates");
            throw;
        }
    }

    public async Task<TemplateManagementDashboard> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allTemplates = await _templateRepository.GetAllAsync(cancellationToken);
            var activeTemplates = allTemplates.Where(t => t.IsActive).ToList();
            var needsReview = await GetTemplatesNeedingReviewAsync(cancellationToken);
            var topPerformers = await _performanceService.GetTopPerformingTemplatesAsync(count: 5, cancellationToken: cancellationToken);
            var needsAttention = await _performanceService.GetUnderperformingTemplatesAsync(cancellationToken: cancellationToken);

            var intentTypeCounts = activeTemplates.GroupBy(t => t.IntentType)
                .ToDictionary(g => g.Key, g => g.Count());

            var recentlyCreated = allTemplates.Where(t => t.CreatedDate > DateTime.UtcNow.AddDays(-7)).Count();
            var recentlyUpdated = allTemplates.Where(t => t.UpdatedDate > DateTime.UtcNow.AddDays(-7) &&
                                                         t.UpdatedDate != t.CreatedDate).Count();

            return new TemplateManagementDashboard
            {
                TotalTemplates = allTemplates.Count,
                ActiveTemplates = activeTemplates.Count,
                TemplatesNeedingReview = needsReview.Count,
                RecentlyCreated = recentlyCreated,
                RecentlyUpdated = recentlyUpdated,
                TemplatesByIntentType = intentTypeCounts,
                TopPerformers = topPerformers.Select(p => new TemplateWithMetrics
                {
                    PerformanceMetrics = p,
                    HealthStatus = CalculateHealthStatus(p)
                }).ToList(),
                NeedsAttention = needsAttention.Take(5).Select(p => new TemplateWithMetrics
                {
                    PerformanceMetrics = p,
                    HealthStatus = CalculateHealthStatus(p)
                }).ToList(),
                GeneratedDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template management dashboard");
            throw;
        }
    }

    #region Private Helper Methods

    private static TemplateHealthStatus CalculateHealthStatus(TemplatePerformanceMetrics? performance)
    {
        if (performance == null)
        {
            return TemplateHealthStatus.Fair;
        }

        if (performance.SuccessRate >= 0.9m && performance.TotalUsages >= 10)
        {
            return TemplateHealthStatus.Excellent;
        }

        if (performance.SuccessRate >= 0.8m && performance.TotalUsages >= 5)
        {
            return TemplateHealthStatus.Good;
        }

        if (performance.SuccessRate >= 0.6m)
        {
            return TemplateHealthStatus.Fair;
        }

        if (performance.SuccessRate >= 0.4m)
        {
            return TemplateHealthStatus.Poor;
        }

        return TemplateHealthStatus.Critical;
    }

    private static decimal CalculateQualityScore(string content, int errorCount, int warningCount)
    {
        var baseScore = 1.0m;

        // Deduct for errors and warnings
        baseScore -= errorCount * 0.3m;
        baseScore -= warningCount * 0.1m;

        // Bonus for good length
        if (content.Length >= 50 && content.Length <= 2000)
        {
            baseScore += 0.1m;
        }

        // Bonus for having placeholders
        if (content.Contains("{") && content.Contains("}"))
        {
            baseScore += 0.1m;
        }

        return Math.Max(0, Math.Min(1, baseScore));
    }

    private static ABTestDetails MapToABTestDetails(TemplateABTestEntity entity)
    {
        return new ABTestDetails
        {
            Id = entity.Id,
            TestName = entity.TestName,
            OriginalTemplateKey = entity.OriginalTemplate?.TemplateKey ?? string.Empty,
            VariantTemplateKey = entity.VariantTemplate?.TemplateKey ?? string.Empty,
            TrafficSplitPercent = entity.TrafficSplitPercent,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = Enum.Parse<ABTestStatus>(entity.Status, true),
            CreatedBy = entity.CreatedBy ?? string.Empty,
            CreatedDate = entity.CreatedDate
        };
    }

    private static BIReportingCopilot.Core.Models.Analytics.TemplateImprovementSuggestion MapToImprovementSuggestion(BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity entity)
    {
        return new BIReportingCopilot.Core.Models.Analytics.TemplateImprovementSuggestion
        {
            Id = entity.Id,
            TemplateKey = entity.TemplateKey,
            TemplateName = entity.TemplateName ?? string.Empty,
            Type = Enum.Parse<BIReportingCopilot.Core.Models.Analytics.ImprovementType>(entity.ImprovementType, true),
            CurrentVersion = entity.CurrentVersion ?? string.Empty,
            SuggestedChanges = entity.SuggestedChanges ?? string.Empty,
            ReasoningExplanation = entity.ReasoningExplanation ?? string.Empty,
            ExpectedImprovementPercent = entity.ExpectedImprovementPercent ?? 0,
            ConfidenceScore = entity.ConfidenceScore ?? 0,
            Status = Enum.Parse<SuggestionStatus>(entity.Status, true),
            CreatedDate = entity.CreatedDate
        };
    }

    #endregion
}