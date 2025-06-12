/**
 * Usage Analytics Component
 *
 * Displays usage history, analytics, and detailed request/response logs
 * with filtering and export capabilities.
 */

import React, { useState, useEffect } from 'react';
import {
  Button,
  Space,
  DatePicker,
  Select,
  Input,
  Statistic,
  Row,
  Col,
  message,
  Flex,
  Tag,
  Modal,
  Typography
} from 'antd';
import {
  BarChartOutlined,
  DownloadOutlined,
  EyeOutlined,
  FilterOutlined,
  ApiOutlined,
  RobotOutlined,
  DollarOutlined,
  ClockCircleOutlined
} from '@ant-design/icons';
import { llmManagementService, LLMUsageLog, LLMUsageAnalytics, LLMProviderConfig } from '../../services/llmManagementService';
import { designTokens } from '../core/design-system';
import {
  LLMTable,
  LLMModal,
  LLMPageHeader,
  type LLMTableColumn
} from './components';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;
const { Option } = Select;
const { Search } = Input;
const { Text, Paragraph } = Typography;

export const UsageAnalytics: React.FC = () => {
  const [usageHistory, setUsageHistory] = useState<LLMUsageLog[]>([]);
  const [analytics, setAnalytics] = useState<LLMUsageAnalytics | null>(null);
  const [providers, setProviders] = useState<LLMProviderConfig[]>([]);
  const [loading, setLoading] = useState(true);
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [selectedLog, setSelectedLog] = useState<LLMUsageLog | null>(null);

  // Filters - Match dashboard date range (30 days)
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(30, 'days'),
    dayjs()
  ]);
  const [selectedProvider, setSelectedProvider] = useState<string | undefined>();
  const [selectedRequestType, setSelectedRequestType] = useState<string | undefined>();
  const [searchUserId, setSearchUserId] = useState<string>('');

  useEffect(() => {
    loadData();
    loadProviders();
  }, []);

  useEffect(() => {
    loadData();
  }, [dateRange, selectedProvider, selectedRequestType, searchUserId]);

  const loadProviders = async () => {
    try {
      const providersData = await llmManagementService.getProviders();
      setProviders(providersData);
    } catch (error) {
      console.error('Error loading providers:', error);
    }
  };

  const loadData = async () => {
    try {
      setLoading(true);
      const startDate = dateRange[0].format('YYYY-MM-DD');
      const endDate = dateRange[1].format('YYYY-MM-DD');

      console.log('Loading usage data for date range:', startDate, 'to', endDate);

      const [historyData, analyticsData] = await Promise.all([
        llmManagementService.getUsageHistory({
          startDate,
          endDate,
          providerId: selectedProvider,
          requestType: selectedRequestType,
          userId: searchUserId || undefined,
          take: 100
        }),
        llmManagementService.getUsageAnalytics(
          startDate,
          endDate,
          selectedProvider
        )
      ]);

      console.log('Usage history data:', historyData.length, 'records');
      console.log('Analytics data:', analyticsData);

      setUsageHistory(historyData);
      setAnalytics(analyticsData);
    } catch (error) {
      message.error('Failed to load usage analytics');
      console.error('Error loading usage analytics:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async () => {
    try {
      const startDate = dateRange[0].format('YYYY-MM-DD');
      const endDate = dateRange[1].format('YYYY-MM-DD');

      const blob = await llmManagementService.exportUsageData(startDate, endDate, 'csv');
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `usage-analytics-${startDate}-to-${endDate}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);

      message.success('Usage data exported successfully');
    } catch (error) {
      message.error('Failed to export usage data');
      console.error('Error exporting data:', error);
    }
  };

  const handleAddSampleData = async () => {
    try {
      const response = await fetch('http://localhost:55243/api/LLMManagement/debug/add-sample-data', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to add sample data');
      }

      const result = await response.json();
      message.success(`Sample data added successfully: ${result.count} records`);
      await loadData(); // Refresh the data
    } catch (error) {
      message.error('Failed to add sample data');
      console.error('Error adding sample data:', error);
    }
  };

  const showLogDetail = (log: LLMUsageLog) => {
    setSelectedLog(log);
    setDetailModalVisible(true);
  };

  const columns: LLMTableColumn[] = [
    {
      title: 'Timestamp',
      dataIndex: 'timestamp',
      key: 'timestamp',
      render: (timestamp: string) => (
        <div style={{
          fontSize: designTokens.typography.fontSize.sm,
          color: designTokens.colors.text
        }}>
          <div>{dayjs(timestamp).format('YYYY-MM-DD')}</div>
          <div style={{
            fontSize: designTokens.typography.fontSize.xs,
            color: designTokens.colors.textSecondary
          }}>
            {dayjs(timestamp).format('HH:mm:ss')}
          </div>
        </div>
      ),
      sorter: (a: LLMUsageLog, b: LLMUsageLog) => dayjs(a.timestamp).unix() - dayjs(b.timestamp).unix(),
    },
    {
      title: 'Provider',
      dataIndex: 'providerId',
      key: 'providerId',
      render: (providerId: string) => {
        const provider = providers.find(p => p.providerId === providerId);
        return (
          <div style={{ display: 'flex', alignItems: 'center', gap: designTokens.spacing.xs }}>
            <ApiOutlined style={{ color: designTokens.colors.primary }} />
            <Tag
              color="blue"
              style={{ borderRadius: designTokens.borderRadius.medium }}
            >
              {provider?.name || providerId}
            </Tag>
          </div>
        );
      },
    },
    {
      title: 'Model',
      dataIndex: 'modelId',
      key: 'modelId',
      render: (modelId: string) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: designTokens.spacing.xs }}>
          <RobotOutlined style={{ color: designTokens.colors.success }} />
          <span style={{
            fontSize: designTokens.typography.fontSize.sm,
            fontFamily: designTokens.typography.fontFamily.mono
          }}>
            {modelId}
          </span>
        </div>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'requestType',
      key: 'requestType',
      render: (type: string) => (
        <Tag
          color={type === 'SQL' ? 'blue' : type === 'Insights' ? 'green' : 'default'}
          style={{ borderRadius: designTokens.borderRadius.medium }}
        >
          {type}
        </Tag>
      ),
    },
    {
      title: 'User',
      dataIndex: 'userId',
      key: 'userId',
      render: (userId: string) => (
        <span style={{
          fontSize: designTokens.typography.fontSize.sm,
          color: designTokens.colors.textSecondary
        }}>
          {userId.length > 20 ? `${userId.substring(0, 20)}...` : userId}
        </span>
      ),
    },
    {
      title: 'Tokens',
      dataIndex: 'totalTokens',
      key: 'totalTokens',
      render: (tokens: number, record: LLMUsageLog) => (
        <div style={{ fontSize: designTokens.typography.fontSize.sm }}>
          <div style={{ fontWeight: designTokens.typography.fontWeight.semibold }}>
            {tokens.toLocaleString()}
          </div>
          <div style={{
            fontSize: designTokens.typography.fontSize.xs,
            color: designTokens.colors.textSecondary
          }}>
            {record.inputTokens}↑ {record.outputTokens}↓
          </div>
        </div>
      ),
    },
    {
      title: 'Cost',
      dataIndex: 'cost',
      key: 'cost',
      render: (cost: number) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: designTokens.spacing.xs }}>
          <DollarOutlined style={{ color: designTokens.colors.warning }} />
          <span style={{
            fontFamily: designTokens.typography.fontFamily.mono,
            fontSize: designTokens.typography.fontSize.sm,
            color: designTokens.colors.warning
          }}>
            ${cost.toFixed(6)}
          </span>
        </div>
      ),
    },
    {
      title: 'Duration',
      dataIndex: 'durationMs',
      key: 'durationMs',
      render: (duration: number) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: designTokens.spacing.xs }}>
          <ClockCircleOutlined style={{ color: designTokens.colors.info }} />
          <span style={{
            fontSize: designTokens.typography.fontSize.sm,
            color: duration > 5000 ? designTokens.colors.danger : designTokens.colors.text
          }}>
            {duration}ms
          </span>
        </div>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'success',
      key: 'success',
      render: (success: boolean) => (
        <Tag
          color={success ? 'green' : 'red'}
          style={{ borderRadius: designTokens.borderRadius.medium }}
        >
          {success ? '✅ Success' : '❌ Failed'}
        </Tag>
      ),
    },
  ];

  const tableActions = [
    {
      key: 'view',
      label: '',
      icon: <EyeOutlined />,
      onClick: (record: LLMUsageLog) => showLogDetail(record),
      tooltip: 'View request details',
    },
  ];

  return (
    <div style={{ padding: designTokens.spacing.lg }}>
      {/* Header */}
      <LLMPageHeader
        title="Usage Analytics"
        description="Monitor LLM usage, costs, and performance across all providers and models"
        onRefresh={loadData}
        refreshLoading={loading}
        actions={[
          {
            key: 'add-sample',
            label: 'Add Sample Data',
            icon: <ApiOutlined />,
            onClick: handleAddSampleData,
            type: 'default',
          },
          {
            key: 'export',
            label: 'Export Data',
            icon: <DownloadOutlined />,
            onClick: handleExport,
            type: 'primary',
          },
        ]}
      />

      {/* Summary Statistics */}
      {analytics && (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
          gap: designTokens.spacing.md,
          marginBottom: designTokens.spacing.lg
        }}>
          <div style={{
            background: `linear-gradient(135deg, ${designTokens.colors.primaryLight} 0%, ${designTokens.colors.white} 100%)`,
            border: `1px solid ${designTokens.colors.border}`,
            borderRadius: designTokens.borderRadius.large,
            padding: designTokens.spacing.lg,
            boxShadow: designTokens.shadows.medium,
          }}>
            <Flex align="center" gap="middle">
              <div style={{
                width: '48px',
                height: '48px',
                borderRadius: designTokens.borderRadius.large,
                background: `linear-gradient(135deg, ${designTokens.colors.primary} 0%, ${designTokens.colors.primaryHover} 100%)`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: designTokens.colors.white,
                fontSize: designTokens.typography.fontSize.lg,
              }}>
                <BarChartOutlined />
              </div>
              <div>
                <div style={{
                  fontSize: designTokens.typography.fontSize['2xl'],
                  fontWeight: designTokens.typography.fontWeight.bold,
                  color: designTokens.colors.primary
                }}>
                  {analytics.totalRequests.toLocaleString()}
                </div>
                <div style={{
                  fontSize: designTokens.typography.fontSize.sm,
                  color: designTokens.colors.textSecondary
                }}>
                  Total Requests
                </div>
              </div>
            </Flex>
          </div>

          <div style={{
            background: `linear-gradient(135deg, ${designTokens.colors.successLight} 0%, ${designTokens.colors.white} 100%)`,
            border: `1px solid ${designTokens.colors.border}`,
            borderRadius: designTokens.borderRadius.large,
            padding: designTokens.spacing.lg,
            boxShadow: designTokens.shadows.medium,
          }}>
            <Flex align="center" gap="middle">
              <div style={{
                width: '48px',
                height: '48px',
                borderRadius: designTokens.borderRadius.large,
                background: `linear-gradient(135deg, ${designTokens.colors.success} 0%, ${designTokens.colors.successHover} 100%)`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: designTokens.colors.white,
                fontSize: designTokens.typography.fontSize.lg,
              }}>
                <RobotOutlined />
              </div>
              <div>
                <div style={{
                  fontSize: designTokens.typography.fontSize['2xl'],
                  fontWeight: designTokens.typography.fontWeight.bold,
                  color: designTokens.colors.success
                }}>
                  {analytics.totalTokens.toLocaleString()}
                </div>
                <div style={{
                  fontSize: designTokens.typography.fontSize.sm,
                  color: designTokens.colors.textSecondary
                }}>
                  Total Tokens
                </div>
              </div>
            </Flex>
          </div>

          <div style={{
            background: `linear-gradient(135deg, ${designTokens.colors.warningLight} 0%, ${designTokens.colors.white} 100%)`,
            border: `1px solid ${designTokens.colors.border}`,
            borderRadius: designTokens.borderRadius.large,
            padding: designTokens.spacing.lg,
            boxShadow: designTokens.shadows.medium,
          }}>
            <Flex align="center" gap="middle">
              <div style={{
                width: '48px',
                height: '48px',
                borderRadius: designTokens.borderRadius.large,
                background: `linear-gradient(135deg, ${designTokens.colors.warning} 0%, ${designTokens.colors.warningHover} 100%)`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: designTokens.colors.white,
                fontSize: designTokens.typography.fontSize.lg,
              }}>
                <DollarOutlined />
              </div>
              <div>
                <div style={{
                  fontSize: designTokens.typography.fontSize['2xl'],
                  fontWeight: designTokens.typography.fontWeight.bold,
                  color: designTokens.colors.warning
                }}>
                  ${analytics.totalCost.toFixed(4)}
                </div>
                <div style={{
                  fontSize: designTokens.typography.fontSize.sm,
                  color: designTokens.colors.textSecondary
                }}>
                  Total Cost
                </div>
              </div>
            </Flex>
          </div>

          <div style={{
            background: `linear-gradient(135deg, ${designTokens.colors.infoLight} 0%, ${designTokens.colors.white} 100%)`,
            border: `1px solid ${designTokens.colors.border}`,
            borderRadius: designTokens.borderRadius.large,
            padding: designTokens.spacing.lg,
            boxShadow: designTokens.shadows.medium,
          }}>
            <Flex align="center" gap="middle">
              <div style={{
                width: '48px',
                height: '48px',
                borderRadius: designTokens.borderRadius.large,
                background: `linear-gradient(135deg, ${designTokens.colors.info} 0%, ${designTokens.colors.infoHover} 100%)`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: designTokens.colors.white,
                fontSize: designTokens.typography.fontSize.lg,
              }}>
                <ClockCircleOutlined />
              </div>
              <div>
                <div style={{
                  fontSize: designTokens.typography.fontSize['2xl'],
                  fontWeight: designTokens.typography.fontWeight.bold,
                  color: analytics.successRate > 0.95 ? designTokens.colors.success : designTokens.colors.danger
                }}>
                  {(analytics.successRate * 100).toFixed(1)}%
                </div>
                <div style={{
                  fontSize: designTokens.typography.fontSize.sm,
                  color: designTokens.colors.textSecondary
                }}>
                  Success Rate
                </div>
              </div>
            </Flex>
          </div>
        </div>
      )}

      {/* Filters */}
      <div style={{
        background: designTokens.colors.white,
        border: `1px solid ${designTokens.colors.border}`,
        borderRadius: designTokens.borderRadius.large,
        padding: designTokens.spacing.md,
        marginBottom: designTokens.spacing.md,
        boxShadow: designTokens.shadows.small,
      }}>
        <Flex justify="between" align="center" wrap="wrap" gap="middle">
          <Space wrap>
            <RangePicker
              value={dateRange}
              onChange={(dates) => dates && setDateRange(dates as [dayjs.Dayjs, dayjs.Dayjs])}
              format="YYYY-MM-DD"
              style={{ borderRadius: designTokens.borderRadius.medium }}
            />
            <Select
              placeholder="Select Provider"
              style={{
                width: 150,
                borderRadius: designTokens.borderRadius.medium
              }}
              value={selectedProvider}
              onChange={setSelectedProvider}
              allowClear
            >
              {providers.map(provider => (
                <Option key={provider.providerId} value={provider.providerId}>
                  {provider.name}
                </Option>
              ))}
            </Select>
            <Select
              placeholder="Request Type"
              style={{
                width: 120,
                borderRadius: designTokens.borderRadius.medium
              }}
              value={selectedRequestType}
              onChange={setSelectedRequestType}
              allowClear
            >
              <Option value="SQL">SQL</Option>
              <Option value="Insights">Insights</Option>
              <Option value="General">General</Option>
            </Select>
            <Search
              placeholder="Search by User ID"
              style={{
                width: 200,
                borderRadius: designTokens.borderRadius.medium
              }}
              value={searchUserId}
              onChange={(e) => setSearchUserId(e.target.value)}
              allowClear
            />
          </Space>
        </Flex>
      </div>

      {/* Usage History Table */}
      <LLMTable
        columns={columns}
        dataSource={usageHistory}
        rowKey="id"
        loading={loading}
        actions={tableActions}
        actionColumnWidth={80}
        pagination={{
          pageSize: 20,
          showSizeChanger: true,
          showQuickJumper: true,
          showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
        }}
      />

      {/* Log Detail Modal */}
      <LLMModal
        title="Request Details"
        open={detailModalVisible}
        onCancel={() => setDetailModalVisible(false)}
        footer={null}
        width={900}
      >
        {selectedLog && (
          <div style={{ padding: designTokens.spacing.sm }}>
            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr',
              gap: designTokens.spacing.md,
              marginBottom: designTokens.spacing.md
            }}>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Request ID:</Text>
                <div style={{
                  fontFamily: designTokens.typography.fontFamily.mono,
                  fontSize: designTokens.typography.fontSize.sm,
                  color: designTokens.colors.textSecondary,
                  marginTop: designTokens.spacing.xs
                }}>
                  {selectedLog.requestId}
                </div>
              </div>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Timestamp:</Text>
                <div style={{
                  fontSize: designTokens.typography.fontSize.sm,
                  color: designTokens.colors.textSecondary,
                  marginTop: designTokens.spacing.xs
                }}>
                  {dayjs(selectedLog.timestamp).format('YYYY-MM-DD HH:mm:ss')}
                </div>
              </div>
            </div>

            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr',
              gap: designTokens.spacing.md,
              marginBottom: designTokens.spacing.md
            }}>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Provider:</Text>
                <div style={{ marginTop: designTokens.spacing.xs }}>
                  <Tag color="blue" style={{ borderRadius: designTokens.borderRadius.medium }}>
                    {selectedLog.providerId}
                  </Tag>
                </div>
              </div>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Model:</Text>
                <div style={{ marginTop: designTokens.spacing.xs }}>
                  <Tag color="green" style={{ borderRadius: designTokens.borderRadius.medium }}>
                    {selectedLog.modelId}
                  </Tag>
                </div>
              </div>
            </div>

            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr',
              gap: designTokens.spacing.md,
              marginBottom: designTokens.spacing.md
            }}>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>User:</Text>
                <div style={{
                  fontSize: designTokens.typography.fontSize.sm,
                  color: designTokens.colors.textSecondary,
                  marginTop: designTokens.spacing.xs
                }}>
                  {selectedLog.userId}
                </div>
              </div>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Type:</Text>
                <div style={{ marginTop: designTokens.spacing.xs }}>
                  <Tag
                    color={selectedLog.requestType === 'SQL' ? 'blue' : selectedLog.requestType === 'Insights' ? 'green' : 'default'}
                    style={{ borderRadius: designTokens.borderRadius.medium }}
                  >
                    {selectedLog.requestType}
                  </Tag>
                </div>
              </div>
            </div>

            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr 1fr',
              gap: designTokens.spacing.md,
              marginBottom: designTokens.spacing.md
            }}>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Input Tokens:</Text>
                <div style={{
                  fontSize: designTokens.typography.fontSize.lg,
                  fontWeight: designTokens.typography.fontWeight.semibold,
                  color: designTokens.colors.success,
                  marginTop: designTokens.spacing.xs
                }}>
                  {selectedLog.inputTokens.toLocaleString()}
                </div>
              </div>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Output Tokens:</Text>
                <div style={{
                  fontSize: designTokens.typography.fontSize.lg,
                  fontWeight: designTokens.typography.fontWeight.semibold,
                  color: designTokens.colors.info,
                  marginTop: designTokens.spacing.xs
                }}>
                  {selectedLog.outputTokens.toLocaleString()}
                </div>
              </div>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Total Tokens:</Text>
                <div style={{
                  fontSize: designTokens.typography.fontSize.lg,
                  fontWeight: designTokens.typography.fontWeight.semibold,
                  color: designTokens.colors.primary,
                  marginTop: designTokens.spacing.xs
                }}>
                  {selectedLog.totalTokens.toLocaleString()}
                </div>
              </div>
            </div>

            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr 1fr',
              gap: designTokens.spacing.md,
              marginBottom: designTokens.spacing.lg
            }}>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Cost:</Text>
                <div style={{
                  fontSize: designTokens.typography.fontSize.lg,
                  fontWeight: designTokens.typography.fontWeight.semibold,
                  color: designTokens.colors.warning,
                  fontFamily: designTokens.typography.fontFamily.mono,
                  marginTop: designTokens.spacing.xs
                }}>
                  ${selectedLog.cost.toFixed(6)}
                </div>
              </div>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Duration:</Text>
                <div style={{
                  fontSize: designTokens.typography.fontSize.lg,
                  fontWeight: designTokens.typography.fontWeight.semibold,
                  color: selectedLog.durationMs > 5000 ? designTokens.colors.danger : designTokens.colors.success,
                  marginTop: designTokens.spacing.xs
                }}>
                  {selectedLog.durationMs}ms
                </div>
              </div>
              <div>
                <Text strong style={{ color: designTokens.colors.text }}>Status:</Text>
                <div style={{ marginTop: designTokens.spacing.xs }}>
                  <Tag
                    color={selectedLog.success ? 'green' : 'red'}
                    style={{ borderRadius: designTokens.borderRadius.medium }}
                  >
                    {selectedLog.success ? '✅ Success' : '❌ Failed'}
                  </Tag>
                </div>
              </div>
            </div>

            <div style={{ marginBottom: designTokens.spacing.md }}>
              <Text strong style={{ color: designTokens.colors.text }}>Request:</Text>
              <Paragraph
                copyable
                style={{
                  background: designTokens.colors.backgroundSecondary,
                  padding: designTokens.spacing.md,
                  marginTop: designTokens.spacing.sm,
                  maxHeight: '200px',
                  overflow: 'auto',
                  borderRadius: designTokens.borderRadius.medium,
                  border: `1px solid ${designTokens.colors.border}`,
                  fontFamily: designTokens.typography.fontFamily.mono,
                  fontSize: designTokens.typography.fontSize.sm,
                  lineHeight: designTokens.typography.lineHeight.relaxed,
                }}
              >
                {selectedLog.requestText}
              </Paragraph>
            </div>

            <div style={{ marginBottom: designTokens.spacing.md }}>
              <Text strong style={{ color: designTokens.colors.text }}>Response:</Text>
              <Paragraph
                copyable
                style={{
                  background: designTokens.colors.backgroundSecondary,
                  padding: designTokens.spacing.md,
                  marginTop: designTokens.spacing.sm,
                  maxHeight: '200px',
                  overflow: 'auto',
                  borderRadius: designTokens.borderRadius.medium,
                  border: `1px solid ${designTokens.colors.border}`,
                  fontFamily: designTokens.typography.fontFamily.mono,
                  fontSize: designTokens.typography.fontSize.sm,
                  lineHeight: designTokens.typography.lineHeight.relaxed,
                }}
              >
                {selectedLog.responseText}
              </Paragraph>
            </div>

            {selectedLog.errorMessage && (
              <div>
                <Text strong style={{ color: designTokens.colors.danger }}>Error:</Text>
                <Paragraph
                  style={{
                    background: designTokens.colors.dangerLight,
                    padding: designTokens.spacing.md,
                    marginTop: designTokens.spacing.sm,
                    border: `1px solid ${designTokens.colors.danger}`,
                    borderRadius: designTokens.borderRadius.medium,
                    fontFamily: designTokens.typography.fontFamily.mono,
                    fontSize: designTokens.typography.fontSize.sm,
                    color: designTokens.colors.danger,
                  }}
                >
                  {selectedLog.errorMessage}
                </Paragraph>
              </div>
            )}
          </div>
        )}
      </LLMModal>
    </div>
  );
};
