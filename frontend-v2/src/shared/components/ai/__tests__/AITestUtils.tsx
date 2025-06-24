import { render, RenderOptions } from '@testing-library/react'
import { ReactElement, ReactNode } from 'react'
import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router-dom'
import { ConfigProvider } from 'antd'
import { store } from '@shared/store'
import { AIIntegrationProvider } from '../AIIntegrationProvider'
import { AIErrorBoundary } from '../AIErrorBoundary'

// ============================================================================
// MOCK DATA GENERATORS
// ============================================================================

export const mockAgentCapabilities = () => ({
  agentId: 'test-agent-1',
  name: 'Test AI Agent',
  agentType: 'query_understanding' as const,
  version: '1.0.0',
  status: 'active' as const,
  supportedTaskTypes: ['query_analysis', 'schema_navigation'],
  configuration: {
    maxConcurrentTasks: 5,
    timeout: 30000,
    retryAttempts: 3,
    priority: 5
  },
  performance: {
    averageResponseTime: 1200,
    successRate: 0.95,
    throughput: 45,
    reliability: 0.98
  },
  lastUpdated: new Date().toISOString()
})

export const mockAgentHealthStatus = () => ({
  agentId: 'test-agent-1',
  agentType: 'query_understanding',
  status: 'healthy' as const,
  lastHeartbeat: new Date().toISOString(),
  uptime: 86400,
  currentLoad: 0.3,
  queueLength: 2,
  activeConnections: 5,
  errors: [],
  warnings: [],
  metadata: {}
})

export const mockTransparencyData = () => ({
  transparencyId: 'test-transparency-1',
  query: 'SELECT * FROM users WHERE age > 25',
  promptConstruction: {
    steps: [
      {
        step: 1,
        description: 'Parse SQL query',
        input: 'SELECT * FROM users WHERE age > 25',
        output: 'Parsed query structure',
        confidence: 0.95,
        reasoning: 'Standard SQL syntax detected'
      }
    ],
    finalPrompt: 'Analyze the following SQL query...',
    modelUsed: 'gpt-4',
    confidence: 0.92
  },
  confidenceBreakdown: {
    overall: 0.92,
    factors: [
      { factor: 'Query Syntax', score: 0.95, weight: 0.3 },
      { factor: 'Schema Match', score: 0.90, weight: 0.4 },
      { factor: 'Business Context', score: 0.88, weight: 0.3 }
    ]
  },
  decisionExplanation: {
    reasoning: 'Query appears to be a standard user filtering operation',
    alternatives: ['Use indexed column for better performance'],
    confidence: 0.92
  },
  timestamp: new Date().toISOString()
})

export const mockSchemaNavigation = () => ({
  schemas: [
    {
      name: 'public',
      tables: [
        {
          name: 'users',
          type: 'table' as const,
          description: 'User account information',
          businessPurpose: 'Store customer data',
          relevanceScore: 0.9,
          estimatedRowCount: 10000,
          columns: [
            {
              name: 'id',
              type: 'column' as const,
              description: 'Primary key',
              relevanceScore: 0.8,
              isPrimaryKey: true,
              isForeignKey: false
            },
            {
              name: 'email',
              type: 'column' as const,
              description: 'User email address',
              relevanceScore: 0.7,
              isPrimaryKey: false,
              isForeignKey: false
            }
          ],
          relationships: []
        }
      ]
    }
  ],
  relationships: [],
  confidence: 0.88
})

export const mockQueryOptimization = () => ({
  queryId: 'test-query-1',
  originalQuery: 'SELECT * FROM users WHERE age > 25',
  optimizationScore: 0.75,
  currentPerformance: {
    executionTime: 2500,
    complexityScore: 6,
    resourceUsage: 45
  },
  suggestions: [
    {
      type: 'index',
      title: 'Add Index on Age Column',
      description: 'Creating an index on the age column will improve query performance',
      reasoning: 'Age column is frequently used in WHERE clauses',
      confidence: 0.92,
      expectedImprovement: 0.6,
      costImpact: 0.1,
      complexityReduction: 0.3,
      impact: 'high' as const,
      optimizedQuery: 'CREATE INDEX idx_users_age ON users(age); SELECT * FROM users WHERE age > 25'
    }
  ],
  analysis: {
    bottlenecks: [
      { type: 'table_scan', description: 'Full table scan on users table' }
    ],
    insights: ['Consider adding an index on frequently queried columns']
  }
})

