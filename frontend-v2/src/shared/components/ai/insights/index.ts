// AI Insights Components
// Advanced AI-powered insights, schema intelligence, and predictive analytics

export { default as AISchemaExplorer } from './AISchemaExplorer'
export type { AISchemaExplorerProps } from './AISchemaExplorer'

export { default as QueryOptimizationEngine } from './QueryOptimizationEngine'
export type { QueryOptimizationEngineProps } from './QueryOptimizationEngine'

export { default as AutomatedInsightsGenerator } from './AutomatedInsightsGenerator'
export type { AutomatedInsightsGeneratorProps } from './AutomatedInsightsGenerator'

export { default as PredictiveAnalyticsPanel } from './PredictiveAnalyticsPanel'
export type { PredictiveAnalyticsPanelProps } from './PredictiveAnalyticsPanel'

// Re-export related types
export type {
  SchemaNode,
  SchemaRecommendation,
  QueryOptimization,
  OptimizationSuggestion,
  AutomatedInsight,
  InsightCategory,
  PredictiveModel,
  Forecast,
  TrendAnalysis
} from '@shared/types/intelligentAgents'
