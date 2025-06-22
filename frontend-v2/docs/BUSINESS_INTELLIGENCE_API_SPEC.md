# Business Intelligence API Endpoints Specification

## Overview
This document specifies all API endpoints required for the Business Intelligence system implementation. These endpoints replace the current demo functionality with production-ready AI-powered business intelligence features.

## Base URL
```
/api/business-intelligence
```

## Authentication
All endpoints require authentication via JWT token in the Authorization header:
```
Authorization: Bearer <jwt_token>
```

## Core Query Analysis Endpoints

### 1. Query Analysis
**Endpoint:** `POST /api/business-intelligence/query/analyze`

**Purpose:** Analyze natural language queries and extract business context, entities, and intent.

**Request Body:**
```json
{
  "query": "Show me quarterly sales by region for the last year",
  "userId": "user-123",
  "context": {
    "userRole": "analyst",
    "department": "sales", 
    "accessLevel": "standard",
    "timezone": "America/New_York"
  },
  "options": {
    "includeEntityDetails": true,
    "includeAlternatives": true,
    "includeSuggestions": true
  }
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "queryId": "query-456",
    "analysisTimestamp": "2024-01-15T10:30:00Z",
    "processingTimeMs": 150,
    "businessContext": {
      "confidence": 0.89,
      "intent": {
        "type": "aggregation",
        "confidence": 0.92,
        "complexity": "moderate",
        "description": "User wants to aggregate sales data by region over a quarterly time period",
        "businessGoal": "Analyze regional sales performance to identify growth opportunities",
        "subIntents": ["time_analysis", "geographic_comparison"],
        "reasoning": [
          "Query contains aggregation keywords: 'quarterly', 'by region'",
          "Time dimension identified: 'last year'",
          "Geographic dimension identified: 'region'"
        ]
      },
      "entities": [
        {
          "id": "ent-001",
          "name": "sales",
          "type": "metric",
          "startPosition": 17,
          "endPosition": 22,
          "confidence": 0.95,
          "businessMeaning": "Revenue generated from product sales",
          "context": "Primary business metric for performance analysis",
          "suggestedColumns": ["amount", "revenue", "total_sales"],
          "relatedTables": ["sales_fact", "revenue_summary"],
          "relationships": [
            {
              "relatedEntity": "revenue",
              "relationshipType": "synonym",
              "strength": 0.9
            }
          ]
        }
      ],
      "domain": {
        "name": "Sales Analytics",
        "description": "Analysis of sales performance, trends, and geographic distribution",
        "relevance": 0.94,
        "concepts": ["Revenue", "Territory Management", "Performance Analysis"],
        "relationships": ["Sales connects to Customer", "Region connects to Territory"]
      },
      "timeContext": {
        "period": "last year",
        "granularity": "quarterly", 
        "relativeTo": "current date",
        "startDate": "2023-01-01",
        "endDate": "2023-12-31"
      }
    },
    "suggestedQueries": [
      {
        "query": "Show me quarterly sales by region for the last year compared to previous year",
        "confidence": 0.82,
        "improvement": "Adds comparison baseline for better insights"
      }
    ],
    "estimatedExecutionTime": 2.3,
    "dataQualityScore": 0.85
  }
}
```

### 2. Query Understanding Flow
**Endpoint:** `POST /api/business-intelligence/query/understand`

**Purpose:** Get step-by-step query processing flow for transparency and debugging.

**Request Body:**
```json
{
  "query": "Show me quarterly sales by region for the last year",
  "includeSteps": true,
  "includeAlternatives": true,
  "includeConfidenceBreakdown": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "queryId": "query-789",
    "steps": [
      {
        "step": 1,
        "title": "Query Parsing",
        "description": "Breaking down the natural language query into components",
        "status": "completed",
        "confidence": 0.95,
        "processingTimeMs": 45,
        "details": {
          "tokens": ["Show", "me", "quarterly", "sales", "by", "region"],
          "syntaxTree": {...},
          "identifiedPatterns": ["aggregation_pattern", "time_pattern"]
        }
      },
      {
        "step": 2,
        "title": "Entity Recognition",
        "description": "Identifying business entities and metrics",
        "status": "completed", 
        "confidence": 0.88,
        "processingTimeMs": 67,
        "details": {
          "entitiesFound": 3,
          "highConfidenceEntities": 2,
          "ambiguousEntities": 1
        }
      }
    ],
    "alternatives": [
      {
        "id": "alt-001",
        "type": "trend_analysis", 
        "confidence": 0.75,
        "description": "Analyze sales trends over time",
        "reasoning": "Time dimension suggests trend analysis"
      }
    ],
    "confidenceBreakdown": {
      "overallConfidence": 0.89,
      "factors": [
        {
          "name": "Entity Recognition",
          "score": 0.92,
          "impact": "high",
          "explanation": "Clear business entities identified"
        }
      ]
    }
  }
}
```

