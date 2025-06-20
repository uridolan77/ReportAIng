// Performance Monitoring Types
export interface PerformanceMetrics {
  averageResponseTime: number
  throughputPerSecond: number
  errorRate: number
  p95ResponseTime: number
  p99ResponseTime: number
  totalRequests: number
  successfulRequests: number
  failedRequests: number
  performanceScore: number
  lastAnalyzed: string
}

export interface PerformanceBottleneck {
  id: string
  type: string
  entityId: string
  entityType: string
  description: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  impactScore: number
  recommendations: string[]
  detectedAt: string
}

export interface PerformanceAnalysisResponse {
  metrics: PerformanceMetrics
  bottlenecks: PerformanceBottleneck[]
  score: number
  recommendations: string[]
  analyzedAt: string
}

export interface BottlenecksResponse {
  bottlenecks: PerformanceBottleneck[]
  total: number
  criticalCount: number
  highCount: number
}

export interface SuggestionsResponse {
  suggestions: OptimizationSuggestion[]
  priorityOrder: string[]
  estimatedImpact: number
}

export interface OptimizationSuggestion {
  id: string
  type: string
  title: string
  description: string
  impact: 'Low' | 'Medium' | 'High'
  effort: 'Low' | 'Medium' | 'High'
  category: string
  steps: string[]
  estimatedImprovement: number
}

export interface TuningResultResponse {
  success: boolean
  changes: TuningChange[]
  beforeMetrics: PerformanceMetrics
  afterMetrics: PerformanceMetrics
  improvement: number
}

export interface TuningChange {
  parameter: string
  oldValue: any
  newValue: any
  reason: string
}

export interface BenchmarksResponse {
  benchmarks: Benchmark[]
  categories: string[]
}

export interface Benchmark {
  id: string
  name: string
  category: string
  description: string
  targetValue: number
  currentValue: number
  status: 'passing' | 'warning' | 'failing'
  lastRun: string
}

export interface BenchmarkResponse {
  benchmark: Benchmark
}

export interface CreateBenchmarkRequest {
  name: string
  category: string
  description: string
  targetValue: number
  metricType: string
}

export interface MetricsResponse {
  metrics: PerformanceMetricsEntry[]
  aggregations: MetricAggregation
}

export interface PerformanceMetricsEntry {
  id: string
  metricName: string
  value: number
  timestamp: string
  entityType: string
  entityId: string
  tags: Record<string, string>
}

export interface MetricAggregation {
  average: number
  min: number
  max: number
  count: number
  sum: number
  percentiles: Record<string, number>
}

export interface AlertsResponse {
  alerts: PerformanceAlert[]
  activeCount: number
  resolvedCount: number
}

export interface PerformanceAlert {
  id: string
  type: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  title: string
  description: string
  entityType: string
  entityId: string
  threshold: number
  currentValue: number
  status: 'active' | 'resolved' | 'acknowledged'
  createdAt: string
  resolvedAt?: string
  resolutionNotes?: string
}

// Cache Statistics Types
export interface CacheStatistics {
  cacheType: string
  totalOperations: number
  hitCount: number
  missCount: number
  setCount: number
  deleteCount: number
  hitRate: number
  averageResponseTime: number
  totalSizeBytes: number
  lastUpdated: string
  periodStart: string
  periodEnd: string
}

// Resource Quota Types
export interface ResourceQuota {
  id: string
  userId: string
  resourceType: string
  maxQuantity: number
  currentUsage: number
  period: string
  resetDate: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

// Performance Entity Types
export type PerformanceEntityType = 'query' | 'user' | 'system' | 'database' | 'api'

export interface PerformanceEntity {
  type: PerformanceEntityType
  id: string
  name: string
  description?: string
}

// Performance Monitoring Configuration
export interface MonitoringConfig {
  enabled: boolean
  samplingRate: number
  alertThresholds: Record<string, number>
  retentionDays: number
  autoTuningEnabled: boolean
}
