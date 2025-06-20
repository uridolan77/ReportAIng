import React from 'react'
import { Card, Typography } from 'antd'
import { PageLayout } from '@shared/components/core/Layout'

const { Text } = Typography

export default function SystemConfiguration() {
  return (
    <PageLayout
      title="System Configuration"
      subtitle="Configure system settings and AI providers"
    >
      <Card>
        <Text>System configuration interface will be implemented here.</Text>
      </Card>
    </PageLayout>
  )
}
