/**
 * LLM Management Dashboard
 * 
 * Overview dashboard showing key metrics, provider status, cost summary,
 * and performance indicators for LLM management.
 */

import React, { useState, useEffect } from 'react';
import { Card, Flex, Space, Badge, Button, Alert, Spin } from 'antd';
import {
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  DollarOutlined,
  ThunderboltOutlined,
  ApiOutlined,
  BarChartOutlined
} from '@ant-design/icons';
import { llmManagementService, DashboardSummary } from '../../services/llmManagementService';
import { designTokens } from '../core/design-system';
import { LLMPageHeader } from './components';

export const LLMDashboard: React.FC = () => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboardSummary();
  }, []);

  const loadDashboardSummary = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await llmManagementService.getDashboardSummary();
      setSummary(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load dashboard summary');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>Loading dashboard...</div>
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="Error Loading Dashboard"
        description={error}
        type="error"
        showIcon
        action={
          <Button size="small" onClick={loadDashboardSummary}>
            Retry
          </Button>
        }
      />
    );
  }

  if (!summary) {
    return (
      <Alert
        message="No Data Available"
        description="Dashboard summary data is not available."
        type="warning"
        showIcon
      />
    );
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat('en-US').format(num);
  };

  const formatPercentage = (value: number) => {
    return `${(value * 100).toFixed(1)}%`;
  };

  const formatDuration = (ms: number) => {
    if (ms < 1000) return `${ms}ms`;
    return `${(ms / 1000).toFixed(1)}s`;
  };

  return (
    <div style={{ padding: designTokens.spacing.lg }}>
      {/* Header */}
      <LLMPageHeader
        title="LLM Management Dashboard"
        description="Overview of provider status, usage metrics, and system performance"
        onRefresh={loadDashboardSummary}
        refreshLoading={loading}
      />

      {/* Header Stats */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
        gap: designTokens.spacing.md,
        marginBottom: designTokens.spacing.lg
      }}>
        {/* Provider Status */}
        <Card
          size="small"
          style={{
            background: `linear-gradient(135deg, ${designTokens.colors.primaryLight} 0%, ${designTokens.colors.white} 100%)`,
            border: `1px solid ${designTokens.colors.border}`,
            borderRadius: designTokens.borderRadius.large,
            boxShadow: designTokens.shadows.medium
          }}
        >
          <Flex align="center" gap="middle">
            <div style={{
              width: '56px',
              height: '56px',
              borderRadius: designTokens.borderRadius.large,
              background: `linear-gradient(135deg, ${designTokens.colors.primary} 0%, ${designTokens.colors.primaryHover} 100%)`,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: designTokens.colors.white,
              fontSize: designTokens.typography.fontSize.xl,
              boxShadow: designTokens.shadows.medium
            }}>
              <ApiOutlined />
            </div>
            <div style={{ flex: 1 }}>
              <div style={{
                fontSize: designTokens.typography.fontSize['3xl'],
                fontWeight: designTokens.typography.fontWeight.bold,
                color: designTokens.colors.primary
              }}>
                {summary.providers.healthy}/{summary.providers.total}
              </div>
              <div style={{ color: '#666', fontSize: '14px' }}>
                Healthy Providers
              </div>
              <div style={{ marginTop: '4px' }}>
                <Badge 
                  status={summary.providers.healthy === summary.providers.total ? "success" : "warning"} 
                  text={`${summary.providers.enabled} enabled`}
                />
              </div>
            </div>
          </Flex>
        </Card>

        {/* Usage Stats */}
        <Card size="small" style={{ background: 'linear-gradient(135deg, #f6ffed 0%, #f0f9ff 100%)' }}>
          <Flex align="center" gap="middle">
            <div style={{ 
              width: '48px', 
              height: '48px', 
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #52c41a 0%, #73d13d 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'white',
              fontSize: '20px'
            }}>
              <BarChartOutlined />
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                {formatNumber(summary.usage.totalRequests)}
              </div>
              <div style={{ color: '#666', fontSize: '14px' }}>
                Total Requests (30d)
              </div>
              <div style={{ marginTop: '4px' }}>
                <Badge 
                  status="processing" 
                  text={`${formatNumber(summary.usage.totalTokens)} tokens`}
                />
              </div>
            </div>
          </Flex>
        </Card>

        {/* Cost Summary */}
        <Card size="small" style={{ background: 'linear-gradient(135deg, #fff7e6 0%, #fffbf0 100%)' }}>
          <Flex align="center" gap="middle">
            <div style={{ 
              width: '48px', 
              height: '48px', 
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #fa8c16 0%, #ffa940 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'white',
              fontSize: '20px'
            }}>
              <DollarOutlined />
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#fa8c16' }}>
                {formatCurrency(summary.costs.currentMonth)}
              </div>
              <div style={{ color: '#666', fontSize: '14px' }}>
                Current Month Cost
              </div>
              <div style={{ marginTop: '4px' }}>
                <Badge 
                  status={summary.costs.activeAlerts > 0 ? "error" : "success"} 
                  text={`${summary.costs.activeAlerts} alerts`}
                />
              </div>
            </div>
          </Flex>
        </Card>

        {/* Performance */}
        <Card size="small" style={{ background: 'linear-gradient(135deg, #f9f0ff 0%, #f0f9ff 100%)' }}>
          <Flex align="center" gap="middle">
            <div style={{ 
              width: '48px', 
              height: '48px', 
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #722ed1 0%, #9254de 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'white',
              fontSize: '20px'
            }}>
              <ThunderboltOutlined />
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#722ed1' }}>
                {formatDuration(summary.performance.averageResponseTime)}
              </div>
              <div style={{ color: '#666', fontSize: '14px' }}>
                Avg Response Time
              </div>
              <div style={{ marginTop: '4px' }}>
                <Badge 
                  status={summary.performance.overallSuccessRate > 0.95 ? "success" : "warning"} 
                  text={`${formatPercentage(summary.performance.overallSuccessRate)} success`}
                />
              </div>
            </div>
          </Flex>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card
        title={
          <span style={{
            fontSize: designTokens.typography.fontSize.lg,
            fontWeight: designTokens.typography.fontWeight.semibold,
            color: designTokens.colors.text,
          }}>
            Quick Actions
          </span>
        }
        size="small"
        style={{
          marginBottom: designTokens.spacing.lg,
          borderRadius: designTokens.borderRadius.large,
          boxShadow: designTokens.shadows.medium,
          border: `1px solid ${designTokens.colors.border}`,
        }}
      >
        <Flex gap="middle" wrap="wrap">
          <Button
            type="primary"
            icon={<ApiOutlined />}
            style={{
              borderRadius: designTokens.borderRadius.medium,
              background: `linear-gradient(135deg, ${designTokens.colors.primary} 0%, ${designTokens.colors.primaryHover} 100%)`,
              border: 'none',
              boxShadow: designTokens.shadows.small,
            }}
          >
            Test All Providers
          </Button>
          <Button
            icon={<BarChartOutlined />}
            style={{
              borderRadius: designTokens.borderRadius.medium,
            }}
          >
            View Usage Report
          </Button>
          <Button
            icon={<DollarOutlined />}
            style={{
              borderRadius: designTokens.borderRadius.medium,
            }}
          >
            Cost Analysis
          </Button>
          <Button
            icon={<ThunderboltOutlined />}
            style={{
              borderRadius: designTokens.borderRadius.medium,
            }}
          >
            Performance Report
          </Button>
        </Flex>
      </Card>

      {/* System Status */}
      <Card
        title={
          <span style={{
            fontSize: designTokens.typography.fontSize.lg,
            fontWeight: designTokens.typography.fontWeight.semibold,
            color: designTokens.colors.text,
          }}>
            System Status
          </span>
        }
        size="small"
        style={{
          borderRadius: designTokens.borderRadius.large,
          boxShadow: designTokens.shadows.medium,
          border: `1px solid ${designTokens.colors.border}`,
        }}
      >
        <Space direction="vertical" style={{ width: '100%' }} size="middle">
          <Flex justify="between" align="center">
            <span style={{
              fontSize: designTokens.typography.fontSize.sm,
              color: designTokens.colors.text
            }}>
              Overall System Health
            </span>
            <Badge
              status={summary.providers.healthy === summary.providers.total ? "success" : "warning"}
              text={summary.providers.healthy === summary.providers.total ? "Healthy" : "Issues Detected"}
            />
          </Flex>

          <Flex justify="between" align="center">
            <span style={{
              fontSize: designTokens.typography.fontSize.sm,
              color: designTokens.colors.text
            }}>
              Success Rate (30d)
            </span>
            <Badge
              status={summary.performance.overallSuccessRate > 0.95 ? "success" : "warning"}
              text={formatPercentage(summary.performance.overallSuccessRate)}
            />
          </Flex>

          <Flex justify="between" align="center">
            <span style={{
              fontSize: designTokens.typography.fontSize.sm,
              color: designTokens.colors.text
            }}>
              Cost Alerts
            </span>
            <Badge
              status={summary.costs.activeAlerts === 0 ? "success" : "error"}
              text={summary.costs.activeAlerts === 0 ? "No alerts" : `${summary.costs.activeAlerts} active`}
            />
          </Flex>

          <Flex justify="between" align="center">
            <span style={{
              fontSize: designTokens.typography.fontSize.sm,
              color: designTokens.colors.text
            }}>
              Total Errors (30d)
            </span>
            <Badge
              status={summary.performance.totalErrors === 0 ? "success" : "warning"}
              text={formatNumber(summary.performance.totalErrors)}
            />
          </Flex>
        </Space>

        <div style={{
          marginTop: designTokens.spacing.md,
          padding: designTokens.spacing.sm,
          background: designTokens.colors.backgroundSecondary,
          borderRadius: designTokens.borderRadius.medium,
          fontSize: designTokens.typography.fontSize.xs,
          color: designTokens.colors.textSecondary
        }}>
          Last updated: {new Date(summary.lastUpdated).toLocaleString()}
        </div>
      </Card>
    </div>
  );
};
