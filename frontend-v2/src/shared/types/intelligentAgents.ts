// Intelligent Agents API Types
// Based on the new IntelligentAgentsController.cs backend DTOs

export interface AgentOrchestrationResult {
  orchestrationId: string
  status: 'pending' | 'running' | 'completed' | 'failed' | 'cancelled'
  startTime: string
  endTime?: string
  duration?: number
  tasks: OrchestrationTask[]
  result?: any
  error?: string
  metadata: Record<string, any>
}

export interface OrchestrationRequest {
  taskType: 'schema_analysis' | 'query_generation' | 'data_analysis' | 'insight_generation' | 'complex_query'
  input: {
    query?: string
    context?: Record<string, any>
    requirements?: string[]
    constraints?: Record<string, any>
  }
  options?: {
    timeout?: number
    priority?: 'low' | 'medium' | 'high' | 'critical'
    enableParallelExecution?: boolean
    maxAgents?: number
  }
  userId?: string
}

export interface OrchestrationTask {
  taskId: string
  agentId: string
  agentType: string
  status: 'pending' | 'running' | 'completed' | 'failed' | 'cancelled'
  startTime: string
  endTime?: string
  duration?: number
  input: any
  output?: any
  error?: string
  confidence?: number
  dependencies: string[]
  metadata: Record<string, any>
}

export interface AgentCapabilities {
  agentId: string
  agentType: 'schema_navigator' | 'query_understander' | 'sql_generator' | 'data_analyzer' | 'insight_generator'
  name: string
  description: string
  version: string
  status: 'active' | 'inactive' | 'maintenance' | 'error'
  capabilities: string[]
  supportedTaskTypes: string[]
  performance: {
    averageResponseTime: number
    successRate: number
    throughput: number
    reliability: number
  }
  configuration: {
    maxConcurrentTasks: number
    timeout: number
    retryAttempts: number
    priority: number
  }
  metadata: Record<string, any>
}

export interface SchemaNavigationResult {
  navigationId: string
  query: string
  discoveredTables: SchemaTable[]
  suggestedJoins: SuggestedJoin[]
  businessContext: BusinessContext
  confidence: number
  reasoning: string[]
  alternatives: SchemaAlternative[]
  metadata: Record<string, any>
}

export interface SchemaNavigationRequest {
  query: string
  context?: {
    userId?: string
    previousQueries?: string[]
    businessDomain?: string
    preferredTables?: string[]
  }
  options?: {
    includeViews?: boolean
    maxTables?: number
    confidenceThreshold?: number
    explainReasoning?: boolean
  }
}

export interface SchemaTable {
  schemaName: string
  tableName: string
  relevanceScore: number
  businessPurpose: string
  keyColumns: string[]
  relationships: TableRelationship[]
  estimatedRowCount?: number
  lastUpdated?: string
  metadata: Record<string, any>
}

export interface SuggestedJoin {
  leftTable: string
  rightTable: string
  joinType: 'inner' | 'left' | 'right' | 'full'
  joinCondition: string
  confidence: number
  reasoning: string
  performance: {
    estimatedCost: number
    indexAvailable: boolean
    cardinalityEstimate: number
  }
}

export interface BusinessContext {
  domain: string
  entities: BusinessEntity[]
  relationships: BusinessRelationship[]
  glossaryTerms: GlossaryTerm[]
  confidence: number
}

export interface BusinessEntity {
  name: string
  type: 'dimension' | 'fact' | 'measure' | 'attribute'
  description: string
  mappedColumns: string[]
  confidence: number
}

export interface BusinessRelationship {
  fromEntity: string
  toEntity: string
  relationshipType: 'one-to-one' | 'one-to-many' | 'many-to-many'
  description: string
  strength: number
}

export interface GlossaryTerm {
  term: string
  definition: string
  category: string
  synonyms: string[]
  relatedTerms: string[]
}

export interface SchemaAlternative {
  description: string
  tables: string[]
  confidence: number
  tradeoffs: string[]
  reasoning: string
}