export const mockAutomatedInsights = () => ({
  insights: [
    {
      id: 'insight-1',
      title: 'High Query Volume Detected',
      description: 'Query volume has increased by 40% in the last hour',
      category: 'performance' as const,
      priority: 'high' as const,
      confidence: 0.88,
      actionable: true,
      impact: 'May cause performance degradation',
      recommendation: 'Consider scaling database resources',
      trending: true,
      generatedAt: new Date().toISOString()
    }
  ],
  trends: [
    { date: '2024-01-01', insightCount: 5, actionableCount: 3 },
    { date: '2024-01-02', insightCount: 7, actionableCount: 4 }
  ]
})

export const mockPredictiveAnalytics = () => ({
  forecast: {
    predictions: [
      { value: 100, confidence: 0.85, upperBound: 120, lowerBound: 80 },
      { value: 105, confidence: 0.83, upperBound: 125, lowerBound: 85 }
    ],
    confidence: 0.84,
    model: 'arima',
    generatedAt: new Date().toISOString()
  },
  historical: [
    { date: '2024-01-01', value: 95 },
    { date: '2024-01-02', value: 98 }
  ],
  metrics: {
    accuracy: 0.92,
    mape: 0.08,
    rmse: 5.2,
    trend: 'increasing' as const,
    seasonality: false,
    volatility: 0.15
  },
  trendAnalysis: {
    patterns: [
      {
        name: 'Upward Trend',
        type: 'increasing' as const,
        confidence: 0.89,
        description: 'Consistent upward trend detected',
        duration: '7 days',
        impact: 'positive'
      }
    ]
  },
  anomalies: [
    {
      title: 'Unusual Spike',
      description: 'Value exceeded normal range by 200%',
      severity: 'high' as const,
      detectedAt: new Date().toISOString(),
      confidence: 0.95
    }
  ]
})

// ============================================================================
// TEST UTILITIES
// ============================================================================

interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  withAIIntegration?: boolean
  withErrorBoundary?: boolean
  withRouter?: boolean
  initialAIState?: any
}

const AllTheProviders = ({ 
  children, 
  withAIIntegration = true, 
  withErrorBoundary = true,
  withRouter = true 
}: { 
  children: ReactNode
  withAIIntegration?: boolean
  withErrorBoundary?: boolean
  withRouter?: boolean
}) => {
  let component = (
    <Provider store={store}>
      <ConfigProvider>
        {children}
      </ConfigProvider>
    </Provider>
  )

  if (withRouter) {
    component = <BrowserRouter>{component}</BrowserRouter>
  }

  if (withAIIntegration) {
    component = (
      <AIIntegrationProvider enableErrorNotifications={false}>
        {component}
      </AIIntegrationProvider>
    )
  }

  if (withErrorBoundary) {
    component = (
      <AIErrorBoundary componentName="TestComponent" showErrorDetails={true}>
        {component}
      </AIErrorBoundary>
    )
  }

  return component
}

export const renderWithProviders = (
  ui: ReactElement,
  options: CustomRenderOptions = {}
) => {
  const {
    withAIIntegration = true,
    withErrorBoundary = true,
    withRouter = true,
    ...renderOptions
  } = options

  return render(ui, {
    wrapper: ({ children }) => (
      <AllTheProviders
        withAIIntegration={withAIIntegration}
        withErrorBoundary={withErrorBoundary}
        withRouter={withRouter}
      >
        {children}
      </AllTheProviders>
    ),
    ...renderOptions
  })
}

// ============================================================================
// MOCK API RESPONSES
// ============================================================================

