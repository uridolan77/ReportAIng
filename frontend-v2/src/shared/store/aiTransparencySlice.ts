import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from './index'
import type {
  PromptConstructionTrace,
  ConfidenceAnalysis,
  AIDecisionExplanation,
  OptimizationSuggestion,
  AlternativeOption
} from '@shared/types/ai'

// ============================================================================
// STATE INTERFACE
// ============================================================================

export interface AITransparencyState {
  // Active traces
  traces: Record<string, PromptConstructionTrace>
  activeTraceId: string | null
  
  // Transparency settings
  showDetailedMetrics: boolean
  transparencyLevel: 'basic' | 'detailed' | 'expert'
  confidenceThreshold: number
  
  // UI state
  activeTab: string
  expandedSections: string[]
  
  // Analysis data
  currentAnalysis: ConfidenceAnalysis | null
  currentExplanation: AIDecisionExplanation | null
  
  // History
  traceHistory: string[]
  maxHistorySize: number
  
  // Loading and error states
  loading: boolean
  error: string | null
  
  // Performance tracking
  performanceMetrics: {
    averageTraceTime: number
    totalTraces: number
    successRate: number
  }
}

// ============================================================================
// INITIAL STATE
// ============================================================================

const initialState: AITransparencyState = {
  traces: {},
  activeTraceId: null,
  showDetailedMetrics: false,
  transparencyLevel: 'detailed',
  confidenceThreshold: 0.6,
  activeTab: 'construction',
  expandedSections: [],
  currentAnalysis: null,
  currentExplanation: null,
  traceHistory: [],
  maxHistorySize: 50,
  loading: false,
  error: null,
  performanceMetrics: {
    averageTraceTime: 0,
    totalTraces: 0,
    successRate: 0
  }
}

// ============================================================================
// SLICE DEFINITION
// ============================================================================

