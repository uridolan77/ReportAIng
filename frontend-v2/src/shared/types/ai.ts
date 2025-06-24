// Core AI Types and Interfaces
// Comprehensive type definitions for AI features

// ============================================================================
// AI TRANSPARENCY TYPES
// ============================================================================



export interface ConfidenceFactor {
  name: string
  score: number
  explanation: string
  impact: 'high' | 'medium' | 'low'
  category: 'context' | 'syntax' | 'semantics' | 'business'
}

export interface ConfidenceAnalysis {
  overallConfidence: number
  factors: ConfidenceFactor[]
  breakdown: {
    contextualRelevance: number
    syntacticCorrectness: number
    semanticClarity: number
    businessAlignment: number
  }
  recommendations: string[]
}

export interface AlternativeOption {
  id: string
  description: string
  confidence: number
  reasoning: string
  tradeoffs: string[]
  estimatedImpact: {
    performance: number
    accuracy: number
    cost: number
  }
}

export interface AIDecisionExplanation {
  decision: string
  reasoning: string[]
  confidence: number
  alternatives: AlternativeOption[]
  factors: ConfidenceFactor[]
  recommendations: string[]
}

// ============================================================================
// STREAMING & REAL-TIME TYPES
// ============================================================================

export interface QueryProcessingProgress {
  phase: 'parsing' | 'understanding' | 'generation' | 'execution' | 'insights' | 'complete' | 'error'
  progress: number
  currentStep: string
  confidence: number
  estimatedTimeRemaining: number
  details?: {
    entitiesFound?: number
    tablesAnalyzed?: number
    rulesApplied?: number
    optimizationsApplied?: number
  }
}

export interface StreamingSession {
  sessionId: string
  query: string
  startTime: string
  currentPhase: QueryProcessingProgress['phase']
  progress: QueryProcessingProgress
  results?: any
  insights?: StreamingInsight[]
  error?: string
  metadata: {
    userId: string
    conversationId?: string
    modelUsed: string
    provider: string
  }
}

export interface StreamingInsight {
  id: string
  type: 'pattern' | 'anomaly' | 'trend' | 'recommendation' | 'warning'
  title: string
  description: string
  confidence: number
  data?: any
  timestamp: string
  relevance: number
}

export interface ProcessingPhaseDetails {
  phase: QueryProcessingProgress['phase']
  startTime: string
  endTime?: string
  duration?: number
  confidence: number
  details: Record<string, any>
  subSteps?: {
    name: string
    status: 'pending' | 'running' | 'complete' | 'error'
    progress: number
  }[]
}

// ============================================================================
// BUSINESS CONTEXT & INTELLIGENCE TYPES
// ============================================================================

export interface BusinessEntity {
  id: string
  name: string
  type: 'table' | 'column' | 'metric' | 'dimension' | 'filter' | 'value' | 'time' | 'person' | 'organization' | 'location'
  startPosition: number
  endPosition: number
  confidence: number
  businessMeaning?: string
  context: string
  relationships?: EntityRelationship[]
  metadata?: Record<string, any>
}

export interface QueryIntent {
  type: 'data_retrieval' | 'aggregation' | 'comparison' | 'trend_analysis' | 'exploration' | 'filtering' | 'ranking' | 'calculation'
  confidence: number
  complexity: 'simple' | 'moderate' | 'complex' | 'very_complex'
  description: string
  businessGoal?: string
  subIntents?: string[]
  reasoning?: string[]
  recommendations?: IntentRecommendation[]
}

export interface BusinessDomain {
  name: string
  description: string
  relevance: number
  concepts: string[]
  relationships: string[]
}

export interface BusinessContextProfile {
  intent: QueryIntent
  entities: BusinessEntity[]
  domain: BusinessDomain
  confidence: number
  businessTerms: string[]
  timeContext?: {
    period: string
    granularity: string
    relativeTo?: string
  }
  userContext?: {
    role: string
    department: string
    accessLevel: string
  }
}

export interface EntityHighlight {
  entityId: string
  startIndex: number
  endIndex: number
  confidence: number
  type: BusinessEntity['type']
  tooltip?: string
  interactive: boolean
}

// ============================================================================
// AI MANAGEMENT TYPES
// ============================================================================

export interface AIProvider {
  id: string
  name: string
  type: 'openai' | 'anthropic' | 'azure' | 'google' | 'local'
  status: 'healthy' | 'degraded' | 'unhealthy' | 'maintenance'
  configuration: {
    apiKey?: string
    endpoint?: string
    model: string
    maxTokens: number
    temperature: number
    timeout: number
  }
  capabilities: string[]
  costPerToken: {
    input: number
    output: number
  }
  limits: {
    requestsPerMinute: number
    tokensPerMinute: number
    dailyLimit?: number
  }
}

export interface ModelPerformanceMetrics {
  modelId: string
  provider: string
  metrics: {
    averageResponseTime: number
    successRate: number
    errorRate: number
    throughput: number
    accuracy?: number
    costEfficiency: number
  }
  benchmarks: {
    queryUnderstanding: number
    sqlGeneration: number
    businessContext: number
    overallScore: number
  }
  trends: {
    timestamp: string
    responseTime: number
    successRate: number
    cost: number
  }[]
}

