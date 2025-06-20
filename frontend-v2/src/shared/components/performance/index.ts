/**
 * Performance Components Export
 * 
 * High-performance components optimized for large datasets and real-time updates
 */

// Virtual scrolling components
export { VirtualDataTable } from './VirtualDataTable'
export type { VirtualDataTableProps, VirtualDataTableColumn } from './VirtualDataTable'

// Optimized chart components
export { PerformantChart } from './PerformantChart'
export type { PerformantChartProps, ChartDataPoint } from './PerformantChart'

// Enhanced error boundary
export { EnhancedErrorBoundary } from './EnhancedErrorBoundary'

// Performance hooks
export {
  useThrottledData,
  useChartResize,
  useChartAnimation,
  useDataSampling,
  useChartPerformance,
  useChartDataManager,
  useChartZoom,
  useChartColors,
} from '../../hooks/useChartOptimization'

// Data processor hook
export { useDataProcessor } from '../../hooks/useDataProcessor'

// Query client configuration
export {
  createBIQueryClient,
  cacheConfigs,
  getCacheTier,
  BackgroundSync,
  queryKeys,
  performanceMonitor,
} from '../../services/queryClientConfig'
export type { CacheTier, CacheConfig } from '../../services/queryClientConfig'
