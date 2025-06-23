# 🚀 ProcessFlow Frontend Migration Guide

## 📋 **Overview**

The backend has been successfully migrated from the old transparency system to the new **ProcessFlow System**. This guide provides the frontend team with the updated API endpoints and data structures.

## 🔄 **API Endpoint Changes**

### **1. Transparency Controller Updates**

#### **✅ Updated Endpoints**

```typescript
// OLD: Get transparency trace
GET /api/transparency/trace/{traceId}
// Returns: TransparencyReport

// NEW: Get process flow session
GET /api/transparency/trace/{sessionId}
// Returns: ProcessFlowSession
```

#### **📊 New ProcessFlowSession Structure**
```typescript
interface ProcessFlowSession {
  sessionId: string;
  userId: string;
  userQuery: string;
  queryType: string;
  status: 'NotStarted' | 'InProgress' | 'Completed' | 'Failed' | 'Cancelled';
  startTime: string;
  endTime?: string;
  totalDurationMs?: number;
  overallConfidence?: number;
  generatedSQL?: string;
  executionResult?: string;
  conversationId?: string;
  messageId?: string;
  
  // Related data
  steps: ProcessFlowStep[];
  logs: ProcessFlowLog[];
  transparency?: ProcessFlowTransparency;
}

interface ProcessFlowStep {
  stepId: string;
  sessionId: string;
  stepType: 'SemanticAnalysis' | 'SchemaRetrieval' | 'PromptBuilding' | 'AIGeneration' | 'SQLExecution';
  name: string;
  status: 'NotStarted' | 'InProgress' | 'Completed' | 'Failed' | 'Skipped';
  startTime: string;
  endTime?: string;
  durationMs?: number;
  confidence?: number;
  inputData?: any;
  outputData?: any;
  errorMessage?: string;
}

interface ProcessFlowTransparency {
  sessionId: string;
  model?: string;
  temperature?: number;
  promptTokens?: number;
  completionTokens?: number;
  totalTokens?: number;
  estimatedCost?: number;
  confidence?: number;
  aiProcessingTimeMs?: number;
  apiCallCount?: number;
}
```

### **2. Analytics Controller Updates**

#### **✅ Updated Token Usage Endpoints**

```typescript
// Token Usage Statistics
GET /api/analytics/token-usage
// Returns: Enhanced statistics from ProcessFlow data
interface TokenUsageStatistics {
  totalRequests: number;
  totalTokens: number;
  totalCost: number;
  averageTokensPerRequest: number;
  averageCostPerRequest: number;
  dateRange: { start: string; end: string };
  userId?: string;
}

// Daily Token Usage
GET /api/analytics/token-usage/daily
// Returns: Daily analytics from ProcessFlow sessions
interface DailyTokenUsage {
  date: string;
  totalRequests: number;
  totalTokens: number;
  totalCost: number;
  averageTokensPerRequest: number;
  requestType: string;
  intentType: string;
}
```

### **3. Query Controller - Enhanced Response**

#### **✅ Enhanced Query Response with ProcessFlow Data**

```typescript
// Enhanced Query Endpoint (unchanged URL)
POST /api/query/enhanced

// Enhanced response with ProcessFlow transparency data
interface EnhancedQueryResponse {
  processedQuery?: ProcessedQuery;
  queryResult?: QueryResult;
  semanticAnalysis?: SemanticAnalysisResponse;
  classification?: ClassificationResponse;
  alternatives: AlternativeQueryResponse[];
  success: boolean;
  timestamp: string;
  
  // NEW: ProcessFlow transparency metadata
  transparencyData?: {
    traceId: string;
    businessContext: {
      intent: string;
      confidence: number;
      domain: string;
      entities: string[];
    };
    tokenUsage: {
      allocatedTokens: number;
      estimatedCost: number;
      provider: string;
    };
    processingSteps: number;
  };
}
```

## 🔧 **Frontend Integration Steps**

### **Step 1: Update API Types**

Create new TypeScript interfaces for ProcessFlow data:

```typescript
// src/types/processFlow.ts
export interface ProcessFlowSession {
  // ... (see structure above)
}

export interface ProcessFlowStep {
  // ... (see structure above)
}

export interface ProcessFlowTransparency {
  // ... (see structure above)
}
```

### **Step 2: Update RTK Query Endpoints**

```typescript
// src/store/api/transparencyApi.ts
export const transparencyApi = createApi({
  reducerPath: 'transparencyApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/transparency',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) headers.set('authorization', `Bearer ${token}`);
      return headers;
    },
  }),
  tagTypes: ['ProcessFlowSession', 'TransparencyMetrics'],
  endpoints: (builder) => ({
    // Updated endpoint
    getProcessFlowSession: builder.query<ProcessFlowSession, string>({
      query: (sessionId) => `/trace/${sessionId}`,
      providesTags: ['ProcessFlowSession'],
    }),
    
    // Updated analytics endpoint
    getTransparencyMetrics: builder.query<any, { userId?: string; days?: number }>({
      query: ({ userId, days = 7 }) => ({
        url: '/metrics',
        params: { userId, days },
      }),
      providesTags: ['TransparencyMetrics'],
    }),
    
    // Updated analysis endpoint
    analyzePromptConstruction: builder.mutation<ProcessFlowSession, AnalyzePromptRequest>({
      query: (body) => ({
        url: '/analyze',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['ProcessFlowSession'],
    }),
  }),
});

export const {
  useGetProcessFlowSessionQuery,
  useGetTransparencyMetricsQuery,
  useAnalyzePromptConstructionMutation,
} = transparencyApi;
```

