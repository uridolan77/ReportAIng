import React from 'react'
import { Card, Row, Col, Statistic } from 'antd'
import { UserOutlined, TeamOutlined, SettingOutlined } from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

interface UserContextSectionProps {
  context: BusinessContextProfile
}

export const UserContextSection: React.FC<UserContextSectionProps> = ({
  context
}) => {
  if (!context.userContext) {
    return null
  }

  return (
    <Card size="small">
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={8}>
          <Statistic
            title="Role"
            value={context.userContext.role}
            prefix={<UserOutlined />}
            valueStyle={{ color: '#52c41a' }}
          />
        </Col>
        <Col xs={24} sm={8}>
          <Statistic
            title="Department"
            value={context.userContext.department}
            prefix={<TeamOutlined />}
            valueStyle={{ color: '#1890ff' }}
          />
        </Col>
        <Col xs={24} sm={8}>
          <Statistic
            title="Access Level"
            value={context.userContext.accessLevel}
            prefix={<SettingOutlined />}
            valueStyle={{ color: '#fa8c16' }}
          />
        </Col>
      </Row>
    </Card>
  )
}

export default UserContextSection
