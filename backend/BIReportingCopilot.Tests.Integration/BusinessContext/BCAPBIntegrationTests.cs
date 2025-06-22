using BIReportingCopilot.API.Controllers;
using BIReportingCopilot.Core.Models.BusinessContext;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace BIReportingCopilot.Tests.Integration.BusinessContext;

public class BCAPBIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BCAPBIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task BusinessContext_CompleteWorkflow_GeneratesBusinessAwarePrompt()
    {
        // Arrange
        var request = new BusinessPromptRequest
        {
            UserQuestion = "Show me daily active users for mobile games last month",
            UserId = "test-user-integration",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = PromptComplexityLevel.Standard,
            IncludeExamples = true,
            IncludeBusinessRules = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/prompt", request);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<BusinessAwarePromptResponse>();
        
        result.Should().NotBeNull();
        result!.GeneratedPrompt.Should().NotBeNullOrEmpty();
        result.ContextProfile.Should().NotBeNull();
        result.ContextProfile.Intent.Type.Should().Be(IntentType.Analytical);
        result.ContextProfile.Domain.Type.Should().Be(BusinessDomainType.Gaming);
        result.UsedSchema.Should().NotBeNull();
        result.UsedSchema.RelevantTables.Should().NotBeEmpty();
        result.ConfidenceScore.Should().BeGreaterThan(0.5);
        result.ProcessingTimeMs.Should().BeLessThan(5000); // 5 second timeout
    }

    [Theory]
    [InlineData("What's our daily active users?", IntentType.Analytical, BusinessDomainType.Gaming)]
    [InlineData("Compare iOS vs Android revenue", IntentType.Comparison, BusinessDomainType.Gaming)]
    [InlineData("Show monthly user growth trend", IntentType.Trend, BusinessDomainType.Gaming)]
    [InlineData("List all active games", IntentType.Detail, BusinessDomainType.Gaming)]
    [InlineData("How many players registered today?", IntentType.Operational, BusinessDomainType.Gaming)]
    public async Task AnalyzeBusinessContext_VariousQueries_ClassifiesCorrectly(
        string userQuestion, IntentType expectedIntent, BusinessDomainType expectedDomain)
    {
        // Arrange
        var request = new ContextAnalysisRequest
        {
            UserQuestion = userQuestion,
            UserId = "test-user"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/analyze", request);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<BusinessContextAnalysisResponse>();
        
        result.Should().NotBeNull();
        result!.ContextProfile.Intent.Type.Should().Be(expectedIntent);
        result.ContextProfile.Domain.Type.Should().Be(expectedDomain);
        result.ContextProfile.ConfidenceScore.Should().BeGreaterThan(0.6);
        result.ProcessingTimeMs.Should().BeLessThan(2000); // 2 second timeout for analysis
    }

    [Fact]
    public async Task GetRelevantMetadata_WithGamingQuery_ReturnsGamingTables()
    {
        // Arrange
        var request = new MetadataRetrievalRequest
        {
            UserQuestion = "Show me player revenue and engagement metrics",
            Domain = BusinessDomainType.Gaming,
            MaxTables = 5,
            IncludeRelationships = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/metadata", request);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<BusinessMetadataResponse>();
        
        result.Should().NotBeNull();
        result!.RelevantTables.Should().NotBeEmpty();
        result.RelevantTables.Should().HaveCountLessOrEqualTo(5);
        result.RelevantTables.Should().Contain(t => 
            t.DomainClassification.Contains("Gaming", StringComparison.OrdinalIgnoreCase));
        result.TableRelationships.Should().NotBeEmpty();
        result.ProcessingTimeMs.Should().BeLessThan(1000); // 1 second for metadata retrieval
    }

    [Fact]
    public async Task ExtractBusinessEntities_WithComplexQuery_ExtractsAllEntityTypes()
    {
        // Arrange
        var request = new EntityExtractionRequest
        {
            UserQuestion = "Show me daily revenue from mobile games for premium users in Q1 2024",
            IncludeConfidenceScores = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/entities", request);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<EntityExtractionResponse>();
        
        result.Should().NotBeNull();
        result!.ExtractedEntities.Should().NotBeEmpty();
        
        // Should extract different entity types
        result.ExtractedEntities.Should().Contain(e => e.Type == EntityType.Table);
        result.ExtractedEntities.Should().Contain(e => e.Type == EntityType.Metric);
        result.ExtractedEntities.Should().Contain(e => e.Type == EntityType.Dimension);
        result.ExtractedEntities.Should().Contain(e => e.Type == EntityType.DateRange);
        
        // All entities should have confidence scores
        result.ExtractedEntities.Should().OnlyContain(e => e.ConfidenceScore > 0);
        result.ProcessingTimeMs.Should().BeLessThan(1500);
    }

    [Fact]
    public async Task FindRelevantTables_WithSemanticSearch_ReturnsRankedResults()
    {
        // Arrange
        var request = new TableSearchRequest
        {
            SearchQuery = "user activity and engagement data",
            Domain = BusinessDomainType.Gaming,
            MaxResults = 10,
            MinimumRelevanceScore = 0.5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/tables", request);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<TableSearchResponse>();
        
        result.Should().NotBeNull();
        result!.RelevantTables.Should().NotBeEmpty();
        result.RelevantTables.Should().HaveCountLessOrEqualTo(10);
        result.RelevantTables.Should().BeInDescendingOrder(t => t.RelevanceScore);
        result.RelevantTables.Should().OnlyContain(t => t.RelevanceScore >= 0.5);
        result.ProcessingTimeMs.Should().BeLessThan(800);
    }

    [Fact]
    public async Task ClassifyQueryIntent_WithVariousComplexities_ReturnsAccurateClassification()
    {
        // Arrange
        var simpleQuery = "Show me users";
        var complexQuery = "Compare year-over-year revenue growth by game category, showing seasonal trends and statistical significance";

        var simpleRequest = new IntentClassificationRequest { UserQuestion = simpleQuery };
        var complexRequest = new IntentClassificationRequest { UserQuestion = complexQuery };

        // Act
        var simpleResponse = await _client.PostAsJsonAsync("/api/businesscontext/intent", simpleRequest);
        var complexResponse = await _client.PostAsJsonAsync("/api/businesscontext/intent", complexRequest);

        // Assert
        simpleResponse.Should().BeSuccessful();
        complexResponse.Should().BeSuccessful();

        var simpleResult = await simpleResponse.Content.ReadFromJsonAsync<IntentClassificationResponse>();
        var complexResult = await complexResponse.Content.ReadFromJsonAsync<IntentClassificationResponse>();

        simpleResult.Should().NotBeNull();
        complexResult.Should().NotBeNull();

        simpleResult!.Intent.Type.Should().Be(IntentType.Listing);
        complexResult!.Intent.Type.Should().Be(IntentType.Analytical);
        
        // Complex query should have higher complexity score
        complexResult.Intent.ComplexityScore.Should().BeGreaterThan(simpleResult.Intent.ComplexityScore);
    }

    [Fact]
    public async Task EmbeddingsGeneration_CompleteFlow_GeneratesAndTestsEmbeddings()
    {
        // Arrange & Act - Generate prompt template embeddings
        var templateResponse = await _client.PostAsync("/api/embeddings/generate/prompt-templates", null);
        
        // Assert template generation
        templateResponse.Should().BeSuccessful();
        var templateResult = await templateResponse.Content.ReadFromJsonAsync<EmbeddingGenerationResponse>();
        templateResult.Should().NotBeNull();
        templateResult!.Success.Should().BeTrue();
        templateResult.EmbeddingsGenerated.Should().BeGreaterThan(0);

        // Act - Generate query example embeddings
        var exampleResponse = await _client.PostAsync("/api/embeddings/generate/query-examples", null);
        
        // Assert example generation
        exampleResponse.Should().BeSuccessful();
        var exampleResult = await exampleResponse.Content.ReadFromJsonAsync<EmbeddingGenerationResponse>();
        exampleResult.Should().NotBeNull();
        exampleResult!.Success.Should().BeTrue();
        exampleResult.EmbeddingsGenerated.Should().BeGreaterThan(0);

        // Act - Test semantic search
        var searchRequest = new SemanticSearchTestRequest
        {
            Query = "Show me gaming revenue metrics",
            IntentType = "Analytical",
            Domain = "Gaming",
            MaxResults = 5
        };

        var searchResponse = await _client.PostAsJsonAsync("/api/embeddings/test/semantic-search", searchRequest);
        
        // Assert semantic search
        searchResponse.Should().BeSuccessful();
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SemanticSearchTestResponse>();
        searchResult.Should().NotBeNull();
        searchResult!.Success.Should().BeTrue();
        searchResult.RelevantTemplates.Should().NotBeEmpty();
        searchResult.RelevantExamples.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PerformanceTest_MultipleSimultaneousRequests_MeetsResponseTimeTargets()
    {
        // Arrange
        var requests = Enumerable.Range(0, 10).Select(i => new BusinessPromptRequest
        {
            UserQuestion = $"Show me daily metrics for test query {i}",
            UserId = $"test-user-{i}",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = PromptComplexityLevel.Standard
        }).ToList();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Send multiple requests simultaneously
        var tasks = requests.Select(request => 
            _client.PostAsJsonAsync("/api/businesscontext/prompt", request)).ToArray();
        
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode);
        
        var results = await Task.WhenAll(responses.Select(r => 
            r.Content.ReadFromJsonAsync<BusinessAwarePromptResponse>()));
        
        results.Should().OnlyContain(r => r != null);
        results.Should().OnlyContain(r => r!.ProcessingTimeMs < 500); // Individual request < 500ms
        
        var averageResponseTime = stopwatch.ElapsedMilliseconds / (double)requests.Count;
        averageResponseTime.Should().BeLessThan(1000); // Average < 1 second
    }

    [Fact]
    public async Task ErrorHandling_InvalidRequests_ReturnsAppropriateErrors()
    {
        // Test empty user question
        var emptyRequest = new BusinessPromptRequest { UserQuestion = "", UserId = "test" };
        var emptyResponse = await _client.PostAsJsonAsync("/api/businesscontext/prompt", emptyRequest);
        emptyResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // Test null user ID
        var nullUserRequest = new BusinessPromptRequest { UserQuestion = "test", UserId = null };
        var nullUserResponse = await _client.PostAsJsonAsync("/api/businesscontext/prompt", nullUserRequest);
        nullUserResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // Test invalid domain
        var invalidDomainRequest = new BusinessPromptRequest 
        { 
            UserQuestion = "test", 
            UserId = "test",
            PreferredDomain = (BusinessDomainType)999 
        };
        var invalidDomainResponse = await _client.PostAsJsonAsync("/api/businesscontext/prompt", invalidDomainRequest);
        invalidDomainResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Dashboard_EmbeddingsStatus_ReturnsAccurateMetrics()
    {
        // Act
        var response = await _client.GetAsync("/api/embeddings/dashboard");

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<EmbeddingsDashboardResponse>();
        
        result.Should().NotBeNull();
        result!.PromptTemplates.Should().NotBeNull();
        result.QueryExamples.Should().NotBeNull();
        result.RecentEmbeddings.Should().NotBeNull();
        
        // Verify data consistency
        result.PromptTemplates.TotalTemplates.Should().BeGreaterThanOrEqualTo(
            result.PromptTemplates.WithEmbeddings + result.PromptTemplates.WithoutEmbeddings);
        
        result.QueryExamples.TotalExamples.Should().BeGreaterThanOrEqualTo(
            result.QueryExamples.WithEmbeddings + result.QueryExamples.WithoutEmbeddings);
    }
}

#region Request/Response DTOs

public class BusinessPromptRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public BusinessDomainType PreferredDomain { get; set; }
    public PromptComplexityLevel ComplexityLevel { get; set; }
    public bool IncludeExamples { get; set; }
    public bool IncludeBusinessRules { get; set; }
}

public class BusinessAwarePromptResponse
{
    public string GeneratedPrompt { get; set; } = string.Empty;
    public BusinessContextProfile ContextProfile { get; set; } = new();
    public ContextualBusinessSchema UsedSchema { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public int ProcessingTimeMs { get; set; }
}

public class ContextAnalysisRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

public class BusinessContextAnalysisResponse
{
    public BusinessContextProfile ContextProfile { get; set; } = new();
    public int ProcessingTimeMs { get; set; }
}

public class MetadataRetrievalRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public BusinessDomainType Domain { get; set; }
    public int MaxTables { get; set; } = 5;
    public bool IncludeRelationships { get; set; }
}

public class BusinessMetadataResponse
{
    public List<BusinessTableContext> RelevantTables { get; set; } = new();
    public List<TableRelationship> TableRelationships { get; set; } = new();
    public int ProcessingTimeMs { get; set; }
}

public class EntityExtractionRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public bool IncludeConfidenceScores { get; set; }
}

public class EntityExtractionResponse
{
    public List<BusinessEntity> ExtractedEntities { get; set; } = new();
    public int ProcessingTimeMs { get; set; }
}

public class TableSearchRequest
{
    public string SearchQuery { get; set; } = string.Empty;
    public BusinessDomainType Domain { get; set; }
    public int MaxResults { get; set; } = 10;
    public double MinimumRelevanceScore { get; set; } = 0.5;
}

public class TableSearchResponse
{
    public List<BusinessTableContext> RelevantTables { get; set; } = new();
    public int ProcessingTimeMs { get; set; }
}

public class IntentClassificationRequest
{
    public string UserQuestion { get; set; } = string.Empty;
}

public class IntentClassificationResponse
{
    public QueryIntent Intent { get; set; } = new();
    public int ProcessingTimeMs { get; set; }
}

public class EmbeddingGenerationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int EmbeddingsGenerated { get; set; }
}

public class SemanticSearchTestRequest
{
    public string Query { get; set; } = string.Empty;
    public string? IntentType { get; set; }
    public string? Domain { get; set; }
    public int MaxResults { get; set; } = 5;
}

public class SemanticSearchTestResponse
{
    public bool Success { get; set; }
    public List<RelevantPromptTemplate> RelevantTemplates { get; set; } = new();
    public List<RelevantQueryExample> RelevantExamples { get; set; } = new();
}

public class EmbeddingsDashboardResponse
{
    public PromptTemplateStats PromptTemplates { get; set; } = new();
    public QueryExampleStats QueryExamples { get; set; } = new();
    public List<RecentEmbedding> RecentEmbeddings { get; set; } = new();
}

public class PromptTemplateStats
{
    public int TotalTemplates { get; set; }
    public int WithEmbeddings { get; set; }
    public int WithoutEmbeddings { get; set; }
}

public class QueryExampleStats
{
    public int TotalExamples { get; set; }
    public int WithEmbeddings { get; set; }
    public int WithoutEmbeddings { get; set; }
}

public class RecentEmbedding
{
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int VectorDimensions { get; set; }
}

public enum PromptComplexityLevel
{
    Simple,
    Standard,
    Complex,
    Advanced
}

#endregion