export const aiTransparencySlice = createSlice({
  name: 'aiTransparency',
  initialState,
  reducers: {
    // Trace management
    addTrace: (state, action: PayloadAction<PromptConstructionTrace>) => {
      const trace = action.payload
      state.traces[trace.traceId] = trace
      
      // Add to history
      state.traceHistory.unshift(trace.traceId)
      if (state.traceHistory.length > state.maxHistorySize) {
        const removedId = state.traceHistory.pop()
        if (removedId && removedId !== state.activeTraceId) {
          delete state.traces[removedId]
        }
      }
      
      // Update performance metrics
      state.performanceMetrics.totalTraces += 1
      const totalTime = Object.values(state.traces).reduce(
        (sum, t) => sum + t.metadata.processingTime, 0
      )
      state.performanceMetrics.averageTraceTime = totalTime / state.performanceMetrics.totalTraces
    },

    setActiveTrace: (state, action: PayloadAction<string>) => {
      state.activeTraceId = action.payload
    },

    updateTrace: (state, action: PayloadAction<{ id: string; changes: Partial<PromptConstructionTrace> }>) => {
      const { id, changes } = action.payload
      if (state.traces[id]) {
        state.traces[id] = { ...state.traces[id], ...changes }
      }
    },

    removeTrace: (state, action: PayloadAction<string>) => {
      const traceId = action.payload
      delete state.traces[traceId]
      state.traceHistory = state.traceHistory.filter(id => id !== traceId)
      if (state.activeTraceId === traceId) {
        state.activeTraceId = state.traceHistory[0] || null
      }
    },

    clearTraces: (state) => {
      state.traces = {}
      state.traceHistory = []
      state.activeTraceId = null
      state.currentAnalysis = null
      state.currentExplanation = null
    },

    // Settings
    setShowDetailedMetrics: (state, action: PayloadAction<boolean>) => {
      state.showDetailedMetrics = action.payload
    },

    setTransparencyLevel: (state, action: PayloadAction<'basic' | 'detailed' | 'expert'>) => {
      state.transparencyLevel = action.payload
    },

    setConfidenceThreshold: (state, action: PayloadAction<number>) => {
      state.confidenceThreshold = Math.max(0, Math.min(1, action.payload))
    },

    // UI state
    setActiveTab: (state, action: PayloadAction<string>) => {
      state.activeTab = action.payload
    },

    toggleSection: (state, action: PayloadAction<string>) => {
      const section = action.payload
      if (state.expandedSections.includes(section)) {
        state.expandedSections = state.expandedSections.filter(s => s !== section)
      } else {
        state.expandedSections.push(section)
      }
    },

    setExpandedSections: (state, action: PayloadAction<string[]>) => {
      state.expandedSections = action.payload
    },

    // Analysis data
    setCurrentAnalysis: (state, action: PayloadAction<ConfidenceAnalysis | null>) => {
      state.currentAnalysis = action.payload
    },

    setCurrentExplanation: (state, action: PayloadAction<AIDecisionExplanation | null>) => {
      state.currentExplanation = action.payload
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

    // Performance metrics
    updatePerformanceMetrics: (state, action: PayloadAction<Partial<AITransparencyState['performanceMetrics']>>) => {
      state.performanceMetrics = { ...state.performanceMetrics, ...action.payload }
    },

    // Bulk operations
    importTraces: (state, action: PayloadAction<PromptConstructionTrace[]>) => {
      action.payload.forEach(trace => {
        state.traces[trace.traceId] = trace
      })
      state.traceHistory = Object.keys(state.traces).slice(0, state.maxHistorySize)
    },

    resetState: () => initialState
  }
})

// ============================================================================
// ACTIONS
// ============================================================================

export const {
  addTrace,
  setActiveTrace,
  updateTrace,
  removeTrace,
  clearTraces,
  setShowDetailedMetrics,
  setTransparencyLevel,
  setConfidenceThreshold,
  setActiveTab,
  toggleSection,
  setExpandedSections,
  setCurrentAnalysis,
  setCurrentExplanation,
  setLoading,
  setError,
  updatePerformanceMetrics,
  importTraces,
  resetState
} = aiTransparencySlice.actions

// ============================================================================
// SELECTORS
// ============================================================================

export const selectAITransparency = (state: RootState) => state.aiTransparency

export const selectActiveTrace = (state: RootState) => {
  const { traces, activeTraceId } = state.aiTransparency
  return activeTraceId ? traces[activeTraceId] : null
}

export const selectTraceById = (state: RootState, traceId: string) => 
  state.aiTransparency.traces[traceId]

export const selectRecentTraces = (state: RootState, limit = 10) => {
  const { traces, traceHistory } = state.aiTransparency
  return traceHistory
    .slice(0, limit)
    .map(id => traces[id])
    .filter(Boolean)
}

export const selectTransparencySettings = (state: RootState) => ({
  showDetailedMetrics: state.aiTransparency.showDetailedMetrics,
  transparencyLevel: state.aiTransparency.transparencyLevel,
  confidenceThreshold: state.aiTransparency.confidenceThreshold
})

export const selectTransparencyUI = (state: RootState) => ({
  activeTab: state.aiTransparency.activeTab,
  expandedSections: state.aiTransparency.expandedSections,
  loading: state.aiTransparency.loading,
  error: state.aiTransparency.error
})

export const selectCurrentAnalysisData = (state: RootState) => ({
  analysis: state.aiTransparency.currentAnalysis,
  explanation: state.aiTransparency.currentExplanation,
  activeTrace: selectActiveTrace(state)
})

export const selectPerformanceMetrics = (state: RootState) => 
  state.aiTransparency.performanceMetrics

// ============================================================================
// THUNKS (Async Actions)
// ============================================================================

// These would be implemented when the API endpoints are available
export const transparencyThunks = {
  // loadTrace: createAsyncThunk('aiTransparency/loadTrace', async (traceId: string) => {
  //   const response = await fetch(`/api/ai/transparency/trace/${traceId}`)
  //   return response.json()
  // }),
  
  // exportTrace: createAsyncThunk('aiTransparency/exportTrace', async (traceId: string) => {
  //   const response = await fetch(`/api/ai/transparency/export/${traceId}`)
  //   return response.blob()
  // })
}

export default aiTransparencySlice.reducer
