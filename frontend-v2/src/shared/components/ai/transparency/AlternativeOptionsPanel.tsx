import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Table, 
  Tag, 
  Button, 
  Space, 
  Typography, 
  Tooltip, 
  Progress, 
  Alert,
  Spin,
  Empty,
  Badge,
  Divider
} from 'antd'
import {
  CheckOutlined,
  InfoCircleOutlined,
  ThunderboltOutlined,
  BulbOutlined,
  TrophyOutlined,
  ExperimentOutlined
} from '@ant-design/icons'
import { useGetAlternativeOptionsQuery } from '@shared/store/api/transparencyApi'
import type { AlternativeOption } from '@shared/types/ai'

const { Title, Text, Paragraph } = Typography

export interface AlternativeOptionsPanelProps {
  traceId: string
  onSelectAlternative?: (option: AlternativeOption) => void
  showDetailedAnalysis?: boolean
  compact?: boolean
  className?: string
}

/**
 * AlternativeOptionsPanel - Display and compare alternative query options
 * 
 * Features:
 * - Score-based ranking of alternatives
 * - Detailed rationale and improvement estimates
 * - Interactive selection and application
 * - Performance impact indicators
 * - Type-based categorization
 */
export const AlternativeOptionsPanel: React.FC<AlternativeOptionsPanelProps> = ({
  traceId,
  onSelectAlternative,
  showDetailedAnalysis = true,
  compact = false,
  className
}) => {
  const [selectedOption, setSelectedOption] = useState<string | null>(null)
  const [applyingOption, setApplyingOption] = useState<string | null>(null)

  const { 
    data: alternatives, 
    isLoading, 
    error,
    refetch 
  } = useGetAlternativeOptionsQuery(traceId)

  // Sort alternatives by score (highest first)
  const sortedAlternatives = useMemo(() => {
    if (!alternatives) return []
    return [...alternatives].sort((a, b) => b.score - a.score)
  }, [alternatives])

  // Get type-specific styling
  const getTypeConfig = (type: string) => {
    const configs = {
      'Template': { color: 'blue', icon: <BulbOutlined /> },
      'Optimization': { color: 'green', icon: <ThunderboltOutlined /> },
      'Alternative': { color: 'orange', icon: <ExperimentOutlined /> },
      'Enhancement': { color: 'purple', icon: <TrophyOutlined /> },
    }
    return configs[type as keyof typeof configs] || { color: 'default', icon: <InfoCircleOutlined /> }
  }

  // Handle option selection
  const handleSelectOption = async (option: AlternativeOption) => {
    if (applyingOption) return

    setApplyingOption(option.id)
    try {
      await onSelectAlternative?.(option)
      setSelectedOption(option.id)
    } catch (error) {
      console.error('Failed to apply alternative option:', error)
    } finally {
      setApplyingOption(null)
    }
  }

  // Table columns configuration
  const columns = [
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 120,
      render: (type: string) => {
        const config = getTypeConfig(type)
        return (
          <Tag color={config.color} icon={config.icon}>
            {type}
          </Tag>
        )
      },
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (description: string, record: AlternativeOption) => (
        <div>
          <Text strong>{description}</Text>
          {showDetailedAnalysis && record.reasoning && (
            <div style={{ marginTop: 4 }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {record.reasoning}
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Score',
      dataIndex: 'score',
      key: 'score',
      width: 120,
      sorter: (a: AlternativeOption, b: AlternativeOption) => a.confidence - b.confidence,
      render: (score: number) => {
        const percentage = Math.round(score * 100)
        const color = percentage >= 80 ? 'success' : percentage >= 60 ? 'normal' : 'exception'
        return (
          <div style={{ textAlign: 'center' }}>
            <Progress
              type="circle"
              size={40}
              percent={percentage}
              status={color}
              format={() => `${percentage}%`}
            />
          </div>
        )
      },
    },
    {
      title: 'Impact',
      dataIndex: 'estimatedImpact',
      key: 'impact',
      width: 120,
      render: (impact: AlternativeOption['estimatedImpact']) => (
        <div style={{ textAlign: 'center' }}>
          <Badge
            count={`+${impact.performance.toFixed(1)}%`}
            style={{
              backgroundColor: impact.performance > 10 ? '#52c41a' : impact.performance > 5 ? '#faad14' : '#1890ff'
            }}
          />
        </div>
      ),
    },
    {
      title: 'Action',
      key: 'action',
      width: 120,
      render: (_: any, record: AlternativeOption) => {
        const isSelected = selectedOption === record.id
        const isApplying = applyingOption === record.id
        
        return (
          <Space direction="vertical" size="small">
            <Button
              type={isSelected ? 'default' : 'primary'}
              size="small"
              icon={isSelected ? <CheckOutlined /> : undefined}
              loading={isApplying}
              disabled={!!applyingOption}
              onClick={() => handleSelectOption(record)}
              style={{ width: '100%' }}
            >
              {isSelected ? 'Applied' : 'Apply'}
            </Button>
            {showDetailedAnalysis && (
              <Tooltip title="View detailed analysis">
                <Button
                  type="text"
                  size="small"
                  icon={<InfoCircleOutlined />}
                  onClick={() => {/* TODO: Show detailed modal */}}
                >
                  Details
                </Button>
              </Tooltip>
            )}
          </Space>
        )
      },
    },
  ]

  if (isLoading) {
    return (
      <Card className={className}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text type="secondary">Loading alternative options...</Text>
          </div>
        </div>
      </Card>
    )
  }

  if (error) {
    return (
      <Card className={className}>
        <Alert
          message="Failed to Load Alternatives"
          description="Unable to fetch alternative options for this query."
          type="error"
          showIcon
          action={
            <Button size="small" onClick={() => refetch()}>
              Retry
            </Button>
          }
        />
      </Card>
    )
  }

  if (!alternatives || alternatives.length === 0) {
    return (
      <Card 
        title="Alternative Options" 
        className={className}
        extra={
          <Tooltip title="No alternative approaches found for this query">
            <InfoCircleOutlined />
          </Tooltip>
        }
      >
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="No alternative options available"
        />
      </Card>
    )
  }

  return (
    <Card
      title={
        <Space>
          <BulbOutlined />
          <span>Alternative Options</span>
          <Badge count={alternatives.length} style={{ backgroundColor: '#1890ff' }} />
        </Space>
      }
      className={className}
      extra={
        <Space>
          <Tooltip title="Options ranked by potential improvement">
            <InfoCircleOutlined />
          </Tooltip>
          <Button size="small" onClick={() => refetch()}>
            Refresh
          </Button>
        </Space>
      }
    >
      {/* Summary Stats */}
      {!compact && (
        <>
          <div style={{ marginBottom: 16 }}>
            <Space split={<Divider type="vertical" />}>
              <Text type="secondary">
                <strong>{alternatives.length}</strong> alternatives found
              </Text>
              <Text type="secondary">
                Best improvement: <strong>+{Math.max(...alternatives.map(a => a.estimatedImpact.performance)).toFixed(1)}%</strong>
              </Text>
              <Text type="secondary">
                Avg score: <strong>{Math.round(alternatives.reduce((sum, a) => sum + a.score, 0) / alternatives.length * 100)}%</strong>
              </Text>
            </Space>
          </div>
          <Divider />
        </>
      )}

      {/* Options Table */}
      <Table
        dataSource={sortedAlternatives}
        columns={columns}
        rowKey="id"
        size={compact ? 'small' : 'middle'}
        pagination={false}
        scroll={{ x: 600 }}
        rowClassName={(record) =>
          selectedOption === record.id ? 'ant-table-row-selected' : ''
        }
      />

      {/* Help Text */}
      {!compact && (
        <div style={{ marginTop: 16, padding: '12px', backgroundColor: '#f6f8fa', borderRadius: '6px' }}>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            💡 <strong>Tip:</strong> Higher scores indicate better alternatives. 
            Click "Apply" to use an alternative approach for similar queries.
          </Text>
        </div>
      )}
    </Card>
  )
}

export default AlternativeOptionsPanel
