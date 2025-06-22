# ReportAIng API Documentation

## 🌐 API Overview

The ReportAIng API provides comprehensive endpoints for business intelligence operations, natural language query processing, and AI-powered data analysis. All endpoints follow RESTful conventions and return JSON responses.

**Base URL**: `https://api.reportaing.com/api`  
**API Version**: v1  
**Authentication**: Bearer Token (JWT)

## 📋 API Categories

### 1. Business Metadata Management
### 2. Natural Language Query Processing  
### 3. AI & Prompt Management
### 4. Schema Discovery & Management
### 5. Analytics & Reporting

---

## 🏢 Business Metadata Management

### Get All Business Tables

```http
GET /api/business/tables
```

**Description**: Retrieve all business tables with metadata

**Parameters**:
- `searchTerm` (query, optional): Filter tables by name or purpose
- `domain` (query, optional): Filter by business domain
- `page` (query, optional): Page number (default: 1)
- `limit` (query, optional): Items per page (default: 50)

**Response**:
```json
{
  "data": [
    {
      "id": 1,
      "tableName": "tbl_Daily_actions",
      "schemaName": "common",
      "businessPurpose": "Track daily player gaming activities",
      "businessContext": "Core gaming analytics table",
      "primaryUseCase": "Player behavior analysis",
      "domainClassification": "Gaming Analytics",
      "isActive": true,
      "columnCount": 15,
      "createdDate": "2024-01-15T10:30:00Z",
      "updatedDate": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 5,
    "totalItems": 247,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### Get Business Table by ID

```http
GET /api/business/tables/{id}
```

**Description**: Retrieve specific business table with complete metadata including columns

**Parameters**:
- `id` (path, required): Table ID

**Response**:
```json
{
  "id": 1,
  "tableName": "tbl_Daily_actions",
  "schemaName": "common",
  "businessPurpose": "Track daily player gaming activities",
  "businessContext": "Core gaming analytics table",
  "primaryUseCase": "Player behavior analysis",
  "businessRules": "Data retention: 7 years, GDPR compliant",
  "columns": [
    {
      "columnName": "PlayerId",
      "dataType": "bigint",
      "businessMeaning": "Unique player identifier",
      "businessContext": "Primary key for player identification",
      "isKeyColumn": true,
      "isRequired": true,
      "dataExamples": ["123456", "789012", "345678"]
    }
  ],
  "naturalLanguageAliases": ["daily actions", "player activities", "gaming events"],
  "businessProcesses": ["Player Analytics", "Revenue Tracking"],
  "analyticalUseCases": ["Retention Analysis", "Behavior Patterns"],
  "vectorSearchKeywords": ["player", "gaming", "daily", "activity"],
  "importanceScore": 9.5,
  "usageFrequency": 8.7,
  "semanticCoverageScore": 9.2
}
```

### Create Business Table

```http
POST /api/business/tables
```

**Description**: Create new business table metadata

**Request Body**:
```json
{
  "tableName": "tbl_Player_sessions",
  "schemaName": "common",
  "businessPurpose": "Track player gaming sessions",
  "businessContext": "Session-level analytics for player engagement",
  "primaryUseCase": "Session analysis and engagement metrics",
  "businessRules": "Session timeout: 30 minutes of inactivity",
  "columns": [
    {
      "columnName": "SessionId",
      "businessMeaning": "Unique session identifier",
      "businessContext": "Primary key for session tracking",
      "dataExamples": ["sess_123", "sess_456"],
      "isKeyColumn": true
    }
  ]
}
```

### Update Business Table

```http
PUT /api/business/tables/{id}
```

**Description**: Update existing business table metadata

### Delete Business Table

```http
DELETE /api/business/tables/{id}
```

**Description**: Soft delete business table (sets IsActive = false)

---

## 🧠 Natural Language Query Processing

### Process Natural Language Query

```http
POST /api/query/natural-language
```

**Description**: Convert natural language question to SQL and execute

**Request Body**:
```json
{
  "question": "Show me total revenue by country for the last 30 days",
  "userId": "user123",
  "context": {
    "preferredDomain": "Gaming",
    "includeExplanation": true,
    "maxExecutionTime": 30000
  }
}
```

**Response**:
```json
{
  "success": true,
  "generatedSql": "SELECT Country, SUM(Revenue) as TotalRevenue FROM...",
  "executionResult": {
    "data": [
      {"Country": "US", "TotalRevenue": 125000.50},
      {"Country": "UK", "TotalRevenue": 89000.25}
    ],
    "rowCount": 15,
    "executionTimeMs": 1250
  },
  "analysis": {
    "queryIntent": "Aggregation",
    "confidenceScore": 0.92,
    "businessContext": "Revenue analysis by geographic region",
    "tablesUsed": ["tbl_Daily_actions", "tbl_Countries"],
    "complexity": "Medium"
  },
  "explanation": {
    "businessSummary": "This query analyzes revenue trends across different countries",
    "sqlExplanation": "The query joins player actions with country data to aggregate revenue",
    "insights": ["US shows highest revenue", "30-day trend is positive"]
  }
}
```

### Validate SQL Query

```http
POST /api/query/validate
```

**Description**: Validate SQL syntax and semantics

**Request Body**:
```json
{
  "sql": "SELECT * FROM tbl_Daily_actions WHERE PlayerId = 123",
  "includeOptimization": true,
  "includeSemanticValidation": true
}
```

**Response**:
```json
{
  "isValid": true,
  "syntaxErrors": [],
  "semanticIssues": [
    {
      "type": "Performance",
      "message": "Consider adding index on PlayerId for better performance",
      "severity": "Warning"
    }
  ],
  "optimizationSuggestions": [
    "Add WHERE clause to limit result set",
    "Consider using specific columns instead of SELECT *"
  ],
  "estimatedExecutionTime": "< 100ms",
  "estimatedRowCount": 1
}
```

---

## 🤖 AI & Prompt Management

### Generate Business-Aware Prompt

```http
POST /api/prompts/business-aware
```

**Description**: Generate contextually-aware prompt for LLM based on user question and business metadata

**Request Body**:
```json
{
  "userQuestion": "What are the top performing games by revenue?",
  "userId": "user123",
  "preferredDomain": "Gaming",
  "complexityLevel": "Standard",
  "includeExamples": true,
  "includeBusinessRules": true,
  "maxTokens": 4000
}
```

**Response**:
```json
{
  "generatedPrompt": "You are a business intelligence expert...",
  "contextProfile": {
    "originalQuestion": "What are the top performing games by revenue?",
    "intent": "Ranking",
    "domain": "Gaming",
    "entities": ["games", "revenue", "performance"],
    "businessTerms": ["revenue", "top performing", "games"],
    "confidenceScore": 0.94
  },
  "usedSchema": {
    "relevantTables": [
      {
        "tableName": "tbl_Daily_actions",
        "relevanceScore": 0.89,
        "businessPurpose": "Track gaming activities and revenue"
      }
    ],
    "relevantColumns": [
      {
        "columnName": "GameId",
        "businessMeaning": "Unique game identifier"
      },
      {
        "columnName": "Revenue",
        "businessMeaning": "Revenue generated from game activity"
      }
    ]
  },
  "confidenceScore": 0.91,
  "warnings": [],
  "metadata": {
    "processingTimeMs": 245,
    "tokensUsed": 3200,
    "templateUsed": "analytical_ranking"
  }
}
```

### Get Prompt Templates

```http
GET /api/prompts/templates
```

**Description**: Retrieve available prompt templates

**Parameters**:
- `category` (query, optional): Filter by template category
- `isActive` (query, optional): Filter by active status

### Create Prompt Template

```http
POST /api/prompts/templates
```

**Description**: Create new prompt template

### Get LLM Usage Statistics

```http
GET /api/ai/usage-stats
```

**Description**: Retrieve AI usage statistics and costs

**Parameters**:
- `startDate` (query, optional): Start date for statistics
- `endDate` (query, optional): End date for statistics
- `groupBy` (query, optional): Group by day/week/month

---

## 🗄️ Schema Discovery & Management

### Get Database Schema

```http
GET /api/schema
```

**Description**: Retrieve complete database schema metadata

**Response**:
```json
{
  "databaseName": "DailyActionsDB",
  "tables": [
    {
      "name": "tbl_Daily_actions",
      "schema": "common",
      "rowCount": 1500000,
      "columns": [
        {
          "name": "PlayerId",
          "dataType": "bigint",
          "isNullable": false,
          "isPrimaryKey": true,
          "maxLength": null
        }
      ]
    }
  ],
  "relationships": [
    {
      "fromTable": "tbl_Daily_actions",
      "fromColumn": "PlayerId",
      "toTable": "tbl_Players",
      "toColumn": "Id",
      "relationshipType": "ManyToOne"
    }
  ]
}
```

### Get Table Metadata

```http
GET /api/schema/tables/{tableName}
```

**Description**: Get detailed metadata for specific table

### Discover Data Sources

```http
GET /api/schema/datasources
```

**Description**: Get available data sources for schema exploration

---

## 📊 Analytics & Reporting

### Get Query Analytics

```http
GET /api/analytics/queries
```

**Description**: Retrieve query performance and usage analytics

### Get Business Glossary

```http
GET /api/business/glossary
```

**Description**: Retrieve business glossary terms

**Parameters**:
- `category` (query, optional): Filter by term category
- `searchTerm` (query, optional): Search in term definitions

### Search Business Terms

```http
GET /api/business/glossary/search
```

**Description**: Search for business terms and definitions

---

## 🔐 Authentication & Error Handling

### Authentication

All API endpoints require authentication via Bearer token:

```http
Authorization: Bearer <jwt_token>
```

### Error Responses

Standard error response format:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request parameters",
    "details": [
      {
        "field": "tableName",
        "message": "Table name is required"
      }
    ],
    "timestamp": "2024-01-20T15:30:00Z",
    "traceId": "abc123-def456"
  }
}
```

### HTTP Status Codes

- `200 OK` - Successful request
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request parameters
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server error

### Rate Limiting

- **Standard endpoints**: 100 requests per minute
- **AI endpoints**: 20 requests per minute
- **Query execution**: 10 requests per minute

Rate limit headers included in responses:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1642694400
```
