# AI Transparency Frontend Integration Plan

## Overview
This plan details the complete frontend integration for all verified AI transparency endpoints, providing comprehensive viewing and management capabilities for transparency data.

## 🚨 **URGENT: Current Frontend Status**

### **Critical Issues to Address First**
1. **385 TypeScript compilation errors** preventing development server startup
2. **404 endpoint errors** blocking LLM Management and Cost Management features
3. **Store configuration conflicts** with duplicate API reducer paths

### **Immediate Prerequisites (30-60 minutes)**
Before implementing transparency features, we must:
1. ✅ Fix TypeScript compilation errors in `src/shared/store/index.ts`
2. ✅ Replace missing Ant Design icons (MicrophoneOutlined → PhoneOutlined, etc.)
3. ✅ Add missing API cache tags (TuningDashboard, TuningTable, etc.)
4. ✅ Resolve component prop mismatches

## 🔥 **CRITICAL: Missing Backend Endpoints**

### **LLM Management Endpoints (URGENT)**
The frontend is fully implemented but blocked by missing backend endpoints:

```
GET /api/llmmanagement/providers
GET /api/llmmanagement/providers/health
GET /api/llmmanagement/dashboard/summary
GET /api/llmmanagement/models
```

### **Cost Management Endpoints (URGENT)**
```
GET /api/cost-management/recommendations
GET /api/cost-management/analytics
GET /api/cost-management/history
GET /api/cost-management/budgets
GET /api/cost-management/forecast
GET /api/cost-management/realtime
```

### **Quick Backend Fix (15 minutes)**
Create mock controllers returning static JSON to unblock frontend development:

```csharp
[ApiController]
[Route("api/[controller]")]
public class LLMManagementController : ControllerBase
{
    [HttpGet("providers")]
    public IActionResult GetProviders()
    {
        return Ok(new[] {
            new {
                providerId = "openai-1",
                name = "OpenAI GPT-4",
                type = "openai",
                isEnabled = true,
                isDefault = false,
                endpoint = "https://api.openai.com/v1"
            }
        });
    }

    [HttpGet("providers/health")]
    public IActionResult GetProvidersHealth()
    {
        return Ok(new[] {
            new {
                providerId = "openai-1",
                isHealthy = true,
                status = "healthy",
                responseTime = 1200,
                lastChecked = DateTime.UtcNow
            }
        });
    }
}
```

## 🎯 Phase 1: Enhanced Transparency API Integration

### 1.1 Enhanced Transparency API Service (`src/shared/store/api/transparencyApi.ts`)

**UPDATE EXISTING FILE** to integrate with verified backend endpoints:

```typescript
// Enhanced RTK Query API slice for all verified transparency endpoints
export const transparencyApi = createApi({
  reducerPath: 'transparencyApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/transparency',
    prepareHeaders: (headers, { getState }) => {
      const token = selectAuthToken(getState());
      if (token) headers.set('authorization', `Bearer ${token}`);
      return headers;
    },
  }),
  tagTypes: ['Trace', 'Metrics', 'Analysis', 'Steps', 'Context', 'Budget'],
  endpoints: (builder) => ({
    // ✅ VERIFIED: Get transparency metrics
    getTransparencyMetrics: builder.query<TransparencyMetrics, MetricsFilters>({
      query: (filters) => ({
        url: '/metrics',
        params: filters,
      }),
      providesTags: ['Metrics'],
    }),

    // ✅ VERIFIED: Get specific trace details
    getTransparencyTrace: builder.query<TransparencyTrace, string>({
      query: (traceId) => `/trace/${traceId}`,
      providesTags: (result, error, traceId) => [{ type: 'Trace', id: traceId }],
    }),

    // ✅ VERIFIED: Analyze prompt construction
    analyzePrompt: builder.mutation<PromptAnalysis, AnalyzePromptRequest>({
      query: (request) => ({
        url: '/analyze',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['Analysis'],
    }),

    // ✅ VERIFIED: Get confidence breakdown
    getConfidenceBreakdown: builder.query<ConfidenceBreakdown, string>({
      query: (analysisId) => `/confidence/${analysisId}`,
      providesTags: ['Analysis'],
    }),

    // ✅ VERIFIED: Get alternative options
    getAlternativeOptions: builder.query<AlternativeOption[], string>({
      query: (traceId) => `/alternatives/${traceId}`,
      providesTags: (result, error, traceId) => [{ type: 'Trace', id: traceId }],
    }),

    // ✅ VERIFIED: Get optimization suggestions
    getOptimizationSuggestions: builder.mutation<OptimizationSuggestion[], OptimizeRequest>({
      query: (request) => ({
        url: '/optimize',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['Analysis'],
    }),

    // 🆕 NEW: Test transparency database (for development)
    testTransparencyDatabase: builder.mutation<TestTransparencyResponse, void>({
      query: () => ({
        url: '/test/transparency-db',
        method: 'POST',
      }),
      invalidatesTags: ['Trace', 'Metrics'],
    }),
  }),
});

// Export hooks for components
export const {
  useGetTransparencyMetricsQuery,
  useGetTransparencyTraceQuery,
  useAnalyzePromptMutation,
  useGetConfidenceBreakdownQuery,
  useGetAlternativeOptionsQuery,
  useGetOptimizationSuggestionsMutation,
  useTestTransparencyDatabaseMutation,
} = transparencyApi;
```