### **Step 3: Update Analytics API**

```typescript
// src/store/api/analyticsApi.ts
export const analyticsApi = createApi({
  reducerPath: 'analyticsApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/analytics',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) headers.set('authorization', `Bearer ${token}`);
      return headers;
    },
  }),
  tagTypes: ['TokenUsage', 'Analytics'],
  endpoints: (builder) => ({
    // Updated token usage endpoint
    getTokenUsageStatistics: builder.query<TokenUsageStatistics, {
      startDate?: string;
      endDate?: string;
      userId?: string;
    }>({
      query: (params) => ({
        url: '/token-usage',
        params,
      }),
      providesTags: ['TokenUsage'],
    }),
    
    // Updated daily usage endpoint
    getDailyTokenUsage: builder.query<DailyTokenUsage[], {
      startDate?: string;
      endDate?: string;
    }>({
      query: (params) => ({
        url: '/token-usage/daily',
        params,
      }),
      providesTags: ['TokenUsage'],
    }),
  }),
});

export const {
  useGetTokenUsageStatisticsQuery,
  useGetDailyTokenUsageQuery,
} = analyticsApi;
```

## 📊 **UI Component Updates**

### **Step 4: Update Transparency Components**

```typescript
// src/components/transparency/ProcessFlowViewer.tsx
import React from 'react';
import { useGetProcessFlowSessionQuery } from '../../store/api/transparencyApi';

interface ProcessFlowViewerProps {
  sessionId: string;
}

export const ProcessFlowViewer: React.FC<ProcessFlowViewerProps> = ({ sessionId }) => {
  const { data: session, isLoading, error } = useGetProcessFlowSessionQuery(sessionId);

  if (isLoading) return <div>Loading process flow...</div>;
  if (error) return <div>Error loading process flow</div>;
  if (!session) return <div>Process flow not found</div>;

  return (
    <div className="process-flow-viewer">
      <div className="session-header">
        <h3>Process Flow Session</h3>
        <div className="session-meta">
          <span>Status: {session.status}</span>
          <span>Duration: {session.totalDurationMs}ms</span>
          <span>Confidence: {session.overallConfidence?.toFixed(2)}</span>
        </div>
      </div>
      
      <div className="steps-timeline">
        {session.steps.map((step) => (
          <div key={step.stepId} className={`step step-${step.status.toLowerCase()}`}>
            <div className="step-header">
              <h4>{step.name}</h4>
              <span className="step-status">{step.status}</span>
            </div>
            <div className="step-details">
              <div>Duration: {step.durationMs}ms</div>
              {step.confidence && <div>Confidence: {step.confidence.toFixed(2)}</div>}
              {step.errorMessage && <div className="error">Error: {step.errorMessage}</div>}
            </div>
          </div>
        ))}
      </div>
      
      {session.transparency && (
        <div className="transparency-info">
          <h4>AI Transparency</h4>
          <div>Model: {session.transparency.model}</div>
          <div>Total Tokens: {session.transparency.totalTokens}</div>
          <div>Estimated Cost: ${session.transparency.estimatedCost?.toFixed(4)}</div>
        </div>
      )}
    </div>
  );
};
```

## 🎯 **Migration Checklist**

### **✅ Backend Changes (Completed)**
- [x] Updated QueryController to use ProcessFlowService
- [x] Updated TransparencyController to use ProcessFlowService  
- [x] Updated AnalyticsController to use ProcessFlowService
- [x] Updated Program.cs service registrations
- [x] All endpoints return ProcessFlow data structures

### **📋 Frontend Changes (Required)**
- [ ] Update TypeScript interfaces for ProcessFlow data
- [ ] Update RTK Query endpoints for new API responses
- [ ] Update transparency components to use ProcessFlowSession
- [ ] Update analytics components to use ProcessFlow metrics
- [ ] Update enhanced query response handling
- [ ] Test all transparency and analytics features
- [ ] Update documentation and user guides

## 🚀 **Benefits for Frontend**

### **📊 Enhanced Data**
- **Real-time Process Tracking**: Step-by-step process visualization
- **Detailed Transparency**: Comprehensive AI operation insights
- **Better Analytics**: More accurate token usage and cost tracking
- **Unified Data Model**: Consistent data structure across all features

### **🎯 Improved UX**
- **Process Flow Visualization**: Users can see exactly what's happening
- **Confidence Indicators**: Per-step and overall confidence scoring
- **Performance Metrics**: Real-time processing time and efficiency data
- **Error Handling**: Detailed error information for better debugging

## 📞 **Support**

For any questions or issues during migration:
- **Backend Team**: Available for API clarification and data structure questions
- **Documentation**: This guide and inline API documentation
- **Testing**: Backend endpoints are fully tested and ready for integration

---

**Migration Priority**: High - All transparency and analytics features depend on these changes
**Timeline**: Frontend integration should be completed within 1-2 sprints
**Testing**: Comprehensive testing required for all transparency and analytics features
