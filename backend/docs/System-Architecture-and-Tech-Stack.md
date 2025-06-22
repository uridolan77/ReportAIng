# ReportAIng System Architecture & Tech Stack

## 🏗️ System Overview

ReportAIng is a sophisticated Business Intelligence Co-Pilot system that transforms natural language queries into SQL, leveraging advanced AI capabilities and rich business metadata for contextually-aware data analysis.

## 🔧 Core Technology Stack

### Backend Framework
- **.NET 8** - Latest LTS version for high performance and modern C# features
- **ASP.NET Core Web API** - RESTful API services with OpenAPI/Swagger documentation
- **Entity Framework Core** - ORM with Code-First migrations and advanced querying
- **SQL Server** - Primary database with advanced analytics capabilities

### AI & Machine Learning
- **OpenAI GPT Models** - Primary LLM for natural language processing and SQL generation
- **Azure OpenAI Service** - Enterprise-grade AI with compliance and security
- **Custom Vector Search** - In-memory semantic similarity matching
- **Semantic Analysis** - Advanced NLU for intent classification and entity extraction

### Architecture Patterns
- **Clean Architecture** - Domain-driven design with clear separation of concerns
- **CQRS (Command Query Responsibility Segregation)** - Separate read/write operations
- **MediatR Pattern** - Decoupled request/response handling
- **Repository Pattern** - Data access abstraction
- **Service Layer Pattern** - Business logic encapsulation

### Cross-Cutting Concerns
- **Serilog** - Structured logging with multiple sinks
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation with fluent syntax
- **Memory Caching** - Performance optimization for frequently accessed data
- **Background Services** - Async processing for long-running operations

## 🏛️ System Architecture

### Layered Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Controllers   │  │   SignalR Hubs  │  │  Middleware  │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Commands      │  │     Queries     │  │   Handlers   │ │
│  │   (CQRS)        │  │    (CQRS)       │  │  (MediatR)   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                             │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │    Entities     │  │  Domain Models  │  │  Interfaces  │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │  Data Access    │  │   AI Services   │  │  External    │ │
│  │  (EF Core)      │  │   (OpenAI)      │  │  Services    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Core Domain Models

#### Business Intelligence Domain
- **BusinessTableInfo** - Rich metadata for database tables
- **BusinessColumnInfo** - Detailed column semantics and business context
- **BusinessGlossary** - Business term definitions and relationships
- **QueryPattern** - Common query templates and patterns

#### AI & Analytics Domain
- **PromptTemplate** - LLM prompt templates for different scenarios
- **LLMModelConfig** - AI model configurations and parameters
- **LLMUsageLog** - Usage tracking and cost optimization
- **SemanticAnalysis** - Query understanding and intent classification

#### Query Processing Domain
- **QueryRequest** - Natural language query processing
- **QueryResult** - SQL execution results with metadata
- **ValidationResult** - SQL validation and semantic checking
- **QueryOptimization** - Performance tuning recommendations

## 🔄 Key Design Patterns

### 1. CQRS with MediatR

```csharp
// Command Example
public class CreateBusinessTableCommand : IRequest<BusinessTableInfoDto>
{
    public CreateTableInfoRequest Request { get; set; }
    public string UserId { get; set; }
}

// Query Example
public class GetBusinessTablesQuery : IRequest<List<BusinessTableInfoDto>>
{
    public string? SearchTerm { get; set; }
    public int PageSize { get; set; } = 50;
}

// Handler Example
public class CreateBusinessTableHandler : IRequestHandler<CreateBusinessTableCommand, BusinessTableInfoDto>
{
    public async Task<BusinessTableInfoDto> Handle(CreateBusinessTableCommand request, CancellationToken cancellationToken)
    {
        // Business logic implementation
    }
}
```

### 2. Repository Pattern with Unit of Work

