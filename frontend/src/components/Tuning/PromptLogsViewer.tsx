import React, { useState, useEffect } from 'react';
import {
  Table,
  Card,
  Typography,
  Space,
  Button,
  Modal,
  Tag,
  DatePicker,
  Select,
  Row,
  Col,
  Tooltip,
  message,
  Spin
} from 'antd';
import {
  EyeOutlined,
  ReloadOutlined,
  FilterOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined
} from '@ant-design/icons';
import { tuningApi } from '../../services/tuningApi';

const { Title, Text, Paragraph } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;

interface PromptLog {
  id: number;
  promptType: string;
  userQuery: string;
  fullPrompt: string;
  generatedSQL?: string;
  success?: boolean;
  errorMessage?: string;
  promptLength: number;
  responseLength?: number;
  executionTimeMs?: number;
  createdDate: string;
  userId?: string;
  sessionId?: string;
  metadata?: string;
}

export const PromptLogsViewer: React.FC = () => {
  const [logs, setLogs] = useState<PromptLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedLog, setSelectedLog] = useState<PromptLog | null>(null);
  const [modalVisible, setModalVisible] = useState(false);
  const [filters, setFilters] = useState({
    promptType: undefined as string | undefined,
    success: undefined as boolean | undefined,
    dateRange: undefined as [string, string] | undefined
  });

  useEffect(() => {
    loadLogs();
  }, []);

  const loadLogs = async () => {
    try {
      setLoading(true);
      const response = await tuningApi.getPromptLogs({
        page: 1,
        pageSize: 100,
        promptType: filters.promptType,
        success: filters.success,
        fromDate: filters.dateRange?.[0],
        toDate: filters.dateRange?.[1]
      });
      setLogs(response);
    } catch (error) {
      message.error('Failed to load prompt logs');
      console.error('Error loading prompt logs:', error);
    } finally {
      setLoading(false);
    }
  };

  const viewLogDetails = (log: PromptLog) => {
    setSelectedLog(log);
    setModalVisible(true);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getSuccessTag = (success?: boolean) => {
    if (success === undefined) return <Tag>Unknown</Tag>;
    return success ? 
      <Tag color="green" icon={<CheckCircleOutlined />}>Success</Tag> :
      <Tag color="red" icon={<CloseCircleOutlined />}>Failed</Tag>;
  };

  const columns = [
    {
      title: 'Date',
      dataIndex: 'createdDate',
      key: 'createdDate',
      render: (date: string) => (
        <Tooltip title={formatDate(date)}>
          <Text>{new Date(date).toLocaleDateString()}</Text>
        </Tooltip>
      ),
      sorter: (a: PromptLog, b: PromptLog) => 
        new Date(a.createdDate).getTime() - new Date(b.createdDate).getTime(),
      defaultSortOrder: 'descend' as const
    },
    {
      title: 'Type',
      dataIndex: 'promptType',
      key: 'promptType',
      render: (type: string) => <Tag color="blue">{type}</Tag>
    },
    {
      title: 'User Query',
      dataIndex: 'userQuery',
      key: 'userQuery',
      render: (query: string) => (
        <Text ellipsis style={{ maxWidth: 200 }}>
          {query}
        </Text>
      )
    },
    {
      title: 'Status',
      dataIndex: 'success',
      key: 'success',
      render: (success: boolean) => getSuccessTag(success)
    },
    {
      title: 'Execution Time',
      dataIndex: 'executionTimeMs',
      key: 'executionTimeMs',
      render: (time?: number) => (
        time ? (
          <Space>
            <ClockCircleOutlined />
            <Text>{time}ms</Text>
          </Space>
        ) : <Text type="secondary">-</Text>
      )
    },
    {
      title: 'Prompt Length',
      dataIndex: 'promptLength',
      key: 'promptLength',
      render: (length: number) => <Text>{length.toLocaleString()} chars</Text>
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: PromptLog) => (
        <Button
          type="primary"
          size="small"
          icon={<EyeOutlined />}
          onClick={() => viewLogDetails(record)}
        >
          View Details
        </Button>
      )
    }
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Card>
        <div style={{ marginBottom: 16 }}>
          <Title level={3}>
            <FilterOutlined /> Prompt Logs
          </Title>
          <Text type="secondary">
            View detailed logs of AI prompts sent to OpenAI, including the full prompt content, 
            generated SQL, and execution results for debugging and optimization.
          </Text>
        </div>

        <Row gutter={16} style={{ marginBottom: 16 }}>
          <Col span={6}>
            <Select
              placeholder="Filter by type"
              allowClear
              style={{ width: '100%' }}
              value={filters.promptType}
              onChange={(value) => setFilters({ ...filters, promptType: value })}
            >
              <Option value="sql_generation">SQL Generation</Option>
              <Option value="insight_generation">Insight Generation</Option>
              <Option value="visualization">Visualization</Option>
            </Select>
          </Col>
          <Col span={6}>
            <Select
              placeholder="Filter by status"
              allowClear
              style={{ width: '100%' }}
              value={filters.success}
              onChange={(value) => setFilters({ ...filters, success: value })}
            >
              <Option value={true}>Success</Option>
              <Option value={false}>Failed</Option>
            </Select>
          </Col>
          <Col span={8}>
            <RangePicker
              style={{ width: '100%' }}
              onChange={(dates, dateStrings) => 
                setFilters({ ...filters, dateRange: dateStrings as [string, string] })
              }
            />
          </Col>
          <Col span={4}>
            <Space>
              <Button
                type="primary"
                icon={<FilterOutlined />}
                onClick={loadLogs}
              >
                Apply Filters
              </Button>
              <Button
                icon={<ReloadOutlined />}
                onClick={loadLogs}
              >
                Refresh
              </Button>
            </Space>
          </Col>
        </Row>

        <Table
          columns={columns}
          dataSource={logs}
          rowKey="id"
          loading={loading}
          pagination={{
            pageSize: 20,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total) => `Total ${total} logs`
          }}
        />
      </Card>

      <Modal
        title="Prompt Log Details"
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        footer={[
          <Button key="close" onClick={() => setModalVisible(false)}>
            Close
          </Button>
        ]}
        width={1000}
      >
        {selectedLog && (
          <div>
            <Row gutter={16} style={{ marginBottom: 16 }}>
              <Col span={12}>
                <Text strong>Date:</Text> {formatDate(selectedLog.createdDate)}
              </Col>
              <Col span={12}>
                <Text strong>Status:</Text> {getSuccessTag(selectedLog.success)}
              </Col>
            </Row>
            
            <Row gutter={16} style={{ marginBottom: 16 }}>
              <Col span={12}>
                <Text strong>Type:</Text> <Tag color="blue">{selectedLog.promptType}</Tag>
              </Col>
              <Col span={12}>
                <Text strong>Execution Time:</Text> {selectedLog.executionTimeMs || 'N/A'}ms
              </Col>
            </Row>

            <div style={{ marginBottom: 16 }}>
              <Text strong>User Query:</Text>
              <Paragraph copyable style={{ marginTop: 8, padding: 8, backgroundColor: '#f5f5f5' }}>
                {selectedLog.userQuery}
              </Paragraph>
            </div>

            <div style={{ marginBottom: 16 }}>
              <Text strong>Full Prompt Sent to AI:</Text>
              <Paragraph 
                copyable 
                style={{ 
                  marginTop: 8, 
                  padding: 12, 
                  backgroundColor: '#f0f2f5',
                  maxHeight: 300,
                  overflow: 'auto',
                  whiteSpace: 'pre-wrap',
                  fontFamily: 'monospace',
                  fontSize: '12px'
                }}
              >
                {selectedLog.fullPrompt}
              </Paragraph>
            </div>

            {selectedLog.generatedSQL && (
              <div style={{ marginBottom: 16 }}>
                <Text strong>Generated SQL:</Text>
                <Paragraph 
                  copyable 
                  style={{ 
                    marginTop: 8, 
                    padding: 12, 
                    backgroundColor: '#e6f7ff',
                    fontFamily: 'monospace',
                    fontSize: '12px'
                  }}
                >
                  {selectedLog.generatedSQL}
                </Paragraph>
              </div>
            )}

            {selectedLog.errorMessage && (
              <div style={{ marginBottom: 16 }}>
                <Text strong>Error Message:</Text>
                <Paragraph style={{ marginTop: 8, padding: 8, backgroundColor: '#fff2f0', color: '#ff4d4f' }}>
                  {selectedLog.errorMessage}
                </Paragraph>
              </div>
            )}

            {selectedLog.metadata && (
              <div>
                <Text strong>Metadata:</Text>
                <Paragraph 
                  style={{ 
                    marginTop: 8, 
                    padding: 8, 
                    backgroundColor: '#f9f9f9',
                    fontFamily: 'monospace',
                    fontSize: '11px'
                  }}
                >
                  {selectedLog.metadata}
                </Paragraph>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
};
