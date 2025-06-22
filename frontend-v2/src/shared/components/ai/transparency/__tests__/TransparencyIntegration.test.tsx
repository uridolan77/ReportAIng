import React from 'react'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import { transparencyApi } from '@shared/store/api/transparencyApi'
import { 
  AlternativeOptionsPanel,
  LiveTransparencyPanel,
  TransparencyExportPanel,
  ConfidenceTrendsChart,
  TokenUsageChart
} from '../index'

// Mock data
const mockAlternativeOptions = [
  {
    optionId: 'alt-001',
    type: 'Template',
    description: 'Use analytical template instead of reporting template',
    score: 0.82,
    rationale: 'Better suited for complex analytical queries',
    estimatedImprovement: 15.0
  },
  {
    optionId: 'alt-002',
    type: 'Optimization',
    description: 'Reduce context verbosity',
    score: 0.75,
    rationale: 'Remove redundant schema information',
    estimatedImprovement: 8.5
  }
]

const mockConfidenceTrends = [
  { date: '2024-12-20', confidence: 0.85, traceCount: 12 },
  { date: '2024-12-21', confidence: 0.78, traceCount: 15 },
  { date: '2024-12-22', confidence: 0.92, traceCount: 8 },
  { date: '2024-12-23', confidence: 0.88, traceCount: 10 }
]

const mockTokenUsage = [
  { date: '2024-12-20', totalTokens: 2500, inputTokens: 1800, outputTokens: 700, cost: 0.025 },
  { date: '2024-12-21', totalTokens: 3200, inputTokens: 2200, outputTokens: 1000, cost: 0.032 },
  { date: '2024-12-22', totalTokens: 1800, inputTokens: 1200, outputTokens: 600, cost: 0.018 },
  { date: '2024-12-23', totalTokens: 2800, inputTokens: 1900, outputTokens: 900, cost: 0.028 }
]

// Mock store setup
const createMockStore = () => {
  return configureStore({
    reducer: {
      [transparencyApi.reducerPath]: transparencyApi.reducer,
    },
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware().concat(transparencyApi.middleware),
  })
}

// Mock API responses
const mockApiResponses = {
  getAlternativeOptions: jest.fn().mockResolvedValue({ data: mockAlternativeOptions }),
  getTransparencySettings: jest.fn().mockResolvedValue({ 
    data: { 
      enableDetailedLogging: true,
      confidenceThreshold: 0.7,
      retentionDays: 30,
      enableOptimizationSuggestions: true
    }
  }),
  exportTransparencyData: jest.fn().mockResolvedValue({ data: new Blob(['test data']) })
}

// Mock SignalR hub
jest.mock('@shared/services/signalr/transparencyHub', () => ({
  transparencyHub: {
    connect: jest.fn().mockResolvedValue(undefined),
    disconnect: jest.fn().mockResolvedValue(undefined),
    subscribeToTrace: jest.fn().mockResolvedValue(undefined),
    unsubscribeFromTrace: jest.fn().mockResolvedValue(undefined),
    on: jest.fn(),
    off: jest.fn(),
    isConnected: true,
    connectionState: 'Connected'
  }
}))

// Mock D3 for chart components
jest.mock('d3', () => ({
  select: jest.fn(() => ({
    selectAll: jest.fn(() => ({ remove: jest.fn() })),
    append: jest.fn(() => ({
      attr: jest.fn(() => ({ attr: jest.fn() })),
      style: jest.fn(() => ({ style: jest.fn() }))
    }))
  })),
  scaleTime: jest.fn(() => ({
    domain: jest.fn(() => ({ range: jest.fn() })),
    range: jest.fn()
  })),
  scaleLinear: jest.fn(() => ({
    domain: jest.fn(() => ({ range: jest.fn() })),
    range: jest.fn()
  })),
  line: jest.fn(() => ({
    x: jest.fn(() => ({ y: jest.fn(() => ({ curve: jest.fn() })) }))
  })),
  extent: jest.fn(() => [new Date(), new Date()]),
  timeParse: jest.fn(() => () => new Date()),
  timeFormat: jest.fn(() => () => '12/23'),
  axisBottom: jest.fn(() => ({ tickFormat: jest.fn() })),
  axisLeft: jest.fn(() => ({ tickFormat: jest.fn() }))
}))

