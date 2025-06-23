// ProcessFlow Analytics Types
// Enhanced analytics interfaces for the new ProcessFlow system

// ============================================================================
// TOKEN USAGE & COST ANALYTICS
// ============================================================================

export interface TokenUsageStatistics {
  totalRequests: number
  totalTokens: number
  totalCost: number
  averageTokensPerRequest: number
  averageCostPerRequest: number
  dateRange: { start: string; end: string }
  userId?: string
  successRate?: number
  errorRate?: number
}

export interface DailyTokenUsage {
  date: string
  totalRequests: number
  totalTokens: number
  totalCost: number
  averageTokensPerRequest: number
  requestType: string
  intentType: string
  promptTokens: number
  completionTokens: number
  averageConfidence: number
}

export interface TokenUsageFilters {
  startDate?: string
  endDate?: string
  userId?: string
  queryType?: string
  intentType?: string
  model?: string
}

// ============================================================================
// PROCESSFLOW SESSION ANALYTICS
// ============================================================================

export interface ProcessFlowSessionAnalytics {
  totalSessions: number
  completedSessions: number
  failedSessions: number
  averageDuration: number
  averageConfidence: number
  averageStepsPerSession: number
  successRate: number
  topFailureReasons: Array<{
    reason: string
    count: number
    percentage: number
  }>
  performanceByStepType: Array<{
    stepType: string
    averageDuration: number
    successRate: number
    averageConfidence: number
  }>
}

export interface ProcessFlowStepAnalytics {
  stepType: string
  totalExecutions: number
  successfulExecutions: number
  failedExecutions: number
  averageDuration: number
  averageConfidence: number
  commonErrors: Array<{
    error: string
    count: number
    percentage: number
  }>
  performanceTrends: Array<{
    date: string
    averageDuration: number
    successRate: number
    confidence: number
  }>
}

// ============================================================================
// ENHANCED QUERY ANALYTICS
// ============================================================================

export interface EnhancedQueryAnalytics {
  totalQueries: number
  successfulQueries: number
  failedQueries: number
  averageProcessingTime: number
  averageConfidence: number
  queryTypeDistribution: Record<string, number>
  intentTypeDistribution: Record<string, number>
  topTables: Array<{
    tableName: string
    queryCount: number
    averageConfidence: number
    successRate: number
  }>
  performanceTrends: Array<{
    date: string
    totalQueries: number
    averageTime: number
    successRate: number
    averageConfidence: number
  }>
}

// ============================================================================
// TRANSPARENCY METRICS
// ============================================================================

export interface ProcessFlowTransparencyMetrics {
  totalSessions: number
  averageTokensPerSession: number
  averageCostPerSession: number
  modelUsageDistribution: Record<string, {
    sessions: number
    totalTokens: number
    totalCost: number
    averageConfidence: number
  }>
  apiCallStatistics: {
    totalCalls: number
    averageCallsPerSession: number
    successRate: number
    averageResponseTime: number
  }
  costAnalysis: {
    totalCost: number
    costByModel: Record<string, number>
    costTrends: Array<{
      date: string
      cost: number
      tokens: number
      sessions: number
    }>
    projectedMonthlyCost: number
  }
}

// ============================================================================
// REAL-TIME MONITORING
// ============================================================================

export interface ProcessFlowRealTimeMetrics {
  activeSessions: number
  sessionsInProgress: number
  averageSessionDuration: number
  currentThroughput: number
  errorRate: number
  systemLoad: number
  recentSessions: Array<{
    sessionId: string
    userId: string
    status: string
    startTime: string
    currentStep?: string
    progress: number
  }>
  alerts: Array<{
    id: string
    type: 'performance' | 'error' | 'cost' | 'capacity'
    severity: 'low' | 'medium' | 'high' | 'critical'
    message: string
    timestamp: string
  }>
}

// ============================================================================
// ANALYTICS REQUEST/RESPONSE TYPES
// ============================================================================

export interface ProcessFlowAnalyticsRequest {
  startDate?: string
  endDate?: string
  userId?: string
  sessionIds?: string[]
  includeStepDetails?: boolean
  includeTokenUsage?: boolean
  includePerformanceMetrics?: boolean
  granularity?: 'hour' | 'day' | 'week' | 'month'
}

export interface ProcessFlowAnalyticsResponse {
  sessionAnalytics: ProcessFlowSessionAnalytics
  tokenUsage: TokenUsageStatistics
  transparencyMetrics: ProcessFlowTransparencyMetrics
  queryAnalytics: EnhancedQueryAnalytics
  realTimeMetrics?: ProcessFlowRealTimeMetrics
  generatedAt: string
  dataRange: {
    start: string
    end: string
  }
}

// ============================================================================
// DASHBOARD SPECIFIC TYPES
// ============================================================================

export interface ProcessFlowDashboardData {
  overview: {
    totalSessions: number
    activeSessions: number
    successRate: number
    averageConfidence: number
    totalCost: number
    costTrend: number
  }
  charts: {
    sessionTrends: Array<{
      date: string
      sessions: number
      successRate: number
      averageConfidence: number
    }>
    tokenUsageTrends: Array<{
      date: string
      tokens: number
      cost: number
      efficiency: number
    }>
    stepPerformance: Array<{
      stepType: string
      averageDuration: number
      successRate: number
      volume: number
    }>
    errorDistribution: Array<{
      category: string
      count: number
      percentage: number
    }>
  }
  alerts: Array<{
    id: string
    type: string
    severity: string
    message: string
    timestamp: string
    resolved: boolean
  }>
  recommendations: Array<{
    type: 'performance' | 'cost' | 'accuracy'
    title: string
    description: string
    impact: 'high' | 'medium' | 'low'
    effort: 'high' | 'medium' | 'low'
  }>
}

// ============================================================================
// EXPORT TYPES
// ============================================================================

export interface ProcessFlowAnalyticsExportConfig {
  format: 'json' | 'csv' | 'excel'
  includeSessionDetails: boolean
  includeStepBreakdown: boolean
  includeTokenUsage: boolean
  includeErrorLogs: boolean
  dateRange: {
    start: string
    end: string
  }
  filters?: ProcessFlowAnalyticsRequest
}
