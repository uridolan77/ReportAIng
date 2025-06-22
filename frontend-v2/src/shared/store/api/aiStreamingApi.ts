import { useState, useEffect, useCallback } from 'react'
import { baseApi } from './baseApi'
import type {
  StreamingSession,
  StreamingQueryResponse,
  StreamingInsightResponse,
  StreamingAnalyticsResponse,
  StartStreamingRequest,
  StreamingStatus
} from '@shared/types/streaming'

/**
 * AI Streaming API - RTK Query service for real-time AI processing
 * 
 * Integrates with the new AIStreamingController.cs backend
 * Base URL: /api/aistreaming
 */
export const aiStreamingApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // Start a new streaming session
    startStreamingSession: builder.mutation<{ sessionId: string }, StartStreamingRequest>({
      query: (body) => ({
        url: '/aistreaming/start',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['StreamingSession'],
    }),

    // Cancel streaming session
    cancelStreamingSession: builder.mutation<void, string>({
      query: (sessionId) => ({
        url: `/aistreaming/cancel/${sessionId}`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, sessionId) => [
        { type: 'StreamingSession', id: sessionId }
      ],
    }),

    // Get session status
    getSessionStatus: builder.query<StreamingSession, string>({
      query: (sessionId) => `/aistreaming/status/${sessionId}`,
      providesTags: (result, error, sessionId) => [
        { type: 'StreamingSession', id: sessionId }
      ],
    }),

    // Get streaming analytics for user
    getStreamingAnalytics: builder.query<{
      activeSessions: number
      totalSessions: number
      averageProcessingTime: number
      successRate: number
      recentSessions: StreamingSession[]
    }, { userId?: string; days?: number }>({
      query: ({ userId, days = 7 }) => ({
        url: '/aistreaming/analytics',
        params: { userId, days },
      }),
      providesTags: ['StreamingAnalytics'],
    }),

    // Get session history
    getSessionHistory: builder.query<StreamingSession[], {
      userId?: string
      limit?: number
      status?: 'active' | 'completed' | 'cancelled' | 'error'
    }>({
      query: ({ userId, limit = 50, status }) => ({
        url: '/aistreaming/history',
        params: { userId, limit, status },
      }),
      providesTags: ['StreamingHistory'],
    }),
  }),
  overrideExisting: false,
})

// Export RTK Query hooks
export const {
  useStartStreamingSessionMutation,
  useCancelStreamingSessionMutation,
  useGetSessionStatusQuery,
  useGetStreamingAnalyticsQuery,
  useGetSessionHistoryQuery,
} = aiStreamingApi

/**
 * Custom hook for streaming query processing with Server-Sent Events
 */
export const useStreamingQuery = (sessionId: string) => {
  const [data, setData] = useState<StreamingQueryResponse[]>([])
  const [isConnected, setIsConnected] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [currentPhase, setCurrentPhase] = useState<string>('')
  const [progress, setProgress] = useState<number>(0)

  useEffect(() => {
    if (!sessionId) return

    const eventSource = new EventSource(`/api/aistreaming/query/${sessionId}`)
    
    eventSource.onopen = () => {
      setIsConnected(true)
      setError(null)
    }
    
    eventSource.onmessage = (event) => {
      try {
        const response: StreamingQueryResponse = JSON.parse(event.data)
        setData(prev => [...prev, response])
        
        // Update current phase and progress
        if (response.type === 'progress') {
          setCurrentPhase(response.data.phase)
          setProgress(response.data.progress)
        }
      } catch (err) {
        console.error('Failed to parse streaming response:', err)
        setError('Failed to parse streaming data')
      }
    }
    
    eventSource.onerror = (event) => {
      console.error('Streaming connection error:', event)
      setIsConnected(false)
      setError('Connection lost')
    }
    
    return () => {
      eventSource.close()
      setIsConnected(false)
    }
  }, [sessionId])

  const clearData = useCallback(() => {
    setData([])
    setProgress(0)
    setCurrentPhase('')
  }, [])

  return { 
    data, 
    isConnected, 
    error, 
    currentPhase, 
    progress,
    clearData 
  }
}

/**
 * Custom hook for streaming insights with Server-Sent Events
 */
