import React, { createContext, useContext, useReducer, useEffect, ReactNode } from 'react'
import { notification } from 'antd'
import { useGetAgentCapabilitiesQuery, useGetAgentHealthStatusQuery } from '@shared/store/api/intelligentAgentsApi'
import { useGetTransparencySettingsQuery } from '@shared/store/api/transparencyApi'

// ============================================================================
// AI INTEGRATION CONTEXT AND STATE MANAGEMENT
// ============================================================================

interface AIIntegrationState {
  // Global AI state
  isAIEnabled: boolean
  currentProvider: string | null
  globalConfidenceThreshold: number
  
  // Feature flags
  features: {
    transparency: boolean
    streaming: boolean
    intelligence: boolean
    management: boolean
    insights: boolean
    predictive: boolean
  }
  
  // Performance monitoring
  performance: {
    responseTime: number
    successRate: number
    errorCount: number
    lastUpdate: string
  }
  
  // Error handling
  errors: Array<{
    id: string
    component: string
    message: string
    timestamp: string
    severity: 'low' | 'medium' | 'high' | 'critical'
  }>
  
  // User preferences
  preferences: {
    autoRefresh: boolean
    refreshInterval: number
    showConfidence: boolean
    showDebugInfo: boolean
    theme: 'light' | 'dark' | 'auto'
  }
}

type AIIntegrationAction =
  | { type: 'SET_AI_ENABLED'; payload: boolean }
  | { type: 'SET_PROVIDER'; payload: string }
  | { type: 'SET_CONFIDENCE_THRESHOLD'; payload: number }
  | { type: 'TOGGLE_FEATURE'; payload: { feature: keyof AIIntegrationState['features']; enabled: boolean } }
  | { type: 'UPDATE_PERFORMANCE'; payload: Partial<AIIntegrationState['performance']> }
  | { type: 'ADD_ERROR'; payload: Omit<AIIntegrationState['errors'][0], 'id' | 'timestamp'> }
  | { type: 'CLEAR_ERRORS' }
  | { type: 'UPDATE_PREFERENCES'; payload: Partial<AIIntegrationState['preferences']> }
  | { type: 'RESET_STATE' }

const initialState: AIIntegrationState = {
  isAIEnabled: true,
  currentProvider: null,
  globalConfidenceThreshold: 0.7,
  features: {
    transparency: true,
    streaming: true,
    intelligence: true,
    management: true,
    insights: true,
    predictive: true
  },
  performance: {
    responseTime: 0,
    successRate: 1,
    errorCount: 0,
    lastUpdate: new Date().toISOString()
  },
  errors: [],
  preferences: {
    autoRefresh: true,
    refreshInterval: 30000,
    showConfidence: true,
    showDebugInfo: false,
    theme: 'auto'
  }
}

