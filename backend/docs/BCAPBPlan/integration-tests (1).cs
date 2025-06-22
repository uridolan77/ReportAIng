// Tests/Integration/BusinessContextAnalyzerTests.cs
namespace ReportAIng.Tests.Integration
{
    public class BusinessContextAnalyzerTests : IntegrationTestBase
    {
        private readonly IBusinessContextAnalyzer _analyzer;
        private readonly IBusinessGlossaryRepository _glossaryRepository;

        public BusinessContextAnalyzerTests(TestWebApplicationFactory factory) : base(factory)
        {
            _analyzer = GetService<IBusinessContextAnalyzer>();
            _glossaryRepository = GetService<IBusinessGlossaryRepository>();
        }

        [Fact]
        public async Task AnalyzeUserQuestion_WithGamingQuery_ReturnsCorrectContext()
        {
            // Arrange
            var question = "Show me daily active users for mobile games last month";

            // Act
            var result = await _analyzer.AnalyzeUserQuestionAsync(question, "test-user");

            // Assert
            result.Should().NotBeNull();
            result.Intent.Type.Should().Be(IntentType.Analytical);
            result.Domain.Name.Should().Contain("Gaming");
            result.Entities.Should().Contain(e => e.Name.Contains("users", StringComparison.OrdinalIgnoreCase));
            result.TimeContext.Should().NotBeNull();
            result.TimeContext!.RelativeExpression.Should().Be("last month");
        }

        [Fact]
        public async Task ExtractBusinessTerms_WithKnownTerms_ReturnsMatchedTerms()
        {
            // Arrange
            await SeedGlossaryTerm("DAU", "Daily Active Users");
            await SeedGlossaryTerm("ARPU", "Average Revenue Per User");
            var question = "What is the DAU and ARPU for our top games?";

            // Act
            var terms = await _analyzer.ExtractBusinessTermsAsync(question);

            // Assert
            terms.Should().Contain("DAU");
            terms.Should().Contain("ARPU");
        }

        [Fact]
        public async Task ClassifyBusinessIntent_WithComparisonQuery_ReturnsComparisonIntent()
        {
            // Arrange
            var question = "Compare revenue between iOS and Android platforms";

            // Act
            var intent = await _analyzer.ClassifyBusinessIntentAsync(question);

            // Assert
            intent.Type.Should().Be(IntentType.Comparison);
            intent.ConfidenceScore.Should().BeGreaterThan(0.7);
        }

        [Fact]
        public async Task ExtractTimeContext_WithRelativeTime_ExtractsCorrectly()
        {
            // Arrange
            var testCases = new[]
            {
                ("Show data for last week", "last week", TimeGranularity.Week),
                ("Revenue year to date", "year to date", TimeGranularity.Year),
                ("Daily stats for March 2024", "March 2024", TimeGranularity.Day)
            };

            foreach (var (question, expectedExpression, expectedGranularity) in testCases)
            {
                // Act
                var timeContext = await _analyzer.ExtractTimeContextAsync(question);

                // Assert
                timeContext.Should().NotBeNull();
                timeContext!.RelativeExpression.Should().Contain(expectedExpression);
                timeContext.Granularity.Should().Be(expectedGranularity);
            }
        }

        private async Task SeedGlossaryTerm(string term, string definition)
        {
            var glossaryTerm = new BusinessGlossaryEntity
            {
                Term = term,
                Definition = definition,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Test"
            };

            await _glossaryRepository.AddAsync(glossaryTerm);
            await _glossaryRepository.SaveChangesAsync();
        }
    }
}

// Tests/Integration/BusinessMetadataRetrievalServiceTests.cs
namespace ReportAIng.Tests.Integration
{
    public class BusinessMetadataRetrievalServiceTests : IntegrationTestBase
    {
        private readonly IBusinessMetadataRetrievalService _metadataService;
        private readonly IBusinessTableRepository _tableRepository;

        public BusinessMetadataRetrievalServiceTests(TestWebApplicationFactory factory) : base(factory)
        {
            _metadataService = GetService<IBusinessMetadataRetrievalService>();
            _tableRepository = GetService<IBusinessTableRepository>();
        }

        [Fact]
        public async Task GetRelevantBusinessMetadata_WithValidProfile_ReturnsRelevantSchema()
        {
            // Arrange
            await SeedBusinessTables();
            var profile = CreateTestProfile(IntentType.Analytical, "Gaming");

            // Act
            var schema = await _metadataService.GetRelevantBusinessMetadataAsync(profile, 5);

            // Assert
            schema.Should().NotBeNull();
            schema.RelevantTables.Should().NotBeEmpty();
            schema.RelevantTables.Should().HaveCountLessOrEqualTo(5);
            schema.RelevantTables.Should().Contain(t => t.DomainClassification.Contains("Gaming"));
        }