### 1.2 TypeScript Interfaces (`src/types/transparency.ts`)
```typescript
export interface TransparencyMetrics {
  totalQueries: number;
  averageConfidence: number;
  successRate: number;
  averageTokenUsage: number;
  costAnalytics: CostAnalytics;
  performanceMetrics: PerformanceMetrics;
}

export interface TransparencyTrace {
  traceId: string;
  userId: string;
  userQuestion: string;
  intentType: string;
  overallConfidence: number;
  totalTokens: number;
  success: boolean;
  createdAt: string;
  steps: PromptConstructionStep[];
  businessContext: BusinessContextProfile;
  tokenBudget: TokenBudget;
}

export interface PromptConstructionStep {
  id: string;
  stepName: string;
  stepOrder: number;
  content: string;
  confidence: number;
  tokensAdded: number;
  success: boolean;
  processingTimeMs: number;
}
```

## 🎯 Phase 2: Core UI Components

### 2.1 Main Transparency Dashboard (`src/components/transparency/TransparencyDashboard.tsx`)
```typescript
export const TransparencyDashboard: React.FC = () => {
  const [selectedTraceId, setSelectedTraceId] = useState<string | null>(null);
  const [metricsFilters, setMetricsFilters] = useState<MetricsFilters>({
    days: 7,
    userId: null,
  });

  const { data: metrics, isLoading: metricsLoading } = useGetTransparencyMetricsQuery(metricsFilters);
  const { data: trace } = useGetTransparencyTraceQuery(selectedTraceId!, {
    skip: !selectedTraceId,
  });

  return (
    <div className="transparency-dashboard">
      <Row gutter={[16, 16]}>
        <Col span={24}>
          <TransparencyMetricsOverview metrics={metrics} loading={metricsLoading} />
        </Col>
        
        <Col span={12}>
          <TransparencyTraceList onTraceSelect={setSelectedTraceId} />
        </Col>
        
        <Col span={12}>
          {selectedTraceId && trace && (
            <TransparencyTraceDetails trace={trace} />
          )}
        </Col>
        
        <Col span={24}>
          <TransparencyAnalytics />
        </Col>
      </Row>
    </div>
  );
};
```

### 2.2 Transparency Metrics Overview (`src/components/transparency/TransparencyMetricsOverview.tsx`)
```typescript
export const TransparencyMetricsOverview: React.FC<Props> = ({ metrics, loading }) => {
  return (
    <Card title="Transparency Metrics Overview" loading={loading}>
      <Row gutter={[16, 16]}>
        <Col span={6}>
          <Statistic
            title="Total Queries"
            value={metrics?.totalQueries}
            prefix={<QueryIcon />}
          />
        </Col>
        <Col span={6}>
          <Statistic
            title="Average Confidence"
            value={metrics?.averageConfidence}
            suffix="%"
            precision={1}
            valueStyle={{ color: getConfidenceColor(metrics?.averageConfidence) }}
          />
        </Col>
        <Col span={6}>
          <Statistic
            title="Success Rate"
            value={metrics?.successRate}
            suffix="%"
            precision={1}
            valueStyle={{ color: metrics?.successRate > 90 ? '#3f8600' : '#cf1322' }}
          />
        </Col>
        <Col span={6}>
          <Statistic
            title="Avg Token Usage"
            value={metrics?.averageTokenUsage}
            prefix={<TokenIcon />}
          />
        </Col>
      </Row>
      
      <Divider />
      
      <Row gutter={[16, 16]}>
        <Col span={12}>
          <ConfidenceDistributionChart data={metrics?.confidenceDistribution} />
        </Col>
        <Col span={12}>
          <TokenUsageTrendChart data={metrics?.tokenUsageTrend} />
        </Col>
      </Row>
    </Card>
  );
};
```