### 3. Query Suggestions
**Endpoint:** `GET /api/business-intelligence/query/suggestions`

**Purpose:** Get query suggestions based on user context and history.

**Query Parameters:**
- `userId` (required): User ID
- `context` (optional): Current context (department, role)
- `limit` (optional): Number of suggestions (default: 10)
- `category` (optional): Suggestion category (popular, recent, recommended)

**Response:**
```json
{
  "success": true,
  "data": {
    "suggestions": [
      {
        "id": "sugg-001",
        "query": "Show me top 10 customers by revenue this month",
        "category": "popular",
        "confidence": 0.87,
        "description": "Popular query for sales analysis",
        "estimatedComplexity": "simple",
        "tags": ["sales", "customers", "revenue"]
      }
    ],
    "categories": ["popular", "recent", "recommended", "similar"],
    "totalSuggestions": 25
  }
}
```

### 4. Query History
**Endpoint:** `GET /api/business-intelligence/query/history`

**Purpose:** Get user's query history with analysis results.

**Query Parameters:**
- `userId` (required): User ID
- `limit` (optional): Number of queries (default: 20)
- `startDate` (optional): Filter from date
- `endDate` (optional): Filter to date

**Response:**
```json
{
  "success": true,
  "data": {
    "queries": [
      {
        "queryId": "query-123",
        "query": "Show me quarterly sales by region",
        "timestamp": "2024-01-15T10:30:00Z",
        "confidence": 0.89,
        "executionTime": 2.3,
        "intent": "aggregation",
        "entities": ["sales", "region", "quarterly"],
        "success": true
      }
    ],
    "pagination": {
      "total": 156,
      "page": 1,
      "pageSize": 20,
      "totalPages": 8
    }
  }
}
```

## Business Context Endpoints

### 5. Business Context Profile
**Endpoint:** `GET /api/business-intelligence/context/business-profile`

**Purpose:** Get comprehensive business context for a query.

**Query Parameters:**
- `query` (required): The natural language query
- `userId` (required): User ID for context
- `includeUserContext` (optional): Include user-specific context

**Response:**
```json
{
  "success": true,
  "data": {
    "profile": {
      "confidence": 0.89,
      "intent": {...},
      "entities": [...],
      "domain": {...},
      "businessTerms": ["Sales", "Revenue", "Region"],
      "timeContext": {...},
      "userContext": {
        "role": "analyst",
        "department": "sales",
        "accessLevel": "standard",
        "preferences": {
          "defaultTimeRange": "last_quarter",
          "preferredVisualization": "chart"
        }
      }
    },
    "metadata": {
      "processingTime": 150,
      "modelVersion": "v2.1",
      "lastUpdated": "2024-01-15T08:00:00Z"
    }
  }
}
```

### 6. Entity Detection
**Endpoint:** `GET /api/business-intelligence/context/entities`

**Purpose:** Get detected entities from a query with detailed information.

**Query Parameters:**
- `query` (required): The natural language query
- `includeRelationships` (optional): Include entity relationships
- `includeTableMappings` (optional): Include database table mappings

**Response:**
```json
{
  "success": true,
  "data": {
    "entities": [
      {
        "id": "ent-001",
        "name": "sales",
        "type": "metric",
        "startPosition": 17,
        "endPosition": 22,
        "confidence": 0.95,
        "businessMeaning": "Revenue generated from product sales",
        "context": "Primary business metric for performance analysis",
        "suggestedColumns": ["amount", "revenue", "total_sales"],
        "relatedTables": ["sales_fact", "revenue_summary"],
        "relationships": [...],
        "synonyms": ["revenue", "income"],
        "dataType": "numeric",
        "aggregationMethods": ["sum", "avg", "count"]
      }
    ],
    "totalEntities": 3,
    "averageConfidence": 0.92,
    "processingTimeMs": 89
  }
}
```

