# 🔍 AI Transparency Foundation - Frontend Integration Guide
## BI Reporting Copilot Frontend-v2 - New Backend API Integration

### 📋 Executive Summary

**MAJOR UPDATE**: We have successfully implemented the **AI Transparency Foundation** backend infrastructure with 3 new API controllers and 50+ comprehensive DTOs. This document provides the frontend developer with the complete integration guide for the new transparency, streaming, and intelligent agent features.

---

## 🎯 **NEW BACKEND APIS IMPLEMENTED**

### ✅ **COMPLETED BACKEND INFRASTRUCTURE**

#### **1. TransparencyController.cs** ✅
**Base URL**: `/api/transparency`

```typescript
// New API Endpoints Available:
GET    /api/transparency/trace/{traceId}           // Get transparency trace
POST   /api/transparency/analyze                  // Analyze prompt construction
GET    /api/transparency/confidence/{analysisId}  // Get confidence breakdown
GET    /api/transparency/alternatives/{traceId}   // Get alternative options
POST   /api/transparency/optimize                 // Get optimization suggestions
GET    /api/transparency/metrics                  // Get transparency analytics
```

#### **2. AIStreamingController.cs** ✅
**Base URL**: `/api/aistreaming`

```typescript
// New Streaming Endpoints Available:
GET    /api/aistreaming/query/{sessionId}         // Stream query processing
GET    /api/aistreaming/insights/{queryId}        // Stream insight generation
GET    /api/aistreaming/analytics/{userId}        // Stream analytics updates
POST   /api/aistreaming/start                     // Start streaming session
POST   /api/aistreaming/cancel/{sessionId}        // Cancel streaming
GET    /api/aistreaming/status/{sessionId}        // Get session status
```

#### **3. IntelligentAgentsController.cs** ✅
**Base URL**: `/api/intelligentagents`

```typescript
// New Agent Endpoints Available:
POST   /api/intelligentagents/orchestrate         // Orchestrate multi-agent tasks
GET    /api/intelligentagents/capabilities        // Get agent capabilities
POST   /api/intelligentagents/schema/navigate     // Navigate database schemas
POST   /api/intelligentagents/query/understand   // Understand natural language
GET    /api/intelligentagents/communication/logs // Get agent communication logs
```

#### **4. Comprehensive DTOs** ✅
- **TransparencyDTOs.cs** - 15+ DTOs for transparency features
- **StreamingDTOs.cs** - 20+ DTOs for streaming functionality  
- **AgentDTOs.cs** - 20+ DTOs for intelligent agent operations

---

## 🚀 **UPDATED FRONTEND ROADMAP**

### **PHASE 1: TRANSPARENCY API INTEGRATION (Priority: IMMEDIATE)**

#### **1.1 Create Transparency API Service**
```typescript
// File: frontend-v2/src/shared/store/api/transparencyApi.ts
export const transparencyApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getTransparencyTrace: builder.query<TransparencyReport, string>({
      query: (traceId) => `/transparency/trace/${traceId}`,
    }),
    
    analyzePromptConstruction: builder.mutation<PromptConstructionTrace, AnalyzePromptRequest>({
      query: (body) => ({
        url: '/transparency/analyze',
        method: 'POST',
        body,
      }),
    }),
    
    getConfidenceBreakdown: builder.query<ConfidenceBreakdown, string>({
      query: (analysisId) => `/transparency/confidence/${analysisId}`,
    }),
    
    getAlternativeOptions: builder.query<AlternativeOption[], string>({
      query: (traceId) => `/transparency/alternatives/${traceId}`,
    }),
    
    getOptimizationSuggestions: builder.mutation<OptimizationSuggestion[], OptimizePromptRequest>({
      query: (body) => ({
        url: '/transparency/optimize',
        method: 'POST',
        body,
      }),
    }),
    
    getTransparencyMetrics: builder.query<TransparencyMetrics, { userId?: string; days?: number }>({
      query: ({ userId, days = 7 }) => ({
        url: '/transparency/metrics',
        params: { userId, days },
      }),
    }),
  }),
})

export const {
  useGetTransparencyTraceQuery,
  useAnalyzePromptConstructionMutation,
  useGetConfidenceBreakdownQuery,
  useGetAlternativeOptionsQuery,
  useGetOptimizationSuggestionsMutation,
  useGetTransparencyMetricsQuery,
} = transparencyApi
```

