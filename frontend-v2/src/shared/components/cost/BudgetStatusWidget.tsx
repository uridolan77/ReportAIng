import React from 'react'
import { Card, Progress, List, Tag, Button, Empty, Spin, Typography } from 'antd'
import { 
  ExclamationCircleOutlined, 
  CheckCircleOutlined, 
  WarningOutlined,
  PlusOutlined 
} from '@ant-design/icons'
import { useGetBudgetsQuery } from '../../store/api/costApi'
import type { BudgetManagement } from '../../types/cost'

const { Text } = Typography

interface BudgetStatusWidgetProps {
  onCreateBudget?: () => void
  showCreateButton?: boolean
}

export const BudgetStatusWidget: React.FC<BudgetStatusWidgetProps> = ({
  onCreateBudget,
  showCreateButton = true
}) => {
  const { data: budgetsData, isLoading, error } = useGetBudgetsQuery()

  const getBudgetStatus = (budget: BudgetManagement) => {
    const usagePercentage = (budget.spentAmount / budget.budgetAmount) * 100
    
    if (usagePercentage >= budget.blockThreshold) {
      return { status: 'critical', color: '#ff4d4f', icon: <ExclamationCircleOutlined /> }
    } else if (usagePercentage >= budget.alertThreshold) {
      return { status: 'warning', color: '#faad14', icon: <WarningOutlined /> }
    } else {
      return { status: 'normal', color: '#52c41a', icon: <CheckCircleOutlined /> }
    }
  }

  const getProgressStatus = (budget: BudgetManagement) => {
    const usagePercentage = (budget.spentAmount / budget.budgetAmount) * 100
    
    if (usagePercentage >= budget.blockThreshold) {
      return 'exception'
    } else if (usagePercentage >= budget.alertThreshold) {
      return 'active'
    } else {
      return 'success'
    }
  }

  if (isLoading) {
    return (
      <Card title="Budget Status">
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
        </div>
      </Card>
    )
  }

  if (error || !budgetsData) {
    return (
      <Card title="Budget Status">
        <Empty description="Failed to load budget data" />
      </Card>
    )
  }

  const activeBudgets = budgetsData.budgets.filter(budget => budget.isActive)

  if (activeBudgets.length === 0) {
    return (
      <Card 
        title="Budget Status"
        extra={
          showCreateButton && onCreateBudget && (
            <Button 
              type="primary" 
              size="small" 
              icon={<PlusOutlined />}
              onClick={onCreateBudget}
            >
              Create Budget
            </Button>
          )
        }
      >
        <Empty 
          description="No active budgets"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      </Card>
    )
  }

  return (
    <Card 
      title="Budget Status"
      extra={
        showCreateButton && onCreateBudget && (
          <Button 
            type="link" 
            size="small" 
            icon={<PlusOutlined />}
            onClick={onCreateBudget}
          >
            Add Budget
          </Button>
        )
      }
    >
      <List
        size="small"
        dataSource={activeBudgets}
        renderItem={(budget) => {
          const { status, color, icon } = getBudgetStatus(budget)
          const usagePercentage = (budget.spentAmount / budget.budgetAmount) * 100
          const progressStatus = getProgressStatus(budget)

          return (
            <List.Item style={{ padding: '12px 0' }}>
              <div style={{ width: '100%' }}>
                {/* Budget Header */}
                <div style={{ 
                  display: 'flex', 
                  justifyContent: 'space-between', 
                  alignItems: 'center',
                  marginBottom: '8px'
                }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{ color, fontSize: '16px' }}>{icon}</span>
                    <Text strong>{budget.name}</Text>
                    <Tag color={color} size="small">
                      {budget.period}
                    </Tag>
                  </div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {status === 'critical' ? 'Over Budget' : 
                     status === 'warning' ? 'Near Limit' : 'On Track'}
                  </Text>
                </div>

                {/* Progress Bar */}
                <Progress
                  percent={Math.min(usagePercentage, 100)}
                  status={progressStatus}
                  size="small"
                  format={(percent) => `${percent?.toFixed(1)}%`}
                />

                {/* Budget Details */}
                <div style={{ 
                  display: 'flex', 
                  justifyContent: 'space-between',
                  marginTop: '8px',
                  fontSize: '12px',
                  color: '#666'
                }}>
                  <span>
                    Spent: ${budget.spentAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </span>
                  <span>
                    Budget: ${budget.budgetAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  </span>
                </div>

                {/* Remaining Amount */}
                <div style={{ 
                  marginTop: '4px',
                  fontSize: '12px',
                  color: budget.remainingAmount > 0 ? '#52c41a' : '#ff4d4f'
                }}>
                  Remaining: ${Math.max(budget.remainingAmount, 0).toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  {budget.remainingAmount < 0 && ' (Over Budget)'}
                </div>

                {/* Alert Thresholds */}
                {usagePercentage >= budget.alertThreshold && (
                  <div style={{ 
                    marginTop: '8px',
                    padding: '4px 8px',
                    backgroundColor: status === 'critical' ? '#fff2f0' : '#fffbe6',
                    border: `1px solid ${status === 'critical' ? '#ffccc7' : '#ffe58f'}`,
                    borderRadius: '4px',
                    fontSize: '11px'
                  }}>
                    {status === 'critical' 
                      ? `Exceeded block threshold (${budget.blockThreshold}%)`
                      : `Exceeded alert threshold (${budget.alertThreshold}%)`
                    }
                  </div>
                )}
              </div>
            </List.Item>
          )
        }}
      />
    </Card>
  )
}
