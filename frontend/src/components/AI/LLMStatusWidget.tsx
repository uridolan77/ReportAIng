/**
 * LLM Status Widget
 * 
 * Quick status widget showing LLM system health and providing
 * quick access to LLM management features.
 */

import React, { useState, useEffect } from 'react';
import { Card, Badge, Space, Button, Tooltip, Statistic, Row, Col } from 'antd';
import { 
  ApiOutlined, 
  SettingOutlined, 
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  DollarOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';
import { llmManagementService, DashboardSummary } from '../../services/llmManagementService';

interface LLMStatusWidgetProps {
  compact?: boolean;
  showActions?: boolean;
}

export const LLMStatusWidget: React.FC<LLMStatusWidgetProps> = ({
  compact = false,
  showActions = true
}) => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadSummary();
    // Refresh every 30 seconds
    const interval = setInterval(loadSummary, 30000);
    return () => clearInterval(interval);
  }, []);

  const loadSummary = async () => {
    try {
      setError(null);
      const data = await llmManagementService.getDashboardSummary();
      setSummary(data);
    } catch (err) {
      console.warn('LLM Status Widget error:', err);
      setError(err instanceof Error ? err.message : 'Failed to load LLM status');
      // Set a basic fallback summary
      setSummary({
        providers: { total: 0, enabled: 0, healthy: 0 },
        usage: { totalRequests: 0, totalTokens: 0, averageResponseTime: 0, successRate: 0 },
        costs: { currentMonth: 0, total30Days: 0, activeAlerts: 0 },
        performance: { averageResponseTime: 0, overallSuccessRate: 0, totalErrors: 0 },
        lastUpdated: new Date().toISOString(),
      });
    } finally {
      setLoading(false);
    }
  };

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

  const getSystemStatus = () => {
    if (!summary) return { status: 'default', text: 'Unknown' };
    
    const allHealthy = summary.providers.healthy === summary.providers.total;
    const hasEnabledProviders = summary.providers.enabled > 0;
    
    if (allHealthy && hasEnabledProviders) {
      return { status: 'success', text: 'Healthy' };
    } else if (hasEnabledProviders) {
      return { status: 'warning', text: 'Issues' };
    } else {
      return { status: 'error', text: 'No Providers' };
    }
  };

  if (loading) {
    return (
      <Card size="small" loading={true}>
        <div style={{ height: compact ? '60px' : '120px' }} />
      </Card>
    );
  }

  if (error) {
    return (
      <Card size="small">
        <div style={{ textAlign: 'center', padding: '16px' }}>
          <ExclamationCircleOutlined style={{ color: '#ff4d4f', fontSize: '24px' }} />
          <div style={{ marginTop: '8px', color: '#666' }}>
            LLM System Unavailable
          </div>
          {showActions && (
            <Button 
              size="small" 
              type="link" 
              onClick={loadSummary}
              style={{ marginTop: '8px' }}
            >
              Retry
            </Button>
          )}
        </div>
      </Card>
    );
  }

  if (!summary) {
    return null;
  }

  const systemStatus = getSystemStatus();

  if (compact) {
    return (
      <Card size="small" style={{ minHeight: '80px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Space>
              <ApiOutlined />
              <span style={{ fontWeight: 'bold' }}>LLM System</span>
            </Space>
            <Badge status={systemStatus.status as any} text={systemStatus.text} />
          </div>
          
          <Row gutter={16}>
            <Col span={8}>
              <Statistic 
                title="Providers" 
                value={`${summary.providers.healthy}/${summary.providers.total}`}
                valueStyle={{ fontSize: '14px' }}
              />
            </Col>
            <Col span={8}>
              <Statistic 
                title="Requests" 
                value={formatNumber(summary.usage.totalRequests)}
                valueStyle={{ fontSize: '14px' }}
              />
            </Col>
            <Col span={8}>
              <Statistic 
                title="Cost" 
                value={formatCurrency(summary.costs.currentMonth)}
                valueStyle={{ fontSize: '14px' }}
              />
            </Col>
          </Row>
          
          {showActions && (
            <div style={{ textAlign: 'center' }}>
              <Button 
                type="link" 
                size="small"
                icon={<SettingOutlined />}
                onClick={() => window.open('/admin/llm', '_blank')}
              >
                Manage
              </Button>
            </div>
          )}
        </Space>
      </Card>
    );
  }

  return (
    <Card 
      title={
        <Space>
          <ApiOutlined />
          LLM System Status
          <Badge status={systemStatus.status as any} text={systemStatus.text} />
        </Space>
      }
      size="small"
      extra={
        showActions && (
          <Space>
            <Tooltip title="Refresh status">
              <Button 
                type="text" 
                size="small"
                icon={<CheckCircleOutlined />}
                onClick={loadSummary}
              />
            </Tooltip>
            <Button 
              type="primary" 
              size="small"
              icon={<SettingOutlined />}
              onClick={() => window.open('/admin/llm', '_blank')}
            >
              Manage
            </Button>
          </Space>
        )
      }
    >
      <Row gutter={[16, 16]}>
        <Col span={6}>
          <Statistic
            title="Active Providers"
            value={summary.providers.healthy}
            suffix={`/ ${summary.providers.total}`}
            prefix={<ApiOutlined />}
            valueStyle={{ 
              color: summary.providers.healthy === summary.providers.total ? '#3f8600' : '#cf1322' 
            }}
          />
        </Col>
        
        <Col span={6}>
          <Statistic
            title="Total Requests (30d)"
            value={formatNumber(summary.usage.totalRequests)}
            prefix={<ThunderboltOutlined />}
            valueStyle={{ color: '#1890ff' }}
          />
        </Col>
        
        <Col span={6}>
          <Statistic
            title="Current Month Cost"
            value={formatCurrency(summary.costs.currentMonth)}
            prefix={<DollarOutlined />}
            valueStyle={{ color: '#fa8c16' }}
          />
        </Col>
        
        <Col span={6}>
          <Statistic
            title="Success Rate"
            value={(summary.performance.overallSuccessRate * 100).toFixed(1)}
            suffix="%"
            prefix={<CheckCircleOutlined />}
            valueStyle={{ 
              color: summary.performance.overallSuccessRate > 0.95 ? '#3f8600' : '#cf1322' 
            }}
          />
        </Col>
      </Row>

      <div style={{ 
        marginTop: '16px', 
        padding: '12px', 
        background: '#f5f5f5', 
        borderRadius: '6px',
        fontSize: '12px',
        color: '#666'
      }}>
        <Space split={<span>•</span>}>
          <span>Avg Response: {summary.performance.averageResponseTime}ms</span>
          <span>Total Tokens: {formatNumber(summary.usage.totalTokens)}</span>
          <span>Active Alerts: {summary.costs.activeAlerts}</span>
          <span>Last Updated: {new Date(summary.lastUpdated).toLocaleTimeString()}</span>
        </Space>
      </div>
    </Card>
  );
};