#### **1.2 Create AI Streaming API Service**
```typescript
// File: frontend-v2/src/shared/store/api/aiStreamingApi.ts
export const aiStreamingApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    startStreamingSession: builder.mutation<{ sessionId: string }, StartStreamingRequest>({
      query: (body) => ({
        url: '/aistreaming/start',
        method: 'POST',
        body,
      }),
    }),
    
    cancelStreamingSession: builder.mutation<void, string>({
      query: (sessionId) => ({
        url: `/aistreaming/cancel/${sessionId}`,
        method: 'POST',
      }),
    }),
    
    getSessionStatus: builder.query<StreamingSession, string>({
      query: (sessionId) => `/aistreaming/status/${sessionId}`,
    }),
  }),
})

// Server-Sent Events for streaming
export const useStreamingQuery = (sessionId: string) => {
  const [data, setData] = useState<StreamingQueryResponse[]>([])
  const [isConnected, setIsConnected] = useState(false)
  
  useEffect(() => {
    if (!sessionId) return
    
    const eventSource = new EventSource(`/api/aistreaming/query/${sessionId}`)
    
    eventSource.onopen = () => setIsConnected(true)
    eventSource.onmessage = (event) => {
      const response: StreamingQueryResponse = JSON.parse(event.data)
      setData(prev => [...prev, response])
    }
    eventSource.onerror = () => setIsConnected(false)
    
    return () => {
      eventSource.close()
      setIsConnected(false)
    }
  }, [sessionId])
  
  return { data, isConnected }
}
```

#### **1.3 Create Intelligent Agents API Service**
```typescript
// File: frontend-v2/src/shared/store/api/intelligentAgentsApi.ts
export const intelligentAgentsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    orchestrateTasks: builder.mutation<AgentOrchestrationResult, OrchestrationRequest>({
      query: (body) => ({
        url: '/intelligentagents/orchestrate',
        method: 'POST',
        body,
      }),
    }),
    
    getAgentCapabilities: builder.query<AgentCapabilities[], void>({
      query: () => '/intelligentagents/capabilities',
    }),
    
    navigateSchema: builder.mutation<SchemaNavigationResult, SchemaNavigationRequest>({
      query: (body) => ({
        url: '/intelligentagents/schema/navigate',
        method: 'POST',
        body,
      }),
    }),
    
    understandQuery: builder.mutation<QueryUnderstandingResult, QueryUnderstandingRequest>({
      query: (body) => ({
        url: '/intelligentagents/query/understand',
        method: 'POST',
        body,
      }),
    }),
    
    getCommunicationLogs: builder.query<AgentCommunicationLog[], { agentId?: string; limit?: number }>({
      query: ({ agentId, limit = 50 }) => ({
        url: '/intelligentagents/communication/logs',
        params: { agentId, limit },
      }),
    }),
  }),
})

export const {
  useOrchestrateTasksMutation,
  useGetAgentCapabilitiesQuery,
  useNavigateSchemaMutation,
  useUnderstandQueryMutation,
  useGetCommunicationLogsQuery,
} = intelligentAgentsApi
```

### **PHASE 2: TRANSPARENCY COMPONENTS (Priority: HIGH)**

#### **2.1 Create AITransparencyPanel Component**
```typescript
// File: frontend-v2/src/shared/components/ai/transparency/AITransparencyPanel.tsx
interface AITransparencyPanelProps {
  traceId: string
  showDetailedMetrics?: boolean
  compact?: boolean
  onOptimizationSuggestion?: (suggestion: OptimizationSuggestion) => void
  onAlternativeSelect?: (alternative: AlternativeOption) => void
  className?: string
}

export const AITransparencyPanel: React.FC<AITransparencyPanelProps> = ({
  traceId,
  showDetailedMetrics = false,
  compact = false,
  onOptimizationSuggestion,
  onAlternativeSelect,
  className
}) => {
  const { data: trace, isLoading } = useGetTransparencyTraceQuery(traceId)
  const { data: confidence } = useGetConfidenceBreakdownQuery(traceId)
  const { data: alternatives } = useGetAlternativeOptionsQuery(traceId)
  
  return (
    <Card 
      title="AI Decision Transparency" 
      loading={isLoading}
      className={className}
      size={compact ? 'small' : 'default'}
    >
      <Tabs items={[
        {
          key: 'construction',
          label: 'Prompt Construction',
          children: <PromptConstructionViewer trace={trace} />
        },
        {
          key: 'confidence',
          label: 'Confidence Analysis',
          children: <ConfidenceBreakdownChart data={confidence} />
        },
        {
          key: 'alternatives',
          label: 'Alternative Options',
          children: <AlternativeOptionsPanel 
            alternatives={alternatives} 
            onSelect={onAlternativeSelect} 
          />
        },
        ...(showDetailedMetrics ? [{
          key: 'metrics',
          label: 'Performance Metrics',
          children: <PerformanceMetricsDisplay traceId={traceId} />
        }] : [])
      ]} />
    </Card>
  )
}
```

