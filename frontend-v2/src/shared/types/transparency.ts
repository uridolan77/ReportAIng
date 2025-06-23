// ProcessFlow Transparency API Types
// Clean ProcessFlow system interfaces - no legacy code

// Re-export ProcessFlow analytics types
export * from './processFlowAnalytics'

// ProcessFlow System Core Interfaces
export interface ProcessFlowSession {
  sessionId: string
  userId: string
  userQuery: string
  queryType: string
  status: 'NotStarted' | 'InProgress' | 'Completed' | 'Failed' | 'Cancelled'
  startTime: string
  endTime?: string
  totalDurationMs?: number
  overallConfidence?: number
  generatedSQL?: string
  executionResult?: string
  conversationId?: string
  messageId?: string

  // Related data
  steps: ProcessFlowStep[]
  logs: ProcessFlowLog[]
  transparency?: ProcessFlowTransparency
}

export interface ProcessFlowStep {
  stepId: string
  sessionId: string
  stepType: 'SemanticAnalysis' | 'SchemaRetrieval' | 'PromptBuilding' | 'AIGeneration' | 'SQLExecution'
  name: string
  status: 'NotStarted' | 'InProgress' | 'Completed' | 'Failed' | 'Skipped'
  startTime: string
  endTime?: string
  durationMs?: number
  confidence?: number
  inputData?: any
  outputData?: any
  errorMessage?: string
}

export interface ProcessFlowLog {
  logId: string
  sessionId: string
  stepId?: string
  level: 'Debug' | 'Info' | 'Warning' | 'Error'
  message: string
  timestamp: string
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

// ProcessFlow Request Types
export interface ProcessFlowAnalyzeRequest {
  userQuestion: string
  context?: Record<string, any>
  includeAlternatives?: boolean
  detailLevel?: 'basic' | 'detailed' | 'comprehensive'
  sessionId?: string
}

export interface ProcessFlowMetricsFilters {
  userId?: string
  days?: number
  startDate?: string
  endDate?: string
  includeDetails?: boolean
}

// Enhanced Query Response with ProcessFlow data
export interface EnhancedQueryTransparencyData {
  traceId: string
  businessContext: {
    intent: string
    confidence: number
    domain: string
    entities: string[]
  }
  tokenUsage: {
    allocatedTokens: number
    estimatedCost: number
    provider: string
  }
  processingSteps: number
}



// Enhanced Query Response with ProcessFlow data
export interface EnhancedQueryTransparencyData {
  traceId: string
  businessContext: {
    intent: string
    confidence: number
    domain: string
    entities: string[]
  }
  tokenUsage: {
    allocatedTokens: number
    estimatedCost: number
    provider: string
  }
  processingSteps: number
}



// NEW: ProcessFlow Request types
export interface ProcessFlowAnalyzeRequest {
  userQuestion: string
  context?: Record<string, any>
  includeAlternatives?: boolean
  detailLevel?: 'basic' | 'detailed' | 'comprehensive'
  sessionId?: string
}

export interface ProcessFlowMetricsFilters {
  userId?: string
  days?: number
  startDate?: string
  endDate?: string
  includeDetails?: boolean
}


