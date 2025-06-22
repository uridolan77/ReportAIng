// Transparency API Types
// Based on the new TransparencyController.cs backend DTOs

export interface TransparencyReport {
  traceId: string
  generatedAt: string
  userQuestion: string
  intentType: string
  summary: string
  detailedMetrics?: Record<string, any>
  performanceAnalysis?: Record<string, any>
  optimizationSuggestions?: string[]
  errorMessage?: string
}

export interface PromptConstructionTrace {
  traceId: string
  startTime: string
  endTime: string
  userQuestion: string
  intentType: IntentType
  steps: PromptConstructionStep[]
  overallConfidence: number
  totalTokens: number
  finalPrompt: string
  metadata: Record<string, any>
}

export interface PromptConstructionStep {
  stepNumber: number
  stepName: string
  description: string
  startTime: string
  endTime: string
  duration: number
  confidence: number
  content: string
  reasoning: string
  alternatives: string[]
  metadata: Record<string, any>
}

export interface ConfidenceBreakdown {
  analysisId: string
  overallConfidence: number
  factorBreakdown: Record<string, number>
  confidenceFactors: ConfidenceFactor[]
  evolution?: ConfidenceEvolution
  timestamp: string
}

export interface ConfidenceFactor {
  factorName: string
  score: number
  weight: number
  category: string
  description: string
  supportingEvidence: string[]
  metadata: Record<string, any>
}

export interface ConfidenceEvolution {
  timePoints: ConfidenceTimePoint[]
  overallTrend: 'improving' | 'stable' | 'declining'
  significantChanges: ConfidenceChange[]
}

export interface ConfidenceTimePoint {
  timestamp: string
  confidence: number
  factors: Record<string, number>
}

export interface ConfidenceChange {
  timestamp: string
  factor: string
  oldValue: number
  newValue: number
  reason: string
}

export interface AlternativeOption {
  optionId: string
  title: string
  description: string
  confidence: number
  estimatedPerformance: number
  tradeoffs: string[]
  implementation: string
  metadata: Record<string, any>
}

export interface OptimizationSuggestion {
  suggestionId: string
  title: string
  description: string
  category: 'performance' | 'accuracy' | 'cost' | 'clarity'
  impact: 'high' | 'medium' | 'low'
  effort: 'high' | 'medium' | 'low'
  expectedImprovement: number
  implementation: string
  estimatedCostSavings?: number
  metadata: Record<string, any>
}

export interface TransparencyMetrics {
  totalTraces: number
  averageConfidence: number
  confidenceDistribution: Record<string, number>
  topOptimizations: OptimizationSuggestion[]
  usageByModel: ModelUsageMetric[]
  performanceTrends: PerformanceTrend[]
  errorAnalysis: ErrorAnalysis
  timeRange: {
    start: string
    end: string
  }
}

export interface ModelUsageMetric {
  modelName: string
  provider: string
  totalRequests: number
  averageConfidence: number
  averageResponseTime: number
  totalCost: number
  successRate: number
}

export interface PerformanceTrend {
  date: string
  averageConfidence: number
  totalRequests: number
  averageResponseTime: number
  errorRate: number
}

export interface ErrorAnalysis {
  totalErrors: number
  errorsByCategory: Record<string, number>
  commonErrorPatterns: string[]
  resolutionSuggestions: string[]
}

// Request types
export interface AnalyzePromptRequest {
  userQuestion: string
  context?: Record<string, any>
  includeAlternatives?: boolean
  detailLevel?: 'basic' | 'detailed' | 'comprehensive'
}

export interface OptimizePromptRequest {
  traceId?: string
  userQuestion?: string
  currentPrompt?: string
  targetMetrics?: {
    performance?: boolean
    accuracy?: boolean
    cost?: boolean
    clarity?: boolean
  }
  constraints?: Record<string, any>
}

// Enums
export enum IntentType {
  DataQuery = 'DataQuery',
  SchemaExploration = 'SchemaExploration',
  BusinessQuestion = 'BusinessQuestion',
  TechnicalSupport = 'TechnicalSupport',
  ReportGeneration = 'ReportGeneration',
  DataAnalysis = 'DataAnalysis',
  Unknown = 'Unknown'
}

export enum ConfidenceLevel {
  VeryHigh = 'VeryHigh',
  High = 'High',
  Medium = 'Medium',
  Low = 'Low',
  VeryLow = 'VeryLow'
}

export enum OptimizationCategory {
  Performance = 'Performance',
  Accuracy = 'Accuracy',
  Cost = 'Cost',
  Clarity = 'Clarity',
  Security = 'Security',
  Compliance = 'Compliance'
}

// Utility types
export interface TransparencySettings {
  enableDetailedLogging: boolean
  confidenceThreshold: number
  retentionDays: number
  enableOptimizationSuggestions: boolean
  autoApplyOptimizations: boolean
  notificationSettings: {
    lowConfidenceAlerts: boolean
    performanceAlerts: boolean
    costAlerts: boolean
  }
}

export interface TransparencyExportOptions {
  traceIds?: string[]
  dateRange?: {
    start: string
    end: string
  }
  format: 'json' | 'csv' | 'excel'
  includeMetadata: boolean
  includeSteps: boolean
  includeAlternatives: boolean
}

// Dashboard specific types
export interface TransparencyDashboardData {
  summary: {
    totalTraces: number
    averageConfidence: number
    totalOptimizations: number
    costSavings: number
  }
  charts: {
    confidenceTrends: Array<{ date: string; confidence: number }>
    usageByModel: Array<{ model: string; count: number; confidence: number }>
    optimizationImpact: Array<{ category: string; impact: number }>
    errorDistribution: Array<{ category: string; count: number }>
  }
  recentTraces: TransparencyReport[]
  topOptimizations: OptimizationSuggestion[]
  alerts: TransparencyAlert[]
}

export interface TransparencyAlert {
  id: string
  type: 'low_confidence' | 'performance_issue' | 'cost_spike' | 'error_rate'
  severity: 'low' | 'medium' | 'high' | 'critical'
  message: string
  timestamp: string
  traceId?: string
  resolved: boolean
  actions?: string[]
}
