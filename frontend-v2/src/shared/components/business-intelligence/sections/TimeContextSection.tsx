import React from 'react'
import { Card, Row, Col, Statistic, Typography, Space } from 'antd'
import { ClockCircleOutlined, BarChartOutlined, InfoCircleOutlined } from '@ant-design/icons'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

const { Text } = Typography

interface TimeContextSectionProps {
  context: BusinessContextProfile
}

export const TimeContextSection: React.FC<TimeContextSectionProps> = ({
  context
}) => {
  if (!context.timeContext) {
    return null
  }

  return (
    <Card size="small">
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={8}>
          <Statistic
            title="Time Period"
            value={context.timeContext.period}
            prefix={<ClockCircleOutlined />}
            valueStyle={{ color: '#fa8c16' }}
          />
        </Col>
        <Col xs={24} sm={8}>
          <Statistic
            title="Granularity"
            value={context.timeContext.granularity}
            prefix={<BarChartOutlined />}
            valueStyle={{ color: '#13c2c2' }}
          />
        </Col>
        <Col xs={24} sm={8}>
          <Statistic
            title="Reference Point"
            value={context.timeContext.relativeTo || 'Absolute'}
            prefix={<InfoCircleOutlined />}
            valueStyle={{ color: '#722ed1' }}
          />
        </Col>
      </Row>

      {context.timeContext.trends && context.timeContext.trends.length > 0 && (
        <div style={{ marginTop: 16 }}>
          <Text strong>Identified Trends:</Text>
          <div style={{ marginTop: 8 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              {context.timeContext.trends.map((trend, index) => (
                <div key={index} style={{ 
                  padding: '8px', 
                  background: '#f5f5f5', 
                  borderRadius: '4px',
                  fontSize: '12px'
                }}>
                  <Text type="secondary">{trend}</Text>
                </div>
              ))}
            </Space>
          </div>
        </div>
      )}
    </Card>
  )
}

export default TimeContextSection