export const mockApiResponses = {
  '/intelligentagents/capabilities': mockAgentCapabilities(),
  '/intelligentagents/health': [mockAgentHealthStatus()],
  '/transparency/settings': { enabled: true, detailLevel: 'detailed' },
  '/transparency/analysis': mockTransparencyData(),
  '/intelligentagents/schema/navigation': mockSchemaNavigation(),
  '/intelligentagents/query/optimization': mockQueryOptimization(),
  '/intelligentagents/insights/automated': mockAutomatedInsights(),
  '/intelligentagents/analytics/predictive': mockPredictiveAnalytics()
}

// ============================================================================
// TEST HELPERS
// ============================================================================

export const waitForAIComponent = async (timeout = 5000) => {
  return new Promise(resolve => setTimeout(resolve, timeout))
}

export const mockConsoleError = () => {
  const originalError = console.error
  const mockError = jest.fn()
  console.error = mockError
  
  return {
    mockError,
    restore: () => {
      console.error = originalError
    }
  }
}

export const mockPerformanceNow = () => {
  const originalNow = performance.now
  let mockTime = 0
  
  performance.now = jest.fn(() => mockTime)
  
  return {
    setTime: (time: number) => { mockTime = time },
    advance: (ms: number) => { mockTime += ms },
    restore: () => {
      performance.now = originalNow
    }
  }
}

export const createMockIntersectionObserver = () => {
  const mockIntersectionObserver = jest.fn()
  mockIntersectionObserver.mockReturnValue({
    observe: () => null,
    unobserve: () => null,
    disconnect: () => null
  })
  
  Object.defineProperty(window, 'IntersectionObserver', {
    writable: true,
    configurable: true,
    value: mockIntersectionObserver
  })
  
  return mockIntersectionObserver
}

export const createMockResizeObserver = () => {
  const mockResizeObserver = jest.fn()
  mockResizeObserver.mockReturnValue({
    observe: () => null,
    unobserve: () => null,
    disconnect: () => null
  })
  
  Object.defineProperty(window, 'ResizeObserver', {
    writable: true,
    configurable: true,
    value: mockResizeObserver
  })
  
  return mockResizeObserver
}

// ============================================================================
// ASSERTION HELPERS
// ============================================================================

export const expectAIComponentToRender = (container: HTMLElement, testId: string) => {
  const component = container.querySelector(`[data-testid="${testId}"]`)
  expect(component).toBeInTheDocument()
  return component
}

export const expectConfidenceIndicator = (container: HTMLElement, expectedConfidence?: number) => {
  const indicator = container.querySelector('[data-testid*="confidence-indicator"]')
  expect(indicator).toBeInTheDocument()
  
  if (expectedConfidence !== undefined) {
    expect(indicator).toHaveTextContent(Math.round(expectedConfidence * 100).toString())
  }
  
  return indicator
}

export const expectLoadingState = (container: HTMLElement) => {
  const loading = container.querySelector('.ant-spin') || 
                 container.querySelector('[data-testid*="loading"]')
  expect(loading).toBeInTheDocument()
  return loading
}

export const expectErrorState = (container: HTMLElement, errorMessage?: string) => {
  const error = container.querySelector('.ant-result-error') ||
               container.querySelector('[data-testid*="error"]')
  expect(error).toBeInTheDocument()
  
  if (errorMessage) {
    expect(error).toHaveTextContent(errorMessage)
  }
  
  return error
}

export default {
  renderWithProviders,
  mockApiResponses,
  mockAgentCapabilities,
  mockAgentHealthStatus,
  mockTransparencyData,
  mockSchemaNavigation,
  mockQueryOptimization,
  mockAutomatedInsights,
  mockPredictiveAnalytics,
  waitForAIComponent,
  mockConsoleError,
  mockPerformanceNow,
  createMockIntersectionObserver,
  createMockResizeObserver,
  expectAIComponentToRender,
  expectConfidenceIndicator,
  expectLoadingState,
  expectErrorState
}