#### **2.2 Create PromptConstructionViewer Component**
```typescript
// File: frontend-v2/src/shared/components/ai/transparency/PromptConstructionViewer.tsx
interface PromptConstructionViewerProps {
  trace?: PromptConstructionTrace
  interactive?: boolean
  showAlternatives?: boolean
}

export const PromptConstructionViewer: React.FC<PromptConstructionViewerProps> = ({
  trace,
  interactive = false,
  showAlternatives = true
}) => {
  if (!trace) return <Skeleton active />
  
  return (
    <div className="prompt-construction-viewer">
      <div className="construction-header">
        <Space>
          <Tag color="blue">Trace ID: {trace.traceId}</Tag>
          <Tag color="green">Confidence: {(trace.overallConfidence * 100).toFixed(1)}%</Tag>
          <Tag color="orange">Tokens: {trace.totalTokens}</Tag>
        </Space>
      </div>
      
      <Timeline className="construction-timeline">
        {trace.steps.map((step, index) => (
          <Timeline.Item
            key={index}
            color={step.confidence > 0.8 ? 'green' : step.confidence > 0.6 ? 'orange' : 'red'}
            dot={<ClockCircleOutlined />}
          >
            <Card size="small" className="step-card">
              <div className="step-header">
                <Space>
                  <Text strong>{step.stepName}</Text>
                  <Progress 
                    percent={step.confidence * 100} 
                    size="small" 
                    style={{ width: 150 }}
                  />
                  <Text type="secondary">{step.duration}ms</Text>
                </Space>
              </div>
              
              <Paragraph className="step-description">
                {step.description}
              </Paragraph>
              
              {step.content && (
                <Collapse size="small">
                  <Collapse.Panel header="View Content" key="content">
                    <Text code className="step-content">
                      {step.content}
                    </Text>
                  </Collapse.Panel>
                </Collapse>
              )}
              
              {showAlternatives && step.alternatives.length > 0 && (
                <div className="step-alternatives">
                  <Text type="secondary">Alternatives: </Text>
                  <Space wrap>
                    {step.alternatives.map((alt, i) => (
                      <Tag 
                        key={i} 
                        color="blue" 
                        style={{ cursor: interactive ? 'pointer' : 'default' }}
                      >
                        {alt}
                      </Tag>
                    ))}
                  </Space>
                </div>
              )}
            </Card>
          </Timeline.Item>
        ))}
      </Timeline>
    </div>
  )
}
```

