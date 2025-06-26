import React, { useState } from 'react';
import {
  Card,
  Collapse,
  Descriptions,
  Tag,
  Space,
  Typography,
  Button,
  Statistic,
  Row,
  Col,
  Timeline,
  Badge,
  Tooltip,
  Progress,
  Divider,
  Alert
} from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  DownloadOutlined,
  BulbOutlined,
  DatabaseOutlined,
  ApiOutlined,
  RocketOutlined
} from '@ant-design/icons';

const { Text, Title } = Typography;

interface TestRunDetailsViewerProps {
  testResult: any;
  className?: string;
}

const TestRunDetailsViewer: React.FC<TestRunDetailsViewerProps> = ({
  testResult,
  className
}) => {
  const [expandedPanels, setExpandedPanels] = useState<string[]>(['summary']);

  if (!testResult) {
    return (
      <Card>
        <Alert
          message="No Test Results"
          description="Run a pipeline test to see detailed results here"
          type="info"
          showIcon
        />
      </Card>
    );
  }

  const getStatusColor = (success: boolean) => success ? 'success' : 'error';
  const getStatusIcon = (success: boolean) => success ? <CheckCircleOutlined /> : <CloseCircleOutlined />;

  const formatDuration = (ms: number) => {
    if (ms < 1000) return `${ms}ms`;
    if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
    return `${(ms / 60000).toFixed(1)}m`;
  };

  const renderBusinessProfile = () => {
    if (!testResult.businessProfile) return null;

    const { businessProfile } = testResult;
    
    return (
      <Card size="small" title={
        <Space>
          <BulbOutlined style={{ color: '#1890ff' }} />
          <span>Business Intelligence Analysis</span>
        </Space>
      }>
        <Row gutter={[16, 8]}>
          <Col span={6}>
            <Statistic
              title="Intent Type"
              value={businessProfile.intent?.type || 'Unknown'}
              prefix={<RocketOutlined />}
              valueStyle={{ fontSize: '14px' }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Domain"
              value={businessProfile.domain?.name || 'Unknown'}
              prefix={<DatabaseOutlined />}
              valueStyle={{ fontSize: '14px' }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Confidence"
              value={`${Math.round((businessProfile.confidenceScore || 0) * 100)}%`}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ fontSize: '14px', color: businessProfile.confidenceScore > 0.8 ? '#52c41a' : '#faad14' }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Entities Found"
              value={businessProfile.entities?.length || 0}
              prefix={<ApiOutlined />}
              valueStyle={{ fontSize: '14px' }}
            />
          </Col>
        </Row>

        {businessProfile.entities && businessProfile.entities.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong>Detected Entities:</Text>
            <div style={{ marginTop: 8 }}>
              {businessProfile.entities.map((entity: any, idx: number) => (
                <Tooltip key={idx} title={`Type: ${entity.type}, Confidence: ${entity.confidenceScore}`}>
                  <Tag 
                    color={entity.type === 'Table' ? 'blue' : entity.type === 'Metric' ? 'orange' : entity.type === 'Dimension' ? 'green' : 'purple'}
                    style={{ margin: '2px', cursor: 'help' }}
                  >
                    {entity.name} ({entity.type})
                  </Tag>
                </Tooltip>
              ))}
            </div>
          </div>
        )}

        {businessProfile.identifiedMetrics && businessProfile.identifiedMetrics.length > 0 && (
          <div style={{ marginTop: 12 }}>
            <Text strong>Identified Metrics:</Text>
            <div style={{ marginTop: 4 }}>
              {businessProfile.identifiedMetrics.map((metric: string, idx: number) => (
                <Tag key={idx} color="orange">{metric}</Tag>
              ))}
            </div>
          </div>
        )}

        {businessProfile.timeContext?.relativeExpression && (
          <div style={{ marginTop: 12 }}>
            <Text strong>Time Context:</Text>
            <Tag color="purple" style={{ marginLeft: 8 }}>
              {businessProfile.timeContext.relativeExpression}
            </Tag>
          </div>
        )}
      </Card>
    );
  };

  const renderStepDetails = (stepName: string, stepData: any) => {
    const success = stepData?.success;
    const duration = stepData?.durationMs;
    
    return (
      <div>
        <Row gutter={[16, 8]} style={{ marginBottom: 12 }}>
          <Col span={6}>
            <Badge 
              status={success ? 'success' : 'error'} 
              text={success ? 'Completed' : 'Failed'} 
            />
          </Col>
          <Col span={6}>
            <Text type="secondary">
              Duration: {duration ? formatDuration(duration) : 'N/A'}
            </Text>
          </Col>
          <Col span={6}>
            {stepData?.confidenceScore && (
              <Text type="secondary">
                Confidence: {Math.round(stepData.confidenceScore * 100)}%
              </Text>
            )}
          </Col>
          <Col span={6}>
            <Button 
              size="small" 
              icon={<EyeOutlined />}
              onClick={() => {
                console.log(`${stepName} Details:`, stepData);
              }}
            >
              View Raw Data
            </Button>
          </Col>
        </Row>

        {stepData?.error && (
          <Alert
            message="Step Error"
            description={stepData.error}
            type="error"
            size="small"
            style={{ marginBottom: 12 }}
          />
        )}

        {/* Special rendering for TokenBudgetManagement */}
        {stepName === 'TokenBudgetManagement' && stepData && (
          <div style={{ marginTop: 12 }}>
            <Text strong>Token Budget Details:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="Intent Type">
                <Tag color="blue">{stepData.tokenBudget?.intentType || stepData.intentType || 'Unknown'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Max Total Tokens">
                <Tag color="purple">{stepData.tokenBudget?.maxTotalTokens || stepData.maxTokens || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Available Context">
                <Tag color="green">{stepData.tokenBudget?.availableContextTokens || stepData.availableContextTokens || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Reserved Response">
                <Tag color="orange">{stepData.tokenBudget?.reservedResponseTokens || stepData.reservedTokens || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Base Prompt">
                <Tag color="cyan">{stepData.tokenBudget?.basePromptTokens || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Schema Budget">
                <Tag color="geekblue">{stepData.tokenBudget?.schemaContextBudget || 'N/A'}</Tag>
              </Descriptions.Item>
            </Descriptions>
          </div>
        )}

        {/* Special rendering for SchemaRetrieval */}
        {stepName === 'SchemaRetrieval' && stepData && (
          <div style={{ marginTop: 12 }}>
            <Text strong>Schema Retrieval Details:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="Tables Retrieved">
                <Tag color="blue">{stepData.tablesRetrieved || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Relevance Score">
                <Progress
                  percent={Math.round((stepData.relevanceScore || 0) * 100)}
                  size="small"
                  status={stepData.relevanceScore > 0.7 ? 'success' : 'normal'}
                />
              </Descriptions.Item>
              <Descriptions.Item label="Schema Metadata" span={2}>
                {stepData.schemaMetadata ? 'Available' : 'Not Available'}
              </Descriptions.Item>
            </Descriptions>
            {stepData.tableNames && stepData.tableNames.length > 0 && (
              <div style={{ marginTop: 8 }}>
                <Text strong>Retrieved Tables:</Text>
                <div style={{ marginTop: 4 }}>
                  {stepData.tableNames.map((tableName: string) => (
                    <Tag key={tableName} color="cyan" style={{ marginBottom: 4 }}>
                      {tableName}
                    </Tag>
                  ))}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Special rendering for PromptBuilding */}
        {stepName === 'PromptBuilding' && stepData && (
          <div style={{ marginTop: 12 }}>
            <Text strong>Prompt Building Details:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="Prompt Length">
                <Tag color="purple">{stepData.promptLength || 'N/A'} characters</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Estimated Tokens">
                <Tag color="orange">{stepData.estimatedTokens || 'N/A'}</Tag>
              </Descriptions.Item>
            </Descriptions>
            {stepData.prompt && (
              <div style={{ marginTop: 8 }}>
                <Text strong>Generated Prompt Preview:</Text>
                <div style={{
                  marginTop: 4,
                  padding: 8,
                  backgroundColor: '#f5f5f5',
                  borderRadius: 4,
                  maxHeight: 200,
                  overflow: 'auto',
                  fontSize: '12px',
                  fontFamily: 'monospace',
                  whiteSpace: 'pre-wrap'
                }}>
                  {stepData.prompt}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Special rendering for AIGeneration */}
        {stepName === 'AIGeneration' && stepData && (
          <div style={{ marginTop: 12 }}>
            <Text strong>AI Generation Details:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="SQL Length">
                <Tag color="blue">{stepData.sqlLength || 'N/A'} characters</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Estimated Cost">
                <Tag color="green">${stepData.estimatedCost || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Duration">
                <Tag color="orange">{stepData.durationMs || 'N/A'}ms</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag color={stepData.success ? 'success' : 'error'}>
                  {stepData.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>
            {stepData.generatedSQL && (
              <div style={{ marginTop: 8 }}>
                <Text strong>Generated SQL Preview:</Text>
                <div style={{
                  marginTop: 4,
                  padding: 8,
                  backgroundColor: '#f5f5f5',
                  borderRadius: 4,
                  maxHeight: 120,
                  overflow: 'auto',
                  fontSize: '12px',
                  fontFamily: 'monospace'
                }}>
                  {stepData.generatedSQL.replace(/```SQL\n?/gi, '').replace(/```\n?$/gi, '')}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Special rendering for SQLValidation */}
        {stepName === 'SQLValidation' && stepData && (
          <div style={{ marginTop: 12 }}>
            <Text strong>SQL Validation Details:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="Valid">
                <Tag color={stepData.isValid ? 'success' : 'error'}>
                  {stepData.isValid ? 'Valid' : 'Invalid'}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Security Score">
                <Tag color="blue">{stepData.securityScore || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Syntax Score">
                <Tag color="green">{stepData.syntaxScore || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Semantic Score">
                <Tag color="orange">{stepData.semanticScore || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Duration">
                <Tag color="purple">{stepData.durationMs || 'N/A'}ms</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag color={stepData.success ? 'success' : 'error'}>
                  {stepData.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>
            {stepData.validationErrors && stepData.validationErrors.length > 0 && (
              <div style={{ marginTop: 8 }}>
                <Text strong>Validation Errors:</Text>
                <ul style={{ marginTop: 4, marginLeft: 16 }}>
                  {stepData.validationErrors.map((error: string, index: number) => (
                    <li key={index} style={{ color: '#ff4d4f', fontSize: '12px' }}>{error}</li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}

        {/* Special rendering for SQLExecution */}
        {stepName === 'SQLExecution' && stepData && (
          <div style={{ marginTop: 12 }}>
            <Text strong>SQL Execution Details:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="Executed">
                <Tag color={stepData.executedSuccessfully ? 'success' : 'error'}>
                  {stepData.executedSuccessfully ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Rows Returned">
                <Tag color="blue">{stepData.rowsReturned || 0}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Execution Time">
                <Tag color="green">{stepData.executionTimeMs || 0}ms</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Mode">
                <Tag color={stepData.isSimulated ? 'orange' : 'blue'}>
                  {stepData.isSimulated ? 'Simulated' : 'Actual'}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Duration">
                <Tag color="purple">{stepData.durationMs || 'N/A'}ms</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag color={stepData.success ? 'success' : 'error'}>
                  {stepData.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>
            {stepData.resultPreview && (
              <div style={{ marginTop: 8 }}>
                <Text strong>Result Preview:</Text>
                <div style={{
                  marginTop: 4,
                  padding: 8,
                  backgroundColor: '#f5f5f5',
                  borderRadius: 4,
                  fontSize: '12px',
                  fontFamily: 'monospace'
                }}>
                  {stepData.resultPreview}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Special rendering for ResultsProcessing */}
        {stepName === 'ResultsProcessing' && stepData && (
          <div style={{ marginTop: 12 }}>
            <Text strong>Results Processing Details:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="Total Steps">
                <Tag color="blue">{stepData.totalSteps || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Successful Steps">
                <Tag color="green">{stepData.successfulSteps || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Success Rate">
                <Tag color="orange">{stepData.successRate ? `${(stepData.successRate * 100).toFixed(1)}%` : 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Export Format">
                <Tag color="purple">{stepData.exportFormat || 'N/A'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Duration">
                <Tag color="cyan">{stepData.durationMs || 'N/A'}ms</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Status">
                <Tag color={stepData.success ? 'success' : 'error'}>
                  {stepData.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>
            {stepData.formattedResults && (
              <div style={{ marginTop: 8 }}>
                <Text strong>Formatted Results:</Text>
                <div style={{
                  marginTop: 4,
                  padding: 8,
                  backgroundColor: '#f5f5f5',
                  borderRadius: 4,
                  fontSize: '12px',
                  fontFamily: 'monospace'
                }}>
                  {stepData.formattedResults}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Special rendering for BusinessContextAnalysis */}
        {stepName === 'BusinessContextAnalysis' && stepData?.businessProfile && (
          <div style={{ marginTop: 12 }}>
            <Text strong>Analysis Results:</Text>
            <Descriptions column={2} size="small" style={{ marginTop: 8 }}>
              <Descriptions.Item label="Intent">
                <Tag color="blue">{stepData.businessProfile.intent?.type}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Domain">
                <Tag color="green">{stepData.businessProfile.domain?.name}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Entities">
                {stepData.businessProfile.entities?.length || 0} detected
              </Descriptions.Item>
              <Descriptions.Item label="Confidence">
                <Progress
                  percent={Math.round((stepData.businessProfile.confidenceScore || 0) * 100)}
                  size="small"
                  status={stepData.businessProfile.confidenceScore > 0.8 ? 'success' : 'normal'}
                />
              </Descriptions.Item>
            </Descriptions>
          </div>
        )}
      </div>
    );
  };

  // Build collapse items array
  const collapseItems = [
    // Test Summary Panel
    {
      key: 'summary',
      label: (
        <Space>
          {getStatusIcon(testResult.success)}
          <Text strong>Test Summary</Text>
          <Tag color={getStatusColor(testResult.success)}>
            {testResult.success ? 'Success' : 'Failed'}
          </Tag>
          <Text type="secondary">
            {testResult.totalDurationMs ? formatDuration(testResult.totalDurationMs) : 'N/A'}
          </Text>
        </Space>
      ),
      children: (
        <Row gutter={[16, 8]}>
          <Col span={12}>
            <Descriptions column={1} size="small">
              <Descriptions.Item label="Test ID">
                <Text code>{testResult.testId}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Query">
                <Text strong>{testResult.query}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Steps Requested">
                {testResult.requestedSteps?.join(', ') || 'N/A'}
              </Descriptions.Item>
            </Descriptions>
          </Col>
          <Col span={12}>
            <Descriptions column={1} size="small">
              <Descriptions.Item label="Start Time">
                {testResult.startTime ? new Date(testResult.startTime).toLocaleString() : 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="End Time">
                {testResult.endTime ? new Date(testResult.endTime).toLocaleString() : 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Results Available">
                {testResult.results ? Object.keys(testResult.results).length : 0} steps
              </Descriptions.Item>
            </Descriptions>
          </Col>
        </Row>
      )
    },
  ];

  // Add Business Profile Panel if available
  if (testResult.businessProfile) {
    collapseItems.push({
      key: 'business-profile',
      label: (
        <Space>
          <BulbOutlined style={{ color: '#1890ff' }} />
          <Text strong>Business Analysis</Text>
          <Tag color="blue">
            {testResult.businessProfile.intent?.type || 'Unknown'}
          </Tag>
          <Tag color="green">
            {testResult.businessProfile.domain?.name || 'Unknown'}
          </Tag>
        </Space>
      ),
      children: renderBusinessProfile()
    });
  }

  // Add Step Results Panels
  if (testResult.results) {
    Object.entries(testResult.results).forEach(([stepName, stepData]: [string, any]) => {
      collapseItems.push({
        key: `step-${stepName}`,
        label: (
          <Space>
            <ThunderboltOutlined style={{ color: stepData?.success ? '#52c41a' : '#ff4d4f' }} />
            <Text strong>{stepName}</Text>
            <Badge
              status={stepData?.success ? 'success' : 'error'}
              text={stepData?.success ? 'Success' : 'Failed'}
            />
            {stepData?.durationMs && (
              <Text type="secondary">{formatDuration(stepData.durationMs)}</Text>
            )}
          </Space>
        ),
        children: renderStepDetails(stepName, stepData)
      });
    });
  }

  return (
    <div className={className}>
      <Collapse
        activeKey={expandedPanels}
        onChange={setExpandedPanels}
        size="small"
        items={collapseItems}
      />

      <div style={{ marginTop: 16, textAlign: 'right' }}>
        <Space>
          <Button 
            icon={<DownloadOutlined />}
            onClick={() => {
              const dataStr = JSON.stringify(testResult, null, 2);
              const dataBlob = new Blob([dataStr], { type: 'application/json' });
              const url = URL.createObjectURL(dataBlob);
              const link = document.createElement('a');
              link.href = url;
              link.download = `test-result-${testResult.testId}.json`;
              link.click();
              URL.revokeObjectURL(url);
            }}
          >
            Export Results
          </Button>
          <Button 
            type="primary"
            icon={<EyeOutlined />}
            onClick={() => {
              console.log('Full Test Result:', testResult);
            }}
          >
            View Full Data
          </Button>
        </Space>
      </div>
    </div>
  );
};

export default TestRunDetailsViewer;
