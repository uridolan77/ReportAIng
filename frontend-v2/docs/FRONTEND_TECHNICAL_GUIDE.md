# 🔧 Frontend Technical Implementation Guide

## 📋 **Quick Start**

### **Backend Status**
- ✅ **Server Running**: `http://localhost:55244`
- ✅ **Swagger UI**: `http://localhost:55244/swagger/index.html`
- ✅ **All Endpoints Live**: 15+ transparency endpoints with real data
- ✅ **SignalR Hub**: `/hubs/transparency` ready for real-time updates

---

## 🚀 **Phase 1: API Integration (Day 1-2)**

### **1.1 Create Transparency API Service**

```typescript
// src/services/api/transparencyApi.ts
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { 
  ConfidenceBreakdown, 
  AlternativeOptionDto, 
  OptimizationSuggestionDto,
  TransparencyMetricsDto,
  TransparencySettingsDto,
  ExportTransparencyRequest
} from '../types/transparency';

export const transparencyApi = createApi({
  reducerPath: 'transparencyApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/transparency',
    prepareHeaders: (headers, { getState }) => {
      const token = selectAuthToken(getState());
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['Transparency', 'Metrics', 'Traces', 'Settings'],
  endpoints: (builder) => ({
    // Core Analysis Endpoints
    getConfidenceBreakdown: builder.query<ConfidenceBreakdown, string>({
      query: (analysisId) => `confidence/${analysisId}`,
      providesTags: ['Transparency'],
    }),
    
    getAlternativeOptions: builder.query<AlternativeOptionDto[], string>({
      query: (traceId) => `alternatives/${traceId}`,
      providesTags: ['Transparency'],
    }),
    
    getOptimizationSuggestions: builder.mutation<OptimizationSuggestionDto[], {
      userQuery: string;
      traceId?: string;
      priority?: string;
    }>({
      query: (body) => ({
        url: 'optimize',
        method: 'POST',
        body,
      }),
    }),
    
    // Metrics & Analytics
    getTransparencyMetrics: builder.query<TransparencyMetricsDto, {
      userId?: string;
      days?: number;
    }>({
      query: (params) => ({
        url: 'metrics',
        params,
      }),
      providesTags: ['Metrics'],
    }),
    
    getDashboardMetrics: builder.query<TransparencyDashboardMetricsDto, {
      days?: number;
    }>({
      query: (params) => ({
        url: 'metrics/dashboard',
        params,
      }),
      providesTags: ['Metrics'],
    }),
    
    // Settings
    getTransparencySettings: builder.query<TransparencySettingsDto, void>({
      query: () => 'settings',
      providesTags: ['Settings'],
    }),
    
    updateTransparencySettings: builder.mutation<void, TransparencySettingsDto>({
      query: (settings) => ({
        url: 'settings',
        method: 'PUT',
        body: settings,
      }),
      invalidatesTags: ['Settings'],
    }),
    
    // Export
    exportTransparencyData: builder.mutation<Blob, ExportTransparencyRequest>({
      query: (request) => ({
        url: 'export',
        method: 'POST',
        body: request,
        responseHandler: (response) => response.blob(),
      }),
    }),
    
    // Traces
    getRecentTraces: builder.query<TransparencyTraceDto[], {
      userId?: string;
      limit?: number;
    }>({
      query: (params) => ({
        url: 'traces/recent',
        params,
      }),
      providesTags: ['Traces'],
    }),
    
    getTraceDetail: builder.query<TransparencyTraceDetailDto, string>({
      query: (traceId) => `traces/${traceId}/detail`,
      providesTags: ['Traces'],
    }),
  }),
});

export const {
  useGetConfidenceBreakdownQuery,
  useGetAlternativeOptionsQuery,
  useGetOptimizationSuggestionsMutation,
  useGetTransparencyMetricsQuery,
  useGetDashboardMetricsQuery,
  useGetTransparencySettingsQuery,
  useUpdateTransparencySettingsMutation,
  useExportTransparencyDataMutation,
  useGetRecentTracesQuery,
  useGetTraceDetailQuery,
} = transparencyApi;
```

### **1.2 TypeScript Interfaces**