export interface QueryUnderstandingResult {
  understandingId: string
  originalQuery: string
  intent: QueryIntent
  entities: ExtractedEntity[]
  businessContext: BusinessContext
  complexity: QueryComplexity
  suggestions: QuerySuggestion[]
  confidence: number
  processingTime: number
  metadata: Record<string, any>
}

export interface QueryUnderstandingRequest {
  query: string
  context?: {
    userId?: string
    conversationHistory?: string[]
    businessDomain?: string
    userRole?: string
  }
  options?: {
    includeAlternatives?: boolean
    explainReasoning?: boolean
    suggestOptimizations?: boolean
  }
}

export interface QueryIntent {
  primaryIntent: 'data_retrieval' | 'aggregation' | 'comparison' | 'trend_analysis' | 'exploration'
  secondaryIntents: string[]
  confidence: number
  reasoning: string[]
}

export interface ExtractedEntity {
  text: string
  type: 'table' | 'column' | 'value' | 'function' | 'operator' | 'keyword'
  startPosition: number
  endPosition: number
  confidence: number
  businessMeaning?: string
  suggestions?: string[]
}

export interface QueryComplexity {
  level: 'simple' | 'moderate' | 'complex' | 'very_complex'
  factors: ComplexityFactor[]
  score: number
  estimatedExecutionTime?: number
  resourceRequirements?: ResourceRequirement[]
}

export interface ComplexityFactor {
  factor: string
  impact: 'low' | 'medium' | 'high'
  description: string
  score: number
}

export interface ResourceRequirement {
  resource: 'cpu' | 'memory' | 'io' | 'network'
  level: 'low' | 'medium' | 'high'
  description: string
}

export interface QuerySuggestion {
  type: 'optimization' | 'clarification' | 'alternative' | 'enhancement'
  title: string
  description: string
  suggestedQuery?: string
  confidence: number
  impact: 'low' | 'medium' | 'high'
}

export interface AgentCommunicationLog {
  logId: string
  timestamp: string
  fromAgentId: string
  toAgentId?: string
  messageType: 'request' | 'response' | 'notification' | 'error'
  content: any
  orchestrationId?: string
  taskId?: string
  metadata: Record<string, any>
}

export interface AgentPerformanceMetrics {
  agentId: string
  timeRange: {
    start: string
    end: string
  }
  metrics: {
    totalTasks: number
    completedTasks: number
    failedTasks: number
    averageResponseTime: number
    successRate: number
    throughput: number
    errorRate: number
  }
  trends: {
    responseTimeOverTime: Array<{ timestamp: string; avgTime: number }>
    successRateOverTime: Array<{ timestamp: string; rate: number }>
    throughputOverTime: Array<{ timestamp: string; throughput: number }>
  }
  topErrors: Array<{ error: string; count: number }>
  resourceUtilization: {
    cpu: number
    memory: number
    network: number
  }
}

export interface AgentHealthStatus {
  agentId: string
  agentType: string
  status: 'healthy' | 'degraded' | 'unhealthy' | 'offline'
  lastHeartbeat: string
  uptime: number
  currentLoad: number
  queueLength: number
  activeConnections: number
  errors: AgentError[]
  warnings: AgentWarning[]
  metadata: Record<string, any>
}

export interface AgentError {
  errorId: string
  timestamp: string
  severity: 'low' | 'medium' | 'high' | 'critical'
  message: string
  details?: string
  resolved: boolean
  resolution?: string
}

export interface AgentWarning {
  warningId: string
  timestamp: string
  type: 'performance' | 'resource' | 'configuration' | 'connectivity'
  message: string
  threshold?: number
  currentValue?: number
  acknowledged: boolean
}

// Dashboard and analytics types
export interface AgentDashboardData {
  summary: {
    totalAgents: number
    activeAgents: number
    totalTasks: number
    successRate: number
    averageResponseTime: number
  }
  agentStatus: AgentHealthStatus[]
  recentOrchestrations: AgentOrchestrationResult[]
  performanceMetrics: AgentPerformanceMetrics[]
  alerts: AgentAlert[]
  systemHealth: {
    overall: 'healthy' | 'degraded' | 'unhealthy'
    components: Array<{ name: string; status: string }>
  }
}

