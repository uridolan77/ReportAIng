# ReportAIng Development Guidelines

## 🎯 Overview

This document establishes coding standards, best practices, and development guidelines for the ReportAIng Business Intelligence Co-Pilot system. These guidelines ensure code quality, maintainability, and team collaboration.

## 🏗️ Architecture Principles

### Clean Architecture
- **Dependency Inversion**: High-level modules should not depend on low-level modules
- **Separation of Concerns**: Each layer has a single responsibility
- **Testability**: Code should be easily unit testable
- **Independence**: Business logic independent of frameworks and external concerns

### SOLID Principles
- **S**ingle Responsibility Principle
- **O**pen/Closed Principle  
- **L**iskov Substitution Principle
- **I**nterface Segregation Principle
- **D**ependency Inversion Principle

## 📁 Project Structure

```
BIReportingCopilot/
├── BIReportingCopilot.API/           # Presentation Layer
│   ├── Controllers/                  # API Controllers
│   ├── Middleware/                   # Custom middleware
│   ├── Extensions/                   # Service registration
│   └── Program.cs                    # Application entry point
├── BIReportingCopilot.Core/          # Domain Layer
│   ├── Commands/                     # CQRS Commands
│   ├── Queries/                      # CQRS Queries
│   ├── DTOs/                         # Data Transfer Objects
│   ├── Interfaces/                   # Abstractions
│   ├── Models/                       # Domain Models
│   └── Enums/                        # Enumerations
├── BIReportingCopilot.Infrastructure/ # Infrastructure Layer
│   ├── Data/                         # Data Access
│   ├── AI/                           # AI Services
│   ├── Business/                     # Business Services
│   ├── Schema/                       # Schema Services
│   └── Handlers/                     # CQRS Handlers
└── BIReportingCopilot.Tests/         # Test Projects
    ├── Unit/                         # Unit Tests
    ├── Integration/                  # Integration Tests
    └── Performance/                  # Performance Tests
```

## 🔧 Coding Standards

### C# Naming Conventions

#### Classes and Methods
```csharp
// ✅ Good - PascalCase for classes, methods, properties
public class BusinessTableManagementService
{
    public async Task<BusinessTableInfoDto> GetBusinessTableAsync(long id)
    {
        // Implementation
    }
    
    public string BusinessPurpose { get; set; }
}

// ❌ Bad
public class businessTableManagementService
{
    public async Task<BusinessTableInfoDto> getBusinessTable(long id)
    {
        // Implementation
    }
}
```

#### Variables and Parameters
```csharp
// ✅ Good - camelCase for local variables and parameters
public async Task ProcessQueryAsync(string naturalLanguageQuery, string userId)
{
    var businessContext = await AnalyzeContextAsync(naturalLanguageQuery);
    var relevantTables = await FindRelevantTablesAsync(businessContext);
}

// ❌ Bad
public async Task ProcessQueryAsync(string NaturalLanguageQuery, string user_id)
{
    var BusinessContext = await AnalyzeContextAsync(NaturalLanguageQuery);
    var relevant_tables = await FindRelevantTablesAsync(BusinessContext);
}
```

#### Constants and Enums
```csharp
// ✅ Good - PascalCase for constants and enums
public const string DefaultSchemaName = "common";

public enum QueryIntent
{
    Unknown,
    Aggregation,
    Filtering,
    Comparison,
    Trend
}

// ❌ Bad
public const string DEFAULT_SCHEMA_NAME = "common";

public enum queryIntent
{
    unknown,
    aggregation
}
```

### Method Design

#### Single Responsibility
```csharp
// ✅ Good - Single responsibility
public async Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId)
{
    var entity = MapToEntity(request, userId);
    await ValidateBusinessTableAsync(entity);
    await _repository.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    return MapToDto(entity);
}

// ❌ Bad - Multiple responsibilities
public async Task<BusinessTableInfoDto> CreateBusinessTableAndSendNotificationAndLogActivity(
    CreateTableInfoRequest request, string userId)
{
    // Creates table, sends email, logs activity - too many responsibilities
}
```

#### Async/Await Best Practices
```csharp
// ✅ Good - Proper async/await usage
public async Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync()
{
    var entities = await _repository.GetAllActiveAsync();
    return entities.Select(MapToDto).ToList();
}

// ✅ Good - ConfigureAwait(false) for library code
public async Task<BusinessTableInfoDto> GetBusinessTableAsync(long id)
{
    var entity = await _repository.GetByIdAsync(id).ConfigureAwait(false);
    return entity != null ? MapToDto(entity) : null;
}

// ❌ Bad - Blocking async calls
public BusinessTableInfoDto GetBusinessTable(long id)
{
    var entity = _repository.GetByIdAsync(id).Result; // Don't do this!
    return MapToDto(entity);
}
```

### Error Handling