```typescript
// src/types/transparency.ts
export interface ConfidenceBreakdown {
  analysisId: string;
  overallConfidence: number;
  factorBreakdown: Record<string, number>;
  confidenceFactors: ConfidenceFactor[];
  timestamp: string;
}

export interface ConfidenceFactor {
  factorName: string;
  score: number;
  weight: number;
  description: string;
}

export interface AlternativeOptionDto {
  optionId: string;
  type: string;
  description: string;
  score: number;
  rationale: string;
  estimatedImprovement: number;
}

export interface OptimizationSuggestionDto {
  suggestionId: string;
  type: string;
  title: string;
  description: string;
  priority: string;
  estimatedTokenSaving: number;
  estimatedPerformanceGain: number;
  implementation: string;
}

export interface TransparencyMetricsDto {
  totalAnalyses: number;
  averageConfidence: number;
  confidenceDistribution: Record<string, number>;
  topIntentTypes: Record<string, number>;
  optimizationImpact: Record<string, double>;
  timeRange: object;
}

// ... Additional 20+ interfaces
```

---

## 🎨 **Phase 2: Core Components (Day 3-5)**

### **2.1 Confidence Breakdown Component**

```typescript
// src/components/transparency/ConfidenceBreakdown.tsx
import React from 'react';
import { Card, Progress, Tooltip, Spin } from 'antd';
import { InfoCircleOutlined } from '@ant-design/icons';
import { useGetConfidenceBreakdownQuery } from '../../services/api/transparencyApi';

interface Props {
  analysisId: string;
  showDetails?: boolean;
}

export const ConfidenceBreakdown: React.FC<Props> = ({ 
  analysisId, 
  showDetails = true 
}) => {
  const { data, isLoading, error } = useGetConfidenceBreakdownQuery(analysisId);

  if (isLoading) return <Spin size="large" />;
  if (error) return <div>Error loading confidence data</div>;
  if (!data) return null;

  return (
    <Card 
      title="Confidence Analysis" 
      extra={
        <Tooltip title="AI confidence in analysis accuracy">
          <InfoCircleOutlined />
        </Tooltip>
      }
    >
      <div className="confidence-overview">
        <Progress
          type="circle"
          percent={Math.round(data.overallConfidence * 100)}
          format={(percent) => `${percent}%`}
          strokeColor={{
            '0%': '#ff4d4f',
            '50%': '#faad14',
            '100%': '#52c41a',
          }}
        />
        <h3>Overall Confidence</h3>
      </div>

      {showDetails && (
        <div className="confidence-factors">
          <h4>Factor Breakdown</h4>
          {data.confidenceFactors.map((factor) => (
            <div key={factor.factorName} className="factor-item">
              <div className="factor-header">
                <span>{factor.factorName}</span>
                <span>{Math.round(factor.score * 100)}%</span>
              </div>
              <Progress 
                percent={Math.round(factor.score * 100)}
                showInfo={false}
                strokeColor="#1890ff"
              />
              <p className="factor-description">{factor.description}</p>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};
```

### **2.2 Alternative Options Panel**

```typescript
// src/components/transparency/AlternativeOptions.tsx
import React from 'react';
import { Card, Table, Tag, Button } from 'antd';
import { useGetAlternativeOptionsQuery } from '../../services/api/transparencyApi';

interface Props {
  traceId: string;
  onSelectAlternative?: (option: AlternativeOptionDto) => void;
}

export const AlternativeOptions: React.FC<Props> = ({ 
  traceId, 
  onSelectAlternative 
}) => {
  const { data, isLoading } = useGetAlternativeOptionsQuery(traceId);

  const columns = [
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type: string) => <Tag color="blue">{type}</Tag>,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Score',
      dataIndex: 'score',
      key: 'score',
      render: (score: number) => `${Math.round(score * 100)}%`,
      sorter: (a, b) => a.score - b.score,
    },
    {
      title: 'Improvement',
      dataIndex: 'estimatedImprovement',
      key: 'improvement',
      render: (improvement: number) => `+${improvement.toFixed(1)}%`,
    },
    {
      title: 'Action',
      key: 'action',
      render: (_, record) => (
        <Button 
          type="primary" 
          size="small"
          onClick={() => onSelectAlternative?.(record)}
        >
          Apply
        </Button>
      ),
    },
  ];

  return (
    <Card title="Alternative Options">
      <Table
        dataSource={data}
        columns={columns}
        loading={isLoading}
        rowKey="optionId"
        size="small"
        pagination={false}
      />
    </Card>
  );
};
```

---

## 🔄 **Phase 3: Real-time Integration (Day 6-8)**

### **3.1 SignalR Hub Setup**