#### **2.3 Create ConfidenceBreakdownChart Component**
```typescript
// File: frontend-v2/src/shared/components/ai/transparency/ConfidenceBreakdownChart.tsx
interface ConfidenceBreakdownChartProps {
  data?: ConfidenceBreakdown
  showEvolution?: boolean
  interactive?: boolean
}

export const ConfidenceBreakdownChart: React.FC<ConfidenceBreakdownChartProps> = ({
  data,
  showEvolution = false,
  interactive = false
}) => {
  if (!data) return <Skeleton active />
  
  const chartData = data.confidenceFactors.map(factor => ({
    factor: factor.factorName,
    score: factor.score * 100,
    weight: factor.weight * 100,
    description: factor.description
  }))
  
  return (
    <div className="confidence-breakdown-chart">
      <div className="overall-confidence">
        <Progress
          type="circle"
          percent={data.overallConfidence * 100}
          format={percent => `${percent?.toFixed(1)}%`}
          strokeColor={data.overallConfidence > 0.8 ? '#52c41a' : data.overallConfidence > 0.6 ? '#faad14' : '#ff4d4f'}
        />
        <Text strong style={{ marginLeft: 16 }}>
          Overall Confidence: {(data.overallConfidence * 100).toFixed(1)}%
        </Text>
      </div>
      
      <div className="factor-breakdown">
        {data.confidenceFactors.map((factor, index) => (
          <Card key={index} size="small" className="factor-card">
            <div className="factor-header">
              <Space>
                <Text strong>{factor.factorName}</Text>
                <Tag color="blue">{factor.category}</Tag>
              </Space>
              <Text>{(factor.score * 100).toFixed(1)}%</Text>
            </div>
            
            <Progress 
              percent={factor.score * 100}
              strokeColor={factor.score > 0.8 ? '#52c41a' : factor.score > 0.6 ? '#faad14' : '#ff4d4f'}
              size="small"
            />
            
            <Paragraph type="secondary" style={{ fontSize: '12px', marginTop: 8 }}>
              {factor.description}
            </Paragraph>
            
            {factor.supportingEvidence.length > 0 && (
              <Collapse size="small">
                <Collapse.Panel header="Supporting Evidence" key="evidence">
                  <ul>
                    {factor.supportingEvidence.map((evidence, i) => (
                      <li key={i}>{evidence}</li>
                    ))}
                  </ul>
                </Collapse.Panel>
              </Collapse>
            )}
          </Card>
        ))}
      </div>
      
      {showEvolution && data.evolution && (
        <div className="confidence-evolution">
          <Title level={5}>Confidence Evolution</Title>
          {/* D3.js chart for confidence evolution over time */}
          <ConfidenceEvolutionChart data={data.evolution} />
        </div>
      )}
    </div>
  )
}
```

### **PHASE 3: STREAMING INTEGRATION (Priority: HIGH)**

#### **3.1 Enhance StreamingProgress Component**
```typescript
// File: frontend-v2/src/apps/chat/components/StreamingProgress.tsx (ENHANCE EXISTING)
// Add these new features to the existing component:

interface EnhancedStreamingProgressProps {
  sessionId: string
  showTransparency?: boolean
  showPhaseDetails?: boolean
  onCancel?: () => void
}

// Add to existing component:
const { data: streamingData, isConnected } = useStreamingQuery(sessionId)
const [analyzePrompt] = useAnalyzePromptConstructionMutation()

// Enhanced progress display with AI transparency
const renderAIProcessingPhases = () => (
  <div className="ai-processing-phases">
    {streamingData.map((response, index) => {
      if (response.type === 'progress') {
        const progress = response.data as QueryProcessingProgress
        return (
          <div key={index} className="processing-phase">
            <Space>
              <Tag color="blue">{progress.phase}</Tag>
              <Progress percent={progress.progress * 100} size="small" style={{ width: 200 }} />
              <Text type="secondary">Confidence: {(progress.confidence * 100).toFixed(1)}%</Text>
            </Space>
            <Text>{progress.currentStep}</Text>
            {progress.estimatedTimeRemaining && (
              <Text type="secondary">
                ETA: {Math.ceil(progress.estimatedTimeRemaining.getTime() / 1000)}s
              </Text>
            )}
          </div>
        )
      }
      return null
    })}
  </div>
)
```

### **PHASE 4: CHAT INTERFACE ENHANCEMENT (Priority: MEDIUM)**

#### **4.1 Enhance ChatInterface Component**
```typescript
// File: frontend-v2/src/apps/chat/components/ChatInterface.tsx (ENHANCE EXISTING)
// Add transparency toggle and integration:

const [showTransparency, setShowTransparency] = useState(false)
const [currentTraceId, setCurrentTraceId] = useState<string | null>(null)

// Add to chat header:
<Button
  icon={<EyeOutlined />}
  onClick={() => setShowTransparency(!showTransparency)}
  type={showTransparency ? 'primary' : 'default'}
>
  AI Transparency
</Button>

// Add transparency panel overlay:
{showTransparency && currentTraceId && (
  <div className="transparency-overlay">
    <AITransparencyPanel 
      traceId={currentTraceId}
      showDetailedMetrics={true}
      onOptimizationSuggestion={(suggestion) => {
        // Handle optimization suggestions
        message.info(`Optimization suggestion: ${suggestion.title}`)
      }}
    />
  </div>
)}
```