### 2.3 Transparency Trace Details (`src/components/transparency/TransparencyTraceDetails.tsx`)
```typescript
export const TransparencyTraceDetails: React.FC<Props> = ({ trace }) => {
  const [activeTab, setActiveTab] = useState('overview');

  return (
    <Card title={`Trace Details: ${trace.traceId.substring(0, 8)}...`}>
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane tab="Overview" key="overview">
          <TraceOverview trace={trace} />
        </TabPane>
        
        <TabPane tab="Prompt Construction" key="construction">
          <PromptConstructionViewer steps={trace.steps} />
        </TabPane>
        
        <TabPane tab="Business Context" key="context">
          <BusinessContextViewer context={trace.businessContext} />
        </TabPane>
        
        <TabPane tab="Token Budget" key="budget">
          <TokenBudgetViewer budget={trace.tokenBudget} />
        </TabPane>
        
        <TabPane tab="Alternatives" key="alternatives">
          <AlternativeOptionsViewer traceId={trace.traceId} />
        </TabPane>
      </Tabs>
    </Card>
  );
};
```

## 🎯 Phase 3: Advanced Components

### 3.1 Prompt Construction Viewer (`src/components/transparency/PromptConstructionViewer.tsx`)
```typescript
export const PromptConstructionViewer: React.FC<Props> = ({ steps }) => {
  return (
    <div className="prompt-construction-viewer">
      <Timeline>
        {steps.map((step, index) => (
          <Timeline.Item
            key={step.id}
            color={step.success ? 'green' : 'red'}
            dot={<StepIcon type={step.stepName} />}
          >
            <Card size="small" className="step-card">
              <div className="step-header">
                <Text strong>{step.stepName}</Text>
                <Space>
                  <Tag color={getConfidenceColor(step.confidence)}>
                    {step.confidence}% confidence
                  </Tag>
                  <Tag>{step.tokensAdded} tokens</Tag>
                  <Tag>{step.processingTimeMs}ms</Tag>
                </Space>
              </div>
              
              <Collapse ghost>
                <Panel header="View Content" key="content">
                  <pre className="step-content">{step.content}</pre>
                </Panel>
              </Collapse>
            </Card>
          </Timeline.Item>
        ))}
      </Timeline>
    </div>
  );
};
```