        [Fact]
        public async Task FindRelevantTables_WithEntityMatching_ReturnsMatchedTables()
        {
            // Arrange
            await SeedBusinessTables();
            var profile = new BusinessContextProfile
            {
                Entities = new List<BusinessEntity>
                {
                    new() { Name = "Users", Type = EntityType.Table, ConfidenceScore = 0.9 },
                    new() { Name = "revenue", Type = EntityType.Metric, ConfidenceScore = 0.8 }
                }
            };

            // Act
            var tables = await _metadataService.FindRelevantTablesAsync(profile, 10);

            // Assert
            tables.Should().Contain(t => t.TableName.Equals("Users", StringComparison.OrdinalIgnoreCase));
            tables.Should().OnlyContain(t => t.IsActive);
        }

        [Fact]
        public async Task DiscoverTableRelationships_WithRelatedTables_ReturnsRelationships()
        {
            // Arrange
            var tableNames = new List<string> { "dbo.Users", "dbo.Transactions" };

            // Act
            var relationships = await _metadataService.DiscoverTableRelationshipsAsync(tableNames);

            // Assert
            relationships.Should().NotBeNull();
            // Actual assertion depends on your test database setup
        }

        private async Task SeedBusinessTables()
        {
            var tables = new List<BusinessTableInfoEntity>
            {
                new()
                {
                    TableName = "Users",
                    SchemaName = "dbo",
                    BusinessPurpose = "Store user information",
                    DomainClassification = "Gaming,Customer",
                    ImportanceScore = 0.9m,
                    UsageFrequency = 0.95m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "Test"
                },
                new()
                {
                    TableName = "Transactions",
                    SchemaName = "dbo",
                    BusinessPurpose = "Financial transactions",
                    DomainClassification = "Finance,Gaming",
                    ImportanceScore = 0.95m,
                    UsageFrequency = 0.9m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "Test"
                }
            };

            foreach (var table in tables)
            {
                await _tableRepository.AddAsync(table);
            }
            await _tableRepository.SaveChangesAsync();
        }

        private BusinessContextProfile CreateTestProfile(IntentType intent, string domain)
        {
            return new BusinessContextProfile
            {
                Intent = new QueryIntent { Type = intent, ConfidenceScore = 0.9 },
                Domain = new BusinessDomain { Name = domain, RelevanceScore = 0.85 },
                ConfidenceScore = 0.9
            };
        }
    }
}

// Tests/Integration/ContextualPromptBuilderTests.cs
namespace ReportAIng.Tests.Integration
{
    public class ContextualPromptBuilderTests : IntegrationTestBase
    {
        private readonly IContextualPromptBuilder _promptBuilder;
        private readonly IPromptTemplateRepository _templateRepository;

        public ContextualPromptBuilderTests(TestWebApplicationFactory factory) : base(factory)
        {
            _promptBuilder = GetService<IContextualPromptBuilder>();
            _templateRepository = GetService<IPromptTemplateRepository>();
        }

        [Fact]
        public async Task BuildBusinessAwarePrompt_WithCompleteContext_GeneratesValidPrompt()
        {
            // Arrange
            await SeedPromptTemplates();
            var userQuestion = "What is the average revenue per user last month?";
            var profile = CreateAnalyticalProfile();
            var schema = CreateTestSchema();

            // Act
            var prompt = await _promptBuilder.BuildBusinessAwarePromptAsync(
                userQuestion, profile, schema);

            // Assert
            prompt.Should().NotBeNullOrEmpty();
            prompt.Should().Contain(userQuestion);
            prompt.Should().Contain("Business Context");
            prompt.Should().Contain("analytical");
        }

        [Fact]
        public async Task SelectOptimalTemplate_ForDifferentIntents_SelectsCorrectTemplate()
        {
            // Arrange
            await SeedPromptTemplates();
            var intents = new[] 
            { 
                IntentType.Analytical, 
                IntentType.Comparison, 
                IntentType.Trend 
            };

            foreach (var intentType in intents)
            {
                var profile = new BusinessContextProfile
                {
                    Intent = new QueryIntent { Type = intentType }
                };

                // Act
                var template = await _promptBuilder.SelectOptimalTemplateAsync(profile);

                // Assert
                template.Should().NotBeNull();
                template.IntentType.Should().Be(intentType.ToString());
            }
        }

