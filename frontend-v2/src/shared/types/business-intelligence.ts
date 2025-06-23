// Business Intelligence Types
// Types for the production Business Intelligence system

// ============================================================================
// REQUEST/RESPONSE TYPES
// ============================================================================

export interface QueryAnalysisRequest {
  query: string
  userId: string
  context: {
    userRole: string
    department: string
    accessLevel: string
    timezone: string
  }
  options: {
    includeEntityDetails: boolean
    includeAlternatives: boolean
    includeSuggestions: boolean
  }
}

export interface QueryAnalysisResponse {
  success: boolean
  data: {
    queryId: string
    analysisTimestamp: string
    processingTimeMs: number
    businessContext: BusinessContextProfile
    suggestedQueries: QuerySuggestion[]
    estimatedExecutionTime: number
    dataQualityScore: number
  }
  error?: BusinessIntelligenceError
}

export interface BusinessIntelligenceError {
  code: string
  message: string
  details: Record<string, any>
  timestamp: string
  requestId: string
}

// ============================================================================
// BUSINESS CONTEXT TYPES
// ============================================================================

export interface BusinessContextProfile {
  id?: string
  confidence: number
  intent: QueryIntent
  entities: BusinessEntity[]
  domain: BusinessDomain
  businessTerms: string[]
  timeContext?: TimeContext
  userContext?: UserContext
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
  recommendedActions?: string[]
}

export interface BusinessEntity {
  id: string
  name: string
  type: 'table' | 'column' | 'metric' | 'dimension' | 'filter' | 'value' | 'time' | 'person' | 'organization' | 'location'
  startPosition: number
  endPosition: number
  confidence: number
  businessMeaning?: string
  context: string
  suggestedColumns?: string[]
  relatedTables?: string[]
  relationships?: EntityRelationship[]
  synonyms?: string[]
  dataType?: string
  aggregationMethods?: string[]
  metadata?: Record<string, any>
}

export interface EntityRelationship {
  relatedEntity: string
  relationshipType: 'synonym' | 'related' | 'parent' | 'child' | 'opposite'
  strength: number
  description?: string
}

export interface BusinessDomain {
  name: string
  description: string
  relevance: number
  concepts: string[]
  relationships: string[]
}

export interface TimeContext {
  period: string
  granularity: string
  relativeTo?: string
  startDate?: string
  endDate?: string
}

export interface UserContext {
  role: string
  department: string
  accessLevel: string
  preferences?: {
    defaultTimeRange?: string
    preferredVisualization?: string
    [key: string]: any
  }
}

// ============================================================================
// QUERY SUGGESTIONS & HISTORY
// ============================================================================

export interface QuerySuggestion {
  id: string
  query: string
  category: 'popular' | 'recent' | 'recommended' | 'similar'
  confidence: number
  description: string
  estimatedComplexity: 'simple' | 'moderate' | 'complex'
  tags: string[]
  improvement?: string
}

export interface QueryHistoryItem {
  queryId: string
  query: string
  timestamp: string
  confidence: number
  executionTime: number
  intent: string
  entities: string[]
  success: boolean
}

// ============================================================================
// SCHEMA INTELLIGENCE TYPES
// ============================================================================

export interface SchemaContext {
  confidence: number
  relevantTables: TableInfo[]
  suggestedJoins: JoinSuggestion[]
  optimizationTips: OptimizationTip[]
  potentialIssues: DataIssue[]
}

export interface TableInfo {
  name: string
  relevanceScore: number
  description: string
  businessPurpose: string
  columns: ColumnInfo[]
  relationships: TableRelationship[]
  estimatedRowCount: number
  lastUpdated: string
  dataQuality: DataQuality
}

export interface ColumnInfo {
  name: string
  type: string
  description: string
  businessMeaning: string
  nullable: boolean
  isPrimaryKey: boolean
  isForeignKey: boolean
  relevanceScore: number
}

export interface TableRelationship {
  relatedTable: string
  relationshipType: 'one-to-one' | 'one-to-many' | 'many-to-one' | 'many-to-many'
  strength: number
  description: string
  joinCondition: string
}

export interface JoinSuggestion {
  tables: string[]
  joinType: string
  condition: string
  confidence: number
  reasoning: string
}

export interface OptimizationTip {
  type: 'indexing' | 'partitioning' | 'caching' | 'query_rewrite'
  suggestion: string
  impact: 'low' | 'medium' | 'high'
  estimatedImprovement: string
}

export interface DataIssue {
  type: 'data_quality' | 'performance' | 'access' | 'completeness'
  description: string
  severity: 'low' | 'medium' | 'high'
  recommendation: string
}

export interface DataQuality {
  completeness: number
  accuracy: number
  consistency: number
}

// ============================================================================
// BUSINESS TERMS TYPES
// ============================================================================

export interface BusinessTerm {
  id: string
  name: string
  definition: string
  category: string
  aliases: string[]
  relatedTerms: TermRelationship[]
  businessContext: string
  dataLineage: DataLineageItem[]
  lastUpdated: string
  approvedBy: string
  version: string
  status: 'draft' | 'approved' | 'deprecated'
}

export interface TermRelationship {
  termId: string
  name: string
  relationshipType: 'synonym' | 'related' | 'parent' | 'child' | 'opposite'
  strength: number
  bidirectional?: boolean
  description?: string
}

export interface DataLineageItem {
  table: string
  column: string
  transformation?: string
}

export interface TermCategory {
  name: string
  description: string
  termCount: number
}

// ============================================================================
// PERFORMANCE & ANALYTICS TYPES
// ============================================================================

export interface PerformanceMetrics {
  averageResponseTime: number
  p95ResponseTime: number
  p99ResponseTime: number
  errorRate: number
  cacheHitRate: number
}

export interface UsageAnalytics {
  summary: {
    totalQueries: number
    uniqueUsers: number
    averageConfidence: number
    averageResponseTime: number
    successRate: number
  }
  trends: {
    dailyQueries: any[]
    popularIntents: any[]
    topEntities: any[]
  }
  performance: PerformanceMetrics
}

// ============================================================================
// QUERY UNDERSTANDING TYPES
// ============================================================================

export interface QueryUnderstandingResult {
  queryId: string
  steps: ProcessingStep[]
  alternatives: IntentAlternative[]
  confidenceBreakdown: ConfidenceBreakdown
  suggestions: QuerySuggestion[]
  metadata: Record<string, any>
}

export interface ProcessingStep {
  step: number
  title: string
  description: string
  status: 'pending' | 'processing' | 'completed' | 'failed'
  confidence: number
  processingTimeMs: number
  details: Record<string, any>
}

export interface IntentAlternative {
  id: string
  type: string
  confidence: number
  description: string
  reasoning: string
  tradeoffs?: string[]
}

export interface ProcessFlowConfidenceBreakdown {
  overallConfidence: number
  factors: ProcessFlowConfidenceFactor[]
}

export interface ProcessFlowConfidenceFactor {
  name: string
  score: number
  impact: 'low' | 'medium' | 'high'
  explanation: string
}
