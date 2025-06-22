import React from 'react'
import { screen, fireEvent, waitFor } from '@testing-library/react'
import { renderWithProviders, mockAgentCapabilities, mockAgentHealthStatus } from './AITestUtils'
import { AIIntegrationProvider, useAIIntegration, useAIFeatureGate, useAIPerformanceMonitor } from '../AIIntegrationProvider'

// Mock the API hooks
jest.mock('@shared/store/api/intelligentAgentsApi', () => ({
  useGetAgentCapabilitiesQuery: jest.fn(),
  useGetAgentHealthStatusQuery: jest.fn()
}))

jest.mock('@shared/store/api/transparencyApi', () => ({
  useGetTransparencySettingsQuery: jest.fn()
}))

import { useGetAgentCapabilitiesQuery, useGetAgentHealthStatusQuery } from '@shared/store/api/intelligentAgentsApi'
import { useGetTransparencySettingsQuery } from '@shared/store/api/transparencyApi'

const mockUseGetAgentCapabilitiesQuery = useGetAgentCapabilitiesQuery as jest.MockedFunction<typeof useGetAgentCapabilitiesQuery>
const mockUseGetAgentHealthStatusQuery = useGetAgentHealthStatusQuery as jest.MockedFunction<typeof useGetAgentHealthStatusQuery>
const mockUseGetTransparencySettingsQuery = useGetTransparencySettingsQuery as jest.MockedFunction<typeof useGetTransparencySettingsQuery>

// Test component that uses the AI integration context
const TestComponent: React.FC = () => {
  const { 
    state, 
    enableFeature, 
    disableFeature, 
    reportError, 
    updatePerformance, 
    isFeatureEnabled,
    isLoading,
    hasErrors
  } = useAIIntegration()

  return (
    <div data-testid="test-component">
      <div data-testid="ai-enabled">{state.isAIEnabled.toString()}</div>
      <div data-testid="current-provider">{state.currentProvider || 'none'}</div>
      <div data-testid="confidence-threshold">{state.globalConfidenceThreshold}</div>
      <div data-testid="transparency-enabled">{state.features.transparency.toString()}</div>
      <div data-testid="is-loading">{isLoading.toString()}</div>
      <div data-testid="has-errors">{hasErrors.toString()}</div>
      <div data-testid="error-count">{state.errors.length}</div>
      
      <button 
        data-testid="enable-transparency" 
        onClick={() => enableFeature('transparency')}
      >
        Enable Transparency
      </button>
      
      <button 
        data-testid="disable-transparency" 
        onClick={() => disableFeature('transparency')}
      >
        Disable Transparency
      </button>
      
      <button 
        data-testid="report-error" 
        onClick={() => reportError('TestComponent', 'Test error message', 'high')}
      >
        Report Error
      </button>
      
      <button 
        data-testid="update-performance" 
        onClick={() => updatePerformance({ responseTime: 1500, successRate: 0.95 })}
      >
        Update Performance
      </button>
    </div>
  )
}

// Test component for feature gating
const FeatureGateTestComponent: React.FC = () => {
  const { isEnabled, isAIEnabled, confidenceThreshold } = useAIFeatureGate('transparency')
  
  return (
    <div data-testid="feature-gate-test">
      <div data-testid="feature-enabled">{isEnabled.toString()}</div>
      <div data-testid="ai-enabled">{isAIEnabled.toString()}</div>
      <div data-testid="confidence-threshold">{confidenceThreshold}</div>
    </div>
  )
}

// Test component for performance monitoring
const PerformanceMonitorTestComponent: React.FC = () => {
  const { trackPerformance } = useAIPerformanceMonitor('TestComponent')
  const [result, setResult] = React.useState<string>('')
  
  const handleTrackPerformance = async () => {
    try {
      const result = await trackPerformance(async () => {
        await new Promise(resolve => setTimeout(resolve, 100))
        return 'success'
      })
      setResult(result)
    } catch (error) {
      setResult('error')
    }
  }
  
  return (
    <div data-testid="performance-monitor-test">
      <div data-testid="result">{result}</div>
      <button data-testid="track-performance" onClick={handleTrackPerformance}>
        Track Performance
      </button>
    </div>
  )
}

