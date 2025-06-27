using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Infrastructure.AI.Core;
using BIReportingCopilot.Infrastructure.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BIReportingCopilot.Tests;

/// <summary>
/// Test fixture for setting up dependency injection and mock services
/// </summary>
public class TestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; private set; }

    public TestFixture()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Mock services
        var mockBusinessContextAnalyzer = new Mock<IEnhancedBusinessContextAnalyzer>();
        var mockMetadataService = new Mock<IBusinessMetadataRetrievalService>();
        var mockRelationshipService = new Mock<IForeignKeyRelationshipService>();
        var mockProcessFlowTracker = new Mock<ProcessFlowTracker>();

        // Setup mock behaviors
        SetupMockBusinessContextAnalyzer(mockBusinessContextAnalyzer);
        SetupMockMetadataService(mockMetadataService);
        SetupMockRelationshipService(mockRelationshipService);
        SetupMockProcessFlowTracker(mockProcessFlowTracker);

        // Register mocks
        services.AddSingleton(mockBusinessContextAnalyzer.Object);
        services.AddSingleton(mockMetadataService.Object);
        services.AddSingleton(mockRelationshipService.Object);
        services.AddSingleton(mockProcessFlowTracker.Object);

        // Register real services
        services.AddScoped<ISqlJoinGeneratorService, SqlJoinGeneratorService>();
        services.AddScoped<ISqlDateFilterService, SqlDateFilterService>();
        services.AddScoped<ISqlAggregationService, SqlAggregationService>();
        services.AddScoped<IEnhancedQueryProcessingService, EnhancedQueryProcessingService>();
    }

    private void SetupMockBusinessContextAnalyzer(Mock<IEnhancedBusinessContextAnalyzer> mock)
    {
        mock.Setup(x => x.AnalyzeUserQuestionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string question, string userId) => new BusinessContextProfile
            {
                OriginalQuestion = question,
                Intent = new BusinessIntent
                {
                    Type = DetermineIntentType(question),
                    Confidence = 0.8
                },
                Domain = new BusinessDomain
                {
                    Name = "Banking",
                    Confidence = 0.9
                },
                BusinessTerms = ExtractBusinessTerms(question),
                TimeContext = ExtractTimeContext(question),
                ConfidenceScore = 0.85
            });
    }

    private void SetupMockMetadataService(Mock<IBusinessMetadataRetrievalService> mock)
    {
        mock.Setup(x => x.GetRelevantBusinessMetadataAsync(It.IsAny<BusinessContextProfile>(), It.IsAny<int>()))
            .ReturnsAsync((BusinessContextProfile profile, int maxTables) => new BusinessMetadata
            {
                RelevantTables = new List<BusinessTableInfoDto>
                {
                    new BusinessTableInfoDto
                    {
                        TableName = "tbl_Daily_actions",
                        SchemaName = "dbo",
                        Description = "Daily player actions and transactions"
                    },
                    new BusinessTableInfoDto
                    {
                        TableName = "tbl_Countries",
                        SchemaName = "dbo",
                        Description = "Country reference data"
                    }
                },
                RelevantColumns = new List<BusinessColumnInfoDto>
                {
                    new BusinessColumnInfoDto
                    {
                        ColumnName = "DepositAmount",
                        TableName = "tbl_Daily_actions",
                        BusinessDataType = "Decimal",
                        Description = "Amount of deposit"
                    },
                    new BusinessColumnInfoDto
                    {
                        ColumnName = "CountryName",
                        TableName = "tbl_Countries",
                        BusinessDataType = "Text",
                        Description = "Country name"
                    },
                    new BusinessColumnInfoDto
                    {
                        ColumnName = "Date",
                        TableName = "tbl_Daily_actions",
                        BusinessDataType = "Date",
                        Description = "Transaction date"
                    }
                },
                TableRelationships = new List<TableRelationship>
                {
                    new TableRelationship
                    {
                        FromTable = "tbl_Daily_actions",
                        ToTable = "tbl_Countries",
                        RelationshipType = "ManyToOne"
                    }
                }
            });
    }

    private void SetupMockRelationshipService(Mock<IForeignKeyRelationshipService> mock)
    {
        mock.Setup(x => x.GetRelationshipsForTablesAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync((List<string> tableNames, string connectionString) => new List<ForeignKeyRelationship>
            {
                new ForeignKeyRelationship
                {
                    ParentTable = "tbl_Daily_actions",
                    ParentColumn = "CountryId",
                    ReferencedTable = "tbl_Countries",
                    ReferencedColumn = "Id",
                    IsEnabled = true,
                    RelationshipType = "ManyToOne"
                }
            });

        mock.Setup(x => x.GenerateJoinPathsAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync((List<string> tableNames, string connectionString) => new List<JoinPath>
            {
                new JoinPath
                {
                    FromTable = "tbl_Daily_actions",
                    ToTable = "tbl_Countries",
                    JoinConditions = new List<JoinCondition>
                    {
                        new JoinCondition
                        {
                            LeftTable = "tbl_Daily_actions",
                            LeftColumn = "CountryId",
                            RightTable = "tbl_Countries",
                            RightColumn = "Id"
                        }
                    },
                    IsOptimal = true,
                    PathLength = 1,
                    PerformanceScore = 0.9
                }
            });
    }

    private void SetupMockProcessFlowTracker(Mock<ProcessFlowTracker> mock)
    {
        mock.Setup(x => x.StartSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("test-session-123");

        mock.Setup(x => x.TrackStepAsync(It.IsAny<string>(), It.IsAny<Func<Task<object>>>(), It.IsAny<string>()))
            .Returns<string, Func<Task<object>>, string>((stepId, action, parentStepId) => action());

        mock.Setup(x => x.TrackStepWithConfidenceAsync(It.IsAny<string>(), It.IsAny<Func<Task<(object, decimal)>>>(), It.IsAny<string>()))
            .Returns<string, Func<Task<(object, decimal)>>, string>((stepId, action, parentStepId) => 
                action().ContinueWith(t => t.Result.Item1));

        mock.Setup(x => x.SetStepOutputAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.CompleteSessionAsync(It.IsAny<ProcessFlowStatus>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);
    }

    private IntentType DetermineIntentType(string question)
    {
        var lowerQuestion = question.ToLower();
        if (lowerQuestion.Contains("total") || lowerQuestion.Contains("sum") || lowerQuestion.Contains("count"))
            return IntentType.Analytics;
        if (lowerQuestion.Contains("show") || lowerQuestion.Contains("list"))
            return IntentType.Informational;
        return IntentType.General;
    }

    private List<string> ExtractBusinessTerms(string question)
    {
        var terms = new List<string>();
        var lowerQuestion = question.ToLower();
        
        if (lowerQuestion.Contains("depositor")) terms.Add("depositor");
        if (lowerQuestion.Contains("deposit")) terms.Add("deposit");
        if (lowerQuestion.Contains("country")) terms.Add("country");
        if (lowerQuestion.Contains("revenue")) terms.Add("revenue");
        if (lowerQuestion.Contains("total")) terms.Add("total");
        
        return terms;
    }

    private TimeRange? ExtractTimeContext(string question)
    {
        var lowerQuestion = question.ToLower();
        
        if (lowerQuestion.Contains("yesterday"))
        {
            return new TimeRange
            {
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today,
                RelativeExpression = "yesterday",
                Granularity = TimeGranularity.Day
            };
        }
        
        if (lowerQuestion.Contains("last month"))
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            return new TimeRange
            {
                StartDate = startOfMonth,
                EndDate = startOfMonth.AddMonths(1).AddDays(-1),
                RelativeExpression = "last month",
                Granularity = TimeGranularity.Month
            };
        }
        
        return null;
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