#### Exception Handling Strategy
```csharp
// ✅ Good - Specific exception handling with logging
public async Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId)
{
    try
    {
        _logger.LogInformation("Creating business table {TableName} by user {UserId}", 
            request.TableName, userId);
            
        var entity = MapToEntity(request, userId);
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Successfully created business table {TableName}", request.TableName);
        return MapToDto(entity);
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning(ex, "Validation failed for table creation: {TableName}", request.TableName);
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating business table {TableName}", request.TableName);
        throw new BusinessException("Failed to create business table", ex);
    }
}

// ❌ Bad - Generic exception handling
public async Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId)
{
    try
    {
        // Implementation
    }
    catch (Exception ex)
    {
        // Log and swallow - loses important error information
        _logger.LogError(ex.Message);
        return null;
    }
}
```

#### Custom Exceptions
```csharp
// ✅ Good - Specific business exceptions
public class BusinessTableNotFoundException : Exception
{
    public BusinessTableNotFoundException(long tableId) 
        : base($"Business table with ID {tableId} was not found")
    {
        TableId = tableId;
    }
    
    public long TableId { get; }
}

public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message, Dictionary<string, string> errors) 
        : base(message)
    {
        Errors = errors;
    }
    
    public Dictionary<string, string> Errors { get; }
}
```

### Dependency Injection

#### Service Registration
```csharp
// ✅ Good - Proper service registration in Program.cs
builder.Services.AddScoped<IBusinessTableManagementService, BusinessTableManagementService>();
builder.Services.AddScoped<IBusinessTableRepository, BusinessTableRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ✅ Good - Interface-based dependencies
public class BusinessTableManagementService : IBusinessTableManagementService
{
    private readonly IBusinessTableRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BusinessTableManagementService> _logger;
    
    public BusinessTableManagementService(
        IBusinessTableRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<BusinessTableManagementService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

## 🧪 Testing Guidelines

### Unit Testing Standards

#### Test Naming Convention
```csharp
// ✅ Good - Descriptive test names
[Fact]
public async Task CreateBusinessTableAsync_WithValidRequest_ReturnsBusinessTableDto()
{
    // Arrange
    var request = new CreateTableInfoRequest
    {
        TableName = "test_table",
        SchemaName = "common",
        BusinessPurpose = "Test purpose"
    };
    
    // Act
    var result = await _service.CreateBusinessTableAsync(request, "testuser");
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("test_table", result.TableName);
}

[Fact]
public async Task CreateBusinessTableAsync_WithDuplicateTableName_ThrowsValidationException()
{
    // Test implementation
}

// ❌ Bad - Unclear test names
[Fact]
public async Task Test1()
{
    // What does this test?
}
```

#### Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task GetBusinessTableAsync_WithExistingId_ReturnsCorrectTable()
{
    // Arrange
    var tableId = 1L;
    var expectedTable = new BusinessTableInfoEntity
    {
        Id = tableId,
        TableName = "test_table",
        SchemaName = "common"
    };
    
    _mockRepository.Setup(r => r.GetByIdAsync(tableId))
                   .ReturnsAsync(expectedTable);
    
    // Act
    var result = await _service.GetBusinessTableAsync(tableId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(tableId, result.Id);
    Assert.Equal("test_table", result.TableName);
    _mockRepository.Verify(r => r.GetByIdAsync(tableId), Times.Once);
}
```

#### Mock Usage
```csharp
// ✅ Good - Proper mock setup and verification
public class BusinessTableManagementServiceTests
{
    private readonly Mock<IBusinessTableRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<BusinessTableManagementService>> _mockLogger;
    private readonly BusinessTableManagementService _service;
    
    public BusinessTableManagementServiceTests()
    {
        _mockRepository = new Mock<IBusinessTableRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<BusinessTableManagementService>>();
        
        _service = new BusinessTableManagementService(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }
}
```

### Integration Testing

#### API Testing
```csharp
[Fact]
public async Task GetBusinessTables_ReturnsSuccessStatusCode()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/business/tables");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    var tables = JsonSerializer.Deserialize<List<BusinessTableInfoDto>>(content);
    Assert.NotNull(tables);
}
```

## 📝 Documentation Standards

### XML Documentation
```csharp
/// <summary>
/// Creates a new business table with the specified metadata
/// </summary>
/// <param name="request">The table creation request containing metadata</param>
/// <param name="userId">The ID of the user creating the table</param>
/// <returns>The created business table DTO</returns>
/// <exception cref="ValidationException">Thrown when request validation fails</exception>
/// <exception cref="BusinessException">Thrown when business rules are violated</exception>
public async Task<BusinessTableInfoDto> CreateBusinessTableAsync(
    CreateTableInfoRequest request, 
    string userId)
{
    // Implementation
}
```

### README Documentation
Each project should include:
- Purpose and overview
- Setup instructions
- Configuration requirements
- Usage examples
- API documentation links

## 🔍 Code Review Guidelines

### Review Checklist