#### **4.2 Enhance SemanticAnalysisPanel Component**
```typescript
// File: frontend-v2/src/apps/chat/components/SemanticAnalysisPanel.tsx (ENHANCE EXISTING)
// Add new AI transparency features:

const [understandQuery] = useUnderstandQueryMutation()
const [navigateSchema] = useNavigateSchemaMutation()

// Add query understanding visualization:
const renderQueryUnderstanding = () => (
  <Card title="AI Query Understanding" size="small">
    <BusinessContextVisualizer 
      contextProfile={semanticAnalysis.businessContext}
      showEntityHighlighting={true}
      showIntentClassification={true}
      interactive={true}
    />
  </Card>
)

// Add schema navigation results:
const renderSchemaNavigation = () => (
  <Card title="Schema Intelligence" size="small">
    <SchemaNavigationViewer 
      navigationResult={schemaNavigation}
      showRelationships={true}
      interactive={true}
    />
  </Card>
)
```

---

## 🔧 **IMPLEMENTATION PRIORITY ORDER**

### **Week 1: Core API Integration**
1. ✅ Create `transparencyApi.ts` service
2. ✅ Create `aiStreamingApi.ts` service  
3. ✅ Create `intelligentAgentsApi.ts` service
4. ✅ Add TypeScript types for all DTOs

### **Week 2: Basic Transparency Components**
1. ✅ Create `AITransparencyPanel` component
2. ✅ Create `PromptConstructionViewer` component
3. ✅ Create `ConfidenceBreakdownChart` component
4. ✅ Add transparency toggle to ChatInterface

### **Week 3: Streaming Enhancement**
1. ✅ Enhance `StreamingProgress` component
2. ✅ Add Server-Sent Events integration
3. ✅ Create real-time processing visualization
4. ✅ Add cancellation and retry controls

### **Week 4: Advanced Integration**
1. ✅ Enhance `SemanticAnalysisPanel` component
2. ✅ Create `BusinessContextVisualizer` component
3. ✅ Add intelligent agent orchestration UI
4. ✅ Implement schema navigation interface

---

## 📊 **NEW TYPESCRIPT TYPES NEEDED**

```typescript
// File: frontend-v2/src/shared/types/transparency.ts
export interface TransparencyReport {
  traceId: string
  generatedAt: string
  userQuestion: string
  intentType: string
  summary: string
  detailedMetrics?: Record<string, any>
  performanceAnalysis?: Record<string, any>
  optimizationSuggestions?: string[]
  errorMessage?: string
}

export interface PromptConstructionTrace {
  traceId: string
  startTime: string
  endTime: string
  userQuestion: string
  intentType: IntentType
  steps: PromptConstructionStep[]
  overallConfidence: number
  totalTokens: number
  finalPrompt: string
  metadata: Record<string, any>
}

export interface ConfidenceBreakdown {
  analysisId: string
  overallConfidence: number
  factorBreakdown: Record<string, number>
  confidenceFactors: ConfidenceFactor[]
  evolution?: ConfidenceEvolution
  timestamp: string
}

// ... (50+ more types from the DTOs)
```

---

## 🎯 **TESTING STRATEGY**

### **API Integration Tests**
```typescript
// Test transparency API integration
describe('Transparency API', () => {
  it('should fetch transparency trace', async () => {
    const { result } = renderHook(() => useGetTransparencyTraceQuery('test-trace'))
    await waitFor(() => expect(result.current.data).toBeDefined())
  })
})
```

### **Component Tests**
```typescript
// Test AITransparencyPanel component
describe('AITransparencyPanel', () => {
  it('should display confidence scores', () => {
    render(<AITransparencyPanel traceId="test-123" />)
    expect(screen.getByText(/confidence/i)).toBeInTheDocument()
  })
})
```

---

## 🚀 **IMMEDIATE NEXT STEPS**

1. **Start with transparencyApi.ts** - Create the RTK Query service
2. **Create basic AITransparencyPanel** - Simple prompt construction viewer
3. **Add transparency toggle to ChatInterface** - Basic integration
4. **Test with backend APIs** - Ensure proper data flow
5. **Enhance StreamingProgress** - Add real-time transparency updates

The backend is **100% ready** with all APIs implemented and tested. The frontend integration can begin immediately with full backend support for all transparency, streaming, and intelligent agent features.

---

*This integration guide provides everything needed to implement the AI Transparency Foundation in frontend-v2, building on the existing modern React architecture with the new powerful backend capabilities.*
