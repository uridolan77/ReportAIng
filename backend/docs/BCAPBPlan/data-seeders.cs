// Infrastructure/Data/Seeders/PromptTemplateSeeder.cs
namespace ReportAIng.Infrastructure.Data.Seeders
{
    public class PromptTemplateSeeder
    {
        private readonly BIReportingContext _context;

        public PromptTemplateSeeder(BIReportingContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.PromptTemplates.AnyAsync()) return;

            var templates = new List<PromptTemplateEntity>
            {
                new PromptTemplateEntity
                {
                    TemplateKey = "analytical_query_template",
                    IntentType = "Analytical",
                    Content = @"You are a SQL expert helping to analyze business data. 

## User Question
{USER_QUESTION}

## Business Context
{PRIMARY_TABLES}

## Business Rules to Consider:
{BUSINESS_RULES}

## Query Requirements
- Generate a SQL query for analytical purposes
- Focus on aggregations, calculations, and insights
- Consider performance for large datasets
- Include appropriate GROUP BY and ORDER BY clauses

## Examples
{EXAMPLES}

## Additional Context
- Current Date: {CURRENT_DATE}
- Identified Metrics: {METRICS}
- Identified Dimensions: {DIMENSIONS}
- Time Context: {TIME_CONTEXT}

Generate a well-structured SQL query that answers the user's analytical question.",
                    Description = "Template for complex analytical queries requiring aggregations and calculations",
                    Priority = 100,
                    IsActive = true,
                    Tags = "analytical,aggregation,calculation",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                
                new PromptTemplateEntity
                {
                    TemplateKey = "comparison_query_template",
                    IntentType = "Comparison",
                    Content = @"You are a SQL expert specializing in comparative analysis.

## User Question
{USER_QUESTION}

## Business Context
{PRIMARY_TABLES}

## Comparison Requirements
- Generate SQL that compares values across dimensions
- Use appropriate techniques (CASE, PIVOT, CTEs) for comparisons
- Ensure clear labeling of compared entities
- Consider using ratios and percentages where appropriate

## Examples
{EXAMPLES}

## Additional Context
- Comparison Terms: {COMPARISON_TERMS}
- Time Context: {TIME_CONTEXT}

Generate a SQL query that clearly shows the comparison requested by the user.",
                    Description = "Template for queries comparing values across different dimensions",
                    Priority = 100,
                    IsActive = true,
                    Tags = "comparison,analysis,pivot",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                
                new PromptTemplateEntity
                {
                    TemplateKey = "trend_analysis_template",
                    IntentType = "Trend",
                    Content = @"You are a SQL expert specializing in time-series and trend analysis.

## User Question
{USER_QUESTION}

## Business Context
{PRIMARY_TABLES}

## Trend Analysis Requirements
- Generate SQL for time-based analysis
- Include proper date grouping (daily, weekly, monthly, etc.)
- Consider using window functions for running totals or moving averages
- Ensure chronological ordering

## Time Context
{TIME_CONTEXT}

## Examples
{EXAMPLES}

Generate a SQL query that reveals trends and patterns over time.",
                    Description = "Template for time-series and trend analysis queries",
                    Priority = 100,
                    IsActive = true,
                    Tags = "trend,time-series,temporal",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },

                new PromptTemplateEntity
                {
                    TemplateKey = "exploratory_query_template",
                    IntentType = "Exploratory",
                    Content = @"You are a SQL expert helping users explore and understand their data.

## User Question
{USER_QUESTION}

## Business Context
{PRIMARY_TABLES}

## Exploration Guidelines
- Generate SQL that helps discover patterns and insights
- Include relevant sample data where appropriate
- Consider showing distributions, counts, and summaries
- Limit results appropriately for exploration (TOP/LIMIT)

## Examples
{EXAMPLES}

Generate a SQL query that helps the user explore and understand their data.",
                    Description = "Template for data exploration and discovery queries",
                    Priority = 100,
                    IsActive = true,
                    Tags = "exploration,discovery,analysis",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            await _context.PromptTemplates.AddRangeAsync(templates);
            await _context.SaveChangesAsync();
        }
    }
}

// Infrastructure/Data/Seeders/QueryExampleSeeder.cs
namespace ReportAIng.Infrastructure.Data.Seeders
{
    public class QueryExampleSeeder
    {
        private readonly BIReportingContext _context;

        public QueryExampleSeeder(BIReportingContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.QueryExamples.AnyAsync()) return;

            var examples = new List<QueryExampleEntity>
            {
                // Gaming Domain - Analytical Examples
                new QueryExampleEntity
                {
                    NaturalLanguageQuery = "What is the average revenue per user by game type last month?",
                    GeneratedSql = @"SELECT 
    g.GameType,
    COUNT(DISTINCT u.UserId) as TotalUsers,
    SUM(t.Amount) as TotalRevenue,
    CAST(SUM(t.Amount) / COUNT(DISTINCT u.UserId) AS DECIMAL(10,2)) as AvgRevenuePerUser
FROM Transactions t
INNER JOIN Users u ON t.UserId = u.UserId
INNER JOIN Games g ON t.GameId = g.GameId
WHERE t.TransactionDate >= DATEADD(MONTH, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0))
    AND t.TransactionDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
    AND t.TransactionType = 'Purchase'
GROUP BY g.GameType
ORDER BY AvgRevenuePerUser DESC",
                    IntentType = "Analytical",
                    Domain = "Gaming",
                    UsedTables = "Transactions,Users,Games",
                    BusinessContext = "Revenue analysis by game type with ARPU calculation",
                    SuccessRate = 0.95,
                    UsageCount = 150,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },

                // Gaming Domain - Comparison Example
                new QueryExampleEntity
                {
                    NaturalLanguageQuery = "Compare daily active users between mobile and desktop platforms this week vs last week",
                    GeneratedSql = @"WITH WeeklyDAU AS (
    SELECT 
        Platform,
        CASE 
            WHEN LoginDate >= DATEADD(WEEK, -1, CAST(GETDATE() AS DATE))
            THEN 'This Week'
            ELSE 'Last Week'
        END as WeekPeriod,
        COUNT(DISTINCT UserId) as DAU
    FROM UserSessions
    WHERE LoginDate >= DATEADD(WEEK, -2, CAST(GETDATE() AS DATE))
    GROUP BY 
        Platform,
        CASE 
            WHEN LoginDate >= DATEADD(WEEK, -1, CAST(GETDATE() AS DATE))
            THEN 'This Week'
            ELSE 'Last Week'
        END
)
SELECT 
    Platform,
    MAX(CASE WHEN WeekPeriod = 'Last Week' THEN DAU END) as LastWeekDAU,
    MAX(CASE WHEN WeekPeriod = 'This Week' THEN DAU END) as ThisWeekDAU,
    CAST((MAX(CASE WHEN WeekPeriod = 'This Week' THEN DAU END) - 
          MAX(CASE WHEN WeekPeriod = 'Last Week' THEN DAU END)) * 100.0 / 
          MAX(CASE WHEN WeekPeriod = 'Last Week' THEN DAU END) AS DECIMAL(5,2)) as PercentChange
FROM WeeklyDAU
GROUP BY Platform
ORDER BY Platform",
                    IntentType = "Comparison",
                    Domain = "Gaming",
                    UsedTables = "UserSessions",
                    BusinessContext = "Platform comparison of daily active users with week-over-week change",
                    SuccessRate = 0.92,
                    UsageCount = 87,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },

                // Finance Domain - Trend Example
                new QueryExampleEntity
                {
                    NaturalLanguageQuery = "Show monthly revenue trend for the past 6 months",
                    GeneratedSql = @"SELECT 
    YEAR(TransactionDate) as Year,
    MONTH(TransactionDate) as Month,
    DATENAME(MONTH, TransactionDate) + ' ' + CAST(YEAR(TransactionDate) AS VARCHAR) as MonthYear,
    SUM(Amount) as MonthlyRevenue,
    SUM(SUM(Amount)) OVER (ORDER BY YEAR(TransactionDate), MONTH(TransactionDate)) as CumulativeRevenue
FROM FinancialTransactions
WHERE TransactionDate >= DATEADD(MONTH, -6, DATEADD(DAY, 1, EOMONTH(GETDATE(), -1)))
    AND TransactionDate <= EOMONTH(GETDATE(), -1)
    AND TransactionType IN ('Sale', 'Subscription')
GROUP BY YEAR(TransactionDate), MONTH(TransactionDate), DATENAME(MONTH, TransactionDate)
ORDER BY Year, Month",
                    IntentType = "Trend",
                    Domain = "Finance",
                    UsedTables = "FinancialTransactions",
                    BusinessContext = "Monthly revenue trend with cumulative totals",
                    SuccessRate = 0.94,
                    UsageCount = 203,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },

                // Operations Domain - Aggregation Example
                new QueryExampleEntity
                {
                    NaturalLanguageQuery = "What are the top 10 products by sales volume this quarter?",
                    GeneratedSql = @"SELECT TOP 10
    p.ProductId,
    p.ProductName,
    p.Category,
    SUM(s.Quantity) as TotalQuantitySold,
    SUM(s.Quantity * s.UnitPrice) as TotalRevenue,
    COUNT(DISTINCT s.OrderId) as NumberOfOrders
FROM Sales s
INNER JOIN Products p ON s.ProductId = p.ProductId
WHERE s.SaleDate >= DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()), 0)
    AND s.SaleDate < GETDATE()
GROUP BY p.ProductId, p.ProductName, p.Category
ORDER BY TotalQuantitySold DESC",
                    IntentType = "Aggregation",
                    Domain = "Operations",
                    UsedTables = "Sales,Products",
                    BusinessContext = "Product performance analysis by sales volume",
                    SuccessRate = 0.96,
                    UsageCount = 178,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            await _context.QueryExamples.AddRangeAsync(examples);
            await _context.SaveChangesAsync();
        }
    }
}

