/**
 * ProcessFlow Analytics Components Export
 * 
 * Comprehensive analytics components for the ProcessFlow system including:
 * - Token usage analytics and visualization
 * - Performance metrics and monitoring
 * - Cost analysis and optimization
 * - Real-time dashboard components
 */

// Main dashboard components
export { default as ProcessFlowAnalyticsDashboard } from './ProcessFlowAnalyticsDashboard'
export type { ProcessFlowAnalyticsDashboardProps } from './ProcessFlowAnalyticsDashboard'

// Token usage components
export { default as ProcessFlowTokenUsage } from './ProcessFlowTokenUsage'
export type { ProcessFlowTokenUsageProps } from './ProcessFlowTokenUsage'

// Re-export transparency components for convenience
export { default as ProcessFlowSessionViewer } from '../ai/transparency/ProcessFlowSessionViewer'
export type { ProcessFlowSessionViewerProps } from '../ai/transparency/ProcessFlowSessionViewer'

// Re-export updated token analyzer
export { default as TokenUsageAnalyzer } from '../ai/transparency/TokenUsageAnalyzer'
export type { TokenUsageAnalyzerProps } from '../ai/transparency/TokenUsageAnalyzer'

/**
 * Convenience hooks for analytics components
 */
export {
  useTokenUsageDashboard,
  useAnalyticsDashboard,
  usePerformanceOverview,
  useGetTokenUsageStatisticsQuery,
  useGetDailyTokenUsageQuery,
  useGetTokenUsageTrendsQuery,
  useGetTopTokenUsersQuery,
  useGetSessionAnalyticsQuery,
  useGetStepAnalyticsQuery,
  useGetQueryAnalyticsQuery,
  useGetTransparencyAnalyticsQuery,
  useGetRealTimeMetricsQuery,
  useGetComprehensiveAnalyticsQuery,
  useGetAnalyticsDashboardQuery,
  useExportAnalyticsDataMutation,
  useGetPerformanceAnalyticsQuery,
  useGetCostAnalyticsQuery,
  useGetErrorAnalyticsQuery
} from '@shared/store/api/analyticsApi'

/**
 * ProcessFlow transparency hooks
 */
export {
  useProcessFlowSession,
  useProcessFlowDashboard,
  useProcessFlowManagement,
  useGetProcessFlowSessionQuery,
  useGetProcessFlowAnalyticsQuery,
  useGetProcessFlowDashboardQuery,
  useExportProcessFlowDataMutation,
  useAnalyzePromptConstructionMutation
} from '@shared/store/api/transparencyApi'

/**
 * Enhanced query hooks with ProcessFlow support
 */
export {
  useProcessFlowQuery,
  useQueryWithTransparency,
  useExecuteProcessFlowQueryMutation,
  useGetQueryProcessFlowSessionQuery
} from '@shared/store/api/queryApi'
