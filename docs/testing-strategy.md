# Comprehensive Testing Strategy for AI-Powered BI Reporting Copilot

## Overview
This document outlines a comprehensive testing strategy that ensures the reliability, security, and performance of the AI-Powered BI Reporting Copilot system across all components.

## Testing Pyramid Structure

```
                    E2E Tests (5%)
                 ┌─────────────────┐
                 │   User Journeys │
                 │   Integration   │
                 └─────────────────┘

            Integration Tests (25%)
         ┌─────────────────────────┐
         │    API Integration      │
         │    Database Tests       │
         │    AI Service Tests     │
         └─────────────────────────┘

        Unit Tests (70%)
    ┌─────────────────────────────┐
    │     Business Logic          │
    │     Service Methods         │
    │     Utility Functions       │
    └─────────────────────────────┘
```

## 1. Unit Testing Strategy

### Backend Unit Tests (.NET)

#### Core Services Testing
```csharp
[TestFixture]
public class QueryProcessingServiceTests
{
    private Mock<IOpenAIService> _mockOpenAI;
    private Mock<ISchemaIntrospector> _mockSchema;
    private Mock<ISqlQueryService> _mockSqlService;
    private QueryProcessingService _service;

    [SetUp]
    public void Setup()
    {
        _mockOpenAI = new Mock<IOpenAIService>();
        _mockSchema = new Mock<ISchemaIntrospector>();
        _mockSqlService = new Mock<ISqlQueryService>();
        _service = new QueryProcessingService(_mockOpenAI.Object, _mockSchema.Object, _mockSqlService.Object);
    }

    [Test]
    public async Task ProcessQuery_ValidInput_ReturnsSuccessfulResult()
    {
        // Arrange
        var query = "Show me total sales last month";
        var expectedSQL = "SELECT SUM(Amount) FROM Sales WHERE Date >= DATEADD(month, -1, GETDATE())";
        var expectedResult = "[{\"TotalSales\": 150000}]";

        _mockSchema.Setup(x => x.GetSchemaSummaryAsync()).ReturnsAsync("Sales(Id, Amount, Date)");
        _mockOpenAI.Setup(x => x.GenerateSQLAsync(It.IsAny<string>())).ReturnsAsync(expectedSQL);
        _mockSqlService.Setup(x => x.ExecuteSelectQueryAsync(expectedSQL)).ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ProcessQueryAsync(query);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.SQL, Is.EqualTo(expectedSQL));
        Assert.That(result.Result, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task ProcessQuery_InvalidSQL_ReturnsError()
    {
        // Arrange
        var query = "Delete all data";
        var maliciousSQL = "DELETE FROM Sales";

        _mockSchema.Setup(x => x.GetSchemaSummaryAsync()).ReturnsAsync("Sales(Id, Amount, Date)");
        _mockOpenAI.Setup(x => x.GenerateSQLAsync(It.IsAny<string>())).ReturnsAsync(maliciousSQL);

        // Act
        var result = await _service.ProcessQueryAsync(query);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Contains.Substring("Only SELECT queries allowed"));
    }

    [Test]
    [TestCase("", "Query cannot be empty")]
    [TestCase(null, "Query cannot be empty")]
    [TestCase("   ", "Query cannot be empty")]
    public async Task ProcessQuery_InvalidInput_ReturnsValidationError(string query, string expectedError)
    {
        // Act
        var result = await _service.ProcessQueryAsync(query);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Contains.Substring(expectedError));
    }
}

[TestFixture]
public class PromptEngineeringServiceTests
{
    [Test]
    public void BuildPrompt_WithSchemaAndQuery_GeneratesCorrectPrompt()
    {
        // Arrange
        var service = new PromptEngineeringService();
        var schema = "Players(Id, Country, RegistrationDate), Transactions(Id, PlayerId, Amount, Date)";
        var query = "Show me players from UK";

        // Act
        var prompt = service.BuildPrompt(schema, query);

        // Assert
        Assert.That(prompt, Contains.Substring("SQL Server expert"));
        Assert.That(prompt, Contains.Substring(schema));
        Assert.That(prompt, Contains.Substring(query));
        Assert.That(prompt, Contains.Substring("SELECT"));
    }
}
```

