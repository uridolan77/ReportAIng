# 🚀 Backend API Requirements - Complete List

## 📋 **Current Status**

The frontend is **fully implemented** and ready for integration. Multiple systems are calling backend endpoints that return 404 errors. This document lists ALL missing endpoints that need to be implemented.

## 🚨 **404 Errors Currently Happening**

```
GET /api/llmmanagement/dashboard/summary
GET /api/llmmanagement/providers
GET /api/llmmanagement/providers/health
GET /api/llmmanagement/models
GET /api/cost-management/recommendations
GET /api/cost-management/analytics
GET /api/cost-management/history
GET /api/cost-management/budgets
GET /api/cost-management/forecast
GET /api/cost-management/realtime
```

## 🎯 **Required API Endpoints**

### **Base URL**: `/api/llmmanagement`

---

## 1. **Provider Management**

### **GET** `/api/llmmanagement/providers`
**Purpose**: Get all LLM providers
**Response**: Array of LLMProviderConfig
```typescript
interface LLMProviderConfig {
  providerId: string
  name: string
  type: 'openai' | 'anthropic' | 'azure' | 'google' | 'local'
  apiKey?: string
  endpoint?: string
  organization?: string
  isEnabled: boolean
  isDefault: boolean
  settings?: string // JSON string
  createdAt: string
  updatedAt: string
}
```

### **GET** `/api/llmmanagement/providers/{providerId}`
**Purpose**: Get specific provider details
**Response**: Single LLMProviderConfig

### **POST** `/api/llmmanagement/providers`
**Purpose**: Create or update provider
**Body**: LLMProviderConfig
**Response**: Updated LLMProviderConfig

### **DELETE** `/api/llmmanagement/providers/{providerId}`
**Purpose**: Delete provider
**Response**: Success message

### **POST** `/api/llmmanagement/providers/{providerId}/test`
**Purpose**: Test provider connection
**Response**: LLMProviderStatus
```typescript
interface LLMProviderStatus {
  providerId: string
  isHealthy: boolean
  status: 'healthy' | 'degraded' | 'unhealthy' | 'offline'
  responseTime?: number
  lastChecked: string
  errorMessage?: string
  metadata?: Record<string, any>
}
```

### **GET** `/api/llmmanagement/providers/health`
**Purpose**: Get health status for all providers
**Response**: Array of LLMProviderStatus

---

## 2. **Model Management**

### **GET** `/api/llmmanagement/models`
**Purpose**: Get all models (optionally filtered by provider)
**Query Params**: `providerId?: string`
**Response**: Array of LLMModelConfig
```typescript
interface LLMModelConfig {
  modelId: string
  providerId: string
  name: string
  displayName: string
  temperature: number
  maxTokens: number
  topP: number
  frequencyPenalty: number
  presencePenalty: number
  isEnabled: boolean
  useCase?: string
  costPerToken: number
  capabilities?: string // JSON string
}
```

### **GET** `/api/llmmanagement/models/{modelId}`
**Purpose**: Get specific model details
**Response**: Single LLMModelConfig

### **POST** `/api/llmmanagement/models`
**Purpose**: Create or update model
**Body**: LLMModelConfig
**Response**: Updated LLMModelConfig

### **DELETE** `/api/llmmanagement/models/{modelId}`
**Purpose**: Delete model
**Response**: Success message

### **GET** `/api/llmmanagement/models/default/{useCase}`
**Purpose**: Get default model for specific use case
**Response**: LLMModelConfig

### **POST** `/api/llmmanagement/models/{modelId}/set-default/{useCase}`
**Purpose**: Set model as default for use case
**Response**: Success message

---

## 3. **Usage Tracking**

### **GET** `/api/llmmanagement/usage/history`
**Purpose**: Get usage history with filtering
**Query Params**:
- `startDate?: string`
- `endDate?: string`
- `providerId?: string`
- `modelId?: string`
- `userId?: string`
- `requestType?: string`
- `skip?: number`
- `take?: number`

**Response**: Array of LLMUsageLog
```typescript
interface LLMUsageLog {
  id: number
  requestId: string
  userId: string
  providerId: string
  modelId: string
  requestType: string
  requestText: string
  responseText: string
  inputTokens: number
  outputTokens: number
  totalTokens: number
  cost: number
  durationMs: number
  success: boolean
  errorMessage?: string
  timestamp: string
  metadata?: string
}
```

### **GET** `/api/llmmanagement/usage/analytics`
**Purpose**: Get usage analytics
**Query Params**: `startDate: string`, `endDate: string`, `providerId?: string`, `modelId?: string`
**Response**: LLMUsageAnalytics
```typescript
interface LLMUsageAnalytics {
  totalRequests: number
  totalTokens: number
  totalCost: number
  averageResponseTime: number
  successRate: number
  requestsByProvider: Record<string, number>
  costsByProvider: Record<string, number>
  tokensByProvider: Record<string, number>
  requestsByModel: Record<string, number>
  dailyUsage: Array<{
    date: string
    requests: number
    tokens: number
    cost: number
  }>
}
```

