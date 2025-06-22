import React from 'react'
import { Card, Empty, Typography } from 'antd'

const { Text } = Typography

interface QueryHistoryPanelProps {
  onQuerySelect?: (query: string) => void
  interactive?: boolean
}

export const QueryHistoryPanel: React.FC<QueryHistoryPanelProps> = ({
  onQuerySelect,
  interactive = true
}) => {
  return (
    <Card title="Query History">
      <Empty description="Query history will be displayed here when the backend API is connected" />
    </Card>
  )
}

export default QueryHistoryPanel
