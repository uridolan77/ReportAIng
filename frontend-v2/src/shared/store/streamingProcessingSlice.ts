import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from './index'
import type {
  StreamingSession,
  QueryProcessingProgress,
  StreamingInsight,
  ProcessingPhaseDetails
} from '@shared/types/ai'

// ============================================================================
// STATE INTERFACE
// ============================================================================

export interface StreamingProcessingState {
  // Active sessions
  activeSessions: Record<string, StreamingSession>
  currentSessionId: string | null
  
  // Processing state
  currentProgress: QueryProcessingProgress | null
  processingPhases: Record<string, ProcessingPhaseDetails>
  
  // Insights
  insights: StreamingInsight[]
  newInsightCount: number
  maxInsights: number
  
  // Real-time metrics
  metrics: {
    averageProcessingTime: number
    successRate: number
    errorRate: number
    throughput: number
    activeConnections: number
  }
  
  // Session history
  sessionHistory: string[]
  maxHistorySize: number
  
  // UI state
  showRealTimeUpdates: boolean
  autoScroll: boolean
  selectedPhase: string | null
  
  // WebSocket connection
  connectionStatus: 'connected' | 'connecting' | 'disconnected' | 'error'
  reconnectAttempts: number
  maxReconnectAttempts: number
  
  // Loading and error states
  loading: boolean
  error: string | null
}

// ============================================================================
// INITIAL STATE
// ============================================================================

const initialState: StreamingProcessingState = {
  activeSessions: {},
  currentSessionId: null,
  currentProgress: null,
  processingPhases: {},
  insights: [],
  newInsightCount: 0,
  maxInsights: 100,
  metrics: {
    averageProcessingTime: 0,
    successRate: 0,
    errorRate: 0,
    throughput: 0,
    activeConnections: 0
  },
  sessionHistory: [],
  maxHistorySize: 50,
  showRealTimeUpdates: true,
  autoScroll: true,
  selectedPhase: null,
  connectionStatus: 'disconnected',
  reconnectAttempts: 0,
  maxReconnectAttempts: 5,
  loading: false,
  error: null
}

// ============================================================================
// SLICE DEFINITION
// ============================================================================

export const streamingProcessingSlice = createSlice({
  name: 'streamingProcessing',
  initialState,
  reducers: {
    // Session management
    startSession: (state, action: PayloadAction<StreamingSession>) => {
      const session = action.payload
      state.activeSessions[session.sessionId] = session
      state.currentSessionId = session.sessionId
      state.currentProgress = session.progress
      state.error = null
    },

    updateSession: (state, action: PayloadAction<{ sessionId: string; updates: Partial<StreamingSession> }>) => {
      const { sessionId, updates } = action.payload
      if (state.activeSessions[sessionId]) {
        state.activeSessions[sessionId] = { ...state.activeSessions[sessionId], ...updates }
        if (sessionId === state.currentSessionId) {
          state.currentProgress = updates.progress || state.currentProgress
        }
      }
    },

    endSession: (state, action: PayloadAction<string>) => {
      const sessionId = action.payload
      if (state.activeSessions[sessionId]) {
        // Move to history
        state.sessionHistory.unshift(sessionId)
        if (state.sessionHistory.length > state.maxHistorySize) {
          const removedId = state.sessionHistory.pop()
          if (removedId && removedId !== sessionId) {
            delete state.activeSessions[removedId]
          }
        }
        
        // Clear current session if it's the one being ended
        if (state.currentSessionId === sessionId) {
          state.currentSessionId = null
          state.currentProgress = null
        }
      }
    },

    setCurrentSession: (state, action: PayloadAction<string | null>) => {
      state.currentSessionId = action.payload
      if (action.payload && state.activeSessions[action.payload]) {
        state.currentProgress = state.activeSessions[action.payload].progress
      } else {
        state.currentProgress = null
      }
    },

    // Progress tracking
    updateProgress: (state, action: PayloadAction<QueryProcessingProgress>) => {
      state.currentProgress = action.payload
      if (state.currentSessionId && state.activeSessions[state.currentSessionId]) {
        state.activeSessions[state.currentSessionId].progress = action.payload
        state.activeSessions[state.currentSessionId].currentPhase = action.payload.phase
      }
    },

    updatePhaseDetails: (state, action: PayloadAction<ProcessingPhaseDetails>) => {
      const phase = action.payload
      state.processingPhases[phase.phase] = phase
    },

    setSelectedPhase: (state, action: PayloadAction<string | null>) => {
      state.selectedPhase = action.payload
    },

    // Insights management
    addInsight: (state, action: PayloadAction<StreamingInsight>) => {
      state.insights.unshift(action.payload)
      state.newInsightCount += 1
      
      // Limit insights to maxInsights
      if (state.insights.length > state.maxInsights) {
        state.insights = state.insights.slice(0, state.maxInsights)
      }
    },

    addInsights: (state, action: PayloadAction<StreamingInsight[]>) => {
      const newInsights = action.payload
      state.insights.unshift(...newInsights)
      state.newInsightCount += newInsights.length
      
      // Limit insights to maxInsights
      if (state.insights.length > state.maxInsights) {
        state.insights = state.insights.slice(0, state.maxInsights)
      }
    },

    dismissInsight: (state, action: PayloadAction<string>) => {
      state.insights = state.insights.filter(insight => insight.id !== action.payload)
    },

    clearInsights: (state) => {
      state.insights = []
      state.newInsightCount = 0
    },

    markInsightsAsRead: (state) => {
      state.newInsightCount = 0
    },

    // Metrics
    updateMetrics: (state, action: PayloadAction<Partial<StreamingProcessingState['metrics']>>) => {
      state.metrics = { ...state.metrics, ...action.payload }
    },

    // UI state
    setShowRealTimeUpdates: (state, action: PayloadAction<boolean>) => {
      state.showRealTimeUpdates = action.payload
    },

    setAutoScroll: (state, action: PayloadAction<boolean>) => {
      state.autoScroll = action.payload
    },

    // Connection management
    setConnectionStatus: (state, action: PayloadAction<StreamingProcessingState['connectionStatus']>) => {
      state.connectionStatus = action.payload
      if (action.payload === 'connected') {
        state.reconnectAttempts = 0
        state.error = null
      }
    },

    incrementReconnectAttempts: (state) => {
      state.reconnectAttempts += 1
    },

    resetReconnectAttempts: (state) => {
      state.reconnectAttempts = 0
    },

    // Loading and error states
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.loading = action.payload
      if (action.payload) {
        state.error = null
      }
    },

    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
      state.loading = false
    },

    // Bulk operations
    clearAllSessions: (state) => {
      state.activeSessions = {}
      state.currentSessionId = null
      state.currentProgress = null
      state.sessionHistory = []
    },

    resetState: () => initialState
  }
})