### **POST** `/api/llmmanagement/usage/export`
**Purpose**: Export usage data
**Query Params**: `startDate: string`, `endDate: string`, `format?: 'csv' | 'json'`
**Response**: File download (CSV or JSON)

---

## 4. **Cost Management**

### **GET** `/api/llmmanagement/costs/summary`
**Purpose**: Get cost summary by provider
**Query Params**: `startDate: string`, `endDate: string`, `providerId?: string`
**Response**: Array of LLMCostSummary

### **GET** `/api/llmmanagement/costs/current-month`
**Purpose**: Get current month cost
**Query Params**: `providerId?: string`
**Response**: number (cost amount)

### **GET** `/api/llmmanagement/costs/projections`
**Purpose**: Get cost projections
**Response**: Record<string, number> (provider -> projected cost)

### **POST** `/api/llmmanagement/costs/limits/{providerId}`
**Purpose**: Set monthly cost limit for provider
**Body**: number (monthly limit)
**Response**: Success message

### **GET** `/api/llmmanagement/costs/alerts`
**Purpose**: Get active cost alerts
**Response**: Array of CostAlert

---

## 5. **Performance Monitoring**

### **GET** `/api/llmmanagement/performance/metrics`
**Purpose**: Get performance metrics
**Query Params**: `startDate: string`, `endDate: string`
**Response**: Record<string, ProviderPerformanceMetrics>

### **GET** `/api/llmmanagement/performance/cache-hit-rates`
**Purpose**: Get cache hit rates by provider
**Response**: Record<string, number>

### **GET** `/api/llmmanagement/performance/error-analysis`
**Purpose**: Get error analysis
**Query Params**: `startDate: string`, `endDate: string`
**Response**: Record<string, ErrorAnalysis>

---

## 6. **Dashboard Summary**

### **GET** `/api/llmmanagement/dashboard/summary`
**Purpose**: Get dashboard summary data
**Response**: DashboardSummary
```typescript
interface DashboardSummary {
  providers: {
    total: number
    enabled: number
    healthy: number
  }
  usage: {
    totalRequests: number
    totalTokens: number
    averageResponseTime: number
    successRate: number
  }
  costs: {
    currentMonth: number
    total30Days: number
    activeAlerts: number
  }
  performance: {
    averageResponseTime: number
    overallSuccessRate: number
    totalErrors: number
  }
  lastUpdated: string
}
```

---

## 🗄️ **Database Tables**

The following tables from `llmtablesscheme.sql` should be used:

1. **LLMProviderConfigs** - Provider configurations
2. **LLMModelConfigs** - Model configurations  
3. **LLMUsageLogs** - Usage tracking
4. **LLMCostTracking** - Cost monitoring
5. **LLMPerformanceMetrics** - Performance data
6. **LLMBudgetManagement** - Budget controls

---

## 🚀 **Frontend Pages Ready**

✅ **LLM Management Dashboard** (`/admin/llm-management`)
- Provider management with real-time health monitoring
- Performance analytics
- Cost optimization
- Configuration management

✅ **Models Management** (`/admin/llm-models`)
- Model CRUD operations
- Provider-specific model configuration
- Default model assignment
- Parameter tuning

---

## 🎯 **Priority Implementation Order**

1. **HIGH PRIORITY** (Required for basic functionality):
   - Provider CRUD endpoints
   - Provider health check
   - Dashboard summary
   - Model CRUD endpoints

2. **MEDIUM PRIORITY** (For full functionality):
   - Usage tracking and analytics
   - Cost management
   - Performance monitoring

3. **LOW PRIORITY** (Nice to have):
   - Export functionality
   - Advanced analytics
   - Cache hit rates

---

## 📝 **Notes for Backend Developer**

1. **Authentication**: All endpoints should require admin authentication
2. **Validation**: Implement proper input validation for all endpoints
3. **Error Handling**: Return consistent error responses with proper HTTP status codes
4. **Logging**: Log all LLM operations for audit purposes
5. **Rate Limiting**: Consider implementing rate limiting for provider testing endpoints
6. **Security**: Never expose API keys in responses (mask or exclude them)

---

## 7. **General Cost Management** (Separate from LLM-specific costs)

### **GET** `/api/cost-management/analytics`
**Purpose**: Get cost analytics summary
**Query Params**: `startDate?: string`, `endDate?: string`
**Response**: CostAnalyticsSummary

### **GET** `/api/cost-management/history`
**Purpose**: Get cost history
**Query Params**: `startDate?: string`, `endDate?: string`, `limit?: number`, `page?: number`
**Response**: CostHistoryResponse