#### Security Testing
```csharp
[TestFixture]
public class SecurityValidationTests
{
    private SqlSecurityValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new SqlSecurityValidator();
    }

    [Test]
    [TestCase("SELECT * FROM Users", true)]
    [TestCase("SELECT Name FROM Users WHERE Id = 1", true)]
    [TestCase("DELETE FROM Users", false)]
    [TestCase("UPDATE Users SET Name = 'Test'", false)]
    [TestCase("DROP TABLE Users", false)]
    [TestCase("INSERT INTO Users VALUES ('test')", false)]
    [TestCase("SELECT * FROM Users; DELETE FROM Users", false)]
    public void ValidateSQL_VariousQueries_ReturnsExpectedResult(string sql, bool expectedValid)
    {
        // Act
        var result = _validator.ValidateSQL(sql);

        // Assert
        Assert.That(result.IsValid, Is.EqualTo(expectedValid));
    }

    [Test]
    public void ValidateSQL_SQLInjectionAttempt_ReturnsFalse()
    {
        // Arrange
        var maliciousSQL = "SELECT * FROM Users WHERE Id = 1; DROP TABLE Users; --";

        // Act
        var result = _validator.ValidateSQL(maliciousSQL);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Multiple statements detected"));
    }
}
```

### Frontend Unit Tests (React + TypeScript)

#### Component Testing
```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryInterface } from '../QueryInterface';
import { useQueryStore } from '../../stores/queryStore';

// Mock the store
jest.mock('../../stores/queryStore');
const mockUseQueryStore = useQueryStore as jest.MockedFunction<typeof useQueryStore>;

describe('QueryInterface', () => {
  beforeEach(() => {
    mockUseQueryStore.mockReturnValue({
      currentResult: null,
      queryHistory: [],
      executeQuery: jest.fn(),
      addToFavorites: jest.fn(),
      clearCurrentResult: jest.fn(),
    });
  });

  test('renders query input and submit button', () => {
    render(<QueryInterface />);

    expect(screen.getByPlaceholderText(/ask a question about your data/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /ask/i })).toBeInTheDocument();
  });

  test('disables submit button when query is empty', () => {
    render(<QueryInterface />);

    const submitButton = screen.getByRole('button', { name: /ask/i });
    expect(submitButton).toBeDisabled();
  });

  test('enables submit button when query has content', () => {
    render(<QueryInterface />);

    const input = screen.getByPlaceholderText(/ask a question about your data/i);
    const submitButton = screen.getByRole('button', { name: /ask/i });

    fireEvent.change(input, { target: { value: 'Show me sales data' } });

    expect(submitButton).not.toBeDisabled();
  });

  test('calls executeQuery when form is submitted', async () => {
    const mockExecuteQuery = jest.fn();
    mockUseQueryStore.mockReturnValue({
      currentResult: null,
      queryHistory: [],
      executeQuery: mockExecuteQuery,
      addToFavorites: jest.fn(),
      clearCurrentResult: jest.fn(),
    });

    render(<QueryInterface />);

    const input = screen.getByPlaceholderText(/ask a question about your data/i);
    const submitButton = screen.getByRole('button', { name: /ask/i });

    fireEvent.change(input, { target: { value: 'Show me sales data' } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockExecuteQuery).toHaveBeenCalledWith({
        question: 'Show me sales data',
        sessionId: expect.any(String),
        options: expect.objectContaining({
          includeVisualization: true,
          maxRows: 1000,
          enableCache: true,
          confidenceThreshold: 0.7
        })
      });
    });
  });
});
```

#### Store Testing (Zustand)
```typescript
import { renderHook, act } from '@testing-library/react';
import { useQueryStore } from '../queryStore';

describe('QueryStore', () => {
  test('initial state is correct', () => {
    const { result } = renderHook(() => useQueryStore());

    expect(result.current.currentResult).toBeNull();
    expect(result.current.queryHistory).toEqual([]);
    expect(result.current.isLoading).toBe(false);
  });

  test('executeQuery updates loading state', async () => {
    const { result } = renderHook(() => useQueryStore());

    act(() => {
      result.current.executeQuery({
        question: 'test query',
        sessionId: 'test-session',
        options: {}
      });
    });

    expect(result.current.isLoading).toBe(true);
  });
});
```

## 2. Integration Testing

