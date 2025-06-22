import React from 'react'
import { Card, Empty, Typography } from 'antd'

const { Text } = Typography

interface PerformanceMetricsPanelProps {
  query: string
  analysisResults?: any
  showOptimizations?: boolean
  interactive?: boolean
}

export const PerformanceMetricsPanel: React.FC<PerformanceMetricsPanelProps> = ({
  query,
  analysisResults,
  showOptimizations = true,
  interactive = true
}) => {
  return (
    <Card title="Performance Metrics">
      <Empty description="Performance metrics will be displayed here when the backend API is connected" />
    </Card>
  )
}

export default PerformanceMetricsPanel
