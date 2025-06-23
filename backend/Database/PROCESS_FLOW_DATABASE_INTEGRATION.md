# 🗄️ Process Flow Database Integration Guide

## 📋 Overview

This document provides a comprehensive guide for integrating process flow tracking into the database and ensuring all process flow information is properly saved to the relevant tables.

## 🏗️ Database Schema

### **Tables Created**

#### 1. **ProcessFlowSessions**
Main table for tracking complete process flow sessions.

```sql
CREATE TABLE [dbo].[ProcessFlowSessions] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [SessionId] NVARCHAR(450) NOT NULL UNIQUE,
    [UserId] NVARCHAR(450) NOT NULL,
    [Query] NVARCHAR(MAX) NOT NULL,
    [QueryType] NVARCHAR(100) NOT NULL DEFAULT 'enhanced',
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'running',
    [StartTime] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [EndTime] DATETIME2 NULL,
    [TotalDurationMs] BIGINT NULL,
    [OverallConfidence] DECIMAL(5,4) NULL,
    [GeneratedSQL] NVARCHAR(MAX) NULL,
    [ExecutionResult] NVARCHAR(MAX) NULL,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [Metadata] NVARCHAR(MAX) NULL,
    [ConversationId] NVARCHAR(450) NULL,
    [MessageId] NVARCHAR(450) NULL,
    -- BaseEntity fields
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NULL,
    [UpdatedBy] NVARCHAR(256) NULL
);
```

#### 2. **ProcessFlowSteps**
Individual step tracking with hierarchy support.

```sql
CREATE TABLE [dbo].[ProcessFlowSteps] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [SessionId] NVARCHAR(450) NOT NULL,
    [StepId] NVARCHAR(100) NOT NULL,
    [ParentStepId] NVARCHAR(100) NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [StepOrder] INT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending',
    [StartTime] DATETIME2 NULL,
    [EndTime] DATETIME2 NULL,
    [DurationMs] BIGINT NULL,
    [Confidence] DECIMAL(5,4) NULL,
    [InputData] NVARCHAR(MAX) NULL,
    [OutputData] NVARCHAR(MAX) NULL,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [Metadata] NVARCHAR(MAX) NULL,
    [RetryCount] INT NOT NULL DEFAULT 0,
    -- BaseEntity fields
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NULL,
    [UpdatedBy] NVARCHAR(256) NULL
);
```

#### 3. **ProcessFlowLogs**
Detailed logging for each step.

```sql
CREATE TABLE [dbo].[ProcessFlowLogs] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [SessionId] NVARCHAR(450) NOT NULL,
    [StepId] NVARCHAR(100) NULL,
    [LogLevel] NVARCHAR(20) NOT NULL DEFAULT 'Info',
    [Message] NVARCHAR(MAX) NOT NULL,
    [Details] NVARCHAR(MAX) NULL,
    [Exception] NVARCHAR(MAX) NULL,
    [Source] NVARCHAR(100) NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    -- BaseEntity fields
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NULL,
    [UpdatedBy] NVARCHAR(256) NULL
);
```

#### 4. **ProcessFlowTransparency**
AI transparency and token usage information.

```sql
CREATE TABLE [dbo].[ProcessFlowTransparency] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [SessionId] NVARCHAR(450) NOT NULL UNIQUE,
    [Model] NVARCHAR(100) NULL,
    [Temperature] DECIMAL(3,2) NULL,
    [MaxTokens] INT NULL,
    [PromptTokens] INT NULL,
    [CompletionTokens] INT NULL,
    [TotalTokens] INT NULL,
    [EstimatedCost] DECIMAL(10,6) NULL,
    [Confidence] DECIMAL(5,4) NULL,
    [AIProcessingTimeMs] BIGINT NULL,
    [ApiCallCount] INT NOT NULL DEFAULT 0,
    [PromptDetails] NVARCHAR(MAX) NULL,
    [ResponseAnalysis] NVARCHAR(MAX) NULL,
    [QualityMetrics] NVARCHAR(MAX) NULL,
    [OptimizationSuggestions] NVARCHAR(MAX) NULL,
    -- BaseEntity fields
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NULL,
    [UpdatedBy] NVARCHAR(256) NULL
);
```

### **Views for Analytics**

#### 1. **vw_ProcessFlowSummary**
Complete session overview with aggregated metrics.

#### 2. **vw_ProcessFlowStepPerformance**
Step performance analysis and success rates.