### API Integration Tests
```csharp
[TestFixture]
public class ReportControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real services with test doubles
                    services.AddScoped<IOpenAIService, MockOpenAIService>();
                    services.AddScoped<ISqlQueryService, MockSqlQueryService>();
                });
            });

        _client = _factory.CreateClient();
    }

    [Test]
    public async Task PostQuery_ValidRequest_ReturnsSuccessfulResponse()
    {
        // Arrange
        var request = new QueryRequest
        {
            Question = "Show me total sales",
            SessionId = Guid.NewGuid().ToString()
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/report/query", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<QueryResponse>(responseString);

        Assert.That(result.Success, Is.True);
        Assert.That(result.SQL, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task PostQuery_UnauthorizedRequest_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new QueryRequest { Question = "test" };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/report/query", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
```

### Database Integration Tests
```csharp
[TestFixture]
public class DatabaseIntegrationTests
{
    private IServiceProvider _serviceProvider;
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        _connectionString = "Server=(localdb)\\mssqllocaldb;Database=BICopilotTest;Integrated Security=true;";

        var services = new ServiceCollection();
        services.AddDbContext<BICopilotContext>(options =>
            options.UseSqlServer(_connectionString));
        services.AddScoped<ISchemaIntrospector, SchemaIntrospector>();

        _serviceProvider = services.BuildServiceProvider();

        // Create test database
        using var context = _serviceProvider.GetRequiredService<BICopilotContext>();
        context.Database.EnsureCreated();
        SeedTestData(context);
    }

    [Test]
    public async Task SchemaIntrospector_GetSchemaSummary_ReturnsCorrectSchema()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var introspector = scope.ServiceProvider.GetRequiredService<ISchemaIntrospector>();

        // Act
        var schema = await introspector.GetSchemaSummaryAsync();

        // Assert
        Assert.That(schema, Contains.Substring("TestTable"));
        Assert.That(schema, Contains.Substring("Id"));
        Assert.That(schema, Contains.Substring("Name"));
    }

    private void SeedTestData(BICopilotContext context)
    {
        // Create test tables and data
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE TestTable (
                Id INT PRIMARY KEY,
                Name NVARCHAR(100),
                Amount DECIMAL(10,2),
                CreatedDate DATETIME2
            )");

        context.Database.ExecuteSqlRaw(@"
            INSERT INTO TestTable VALUES
            (1, 'Test1', 100.50, GETDATE()),
            (2, 'Test2', 200.75, GETDATE())");
    }
}
```

## 3. End-to-End Testing

### User Journey Tests
```typescript
import { test, expect } from '@playwright/test';

test.describe('BI Copilot User Journeys', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('[data-testid=username]', 'test@company.com');
    await page.fill('[data-testid=password]', 'password123');
    await page.click('[data-testid=login-button]');
    await expect(page).toHaveURL('/dashboard');
  });

  test('Complete query workflow', async ({ page }) => {
    // Navigate to query interface
    await page.click('[data-testid=query-tab]');

    // Enter a natural language query
    await page.fill('[data-testid=query-input]', 'Show me total sales by country last month');

    // Submit query
    await page.click('[data-testid=submit-query]');

    // Wait for results
    await expect(page.locator('[data-testid=query-results]')).toBeVisible({ timeout: 10000 });

    // Verify SQL was generated
    await expect(page.locator('[data-testid=generated-sql]')).toContainText('SELECT');

    // Verify results table is displayed
    await expect(page.locator('[data-testid=results-table]')).toBeVisible();

    // Verify chart is displayed
    await expect(page.locator('[data-testid=results-chart]')).toBeVisible();

    // Add to favorites
    await page.click('[data-testid=add-to-favorites]');
    await expect(page.locator('[data-testid=favorites-notification]')).toBeVisible();
  });

  test('Query history functionality', async ({ page }) => {
    // Execute a query first
    await page.fill('[data-testid=query-input]', 'Show me user count');
    await page.click('[data-testid=submit-query]');
    await expect(page.locator('[data-testid=query-results]')).toBeVisible();

    // Navigate to history tab
    await page.click('[data-testid=history-tab]');

    // Verify query appears in history
    await expect(page.locator('[data-testid=history-item]')).toContainText('Show me user count');

    // Click on history item to rerun
    await page.click('[data-testid=history-item]:first-child');

    // Verify query is populated in input
    await expect(page.locator('[data-testid=query-input]')).toHaveValue('Show me user count');
  });

  test('Error handling', async ({ page }) => {
    // Enter an invalid query
    await page.fill('[data-testid=query-input]', 'This is not a valid business question');
    await page.click('[data-testid=submit-query]');

    // Verify error message is displayed
    await expect(page.locator('[data-testid=error-message]')).toBeVisible();
    await expect(page.locator('[data-testid=error-message]')).toContainText('could not be processed');
  });
});
```

