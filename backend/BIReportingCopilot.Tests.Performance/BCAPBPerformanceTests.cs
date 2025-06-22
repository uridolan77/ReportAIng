using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BIReportingCopilot.Core.Models.BusinessContext;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace BIReportingCopilot.Tests.Performance;

/// <summary>
/// Performance tests for BCAPB system with response time validation and load testing
/// </summary>
public class BCAPBPerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public BCAPBPerformanceTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task BusinessContextAnalysis_ResponseTime_ShouldBeLessThan500Ms()
    {
        // Arrange
        var request = new BusinessPromptRequest
        {
            UserQuestion = "Show me daily active users for mobile games",
            UserId = "perf-test-user",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = PromptComplexityLevel.Standard
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/analyze", request);
        stopwatch.Stop();

        // Assert
        response.Should().BeSuccessful();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
            $"Business context analysis took {stopwatch.ElapsedMilliseconds}ms, should be < 500ms");

        _output.WriteLine($"Business Context Analysis: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MetadataRetrieval_ResponseTime_ShouldBeLessThan200Ms()
    {
        // Arrange
        var request = new MetadataRetrievalRequest
        {
            UserQuestion = "Show me gaming revenue data",
            Domain = BusinessDomainType.Gaming,
            MaxTables = 5
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/metadata", request);
        stopwatch.Stop();

        // Assert
        response.Should().BeSuccessful();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200,
            $"Metadata retrieval took {stopwatch.ElapsedMilliseconds}ms, should be < 200ms");

        _output.WriteLine($"Metadata Retrieval: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task PromptGeneration_ResponseTime_ShouldBeLessThan100Ms()
    {
        // Arrange
        var request = new BusinessPromptRequest
        {
            UserQuestion = "Show me user metrics",
            UserId = "perf-test-user",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = PromptComplexityLevel.Simple
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/prompt", request);
        stopwatch.Stop();

        // Assert
        response.Should().BeSuccessful();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
            $"Prompt generation took {stopwatch.ElapsedMilliseconds}ms, should be < 100ms");

        _output.WriteLine($"Prompt Generation: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task ConcurrentRequests_LoadTest_ShouldHandleMultipleUsers(int concurrentUsers)
    {
        // Arrange
        var requests = Enumerable.Range(0, concurrentUsers).Select(i => new BusinessPromptRequest
        {
            UserQuestion = $"Show me daily metrics for user {i}",
            UserId = $"load-test-user-{i}",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = PromptComplexityLevel.Standard
        }).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = requests.Select(async request =>
        {
            var requestStopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsJsonAsync("/api/businesscontext/prompt", request);
            requestStopwatch.Stop();
            
            return new
            {
                Response = response,
                ElapsedMs = requestStopwatch.ElapsedMilliseconds,
                IsSuccess = response.IsSuccessStatusCode
            };
        }).ToArray();

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var successfulRequests = results.Count(r => r.IsSuccess);
        var failedRequests = results.Length - successfulRequests;
        var averageResponseTime = results.Where(r => r.IsSuccess).Average(r => r.ElapsedMs);
        var maxResponseTime = results.Where(r => r.IsSuccess).Max(r => r.ElapsedMs);
        var totalThroughput = concurrentUsers / (stopwatch.ElapsedMilliseconds / 1000.0);

        // Performance assertions
        successfulRequests.Should().Be(concurrentUsers, "All requests should succeed");
        averageResponseTime.Should().BeLessThan(1000, "Average response time should be < 1 second");
        maxResponseTime.Should().BeLessThan(2000, "Maximum response time should be < 2 seconds");
        totalThroughput.Should().BeGreaterThan(10, "Should handle at least 10 requests per second");

        _output.WriteLine($"Load Test Results for {concurrentUsers} concurrent users:");
        _output.WriteLine($"  Total Time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  Successful Requests: {successfulRequests}/{concurrentUsers}");
        _output.WriteLine($"  Failed Requests: {failedRequests}");
        _output.WriteLine($"  Average Response Time: {averageResponseTime:F2}ms");
        _output.WriteLine($"  Max Response Time: {maxResponseTime}ms");
        _output.WriteLine($"  Throughput: {totalThroughput:F2} requests/second");
    }

    [Fact]
    public async Task SemanticSearch_Performance_ShouldBeFastAndAccurate()
    {
        // Arrange
        var searchQueries = new[]
        {
            "Show me gaming revenue metrics",
            "Daily active users analysis",
            "Player retention and engagement",
            "Financial performance indicators",
            "Operational efficiency metrics"
        };

        var results = new List<(string Query, long ElapsedMs, int ResultCount)>();

        // Act
        foreach (var query in searchQueries)
        {
            var request = new SemanticSearchTestRequest
            {
                Query = query,
                MaxResults = 5
            };

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsJsonAsync("/api/embeddings/test/semantic-search", request);
            stopwatch.Stop();

            response.Should().BeSuccessful();
            var result = await response.Content.ReadFromJsonAsync<SemanticSearchTestResponse>();
            
            results.Add((query, stopwatch.ElapsedMilliseconds, 
                result?.RelevantTemplates?.Count + result?.RelevantExamples?.Count ?? 0));
        }

        // Assert
        var averageResponseTime = results.Average(r => r.ElapsedMs);
        var maxResponseTime = results.Max(r => r.ElapsedMs);
        var averageResultCount = results.Average(r => r.ResultCount);

        averageResponseTime.Should().BeLessThan(300, "Average semantic search should be < 300ms");
        maxResponseTime.Should().BeLessThan(500, "Max semantic search should be < 500ms");
        averageResultCount.Should().BeGreaterThan(3, "Should return relevant results");

        _output.WriteLine("Semantic Search Performance:");
        foreach (var (query, elapsed, count) in results)
        {
            _output.WriteLine($"  '{query}': {elapsed}ms, {count} results");
        }
        _output.WriteLine($"Average: {averageResponseTime:F2}ms, {averageResultCount:F1} results");
    }

    [Fact]
    public async Task MemoryUsage_UnderLoad_ShouldRemainStable()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var requests = Enumerable.Range(0, 50).Select(i => new BusinessPromptRequest
        {
            UserQuestion = $"Memory test query {i} with various complexity levels",
            UserId = $"memory-test-{i}",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = i % 2 == 0 ? PromptComplexityLevel.Simple : PromptComplexityLevel.Complex
        }).ToList();

        // Act
        var tasks = requests.Select(request => 
            _client.PostAsJsonAsync("/api/businesscontext/prompt", request)).ToArray();
        
        var responses = await Task.WhenAll(tasks);
        
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode);
        
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreasePerRequest = memoryIncrease / requests.Count;

        memoryIncreasePerRequest.Should().BeLessThan(1024 * 1024, // 1MB per request
            "Memory usage per request should be reasonable");

        _output.WriteLine($"Memory Usage Test:");
        _output.WriteLine($"  Initial Memory: {initialMemory / 1024 / 1024:F2} MB");
        _output.WriteLine($"  Final Memory: {finalMemory / 1024 / 1024:F2} MB");
        _output.WriteLine($"  Memory Increase: {memoryIncrease / 1024 / 1024:F2} MB");
        _output.WriteLine($"  Per Request: {memoryIncreasePerRequest / 1024:F2} KB");
    }

    [Fact]
    public async Task DatabaseConnections_UnderLoad_ShouldNotExceedLimits()
    {
        // Arrange
        var concurrentRequests = 20;
        var requests = Enumerable.Range(0, concurrentRequests).Select(i => new MetadataRetrievalRequest
        {
            UserQuestion = $"Database connection test {i}",
            Domain = BusinessDomainType.Gaming,
            MaxTables = 3
        }).ToList();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var tasks = requests.Select(request => 
            _client.PostAsJsonAsync("/api/businesscontext/metadata", request)).ToArray();
        
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode, 
            "All database requests should succeed without connection pool exhaustion");

        var averageResponseTime = stopwatch.ElapsedMilliseconds / (double)concurrentRequests;
        averageResponseTime.Should().BeLessThan(500, 
            "Database operations should remain fast under load");

        _output.WriteLine($"Database Connection Test:");
        _output.WriteLine($"  Concurrent Requests: {concurrentRequests}");
        _output.WriteLine($"  Total Time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  Average Response Time: {averageResponseTime:F2}ms");
        _output.WriteLine($"  All requests successful: {responses.All(r => r.IsSuccessStatusCode)}");
    }

    [Theory]
    [InlineData(PromptComplexityLevel.Simple, 50)]
    [InlineData(PromptComplexityLevel.Standard, 200)]
    [InlineData(PromptComplexityLevel.Complex, 500)]
    [InlineData(PromptComplexityLevel.Advanced, 1000)]
    public async Task ComplexityBasedPerformance_ShouldScaleAppropriately(
        PromptComplexityLevel complexity, int expectedMaxMs)
    {
        // Arrange
        var request = new BusinessPromptRequest
        {
            UserQuestion = complexity switch
            {
                PromptComplexityLevel.Simple => "Show users",
                PromptComplexityLevel.Standard => "Show daily active users for mobile games",
                PromptComplexityLevel.Complex => "Compare year-over-year revenue growth by game category and platform",
                PromptComplexityLevel.Advanced => "Analyze seasonal trends in user engagement with statistical significance testing and predictive modeling for revenue optimization",
                _ => "Default query"
            },
            UserId = "complexity-test-user",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = complexity,
            IncludeExamples = complexity >= PromptComplexityLevel.Standard,
            IncludeBusinessRules = complexity >= PromptComplexityLevel.Complex
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/prompt", request);
        stopwatch.Stop();

        // Assert
        response.Should().BeSuccessful();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(expectedMaxMs,
            $"{complexity} complexity should complete within {expectedMaxMs}ms, took {stopwatch.ElapsedMilliseconds}ms");

        var result = await response.Content.ReadFromJsonAsync<BusinessAwarePromptResponse>();
        result.Should().NotBeNull();
        result!.GeneratedPrompt.Should().NotBeNullOrEmpty();

        _output.WriteLine($"{complexity} Complexity: {stopwatch.ElapsedMilliseconds}ms (limit: {expectedMaxMs}ms)");
    }
}

#region Benchmark Tests (for detailed performance analysis)

[MemoryDiagnoser]
[SimpleJob]
public class BCAPBBenchmarks
{
    private HttpClient _client = null!;
    private WebApplicationFactory<Program> _factory = null!;

    [GlobalSetup]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Benchmark]
    public async Task BusinessContextAnalysis()
    {
        var request = new BusinessPromptRequest
        {
            UserQuestion = "Show me daily active users",
            UserId = "benchmark-user",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = PromptComplexityLevel.Standard
        };

        var response = await _client.PostAsJsonAsync("/api/businesscontext/analyze", request);
        _ = await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task MetadataRetrieval()
    {
        var request = new MetadataRetrievalRequest
        {
            UserQuestion = "Show me gaming data",
            Domain = BusinessDomainType.Gaming,
            MaxTables = 5
        };

        var response = await _client.PostAsJsonAsync("/api/businesscontext/metadata", request);
        _ = await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task PromptGeneration()
    {
        var request = new BusinessPromptRequest
        {
            UserQuestion = "Show me metrics",
            UserId = "benchmark-user",
            PreferredDomain = BusinessDomainType.Gaming,
            ComplexityLevel = PromptComplexityLevel.Simple
        };

        var response = await _client.PostAsJsonAsync("/api/businesscontext/prompt", request);
        _ = await response.Content.ReadAsStringAsync();
    }
}

#endregion

#region Helper Classes (reusing from integration tests)

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

public class MetadataRetrievalRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public BusinessDomainType Domain { get; set; }
    public int MaxTables { get; set; } = 5;
    public bool IncludeRelationships { get; set; }
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

public enum PromptComplexityLevel
{
    Simple,
    Standard,
    Complex,
    Advanced
}

#endregion