```csharp
public interface IBusinessTableRepository : IRepository<BusinessTableInfoEntity>
{
    Task<List<BusinessTableInfoEntity>> GetByDomainAsync(string domain);
    Task<BusinessTableInfoEntity?> GetWithColumnsAsync(long id);
}

public interface IUnitOfWork : IDisposable
{
    IBusinessTableRepository BusinessTables { get; }
    Task<int> SaveChangesAsync();
}
```

### 3. Service Layer Pattern

```csharp
public interface IBusinessTableManagementService
{
    Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync();
    Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId);
    Task<bool> DeleteBusinessTableAsync(long id);
}
```

### 4. Strategy Pattern for AI Services

```csharp
public interface IAIProvider
{
    Task<string> GenerateCompletionAsync(string prompt, AIModelConfig config);
    Task<double[]> GenerateEmbeddingAsync(string text);
}

public class OpenAIProvider : IAIProvider { }
public class AzureOpenAIProvider : IAIProvider { }
```

## 🧠 AI & Intelligence Architecture

### Natural Language Understanding Pipeline

```
User Query → Intent Classification → Entity Extraction → Business Context Analysis → Schema Contextualization → Prompt Generation → SQL Generation → Validation → Execution
```

### Key AI Components

#### 1. Semantic Layer Service
- **Business Context Analysis** - Understanding business intent from natural language
- **Schema Contextualization** - Mapping queries to relevant business metadata
- **Vector Search** - Semantic similarity matching for tables and columns

#### 2. Prompt Engineering Service
- **Template Management** - Dynamic prompt templates for different query types
- **Context Injection** - Business metadata integration into prompts
- **Optimization** - Adaptive prompt improvement based on success rates

#### 3. Query Intelligence Service
- **SQL Validation** - Syntax and semantic validation with auto-correction
- **Performance Analysis** - Query optimization recommendations
- **Pattern Recognition** - Learning from successful query patterns

## 📊 Data Architecture

### Primary Database (BIReportingCopilot_Dev)
- **Business Metadata Tables** - BusinessTableInfo, BusinessColumnInfo, BusinessGlossary
- **AI Configuration** - LLMModelConfigs, LLMProviderConfigs, PromptTemplates
- **Usage Analytics** - LLMUsageLogs, QueryLogs, UserInteractions
- **System Configuration** - Settings, UserProfiles, Permissions

### External Data Sources
- **DailyActionsDB** (Remote) - Gaming analytics data on 185.64.56.157
- **Additional BI Sources** - Configurable external database connections

### Caching Strategy
- **Memory Cache** - Frequently accessed business metadata
- **Distributed Cache** - Shared cache for multi-instance deployments
- **Query Result Cache** - Cached results for expensive analytical queries

## 🔐 Security & Compliance

### Authentication & Authorization
- **JWT Bearer Tokens** - Stateless authentication
- **Role-Based Access Control** - Granular permissions for different user types
- **API Key Management** - Secure external service integration

### Data Protection
- **Sensitive Data Masking** - PII protection in query results
- **Audit Logging** - Comprehensive activity tracking
- **GDPR Compliance** - Data retention and deletion policies

## 🚀 Performance & Scalability

### Optimization Strategies
- **Async/Await Pattern** - Non-blocking operations throughout
- **Connection Pooling** - Efficient database connection management
- **Query Optimization** - EF Core query tuning and monitoring
- **Background Processing** - Long-running tasks handled asynchronously

### Monitoring & Observability
- **Structured Logging** - Serilog with correlation IDs
- **Performance Metrics** - Response times, throughput, error rates
- **Health Checks** - System component monitoring
- **Cost Tracking** - LLM usage and cost optimization

## 🔧 Development Practices

### Code Quality
- **Clean Code Principles** - SOLID principles and clean architecture
- **Unit Testing** - Comprehensive test coverage with xUnit
- **Integration Testing** - End-to-end API testing
- **Code Reviews** - Peer review process for quality assurance

### DevOps & Deployment
- **Git Flow** - Feature branch workflow with pull requests
- **CI/CD Pipeline** - Automated build, test, and deployment
- **Environment Management** - Development, staging, production environments
- **Database Migrations** - Code-first EF Core migrations

## 📈 Scalability Considerations

