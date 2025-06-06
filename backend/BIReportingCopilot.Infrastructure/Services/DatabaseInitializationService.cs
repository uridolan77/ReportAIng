using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Constants;
using BIReportingCopilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Services;

public interface IDatabaseInitializationService
{
    Task InitializeAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly BICopilotContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        BICopilotContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Check if we're using an in-memory database
            var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

            if (isInMemory)
            {
                // For in-memory database, just ensure it's created
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("In-memory database created successfully");
            }
            else
            {
                // For relational databases, apply migrations
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migrations applied successfully");
            }

            // Initialize query suggestions system if not already present
            await InitializeQuerySuggestionsAsync();

            // Initialize admin user password if not set
            await InitializeAdminUserAsync();

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization");
            throw;
        }
    }

    private async Task InitializeAdminUserAsync()
    {
        try
        {
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == ApplicationConstants.Roles.Admin.ToLower());

            if (adminUser == null)
            {
                _logger.LogWarning("Admin user not found in database");
                return;
            }

            // Check if admin user already has a password hash
            if (!string.IsNullOrEmpty(adminUser.PasswordHash))
            {
                _logger.LogInformation("Admin user already has password set");
                return;
            }

            // Get default admin password from configuration or use default
            var defaultPassword = _configuration["DefaultAdminPassword"] ?? "Admin123!";

            // Hash the password and update the user
            adminUser.PasswordHash = _passwordHasher.HashPassword(defaultPassword);
            adminUser.UpdatedDate = DateTime.UtcNow;
            adminUser.UpdatedBy = ApplicationConstants.EntityTypes.System;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin user password initialized successfully");
            _logger.LogWarning("Default admin credentials - Username: {Username}, Password: {Password}",
                ApplicationConstants.Roles.Admin.ToLower(), defaultPassword);
            _logger.LogWarning("Please change the default admin password after first login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing admin user password");
            throw;
        }
    }

    private async Task InitializeQuerySuggestionsAsync()
    {
        try
        {
            // Check if suggestion categories already exist
            var existingCategories = await _context.SuggestionCategories.AnyAsync();
            if (existingCategories)
            {
                _logger.LogInformation("Query suggestions system already initialized");
                return;
            }

            _logger.LogInformation("Initializing query suggestions system...");

            // Create suggestion categories with proper emoji icons
            var categories = new[]
            {
                new Core.Models.QuerySuggestions.SuggestionCategory
                {
                    CategoryKey = "financial",
                    Title = "💰 Financial & Revenue",
                    Icon = "💰",
                    Description = "Revenue, deposits, withdrawals, and financial performance metrics",
                    SortOrder = 1,
                    CreatedBy = "System"
                },
                new Core.Models.QuerySuggestions.SuggestionCategory
                {
                    CategoryKey = "players",
                    Title = "👥 Player Analytics",
                    Icon = "👥",
                    Description = "Player behavior, demographics, and lifecycle analysis",
                    SortOrder = 2,
                    CreatedBy = "System"
                },
                new Core.Models.QuerySuggestions.SuggestionCategory
                {
                    CategoryKey = "gaming",
                    Title = "🎮 Gaming & Products",
                    Icon = "🎮",
                    Description = "Game performance, provider analysis, and product metrics",
                    SortOrder = 3,
                    CreatedBy = "System"
                },
                new Core.Models.QuerySuggestions.SuggestionCategory
                {
                    CategoryKey = "transactions",
                    Title = "💳 Transactions & Payments",
                    Icon = "💳",
                    Description = "Payment methods, transaction analysis, and processing metrics",
                    SortOrder = 4,
                    CreatedBy = "System"
                },
                new Core.Models.QuerySuggestions.SuggestionCategory
                {
                    CategoryKey = "demographics",
                    Title = "📊 Demographics & Behavior",
                    Icon = "📊",
                    Description = "Player demographics, behavior patterns, and segmentation",
                    SortOrder = 5,
                    CreatedBy = "System"
                },
                new Core.Models.QuerySuggestions.SuggestionCategory
                {
                    CategoryKey = "accounts",
                    Title = "👤 Account & Status",
                    Icon = "👤",
                    Description = "Account management, verification, and player lifecycle",
                    SortOrder = 6,
                    CreatedBy = "System"
                }
            };

            _context.SuggestionCategories.AddRange(categories);
            await _context.SaveChangesAsync();

            // Add some basic suggestions for each category
            await AddBasicQuerySuggestionsAsync();

            _logger.LogInformation("Query suggestions categories created successfully with proper icons");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing query suggestions system");
        }
    }

    private async Task AddBasicQuerySuggestionsAsync()
    {
        try
        {
            var categories = await _context.SuggestionCategories.ToListAsync();
            var suggestions = new List<Core.Models.QuerySuggestions.QuerySuggestion>();

            foreach (var category in categories)
            {
                switch (category.CategoryKey)
                {
                    case "financial":
                        suggestions.AddRange(new[]
                        {
                            new Core.Models.QuerySuggestions.QuerySuggestion
                            {
                                CategoryId = category.Id,
                                QueryText = "Show me total deposits for yesterday",
                                Description = "Daily deposit performance tracking",
                                DefaultTimeFrame = "yesterday",
                                SortOrder = 1,
                                TargetTables = "[\"tbl_Daily_actions\"]",
                                Complexity = 1,
                                Tags = "[\"deposits\", \"daily\", \"financial\"]",
                                CreatedBy = "System"
                            },
                            new Core.Models.QuerySuggestions.QuerySuggestion
                            {
                                CategoryId = category.Id,
                                QueryText = "Revenue breakdown by country for last week",
                                Description = "Geographic revenue analysis and performance",
                                DefaultTimeFrame = "last_7_days",
                                SortOrder = 2,
                                TargetTables = "[\"tbl_Daily_actions\", \"tbl_Daily_actions_players\", \"tbl_Countries\"]",
                                Complexity = 2,
                                Tags = "[\"revenue\", \"geography\", \"countries\"]",
                                CreatedBy = "System"
                            }
                        });
                        break;

                    case "players":
                        suggestions.AddRange(new[]
                        {
                            new Core.Models.QuerySuggestions.QuerySuggestion
                            {
                                CategoryId = category.Id,
                                QueryText = "Top 10 players by deposits in the last 7 days",
                                Description = "High-value player identification and analysis",
                                DefaultTimeFrame = "last_7_days",
                                SortOrder = 1,
                                TargetTables = "[\"tbl_Daily_actions\", \"tbl_Daily_actions_players\"]",
                                Complexity = 2,
                                Tags = "[\"high_value\", \"players\", \"deposits\"]",
                                CreatedBy = "System"
                            },
                            new Core.Models.QuerySuggestions.QuerySuggestion
                            {
                                CategoryId = category.Id,
                                QueryText = "New player registrations by country this week",
                                Description = "Player acquisition analysis by geography",
                                DefaultTimeFrame = "this_week",
                                SortOrder = 2,
                                TargetTables = "[\"tbl_Daily_actions\", \"tbl_Daily_actions_players\", \"tbl_Countries\"]",
                                Complexity = 2,
                                Tags = "[\"registrations\", \"acquisition\", \"geography\"]",
                                CreatedBy = "System"
                            }
                        });
                        break;

                    case "gaming":
                        suggestions.AddRange(new[]
                        {
                            new Core.Models.QuerySuggestions.QuerySuggestion
                            {
                                CategoryId = category.Id,
                                QueryText = "Casino vs sports betting revenue split this month",
                                Description = "Product vertical comparison and analysis",
                                DefaultTimeFrame = "this_month",
                                SortOrder = 1,
                                TargetTables = "[\"tbl_Daily_actions\"]",
                                Complexity = 2,
                                Tags = "[\"casino\", \"sports\", \"verticals\", \"comparison\"]",
                                CreatedBy = "System"
                            }
                        });
                        break;

                    case "transactions":
                        suggestions.AddRange(new[]
                        {
                            new Core.Models.QuerySuggestions.QuerySuggestion
                            {
                                CategoryId = category.Id,
                                QueryText = "Transaction volumes by payment method today",
                                Description = "Payment method usage analysis",
                                DefaultTimeFrame = "today",
                                SortOrder = 1,
                                TargetTables = "[\"tbl_Daily_actions_transactions\"]",
                                Complexity = 2,
                                Tags = "[\"transactions\", \"payment_methods\", \"volumes\"]",
                                CreatedBy = "System"
                            }
                        });
                        break;
                }
            }

            if (suggestions.Any())
            {
                _context.QuerySuggestions.AddRange(suggestions);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added {Count} basic query suggestions", suggestions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding basic query suggestions");
        }
    }
}