export const useStreamingInsights = (queryId: string) => {
  const [insights, setInsights] = useState<StreamingInsightResponse[]>([])
  const [isConnected, setIsConnected] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!queryId) return

    const eventSource = new EventSource(`/api/aistreaming/insights/${queryId}`)
    
    eventSource.onopen = () => {
      setIsConnected(true)
      setError(null)
    }
    
    eventSource.onmessage = (event) => {
      try {
        const insight: StreamingInsightResponse = JSON.parse(event.data)
        setInsights(prev => [...prev, insight])
      } catch (err) {
        console.error('Failed to parse insight response:', err)
        setError('Failed to parse insight data')
      }
    }
    
    eventSource.onerror = (event) => {
      console.error('Insights streaming error:', event)
      setIsConnected(false)
      setError('Insights connection lost')
    }
    
    return () => {
      eventSource.close()
      setIsConnected(false)
    }
  }, [queryId])

  const clearInsights = useCallback(() => {
    setInsights([])
  }, [])

  return { 
    insights, 
    isConnected, 
    error,
    clearInsights 
  }
}

/**
 * Custom hook for streaming analytics with Server-Sent Events
 */
export const useStreamingAnalyticsUpdates = (userId?: string) => {
  const [analytics, setAnalytics] = useState<StreamingAnalyticsResponse[]>([])
  const [isConnected, setIsConnected] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const url = userId 
      ? `/api/aistreaming/analytics/${userId}`
      : '/api/aistreaming/analytics/global'
    
    const eventSource = new EventSource(url)
    
    eventSource.onopen = () => {
      setIsConnected(true)
      setError(null)
    }
    
    eventSource.onmessage = (event) => {
      try {
        const analyticsUpdate: StreamingAnalyticsResponse = JSON.parse(event.data)
        setAnalytics(prev => [...prev.slice(-99), analyticsUpdate]) // Keep last 100 updates
      } catch (err) {
        console.error('Failed to parse analytics response:', err)
        setError('Failed to parse analytics data')
      }
    }
    
    eventSource.onerror = (event) => {
      console.error('Analytics streaming error:', event)
      setIsConnected(false)
      setError('Analytics connection lost')
    }
    
    return () => {
      eventSource.close()
      setIsConnected(false)
    }
  }, [userId])

  const clearAnalytics = useCallback(() => {
    setAnalytics([])
  }, [])

  return { 
    analytics, 
    isConnected, 
    error,
    clearAnalytics 
  }
}

/**
 * Enhanced hook for complete streaming session management
 */
export const useStreamingSession = () => {
  const [startSession] = useStartStreamingSessionMutation()
  const [cancelSession] = useCancelStreamingSessionMutation()
  const [currentSessionId, setCurrentSessionId] = useState<string | null>(null)
  
  const { data: sessionStatus } = useGetSessionStatusQuery(currentSessionId!, {
    skip: !currentSessionId,
    pollingInterval: 2000, // Poll every 2 seconds
  })

  const streamingData = useStreamingQuery(currentSessionId!)
  const streamingInsights = useStreamingInsights(currentSessionId!)

  const startNewSession = useCallback(async (request: StartStreamingRequest) => {
    try {
      const result = await startSession(request).unwrap()
      setCurrentSessionId(result.sessionId)
      return result.sessionId
    } catch (error) {
      console.error('Failed to start streaming session:', error)
      throw error
    }
  }, [startSession])

  const cancelCurrentSession = useCallback(async () => {
    if (currentSessionId) {
      try {
        await cancelSession(currentSessionId).unwrap()
        setCurrentSessionId(null)
        streamingData.clearData()
        streamingInsights.clearInsights()
      } catch (error) {
        console.error('Failed to cancel streaming session:', error)
        throw error
      }
    }
  }, [currentSessionId, cancelSession, streamingData, streamingInsights])

  const resetSession = useCallback(() => {
    setCurrentSessionId(null)
    streamingData.clearData()
    streamingInsights.clearInsights()
  }, [streamingData, streamingInsights])

  return {
    sessionId: currentSessionId,
    sessionStatus,
    streamingData,
    streamingInsights,
    startSession: startNewSession,
    cancelSession: cancelCurrentSession,
    resetSession,
    isActive: !!currentSessionId && sessionStatus?.status === 'active',
  }
}

export default aiStreamingApi