### Horizontal Scaling
- **Stateless Design** - No server-side session state
- **Load Balancing** - Multiple API instances behind load balancer
- **Database Scaling** - Read replicas for query-heavy workloads

### Vertical Scaling
- **Resource Optimization** - Memory and CPU usage monitoring
- **Connection Limits** - Database connection pool tuning
- **Cache Optimization** - Intelligent caching strategies

## 🔮 Future Architecture Considerations

### Microservices Evolution
- **Service Decomposition** - Breaking monolith into focused services
- **Event-Driven Architecture** - Async communication between services
- **API Gateway** - Centralized routing and cross-cutting concerns

### Advanced AI Integration
- **Multi-Model Support** - Integration with various LLM providers
- **Custom Model Training** - Domain-specific model fine-tuning
- **Real-Time Analytics** - Streaming data processing capabilities

This architecture provides a solid foundation for a sophisticated BI Co-Pilot system while maintaining flexibility for future enhancements and scaling requirements.

## 🎯 Technology Decision Rationale

### Why .NET 8?
- **Performance**: Superior performance compared to previous versions
- **Long-Term Support**: 3-year LTS with enterprise stability
- **Modern C# Features**: Latest language features for developer productivity
- **Cross-Platform**: Runs on Windows, Linux, and macOS

### Why Entity Framework Core?
- **Code-First Approach**: Database schema managed through code
- **Advanced Querying**: LINQ support with complex query capabilities
- **Migration Management**: Automated database schema evolution
- **Performance**: Optimized query generation and change tracking

### Why CQRS + MediatR?
- **Separation of Concerns**: Clear distinction between read and write operations
- **Scalability**: Independent scaling of query and command operations
- **Testability**: Easy to unit test individual handlers
- **Maintainability**: Reduced coupling between components

### Why OpenAI Integration?
- **State-of-the-Art Models**: Access to latest GPT models
- **Enterprise Features**: Azure OpenAI provides compliance and security
- **Flexible API**: Support for various AI tasks (completion, embedding, etc.)
- **Cost Management**: Built-in usage tracking and optimization

## 🔍 Detailed Component Breakdown

### Controllers Layer
- **BusinessController**: Business metadata management
- **SchemaController**: Database schema operations
- **QueryController**: Natural language query processing
- **AIController**: AI model management and configuration

### Service Layer
- **BusinessTableManagementService**: Core business metadata operations
- **PromptService**: AI prompt generation and management
- **SemanticLayerService**: Business context and semantic analysis
- **QueryProcessingService**: End-to-end query processing pipeline

### Infrastructure Layer
- **Data Access**: EF Core repositories and unit of work
- **AI Services**: OpenAI integration and model management
- **Caching**: Memory and distributed caching implementations
- **External Services**: Database connections and third-party integrations

## 📋 Configuration Management

### Application Settings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BIReportingCopilot_Dev;",
    "DailyActionsDB": "Server=185.64.56.157;Database=DailyActionsDB;"
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "DefaultModel": "gpt-4",
    "MaxTokens": 4000
  },
  "BusinessMetadata": {
    "CacheExpirationMinutes": 30,
    "MaxTablesPerQuery": 10,
    "SemanticThreshold": 0.7
  }
}
```

### Environment-Specific Configuration
- **Development**: Local SQL Server, debug logging
- **Staging**: Azure SQL Database, structured logging
- **Production**: High-availability setup, comprehensive monitoring

## 🧪 Testing Strategy

### Unit Tests
- **Service Layer**: Business logic validation
- **Repository Layer**: Data access testing with in-memory database
- **AI Components**: Mock AI services for predictable testing

### Integration Tests
- **API Endpoints**: Full request/response cycle testing
- **Database Operations**: Real database integration testing
- **External Services**: AI service integration testing

### Performance Tests
- **Load Testing**: API performance under various loads
- **Database Performance**: Query execution time monitoring
- **AI Service Latency**: Response time optimization

This comprehensive architecture ensures robust, scalable, and maintainable business intelligence capabilities.
