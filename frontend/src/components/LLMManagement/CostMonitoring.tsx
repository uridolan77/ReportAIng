/**
 * Cost Monitoring Component
 *
 * Monitors and manages LLM costs including real-time tracking,
 * alerts, limits, and cost projections.
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Space,
  Statistic,
  Row,
  Col,
  Modal,
  Form,
  InputNumber,
  Select,
  message,
  Flex,
  Tag,
  Alert,
  Progress,
  DatePicker
} from 'antd';
import {
  DollarOutlined,
  BellOutlined,
  ReloadOutlined,
  SettingOutlined,
  TrendingUpOutlined,
  WarningOutlined
} from '@ant-design/icons';
import {
  llmManagementService,
  LLMCostSummary,
  LLMProviderConfig,
  CostAlert
} from '../../services/llmManagementService';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;
const { Option } = Select;

export const CostMonitoring: React.FC = () => {
  const [costSummary, setCostSummary] = useState<LLMCostSummary[]>([]);
  const [providers, setProviders] = useState<LLMProviderConfig[]>([]);
  const [costAlerts, setCostAlerts] = useState<CostAlert[]>([]);
  const [currentMonthCost, setCurrentMonthCost] = useState<number>(0);
  const [costProjections, setCostProjections] = useState<Record<string, number>>({});
  const [loading, setLoading] = useState(true);
  const [limitModalVisible, setLimitModalVisible] = useState(false);
  const [selectedProvider, setSelectedProvider] = useState<string>('');
  const [form] = Form.useForm();

  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().subtract(30, 'days'),
    dayjs()
  ]);

  useEffect(() => {
    loadData();
    loadProviders();
  }, []);

  useEffect(() => {
    loadCostSummary();
  }, [dateRange]);

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
      const [currentCost, projections, alerts] = await Promise.all([
        llmManagementService.getCurrentMonthCost(),
        llmManagementService.getCostProjections(),
        llmManagementService.getCostAlerts()
      ]);

      setCurrentMonthCost(currentCost);
      setCostProjections(projections);
      setCostAlerts(alerts);
    } catch (error) {
      message.error('Failed to load cost monitoring data');
      console.error('Error loading cost data:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadCostSummary = async () => {
    try {
      const startDate = dateRange[0].format('YYYY-MM-DD');
      const endDate = dateRange[1].format('YYYY-MM-DD');

      const summary = await llmManagementService.getCostSummary(startDate, endDate);
      setCostSummary(summary);
    } catch (error) {
      message.error('Failed to load cost summary');
      console.error('Error loading cost summary:', error);
    }
  };

  const handleSetCostLimit = async (values: any) => {
    try {
      await llmManagementService.setCostLimit(selectedProvider, values.monthlyLimit);
      setLimitModalVisible(false);
      message.success('Cost limit set successfully');
      await loadData();
    } catch (error) {
      message.error('Failed to set cost limit');
      console.error('Error setting cost limit:', error);
    }
  };

  const showSetLimitModal = (providerId: string) => {
    setSelectedProvider(providerId);
    form.resetFields();
    setLimitModalVisible(true);
  };

  const costColumns = [
    {
      title: 'Date',
      dataIndex: 'date',
      key: 'date',
      render: (date: string) => dayjs(date).format('YYYY-MM-DD'),
      sorter: (a: LLMCostSummary, b: LLMCostSummary) => dayjs(a.date).unix() - dayjs(b.date).unix(),
    },
    {
      title: 'Provider',
      dataIndex: 'providerId',
      key: 'providerId',
      render: (providerId: string) => {
        const provider = providers.find(p => p.providerId === providerId);
        return provider?.name || providerId;
      },
    },
    {
      title: 'Model',
      dataIndex: 'modelId',
      key: 'modelId',
    },
    {
      title: 'Requests',
      dataIndex: 'totalRequests',
      key: 'totalRequests',
      render: (requests: number) => requests.toLocaleString(),
    },
    {
      title: 'Tokens',
      dataIndex: 'totalTokens',
      key: 'totalTokens',
      render: (tokens: number) => tokens.toLocaleString(),
    },
    {
      title: 'Total Cost',
      dataIndex: 'totalCost',
      key: 'totalCost',
      render: (cost: number) => `$${cost.toFixed(4)}`,
      sorter: (a: LLMCostSummary, b: LLMCostSummary) => a.totalCost - b.totalCost,
    },
    {
      title: 'Avg Cost',
      dataIndex: 'averageCost',
      key: 'averageCost',
      render: (cost: number) => `$${cost.toFixed(6)}`,
    },
    {
      title: 'Success Rate',
      dataIndex: 'successRate',
      key: 'successRate',
      render: (rate: number) => `${(rate * 100).toFixed(1)}%`,
    },
  ];

  const alertColumns = [
    {
      title: 'Provider',
      dataIndex: 'providerId',
      key: 'providerId',
      render: (providerId: string) => {
        const provider = providers.find(p => p.providerId === providerId);
        return provider?.name || providerId;
      },
    },
    {
      title: 'Alert Type',
      dataIndex: 'alertType',
      key: 'alertType',
      render: (type: string) => (
        <Tag color={type === 'EXCEEDED' ? 'red' : 'orange'}>
          {type}
        </Tag>
      ),
    },
    {
      title: 'Threshold',
      dataIndex: 'thresholdAmount',
      key: 'thresholdAmount',
      render: (amount: number) => `$${amount.toFixed(2)}`,
    },
    {
      title: 'Status',
      dataIndex: 'isEnabled',
      key: 'isEnabled',
      render: (enabled: boolean) => (
        <Tag color={enabled ? 'green' : 'red'}>
          {enabled ? 'Active' : 'Disabled'}
        </Tag>
      ),
    },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => dayjs(date).format('YYYY-MM-DD HH:mm'),
    },
  ];

  const totalProjectedCost = Object.values(costProjections).reduce((sum, cost) => sum + cost, 0);

  return (
    <div style={{ padding: '24px' }}>
      {/* Cost Overview */}
      <Row gutter={16} style={{ marginBottom: '24px' }}>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Current Month Cost"
              value={currentMonthCost}
              precision={2}
              prefix="$"
              valueStyle={{ color: '#fa8c16' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Projected Month Cost"
              value={totalProjectedCost}
              precision={2}
              prefix="$"
              valueStyle={{ color: totalProjectedCost > currentMonthCost * 1.5 ? '#ff4d4f' : '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Active Alerts"
              value={costAlerts.length}
              valueStyle={{ color: costAlerts.length > 0 ? '#ff4d4f' : '#52c41a' }}
              prefix={costAlerts.length > 0 ? <WarningOutlined /> : undefined}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Total Providers"
              value={providers.length}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Cost Alerts */}
      {costAlerts.length > 0 && (
        <Alert
          message="Cost Alerts Active"
          description={`You have ${costAlerts.length} active cost alert(s). Please review your spending.`}
          type="warning"
          showIcon
          style={{ marginBottom: '24px' }}
        />
      )}

      {/* Provider Cost Projections */}
      <Card
        title="Provider Cost Projections"
        size="small"
        style={{ marginBottom: '16px' }}
        extra={
          <Button
            type="primary"
            icon={<SettingOutlined />}
            onClick={() => showSetLimitModal('')}
          >
            Set Limits
          </Button>
        }
      >
        <Row gutter={16}>
          {providers.map(provider => {
            const projection = costProjections[provider.providerId] || 0;
            const currentCost = currentMonthCost; // This should be per-provider in real implementation
            const percentage = currentCost > 0 ? Math.min((currentCost / projection) * 100, 100) : 0;

            return (
              <Col span={8} key={provider.providerId} style={{ marginBottom: '16px' }}>
                <Card size="small">
                  <div style={{ marginBottom: '8px' }}>
                    <Flex justify="between" align="center">
                      <span style={{ fontWeight: 'bold' }}>{provider.name}</span>
                      <Button
                        type="text"
                        size="small"
                        icon={<SettingOutlined />}
                        onClick={() => showSetLimitModal(provider.providerId)}
                      />
                    </Flex>
                  </div>
                  <div style={{ marginBottom: '8px' }}>
                    <span style={{ fontSize: '18px', fontWeight: 'bold' }}>
                      ${projection.toFixed(2)}
                    </span>
                    <span style={{ color: '#666', marginLeft: '8px' }}>projected</span>
                  </div>
                  <Progress
                    percent={percentage}
                    size="small"
                    status={percentage > 80 ? 'exception' : percentage > 60 ? 'active' : 'success'}
                  />
                </Card>
              </Col>
            );
          })}
        </Row>
      </Card>

      {/* Filters */}
      <Card size="small" style={{ marginBottom: '16px' }}>
        <Flex justify="between" align="center">
          <Space>
            <RangePicker
              value={dateRange}
              onChange={(dates) => dates && setDateRange(dates as [dayjs.Dayjs, dayjs.Dayjs])}
              format="YYYY-MM-DD"
            />
          </Space>
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={loadData}
            >
              Refresh
            </Button>
          </Space>
        </Flex>
      </Card>

      {/* Cost Summary Table */}
      <Card title="Cost Summary" size="small" style={{ marginBottom: '16px' }}>
        <Table
          columns={costColumns}
          dataSource={costSummary}
          rowKey={(record) => `${record.providerId}-${record.modelId}-${record.date}`}
          loading={loading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
          }}
          scroll={{ x: 800 }}
        />
      </Card>

      {/* Cost Alerts Table */}
      <Card title="Cost Alerts" size="small">
        <Table
          columns={alertColumns}
          dataSource={costAlerts}
          rowKey="id"
          pagination={false}
          locale={{ emptyText: 'No active cost alerts' }}
        />
      </Card>

      {/* Set Cost Limit Modal */}
      <Modal
        title="Set Cost Limit"
        open={limitModalVisible}
        onCancel={() => setLimitModalVisible(false)}
        onOk={() => form.submit()}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSetCostLimit}
        >
          <Form.Item
            name="providerId"
            label="Provider"
            rules={[{ required: true, message: 'Please select a provider' }]}
          >
            <Select placeholder="Select provider">
              {providers.map(provider => (
                <Option key={provider.providerId} value={provider.providerId}>
                  {provider.name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="monthlyLimit"
            label="Monthly Limit ($)"
            rules={[{ required: true, message: 'Please enter monthly limit' }]}
          >
            <InputNumber
              min={0}
              step={0.01}
              style={{ width: '100%' }}
              placeholder="e.g., 100.00"
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};
