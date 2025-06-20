/**
 * Core UI Components - Modern Reusable System
 *
 * This is the new consolidated component system that replaces all scattered UI components.
 * All components follow modern React patterns with compound components, proper TypeScript,
 * and consistent design system integration.
 */

// Base Components
export { Button, IconButton, ButtonGroup } from './Button'
export { Chart } from './Chart'
export { SqlEditor } from './SqlEditor'
export { AppLayout, PageLayout } from './Layout'

// Dashboard Components (NEW - Consolidated from multiple dashboard implementations)
export {
  Dashboard,
  DashboardLayout,
  DashboardWidget,
  MetricGrid,
  ChartSection,
  MetricWidget,
  KPIWidget,
  useDashboardRefresh,
  useDashboardAlerts,
  useDashboardMetrics,
} from '../dashboard'
export type {
  DashboardLayoutProps,
  DashboardWidgetProps,
  MetricGridProps,
  ChartSectionProps,
  DashboardMetric,
} from '../dashboard'

// Performance Components (NEW - High-performance components for large datasets)
export {
  VirtualDataTable,
  PerformantChart,
  EnhancedErrorBoundary,
  useThrottledData,
  useChartResize,
  useChartAnimation,
  useDataSampling,
  useChartPerformance,
  useChartDataManager,
  useChartZoom,
  useChartColors,
  useDataProcessor,
  createBIQueryClient,
  BackgroundSync,
  queryKeys,
  performanceMonitor,
} from '../performance'
export type {
  VirtualDataTableProps,
  VirtualDataTableColumn,
  PerformantChartProps,
  ChartDataPoint,
  CacheTier,
  CacheConfig,
} from '../performance'

// Advanced Components
export { MonacoSQLEditor, useMonacoSQL } from './MonacoSQLEditor'
export { ExportManager, useExport } from './ExportManager'
export { VirtualTable, VirtualList, useVirtualScroll } from './VirtualTable'

// Design System
export { designTokens, generateCSSVariables } from './design-system'
export type { ColorKey, SpacingKey, FontSizeKey, BorderRadiusKey, ShadowKey, ComponentSize } from './design-system'

// Re-export commonly used types
export type { ButtonProps, IconButtonProps, ButtonGroupProps } from './Button'
