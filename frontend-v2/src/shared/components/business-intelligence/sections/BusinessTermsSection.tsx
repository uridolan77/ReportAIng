import React from 'react'
import { Card, Space, Typography, Tag } from 'antd'
import type { BusinessContextProfile } from '@shared/types/business-intelligence'

const { Text } = Typography

interface BusinessTermsSectionProps {
  context: BusinessContextProfile
  interactive?: boolean
}

export const BusinessTermsSection: React.FC<BusinessTermsSectionProps> = ({
  context,
  interactive = true
}) => {
  if (context.businessTerms.length === 0) {
    return null
  }

  return (
    <Card size="small">
      <Space direction="vertical" style={{ width: '100%' }}>
        <Text type="secondary">
          Business terms and concepts identified in your query context
        </Text>
        <Space wrap size="middle">
          {context.businessTerms.map((term, index) => (
            <Tag
              key={index}
              color="purple"
              style={{
                cursor: interactive ? 'pointer' : 'default',
                fontSize: '13px',
                padding: '4px 8px'
              }}
              onClick={() => interactive && console.log('Explore term:', term)}
            >
              {term}
            </Tag>
          ))}
        </Space>
      </Space>
    </Card>
  )
}

export default BusinessTermsSection