### 7. Intent Classification
**Endpoint:** `GET /api/business-intelligence/context/intent`

**Purpose:** Get detailed intent classification with alternatives.

**Query Parameters:**
- `query` (required): The natural language query
- `includeAlternatives` (optional): Include alternative interpretations
- `includeConfidenceBreakdown` (optional): Include confidence analysis

**Response:**
```json
{
  "success": true,
  "data": {
    "primaryIntent": {
      "type": "aggregation",
      "confidence": 0.92,
      "complexity": "moderate",
      "description": "User wants to aggregate sales data by region over a quarterly time period",
      "businessGoal": "Analyze regional sales performance to identify growth opportunities",
      "subIntents": ["time_analysis", "geographic_comparison"],
      "reasoning": [
        "Query contains aggregation keywords: 'quarterly', 'by region'",
        "Time dimension identified: 'last year'",
        "Geographic dimension identified: 'region'"
      ],
      "recommendedActions": [
        "Use GROUP BY for regional aggregation",
        "Apply quarterly date filtering",
        "Consider adding comparison metrics"
      ]
    },
    "alternatives": [
      {
        "id": "alt-001",
        "type": "trend_analysis",
        "confidence": 0.75,
        "description": "Analyze sales trends over time",
        "reasoning": "Time dimension suggests trend analysis",
        "tradeoffs": ["More complex analysis", "Requires time series data"]
      }
    ],
    "confidenceBreakdown": {
      "overallConfidence": 0.92,
      "factors": [
        {
          "name": "Keyword Analysis",
          "score": 0.95,
          "impact": "high",
          "explanation": "Clear aggregation keywords identified"
        },
        {
          "name": "Context Relevance",
          "score": 0.88,
          "impact": "medium",
          "explanation": "Query aligns with user's department context"
        }
      ]
    }
  }
}
```

## Schema Intelligence Endpoints

### 8. Contextual Schema Help
**Endpoint:** `GET /api/business-intelligence/schema/contextual-help`

**Purpose:** Get contextual schema assistance for query construction.

**Query Parameters:**
- `query` (required): The natural language query
- `tables` (optional): Specific tables to focus on
- `includeOptimization` (optional): Include optimization suggestions

**Response:**
```json
{
  "success": true,
  "data": {
    "relevantTables": [
      {
        "name": "sales_fact",
        "relevanceScore": 0.95,
        "description": "Main sales transaction table with detailed sales records",
        "businessPurpose": "Core sales data for analytics and reporting",
        "columns": [
          {
            "name": "sale_id",
            "type": "string",
            "description": "Unique sale identifier",
            "businessMeaning": "Primary key for sales transactions",
            "nullable": false,
            "isPrimaryKey": true,
            "isForeignKey": false,
            "relevanceScore": 0.8
          },
          {
            "name": "amount",
            "type": "number",
            "description": "Sale amount in USD",
            "businessMeaning": "Revenue value of the transaction",
            "nullable": false,
            "isPrimaryKey": false,
            "isForeignKey": false,
            "relevanceScore": 0.98
          }
        ],
        "relationships": [
          {
            "relatedTable": "region_dim",
            "relationshipType": "many-to-one",
            "strength": 0.9,
            "description": "Sales belong to regions",
            "joinCondition": "sales_fact.region_id = region_dim.region_id"
          }
        ],
        "estimatedRowCount": 2500000,
        "lastUpdated": "2024-01-15T08:00:00Z",
        "dataQuality": {
          "completeness": 0.98,
          "accuracy": 0.95,
          "consistency": 0.92
        }
      }
    ],
    "suggestedJoins": [
      {
        "tables": ["sales_fact", "region_dim"],
        "joinType": "INNER JOIN",
        "condition": "sales_fact.region_id = region_dim.region_id",
        "confidence": 0.95,
        "reasoning": "Required for regional analysis"
      }
    ],
    "optimizationTips": [
      {
        "type": "indexing",
        "suggestion": "Consider adding index on sale_date for time-based queries",
        "impact": "high",
        "estimatedImprovement": "60% faster execution"
      }
    ],
    "potentialIssues": [
      {
        "type": "data_quality",
        "description": "Some regions may have incomplete data for Q4 2023",
        "severity": "medium",
        "recommendation": "Filter out incomplete records or use data imputation"
      }
    ]
  }
}
```