describe('AIIntegrationProvider', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    
    // Default mock implementations
    mockUseGetAgentCapabilitiesQuery.mockReturnValue({
      data: [mockAgentCapabilities()],
      isLoading: false,
      error: null
    } as any)
    
    mockUseGetAgentHealthStatusQuery.mockReturnValue({
      data: [mockAgentHealthStatus()],
      isLoading: false,
      error: null
    } as any)
    
    mockUseGetTransparencySettingsQuery.mockReturnValue({
      data: { enabled: true, detailLevel: 'detailed' },
      isLoading: false,
      error: null
    } as any)
  })

  describe('Basic Functionality', () => {
    it('should render children and provide context', () => {
      renderWithProviders(
        <AIIntegrationProvider>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      expect(screen.getByTestId('test-component')).toBeInTheDocument()
      expect(screen.getByTestId('ai-enabled')).toHaveTextContent('true')
      expect(screen.getByTestId('transparency-enabled')).toHaveTextContent('true')
    })

    it('should throw error when used outside provider', () => {
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {})
      
      expect(() => {
        renderWithProviders(<TestComponent />, { withAIIntegration: false, withErrorBoundary: false })
      }).toThrow('useAIIntegration must be used within an AIIntegrationProvider')
      
      consoleSpy.mockRestore()
    })
  })

  describe('Feature Management', () => {
    it('should enable and disable features', async () => {
      renderWithProviders(
        <AIIntegrationProvider>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      // Initially enabled
      expect(screen.getByTestId('transparency-enabled')).toHaveTextContent('true')

      // Disable feature
      fireEvent.click(screen.getByTestId('disable-transparency'))
      await waitFor(() => {
        expect(screen.getByTestId('transparency-enabled')).toHaveTextContent('false')
      })

      // Enable feature
      fireEvent.click(screen.getByTestId('enable-transparency'))
      await waitFor(() => {
        expect(screen.getByTestId('transparency-enabled')).toHaveTextContent('true')
      })
    })

    it('should update feature flags based on API data', async () => {
      mockUseGetTransparencySettingsQuery.mockReturnValue({
        data: { enabled: false, detailLevel: 'basic' },
        isLoading: false,
        error: null
      } as any)

      renderWithProviders(
        <AIIntegrationProvider>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      await waitFor(() => {
        expect(screen.getByTestId('transparency-enabled')).toHaveTextContent('false')
      })
    })
  })

  describe('Error Handling', () => {
    it('should report and track errors', async () => {
      renderWithProviders(
        <AIIntegrationProvider enableErrorNotifications={false}>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      expect(screen.getByTestId('error-count')).toHaveTextContent('0')

      fireEvent.click(screen.getByTestId('report-error'))

      await waitFor(() => {
        expect(screen.getByTestId('error-count')).toHaveTextContent('1')
      })
    })

    it('should handle API errors', async () => {
      mockUseGetAgentCapabilitiesQuery.mockReturnValue({
        data: null,
        isLoading: false,
        error: new Error('API Error')
      } as any)

      renderWithProviders(
        <AIIntegrationProvider>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      await waitFor(() => {
        expect(screen.getByTestId('has-errors')).toHaveTextContent('true')
      })
    })
  })

  describe('Performance Monitoring', () => {
    it('should update performance metrics', async () => {
      renderWithProviders(
        <AIIntegrationProvider enablePerformanceMonitoring={true}>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      fireEvent.click(screen.getByTestId('update-performance'))

      // Performance updates are internal, but we can verify the function doesn't throw
      await waitFor(() => {
        expect(screen.getByTestId('test-component')).toBeInTheDocument()
      })
    })

    it('should set current provider based on performance', async () => {
      const mockCapabilities = mockAgentCapabilities()
      mockUseGetAgentCapabilitiesQuery.mockReturnValue({
        data: [mockCapabilities],
        isLoading: false,
        error: null
      } as any)

      renderWithProviders(
        <AIIntegrationProvider enablePerformanceMonitoring={true}>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      await waitFor(() => {
        expect(screen.getByTestId('current-provider')).toHaveTextContent(mockCapabilities.agentId)
      })
    })
  })

  describe('Loading States', () => {
    it('should show loading state when APIs are loading', () => {
      mockUseGetAgentCapabilitiesQuery.mockReturnValue({
        data: null,
        isLoading: true,
        error: null
      } as any)

      renderWithProviders(
        <AIIntegrationProvider>
          <TestComponent />
        </AIIntegrationProvider>,
        { withAIIntegration: false }
      )

      expect(screen.getByTestId('is-loading')).toHaveTextContent('true')
    })
  })
})

describe('useAIFeatureGate', () => {
  it('should return correct feature gate status', () => {
    renderWithProviders(
      <AIIntegrationProvider>
        <FeatureGateTestComponent />
      </AIIntegrationProvider>,
      { withAIIntegration: false }
    )

    expect(screen.getByTestId('feature-enabled')).toHaveTextContent('true')
    expect(screen.getByTestId('ai-enabled')).toHaveTextContent('true')
    expect(screen.getByTestId('confidence-threshold')).toHaveTextContent('0.7')
  })
})

describe('useAIPerformanceMonitor', () => {
  it('should track performance of operations', async () => {
    renderWithProviders(
      <AIIntegrationProvider>
        <PerformanceMonitorTestComponent />
      </AIIntegrationProvider>,
      { withAIIntegration: false }
    )

    fireEvent.click(screen.getByTestId('track-performance'))

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('success')
    }, { timeout: 1000 })
  })

  it('should handle errors in tracked operations', async () => {
    const PerformanceErrorTestComponent: React.FC = () => {
      const { trackPerformance } = useAIPerformanceMonitor('TestComponent')
      const [result, setResult] = React.useState<string>('')
      
      const handleTrackError = async () => {
        try {
          await trackPerformance(async () => {
            throw new Error('Test error')
          })
        } catch (error) {
          setResult('error caught')
        }
      }
      
      return (
        <div data-testid="performance-error-test">
          <div data-testid="result">{result}</div>
          <button data-testid="track-error" onClick={handleTrackError}>
            Track Error
          </button>
        </div>
      )
    }

    renderWithProviders(
      <AIIntegrationProvider>
        <PerformanceErrorTestComponent />
      </AIIntegrationProvider>,
      { withAIIntegration: false }
    )

    fireEvent.click(screen.getByTestId('track-error'))

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('error caught')
    })
  })
})
