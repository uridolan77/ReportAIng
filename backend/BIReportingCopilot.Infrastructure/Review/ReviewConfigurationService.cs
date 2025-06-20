using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Models;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Infrastructure.Review;

/// <summary>
/// Phase 4: Review Configuration Service Implementation
/// Manages configuration settings for human review workflows
/// </summary>
public class ReviewConfigurationService : IReviewConfigurationService
{
    private readonly ILogger<ReviewConfigurationService> _logger;
    
    // In-memory storage for demo (would be database in production)
    private ReviewConfiguration _configuration;
    private readonly ConcurrentDictionary<ReviewType, ReviewTypeConfig> _typeConfigurations = new();

    public ReviewConfigurationService(ILogger<ReviewConfigurationService> logger)
    {
        _logger = logger;
        InitializeDefaultConfiguration();
    }

    /// <summary>
    /// Get review configuration
    /// </summary>
    public async Task<ReviewConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation
            return _configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting review configuration");
            throw;
        }
    }

    /// <summary>
    /// Update review configuration
    /// </summary>
    public async Task<bool> UpdateConfigurationAsync(
        ReviewConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("⚙️ Updating review configuration");

            // Validate configuration
            var validationResult = await ValidateConfigurationAsync(configuration, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("⚠️ Configuration validation failed: {Errors}", 
                    string.Join(", ", validationResult.Errors));
                return false;
            }

            _configuration = configuration;

            // Update type configurations
            foreach (var typeConfig in configuration.TypeConfigurations)
            {
                _typeConfigurations[typeConfig.Key] = typeConfig.Value;
            }

            _logger.LogInformation("✅ Review configuration updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating review configuration");
            return false;
        }
    }

    /// <summary>
    /// Get configuration for specific review type
    /// </summary>
    public async Task<ReviewTypeConfig> GetTypeConfigurationAsync(
        ReviewType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            return _typeConfigurations.TryGetValue(type, out var config) 
                ? config 
                : GetDefaultTypeConfiguration(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting type configuration for: {Type}", type);
            return GetDefaultTypeConfiguration(type);
        }
    }

    /// <summary>
    /// Update configuration for specific review type
    /// </summary>
    public async Task<bool> UpdateTypeConfigurationAsync(
        ReviewType type,
        ReviewTypeConfig configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("⚙️ Updating type configuration for: {Type}", type);

            await Task.Delay(1, cancellationToken); // Simulate async operation

            _typeConfigurations[type] = configuration;
            
            // Update main configuration
            _configuration.TypeConfigurations[type] = configuration;

            _logger.LogInformation("✅ Type configuration updated successfully for: {Type}", type);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating type configuration for: {Type}", type);
            return false;
        }
    }

    /// <summary>
    /// Validate configuration
    /// </summary>
    public async Task<ConfigurationValidationResult> ValidateConfigurationAsync(
        ReviewConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            var result = new ConfigurationValidationResult { IsValid = true };

            // Validate thresholds
            if (configuration.AutoApprovalThreshold < 0 || configuration.AutoApprovalThreshold > 1)
            {
                result.Errors.Add("AutoApprovalThreshold must be between 0 and 1");
                result.IsValid = false;
            }

            if (configuration.ManualReviewThreshold < 0 || configuration.ManualReviewThreshold > 1)
            {
                result.Errors.Add("ManualReviewThreshold must be between 0 and 1");
                result.IsValid = false;
            }

            if (configuration.AutoApprovalThreshold <= configuration.ManualReviewThreshold)
            {
                result.Warnings.Add("AutoApprovalThreshold should be higher than ManualReviewThreshold");
            }

            // Validate timeouts
            if (configuration.DefaultReviewTimeout <= TimeSpan.Zero)
            {
                result.Errors.Add("DefaultReviewTimeout must be positive");
                result.IsValid = false;
            }

            if (configuration.NotificationInterval <= TimeSpan.Zero)
            {
                result.Errors.Add("NotificationInterval must be positive");
                result.IsValid = false;
            }

            // Validate type configurations
            foreach (var typeConfig in configuration.TypeConfigurations)
            {
                var typeValidation = ValidateTypeConfiguration(typeConfig.Key, typeConfig.Value);
                result.Errors.AddRange(typeValidation.Errors);
                result.Warnings.AddRange(typeValidation.Warnings);
                
                if (!typeValidation.IsValid)
                {
                    result.IsValid = false;
                }
            }

            // Add suggestions
            if (configuration.AutoReviewEnabled && configuration.AutoApprovalThreshold > 0.95)
            {
                result.Suggestions.Add("Consider lowering AutoApprovalThreshold to allow more human oversight");
            }

            if (!configuration.NotificationsEnabled)
            {
                result.Warnings.Add("Notifications are disabled - users may not be aware of pending reviews");
            }

            _logger.LogDebug("✅ Configuration validation completed. Valid: {IsValid}", result.IsValid);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating configuration");
            return new ConfigurationValidationResult 
            { 
                IsValid = false, 
                Errors = new List<string> { "Validation failed due to internal error" } 
            };
        }
    }

    // Private helper methods
    private void InitializeDefaultConfiguration()
    {
        _configuration = new ReviewConfiguration
        {
            AutoReviewEnabled = true,
            AutoApprovalThreshold = 0.9,
            ManualReviewThreshold = 0.7,
            DefaultReviewTimeout = TimeSpan.FromHours(24),
            RequiredApprovalRoles = new List<string> { "Admin", "ReviewManager", "SeniorDeveloper" },
            NotificationsEnabled = true,
            NotificationInterval = TimeSpan.FromHours(4)
        };

        // Initialize type configurations
        InitializeTypeConfigurations();

        _logger.LogInformation("✅ Initialized default review configuration");
    }

    private void InitializeTypeConfigurations()
    {
        // SQL Validation Configuration
        var sqlValidationConfig = new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "Developer", "SeniorDeveloper" },
            Timeout = TimeSpan.FromHours(8),
            DefaultPriority = ReviewPriority.Normal,
            AutoAssignEnabled = true,
            AutoAssignToRoles = new List<string> { "Developer" }
        };

        // Security Review Configuration
        var securityReviewConfig = new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "SecurityAnalyst", "SecurityOfficer", "SecurityManager" },
            Timeout = TimeSpan.FromHours(24),
            DefaultPriority = ReviewPriority.High,
            AutoAssignEnabled = true,
            AutoAssignToRoles = new List<string> { "SecurityAnalyst" }
        };

        // Business Logic Configuration
        var businessLogicConfig = new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "BusinessAnalyst", "ProductOwner" },
            Timeout = TimeSpan.FromHours(12),
            DefaultPriority = ReviewPriority.Normal,
            AutoAssignEnabled = true,
            AutoAssignToRoles = new List<string> { "BusinessAnalyst" }
        };

        // Semantic Alignment Configuration
        var semanticAlignmentConfig = new ReviewTypeConfig
        {
            RequiresApproval = false,
            RequiredRoles = new List<string> { "DataAnalyst" },
            Timeout = TimeSpan.FromHours(4),
            DefaultPriority = ReviewPriority.Low,
            AutoAssignEnabled = true,
            AutoAssignToRoles = new List<string> { "DataAnalyst" }
        };

        // Performance Review Configuration
        var performanceReviewConfig = new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "DatabaseAdmin", "PerformanceEngineer" },
            Timeout = TimeSpan.FromHours(6),
            DefaultPriority = ReviewPriority.High,
            AutoAssignEnabled = true,
            AutoAssignToRoles = new List<string> { "DatabaseAdmin" }
        };

        // Compliance Review Configuration
        var complianceReviewConfig = new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "ComplianceOfficer", "LegalTeam" },
            Timeout = TimeSpan.FromHours(48),
            DefaultPriority = ReviewPriority.Critical,
            AutoAssignEnabled = false,
            AutoAssignToRoles = new List<string>()
        };

        // Data Access Configuration
        var dataAccessConfig = new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "DataSteward", "DataOwner" },
            Timeout = TimeSpan.FromHours(16),
            DefaultPriority = ReviewPriority.Normal,
            AutoAssignEnabled = true,
            AutoAssignToRoles = new List<string> { "DataSteward" }
        };

        // Sensitive Data Configuration
        var sensitiveDataConfig = new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "DataProtectionOfficer", "SecurityManager", "ComplianceOfficer" },
            Timeout = TimeSpan.FromHours(72),
            DefaultPriority = ReviewPriority.Critical,
            AutoAssignEnabled = false,
            AutoAssignToRoles = new List<string>()
        };

        _typeConfigurations[ReviewType.SqlValidation] = sqlValidationConfig;
        _typeConfigurations[ReviewType.SecurityReview] = securityReviewConfig;
        _typeConfigurations[ReviewType.BusinessLogic] = businessLogicConfig;
        _typeConfigurations[ReviewType.SemanticAlignment] = semanticAlignmentConfig;
        _typeConfigurations[ReviewType.PerformanceReview] = performanceReviewConfig;
        _typeConfigurations[ReviewType.ComplianceReview] = complianceReviewConfig;
        _typeConfigurations[ReviewType.DataAccess] = dataAccessConfig;
        _typeConfigurations[ReviewType.SensitiveData] = sensitiveDataConfig;

        _configuration.TypeConfigurations = _typeConfigurations.ToDictionary(kv => kv.Key, kv => kv.Value);

        _logger.LogInformation("✅ Initialized {Count} type configurations", _typeConfigurations.Count);
    }

    private ReviewTypeConfig GetDefaultTypeConfiguration(ReviewType type)
    {
        return new ReviewTypeConfig
        {
            RequiresApproval = true,
            RequiredRoles = new List<string> { "Admin" },
            Timeout = TimeSpan.FromHours(24),
            DefaultPriority = ReviewPriority.Normal,
            AutoAssignEnabled = false,
            AutoAssignToRoles = new List<string>()
        };
    }

    private ConfigurationValidationResult ValidateTypeConfiguration(ReviewType type, ReviewTypeConfig config)
    {
        var result = new ConfigurationValidationResult { IsValid = true };

        // Validate timeout
        if (config.Timeout <= TimeSpan.Zero)
        {
            result.Errors.Add($"{type}: Timeout must be positive");
            result.IsValid = false;
        }

        // Validate roles
        if (config.RequiresApproval && !config.RequiredRoles.Any())
        {
            result.Warnings.Add($"{type}: RequiresApproval is true but no RequiredRoles specified");
        }

        if (config.AutoAssignEnabled && !config.AutoAssignToRoles.Any())
        {
            result.Warnings.Add($"{type}: AutoAssignEnabled is true but no AutoAssignToRoles specified");
        }

        // Type-specific validations
        switch (type)
        {
            case ReviewType.SecurityReview:
            case ReviewType.SensitiveData:
                if (config.Timeout < TimeSpan.FromHours(24))
                {
                    result.Suggestions.Add($"{type}: Consider longer timeout for security-sensitive reviews");
                }
                break;

            case ReviewType.SemanticAlignment:
                if (config.RequiresApproval)
                {
                    result.Suggestions.Add($"{type}: Consider disabling approval requirement for semantic reviews");
                }
                break;
        }

        return result;
    }
}