### 9. Table Relationships
**Endpoint:** `GET /api/business-intelligence/schema/relationships`

**Purpose:** Get table relationships relevant to the query.

**Query Parameters:**
- `tables` (required): Comma-separated list of table names
- `includeIndirect` (optional): Include indirect relationships
- `maxDepth` (optional): Maximum relationship depth (default: 3)

**Response:**
```json
{
  "success": true,
  "data": {
    "relationships": [
      {
        "fromTable": "sales_fact",
        "toTable": "region_dim",
        "relationshipType": "many-to-one",
        "strength": 0.9,
        "joinCondition": "sales_fact.region_id = region_dim.region_id",
        "description": "Sales transactions are associated with geographic regions",
        "cardinality": {
          "from": "many",
          "to": "one"
        },
        "isRequired": true
      }
    ],
    "relationshipGraph": {
      "nodes": [...],
      "edges": [...],
      "clusters": [...]
    },
    "suggestedQueryPath": [
      "sales_fact",
      "region_dim",
      "time_dim"
    ]
  }
}
```

## Business Terms & Glossary Endpoints

### 10. Business Terms Glossary
**Endpoint:** `GET /api/business-intelligence/terms/glossary`

**Purpose:** Get business terms glossary with definitions and relationships.

**Query Parameters:**
- `category` (optional): Filter by category
- `search` (optional): Search term
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20)

**Response:**
```json
{
  "success": true,
  "data": {
    "terms": [
      {
        "id": "term-001",
        "name": "Sales",
        "definition": "Revenue generated from product or service transactions",
        "category": "Financial",
        "aliases": ["Revenue", "Income"],
        "relatedTerms": [
          {
            "termId": "term-002",
            "name": "Revenue",
            "relationshipType": "synonym",
            "strength": 0.95
          }
        ],
        "businessContext": "Used in financial reporting and performance analysis",
        "dataLineage": [
          {
            "table": "sales_fact",
            "column": "amount",
            "transformation": "SUM(amount)"
          }
        ],
        "lastUpdated": "2024-01-10T10:00:00Z",
        "approvedBy": "business-analyst-123",
        "version": "1.2",
        "status": "approved"
      }
    ],
    "categories": [
      {
        "name": "Financial",
        "description": "Financial and accounting terms",
        "termCount": 15
      }
    ],
    "pagination": {
      "total": 156,
      "page": 1,
      "pageSize": 20,
      "totalPages": 8
    }
  }
}
```

### 11. Term Search
**Endpoint:** `GET /api/business-intelligence/terms/search`

**Purpose:** Search business terms with advanced filtering.

**Query Parameters:**
- `q` (required): Search query
- `category` (optional): Filter by category
- `includeDefinitions` (optional): Include full definitions
- `fuzzy` (optional): Enable fuzzy search (default: true)

