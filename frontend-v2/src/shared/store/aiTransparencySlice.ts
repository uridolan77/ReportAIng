import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from './index'

// ============================================================================
// AI TRANSPARENCY STATE MANAGEMENT
// ============================================================================

export interface ProcessFlowSession {
  sessionId: string
  userId: string
  startTime: string
  endTime?: string
  status: 'active' | 'completed' | 'failed'
  steps: ProcessFlowStep[]
  logs: ProcessFlowLog[]
  transparency?: ProcessFlowTransparency
}

export interface ProcessFlowStep {
  stepId: string
  sessionId: string
  stepName: string
  startTime: string
  endTime?: string
  status: 'pending' | 'running' | 'completed' | 'failed'
  confidence?: number
  metadata?: Record<string, any>
}

export interface ProcessFlowLog {
  logId: string
  sessionId: string
  stepId?: string
  timestamp: string
  level: 'info' | 'warning' | 'error' | 'debug'
  message: string
  metadata?: Record<string, any>
}

export interface ProcessFlowTransparency {
  sessionId: string
  model?: string
  temperature?: number
  promptTokens?: number
  completionTokens?: number
  totalTokens?: number
  estimatedCost?: number
  confidence?: number
  aiProcessingTimeMs?: number
  apiCallCount?: number
}

export interface PromptConstructionTrace {
  traceId: string
  sessionId: string
  userQuery: string
  businessContext: any
  schema: any
  constructedPrompt: string
  confidence: number
  timestamp: string
}

export interface TransparencyMetrics {
  totalSessions: number
  successRate: number
  avgProcessingTime: number
  avgConfidence: number
  stepPerformance: Array<{
    stepName: string
    avgDuration: number
    successRate: number
    avgConfidence: number
  }>
  tokenUsage: number
  estimatedCost: number
}

export interface AITransparencyState {
  // Active session tracking
  activeSessionId: string | null
  activeSessions: Record<string, ProcessFlowSession>
  
  // Transparency settings
  showTransparencyPanel: boolean
  transparencyLevel: 'basic' | 'detailed' | 'expert'
  confidenceThreshold: number
  autoShowTransparency: boolean
  
  // Traces and analysis
  traces: Record<string, PromptConstructionTrace>
  activeTraceId: string | null
  
  // Metrics and analytics
  metrics: TransparencyMetrics | null
  metricsLoading: boolean
  
  // UI state
  selectedStepId: string | null
  expandedSections: string[]
  
  // Real-time updates
  isStreaming: boolean
  lastUpdate: string | null
}

const initialState: AITransparencyState = {
  activeSessionId: null,
  activeSessions: {},
  showTransparencyPanel: false,
  transparencyLevel: 'basic',
  confidenceThreshold: 0.7,
  autoShowTransparency: true,
  traces: {},
  activeTraceId: null,
  metrics: null,
  metricsLoading: false,
  selectedStepId: null,
  expandedSections: [],
  isStreaming: false,
  lastUpdate: null,
}

export const aiTransparencySlice = createSlice({
  name: 'aiTransparency',
  initialState,
  reducers: {
    // Session management
    setActiveSession: (state, action: PayloadAction<string>) => {
      state.activeSessionId = action.payload
    },
    
    updateSession: (state, action: PayloadAction<ProcessFlowSession>) => {
      const session = action.payload
      state.activeSessions[session.sessionId] = session
      state.lastUpdate = new Date().toISOString()
    },
    
    clearSession: (state, action: PayloadAction<string>) => {
      delete state.activeSessions[action.payload]
      if (state.activeSessionId === action.payload) {
        state.activeSessionId = null
      }
    },
    
    // Transparency panel controls
    toggleTransparencyPanel: (state) => {
      state.showTransparencyPanel = !state.showTransparencyPanel
    },
    
    setTransparencyPanel: (state, action: PayloadAction<boolean>) => {
      state.showTransparencyPanel = action.payload
    },
    
    setTransparencyLevel: (state, action: PayloadAction<'basic' | 'detailed' | 'expert'>) => {
      state.transparencyLevel = action.payload
    },
    
    setConfidenceThreshold: (state, action: PayloadAction<number>) => {
      state.confidenceThreshold = action.payload
    },
    
    setAutoShowTransparency: (state, action: PayloadAction<boolean>) => {
      state.autoShowTransparency = action.payload
    },
    
    // Trace management
    setActiveTrace: (state, action: PayloadAction<string>) => {
      state.activeTraceId = action.payload
    },
    
    updateTrace: (state, action: PayloadAction<PromptConstructionTrace>) => {
      const trace = action.payload
      state.traces[trace.traceId] = trace
    },
    
    clearTrace: (state, action: PayloadAction<string>) => {
      delete state.traces[action.payload]
      if (state.activeTraceId === action.payload) {
        state.activeTraceId = null
      }
    },
    
    // Metrics management
    setMetrics: (state, action: PayloadAction<TransparencyMetrics>) => {
      state.metrics = action.payload
      state.metricsLoading = false
    },
    
    setMetricsLoading: (state, action: PayloadAction<boolean>) => {
      state.metricsLoading = action.payload
    },
    
    // UI state management
    setSelectedStep: (state, action: PayloadAction<string | null>) => {
      state.selectedStepId = action.payload
    },
    
    toggleExpandedSection: (state, action: PayloadAction<string>) => {
      const sectionId = action.payload
      const index = state.expandedSections.indexOf(sectionId)
      if (index >= 0) {
        state.expandedSections.splice(index, 1)
      } else {
        state.expandedSections.push(sectionId)
      }
    },
    
    setExpandedSections: (state, action: PayloadAction<string[]>) => {
      state.expandedSections = action.payload
    },
    
    // Streaming state
    setStreaming: (state, action: PayloadAction<boolean>) => {
      state.isStreaming = action.payload
    },
    
    // Reset state
    resetTransparency: (state) => {
      return { ...initialState }
    },
  },
})

export const aiTransparencyActions = aiTransparencySlice.actions

// Selectors
export const selectActiveSession = (state: RootState) => 
  state.aiTransparency.activeSessionId 
    ? state.aiTransparency.activeSessions[state.aiTransparency.activeSessionId]
    : null

export const selectActiveTrace = (state: RootState) =>
  state.aiTransparency.activeTraceId
    ? state.aiTransparency.traces[state.aiTransparency.activeTraceId]
    : null

export const selectTransparencySettings = (state: RootState) => ({
  showPanel: state.aiTransparency.showTransparencyPanel,
  level: state.aiTransparency.transparencyLevel,
  confidenceThreshold: state.aiTransparency.confidenceThreshold,
  autoShow: state.aiTransparency.autoShowTransparency,
})

export const selectSessionSteps = (state: RootState, sessionId: string) =>
  state.aiTransparency.activeSessions[sessionId]?.steps || []

export const selectSessionLogs = (state: RootState, sessionId: string) =>
  state.aiTransparency.activeSessions[sessionId]?.logs || []

export const selectTransparencyMetrics = (state: RootState) =>
  state.aiTransparency.metrics

export default aiTransparencySlice.reducer