// Infrastructure/Data/Seeders/BusinessDomainSeeder.cs
namespace ReportAIng.Infrastructure.Data.Seeders
{
    public class BusinessDomainSeeder
    {
        private readonly BIReportingContext _context;

        public BusinessDomainSeeder(BIReportingContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.BusinessDomains.AnyAsync()) return;

            var domains = new List<BusinessDomainEntity>
            {
                new BusinessDomainEntity
                {
                    DomainName = "Gaming",
                    BusinessFriendlyName = "Gaming Analytics",
                    Description = "Gaming industry analytics including player behavior, monetization, and engagement metrics",
                    RelatedTables = "Users,Games,UserSessions,Transactions,GameEvents,PlayerStats",
                    KeyConcepts = "DAU,MAU,ARPU,Retention,Churn,LTV,Engagement,Monetization",
                    CommonQueries = "daily active users,revenue per user,player retention,game performance",
                    BusinessOwner = "Gaming Analytics Team",
                    RelatedDomains = "Finance,Marketing",
                    ImportanceScore = 0.95m,
                    UsageFrequency = 0.90m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                
                new BusinessDomainEntity
                {
                    DomainName = "Finance",
                    BusinessFriendlyName = "Financial Analytics",
                    Description = "Financial data analysis including revenue, costs, profitability, and financial KPIs",
                    RelatedTables = "FinancialTransactions,Accounts,Budgets,CostCenters,Revenue",
                    KeyConcepts = "Revenue,Profit,Margin,ROI,Cash Flow,Budget,Forecast,P&L",
                    CommonQueries = "monthly revenue,profit margins,budget vs actual,financial forecast",
                    BusinessOwner = "Finance Department",
                    RelatedDomains = "Operations,Sales",
                    ImportanceScore = 1.0m,
                    UsageFrequency = 0.85m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                
                new BusinessDomainEntity
                {
                    DomainName = "Operations",
                    BusinessFriendlyName = "Operational Analytics",
                    Description = "Operational efficiency metrics including productivity, quality, and resource utilization",
                    RelatedTables = "Operations,Resources,Tasks,Performance,Quality",
                    KeyConcepts = "Efficiency,Productivity,Quality,SLA,Utilization,Throughput",
                    CommonQueries = "operational efficiency,resource utilization,quality metrics,SLA compliance",
                    BusinessOwner = "Operations Management",
                    RelatedDomains = "Finance,Customer",
                    ImportanceScore = 0.85m,
                    UsageFrequency = 0.75m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            await _context.BusinessDomains.AddRangeAsync(domains);
            await _context.SaveChangesAsync();
        }
    }
}

// Infrastructure/Data/Seeders/DatabaseSeeder.cs
namespace ReportAIng.Infrastructure.Data.Seeders
{
    public class DatabaseSeeder
    {
        private readonly BIReportingContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(BIReportingContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAllAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding");

                // Seed in dependency order
                await new BusinessDomainSeeder(_context).SeedAsync();
                _logger.LogInformation("Business domains seeded");

                await new PromptTemplateSeeder(_context).SeedAsync();
                _logger.LogInformation("Prompt templates seeded");

                await new QueryExampleSeeder(_context).SeedAsync();
                _logger.LogInformation("Query examples seeded");

                _logger.LogInformation("Database seeding completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding database");
                throw;
            }
        }
    }
}