export interface AIHealthStatus {
  timestamp: string
  overall: 'healthy' | 'degraded' | 'unhealthy'
  providers: {
    providerId: string
    status: AIProvider['status']
    responseTime?: number
    lastCheck: string
    issues?: string[]
  }[]
  systemMetrics: {
    cpuUsage: number
    memoryUsage: number
    activeConnections: number
    queueLength: number
  }
  alerts: {
    id: string
    level: 'info' | 'warning' | 'error' | 'critical'
    message: string
    timestamp: string
    resolved?: boolean
  }[]
}

export interface PromptTemplate {
  id: string
  name: string
  description: string
  category: string
  template: string
  variables: {
    name: string
    type: 'string' | 'number' | 'boolean' | 'array'
    required: boolean
    description: string
    defaultValue?: any
  }[]
  metadata: {
    version: string
    author: string
    createdAt: string
    updatedAt: string
    tags: string[]
  }
  performance: {
    averageScore: number
    usageCount: number
    successRate: number
  }
}

// ============================================================================
// COST & ANALYTICS TYPES
// ============================================================================

export interface CostAnalytics {
  period: {
    start: string
    end: string
  }
  totalCost: number
  breakdown: {
    provider: string
    model: string
    inputTokens: number
    outputTokens: number
    cost: number
    requests: number
  }[]
  trends: {
    date: string
    cost: number
    tokens: number
    requests: number
  }[]
  projections: {
    nextMonth: number
    nextQuarter: number
    confidence: number
  }
  recommendations: {
    type: 'optimization' | 'alert' | 'suggestion'
    message: string
    potentialSavings?: number
  }[]
}

// ============================================================================
// COMMON TYPES
// ============================================================================

export interface AIFeatureFlags {
  transparencyPanelEnabled: boolean
  streamingProcessingEnabled: boolean
  advancedAnalyticsEnabled: boolean
  llmManagementEnabled: boolean
  businessContextEnabled: boolean
  costOptimizationEnabled: boolean
  predictiveAnalyticsEnabled: boolean
}

export interface AIError {
  code: string
  message: string
  details?: Record<string, any>
  timestamp: string
  recoverable: boolean
  suggestions?: string[]
}

export interface AIPerformanceMetrics {
  componentName: string
  renderTime: number
  memoryUsage: number
  apiCalls: number
  cacheHits: number
  errors: number
  timestamp: string
}

// ============================================================================
// EXTENDED BUSINESS INTELLIGENCE TYPES
// ============================================================================

export interface EntityRelationship {
  relatedEntity: string
  relationshipType: 'synonym' | 'related' | 'parent' | 'child' | 'opposite'
  strength: number
  description?: string
}

export interface IntentAlternative {
  id: string
  type: string
  confidence: number
  description: string
  reasoning?: string
}

export interface ProcessFlowConfidenceFactor {
  factor: string
  impact: number
  description: string
}

export interface IntentConfidenceBreakdown {
  factors: ProcessFlowConfidenceFactor[]
  overallScore: number
}

export interface IntentRecommendation {
  type: 'optimization' | 'clarification' | 'alternative'
  title: string
  description: string
}

export interface QueryUnderstandingResult {
  understandingId: string
  originalQuery: string
  intent: QueryIntent
  entities: BusinessEntity[]
  complexity: QueryComplexity
  confidence: number
  processingTime: number
  processingSteps: QueryProcessingStep[]
  suggestions: QuerySuggestion[]
  metadata: Record<string, any>
}

export interface QueryProcessingStep {
  id: string
  name: string
  description: string
  status: 'pending' | 'processing' | 'completed' | 'error'
  confidence: number
  startTime?: string
  endTime?: string
  duration?: number
  reasoning?: string
  alternatives?: string[]
  metadata?: Record<string, any>
}

export interface QueryComplexity {
  level: 'simple' | 'moderate' | 'complex' | 'very_complex'
  factors: string[]
  score: number
}

export interface QuerySuggestion {
  type: 'optimization' | 'clarification' | 'alternative' | 'enhancement'
  title: string
  description: string
  suggestedQuery?: string
  confidence: number
  impact: 'low' | 'medium' | 'high'
}

export interface SchemaContext {
  confidence: number
  tables: TableMetadata[]
  businessTerms: BusinessTerm[]
  suggestions: string[]
}

export interface TableMetadata {
  name: string
  description: string
  businessPurpose?: string
  relevanceScore: number
  columns: ColumnMetadata[]
  keyColumns: string[]
  relationships: TableRelationship[]
  estimatedRowCount?: number
  lastUpdated?: string
}

export interface ColumnMetadata {
  name: string
  type: string
  description?: string
  businessMeaning?: string
  nullable: boolean
  isPrimaryKey: boolean
  isForeignKey: boolean
}

export interface TableRelationship {
  relatedTable: string
  relationshipType: 'one-to-one' | 'one-to-many' | 'many-to-many'
  strength: number
  description?: string
}

export interface BusinessTerm {
  term: string
  definition: string
  category: string
  synonyms: string[]
  relatedTerms: string[]
}

export interface TermRelationship {
  fromTerm: string
  toTerm: string
  relationshipType: 'synonym' | 'related' | 'parent' | 'child' | 'opposite'
  strength?: number
}

export interface TermCategory {
  name: string
  description: string
  termCount: number
}

export interface TimeContext {
  period: string
  granularity: string
  relativeTo?: string
}
