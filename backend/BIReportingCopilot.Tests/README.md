# BI Reporting Copilot Tests

This directory contains comprehensive unit and integration tests for the BI Reporting Copilot application.

## Quick Start

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

## Test Structure

### Unit Tests (`/Unit`)

#### Controllers
- **DashboardControllerTests**: Dashboard endpoints, metrics calculation, error handling
- **UserControllerTests**: User management, session tracking, preferences
- **QueryControllerTests**: Query execution, history, suggestions
- **AuthControllerTests**: Authentication, token management
- **SchemaControllerTests**: Schema introspection, metadata

#### Services
- **QueryServiceTests**: Query processing, history retrieval, cache management
- **UserServiceTests**: User operations, activity tracking, preferences
- **AuthenticationServiceTests**: JWT handling, token validation
- **SchemaServiceTests**: Database schema operations
- **AuditServiceTests**: Audit logging, compliance tracking

#### Repositories
- **TokenRepositoryTests**: Token storage, expiration, cleanup
- **UserRepositoryTests**: User CRUD operations, permissions, roles
- **QueryRepositoryTests**: Query persistence, retrieval

#### Background Jobs
- **CleanupJobTests**: Session cleanup, token expiration, audit log retention
- **SchemaRefreshJobTests**: Schema synchronization, cache invalidation

### Integration Tests (`/Integration`)

- **DashboardIntegrationTests**: End-to-end dashboard API testing
- **AuthenticationIntegrationTests**: Complete auth flow testing
- **QueryIntegrationTests**: Full query execution pipeline
- **UserIntegrationTests**: User management workflows

## Key Features Tested

### Enhanced Dashboard Functionality
- Real-time system metrics calculation
- User activity aggregation and analysis
- Query history retrieval with pagination
- Quick stats generation from audit logs
- Error handling and fallback mechanisms

### Improved Repository Operations
- Database-backed token management
- User credential validation
- Permission and role management
- Session tracking and cleanup

### Background Job Processing
- Automated cleanup of expired sessions
- Token lifecycle management
- Audit log retention policies
- Schema change detection and cache invalidation

### API Integration
- Complete HTTP request/response testing
- Authentication and authorization flows
- Error response validation
- Rate limiting verification

## Test Data Management

### Test Builders
```csharp
// Create test users
var user = TestDataBuilder.CreateTestUser("test-user-id");

// Create audit logs
var auditLogs = TestDataBuilder.CreateTestAuditLogs("user-id", count: 10);

// Create query history
var queries = TestDataBuilder.CreateTestQueries("user-id", count: 5);
```

### Mock Services
```csharp
// Setup mock services
var mockUserService = MockServiceBuilder.CreateMockUserService();
var mockAuditService = MockServiceBuilder.CreateMockAuditService();
```

## Running Specific Tests

### By Category
```bash
# Unit tests only
dotnet test --filter Category=Unit

# Integration tests only
dotnet test --filter Category=Integration

# Controller tests only
dotnet test --filter FullyQualifiedName~Controllers
```

### By Component
```bash
# Dashboard tests
dotnet test --filter FullyQualifiedName~Dashboard

# User management tests
dotnet test --filter FullyQualifiedName~User

# Query processing tests
dotnet test --filter FullyQualifiedName~Query
```

### Individual Test Classes
```bash
# Specific test class
dotnet test --filter ClassName=DashboardControllerTests

# Specific test method
dotnet test --filter FullyQualifiedName=BIReportingCopilot.Tests.Unit.Controllers.DashboardControllerTests.GetDashboardOverview_WithValidUser_ReturnsOverview
```

## Test Configuration

### Database Setup
Tests use in-memory databases for isolation:
```csharp
var options = new DbContextOptionsBuilder<BICopilotContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

### Mock Authentication
Integration tests use mock JWT tokens:
```csharp
_client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", "mock-jwt-token");
```

## Coverage Reports

Generate detailed coverage reports:

```bash
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open report
start coveragereport/index.html
```

## Debugging Tests

### Visual Studio
1. Set breakpoints in test methods
2. Right-click test → Debug Test(s)
3. Inspect variables and call stack

### VS Code
1. Set breakpoints in test files
2. Open Command Palette (Ctrl+Shift+P)
3. Run ".NET: Debug Test"

### Console Output
```csharp
[Test]
public async Task Debug_Test()
{
    // Add debug output
    TestContext.WriteLine($"Testing with user: {userId}");
    
    var result = await _service.GetData();
    
    TestContext.WriteLine($"Result count: {result.Count}");
    
    result.Should().NotBeNull();
}
```

## Common Test Patterns

### Controller Testing
```csharp
[Test]
public async Task ControllerMethod_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    SetupMockServices();
    SetupUserContext("test-user-id");

    // Act
    var result = await _controller.Method(input);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var data = GetResponseValue<ExpectedType>(result);
    data.Should().NotBeNull();
    
    // Verify service calls
    _mockService.Verify(x => x.ExpectedMethod(It.IsAny<string>()), Times.Once);
}
```

### Service Testing
```csharp
[Test]
public async Task ServiceMethod_WithValidData_ProcessesCorrectly()
{
    // Arrange
    var input = CreateTestInput();
    var expectedOutput = CreateExpectedOutput();
    
    _mockRepository.Setup(x => x.GetData(It.IsAny<string>()))
        .ReturnsAsync(expectedOutput);

    // Act
    var result = await _service.ProcessData(input);

    // Assert
    result.Should().BeEquivalentTo(expectedOutput);
    _mockRepository.Verify(x => x.SaveData(It.IsAny<Data>()), Times.Once);
}
```

### Repository Testing
```csharp
[Test]
public async Task Repository_WithValidEntity_SavesSuccessfully()
{
    // Arrange
    var entity = CreateTestEntity();

    // Act
    var result = await _repository.CreateAsync(entity);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().NotBeNullOrEmpty();
    
    var saved = await _context.Entities.FindAsync(result.Id);
    saved.Should().NotBeNull();
    saved.Should().BeEquivalentTo(entity, options => options.Excluding(x => x.Id));
}
```

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Ensure LocalDB is installed and running
   - Check connection strings in test configuration

2. **Mock Setup Issues**
   - Verify mock setups match actual method signatures
   - Use `It.IsAny<T>()` for flexible parameter matching

3. **Async/Await Issues**
   - Always await async methods in tests
   - Don't mix async and sync code

4. **Test Isolation Problems**
   - Use fresh database instances for each test
   - Reset mocks between tests

### Getting Help

1. Check the [Testing Documentation](../../docs/Testing_Documentation.md)
2. Review existing test patterns in similar test classes
3. Use debugger to step through test execution
4. Check test output for detailed error messages

## Contributing

When adding new tests:

1. Follow existing naming conventions
2. Use appropriate test categories
3. Include both positive and negative test cases
4. Add integration tests for new API endpoints
5. Maintain test coverage above 80%
6. Update this README if adding new test categories

## Test Dependencies

Key testing packages used:

- **NUnit**: Test framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database
- **coverlet.collector**: Code coverage collection