**Response:**
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "id": "term-001",
        "name": "Sales",
        "definition": "Revenue generated from product or service transactions",
        "category": "Financial",
        "relevanceScore": 0.95,
        "matchType": "exact",
        "highlightedText": "<mark>Sales</mark>"
      }
    ],
    "totalResults": 12,
    "searchTime": 45,
    "suggestions": ["Revenue", "Income", "Profit"]
  }
}
```

### 12. Term Relationships
**Endpoint:** `GET /api/business-intelligence/terms/relationships`

**Purpose:** Get relationships between business terms.

**Query Parameters:**
- `termId` (required): Primary term ID
- `relationshipTypes` (optional): Filter by relationship types
- `maxDepth` (optional): Maximum relationship depth

**Response:**
```json
{
  "success": true,
  "data": {
    "primaryTerm": {
      "id": "term-001",
      "name": "Sales",
      "definition": "Revenue generated from product or service transactions"
    },
    "relationships": [
      {
        "termId": "term-002",
        "name": "Revenue",
        "relationshipType": "synonym",
        "strength": 0.95,
        "bidirectional": true,
        "description": "Sales and Revenue are used interchangeably"
      }
    ],
    "relationshipGraph": {
      "nodes": [...],
      "edges": [...],
      "clusters": [...]
    }
  }
}
```

## Enhanced Schema Intelligence Endpoints

### 13. Schema Analysis
**Endpoint:** `POST /api/business-intelligence/schema/analyze`

**Purpose:** Comprehensive schema analysis for query optimization and table recommendations.

**Request Body:**
```json
{
  "query": "Show me quarterly sales by region for the last year",
  "includeOptimizations": true,
  "includeDataQuality": true,
  "includeRelationships": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "relevantTables": [
      {
        "name": "sales_fact",
        "relevanceScore": 0.95,
        "description": "Main sales transaction table with detailed sales records",
        "businessPurpose": "Core sales data for analytics and reporting",
        "columns": [...],
        "relationships": [...],
        "estimatedRowCount": 2500000,
        "lastUpdated": "2024-01-15T08:00:00Z",
        "dataQuality": {
          "completeness": 0.98,
          "accuracy": 0.95,
          "consistency": 0.92
        }
      }
    ],
    "suggestedJoins": [
      {
        "tables": ["sales_fact", "region_dim"],
        "joinType": "INNER JOIN",
        "condition": "sales_fact.region_id = region_dim.region_id",
        "confidence": 0.95,
        "reasoning": "Required for regional analysis - high confidence foreign key relationship"
      }
    ],
    "optimizationTips": [
      {
        "type": "indexing",
        "suggestion": "Consider adding composite index on (region_id, sale_date) for time-based regional queries",
        "impact": "high",
        "estimatedImprovement": "60% faster execution for regional time series queries"
      }
    ],
    "potentialIssues": [
      {
        "type": "data_quality",
        "description": "Some regions may have incomplete data for Q4 2023",
        "severity": "medium",
        "recommendation": "Filter out incomplete records or use data imputation"
      }
    ]
  }
}
```

### 14. Join Recommendations
**Endpoint:** `GET /api/business-intelligence/schema/join-recommendations`

**Purpose:** Get intelligent join recommendations based on query context.

**Query Parameters:**
- `tables` (required): Comma-separated list of table names
- `query` (optional): Natural language query for context
- `includeAlternatives` (optional): Include alternative join strategies

**Response:**
```json
{
  "success": true,
  "data": {
    "recommendations": [
      {
        "id": "join-001",
        "tables": ["sales_fact", "region_dim"],
        "joinType": "INNER JOIN",
        "condition": "sales_fact.region_id = region_dim.region_id",
        "confidence": 0.95,
        "reasoning": "Required for regional analysis - high confidence foreign key relationship",
        "performance": {
          "estimatedRows": 2500000,
          "estimatedCost": "medium",
          "indexRecommendations": ["region_id"]
        }
      }
    ],
    "alternatives": [
      {
        "id": "join-alt-001",
        "joinType": "LEFT JOIN",
        "reasoning": "Use if you want to include sales without region data",
        "tradeoffs": ["May include null regions", "Slightly better performance"]
      }
    ]
  }
}
```

### 15. Schema Optimization
**Endpoint:** `GET /api/business-intelligence/schema/optimization`

**Purpose:** Get schema optimization suggestions for better query performance.

**Query Parameters:**
- `tables` (optional): Focus on specific tables
- `queryPatterns` (optional): Common query patterns to optimize for
- `includeIndexing` (optional): Include indexing recommendations

**Response:**
```json
{
  "success": true,
  "data": {
    "optimizations": [
      {
        "type": "indexing",
        "table": "sales_fact",
        "suggestion": "CREATE INDEX idx_sales_region_date ON sales_fact(region_id, sale_date)",
        "impact": "high",
        "estimatedImprovement": "60% faster execution for regional time series queries",
        "queryPatterns": ["regional analysis", "time-based filtering"]
      },
      {
        "type": "partitioning",
        "table": "sales_fact",
        "suggestion": "Partition sales_fact table by sale_date for improved query performance",
        "impact": "medium",
        "estimatedImprovement": "40% faster execution for date range queries"
      }
    ],
    "performanceInsights": {
      "slowQueries": [...],
      "bottlenecks": [...],
      "recommendations": [...]
    }
  }
}
```

## Enhanced Business Terms Endpoints

### 16. Term Categories
**Endpoint:** `GET /api/business-intelligence/terms/categories`

**Purpose:** Get business term categories with hierarchical structure.

**Query Parameters:**
- `includeSubcategories` (optional): Include subcategory details
- `includeTermCounts` (optional): Include term counts per category

**Response:**
```json
{
  "success": true,
  "data": {
    "categories": [
      {
        "id": "sales",
        "name": "Sales & Revenue",
        "description": "Terms related to sales processes, revenue tracking, and customer acquisition",
        "termCount": 25,
        "color": "#52c41a",
        "subcategories": [
          {
            "id": "sales-metrics",
            "name": "Sales Metrics",
            "description": "KPIs and performance indicators",
            "termCount": 12
          }
        ]
      }
    ],
    "totalCategories": 5,
    "totalTerms": 89
  }
}
```

### 17. Term Usage Analytics
**Endpoint:** `GET /api/business-intelligence/terms/usage-analytics`

**Purpose:** Get usage analytics for business terms.

**Query Parameters:**
- `termId` (optional): Specific term ID
- `timeRange` (optional): Time range for analytics
- `includeFrequency` (optional): Include usage frequency data

**Response:**
```json
{
  "success": true,
  "data": {
    "termUsage": [
      {
        "termId": "term-001",
        "name": "Revenue",
        "usageFrequency": 0.95,
        "queryCount": 156,
        "lastUsed": "2024-01-15T10:30:00Z",
        "popularContexts": ["quarterly analysis", "regional comparison"],
        "userDepartments": ["sales", "finance", "marketing"]
      }
    ],
    "trends": {
      "mostUsedTerms": [...],
      "emergingTerms": [...],
      "deprecatedTerms": [...]
    }
  }
}
```

### 18. Term Validation
**Endpoint:** `POST /api/business-intelligence/terms/validate`

**Purpose:** Validate and suggest corrections for business terms in queries.

**Request Body:**
```json
{
  "query": "Show me quartly sales by reigon",
  "strictMode": false,
  "includeSuggestions": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "validationResults": [
      {
        "term": "quartly",
        "isValid": false,
        "suggestions": ["quarterly", "quarter"],
        "confidence": 0.92,
        "position": {
          "start": 12,
          "end": 19
        }
      },
      {
        "term": "reigon",
        "isValid": false,
        "suggestions": ["region", "regions"],
        "confidence": 0.95,
        "position": {
          "start": 29,
          "end": 35
        }
      }
    ],
    "correctedQuery": "Show me quarterly sales by region",
    "overallConfidence": 0.94
  }
}
```

## Advanced Query Processing Endpoints

### 19. Query Alternatives
**Endpoint:** `POST /api/business-intelligence/query/alternatives`

**Purpose:** Get alternative query interpretations with detailed reasoning.

**Request Body:**
```json
{
  "query": "Show me sales by region last quarter",
  "maxAlternatives": 5,
  "includeConfidenceBreakdown": true,
  "includeQuerySuggestions": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "alternatives": [
      {
        "id": "alt-001",
        "interpretation": "Regional sales performance comparison for Q4 2023",
        "confidence": 0.92,
        "reasoning": "High confidence based on temporal indicators and comparison keywords",
        "suggestedQuery": "Show me sales performance by region for Q4 2023 compared to Q3 2023",
        "queryType": "comparison",
        "complexity": "moderate"
      },
      {
        "id": "alt-002",
        "interpretation": "Year-over-year regional sales analysis",
        "confidence": 0.78,
        "reasoning": "Alternative interpretation focusing on annual comparison",
        "suggestedQuery": "Compare regional sales performance Q4 2023 vs Q4 2022",
        "queryType": "trend_analysis",
        "complexity": "complex"
      }
    ],
    "recommendedAlternative": "alt-001",
    "confidenceBreakdown": {
      "factors": [
        {
          "name": "Temporal Context",
          "score": 0.95,
          "impact": "high",
          "explanation": "Clear quarterly time reference"
        }
      ]
    }
  }
}
```

### 20. Query Refinement
**Endpoint:** `POST /api/business-intelligence/query/refine`

**Purpose:** Refine and optimize queries based on AI suggestions.

**Request Body:**
```json
{
  "originalQuery": "sales by region",
  "refinementType": "enhance",
  "includeTimeContext": true,
  "includeComparisons": true,
  "userContext": {
    "role": "analyst",
    "department": "sales"
  }
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "refinedQuery": "Show me quarterly sales performance by region for the last year with year-over-year comparison",
    "improvements": [
      {
        "type": "time_context",
        "description": "Added quarterly granularity and time range",
        "impact": "Provides more specific temporal analysis"
      },
      {
        "type": "comparison",
        "description": "Added year-over-year comparison",
        "impact": "Enables trend analysis and performance evaluation"
      }
    ],
    "confidence": 0.91,
    "estimatedComplexity": "moderate"
  }
}
```

## Analytics & Performance Endpoints

### 21. Usage Analytics
**Endpoint:** `GET /api/business-intelligence/analytics/usage`

**Purpose:** Get Business Intelligence system usage analytics.

**Query Parameters:**
- `startDate` (optional): Start date for analytics
- `endDate` (optional): End date for analytics
- `userId` (optional): Filter by specific user
- `department` (optional): Filter by department

**Response:**
```json
{
  "success": true,
  "data": {
    "summary": {
      "totalQueries": 1250,
      "uniqueUsers": 45,
      "averageConfidence": 0.87,
      "averageResponseTime": 1.8,
      "successRate": 0.94
    },
    "trends": {
      "dailyQueries": [...],
      "popularIntents": [...],
      "topEntities": [...]
    },
    "performance": {
      "averageAnalysisTime": 150,
      "cacheHitRate": 0.78,
      "errorRate": 0.06
    }
  }
}
```

### 22. Query Performance
**Endpoint:** `GET /api/business-intelligence/analytics/performance`

**Purpose:** Get query performance metrics and optimization insights.

**Query Parameters:**
- `queryId` (optional): Specific query ID
- `timeRange` (optional): Time range for metrics
- `includeOptimizations` (optional): Include optimization suggestions

**Response:**
```json
{
  "success": true,
  "data": {
    "metrics": {
      "averageResponseTime": 1.8,
      "p95ResponseTime": 3.2,
      "p99ResponseTime": 5.1,
      "errorRate": 0.06,
      "cacheHitRate": 0.78
    },
    "slowQueries": [
      {
        "queryId": "query-123",
        "query": "Complex aggregation query",
        "responseTime": 8.5,
        "confidence": 0.72,
        "optimizationSuggestions": [...]
      }
    ],
    "optimizations": [
      {
        "type": "caching",
        "description": "Enable caching for frequently used entity patterns",
        "estimatedImprovement": "40% faster response time"
      }
    ]
  }
}
```

### 23. Real-time Metrics
**Endpoint:** `GET /api/business-intelligence/analytics/real-time`

**Purpose:** Get real-time system metrics and performance indicators.

**Query Parameters:**
- `includeActiveQueries` (optional): Include currently processing queries
- `includeSystemHealth` (optional): Include system health metrics

**Response:**
```json
{
  "success": true,
  "data": {
    "currentMetrics": {
      "activeQueries": 12,
      "averageResponseTime": 1.6,
      "systemLoad": 0.65,
      "cacheHitRate": 0.82,
      "errorRate": 0.03
    },
    "activeQueries": [
      {
        "queryId": "query-active-001",
        "query": "Show me sales trends",
        "startTime": "2024-01-15T10:30:00Z",
        "estimatedCompletion": "2024-01-15T10:30:02Z",
        "progress": 0.75
      }
    ],
    "systemHealth": {
      "status": "healthy",
      "uptime": 99.98,
      "lastIncident": "2024-01-10T08:00:00Z"
    }
  }
}
```

## Error Handling

All endpoints follow consistent error response format:

```json
{
  "success": false,
  "error": {
    "code": "INVALID_QUERY",
    "message": "The provided query could not be analyzed",
    "details": {
      "reason": "Query too short or ambiguous",
      "suggestions": ["Try adding more specific terms", "Include time context"]
    },
    "timestamp": "2024-01-15T10:30:00Z",
    "requestId": "req-789"
  }
}
```

### Common Error Codes:
- `INVALID_QUERY`: Query cannot be processed
- `INSUFFICIENT_CONTEXT`: Not enough context for analysis
- `RATE_LIMIT_EXCEEDED`: Too many requests
- `UNAUTHORIZED`: Invalid or missing authentication
- `INTERNAL_ERROR`: Server-side processing error
- `TIMEOUT`: Request processing timeout

## Rate Limiting

All endpoints are subject to rate limiting:
- **Standard Users**: 100 requests per minute
- **Premium Users**: 500 requests per minute
- **Admin Users**: 1000 requests per minute

Rate limit headers included in responses:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1642248000
```

