/**
 * Enhanced TypeScript Types for Dashboard Components
 * 
 * Provides comprehensive type safety for the unified dashboard system
 * with branded types and discriminated unions for better type checking.
 */

// Branded types for ID validation
export type DashboardId = string & { readonly __brand: 'DashboardId' }
export type WidgetId = string & { readonly __brand: 'WidgetId' }
export type ChartId = string & { readonly __brand: 'ChartId' }
export type UserId = string & { readonly __brand: 'UserId' }

// Helper functions to create branded types
export const createDashboardId = (id: string): DashboardId => id as DashboardId
export const createWidgetId = (id: string): WidgetId => id as WidgetId
export const createChartId = (id: string): ChartId => id as ChartId
export const createUserId = (id: string): UserId => id as UserId

// Chart type discriminated union
export type ChartType = 'line' | 'bar' | 'pie' | 'area' | 'scatter' | 'gauge'

export interface BaseChartData {
  id: ChartId
  type: ChartType
  title: string
  description?: string
}

export interface TimeSeriesData {
  timestamp: string
  value: number
  label?: string
}

export interface CategoryData {
  category: string
  value: number
  color?: string
}

export interface PieSliceData {
  name: string
  value: number
  color?: string
}

export interface ScatterPointData {
  x: number
  y: number
  label?: string
  size?: number
}

export interface GaugeData {
  value: number
  min: number
  max: number
  target?: number
  unit?: string
}

// Chart data mapping with discriminated unions
export type ChartDataMap = {
  line: TimeSeriesData[]
  bar: CategoryData[]
  pie: PieSliceData[]
  area: TimeSeriesData[]
  scatter: ScatterPointData[]
  gauge: GaugeData
}

export interface ChartConfig<T extends ChartType> {
  type: T
  data: ChartDataMap[T]
  options: ChartOptionsMap[T]
  responsive: boolean
  height?: number
  width?: number
}

export type ChartOptionsMap = {
  line: {
    showGrid?: boolean
    showLegend?: boolean
    showAnimation?: boolean
    strokeWidth?: number
    smooth?: boolean
  }
  bar: {
    showGrid?: boolean
    showLegend?: boolean
    showAnimation?: boolean
    barSize?: number
    stacked?: boolean
  }
  pie: {
    showLegend?: boolean
    showAnimation?: boolean
    innerRadius?: number
    outerRadius?: number
    showLabels?: boolean
  }
  area: {
    showGrid?: boolean
    showLegend?: boolean
    showAnimation?: boolean
    fillOpacity?: number
    stacked?: boolean
  }
  scatter: {
    showGrid?: boolean
    showLegend?: boolean
    showAnimation?: boolean
    pointSize?: number
    showTrendline?: boolean
  }
  gauge: {
    showAnimation?: boolean
    colorRanges?: Array<{
      min: number
      max: number
      color: string
    }>
    showTarget?: boolean
  }
}

// Widget types
export type WidgetType = 'metric' | 'chart' | 'table' | 'alert' | 'status'

export interface BaseWidget {
  id: WidgetId
  type: WidgetType
  title: string
  description?: string
  position: GridPosition
  refreshInterval?: number
  lastUpdated?: Date
}

export interface MetricWidget extends BaseWidget {
  type: 'metric'
  data: {
    value: number | string
    unit?: string
    trend?: TrendData
    target?: number
    status?: 'success' | 'warning' | 'error' | 'info'
  }
}

export interface ChartWidget extends BaseWidget {
  type: 'chart'
  data: ChartConfig<ChartType>
}

export interface TableWidget extends BaseWidget {
  type: 'table'
  data: {
    columns: Array<{
      key: string
      title: string
      dataIndex: string
      width?: number
      sortable?: boolean
    }>
    rows: Record<string, unknown>[]
    pagination?: {
      pageSize: number
      showSizeChanger: boolean
    }
  }
}

export interface AlertWidget extends BaseWidget {
  type: 'alert'
  data: {
    alerts: Array<{
      id: string
      level: 'info' | 'warning' | 'error' | 'success'
      message: string
      timestamp: Date
      acknowledged?: boolean
    }>
  }
}

export interface StatusWidget extends BaseWidget {
  type: 'status'
  data: {
    status: 'online' | 'offline' | 'degraded' | 'maintenance'
    uptime?: number
    lastCheck?: Date
    details?: Record<string, unknown>
  }
}

export type DashboardWidget = 
  | MetricWidget 
  | ChartWidget 
  | TableWidget 
  | AlertWidget 
  | StatusWidget

// Grid and layout types
export interface GridPosition {
  x: number
  y: number
  width: number
  height: number
}

export interface ResponsiveBreakpoints {
  xs?: number
  sm?: number
  md?: number
  lg?: number
  xl?: number
  xxl?: number
}

// Trend data
export interface TrendData {
  value: number
  isPositive: boolean
  period: 'hour' | 'day' | 'week' | 'month'
  suffix?: string
}

// Dashboard configuration
export interface DashboardConfig {
  id: DashboardId
  title: string
  description?: string
  widgets: DashboardWidget[]
  layout: 'grid' | 'flex' | 'masonry'
  theme?: 'light' | 'dark' | 'auto'
  refreshInterval?: number
  permissions?: {
    view: UserId[]
    edit: UserId[]
    admin: UserId[]
  }
  metadata?: {
    createdBy: UserId
    createdAt: Date
    updatedBy: UserId
    updatedAt: Date
    version: number
  }
}

// API response types
export interface DashboardApiResponse<T = unknown> {
  success: boolean
  data?: T
  error?: {
    code: string
    message: string
    details?: Record<string, unknown>
  }
  metadata?: {
    timestamp: Date
    requestId: string
    cached?: boolean
  }
}

// Loading and error states
export interface LoadingState {
  isLoading: boolean
  error: string | null
  lastFetch?: Date
}

export interface DashboardState extends LoadingState {
  dashboards: Record<string, DashboardConfig>
  currentDashboard: DashboardId | null
  filters: Record<string, unknown>
  cache: Record<string, {
    data: unknown
    timestamp: Date
    ttl: number
  }>
}

// Utility types for type guards
export const isMetricWidget = (widget: DashboardWidget): widget is MetricWidget =>
  widget.type === 'metric'

export const isChartWidget = (widget: DashboardWidget): widget is ChartWidget =>
  widget.type === 'chart'

export const isTableWidget = (widget: DashboardWidget): widget is TableWidget =>
  widget.type === 'table'

export const isAlertWidget = (widget: DashboardWidget): widget is AlertWidget =>
  widget.type === 'alert'

export const isStatusWidget = (widget: DashboardWidget): widget is StatusWidget =>
  widget.type === 'status'