// ============================================================================
// ACTIONS
// ============================================================================

export const {
  startSession,
  updateSession,
  endSession,
  setCurrentSession,
  updateProgress,
  updatePhaseDetails,
  setSelectedPhase,
  addInsight,
  addInsights,
  dismissInsight,
  clearInsights,
  markInsightsAsRead,
  updateMetrics,
  setShowRealTimeUpdates,
  setAutoScroll,
  setConnectionStatus,
  incrementReconnectAttempts,
  resetReconnectAttempts,
  setLoading,
  setError,
  clearAllSessions,
  resetState
} = streamingProcessingSlice.actions

// ============================================================================
// SELECTORS
// ============================================================================

export const selectStreamingProcessing = (state: RootState) => state.streamingProcessing

export const selectCurrentSession = (state: RootState) => {
  const { activeSessions, currentSessionId } = state.streamingProcessing
  return currentSessionId ? activeSessions[currentSessionId] : null
}

export const selectCurrentProgress = (state: RootState) => 
  state.streamingProcessing.currentProgress

export const selectActiveSessions = (state: RootState) => 
  Object.values(state.streamingProcessing.activeSessions)

export const selectSessionById = (state: RootState, sessionId: string) => 
  state.streamingProcessing.activeSessions[sessionId]

export const selectInsights = (state: RootState) => state.streamingProcessing.insights

export const selectNewInsights = (state: RootState) => {
  const { insights, newInsightCount } = state.streamingProcessing
  return insights.slice(0, newInsightCount)
}

export const selectInsightsByType = (state: RootState, type: StreamingInsight['type']) => 
  state.streamingProcessing.insights.filter(insight => insight.type === type)

export const selectProcessingMetrics = (state: RootState) => 
  state.streamingProcessing.metrics

export const selectConnectionStatus = (state: RootState) => ({
  status: state.streamingProcessing.connectionStatus,
  reconnectAttempts: state.streamingProcessing.reconnectAttempts,
  maxReconnectAttempts: state.streamingProcessing.maxReconnectAttempts
})

export const selectStreamingUI = (state: RootState) => ({
  showRealTimeUpdates: state.streamingProcessing.showRealTimeUpdates,
  autoScroll: state.streamingProcessing.autoScroll,
  selectedPhase: state.streamingProcessing.selectedPhase,
  loading: state.streamingProcessing.loading,
  error: state.streamingProcessing.error
})

export const selectProcessingPhases = (state: RootState) => 
  state.streamingProcessing.processingPhases

export const selectPhaseDetails = (state: RootState, phase: string) => 
  state.streamingProcessing.processingPhases[phase]

export const selectIsProcessing = (state: RootState) => {
  const currentProgress = state.streamingProcessing.currentProgress
  return currentProgress && currentProgress.phase !== 'complete' && currentProgress.phase !== 'error'
}

export default streamingProcessingSlice.reducer
