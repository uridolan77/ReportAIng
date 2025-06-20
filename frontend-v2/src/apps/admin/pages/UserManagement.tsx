import React from 'react'
import { Card, Typography } from 'antd'
import { PageLayout } from '@shared/components/core/Layout'

const { Text } = Typography

export default function UserManagement() {
  return (
    <PageLayout
      title="User Management"
      subtitle="Manage users, roles, and permissions"
    >
      <Card>
        <Text>User management interface will be implemented here.</Text>
      </Card>
    </PageLayout>
  )
}
