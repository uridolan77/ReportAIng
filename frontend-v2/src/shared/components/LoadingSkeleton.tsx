import React from 'react'
import { Skeleton, Card, Row, Col } from 'antd'

interface LoadingSkeletonProps {
  type?: 'dashboard' | 'table' | 'chart' | 'form'
  rows?: number
  columns?: number
}

export const LoadingSkeleton: React.FC<LoadingSkeletonProps> = ({
  type = 'dashboard',
  rows = 3,
  columns = 4
}) => {
  if (type === 'dashboard') {
    return (
      <div>
        {/* Header skeleton */}
        <div style={{ marginBottom: '24px' }}>
          <Skeleton.Input style={{ width: 300, height: 32 }} active />
          <div style={{ marginTop: '8px' }}>
            <Skeleton.Input style={{ width: 500, height: 20 }} active />
          </div>
        </div>

        {/* Metrics cards skeleton */}
        <Row gutter={16} style={{ marginBottom: '24px' }}>
          {Array.from({ length: columns }).map((_, index) => (
            <Col span={24 / columns} key={index}>
              <Card>
                <Skeleton active paragraph={{ rows: 2 }} />
              </Card>
            </Col>
          ))}
        </Row>

        {/* Charts skeleton */}
        <Row gutter={16}>
          <Col span={12}>
            <Card>
              <Skeleton.Input style={{ width: '100%', height: 300 }} active />
            </Card>
          </Col>
          <Col span={12}>
            <Card>
              <Skeleton.Input style={{ width: '100%', height: 300 }} active />
            </Card>
          </Col>
        </Row>
      </div>
    )
  }

  if (type === 'table') {
    return (
      <Card>
        <Skeleton active paragraph={{ rows: rows * 2 }} />
      </Card>
    )
  }

  if (type === 'chart') {
    return (
      <Card>
        <Skeleton.Input style={{ width: '100%', height: 300 }} active />
      </Card>
    )
  }

  if (type === 'form') {
    return (
      <Card>
        <Skeleton active paragraph={{ rows: rows }} />
      </Card>
    )
  }

  return <Skeleton active />
}

export default LoadingSkeleton
