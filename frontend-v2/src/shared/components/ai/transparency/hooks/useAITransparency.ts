import { useState, useEffect, useCallback } from 'react'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import {
  selectTraceById,
  selectCurrentAnalysisData,
  addTrace,
  setActiveTrace,
  setLoading,
  setError,
  setCurrentAnalysis,
  setCurrentExplanation
} from '@shared/store/aiTransparencySlice'
import { 
  useGetTransparencyTraceQuery,
  useGetConfidenceBreakdownQuery,
  useGetAlternativeOptionsQuery,
  useGetTransparencyTracesQuery
} from '@shared/store/api/transparencyApi'
import { transparencySignalR } from '@shared/services/transparencySignalR'
import type { UseAITransparencyResult, TransparencyData } from '../types'
import type { PromptConstructionTrace, ConfidenceAnalysis, AIDecisionExplanation } from '@shared/types/ai'

/**
 * Hook for managing AI transparency data and real-time updates
 * 
 * Provides comprehensive transparency information including:
 * - Prompt construction traces
 * - Confidence analysis
 * - Decision explanations
 * - Alternative options
 * - Real-time updates via SignalR
 */
export const useAITransparency = (traceId: string): UseAITransparencyResult => {
  const dispatch = useAppDispatch()
  const trace = useAppSelector(state => selectTraceById(state, traceId))
  const { analysis, explanation, activeTrace } = useAppSelector(selectCurrentAnalysisData)
  const [localLoading, setLocalLoading] = useState(false)
  const [localError, setLocalError] = useState<string>()

  // Use real API queries
  const { data: traceData, isLoading: traceLoading, error: traceError } = useGetTransparencyTraceQuery(
    traceId, 
    { skip: !traceId }
  )
  const { data: confidenceData, isLoading: confidenceLoading } = useGetConfidenceBreakdownQuery(
    traceData?.analysisId || '', 
    { skip: !traceData?.analysisId }
  )
  const { data: alternativesData, isLoading: alternativesLoading } = useGetAlternativeOptionsQuery(
    traceId, 
    { skip: !traceId }
  )

  // Combine transparency data from real API responses
  const transparencyData: TransparencyData | undefined = traceData ? {
    trace: {
      traceId: traceData.traceId,
      steps: traceData.detailedMetrics?.steps || [],
      finalPrompt: traceData.summary || '',
      totalConfidence: traceData.overallConfidence || 0,
      optimizationSuggestions: traceData.optimizationSuggestions || [],
      metadata: {
        modelUsed: traceData.detailedMetrics?.modelUsed || 'unknown',
        provider: traceData.detailedMetrics?.provider || 'unknown',
        tokensUsed: traceData.detailedMetrics?.tokensUsed || 0,
        processingTime: traceData.detailedMetrics?.processingTime || 0
      }
    },
    analysis: confidenceData ? {
      overallConfidence: confidenceData.overallConfidence,
      factors: confidenceData.confidenceFactors || [],
      breakdown: confidenceData.factorBreakdown || {
        contextualRelevance: 0,
        syntacticCorrectness: 0,
        semanticClarity: 0,
        businessAlignment: 0
      },
      recommendations: confidenceData.recommendations || []
    } : undefined,
    explanation: {
      decision: traceData.summary || 'AI decision based on analysis',
      reasoning: traceData.detailedMetrics?.reasoning || [],
      confidence: traceData.overallConfidence || 0,
      alternatives: alternativesData || [],
      factors: confidenceData?.confidenceFactors || [],
      recommendations: traceData.optimizationSuggestions || []
    },
    alternatives: alternativesData || [],
    recommendations: traceData.optimizationSuggestions || [],
    metadata: {
      timestamp: traceData.generatedAt || new Date().toISOString(),
      version: '2.0',
      source: 'transparency-api'
    }
  } : undefined

  // Load trace data - now handled by RTK Query automatically
  const loadTrace = useCallback(async (id: string) => {
    setLocalLoading(true)
    setLocalError(undefined)
    dispatch(setLoading(true))

    try {
      // Data is automatically loaded by RTK Query hooks
      // Just need to handle the state updates
      if (traceData) {
        dispatch(setActiveTrace(id))
        
        if (confidenceData) {
          dispatch(setCurrentAnalysis({
            overallConfidence: confidenceData.overallConfidence,
            factors: confidenceData.confidenceFactors || [],
            breakdown: confidenceData.factorBreakdown || {
              contextualRelevance: 0,
              syntacticCorrectness: 0,
              semanticClarity: 0,
              businessAlignment: 0
            },
            recommendations: confidenceData.recommendations || []
          }))
        }
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to load transparency data'
      setLocalError(errorMessage)
      dispatch(setError(errorMessage))
    } finally {
      setLocalLoading(false)
      dispatch(setLoading(false))
    }
  }, [dispatch, traceData, confidenceData])

  // Handle API loading states and errors
  useEffect(() => {
    const isLoading = traceLoading || confidenceLoading || alternativesLoading
    const hasError = traceError

    setLocalLoading(isLoading)
    
    if (hasError) {
      const errorMessage = 'error' in hasError ? hasError.error : 'Failed to load transparency data'
      setLocalError(errorMessage)
      dispatch(setError(errorMessage))
    } else {
      setLocalError(undefined)
    }

    dispatch(setLoading(isLoading))
  }, [traceLoading, confidenceLoading, alternativesLoading, traceError, dispatch])

  // Legacy mock trace structure for backward compatibility
  const createLegacyTrace = useCallback((apiData: any): PromptConstructionTrace => {
    return {
      traceId: apiData.traceId || '',
      steps: apiData.detailedMetrics?.steps || [],
      finalPrompt: apiData.summary || '',
      totalConfidence: apiData.overallConfidence || 0,
      optimizationSuggestions: apiData.optimizationSuggestions || [],
      metadata: {
        modelUsed: apiData.detailedMetrics?.modelUsed || 'unknown',
        provider: apiData.detailedMetrics?.provider || 'unknown',
        tokensUsed: apiData.detailedMetrics?.tokensUsed || 0,
        processingTime: apiData.detailedMetrics?.processingTime || 0
      }
    }
  }, [])

  // Update store when API data is available
  useEffect(() => {
    if (traceData && !trace) {
      const legacyTrace = createLegacyTrace(traceData)
      dispatch(addTrace(legacyTrace))
      dispatch(setActiveTrace(traceData.traceId))
    }
  }, [traceData, trace, createLegacyTrace, dispatch])

  // Refresh trace data
  const refreshTrace = useCallback(async () => {
    if (traceId) {
      await loadTrace(traceId)
    }
  }, [traceId, loadTrace])

  // Export trace data
  const exportTrace = useCallback(() => {
    if (transparencyData) {
      const dataStr = JSON.stringify(transparencyData, null, 2)
      const dataBlob = new Blob([dataStr], { type: 'application/json' })
      const url = URL.createObjectURL(dataBlob)
      const link = document.createElement('a')
      link.href = url
      link.download = `transparency-trace-${traceId}.json`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      URL.revokeObjectURL(url)
    }
  }, [transparencyData, traceId])

  // Load trace on mount or when traceId changes
  useEffect(() => {
    if (traceId && !trace) {
      loadTrace(traceId)
    }
  }, [traceId, trace, loadTrace])

  return {
    transparencyData,
    loading: localLoading,
    error: localError,
    refreshTrace,
    exportTrace
  }
}

/**
 * Hook for managing multiple transparency traces
 */
export const useAITransparencyHistory = (limit = 10) => {
  const dispatch = useAppDispatch()
  
  // Use real API query for traces
  const { 
    data: tracesData, 
    isLoading: loading, 
    error: apiError,
    refetch: loadTraceHistory 
  } = useGetTransparencyTracesQuery({
    page: 1,
    pageSize: limit,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  })

  const error = apiError ? 
    ('error' in apiError ? apiError.error : 'Failed to load trace history') : 
    undefined

  // Update store when traces are loaded
  useEffect(() => {
    if (tracesData?.traces) {
      tracesData.traces.forEach(apiTrace => {
        const legacyTrace: PromptConstructionTrace = {
          traceId: apiTrace.traceId,
          steps: [],
          finalPrompt: apiTrace.userQuestion,
          totalConfidence: apiTrace.overallConfidence || 0,
          optimizationSuggestions: [],
          metadata: {
            modelUsed: 'unknown',
            provider: 'unknown',
            tokensUsed: 0,
            processingTime: 0
          }
        }
        dispatch(addTrace(legacyTrace))
      })
    }
  }, [tracesData, dispatch])

  return {
    loadTraceHistory,
    loading,
    error,
    traces: tracesData?.traces || []
  }
}

/**
 * Hook for real-time transparency updates using SignalR
 */
export const useRealTimeTransparency = (sessionId?: string) => {
  const [isConnected, setIsConnected] = useState(false)
  const [lastUpdate, setLastUpdate] = useState<string>()
  const [connectionError, setConnectionError] = useState<string>()

  useEffect(() => {
    if (!sessionId) return

    let cleanup: (() => void) | undefined

    const initializeConnection = async () => {
      try {
        // Connect to SignalR
        await transparencySignalR.connect()
        setIsConnected(transparencySignalR.getConnectionStatus())
        setConnectionError(undefined)

        // Subscribe to real-time events
        const unsubscribeTraceStarted = transparencySignalR.subscribe('TraceStarted', (data) => {
          console.log('🚀 Real-time trace started:', data)
          setLastUpdate(new Date().toISOString())
        })

        const unsubscribeTraceUpdated = transparencySignalR.subscribe('TraceUpdated', (data) => {
          console.log('📝 Real-time trace updated:', data)
          setLastUpdate(new Date().toISOString())
        })

        const unsubscribeTraceCompleted = transparencySignalR.subscribe('TraceCompleted', (data) => {
          console.log('✅ Real-time trace completed:', data)
          setLastUpdate(new Date().toISOString())
        })

        const unsubscribeStepCompleted = transparencySignalR.subscribe('StepCompleted', (data) => {
          console.log('🔄 Real-time step completed:', data)
          setLastUpdate(new Date().toISOString())
        })

        const unsubscribeConfidenceUpdated = transparencySignalR.subscribe('ConfidenceUpdated', (data) => {
          console.log('📊 Real-time confidence updated:', data)
          setLastUpdate(new Date().toISOString())
        })

        // Cleanup function
        cleanup = () => {
          unsubscribeTraceStarted()
          unsubscribeTraceUpdated()
          unsubscribeTraceCompleted()
          unsubscribeStepCompleted()
          unsubscribeConfidenceUpdated()
          transparencySignalR.disconnect()
        }

      } catch (error) {
        console.error('❌ Failed to initialize SignalR connection:', error)
        setConnectionError(error instanceof Error ? error.message : 'Connection failed')
        setIsConnected(false)
      }
    }

    initializeConnection()

    return () => {
      if (cleanup) {
        cleanup()
      }
      setIsConnected(false)
    }
  }, [sessionId])

  // Monitor connection status
  useEffect(() => {
    const interval = setInterval(() => {
      const currentStatus = transparencySignalR.getConnectionStatus()
      if (currentStatus !== isConnected) {
        setIsConnected(currentStatus)
      }
    }, 1000)

    return () => clearInterval(interval)
  }, [isConnected])

  return {
    isConnected,
    lastUpdate,
    connectionError,
    connectionState: transparencySignalR.getConnectionState()
  }
}

export default useAITransparency