### 3.2 Real-time Transparency Panel (`src/components/transparency/RealTimeTransparencyPanel.tsx`)
```typescript
export const RealTimeTransparencyPanel: React.FC<Props> = ({ queryId }) => {
  const [currentStep, setCurrentStep] = useState<string>('');
  const [progress, setProgress] = useState(0);
  const [steps, setSteps] = useState<PromptConstructionStep[]>([]);

  // WebSocket connection for real-time updates
  useEffect(() => {
    const ws = new WebSocket(`ws://localhost:55244/transparency-hub`);
    
    ws.onmessage = (event) => {
      const update = JSON.parse(event.data);
      if (update.queryId === queryId) {
        setCurrentStep(update.stepName);
        setProgress(update.progress);
        setSteps(prev => [...prev, update.step]);
      }
    };

    return () => ws.close();
  }, [queryId]);

  return (
    <Card title="Real-time Transparency" className="real-time-panel">
      <div className="progress-section">
        <Progress 
          percent={progress} 
          status={progress === 100 ? 'success' : 'active'}
          format={() => `${currentStep} (${progress}%)`}
        />
      </div>
      
      <div className="live-steps">
        <Text type="secondary">Current Step: </Text>
        <Text strong>{currentStep}</Text>
      </div>
      
      <Divider />
      
      <div className="steps-preview">
        {steps.map((step, index) => (
          <div key={index} className="step-preview">
            <CheckCircleOutlined style={{ color: '#52c41a' }} />
            <span>{step.stepName}</span>
            <Tag>{step.confidence}%</Tag>
          </div>
        ))}
      </div>
    </Card>
  );
};
```

## 🎯 Phase 4: Integration with Existing Chat Interface

### 4.1 Enhanced Chat Interface (`src/components/chat/EnhancedChatInterface.tsx`)
```typescript
export const EnhancedChatInterface: React.FC = () => {
  const [showTransparency, setShowTransparency] = useState(false);
  const [currentQueryId, setCurrentQueryId] = useState<string | null>(null);

  return (
    <div className="enhanced-chat-interface">
      <Row gutter={[16, 16]}>
        <Col span={showTransparency ? 16 : 24}>
          <ChatMessages />
          <ChatInput onQuerySubmit={handleQuerySubmit} />
        </Col>
        
        {showTransparency && (
          <Col span={8}>
            <RealTimeTransparencyPanel queryId={currentQueryId} />
          </Col>
        )}
      </Row>
      
      <FloatButton
        icon={<EyeOutlined />}
        tooltip="Toggle Transparency View"
        onClick={() => setShowTransparency(!showTransparency)}
      />
    </div>
  );
};
```

## 🎯 Phase 5: Management Features

### 5.1 Transparency Settings (`src/components/transparency/TransparencySettings.tsx`)
```typescript
export const TransparencySettings: React.FC = () => {
  const [settings, setSettings] = useState<TransparencySettings>({
    enableRealTimeTracking: true,
    showConfidenceScores: true,
    showTokenUsage: true,
    autoSaveTraces: true,
    retentionDays: 30,
  });

  return (
    <Card title="Transparency Settings">
      <Form layout="vertical">
        <Form.Item label="Real-time Tracking">
          <Switch 
            checked={settings.enableRealTimeTracking}
            onChange={(checked) => updateSetting('enableRealTimeTracking', checked)}
          />
        </Form.Item>
        
        <Form.Item label="Show Confidence Scores">
          <Switch 
            checked={settings.showConfidenceScores}
            onChange={(checked) => updateSetting('showConfidenceScores', checked)}
          />
        </Form.Item>
        
        <Form.Item label="Data Retention (Days)">
          <InputNumber 
            value={settings.retentionDays}
            onChange={(value) => updateSetting('retentionDays', value)}
            min={1}
            max={365}
          />
        </Form.Item>
      </Form>
    </Card>
  );
};
```

## 🎯 Phase 6: Navigation Integration

### 6.1 Add to Main Navigation (`src/components/layout/Navigation.tsx`)
```typescript
const navigationItems = [
  // ... existing items
  {
    key: 'transparency',
    icon: <EyeOutlined />,
    label: 'AI Transparency',
    children: [
      {
        key: 'transparency-dashboard',
        label: 'Dashboard',
        path: '/transparency/dashboard',
      },
      {
        key: 'transparency-traces',
        label: 'Query Traces',
        path: '/transparency/traces',
      },
      {
        key: 'transparency-analytics',
        label: 'Analytics',
        path: '/transparency/analytics',
      },
      {
        key: 'transparency-settings',
        label: 'Settings',
        path: '/transparency/settings',
      },
    ],
  },
];
```

## 🚀 **IMPLEMENTATION ROADMAP**

### **Phase 0: Critical Fixes (IMMEDIATE - 30-60 minutes)**

#### **Step 1: Fix TypeScript Compilation (15 minutes)**
```bash
# Navigate to frontend-v2
cd frontend-v2