        [Fact]
        public async Task EnrichPromptWithBusinessContext_AddsAllContextSections()
        {
            // Arrange
            var basePrompt = "Generate SQL for: {USER_QUESTION}";
            var schema = CreateTestSchema();

            // Act
            var enrichedPrompt = await _promptBuilder.EnrichPromptWithBusinessContextAsync(
                basePrompt, schema);

            // Assert
            enrichedPrompt.Should().Contain("Business Context");
            enrichedPrompt.Should().Contain("Relevant Tables");
            if (schema.BusinessRules.Any())
                enrichedPrompt.Should().Contain("Business Rules");
            if (schema.RelevantGlossaryTerms.Any())
                enrichedPrompt.Should().Contain("Business Term Definitions");
        }

        private async Task SeedPromptTemplates()
        {
            // Use the PromptTemplateSeeder
            var seeder = new PromptTemplateSeeder(Context);
            await seeder.SeedAsync();
        }

        private BusinessContextProfile CreateAnalyticalProfile()
        {
            return new BusinessContextProfile
            {
                OriginalQuestion = "What is the average revenue per user?",
                Intent = new QueryIntent 
                { 
                    Type = IntentType.Analytical, 
                    ConfidenceScore = 0.9 
                },
                Domain = new BusinessDomain 
                { 
                    Name = "Finance", 
                    RelevanceScore = 0.85 
                },
                IdentifiedMetrics = new List<string> { "revenue", "users" },
                TimeContext = new TimeRange 
                { 
                    RelativeExpression = "last month" 
                }
            };
        }

        private ContextualBusinessSchema CreateTestSchema()
        {
            return new ContextualBusinessSchema
            {
                RelevantTables = new List<BusinessTableInfoDto>
                {
                    new()
                    {
                        Id = 1,
                        TableName = "Users",
                        SchemaName = "dbo",
                        BusinessPurpose = "User information",
                        PrimaryUseCase = "User analytics"
                    }
                },
                BusinessRules = new List<BusinessRule>
                {
                    new()
                    {
                        Description = "Only include active users",
                        Type = RuleType.Filter,
                        SqlExpression = "IsActive = 1"
                    }
                },
                RelevantGlossaryTerms = new List<BusinessGlossaryDto>
                {
                    new()
                    {
                        Term = "ARPU",
                        Definition = "Average Revenue Per User",
                        PreferredCalculation = "SUM(Revenue) / COUNT(DISTINCT UserId)"
                    }
                },
                Complexity = SchemaComplexity.Moderate
            };
        }
    }
}

// Tests/Integration/EndToEndPromptGenerationTests.cs
namespace ReportAIng.Tests.Integration
{
    public class EndToEndPromptGenerationTests : IntegrationTestBase
    {
        private readonly HttpClient _client;

        public EndToEndPromptGenerationTests(TestWebApplicationFactory factory) : base(factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", GetTestToken());
        }

        [Fact]
        public async Task GenerateBusinessAwarePrompt_CompleteFlow_Success()
        {
            // Arrange
            await SeedAllRequiredData();
            var request = new BusinessPromptRequest
            {
                UserQuestion = "Show me the top 10 games by revenue last quarter",
                UserId = "test-user",
                PreferredDomain = BusinessDomainType.Gaming,
                ComplexityLevel = PromptComplexityLevel.Standard,
                IncludeExamples = true,
                IncludeBusinessRules = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/prompts/business-aware", request);

            // Assert
            response.Should().BeSuccessful();
            var result = await response.Content.ReadFromJsonAsync<BusinessAwarePromptResponse>();
            
            result.Should().NotBeNull();
            result!.GeneratedPrompt.Should().NotBeNullOrEmpty();
            result.ContextProfile.Should().NotBeNull();
            result.ContextProfile.Intent.Type.Should().Be(IntentType.Analytical);
            result.UsedSchema.RelevantTables.Should().NotBeEmpty();
            result.ConfidenceScore.Should().BeGreaterThan(0.5);
        }

        [Theory]
        [InlineData("What's our DAU?", IntentType.Analytical)]
        [InlineData("Compare iOS vs Android revenue", IntentType.Comparison)]
        [InlineData("Show monthly user growth trend", IntentType.Trend)]
        public async Task AnalyzeBusinessContext_DifferentQueries_CorrectIntentClassification(
            string question, IntentType expectedIntent)
        {
            // Arrange
            await SeedAllRequiredData();
            var request = new ContextAnalysisRequest
            {
                UserQuestion = question,
                UserId = "test-user"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/prompts/analyze-context", request);

            // Assert
            response.Should().BeSuccessful();
            var result = await response.Content.ReadFromJsonAsync<BusinessContextAnalysisResponse>();
            
            result.Should().NotBeNull();
            result!.ContextProfile.Intent.Type.Should().Be(expectedIntent);
        }

        private async Task SeedAllRequiredData()
        {
            var seeder = new DatabaseSeeder(Context, GetService<ILogger<DatabaseSeeder>>());
            await seeder.SeedAllAsync();
        }
    }
}