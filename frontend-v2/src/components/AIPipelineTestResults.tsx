import React from 'react';
import {
  Card,
  Row,
  Col,
  Typography,
  Tag,
  Collapse,
  Descriptions,
  Alert,
  Progress,
  Statistic,
  Space,
  Button,
  Tooltip,
  Table,
  Divider
} from 'antd';
import {
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  InfoCircleOutlined,
  CopyOutlined,
  DownloadOutlined,
  EyeOutlined
} from '@ant-design/icons';
import { PipelineTestResult, PipelineStep } from '../types/aiPipelineTest';

const { Title, Text, Paragraph } = Typography;
const { Panel } = Collapse;

interface AIPipelineTestResultsProps {
  result: PipelineTestResult;
  onExport?: () => void;
  onViewDetails?: (step: PipelineStep) => void;
}

const AIPipelineTestResults: React.FC<AIPipelineTestResultsProps> = ({
  result,
  onExport,
  onViewDetails
}) => {
  const getStepIcon = (step: PipelineStep) => {
    const icons = {
      [PipelineStep.BusinessContextAnalysis]: '🧠',
      [PipelineStep.TokenBudgetManagement]: '💰',
      [PipelineStep.SchemaRetrieval]: '📊',
      [PipelineStep.PromptBuilding]: '🔧',
      [PipelineStep.AIGeneration]: '🤖'
    };
    return icons[step] || '⚙️';
  };

  const getStepStatus = (step: PipelineStep) => {
    const stepResult = result.results[step];
    if (!stepResult) return { status: 'default', text: 'Not executed' };
    
    return stepResult.success 
      ? { status: 'success', text: 'Success' }
      : { status: 'error', text: 'Error' };
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  const renderOverviewStats = () => (
    <Row gutter={[16, 16]} className="mb-6">
      <Col span={6}>
        <Statistic
          title="Total Duration"
          value={result.totalDurationMs}
          suffix="ms"
          prefix={<ClockCircleOutlined />}
        />
      </Col>
      <Col span={6}>
        <Statistic
          title="Steps Executed"
          value={result.requestedSteps.length}
          prefix={<InfoCircleOutlined />}
        />
      </Col>
      <Col span={6}>
        <Statistic
          title="Success Rate"
          value={result.success ? 100 : 0}
          suffix="%"
          prefix={result.success ? <CheckCircleOutlined /> : <ExclamationCircleOutlined />}
          valueStyle={{ color: result.success ? '#3f8600' : '#cf1322' }}
        />
      </Col>
      <Col span={6}>
        <div className="text-center">
          <Text type="secondary">Test ID</Text>
          <div className="flex items-center justify-center space-x-2">
            <Text code>{result.testId}</Text>
            <Button
              type="text"
              size="small"
              icon={<CopyOutlined />}
              onClick={() => copyToClipboard(result.testId)}
            />
          </div>
        </div>
      </Col>
    </Row>
  );

  const renderStepResults = () => (
    <Collapse className="mb-6">
      {result.requestedSteps.map(step => {
        const stepResult = result.results[step];
        const status = getStepStatus(step);
        
        return (
          <Panel
            key={step}
            header={
              <div className="flex items-center justify-between w-full">
                <div className="flex items-center space-x-3">
                  <span className="text-lg">{getStepIcon(step)}</span>
                  <Text strong>{step.replace(/([A-Z])/g, ' $1').trim()}</Text>
                  <Tag color={status.status}>{status.text}</Tag>
                </div>
                <div className="flex items-center space-x-2">
                  {stepResult && (
                    <Text type="secondary">{stepResult.durationMs}ms</Text>
                  )}
                  <Button
                    type="text"
                    size="small"
                    icon={<EyeOutlined />}
                    onClick={(e) => {
                      e.stopPropagation();
                      onViewDetails?.(step);
                    }}
                  />
                </div>
              </div>
            }
          >
            {stepResult ? (
              <div className="space-y-4">
                {stepResult.error && (
                  <Alert
                    message="Step Error"
                    description={stepResult.error}
                    type="error"
                    showIcon
                  />
                )}
                
                {renderStepSpecificResults(step, stepResult)}
              </div>
            ) : (
              <Text type="secondary">Step was not executed</Text>
            )}
          </Panel>
        );
      })}
    </Collapse>
  );

  const renderStepSpecificResults = (step: PipelineStep, stepResult: any) => {
    switch (step) {
      case PipelineStep.BusinessContextAnalysis:
        return (
          <Descriptions bordered size="small">
            <Descriptions.Item label="Intent" span={2}>
              {stepResult.intent}
            </Descriptions.Item>
            <Descriptions.Item label="Domain">
              {stepResult.domain}
            </Descriptions.Item>
            <Descriptions.Item label="Confidence Score" span={2}>
              <Progress
                percent={Math.round(stepResult.confidenceScore * 100)}
                size="small"
                status={stepResult.confidenceScore > 0.7 ? 'success' : 'normal'}
              />
            </Descriptions.Item>
            <Descriptions.Item label="Entities Extracted">
              {stepResult.extractedEntities}
            </Descriptions.Item>
          </Descriptions>
        );

      case PipelineStep.TokenBudgetManagement:
        return (
          <Descriptions bordered size="small">
            <Descriptions.Item label="Max Tokens">
              {stepResult.maxTokens}
            </Descriptions.Item>
            <Descriptions.Item label="Available Context">
              {stepResult.availableContextTokens}
            </Descriptions.Item>
            <Descriptions.Item label="Reserved Tokens">
              {stepResult.reservedTokens}
            </Descriptions.Item>
          </Descriptions>
        );

      case PipelineStep.SchemaRetrieval:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="Tables Retrieved">
                {stepResult.tablesRetrieved}
              </Descriptions.Item>
              <Descriptions.Item label="Relevance Score">
                <Progress
                  percent={Math.round(stepResult.relevanceScore * 100)}
                  size="small"
                />
              </Descriptions.Item>
            </Descriptions>
            {stepResult.tableNames && stepResult.tableNames.length > 0 && (
              <div>
                <Text strong>Retrieved Tables:</Text>
                <div className="mt-2">
                  {stepResult.tableNames.map((tableName: string) => (
                    <Tag key={tableName} className="mb-1">{tableName}</Tag>
                  ))}
                </div>
              </div>
            )}
          </div>
        );

      case PipelineStep.PromptBuilding:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="Prompt Length">
                {stepResult.promptLength} characters
              </Descriptions.Item>
              <Descriptions.Item label="Estimated Tokens">
                {stepResult.estimatedTokens}
              </Descriptions.Item>
            </Descriptions>
            {stepResult.prompt && (
              <div>
                <div className="flex items-center justify-between mb-2">
                  <Text strong>Generated Prompt:</Text>
                  <Button
                    type="text"
                    size="small"
                    icon={<CopyOutlined />}
                    onClick={() => copyToClipboard(stepResult.prompt)}
                  >
                    Copy
                  </Button>
                </div>
                <div className="bg-gray-50 p-3 rounded max-h-60 overflow-y-auto">
                  <Text code className="whitespace-pre-wrap">
                    {stepResult.prompt}
                  </Text>
                </div>
              </div>
            )}
          </div>
        );

      case PipelineStep.AIGeneration:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="SQL Length">
                {stepResult.sqlLength} characters
              </Descriptions.Item>
              <Descriptions.Item label="Estimated Cost">
                ${stepResult.estimatedCost?.toFixed(4) || '0.0000'}
              </Descriptions.Item>
            </Descriptions>
            {stepResult.generatedSQL && (
              <div>
                <div className="flex items-center justify-between mb-2">
                  <Text strong>Generated SQL:</Text>
                  <Button
                    type="text"
                    size="small"
                    icon={<CopyOutlined />}
                    onClick={() => copyToClipboard(stepResult.generatedSQL)}
                  >
                    Copy
                  </Button>
                </div>
                <div className="bg-gray-50 p-3 rounded max-h-60 overflow-y-auto">
                  <Text code className="whitespace-pre-wrap">
                    {stepResult.generatedSQL}
                  </Text>
                </div>
              </div>
            )}
          </div>
        );

      default:
        return (
          <div className="bg-gray-50 p-3 rounded">
            <Text code>{JSON.stringify(stepResult, null, 2)}</Text>
          </div>
        );
    }
  };

  const renderTestMetadata = () => (
    <Card title="Test Metadata" size="small">
      <Descriptions bordered size="small">
        <Descriptions.Item label="Query" span={3}>
          <Text code>{result.query}</Text>
        </Descriptions.Item>
        <Descriptions.Item label="Start Time">
          {new Date(result.startTime).toLocaleString()}
        </Descriptions.Item>
        <Descriptions.Item label="End Time">
          {new Date(result.endTime).toLocaleString()}
        </Descriptions.Item>
        <Descriptions.Item label="Duration">
          {result.totalDurationMs}ms
        </Descriptions.Item>
      </Descriptions>
    </Card>
  );

  return (
    <div className="space-y-6">
      {/* Header with actions */}
      <div className="flex items-center justify-between">
        <Title level={4}>Test Results</Title>
        <Space>
          <Button
            icon={<DownloadOutlined />}
            onClick={onExport}
          >
            Export Results
          </Button>
        </Space>
      </div>

      {/* Global error */}
      {result.error && (
        <Alert
          message="Test Failed"
          description={result.error}
          type="error"
          showIcon
          className="mb-4"
        />
      )}

      {/* Overview statistics */}
      {renderOverviewStats()}

      {/* Step-by-step results */}
      {renderStepResults()}

      {/* Test metadata */}
      {renderTestMetadata()}
    </div>
  );
};

export default AIPipelineTestResults;