#### Functionality
- [ ] Code meets requirements
- [ ] Edge cases are handled
- [ ] Error handling is appropriate
- [ ] Performance considerations addressed

#### Code Quality
- [ ] Follows naming conventions
- [ ] Single responsibility principle
- [ ] Proper abstraction levels
- [ ] No code duplication

#### Testing
- [ ] Unit tests included
- [ ] Test coverage adequate
- [ ] Integration tests where needed
- [ ] Tests are meaningful

#### Security
- [ ] Input validation present
- [ ] No sensitive data exposure
- [ ] Authentication/authorization correct
- [ ] SQL injection prevention

### Review Process
1. **Self Review**: Author reviews own code before submission
2. **Peer Review**: At least one team member reviews
3. **Automated Checks**: CI/CD pipeline validates
4. **Approval**: Code approved before merge

## 🚀 Performance Guidelines

### Database Access
```csharp
// ✅ Good - Efficient querying
public async Task<List<BusinessTableInfoDto>> GetActiveTablesAsync()
{
    return await _context.BusinessTableInfo
        .Where(t => t.IsActive)
        .Select(t => new BusinessTableInfoDto
        {
            Id = t.Id,
            TableName = t.TableName,
            SchemaName = t.SchemaName
        })
        .AsNoTracking()
        .ToListAsync();
}

// ❌ Bad - Inefficient querying
public async Task<List<BusinessTableInfoDto>> GetActiveTablesAsync()
{
    var allTables = await _context.BusinessTableInfo.ToListAsync();
    return allTables.Where(t => t.IsActive).Select(MapToDto).ToList();
}
```

### Caching Strategy
```csharp
// ✅ Good - Proper caching implementation
public async Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync()
{
    const string cacheKey = "business_tables_all";
    
    if (_cache.TryGetValue(cacheKey, out List<BusinessTableInfoDto> cachedTables))
    {
        return cachedTables;
    }
    
    var tables = await _repository.GetAllActiveAsync();
    var dtos = tables.Select(MapToDto).ToList();
    
    _cache.Set(cacheKey, dtos, TimeSpan.FromMinutes(30));
    return dtos;
}
```

## 🔐 Security Guidelines

### Input Validation
```csharp
// ✅ Good - Proper validation
public async Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId)
{
    if (request == null)
        throw new ArgumentNullException(nameof(request));
        
    if (string.IsNullOrWhiteSpace(request.TableName))
        throw new ValidationException("Table name is required");
        
    if (string.IsNullOrWhiteSpace(userId))
        throw new ArgumentNullException(nameof(userId));
    
    // Sanitize input
    request.TableName = request.TableName.Trim();
    
    // Implementation
}
```

### SQL Injection Prevention
```csharp
// ✅ Good - Parameterized queries with EF Core
public async Task<BusinessTableInfoEntity?> GetTableByNameAsync(string tableName, string schemaName)
{
    return await _context.BusinessTableInfo
        .FirstOrDefaultAsync(t => t.TableName == tableName && t.SchemaName == schemaName);
}

// ❌ Bad - String concatenation (vulnerable to SQL injection)
public async Task<BusinessTableInfoEntity?> GetTableByNameAsync(string tableName, string schemaName)
{
    var sql = $"SELECT * FROM BusinessTableInfo WHERE TableName = '{tableName}'";
    // Don't do this!
}
```

## 📊 Logging Guidelines

### Structured Logging
```csharp
// ✅ Good - Structured logging with Serilog
_logger.LogInformation("Creating business table {TableName} in schema {SchemaName} by user {UserId}", 
    request.TableName, request.SchemaName, userId);

_logger.LogWarning("Business table {TableName} already exists in schema {SchemaName}", 
    request.TableName, request.SchemaName);

_logger.LogError(ex, "Failed to create business table {TableName} for user {UserId}", 
    request.TableName, userId);

// ❌ Bad - String interpolation in logs
_logger.LogInformation($"Creating business table {request.TableName} by user {userId}");
```

### Log Levels
- **Trace**: Very detailed information, typically only of interest when diagnosing problems
- **Debug**: Information useful for debugging, not needed in production
- **Information**: General information about application flow
- **Warning**: Something unexpected happened, but application continues
- **Error**: Error occurred, but application can continue
- **Critical**: Critical error, application may not be able to continue

## 🔄 Git Workflow

### Branch Naming
- `feature/business-metadata-enhancement`
- `bugfix/query-validation-issue`
- `hotfix/security-vulnerability`
- `release/v1.2.0`

### Commit Messages
```
feat: add business context analyzer service

- Implement intent classification for user queries
- Add business domain detection
- Include entity extraction capabilities
- Add comprehensive unit tests

Closes #123
```

### Pull Request Guidelines
- Clear title and description
- Link to related issues
- Include testing instructions
- Request appropriate reviewers
- Ensure CI/CD passes

These guidelines ensure consistent, maintainable, and high-quality code across the ReportAIng project.
