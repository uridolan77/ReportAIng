import React from 'react';
import { Card, Progress, Typography, Space, Spin, Row, Col, Tag, List, Statistic } from 'antd';
import {
  LoadingOutlined,
  RobotOutlined,
  TableOutlined,
  ColumnHeightOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  DatabaseOutlined
} from '@ant-design/icons';

const { Title, Text } = Typography;

interface AutoGenerationProgressProps {
  progress: number;
  currentTask: string;
  estimatedTimeRemaining?: string;
  recentlyCompleted?: string[];
  currentTable?: string;
  currentStage?: string;
  currentColumn?: string;
  tablesProcessed?: number;
  totalTables?: number;
  columnsProcessed?: number;
  totalColumns?: number;
  glossaryTermsGenerated?: number;
  relationshipsFound?: number;
  processingDetails?: ProcessingDetail[];
  aiPrompts?: AIPromptInfo[];
  onCancel?: () => void;
  mockMode?: boolean;
}

interface ProcessingDetail {
  tableName: string;
  status: 'pending' | 'processing' | 'completed' | 'error';
  stage?: string;
  columnsProcessed?: number;
  totalColumns?: number;
  startTime?: Date;
  endTime?: Date;
  generatedTerms?: number;
}

interface AIPromptInfo {
  id: string;
  timestamp: Date;
  table: string;
  promptType: 'table_context' | 'glossary_terms' | 'relationships';
  prompt: string;
  response?: string;
  status: 'sending' | 'completed' | 'error';
  tokenCount?: number;
}

