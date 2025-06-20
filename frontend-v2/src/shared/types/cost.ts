// Cost Management Types
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

export interface CostHistoryResponse {
  history: CostHistoryItem[]
  total: number
  page: number
  limit: number
}

export interface CostHistoryItem {
  id: string
  timestamp: string
  amount: number
  category: string
  provider: string
  userId: string
  queryId?: string
  metadata: Record<string, any>
}

export interface CostBreakdownResponse {
  dimension: string
  breakdown: CostBreakdownItem[]
  total: number
  generatedAt: string
}

export interface CostBreakdownItem {
  label: string
  value: number
  percentage: number
  trend?: number
}

export interface CostTrendsResponse {
  trends: CostTrend[]
  summary: {
    totalPeriods: number
    averageCost: number
    trendDirection: 'up' | 'down' | 'stable'
    changePercentage: number
  }
}

// Budget Management Types
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

export interface BudgetsResponse {
  budgets: BudgetManagement[]
  total: number
}

export interface BudgetResponse {
  budget: BudgetManagement
}

export interface CreateBudgetRequest {
  name: string
  type: string
  entityId: string
  budgetAmount: number
  period: BudgetPeriod
  startDate: string
  endDate: string
  alertThreshold: number
  blockThreshold: number
}

export interface UpdateBudgetRequest extends Partial<CreateBudgetRequest> {
  isActive?: boolean
}

// Cost Predictions & Recommendations
export interface CostPredictionRequest {
  timeframe: number
  factors?: string[]
  includeSeasonality?: boolean
}

export interface CostPredictionResponse {
  prediction: number
  confidence: number
  factors: PredictionFactor[]
  timeframe: number
  generatedAt: string
}

export interface PredictionFactor {
  name: string
  impact: number
  confidence: number
}

export interface CostForecastResponse {
  forecast: ForecastItem[]
  accuracy: number
  methodology: string
}

export interface ForecastItem {
  date: string
  predictedCost: number
  confidence: number
  factors: string[]
}

export interface RecommendationsResponse {
  recommendations: CostRecommendation[]
  totalPotentialSavings: number
}

export interface CostRecommendation {
  id: string
  type: string
  title: string
  description: string
  potentialSavings: number
  effort: 'Low' | 'Medium' | 'High'
  impact: 'Low' | 'Medium' | 'High'
  category: string
  actionItems: string[]
}

export interface ROIAnalysisResponse {
  totalInvestment: number
  totalReturns: number
  roi: number
  roiPercentage: number
  paybackPeriod: number
  breakdownByCategory: Record<string, ROIBreakdown>
}

export interface ROIBreakdown {
  investment: number
  returns: number
  roi: number
}

export interface RealTimeMetricsResponse {
  currentCost: number
  costPerMinute: number
  activeQueries: number
  costPerQuery: number
  efficiency: number
  alerts: CostAlert[]
  lastUpdated: string
}

export interface CostAlert {
  id: string
  type: 'budget_exceeded' | 'cost_spike' | 'efficiency_low'
  severity: 'low' | 'medium' | 'high' | 'critical'
  message: string
  threshold: number
  currentValue: number
  timestamp: string
}

// Time Range Types
export type TimeRange = '1h' | '24h' | '7d' | '30d' | '90d' | '1y'

export interface DateRange {
  startDate: string
  endDate: string
}
