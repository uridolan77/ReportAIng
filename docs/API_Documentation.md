# BI Reporting Copilot API Documentation

## Overview

The BI Reporting Copilot API provides endpoints for natural language query processing, user management, dashboard analytics, and system administration. This documentation covers all available endpoints, request/response formats, and authentication requirements.

## Base URL

```
https://api.bireportingcopilot.com/api
```

## Authentication

All API endpoints require JWT Bearer token authentication unless otherwise specified.

```http
Authorization: Bearer <your-jwt-token>
```

### Authentication Endpoints

#### POST /auth/login
Authenticate user and receive access token.

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "user": {
    "id": "user-id",
    "username": "username",
    "email": "user@example.com",
    "displayName": "User Name",
    "roles": ["User", "Analyst"]
  }
}
```

#### POST /auth/refresh
Refresh access token using refresh token.

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

#### POST /auth/logout
Logout user and revoke tokens.

---

## Dashboard Endpoints

### GET /dashboard/overview
Get comprehensive dashboard overview including user activity, recent queries, system metrics, and quick stats.

**Response:**
```json
{
  "userActivity": {
    "totalQueries": 42,
    "queriesThisWeek": 12,
    "queriesThisMonth": 35,
    "averageQueryTime": 1250.5,
    "lastActivity": "2024-01-15T10:30:00Z",
    "dailyActivities": [
      {
        "date": "2024-01-15",
        "queryCount": 5,
        "averageResponseTime": 1100.2
      }
    ]
  },
  "recentQueries": [
    {
      "id": "query-id",
      "question": "Show me sales data for last month",
      "sql": "SELECT * FROM Sales WHERE date >= '2023-12-01'",
      "timestamp": "2024-01-15T10:30:00Z",
      "successful": true,
      "executionTimeMs": 150,
      "confidence": 0.95,
      "userId": "user-id",
      "sessionId": "session-id"
    }
  ],
  "systemMetrics": {
    "databaseConnections": 5,
    "cacheHitRate": 85.5,
    "averageQueryTime": 1250,
    "systemUptime": "24:15:30"
  },
  "quickStats": {
    "totalQueries": 42,
    "queriesThisWeek": 12,
    "averageQueryTime": 1100,
    "favoriteTable": "sales_data"
  }
}
```

### GET /dashboard/activity
Get user activity summary for specified time period.

**Query Parameters:**
- `days` (optional): Number of days to look back (default: 30)

**Response:**
```json
{
  "totalQueries": 42,
  "queriesThisWeek": 12,
  "queriesThisMonth": 35,
  "averageQueryTime": 1250.5,
  "lastActivity": "2024-01-15T10:30:00Z",
  "dailyActivities": [
    {
      "date": "2024-01-15",
      "queryCount": 5,
      "averageResponseTime": 1100.2
    }
  ],
  "queryTypeBreakdown": {
    "SELECT": 30,
    "AGGREGATE": 8,
    "JOIN": 4
  }
}
```

### GET /dashboard/recent-queries
Get recent queries for the current user.

**Query Parameters:**
- `limit` (optional): Maximum number of queries to return (default: 10, max: 50)

**Response:**
```json
[
  {
    "id": "query-id",
    "question": "Show me sales data for last month",
    "sql": "SELECT * FROM Sales WHERE date >= '2023-12-01'",
    "timestamp": "2024-01-15T10:30:00Z",
    "successful": true,
    "executionTimeMs": 150,
    "confidence": 0.95,
    "error": null,
    "userId": "user-id",
    "sessionId": "session-id"
  }
]
```

### GET /dashboard/system-metrics
Get real-time system performance metrics.

**Response:**
```json
{
  "databaseConnections": 5,
  "cacheHitRate": 85.5,
  "averageQueryTime": 1250,
  "systemUptime": "24:15:30",
  "memoryUsage": {
    "used": 2048,
    "total": 8192,
    "percentage": 25.0
  },
  "queryThroughput": {
    "queriesPerMinute": 12.5,
    "peakQueriesPerMinute": 45.2
  }
}
```

### GET /dashboard/quick-stats
Get quick statistics for the current user.

**Response:**
```json
{
  "totalQueries": 42,
  "queriesThisWeek": 12,
  "averageQueryTime": 1100,
  "favoriteTable": "sales_data",
  "successRate": 92.5,
  "mostUsedQueryType": "SELECT"
}
```

---

## Query Endpoints

### POST /query/execute
Execute a natural language query.

**Request Body:**
```json
{
  "question": "Show me total sales by region for last quarter",
  "connectionName": "primary",
  "maxRows": 1000,
  "sessionId": "session-id"
}
```

**Response:**
```json
{
  "queryId": "query-id",
  "sql": "SELECT region, SUM(amount) as total_sales FROM sales WHERE date >= '2023-10-01' GROUP BY region",
  "result": {
    "data": [
      ["North", 150000],
      ["South", 120000],
      ["East", 180000],
      ["West", 95000]
    ],
    "metadata": {
      "columnCount": 2,
      "rowCount": 4,
      "executionTimeMs": 150,
      "columns": [
        {
          "name": "region",
          "dataType": "varchar",
          "isNullable": false
        },
        {
          "name": "total_sales",
          "dataType": "decimal",
          "isNullable": false
        }
      ]
    },
    "isSuccessful": true
  },
  "visualization": {
    "type": "bar",
    "config": {
      "xAxis": "region",
      "yAxis": "total_sales",
      "title": "Total Sales by Region"
    }
  },
  "confidence": 0.95,
  "suggestions": [
    "Show sales trend over time",
    "Compare with previous quarter"
  ],
  "cached": false,
  "success": true,
  "timestamp": "2024-01-15T10:30:00Z",
  "executionTimeMs": 150
}
```

### GET /query/history
Get query history for the current user.

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)
- `startDate` (optional): Filter from date (ISO 8601)
- `endDate` (optional): Filter to date (ISO 8601)
- `searchTerm` (optional): Search in questions and SQL

**Response:**
```json
{
  "items": [
    {
      "id": "query-id",
      "question": "Show me sales data",
      "sql": "SELECT * FROM Sales",
      "timestamp": "2024-01-15T10:30:00Z",
      "successful": true,
      "executionTimeMs": 150,
      "confidence": 0.95,
      "error": null,
      "userId": "user-id",
      "sessionId": "session-id"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

### GET /query/suggestions
Get query suggestions based on schema and user history.

**Query Parameters:**
- `partial` (optional): Partial query text for autocomplete

**Response:**
```json
{
  "suggestions": [
    "Show me total sales by month",
    "Get customer count by region",
    "List top 10 products by revenue"
  ],
  "schemaSuggestions": [
    {
      "table": "sales",
      "description": "Sales transaction data",
      "sampleQueries": [
        "Show me sales for last month",
        "Get total sales by product"
      ]
    }
  ]
}
```

---

## User Management Endpoints

### GET /user/profile
Get current user profile.

**Response:**
```json
{
  "id": "user-id",
  "username": "username",
  "email": "user@example.com",
  "displayName": "User Name",
  "roles": ["User", "Analyst"],
  "lastLoginDate": "2024-01-15T09:00:00Z",
  "isActive": true,
  "createdDate": "2024-01-01T00:00:00Z",
  "preferences": {
    "theme": "dark",
    "language": "en",
    "timeZone": "UTC"
  }
}
```

### PUT /user/profile
Update user profile.

**Request Body:**
```json
{
  "displayName": "Updated Name",
  "email": "newemail@example.com"
}
```

### POST /user/login
Update last login timestamp.

**Response:**
```json
{
  "message": "Last login updated successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### GET /user/sessions
Get active user sessions.

**Response:**
```json
[
  {
    "sessionId": "session-id",
    "userId": "user-id",
    "startTime": "2024-01-15T09:00:00Z",
    "lastActivity": "2024-01-15T10:30:00Z",
    "ipAddress": "192.168.1.100",
    "userAgent": "Mozilla/5.0...",
    "isActive": true
  }
]
```

### GET /user/preferences
Get user preferences.

**Response:**
```json
{
  "userId": "user-id",
  "theme": "dark",
  "language": "en",
  "timeZone": "UTC",
  "notificationSettings": {
    "email": true,
    "push": false,
    "queryCompletion": true
  },
  "dashboardLayout": {
    "widgets": ["recent-queries", "quick-stats", "system-metrics"]
  }
}
```

### PUT /user/preferences
Update user preferences.

**Request Body:**
```json
{
  "theme": "light",
  "language": "es",
  "timeZone": "EST",
  "notificationSettings": {
    "email": false,
    "push": true
  }
}
```

---

## Schema Endpoints

### GET /schema/tables
Get available database tables and their metadata.

**Response:**
```json
[
  {
    "name": "sales",
    "schema": "dbo",
    "description": "Sales transaction data",
    "rowCount": 150000,
    "columns": [
      {
        "name": "id",
        "dataType": "int",
        "isNullable": false,
        "isPrimaryKey": true,
        "description": "Unique identifier"
      },
      {
        "name": "amount",
        "dataType": "decimal",
        "isNullable": false,
        "description": "Sale amount"
      }
    ],
    "lastUpdated": "2024-01-15T10:00:00Z"
  }
]
```

### GET /schema/tables/{tableName}
Get detailed information about a specific table.

**Response:**
```json
{
  "name": "sales",
  "schema": "dbo",
  "description": "Sales transaction data",
  "rowCount": 150000,
  "columns": [
    {
      "name": "id",
      "dataType": "int",
      "isNullable": false,
      "isPrimaryKey": true,
      "isForeignKey": false,
      "description": "Unique identifier",
      "semanticTags": ["identifier"],
      "sampleValues": ["1", "2", "3"]
    }
  ],
  "indexes": [
    {
      "name": "PK_sales",
      "type": "PRIMARY KEY",
      "columns": ["id"]
    }
  ],
  "relationships": [
    {
      "type": "FOREIGN KEY",
      "column": "customer_id",
      "referencedTable": "customers",
      "referencedColumn": "id"
    }
  ],
  "lastUpdated": "2024-01-15T10:00:00Z"
}
```

### POST /schema/refresh
Trigger schema refresh from database.

**Response:**
```json
{
  "message": "Schema refresh initiated",
  "jobId": "refresh-job-id",
  "estimatedCompletionTime": "2024-01-15T10:35:00Z"
}
```

---

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{
  "error": "Invalid request parameters",
  "details": {
    "field": "question",
    "message": "Question cannot be empty"
  }
}
```

### 401 Unauthorized
```json
{
  "error": "Authentication required",
  "message": "Please provide a valid access token"
}
```

### 403 Forbidden
```json
{
  "error": "Insufficient permissions",
  "message": "User does not have permission to access this resource"
}
```

### 404 Not Found
```json
{
  "error": "Resource not found",
  "message": "The requested resource could not be found"
}
```

### 429 Too Many Requests
```json
{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60
}
```

### 500 Internal Server Error
```json
{
  "error": "Internal server error",
  "message": "An unexpected error occurred",
  "correlationId": "correlation-id-here"
}
```

---

## Rate Limiting

API endpoints are rate limited to ensure fair usage:

- **Authentication endpoints**: 5 requests per minute per IP
- **Query execution**: 30 requests per minute per user
- **Dashboard endpoints**: 60 requests per minute per user
- **Other endpoints**: 100 requests per minute per user

Rate limit headers are included in responses:
```
X-RateLimit-Limit: 30
X-RateLimit-Remaining: 25
X-RateLimit-Reset: 1642248000
```

---

## Webhooks

### Query Completion Webhook
Notifies when a long-running query completes.

**Payload:**
```json
{
  "event": "query.completed",
  "queryId": "query-id",
  "userId": "user-id",
  "success": true,
  "executionTimeMs": 5000,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Schema Update Webhook
Notifies when database schema is updated.

**Payload:**
```json
{
  "event": "schema.updated",
  "changes": [
    {
      "type": "TABLE_ADDED",
      "objectName": "new_table",
      "timestamp": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

## SDK Examples

### JavaScript/TypeScript
```typescript
import { BIReportingCopilotClient } from '@bireportingcopilot/sdk';

const client = new BIReportingCopilotClient({
  baseUrl: 'https://api.bireportingcopilot.com',
  apiKey: 'your-api-key'
});

// Execute a query
const result = await client.query.execute({
  question: 'Show me sales by region',
  maxRows: 100
});

// Get dashboard overview
const dashboard = await client.dashboard.getOverview();
```

### Python
```python
from bireportingcopilot import Client

client = Client(
    base_url='https://api.bireportingcopilot.com',
    api_key='your-api-key'
)

# Execute a query
result = client.query.execute(
    question='Show me sales by region',
    max_rows=100
)

# Get dashboard overview
dashboard = client.dashboard.get_overview()
```

---

## Changelog

### Version 2.0.0 (Current)
- Enhanced dashboard endpoints with real-time metrics
- Improved query history with advanced filtering
- Added user session management
- Implemented comprehensive error handling
- Added rate limiting and security enhancements

### Version 1.0.0
- Initial API release
- Basic query execution
- User authentication
- Schema introspection