## 🔧 Entity Framework Integration

### **Entity Classes**
- `ProcessFlowSessionEntity` - Main session entity
- `ProcessFlowStepEntity` - Individual step entity with hierarchy
- `ProcessFlowLogEntity` - Log entry entity
- `ProcessFlowTransparencyEntity` - AI transparency entity

### **DbContext Configuration**
All entities are properly configured in `BICopilotContext` with:
- Primary keys and indexes
- Foreign key relationships
- Cascade delete behaviors
- Precision settings for decimal fields

## 🚀 Service Layer

### **IProcessFlowService Interface**
Core service interface providing:
- Session management (start, update, complete)
- Step tracking (add, update, status changes)
- Logging capabilities
- Transparency data management
- Analytics and reporting

### **ProcessFlowService Implementation**
Full implementation with:
- Entity mapping
- Database operations
- Comprehensive logging
- Error handling

### **ProcessFlowTracker Helper**
Convenient helper service for:
- Automatic step tracking
- Extension methods for easy integration
- Confidence scoring
- Input/output data capture

## 📊 Data Flow Integration

### **1. Query Processing Pipeline Integration**

```csharp
// In QueryController.cs
public async Task<IActionResult> ProcessEnhancedQuery([FromBody] EnhancedQueryRequest request)
{
    var tracker = _serviceProvider.GetRequiredService<ProcessFlowTracker>();
    
    // Start process flow tracking
    var sessionId = await tracker.StartSessionAsync(userId, request.Query, "enhanced", conversationId, messageId);
    
    try
    {
        // Track authentication step
        await tracker.TrackStepAsync(ProcessFlowSteps.Authentication, async () => {
            // Validate JWT token
            await ValidateUserAsync(userId);
        });
        
        // Track semantic analysis
        var semanticResult = await tracker.TrackStepWithConfidenceAsync(
            ProcessFlowSteps.SemanticAnalysis, 
            async () => {
                var result = await _semanticAnalyzer.AnalyzeQueryAsync(request.Query);
                return (result, result.Confidence);
            });
        
        // Track AI generation with transparency
        var sqlResult = await tracker.TrackStepAsync(ProcessFlowSteps.AIGeneration, async () => {
            var result = await _aiService.GenerateSQLAsync(prompt);
            
            // Set transparency data
            await tracker.SetTransparencyAsync(
                model: "gpt-4",
                temperature: 0.1m,
                promptTokens: result.PromptTokens,
                completionTokens: result.CompletionTokens,
                cost: result.EstimatedCost,
                confidence: result.Confidence,
                processingTimeMs: result.ProcessingTimeMs
            );
            
            return result;
        });
        
        // Complete session successfully
        await tracker.CompleteSessionAsync(
            ProcessFlowStatus.Completed,
            generatedSQL: sqlResult.SQL,
            executionResult: JsonSerializer.Serialize(executionResult),
            overallConfidence: overallConfidence
        );
        
        return Ok(response);
    }
    catch (Exception ex)
    {
        await tracker.CompleteSessionAsync(ProcessFlowStatus.Error, errorMessage: ex.Message);
        throw;
    }
}
```

### **2. AI Service Integration**

```csharp
// In AIService.cs
public async Task<AIResult> GenerateSQLAsync(string prompt)
{
    var tracker = _serviceProvider.GetRequiredService<ProcessFlowTracker>();
    
    // Track OpenAI API call
    return await tracker.TrackStepAsync(ProcessFlowSteps.OpenAIRequest, async () => {
        var startTime = DateTime.UtcNow;
        
        // Set input data
        await tracker.SetStepInputAsync(ProcessFlowSteps.OpenAIRequest, new {
            Model = "gpt-4",
            Temperature = 0.1,
            MaxTokens = 1000,
            PromptLength = prompt.Length
        });
        
        // Make API call
        var response = await _openAIProvider.GenerateCompletionAsync(prompt);
        
        // Set output data
        await tracker.SetStepOutputAsync(ProcessFlowSteps.OpenAIRequest, new {
            GeneratedSQL = response.Content,
            PromptTokens = response.Usage.PromptTokens,
            CompletionTokens = response.Usage.CompletionTokens,
            TotalTokens = response.Usage.TotalTokens
        });
        
        // Log API call details
        await tracker.LogAsync(ProcessFlowSteps.OpenAIRequest, ProcessFlowLogLevel.Info, 
            $"OpenAI API call completed - Model: gpt-4, Tokens: {response.Usage.TotalTokens}");
        
        return response;
    });
}
```