### **GET** `/api/cost-management/breakdown/{dimension}`
**Purpose**: Get cost breakdown by dimension
**Query Params**: `startDate?: string`, `endDate?: string`
**Response**: CostBreakdownResponse

### **GET** `/api/cost-management/trends`
**Purpose**: Get cost trends
**Query Params**: `category?: string`, `granularity?: string`, `periods?: number`
**Response**: CostTrendsResponse

### **GET** `/api/cost-management/budgets`
**Purpose**: Get all budgets
**Response**: BudgetsResponse

### **POST** `/api/cost-management/budgets`
**Purpose**: Create new budget
**Body**: CreateBudgetRequest
**Response**: BudgetResponse

### **PUT** `/api/cost-management/budgets/{budgetId}`
**Purpose**: Update budget
**Body**: UpdateBudgetRequest
**Response**: BudgetResponse

### **DELETE** `/api/cost-management/budgets/{budgetId}`
**Purpose**: Delete budget
**Response**: Success message

### **POST** `/api/cost-management/predict`
**Purpose**: Predict costs
**Body**: CostPredictionRequest
**Response**: CostPredictionResponse

### **GET** `/api/cost-management/forecast`
**Purpose**: Get cost forecast
**Query Params**: `days?: number`
**Response**: CostForecastResponse

### **GET** `/api/cost-management/recommendations`
**Purpose**: Get optimization recommendations
**Response**: RecommendationsResponse

### **GET** `/api/cost-management/roi`
**Purpose**: Get ROI analysis
**Query Params**: `startDate?: string`, `endDate?: string`
**Response**: ROIAnalysisResponse

### **GET** `/api/cost-management/realtime`
**Purpose**: Get real-time cost metrics
**Response**: RealTimeMetricsResponse

---

## 🗄️ **Database Tables**

The following tables from `llmtablesscheme.sql` should be used:

1. **LLMProviderConfigs** - Provider configurations
2. **LLMModelConfigs** - Model configurations
3. **LLMUsageLogs** - Usage tracking
4. **LLMCostTracking** - Cost monitoring
5. **LLMPerformanceMetrics** - Performance data
6. **LLMBudgetManagement** - Budget controls

**Additional tables needed for general cost management:**
7. **CostAnalytics** - General cost analytics
8. **BudgetManagement** - General budget management
9. **CostHistory** - Historical cost data
10. **CostRecommendations** - Optimization recommendations

---

## 🚀 **Frontend Pages Ready**

✅ **LLM Management Dashboard** (`/admin/llm-management`)
- Provider management with real-time health monitoring
- Performance analytics
- Cost optimization
- Configuration management

✅ **Models Management** (`/admin/llm-models`)
- Model CRUD operations
- Provider-specific model configuration
- Default model assignment
- Parameter tuning

✅ **Cost Management Dashboard** (`/admin/cost-management`)
- Real-time cost tracking
- Budget management
- Cost optimization recommendations
- ROI analysis

---

## 🎯 **Priority Implementation Order**

### **CRITICAL (Required immediately)**:
1. **LLM Provider endpoints**:
   - `GET /api/llmmanagement/providers`
   - `GET /api/llmmanagement/providers/health`
   - `GET /api/llmmanagement/dashboard/summary`

2. **Cost Management endpoints**:
   - `GET /api/cost-management/recommendations`
   - `GET /api/cost-management/analytics`
   - `GET /api/cost-management/realtime`

### **HIGH PRIORITY**:
3. **LLM Models endpoints**:
   - `GET /api/llmmanagement/models`
   - `POST /api/llmmanagement/models`

4. **Budget Management**:
   - `GET /api/cost-management/budgets`
   - `POST /api/cost-management/budgets`

### **MEDIUM PRIORITY**:
5. **Usage tracking and analytics**
6. **Performance monitoring**
7. **Advanced cost features**

### **LOW PRIORITY**:
8. **Export functionality**
9. **Advanced analytics**
10. **Cache hit rates**

---

## 📝 **Notes for Backend Developer**

1. **Authentication**: All endpoints should require admin authentication
2. **Validation**: Implement proper input validation for all endpoints
3. **Error Handling**: Return consistent error responses with proper HTTP status codes
4. **Logging**: Log all LLM operations for audit purposes
5. **Rate Limiting**: Consider implementing rate limiting for provider testing endpoints
6. **Security**: Never expose API keys in responses (mask or exclude them)
7. **CORS**: Ensure proper CORS configuration for frontend integration

## 🔥 **URGENT**

The frontend is **100% ready** and will work immediately once these endpoints are implemented!

**Start with the CRITICAL endpoints to get basic functionality working, then implement HIGH PRIORITY for full features.**