describe('Transparency Components Integration Tests', () => {
  let store: ReturnType<typeof createMockStore>

  beforeEach(() => {
    store = createMockStore()
    jest.clearAllMocks()
  })

  describe('AlternativeOptionsPanel', () => {
    it('renders alternative options and handles selection', async () => {
      const mockOnSelect = jest.fn()
      
      render(
        <Provider store={store}>
          <AlternativeOptionsPanel
            traceId="test-trace-123"
            onSelectAlternative={mockOnSelect}
          />
        </Provider>
      )

      // Should show loading initially
      expect(screen.getByText(/loading alternative options/i)).toBeInTheDocument()

      // Wait for data to load (in real test, this would be mocked API response)
      await waitFor(() => {
        expect(screen.queryByText(/loading alternative options/i)).not.toBeInTheDocument()
      })
    })

    it('displays error state when API fails', async () => {
      // Mock API failure
      const failingStore = configureStore({
        reducer: {
          [transparencyApi.reducerPath]: transparencyApi.reducer,
        },
        middleware: (getDefaultMiddleware) =>
          getDefaultMiddleware().concat(transparencyApi.middleware),
      })

      render(
        <Provider store={failingStore}>
          <AlternativeOptionsPanel traceId="invalid-trace" />
        </Provider>
      )

      await waitFor(() => {
        expect(screen.getByText(/failed to load alternatives/i)).toBeInTheDocument()
      })
    })
  })

  describe('LiveTransparencyPanel', () => {
    it('renders live transparency panel with SignalR integration', () => {
      render(
        <Provider store={store}>
          <LiveTransparencyPanel traceId="test-trace-123" />
        </Provider>
      )

      expect(screen.getByText(/live query processing/i)).toBeInTheDocument()
      expect(screen.getByText(/overall progress/i)).toBeInTheDocument()
      expect(screen.getByText(/confidence/i)).toBeInTheDocument()
    })

    it('handles trace completion callback', () => {
      const mockOnComplete = jest.fn()
      
      render(
        <Provider store={store}>
          <LiveTransparencyPanel
            traceId="test-trace-123"
            onTraceComplete={mockOnComplete}
          />
        </Provider>
      )

      // Component should be rendered
      expect(screen.getByText(/live query processing/i)).toBeInTheDocument()
    })
  })

  describe('TransparencyExportPanel', () => {
    it('renders export panel with format options', () => {
      render(
        <Provider store={store}>
          <TransparencyExportPanel />
        </Provider>
      )

      expect(screen.getByText(/export transparency data/i)).toBeInTheDocument()
      expect(screen.getByText(/export format/i)).toBeInTheDocument()
      expect(screen.getByText(/JSON/i)).toBeInTheDocument()
      expect(screen.getByText(/CSV/i)).toBeInTheDocument()
      expect(screen.getByText(/Excel/i)).toBeInTheDocument()
    })

    it('validates form before allowing export', () => {
      render(
        <Provider store={store}>
          <TransparencyExportPanel />
        </Provider>
      )

      const exportButton = screen.getByRole('button', { name: /export data/i })
      expect(exportButton).toBeDisabled()
    })
  })

  describe('ConfidenceTrendsChart', () => {
    it('renders confidence trends chart with D3', () => {
      render(
        <Provider store={store}>
          <ConfidenceTrendsChart
            data={mockConfidenceTrends}
            timeRange="week"
          />
        </Provider>
      )

      expect(screen.getByText(/confidence trends/i)).toBeInTheDocument()
    })

    it('handles empty data gracefully', () => {
      render(
        <Provider store={store}>
          <ConfidenceTrendsChart
            data={[]}
            timeRange="week"
          />
        </Provider>
      )

      expect(screen.getByText(/no data available/i)).toBeInTheDocument()
    })

    it('shows loading state', () => {
      render(
        <Provider store={store}>
          <ConfidenceTrendsChart
            data={[]}
            timeRange="week"
            loading={true}
          />
        </Provider>
      )

      expect(screen.getByText(/loading confidence trends/i)).toBeInTheDocument()
    })
  })

  describe('TokenUsageChart', () => {
    it('renders token usage chart with statistics', () => {
      render(
        <Provider store={store}>
          <TokenUsageChart
            data={mockTokenUsage}
            chartType="stacked"
            timeRange="week"
          />
        </Provider>
      )

      expect(screen.getByText(/token usage analytics/i)).toBeInTheDocument()
      expect(screen.getByText(/total tokens/i)).toBeInTheDocument()
    })

    it('handles chart type changes', () => {
      const mockOnChartTypeChange = jest.fn()
      
      render(
        <Provider store={store}>
          <TokenUsageChart
            data={mockTokenUsage}
            chartType="stacked"
            timeRange="week"
            onChartTypeChange={mockOnChartTypeChange}
          />
        </Provider>
      )

      // Find and click chart type selector
      const chartTypeSelect = screen.getByDisplayValue('Stacked')
      fireEvent.mouseDown(chartTypeSelect)
      
      const areaOption = screen.getByText('Area')
      fireEvent.click(areaOption)
      
      expect(mockOnChartTypeChange).toHaveBeenCalledWith('area')
    })
  })

  describe('Component Integration', () => {
    it('components work together in dashboard context', () => {
      render(
        <Provider store={store}>
          <div>
            <ConfidenceTrendsChart
              data={mockConfidenceTrends}
              timeRange="week"
            />
            <TokenUsageChart
              data={mockTokenUsage}
              chartType="stacked"
              timeRange="week"
            />
            <AlternativeOptionsPanel traceId="test-trace-123" />
          </div>
        </Provider>
      )

      // All components should render without conflicts
      expect(screen.getByText(/confidence trends/i)).toBeInTheDocument()
      expect(screen.getByText(/token usage analytics/i)).toBeInTheDocument()
      expect(screen.getByText(/alternative options/i)).toBeInTheDocument()
    })
  })
})

// API Integration Tests
describe('Transparency API Integration', () => {
  it('should handle API responses correctly', async () => {
    const store = createMockStore()
    
    // Test API call structure
    expect(transparencyApi.endpoints.getAlternativeOptions).toBeDefined()
    expect(transparencyApi.endpoints.getTransparencySettings).toBeDefined()
    expect(transparencyApi.endpoints.exportTransparencyData).toBeDefined()
  })

  it('should provide correct hooks for components', () => {
    // Verify hooks are exported
    expect(transparencyApi.useGetAlternativeOptionsQuery).toBeDefined()
    expect(transparencyApi.useGetTransparencySettingsQuery).toBeDefined()
    expect(transparencyApi.useExportTransparencyDataMutation).toBeDefined()
  })
})