### **3. Real-time Updates via SignalR**

```csharp
// In ProcessFlowService.cs
public async Task UpdateStepStatusAsync(string sessionId, string stepId, string status, string? errorMessage = null)
{
    // Update database
    await base.UpdateStepStatusAsync(sessionId, stepId, status, errorMessage);
    
    // Send real-time update
    await _hubContext.Clients.Group($"session-{sessionId}")
        .SendAsync("ProcessStepUpdated", new {
            SessionId = sessionId,
            StepId = stepId,
            Status = status,
            Timestamp = DateTime.UtcNow
        });
}
```

## 📈 Analytics and Reporting

### **Performance Metrics**
```sql
-- Get step performance analytics
SELECT * FROM vw_ProcessFlowStepPerformance 
ORDER BY AvgDurationMs DESC;

-- Get session success rates by user
SELECT 
    UserId,
    COUNT(*) as TotalSessions,
    SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) as SuccessfulSessions,
    CAST(SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as SuccessRate
FROM ProcessFlowSessions
GROUP BY UserId;
```

### **Cost Analytics**
```sql
-- Get cost analytics by user and time period
SELECT 
    s.UserId,
    DATE(s.StartTime) as Date,
    COUNT(*) as SessionCount,
    SUM(t.TotalTokens) as TotalTokens,
    SUM(t.EstimatedCost) as TotalCost,
    AVG(t.EstimatedCost) as AvgCostPerSession
FROM ProcessFlowSessions s
JOIN ProcessFlowTransparency t ON s.SessionId = t.SessionId
WHERE s.StartTime >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY s.UserId, DATE(s.StartTime)
ORDER BY Date DESC, TotalCost DESC;
```

## 🔄 Migration and Deployment

### **1. Run Migration Script**
```bash
# Execute the migration script
sqlcmd -S your-server -d BIReportingCopilot_dev -i Migration_ProcessFlow_Tables.sql
```

### **2. Verify Tables**
```sql
-- Check if all tables were created
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE 'ProcessFlow%';

-- Verify sample data
SELECT COUNT(*) FROM ProcessFlowSessions;
SELECT COUNT(*) FROM ProcessFlowSteps;
SELECT COUNT(*) FROM ProcessFlowLogs;
SELECT COUNT(*) FROM ProcessFlowTransparency;
```

### **3. Test Integration**
```csharp
// Test process flow tracking
var processFlowService = serviceProvider.GetRequiredService<IProcessFlowService>();
var tracker = serviceProvider.GetRequiredService<ProcessFlowTracker>();

// Start a test session
var sessionId = await tracker.StartSessionAsync("test-user", "Test query");

// Add some steps
await tracker.StartStepAsync(ProcessFlowSteps.Authentication);
await tracker.CompleteStepAsync(ProcessFlowSteps.Authentication, confidence: 1.0m);

// Complete session
await tracker.CompleteSessionAsync(ProcessFlowStatus.Completed);

// Verify data was saved
var session = await processFlowService.GetSessionAsync(sessionId);
Assert.NotNull(session);
Assert.Equal(ProcessFlowStatus.Completed, session.Status);
```

## 🎯 Benefits

### **Complete Transparency**
- Every step of AI processing is tracked and stored
- Full audit trail for compliance and debugging
- Real-time visibility into processing pipeline

### **Performance Analytics**
- Step-by-step performance metrics
- Bottleneck identification
- Success rate tracking

### **Cost Management**
- Token usage tracking per session
- Cost analytics and budgeting
- Optimization opportunities

### **User Experience**
- Real-time progress updates
- Detailed error information
- Historical process flow viewing

## 🔧 Maintenance

### **Data Retention**
```csharp
// Cleanup old process flow data
await processFlowService.CleanupOldDataAsync(TimeSpan.FromDays(90));
```

### **Performance Monitoring**
```sql
-- Monitor table sizes
SELECT 
    t.NAME AS TableName,
    s.Name AS SchemaName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.NAME LIKE 'ProcessFlow%'
GROUP BY t.Name, s.Name, p.Rows
ORDER BY TotalSpaceKB DESC;
```

This comprehensive integration ensures that all process flow information is properly captured, stored, and made available for analysis and transparency reporting.
