import React from 'react'
import { Card, Typography } from 'antd'
import { PageLayout } from '@shared/components/core/Layout'

const { Text } = Typography

export default function Analytics() {
  return (
    <PageLayout
      title="Analytics & Monitoring"
      subtitle="View system analytics and performance metrics"
    >
      <Card>
        <Text>Analytics and monitoring interface will be implemented here.</Text>
      </Card>
    </PageLayout>
  )
}
