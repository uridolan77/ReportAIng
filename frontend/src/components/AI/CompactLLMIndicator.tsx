/**
 * Compact LLM Status Indicator
 * 
 * A compact status indicator for the header showing LLM system health
 */

import React, { useState, useEffect } from 'react';
import { Tooltip, Space, Typography, Button, Modal } from 'antd';
import {
  ApiOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  ReloadOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { llmManagementService, DashboardSummary } from '../../services/llmManagementService';

const { Text } = Typography;

export const CompactLLMIndicator: React.FC = () => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showDetails, setShowDetails] = useState(false);

  const loadSummary = async () => {
    setLoading(true);
    try {
      setError(null);
      const data = await llmManagementService.getDashboardSummary();
      setSummary(data);
    } catch (err) {
      console.warn('LLM Status Indicator error:', err);
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

  useEffect(() => {
    // Delay initial check to allow authentication to settle
    const initialTimeout = setTimeout(() => {
      loadSummary();
    }, 1500);

    // Check status every 30 seconds
    const interval = setInterval(loadSummary, 30000);

    return () => {
      clearTimeout(initialTimeout);
      clearInterval(interval);
    };
  }, []);

  const getSystemStatus = () => {
    if (!summary) return { status: 'default', text: 'Unknown', connected: false };
    
    const allHealthy = summary.providers.healthy === summary.providers.total;
    const hasEnabledProviders = summary.providers.enabled > 0;
    
    if (allHealthy && hasEnabledProviders) {
      return { status: 'success', text: 'Healthy', connected: true };
    } else if (hasEnabledProviders) {
      return { status: 'warning', text: 'Issues', connected: true };
    } else {
      return { status: 'error', text: 'No Providers', connected: false };
    }
  };

  const getStatusIcon = () => {
    if (loading) {
      return <ReloadOutlined spin />;
    }

    if (error) {
      return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
    }

    const systemStatus = getSystemStatus();
    if (systemStatus.connected) {
      return systemStatus.status === 'success' 
        ? <CheckCircleOutlined style={{ color: '#52c41a' }} />
        : <ExclamationCircleOutlined style={{ color: '#faad14' }} />;
    }

    return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
  };

  const getTooltipTitle = () => {
    if (loading) return 'Checking LLM system...';

    if (error) {
      return (
        <div>
          <div>✗ LLM System Unavailable</div>
          <div>Error: {error}</div>
        </div>
      );
    }

    if (!summary) return 'LLM status unknown';

    const systemStatus = getSystemStatus();
    return (
      <div>
        <div>🤖 LLM System: {systemStatus.text}</div>
        <div>Providers: {summary.providers.healthy}/{summary.providers.total}</div>
        <div>Success Rate: {(summary.performance.overallSuccessRate * 100).toFixed(1)}%</div>
        <div>Avg Response: {summary.performance.averageResponseTime}ms</div>
        <div>Last updated: {new Date(summary.lastUpdated).toLocaleTimeString()}</div>
      </div>
    );
  };

  const systemStatus = getSystemStatus();

  return (
    <>
      <Tooltip
        title={getTooltipTitle()}
        classNames={{ root: "llm-status-tooltip" }}
      >
        <div
          className={`llm-status-indicator ${systemStatus.connected ? 'connected' : 'disconnected'}`}
          onClick={() => setShowDetails(true)}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            padding: '6px 12px',
            borderRadius: '4px',
            cursor: 'pointer',
            background: systemStatus.connected
              ? (systemStatus.status === 'success' ? 'rgba(34, 197, 94, 0.1)' : 'rgba(250, 173, 20, 0.1)')
              : 'rgba(239, 68, 68, 0.1)',
            border: `1px solid ${
              systemStatus.connected
                ? (systemStatus.status === 'success' ? '#22c55e' : '#faad14')
                : '#ef4444'
            }`,
            transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
            backdropFilter: 'blur(8px)',
            height: '36px',
            boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-1px)';
            e.currentTarget.style.boxShadow = systemStatus.connected
              ? (systemStatus.status === 'success' 
                  ? '0 4px 12px rgba(34, 197, 94, 0.15)' 
                  : '0 4px 12px rgba(250, 173, 20, 0.15)')
              : '0 4px 12px rgba(239, 68, 68, 0.15)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)';
          }}
          data-testid="llm-status"
        >
          {getStatusIcon()}
          <Text style={{
            fontSize: '13px',
            color: systemStatus.connected
              ? (systemStatus.status === 'success' ? '#22c55e' : '#faad14')
              : '#ef4444',
            fontWeight: 600,
            fontFamily: "'Inter', sans-serif"
          }}>
            {systemStatus.connected ? 'LLM Ready' : 'LLM Offline'}
          </Text>
        </div>
      </Tooltip>

      <Modal
        title={
          <Space>
            <ApiOutlined />
            LLM System Status
          </Space>
        }
        open={showDetails}
        onCancel={() => setShowDetails(false)}
        footer={[
          <Button key="refresh" icon={<ReloadOutlined />} onClick={loadSummary} loading={loading}>
            Refresh
          </Button>,
          <Button 
            key="manage" 
            type="primary"
            icon={<SettingOutlined />}
            onClick={() => window.open('/admin/llm', '_blank')}
          >
            Manage
          </Button>,
          <Button key="close" onClick={() => setShowDetails(false)}>
            Close
          </Button>
        ]}
        width={600}
        className="llm-status-modal"
      >
        {summary && (
          <div style={{ padding: '16px 0' }}>
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Text strong>System Health: </Text>
                <Text style={{ 
                  color: systemStatus.status === 'success' ? '#52c41a' : 
                        systemStatus.status === 'warning' ? '#faad14' : '#ff4d4f' 
                }}>
                  {systemStatus.text}
                </Text>
              </div>
              
              <div>
                <Text strong>Providers: </Text>
                <Text>{summary.providers.healthy}/{summary.providers.total} healthy</Text>
              </div>
              
              <div>
                <Text strong>Performance: </Text>
                <Text>
                  {(summary.performance.overallSuccessRate * 100).toFixed(1)}% success rate, 
                  {summary.performance.averageResponseTime}ms avg response
                </Text>
              </div>
              
              <div>
                <Text strong>Usage (30d): </Text>
                <Text>{summary.usage.totalRequests.toLocaleString()} requests</Text>
              </div>
              
              <div>
                <Text strong>Cost (current month): </Text>
                <Text>${summary.costs.currentMonth.toFixed(2)}</Text>
              </div>
            </Space>
          </div>
        )}
        
        {error && (
          <div style={{ 
            padding: '16px', 
            background: '#fff2f0', 
            border: '1px solid #ffccc7',
            borderRadius: '6px',
            marginTop: '16px'
          }}>
            <Space>
              <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
              <Text type="danger">
                LLM system is currently unavailable. Please check the system configuration.
              </Text>
            </Space>
          </div>
        )}
      </Modal>
    </>
  );
};