```typescript
// src/services/signalr/transparencyHub.ts
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';

class TransparencyHubService {
  private connection: HubConnection | null = null;
  private listeners: Map<string, Function[]> = new Map();

  async connect(token: string): Promise<void> {
    this.connection = new HubConnectionBuilder()
      .withUrl('/hubs/transparency', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    // Setup event handlers
    this.connection.on('TransparencyUpdate', (update) => {
      this.emit('transparencyUpdate', update);
    });

    this.connection.on('StepCompleted', (data) => {
      this.emit('stepCompleted', data);
    });

    this.connection.on('ConfidenceUpdate', (data) => {
      this.emit('confidenceUpdate', data);
    });

    await this.connection.start();
  }

  async subscribeToTrace(traceId: string): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('SubscribeToTrace', traceId);
    }
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(callback);
  }

  private emit(event: string, data: any): void {
    const callbacks = this.listeners.get(event) || [];
    callbacks.forEach(callback => callback(data));
  }
}

export const transparencyHub = new TransparencyHubService();
```

### **3.2 Live Transparency Panel**

```typescript
// src/components/transparency/LiveTransparencyPanel.tsx
import React, { useEffect, useState } from 'react';
import { Card, Steps, Progress, Alert } from 'antd';
import { transparencyHub } from '../../services/signalr/transparencyHub';

interface Props {
  traceId: string;
}

export const LiveTransparencyPanel: React.FC<Props> = ({ traceId }) => {
  const [currentStep, setCurrentStep] = useState(0);
  const [confidence, setConfidence] = useState(0);
  const [steps, setSteps] = useState<any[]>([]);
  const [isComplete, setIsComplete] = useState(false);

  useEffect(() => {
    // Subscribe to real-time updates
    transparencyHub.subscribeToTrace(traceId);

    transparencyHub.on('stepCompleted', (data) => {
      if (data.traceId === traceId) {
        setSteps(prev => [...prev, data.step]);
        setCurrentStep(prev => prev + 1);
      }
    });

    transparencyHub.on('confidenceUpdate', (data) => {
      if (data.traceId === traceId) {
        setConfidence(data.confidence);
      }
    });

    transparencyHub.on('traceCompleted', (data) => {
      if (data.traceId === traceId) {
        setIsComplete(true);
      }
    });

    return () => {
      // Cleanup subscriptions
    };
  }, [traceId]);

  return (
    <Card title="Live Query Processing">
      <div className="live-confidence">
        <Progress
          percent={Math.round(confidence * 100)}
          status={isComplete ? 'success' : 'active'}
        />
        <p>Current Confidence: {Math.round(confidence * 100)}%</p>
      </div>

      <Steps
        current={currentStep}
        direction="vertical"
        size="small"
        items={steps.map((step, index) => ({
          title: step.stepName,
          description: `Confidence: ${Math.round(step.confidence * 100)}%`,
          status: step.success ? 'finish' : 'error',
        }))}
      />

      {isComplete && (
        <Alert
          message="Query Processing Complete"
          type="success"
          showIcon
          style={{ marginTop: 16 }}
        />
      )}
    </Card>
  );
};
```

---

## 📊 **Phase 4: Testing & Validation**

### **4.1 Test API Integration**

```typescript
// Test with real backend endpoints
const testTransparencyAPI = async () => {
  try {
    // Test authentication
    const token = await getAuthToken();
    
    // Test metrics endpoint
    const metrics = await fetch('/api/transparency/metrics?days=7', {
      headers: { Authorization: `Bearer ${token}` }
    });
    
    console.log('Metrics:', await metrics.json());
    
    // Test settings endpoint
    const settings = await fetch('/api/transparency/settings', {
      headers: { Authorization: `Bearer ${token}` }
    });
    
    console.log('Settings:', await settings.json());
    
  } catch (error) {
    console.error('API Test Failed:', error);
  }
};
```

### **4.2 Component Testing**

```typescript
// src/components/transparency/__tests__/ConfidenceBreakdown.test.tsx
import { render, screen } from '@testing-library/react';
import { Provider } from 'react-redux';
import { ConfidenceBreakdown } from '../ConfidenceBreakdown';

test('renders confidence breakdown with real data', async () => {
  render(
    <Provider store={store}>
      <ConfidenceBreakdown analysisId="test-analysis-123" />
    </Provider>
  );

  // Wait for API call to complete
  await screen.findByText('Overall Confidence');
  
  // Verify confidence display
  expect(screen.getByRole('progressbar')).toBeInTheDocument();
});
```

---

## 🚀 **Ready to Start!**

1. **Backend is Live**: All endpoints working at `http://localhost:55244`
2. **No Mock Data Needed**: Real database integration complete
3. **Authentication Ready**: JWT tokens work with all endpoints
4. **Real-time Ready**: SignalR hub functional
5. **Start with API Integration**: Foundation for all transparency features

The backend transparency system is production-ready and waiting for frontend integration!