export interface AgentAlert {
  alertId: string
  type: 'performance' | 'error' | 'resource' | 'connectivity'
  severity: 'low' | 'medium' | 'high' | 'critical'
  agentId?: string
  message: string
  timestamp: string
  acknowledged: boolean
  resolved: boolean
  actions?: string[]
}

// Configuration types
export interface AgentConfiguration {
  agentId: string
  settings: {
    enabled: boolean
    maxConcurrentTasks: number
    timeout: number
    retryAttempts: number
    priority: number
    resourceLimits: {
      maxMemory: number
      maxCpu: number
    }
  }
  features: {
    enableLogging: boolean
    enableMetrics: boolean
    enableTracing: boolean
  }
}

// ============================================================================
// ADVANCED AI INSIGHTS TYPES
// ============================================================================

export interface SchemaNode {
  name: string
  type: 'schema' | 'table' | 'column'
  description?: string
  businessPurpose?: string
  relevanceScore: number
  estimatedRowCount?: number
  isPrimaryKey?: boolean
  isForeignKey?: boolean
  relationships?: TableRelationship[]
  columns?: SchemaNode[]
  tables?: SchemaNode[]
}

export interface SchemaRecommendation {
  id: string
  type: 'join' | 'filter' | 'aggregation' | 'index'
  title: string
  description: string
  reasoning?: string
  confidence: number
  impact: 'low' | 'medium' | 'high'
  suggestedQuery?: string
}

export interface QueryOptimization {
  id: string
  type: 'index' | 'join' | 'filter' | 'aggregation' | 'subquery'
  description: string
  optimizedQuery: string
  confidence: number
  expectedImprovement: number
  appliedAt: string
}

export interface OptimizationSuggestion {
  type: string
  title: string
  description: string
  reasoning?: string
  confidence: number
  expectedImprovement: number
  costImpact?: number
  complexityReduction?: number
  impact: 'low' | 'medium' | 'high'
  optimizedQuery: string
}

export interface PerformanceMetrics {
  executionTime: number
  complexityScore: number
  resourceUsage: number
}

export interface QueryAnalysis {
  bottlenecks: Array<{
    type: string
    description: string
  }>
  insights: string[]
}

export interface AutomatedInsight {
  id: string
  title: string
  description: string
  category: InsightCategory
  priority: 'low' | 'medium' | 'high' | 'critical'
  confidence: number
  actionable: boolean
  impact?: string
  recommendation?: string
  trending?: boolean
  generatedAt: string
}

export type InsightCategory = 'performance' | 'cost' | 'usage' | 'anomaly' | 'trend' | 'optimization' | 'all'

export interface InsightTrend {
  date: string
  insightCount: number
  actionableCount: number
}

export interface PredictiveModel {
  id: string
  name: string
  type: 'arima' | 'lstm' | 'prophet' | 'auto'
  accuracy: number
  description: string
}

export interface Forecast {
  predictions: Array<{
    value: number
    confidence: number
    upperBound: number
    lowerBound: number
  }>
  confidence: number
  model: string
  generatedAt: string
}

export interface PredictionMetrics {
  accuracy: number
  mape: number
  rmse: number
  trend: 'increasing' | 'decreasing' | 'stable'
  seasonality: boolean
  volatility: number
}

export interface TrendAnalysis {
  patterns: Array<{
    name: string
    type: 'increasing' | 'decreasing' | 'stable' | 'seasonal'
    confidence: number
    description: string
    duration: string
    impact: string
  }>
}

export interface AnomalyDetection {
  title: string
  description: string
  severity: 'low' | 'medium' | 'high'
  detectedAt: string
  confidence: number
}

// Export utility types
export type AgentType = AgentCapabilities['agentType']
export type OrchestrationStatus = AgentOrchestrationResult['status']
export type TaskStatus = OrchestrationTask['status']
export type AgentStatus = AgentCapabilities['status']