## Implementation Priority

### Phase 1 (Core Functionality - Essential for Basic Operation):
1. `POST /api/business-intelligence/query/analyze` - Core query analysis
2. `GET /api/business-intelligence/context/business-profile` - Business context
3. `GET /api/business-intelligence/context/entities` - Entity detection
4. `GET /api/business-intelligence/query/suggestions` - Query suggestions
5. `GET /api/business-intelligence/query/history` - Query history

### Phase 2 (Enhanced Features - Advanced Analysis):
6. `POST /api/business-intelligence/query/understand` - Step-by-step processing
7. `GET /api/business-intelligence/context/intent` - Intent classification
8. `POST /api/business-intelligence/schema/analyze` - Schema intelligence
9. `GET /api/business-intelligence/terms/glossary` - Business terms
10. `POST /api/business-intelligence/query/alternatives` - Alternative interpretations

### Phase 3 (Advanced Features - Professional Capabilities):
11. `GET /api/business-intelligence/schema/join-recommendations` - Join suggestions
12. `GET /api/business-intelligence/schema/optimization` - Schema optimization
13. `GET /api/business-intelligence/terms/categories` - Term categories
14. `GET /api/business-intelligence/terms/search` - Advanced term search
15. `POST /api/business-intelligence/terms/validate` - Term validation