export const AutoGenerationProgress: React.FC<AutoGenerationProgressProps> = ({
  progress,
  currentTask,
  estimatedTimeRemaining,
  recentlyCompleted = [],
  currentTable,
  currentStage,
  currentColumn,
  tablesProcessed = 0,
  totalTables = 0,
  columnsProcessed = 0,
  totalColumns = 0,
  glossaryTermsGenerated = 0,
  relationshipsFound = 0,
  processingDetails = [],
  aiPrompts = [],
  mockMode = false
}) => {
  const getProgressStatus = () => {
    if (progress < 30) return 'active';
    if (progress < 70) return 'active';
    if (progress < 100) return 'active';
    return 'success';
  };

  const getProgressColor = () => {
    if (progress < 30) return '#1890ff';
    if (progress < 70) return '#52c41a';
    if (progress < 100) return '#722ed1';
    return '#52c41a';
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'completed': return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'processing': return <LoadingOutlined spin style={{ color: '#1890ff' }} />;
      case 'error': return <ClockCircleOutlined style={{ color: '#ff4d4f' }} />;
      default: return <ClockCircleOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed': return 'success';
      case 'processing': return 'processing';
      case 'error': return 'error';
      default: return 'default';
    }
  };

  return (
    <div style={{ marginBottom: '24px' }}>
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <Title level={4}>
          <RobotOutlined style={{ marginRight: '8px', color: mockMode ? '#722ed1' : '#1890ff' }} />
          {mockMode ? 'Mock Generation in Progress 🧪' : 'Auto-Generation in Progress'}
        </Title>
        {mockMode && (
          <div style={{ marginTop: '8px' }}>
            <Tag color="purple" style={{ fontSize: '12px', padding: '4px 12px' }}>
              🧪 Mock Mode - No AI API Calls
            </Tag>
          </div>
        )}
      </div>

      {/* Overall Progress */}
      <div className="overall-progress-section">
        <div className="progress-header">
          <Text strong style={{ fontSize: '16px', color: '#1890ff' }}>
            Overall Progress
          </Text>
          <Text strong style={{ fontSize: '20px', color: '#1890ff' }}>
            {Math.round(progress)}%
          </Text>
        </div>
        <Progress
          percent={Math.round(progress)}
          status={getProgressStatus()}
          strokeColor={getProgressColor()}
          showInfo={false}
          strokeWidth={8}
          style={{ marginTop: '8px' }}
        />
      </div>

      {/* Current Status */}
      <Card className="current-status-card" size="small" style={{ marginBottom: '16px' }}>
        <div className="current-task-display">
          <div className="task-indicator">
            <Spin
              indicator={<LoadingOutlined style={{ fontSize: 18, color: '#1890ff' }} spin />}
            />
          </div>
          <div className="task-content">
            <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginBottom: '4px' }}>
              Currently Processing
            </Text>
            <Text strong style={{ fontSize: '16px', color: '#1890ff' }}>
              {currentTask}
            </Text>
            {currentTable && (
              <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginTop: '4px' }}>
                Table: <Text code>{currentTable}</Text>
                {currentColumn && (
                  <span> | Column: <Text code>{currentColumn}</Text></span>
                )}
              </Text>
            )}
          </div>
        </div>

        {currentTable && (
          <div className="current-status-info">
            <div className="status-info-row">
              <div className="status-info-item">
                <div className="status-info-label">
                  <TableOutlined style={{ color: '#1890ff', fontSize: '16px' }} />
                  <Text strong style={{ fontSize: '14px', marginLeft: '8px' }}>Current Table:</Text>
                </div>
                <Tag
                  color="blue"
                  style={{
                    fontSize: '12px',
                    fontWeight: '500',
                    padding: '4px 12px',
                    borderRadius: '6px',
                    marginLeft: '12px'
                  }}
                >
                  {currentTable}
                </Tag>
              </div>

              {currentStage && (
                <div className="status-info-item">
                  <div className="status-info-label">
                    <DatabaseOutlined style={{ color: '#722ed1', fontSize: '16px' }} />
                    <Text strong style={{ fontSize: '14px', marginLeft: '8px' }}>Stage:</Text>
                  </div>
                  <Tag
                    color="purple"
                    style={{
                      fontSize: '12px',
                      fontWeight: '500',
                      padding: '4px 12px',
                      borderRadius: '6px',
                      marginLeft: '12px'
                    }}
                  >
                    {currentStage}
                  </Tag>
                </div>
              )}

              {currentColumn && (
                <div className="status-info-item">
                  <div className="status-info-label">
                    <ColumnHeightOutlined style={{ color: '#52c41a', fontSize: '16px' }} />
                    <Text strong style={{ fontSize: '14px', marginLeft: '8px' }}>Current Column:</Text>
                  </div>
                  <Tag
                    color="green"
                    style={{
                      fontSize: '12px',
                      fontWeight: '500',
                      padding: '4px 12px',
                      borderRadius: '6px',
                      marginLeft: '12px'
                    }}
                  >
                    {currentColumn}
                  </Tag>
                </div>
              )}
            </div>
          </div>
        )}

        {estimatedTimeRemaining && (
          <div style={{ textAlign: 'center' }}>
            <Text type="secondary">
              Estimated time remaining: {estimatedTimeRemaining}
            </Text>
          </div>
        )}
      </Card>

      {/* Live Statistics */}
      <Row gutter={16} style={{ marginBottom: '16px' }}>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Tables"
              value={tablesProcessed}
              suffix={`/ ${totalTables}`}
              prefix={<TableOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Columns"
              value={columnsProcessed}
              suffix={`/ ${totalColumns}`}
              prefix={<ColumnHeightOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Glossary Terms"
              value={glossaryTermsGenerated}
              prefix={<DatabaseOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Relationships"
              value={relationshipsFound}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#fa8c16' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Detailed Table Processing List */}
      {processingDetails.length > 0 && (
        <Card
          size="small"
          title={
            <Space>
              <TableOutlined />
              <span>Table Processing Details</span>
            </Space>
          }
          style={{ marginBottom: '16px' }}
        >
          <List
            size="small"
            dataSource={processingDetails}
            renderItem={(detail) => (
              <List.Item className="processing-detail-item">
                <div className="processing-detail-content">
                  <div className="processing-detail-header">
                    <div className="processing-detail-icon">
                      {getStatusIcon(detail.status)}
                    </div>
                    <div className="processing-detail-info">
                      <div className="processing-detail-title">
                        <Text strong style={{ fontSize: '14px', flex: 1 }}>{detail.tableName}</Text>
                        <div className="processing-detail-tags">
                          <Tag color={getStatusColor(detail.status)}>
                            {detail.status.toUpperCase()}
                          </Tag>
                          {detail.stage && (
                            <Tag color="purple">
                              {detail.stage}
                            </Tag>
                          )}
                        </div>
                      </div>

                      <div className="processing-detail-progress">
                        {detail.columnsProcessed !== undefined && detail.totalColumns && (
                          <div className="progress-row">
                            <Text type="secondary" style={{ fontSize: '12px', minWidth: '120px' }}>
                              Columns: {detail.columnsProcessed}/{detail.totalColumns}
                            </Text>
                            <Progress
                              percent={Math.round((detail.columnsProcessed / detail.totalColumns) * 100)}
                              size="small"
                              showInfo={false}
                              style={{ flex: 1, maxWidth: '150px', marginLeft: '12px' }}
                              strokeColor="#1890ff"
                            />
                          </div>
                        )}

                        <div className="processing-detail-meta">
                          {detail.generatedTerms !== undefined && detail.generatedTerms > 0 && (
                            <Text type="secondary" style={{ fontSize: '12px' }}>
                              Generated {detail.generatedTerms} glossary terms
                            </Text>
                          )}
                          {detail.startTime && detail.status === 'processing' && (
                            <Text type="secondary" style={{ fontSize: '12px' }}>
                              Started: {detail.startTime.toLocaleTimeString()}
                            </Text>
                          )}
                          {detail.endTime && detail.status === 'completed' && (
                            <Text type="secondary" style={{ fontSize: '12px' }}>
                              Completed: {detail.endTime.toLocaleTimeString()}
                            </Text>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </List.Item>
            )}
          />
        </Card>
      )}

      {/* AI Prompts Section */}
      {aiPrompts.length > 0 && (
        <Card
          size="small"
          title={
            <Space>
              <RobotOutlined />
              <span>AI Communication</span>
              <Tag color="blue">{aiPrompts.length} prompts</Tag>
            </Space>
          }
          style={{ marginBottom: '16px' }}
        >
          <List
            size="small"
            dataSource={aiPrompts.slice(-3)} // Show last 3 prompts
            renderItem={(prompt) => (
              <List.Item>
                <div style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
                    <Space>
                      <Tag color={prompt.promptType === 'table_context' ? 'blue' : prompt.promptType === 'glossary_terms' ? 'green' : 'purple'}>
                        {prompt.promptType.replace('_', ' ').toUpperCase()}
                      </Tag>
                      <Text strong style={{ fontSize: '12px' }}>{prompt.table}</Text>
                      <Tag color={prompt.status === 'completed' ? 'success' : prompt.status === 'sending' ? 'processing' : 'error'}>
                        {prompt.status.toUpperCase()}
                      </Tag>
                    </Space>
                    <Text type="secondary" style={{ fontSize: '11px' }}>
                      {prompt.timestamp.toLocaleTimeString()}
                      {prompt.tokenCount && ` • ${prompt.tokenCount} tokens`}
                    </Text>
                  </div>

                  <details style={{ marginTop: '8px' }}>
                    <summary style={{
                      cursor: 'pointer',
                      color: '#1890ff',
                      fontSize: '12px',
                      padding: '4px 8px',
                      background: '#f0f8ff',
                      border: '1px solid #d6e4ff',
                      borderRadius: '4px'
                    }}>
                      🤖 View AI Prompt
                    </summary>
                    <pre style={{
                      whiteSpace: 'pre-wrap',
                      fontSize: '10px',
                      color: '#595959',
                      maxHeight: '200px',
                      overflow: 'auto',
                      margin: '8px 0 0 0',
                      padding: '8px',
                      background: '#fafafa',
                      border: '1px solid #e8e8e8',
                      borderRadius: '4px',
                      fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace'
                    }}>
                      {prompt.prompt}
                    </pre>
                  </details>

                  {prompt.response && (
                    <details style={{ marginTop: '8px' }}>
                      <summary style={{
                        cursor: 'pointer',
                        color: '#52c41a',
                        fontSize: '12px',
                        padding: '4px 8px',
                        background: '#f6ffed',
                        border: '1px solid #b7eb8f',
                        borderRadius: '4px'
                      }}>
                        ✅ View AI Response
                      </summary>
                      <pre style={{
                        whiteSpace: 'pre-wrap',
                        fontSize: '10px',
                        color: '#595959',
                        maxHeight: '200px',
                        overflow: 'auto',
                        margin: '8px 0 0 0',
                        padding: '8px',
                        background: '#f6ffed',
                        border: '1px solid #b7eb8f',
                        borderRadius: '4px',
                        fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace'
                      }}>
                        {prompt.response}
                      </pre>
                    </details>
                  )}
                </div>
              </List.Item>
            )}
          />

          {aiPrompts.length > 3 && (
            <div style={{ textAlign: 'center', marginTop: '8px' }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Showing latest 3 of {aiPrompts.length} AI communications
              </Text>
            </div>
          )}
        </Card>
      )}

      {/* Recently Completed */}
      {recentlyCompleted.length > 0 && (
        <Card size="small" title="Recently Completed" style={{ marginBottom: '16px' }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            {recentlyCompleted.slice(-5).map((item, index) => (
              <div key={index} style={{ display: 'flex', alignItems: 'center' }}>
                <CheckCircleOutlined style={{ color: '#52c41a', marginRight: '8px' }} />
                <Text style={{ fontSize: '12px' }}>{item}</Text>
              </div>
            ))}
          </Space>
        </Card>
      )}

      <div style={{ marginTop: '24px', padding: '16px', backgroundColor: '#f6f8fa', borderRadius: '6px' }}>
        <Space direction="vertical" size="small" style={{ width: '100%' }}>
          <Text strong style={{ fontSize: '14px' }}>
            What's happening:
          </Text>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 10 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 10 ? undefined : '#bfbfbf' }}>
              Analyzing database schema and extracting metadata
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 30 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 30 ? undefined : '#bfbfbf' }}>
              Generating business-friendly table descriptions using AI
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 60 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 60 ? undefined : '#bfbfbf' }}>
              Creating business glossary terms from column patterns
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 80 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 80 ? undefined : '#bfbfbf' }}>
              Analyzing table relationships and business domains
            </Text>
          </div>

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: progress >= 95 ? '#52c41a' : '#d9d9d9',
              marginRight: '8px'
            }} />
            <Text type="secondary" style={{ fontSize: '12px', color: progress >= 95 ? undefined : '#bfbfbf' }}>
              Finalizing results and calculating confidence scores
            </Text>
          </div>
        </Space>
      </div>

      <div style={{ marginTop: '16px', textAlign: 'center' }}>
        <Text type="secondary" style={{ fontSize: '12px' }}>
          This process typically takes 1-3 minutes depending on your database size.
          <br />
          You can review and edit all generated content before applying it.
        </Text>
      </div>
    </div>
  );
};
