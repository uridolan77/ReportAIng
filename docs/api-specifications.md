# AI-Powered BI Reporting Copilot - API Specifications

## Overview
This document defines the REST API endpoints for the AI-Powered BI Reporting Copilot system.

## Base URL
- Development: `https://localhost:5001/api`
- Production: `https://bi-copilot.company.com/api`

## Authentication
All endpoints require JWT authentication via the `Authorization` header:
```
Authorization: Bearer <jwt_token>
```

## Core Query Endpoints

### POST /api/query/natural-language
Execute a natural language query and return results with generated SQL.

**Request Body:**
```json
{
  "question": "What was the total revenue by country last month?",
  "sessionId": "uuid-session-id",
  "options": {
    "includeVisualization": true,
    "maxRows": 1000,
    "enableCache": true,
    "confidenceThreshold": 0.7
  }
}
```

**Response:**
```json
{
  "queryId": "uuid-query-id",
  "sql": "SELECT Country, SUM(Revenue) as TotalRevenue FROM...",
  "result": {
    "data": [...],
    "metadata": {
      "columnCount": 2,
      "rowCount": 15,
      "executionTimeMs": 245
    }
  },
  "visualization": {
    "type": "bar",
    "config": {...}
  },
  "confidence": 0.85,
  "suggestions": ["Try: 'Show revenue trends over time'"],
  "cached": false
}
```

### GET /api/query/history
Retrieve user's query history with pagination.

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)
- `startDate`: Filter from date (ISO 8601)
- `endDate`: Filter to date (ISO 8601)
- `successful`: Filter by success status (true/false)

**Response:**
```json
{
  "queries": [
    {
      "id": "uuid",
      "question": "Total sales last week",
      "timestamp": "2024-01-15T10:30:00Z",
      "successful": true,
      "executionTime": 150,
      "confidence": 0.9
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 5,
    "totalItems": 95,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### POST /api/query/feedback
Submit feedback for a specific query to improve the system.

**Request Body:**
```json
{
  "queryId": "uuid-query-id",
  "feedback": "positive", // "positive", "negative", "neutral"
  "comments": "The result was exactly what I needed",
  "suggestedImprovement": "Could include percentage changes"
}
```

## Schema and Metadata Endpoints

### GET /api/schema/tables
Get available tables and their metadata.

**Query Parameters:**
- `includeColumns`: Include column details (default: false)
- `search`: Filter tables by name

**Response:**
```json
{
  "tables": [
    {
      "name": "Sales",
      "description": "Sales transaction data",
      "rowCount": 1500000,
      "lastUpdated": "2024-01-15T08:00:00Z",
      "columns": [
        {
          "name": "SaleDate",
          "type": "datetime",
          "description": "Date of the sale",
          "nullable": false,
          "semanticTags": ["date", "transaction"]
        }
      ]
    }
  ]
}
```

### GET /api/schema/suggestions
Get query suggestions based on available data and user history.

**Response:**
```json
{
  "suggestions": [
    {
      "category": "Revenue Analysis",
      "queries": [
        "Show monthly revenue trends",
        "Compare revenue by product category",
        "Top 10 customers by revenue"
      ]
    }
  ],
  "personalizedSuggestions": [
    "Your usual weekly sales report",
    "Customer churn analysis (you asked this last month)"
  ]
}
```

## User Management Endpoints

### GET /api/user/profile
Get current user's profile and preferences.

**Response:**
```json
{
  "userId": "user@company.com",
  "displayName": "John Doe",
  "preferences": {
    "defaultChartType": "bar",
    "maxRowsPerQuery": 1000,
    "enableNotifications": true,
    "timezone": "UTC"
  },
  "permissions": ["read_sales", "read_marketing"],
  "lastLogin": "2024-01-15T09:00:00Z"
}
```

### PUT /api/user/preferences
Update user preferences.

**Request Body:**
```json
{
  "defaultChartType": "line",
  "maxRowsPerQuery": 2000,
  "enableNotifications": false,
  "notificationSettings": {
    "email": true,
    "slack": false
  }
}
```

## Analytics and Monitoring Endpoints

### GET /api/analytics/usage
Get system usage analytics (admin only).

**Query Parameters:**
- `period`: "day", "week", "month" (default: "week")
- `startDate`: Start date for analytics
- `endDate`: End date for analytics

**Response:**
```json
{
  "period": "week",
  "metrics": {
    "totalQueries": 1250,
    "uniqueUsers": 45,
    "averageResponseTime": 850,
    "successRate": 0.92,
    "cacheHitRate": 0.35
  },
  "trends": {
    "queriesPerDay": [180, 220, 195, 240, 185, 160, 70],
    "topUsers": [
      {"userId": "analyst1@company.com", "queryCount": 85},
      {"userId": "manager2@company.com", "queryCount": 62}
    ]
  }
}
```

### GET /api/health
System health check endpoint.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "services": {
    "database": "healthy",
    "openai": "healthy",
    "cache": "healthy"
  },
  "metrics": {
    "uptime": "7d 14h 23m",
    "memoryUsage": "65%",
    "activeConnections": 12
  }
}
```

## Error Responses

All endpoints return consistent error responses:

```json
{
  "error": {
    "code": "INVALID_QUERY",
    "message": "The natural language query could not be processed",
    "details": "Ambiguous table reference: 'sales' could refer to 'Sales' or 'SalesHistory'",
    "timestamp": "2024-01-15T10:30:00Z",
    "traceId": "uuid-trace-id"
  }
}
```

## Rate Limiting

- Standard users: 100 requests per hour
- Premium users: 500 requests per hour
- Admin users: 1000 requests per hour

Rate limit headers are included in all responses:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1642248000
```

## WebSocket Endpoints

### /ws/query-status
Real-time query execution status updates.

**Connection:** `wss://bi-copilot.company.com/ws/query-status?token=<jwt_token>`

**Messages:**
```json
{
  "type": "query_started",
  "queryId": "uuid",
  "timestamp": "2024-01-15T10:30:00Z"
}

{
  "type": "query_progress",
  "queryId": "uuid",
  "progress": 0.6,
  "message": "Executing SQL query..."
}

{
  "type": "query_completed",
  "queryId": "uuid",
  "success": true,
  "executionTime": 1250
}
```