### Phase 4 (Analytics & Performance - Enterprise Features):
16. `GET /api/business-intelligence/analytics/usage` - Usage analytics
17. `GET /api/business-intelligence/analytics/performance` - Performance metrics
18. `GET /api/business-intelligence/analytics/real-time` - Real-time monitoring
19. `GET /api/business-intelligence/terms/usage-analytics` - Term analytics
20. `POST /api/business-intelligence/query/refine` - Query refinement

### Phase 5 (Extended Features - Additional Capabilities):
21. `GET /api/business-intelligence/schema/relationships` - Table relationships
22. `GET /api/business-intelligence/terms/relationships` - Term relationships
23. Additional optimization and monitoring endpoints

## Notes for Backend Implementation

1. **AI/ML Integration**: Endpoints require integration with NLP models for entity recognition, intent classification, and query understanding.

2. **Caching Strategy**: Implement caching for frequently analyzed queries and business context to improve performance.

3. **Database Integration**: Requires access to metadata database, business glossary, and schema information.

4. **User Context**: All endpoints should consider user role, department, and access permissions for personalized results.

5. **Performance Monitoring**: Implement comprehensive logging and monitoring for all endpoints to track usage and performance.

6. **Security**: Ensure all endpoints validate user permissions and sanitize inputs to prevent injection attacks.