function aiIntegrationReducer(state: AIIntegrationState, action: AIIntegrationAction): AIIntegrationState {
  switch (action.type) {
    case 'SET_AI_ENABLED':
      return { ...state, isAIEnabled: action.payload }
    
    case 'SET_PROVIDER':
      return { ...state, currentProvider: action.payload }
    
    case 'SET_CONFIDENCE_THRESHOLD':
      return { ...state, globalConfidenceThreshold: action.payload }
    
    case 'TOGGLE_FEATURE':
      return {
        ...state,
        features: {
          ...state.features,
          [action.payload.feature]: action.payload.enabled
        }
      }
    
    case 'UPDATE_PERFORMANCE':
      return {
        ...state,
        performance: {
          ...state.performance,
          ...action.payload,
          lastUpdate: new Date().toISOString()
        }
      }
    
    case 'ADD_ERROR':
      const newError = {
        ...action.payload,
        id: `error-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        timestamp: new Date().toISOString()
      }
      return {
        ...state,
        errors: [newError, ...state.errors.slice(0, 49)] // Keep last 50 errors
      }
    
    case 'CLEAR_ERRORS':
      return { ...state, errors: [] }
    
    case 'UPDATE_PREFERENCES':
      return {
        ...state,
        preferences: { ...state.preferences, ...action.payload }
      }
    
    case 'RESET_STATE':
      return initialState
    
    default:
      return state
  }
}

// ============================================================================
// CONTEXT DEFINITION
// ============================================================================

interface AIIntegrationContextType {
  state: AIIntegrationState
  dispatch: React.Dispatch<AIIntegrationAction>
  
  // Convenience methods
  enableFeature: (feature: keyof AIIntegrationState['features']) => void
  disableFeature: (feature: keyof AIIntegrationState['features']) => void
  reportError: (component: string, message: string, severity?: 'low' | 'medium' | 'high' | 'critical') => void
  updatePerformance: (metrics: Partial<AIIntegrationState['performance']>) => void
  isFeatureEnabled: (feature: keyof AIIntegrationState['features']) => boolean
  
  // API status
  isLoading: boolean
  hasErrors: boolean
}

const AIIntegrationContext = createContext<AIIntegrationContextType | undefined>(undefined)

// ============================================================================
// PROVIDER COMPONENT
// ============================================================================

interface AIIntegrationProviderProps {
  children: ReactNode
  enableErrorNotifications?: boolean
  enablePerformanceMonitoring?: boolean
}

export const AIIntegrationProvider: React.FC<AIIntegrationProviderProps> = ({
  children,
  enableErrorNotifications = true,
  enablePerformanceMonitoring = true
}) => {
  const [state, dispatch] = useReducer(aiIntegrationReducer, initialState)
  
  // Real API data for monitoring
  const { data: capabilities, isLoading: capabilitiesLoading, error: capabilitiesError } = useGetAgentCapabilitiesQuery()
  const { data: healthStatus, isLoading: healthLoading, error: healthError } = useGetAgentHealthStatusQuery()
  const { data: transparencySettings, isLoading: transparencyLoading, error: transparencyError } = useGetTransparencySettingsQuery()
  
  const isLoading = capabilitiesLoading || healthLoading || transparencyLoading
  const hasErrors = !!(capabilitiesError || healthError || transparencyError)
  
  // Convenience methods
  const enableFeature = (feature: keyof AIIntegrationState['features']) => {
    dispatch({ type: 'TOGGLE_FEATURE', payload: { feature, enabled: true } })
  }
  
  const disableFeature = (feature: keyof AIIntegrationState['features']) => {
    dispatch({ type: 'TOGGLE_FEATURE', payload: { feature, enabled: false } })
  }
  
  const reportError = (component: string, message: string, severity: 'low' | 'medium' | 'high' | 'critical' = 'medium') => {
    dispatch({ type: 'ADD_ERROR', payload: { component, message, severity } })
    
    if (enableErrorNotifications) {
      const notificationType = severity === 'critical' ? 'error' : severity === 'high' ? 'warning' : 'info'
      notification[notificationType]({
        message: `AI Component Error`,
        description: `${component}: ${message}`,
        duration: severity === 'critical' ? 0 : 4.5
      })
    }
  }
  
  const updatePerformance = (metrics: Partial<AIIntegrationState['performance']>) => {
    dispatch({ type: 'UPDATE_PERFORMANCE', payload: metrics })
  }
  
  const isFeatureEnabled = (feature: keyof AIIntegrationState['features']) => {
    return state.isAIEnabled && state.features[feature]
  }
  
  // Monitor API health and update performance metrics
  useEffect(() => {
    if (enablePerformanceMonitoring && capabilities && healthStatus) {
      const activeAgents = capabilities.filter(agent => agent.status === 'active')
      const healthyAgents = healthStatus.filter(status => status.status === 'healthy')
      
      const avgResponseTime = capabilities.reduce((acc, agent) => 
        acc + agent.performance.averageResponseTime, 0) / capabilities.length
      
      const successRate = capabilities.reduce((acc, agent) => 
        acc + agent.performance.successRate, 0) / capabilities.length
      
      updatePerformance({
        responseTime: avgResponseTime,
        successRate: successRate,
        errorCount: healthStatus.reduce((acc, status) => acc + status.errors.length, 0)
      })
      
      // Set current provider to the best performing one
      if (activeAgents.length > 0) {
        const bestProvider = activeAgents.reduce((best, current) => 
          current.performance.successRate > best.performance.successRate ? current : best
        )
        dispatch({ type: 'SET_PROVIDER', payload: bestProvider.agentId })
      }
    }
  }, [capabilities, healthStatus, enablePerformanceMonitoring])
  
  // Handle API errors
  useEffect(() => {
    if (capabilitiesError) {
      reportError('AgentCapabilities', 'Failed to load agent capabilities', 'high')
    }
    if (healthError) {
      reportError('AgentHealth', 'Failed to load agent health status', 'high')
    }
    if (transparencyError) {
      reportError('Transparency', 'Failed to load transparency settings', 'medium')
    }
  }, [capabilitiesError, healthError, transparencyError])
  
  // Update feature flags based on API data
  useEffect(() => {
    if (transparencySettings) {
      dispatch({ 
        type: 'TOGGLE_FEATURE', 
        payload: { feature: 'transparency', enabled: transparencySettings.enabled } 
      })
    }
  }, [transparencySettings])
  
  const contextValue: AIIntegrationContextType = {
    state,
    dispatch,
    enableFeature,
    disableFeature,
    reportError,
    updatePerformance,
    isFeatureEnabled,
    isLoading,
    hasErrors
  }
  
  return (
    <AIIntegrationContext.Provider value={contextValue}>
      {children}
    </AIIntegrationContext.Provider>
  )
}

// ============================================================================
// CUSTOM HOOK
// ============================================================================

export const useAIIntegration = () => {
  const context = useContext(AIIntegrationContext)
  if (context === undefined) {
    throw new Error('useAIIntegration must be used within an AIIntegrationProvider')
  }
  return context
}

// ============================================================================
// UTILITY HOOKS
// ============================================================================

/**
 * Hook for monitoring AI component performance
 */
export const useAIPerformanceMonitor = (componentName: string) => {
  const { reportError, updatePerformance } = useAIIntegration()
  
  const trackPerformance = async <T,>(operation: () => Promise<T>): Promise<T> => {
    const startTime = performance.now()
    try {
      const result = await operation()
      const endTime = performance.now()
      const responseTime = endTime - startTime
      
      updatePerformance({ responseTime })
      return result
    } catch (error) {
      const endTime = performance.now()
      const responseTime = endTime - startTime
      
      reportError(componentName, error instanceof Error ? error.message : 'Unknown error', 'high')
      updatePerformance({ responseTime })
      throw error
    }
  }
  
  return { trackPerformance }
}

/**
 * Hook for AI feature gating
 */
export const useAIFeatureGate = (feature: keyof AIIntegrationState['features']) => {
  const { isFeatureEnabled, state } = useAIIntegration()
  
  return {
    isEnabled: isFeatureEnabled(feature),
    isAIEnabled: state.isAIEnabled,
    confidenceThreshold: state.globalConfidenceThreshold
  }
}

/**
 * Hook for AI error boundary integration
 */
export const useAIErrorHandler = (componentName: string) => {
  const { reportError } = useAIIntegration()
  
  const handleError = (error: Error, errorInfo?: any) => {
    reportError(componentName, error.message, 'critical')
    console.error(`AI Component Error in ${componentName}:`, error, errorInfo)
  }
  
  return { handleError }
}

export default AIIntegrationProvider
