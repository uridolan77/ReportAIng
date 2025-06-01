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
  tablesProcessed?: number;
  totalTables?: number;
  columnsProcessed?: number;
  totalColumns?: number;
  glossaryTermsGenerated?: number;
  relationshipsFound?: number;
  processingDetails?: ProcessingDetail[];
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

export const AutoGenerationProgress: React.FC<AutoGenerationProgressProps> = ({
  progress,
  currentTask,
  estimatedTimeRemaining,
  recentlyCompleted = [],
  currentTable,
  currentStage,
  tablesProcessed = 0,
  totalTables = 0,
  columnsProcessed = 0,
  totalColumns = 0,
  glossaryTermsGenerated = 0,
  relationshipsFound = 0,
  processingDetails = []
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
    <Card style={{ marginBottom: '24px' }}>
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <Title level={4}>
          <RobotOutlined style={{ marginRight: '8px', color: '#1890ff' }} />
          Auto-Generation in Progress
        </Title>
      </div>

      {/* Overall Progress */}
      <div style={{ marginBottom: '24px' }}>
        <Progress
          percent={Math.round(progress)}
          status={getProgressStatus()}
          strokeColor={getProgressColor()}
          showInfo={true}
          format={(percent) => `${percent}%`}
        />
      </div>

      {/* Current Status */}
      <Card size="small" style={{ marginBottom: '16px', backgroundColor: '#f6f8fa' }}>
        <Row gutter={16}>
          <Col span={24}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', marginBottom: '12px' }}>
              <Spin
                indicator={<LoadingOutlined style={{ fontSize: 16, marginRight: '8px' }} spin />}
              />
              <Text strong style={{ fontSize: '16px' }}>
                {currentTask}
              </Text>
            </div>
          </Col>
        </Row>

        {currentTable && (
          <Row gutter={16} style={{ marginBottom: '12px' }}>
            <Col span={12}>
              <Space>
                <TableOutlined style={{ color: '#1890ff' }} />
                <Text strong>Current Table:</Text>
                <Tag color="blue">{currentTable}</Tag>
              </Space>
            </Col>
            <Col span={12}>
              {currentStage && (
                <Space>
                  <DatabaseOutlined style={{ color: '#722ed1' }} />
                  <Text strong>Stage:</Text>
                  <Tag color="purple">{currentStage}</Tag>
                </Space>
              )}
            </Col>
          </Row>
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
              <List.Item>
                <List.Item.Meta
                  avatar={getStatusIcon(detail.status)}
                  title={
                    <Space>
                      <Text strong>{detail.tableName}</Text>
                      <Tag color={getStatusColor(detail.status)}>
                        {detail.status.toUpperCase()}
                      </Tag>
                      {detail.stage && (
                        <Tag color="purple">{detail.stage}</Tag>
                      )}
                    </Space>
                  }
                  description={
                    <Space direction="vertical" size="small" style={{ width: '100%' }}>
                      {detail.columnsProcessed !== undefined && detail.totalColumns && (
                        <div>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            Columns: {detail.columnsProcessed}/{detail.totalColumns}
                          </Text>
                          <Progress
                            percent={Math.round((detail.columnsProcessed / detail.totalColumns) * 100)}
                            size="small"
                            showInfo={false}
                            style={{ marginLeft: '8px', width: '100px' }}
                          />
                        </div>
                      )}
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
                    </Space>
                  }
                />
              </List.Item>
            )}
          />
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
    </Card>
  );
};