7. **Real-time Processing**: Several endpoints support real-time analysis and require efficient processing pipelines.

8. **Data Quality**: Implement data quality scoring and validation across all analysis endpoints.

## API Coverage Summary

This specification provides **23 comprehensive endpoints** covering all aspects of the Business Intelligence system:

### Core Analysis (5 endpoints)
- Query analysis and understanding
- Business context profiling
- Entity detection and classification
- Query suggestions and history

### Enhanced Features (10 endpoints)
- Step-by-step query processing
- Intent classification with alternatives
- Schema intelligence and optimization
- Business terms management and validation
- Query refinement and alternatives

### Analytics & Performance (8 endpoints)
- Usage analytics and monitoring
- Performance metrics and optimization
- Real-time system health
- Term usage analytics

### Key Features Supported:
✅ **Natural Language Processing** - Advanced NLP for query understanding
✅ **Business Context Intelligence** - Domain-aware analysis with user context
✅ **Schema Intelligence** - Automated schema analysis and optimization
✅ **Business Terms Integration** - Comprehensive glossary management
✅ **Performance Analytics** - Real-time monitoring and optimization
✅ **Query Alternatives** - AI-powered alternative interpretations
✅ **Data Quality Assessment** - Comprehensive quality scoring
✅ **User Personalization** - Role-based and department-specific results

This API specification supports all features implemented in the enhanced Business Intelligence frontend and provides a complete foundation for enterprise-grade business intelligence capabilities.