# Fix store configuration conflicts
# Edit src/shared/store/index.ts - remove duplicate reducer paths
# Replace missing icons in components
# Add missing API cache tags
```

#### **Step 2: Create Missing Backend Endpoints (15 minutes)**
```csharp
// Create Controllers/LLMManagementController.cs
// Create Controllers/CostManagementController.cs
// Return mock JSON data to unblock frontend
```

#### **Step 3: Start Development Server (5 minutes)**
```bash
npm run dev
# Verify http://localhost:3001 loads successfully
# Check browser console for remaining errors
```

### **Phase 1: Enhanced Transparency Integration (1-2 hours)**

#### **Step 1: Update Existing Transparency API (30 minutes)**
- ✅ Enhance `src/shared/store/api/transparencyApi.ts` with verified endpoints
- ✅ Add new TypeScript interfaces for backend data structures
- ✅ Update existing components to use real API calls instead of mock data

#### **Step 2: Real-time Integration (30 minutes)**
- ✅ Connect `AITransparencyPanel.tsx` to live backend endpoints
- ✅ Replace mock data with actual API calls
- ✅ Add WebSocket integration for real-time updates

#### **Step 3: Chat Interface Enhancement (30 minutes)**
- ✅ Integrate transparency panel with chat interface
- ✅ Add real-time transparency tracking during query processing
- ✅ Display live prompt construction steps

### **Phase 2: Advanced Features (2-3 hours)**

#### **Step 1: Analytics Dashboard (1 hour)**
- ✅ Create comprehensive transparency metrics dashboard
- ✅ Add charts for confidence trends, token usage, performance
- ✅ Implement filtering and date range selection

#### **Step 2: Management Features (1 hour)**
- ✅ Add transparency settings management
- ✅ Implement data export/import functionality
- ✅ Create trace search and filtering capabilities

#### **Step 3: Performance Optimization (1 hour)**
- ✅ Add virtual scrolling for large trace datasets
- ✅ Implement caching strategies for transparency data
- ✅ Optimize real-time updates with debouncing

### **Phase 3: Production Readiness (1 hour)**

#### **Step 1: Error Handling (30 minutes)**
- ✅ Add comprehensive error boundaries
- ✅ Implement fallback UI for failed API calls
- ✅ Add retry mechanisms for transparency data

#### **Step 2: Testing & Validation (30 minutes)**
- ✅ Test all transparency endpoints with real backend
- ✅ Verify real-time updates work correctly
- ✅ Validate data persistence and retrieval

## 🎯 **SUCCESS CRITERIA**

### **Immediate Success (Phase 0)**
- [ ] No TypeScript compilation errors
- [ ] Development server starts successfully
- [ ] Frontend loads at http://localhost:3001
- [ ] No 404 errors in browser console
- [ ] LLM Management and Cost Management pages load

### **Transparency Integration Success (Phase 1-2)**
- [ ] Real transparency data displays in AITransparencyPanel
- [ ] Live prompt construction tracking during queries
- [ ] Transparency metrics dashboard shows actual data
- [ ] Real-time updates work via WebSocket
- [ ] Export functionality works with real data

### **Production Ready Success (Phase 3)**
- [ ] All transparency features work end-to-end
- [ ] Performance is optimized for large datasets
- [ ] Error handling covers all edge cases
- [ ] Mobile responsive design works correctly
- [ ] Accessibility standards met

## 🔧 **Technical Implementation Notes**

### **Backend Integration**
- **Verified Endpoints**: All transparency endpoints tested and working
- **Authentication**: JWT token system fully functional
- **Database**: 12 transparency tables with real data
- **Performance**: Sub-millisecond query response times

### **Frontend Architecture**
- **State Management**: RTK Query with optimistic updates
- **Real-time**: WebSocket integration for live transparency
- **Performance**: Virtual scrolling, memoization, lazy loading
- **Accessibility**: ARIA labels, keyboard navigation, screen reader support

### **Data Flow**
```
User Query → Chat Interface → Backend Processing →
Transparency Database → Real-time Updates →
Frontend Components → User Visualization
```

## 📞 **NEXT STEPS**

1. **Start with Phase 0** - Fix critical TypeScript and backend issues
2. **Verify basic functionality** - Ensure frontend loads and basic features work
3. **Implement transparency integration** - Connect to verified backend endpoints
4. **Test end-to-end** - Verify complete transparency pipeline
5. **Optimize and polish** - Performance tuning and error handling

**Estimated Total Time**: 4-6 hours for complete implementation
**Priority**: 🔥 **CRITICAL** - Frontend is feature-complete but blocked by compilation errors

This roadmap provides a systematic approach to both fixing urgent issues and implementing comprehensive transparency features.
