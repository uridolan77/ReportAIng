/**
 * Compound Dashboard Component System
 * 
 * Provides a flexible, composable dashboard architecture using compound components pattern.
 * This consolidates patterns from all existing dashboard implementations.
 */

import React from 'react'
import { DashboardLayout, MetricGrid, ChartSection } from './DashboardLayout'
import { DashboardWidget, MetricWidget, KPIWidget } from './DashboardWidget'
import { Chart } from '@shared/components/core'

// Main Dashboard compound component
export interface DashboardProps {
  children: React.ReactNode
}

const DashboardRoot: React.FC<DashboardProps> = ({ children }) => {
  return <div className="dashboard-root">{children}</div>
}

// Compound component structure
export const Dashboard = {
  // Layout components
  Root: DashboardRoot,
  Layout: DashboardLayout,
  MetricGrid: MetricGrid,
  ChartSection: ChartSection,
  
  // Widget components
  Widget: DashboardWidget,
  MetricWidget: MetricWidget,
  KPIWidget: KPIWidget,
  
  // Chart component (re-export for convenience)
  Chart: Chart,
}

// Convenience hooks for common dashboard patterns
export const useDashboardRefresh = (refreshFunctions: Array<() => void>) => {
  const handleRefreshAll = React.useCallback(() => {
    refreshFunctions.forEach(fn => fn())
  }, [refreshFunctions])

  return { handleRefreshAll }
}

export const useDashboardAlerts = () => {
  const [alerts, setAlerts] = React.useState<Array<{
    id: string
    message: string
    description?: string
    type: 'success' | 'info' | 'warning' | 'error'
    showIcon?: boolean
    closable?: boolean
  }>>([])

  const addAlert = React.useCallback((alert: Omit<typeof alerts[0], 'id'>) => {
    const id = Date.now().toString()
    setAlerts(prev => [...prev, { ...alert, id }])
  }, [])

  const removeAlert = React.useCallback((id: string) => {
    setAlerts(prev => prev.filter(alert => alert.id !== id))
  }, [])

  const clearAlerts = React.useCallback(() => {
    setAlerts([])
  }, [])

  return {
    alerts,
    addAlert,
    removeAlert,
    clearAlerts,
  }
}

// Common dashboard data patterns
export interface DashboardMetric {
  key: string
  title: string
  value: string | number
  prefix?: React.ReactNode
  suffix?: string
  loading?: boolean
  error?: string | null
  trend?: {
    value: number
    isPositive?: boolean
    suffix?: string
  }
  tooltip?: string
  onRefresh?: () => void
}

export const useDashboardMetrics = (metrics: DashboardMetric[]) => {
  const renderMetrics = React.useCallback(() => {
    return metrics.map(metric => (
      <Dashboard.MetricWidget
        key={metric.key}
        title={metric.title}
        value={metric.value}
        prefix={metric.prefix}
        suffix={metric.suffix}
        loading={metric.loading}
        error={metric.error}
        trend={metric.trend}
        tooltip={metric.tooltip}
        onRefresh={metric.onRefresh}
      />
    ))
  }, [metrics])

  return { renderMetrics }
}

// Example usage patterns (for documentation)
export const DashboardExamples = {
  // Basic metric dashboard
  BasicMetrics: () => (
    <Dashboard.Root>
      <Dashboard.Layout
        title="System Metrics"
        subtitle="Overview of system performance"
        sections={[
          {
            children: (
              <Dashboard.MetricGrid
                metrics={[
                  <Dashboard.MetricWidget
                    key="users"
                    title="Total Users"
                    value={1234}
                    prefix={<span>👥</span>}
                  />,
                  <Dashboard.MetricWidget
                    key="queries"
                    title="Total Queries"
                    value={5678}
                    prefix={<span>🔍</span>}
                  />,
                ]}
              />
            ),
          },
        ]}
      />
    </Dashboard.Root>
  ),

  // Advanced dashboard with charts
  AdvancedDashboard: () => (
    <Dashboard.Root>
      <Dashboard.Layout
        title="Analytics Dashboard"
        subtitle="Comprehensive system analytics"
        sections={[
          {
            title: "Key Metrics",
            children: (
              <Dashboard.MetricGrid
                metrics={[
                  <Dashboard.KPIWidget
                    key="revenue"
                    title="Revenue"
                    value="$12,345"
                    highlight
                    trend={{ value: 12.5, isPositive: true, suffix: '%' }}
                  />,
                ]}
              />
            ),
          },
          {
            title: "Trends",
            children: (
              <Dashboard.ChartSection
                charts={[
                  <Dashboard.Chart
                    key="trend"
                    data={[]}
                    columns={['date', 'value']}
                    config={{
                      type: 'line',
                      title: 'Usage Trend',
                      xAxis: 'date',
                      yAxis: 'value',
                    }}
                    height={300}
                  />,
                ]}
              />
            ),
          },
        ]}
      />
    </Dashboard.Root>
  ),
}

export default Dashboard