## 4. Performance Testing

### Load Testing with k6
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 10 }, // Ramp up
    { duration: '5m', target: 10 }, // Stay at 10 users
    { duration: '2m', target: 20 }, // Ramp up to 20 users
    { duration: '5m', target: 20 }, // Stay at 20 users
    { duration: '2m', target: 0 },  // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'], // 95% of requests should be below 2s
    http_req_failed: ['rate<0.1'],     // Error rate should be below 10%
  },
};

export default function () {
  const payload = JSON.stringify({
    question: 'Show me total sales last month',
    sessionId: `session-${__VU}-${__ITER}`,
    options: {
      includeVisualization: true,
      maxRows: 1000,
      enableCache: true
    }
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + getAuthToken(),
    },
  };

  const response = http.post('http://localhost:5000/api/report/query', payload, params);

  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 2000ms': (r) => r.timings.duration < 2000,
    'has SQL in response': (r) => JSON.parse(r.body).sql !== null,
  });

  sleep(1);
}

function getAuthToken() {
  // Implementation to get auth token
  return 'mock-jwt-token';
}
```

## 5. Security Testing

### Penetration Testing Checklist
- [ ] SQL Injection attempts in natural language queries
- [ ] Authentication bypass attempts
- [ ] Authorization escalation tests
- [ ] Input validation testing
- [ ] Rate limiting verification
- [ ] CORS policy testing
- [ ] JWT token manipulation tests
- [ ] Sensitive data exposure checks

### Automated Security Tests
```csharp
[TestFixture]
public class SecurityTests
{
    [Test]
    [TestCase("'; DROP TABLE Users; --")]
    [TestCase("1' OR '1'='1")]
    [TestCase("<script>alert('xss')</script>")]
    public async Task QueryEndpoint_MaliciousInput_RejectsRequest(string maliciousInput)
    {
        // Test that malicious inputs are properly sanitized
        var request = new QueryRequest { Question = maliciousInput };
        var result = await _queryService.ProcessQueryAsync(request);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Contains.Substring("Invalid input"));
    }
}
```

## 6. Test Data Management

### Test Data Strategy
- **Unit Tests**: Use in-memory databases and mocked services
- **Integration Tests**: Use dedicated test databases with known datasets
- **E2E Tests**: Use staging environment with production-like data (anonymized)

### Test Data Cleanup
```csharp
[TearDown]
public async Task Cleanup()
{
    using var context = new BICopilotContext(_testConnectionString);
    await context.Database.ExecuteSqlRawAsync("DELETE FROM QueryHistory WHERE SessionId LIKE 'test-%'");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM QueryCache WHERE QueryHash LIKE 'test-%'");
}
```

## 7. Continuous Testing Pipeline

### CI/CD Integration
```yaml
# Azure DevOps Pipeline
stages:
- stage: Test
  jobs:
  - job: UnitTests
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Run Unit Tests'
      inputs:
        command: 'test'
        projects: '**/*Tests.csproj'
        arguments: '--configuration Release --collect:"XPlat Code Coverage"'

  - job: IntegrationTests
    dependsOn: UnitTests
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Run Integration Tests'
      inputs:
        command: 'test'
        projects: '**/*IntegrationTests.csproj'

  - job: E2ETests
    dependsOn: IntegrationTests
    steps:
    - task: PlaywrightTest@1
      displayName: 'Run E2E Tests'
      inputs:
        testDir: 'tests/e2e'
```

This comprehensive testing strategy ensures high quality, reliability, and security across all components of the AI-Powered BI Reporting Copilot system.

## 8. Test Metrics and Reporting

### Key Testing Metrics
- **Code Coverage**: Minimum 80% for unit tests, 70% for integration tests
- **Test Execution Time**: Unit tests <5 minutes, Integration tests <15 minutes, E2E tests <30 minutes
- **Test Reliability**: >95% pass rate on main branch
- **Defect Escape Rate**: <5% of bugs found in production

### Automated Reporting
```csharp
public class TestMetricsCollector
{
    public async Task<TestMetricsReport> GenerateReportAsync()
    {
        return new TestMetricsReport
        {
            CodeCoverage = await GetCodeCoverageAsync(),
            TestExecutionTimes = await GetExecutionTimesAsync(),
            PassRates = await GetPassRatesAsync(),
            TrendAnalysis = await GetTrendAnalysisAsync()
        };
    }
}
```
