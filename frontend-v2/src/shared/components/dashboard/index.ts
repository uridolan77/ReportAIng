/**
 * Dashboard Components Export
 * 
 * Unified dashboard component system that consolidates patterns from:
 * - Admin Dashboard (system statistics)
 * - Cost Dashboard (cost metrics)
 * - API Status Dashboard (status monitoring)
 * - Real-time Dashboard (live metrics)
 */

// Main compound component
export { default as Dashboard } from './Dashboard'

// Layout components
export {
  DashboardLayout,
  MetricGrid,
  ChartSection,
  type DashboardSection,
  type DashboardLayoutProps,
  type MetricGridProps,
  type ChartSectionProps,
} from './DashboardLayout'

// Widget components
export {
  DashboardWidget,
  MetricWidget,
  KPIWidget,
  type DashboardWidgetProps,
} from './DashboardWidget'

// Hooks and utilities
export {
  useDashboardRefresh,
  useDashboardAlerts,
  useDashboardMetrics,
  type DashboardMetric,
} from './Dashboard'

// Re-export examples for documentation
export { DashboardExamples } from './Dashboard'
