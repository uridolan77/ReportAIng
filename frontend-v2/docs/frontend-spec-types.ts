// shared/types/cost.ts
export interface CostAnalyticsSummary {
  totalCost: number
  dailyCost: number
  weeklyCost: number
  monthlyCost: number
  costByProvider: Record<string, number>
  costByUser: Record<string, number>
  costByDepartment: Record<string, number>
  costByModel: Record<string, number>
  trends: CostTrend[]
  generatedAt: string
  costEfficiency?: number
  predictedMonthlyCost?: number
  costSavingsOpportunities?: number
  roiMetrics?: Record<string, any>
}

export interface CostTrend {
  date: string
  amount: number
  category: string
  metadata: Record<string, any>
}

export interface BudgetManagement {
  id: string
  name: string
  type: string
  entityId: string
  budgetAmount: number
  spentAmount: number
  remainingAmount: number
  period: BudgetPeriod
  startDate: string
  endDate: string
  alertThreshold: number
  blockThreshold: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export enum BudgetPeriod {
  Daily = 'Daily',
  Weekly = 'Weekly',
  Monthly = 'Monthly',
  Quarterly = 'Quarterly',
  Yearly = 'Yearly'
}

// shared/types/performance.ts
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

// shared/types/cache.ts
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

// shared/types/resource.ts
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
