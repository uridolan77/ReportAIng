/**
 * Unified Dashboard Layout Component
 * 
 * Provides consistent layout patterns for all dashboard types:
 * - Responsive grid system
 * - Consistent spacing and breakpoints
 * - Header with actions
 * - Alert management
 * - Loading states
 */

import React from 'react'
import { Row, Col, Space, Button, Alert, Typography, Divider } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import { PageLayout } from '@shared/components/core/Layout'

const { Title, Text } = Typography

export interface DashboardSection {
  /** Section title */
  title?: string
  /** Section content */
  children: React.ReactNode
  /** Column span configuration */
  span?: {
    xs?: number
    sm?: number
    md?: number
    lg?: number
    xl?: number
    xxl?: number
  }
  /** Custom column props */
  colProps?: React.ComponentProps<typeof Col>
}

export interface DashboardLayoutProps {
  /** Dashboard title */
  title: string
  /** Dashboard subtitle */
  subtitle?: string
  /** Header actions */
  actions?: React.ReactNode
  /** Global refresh handler */
  onRefresh?: () => void
  /** Loading state */
  loading?: boolean
  /** Global alerts */
  alerts?: Array<{
    message: string
    description?: string
    type: 'success' | 'info' | 'warning' | 'error'
    showIcon?: boolean
    closable?: boolean
  }>
  /** Dashboard sections */
  sections: DashboardSection[]
  /** Custom gutter spacing */
  gutter?: [number, number]
  /** Extra content in header */
  extra?: React.ReactNode
}

export const DashboardLayout: React.FC<DashboardLayoutProps> = ({
  title,
  subtitle,
  actions,
  onRefresh,
  loading = false,
  alerts = [],
  sections,
  gutter = [16, 16],
  extra,
}) => {
  const headerActions = (
    <Space>
      {onRefresh && (
        <Button 
          icon={<ReloadOutlined />} 
          onClick={onRefresh}
          loading={loading}
        >
          Refresh
        </Button>
      )}
      {actions}
      {extra}
    </Space>
  )

  return (
    <PageLayout
      title={title}
      subtitle={subtitle}
      extra={headerActions}
    >
      {/* Global Alerts */}
      {alerts.length > 0 && (
        <div style={{ marginBottom: 24 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            {alerts.map((alert, index) => (
              <Alert
                key={index}
                message={alert.message}
                description={alert.description}
                type={alert.type}
                showIcon={alert.showIcon}
                closable={alert.closable}
              />
            ))}
          </Space>
        </div>
      )}

      {/* Dashboard Sections */}
      {sections.map((section, sectionIndex) => (
        <div key={sectionIndex}>
          {section.title && (
            <>
              <Title level={4} style={{ marginBottom: 16 }}>
                {section.title}
              </Title>
            </>
          )}
          
          <Row gutter={gutter} style={{ marginBottom: 24 }}>
            <Col
              xs={section.span?.xs || 24}
              sm={section.span?.sm || 24}
              md={section.span?.md || 24}
              lg={section.span?.lg || 24}
              xl={section.span?.xl || 24}
              xxl={section.span?.xxl || 24}
              {...section.colProps}
            >
              {section.children}
            </Col>
          </Row>
          
          {sectionIndex < sections.length - 1 && <Divider />}
        </div>
      ))}
    </PageLayout>
  )
}

// Specialized layout for metric grids (common pattern in dashboards)
export interface MetricGridProps {
  /** Grid title */
  title?: string
  /** Metric widgets */
  metrics: React.ReactNode[]
  /** Responsive breakpoints for each metric */
  breakpoints?: {
    xs?: number
    sm?: number
    md?: number
    lg?: number
    xl?: number
  }
  /** Custom gutter */
  gutter?: [number, number]
  /** Loading state */
  loading?: boolean
}

export const MetricGrid: React.FC<MetricGridProps> = ({
  title,
  metrics,
  breakpoints = { xs: 24, sm: 12, md: 8, lg: 6, xl: 6 },
  gutter = [16, 16],
  loading = false,
}) => {
  return (
    <div>
      {title && (
        <Title level={4} style={{ marginBottom: 16 }}>
          {title}
        </Title>
      )}
      
      <Row gutter={gutter}>
        {metrics.map((metric, index) => (
          <Col
            key={index}
            xs={breakpoints.xs}
            sm={breakpoints.sm}
            md={breakpoints.md}
            lg={breakpoints.lg}
            xl={breakpoints.xl}
          >
            {metric}
          </Col>
        ))}
      </Row>
    </div>
  )
}

// Chart section layout (common pattern for dashboard charts)
export interface ChartSectionProps {
  /** Section title */
  title?: string
  /** Chart components */
  charts: React.ReactNode[]
  /** Charts per row on different breakpoints */
  chartsPerRow?: {
    xs?: number
    sm?: number
    md?: number
    lg?: number
    xl?: number
  }
  /** Custom gutter */
  gutter?: [number, number]
}

export const ChartSection: React.FC<ChartSectionProps> = ({
  title,
  charts,
  chartsPerRow = { xs: 1, sm: 1, md: 2, lg: 2, xl: 2 },
  gutter = [16, 16],
}) => {
  const getSpan = (breakpoint: keyof typeof chartsPerRow) => {
    const perRow = chartsPerRow[breakpoint] || 1
    return 24 / perRow
  }

  return (
    <div>
      {title && (
        <Title level={4} style={{ marginBottom: 16 }}>
          {title}
        </Title>
      )}
      
      <Row gutter={gutter}>
        {charts.map((chart, index) => (
          <Col
            key={index}
            xs={getSpan('xs')}
            sm={getSpan('sm')}
            md={getSpan('md')}
            lg={getSpan('lg')}
            xl={getSpan('xl')}
          >
            {chart}
          </Col>
        ))}
      </Row>
    </div>
  )
}

export default DashboardLayout
