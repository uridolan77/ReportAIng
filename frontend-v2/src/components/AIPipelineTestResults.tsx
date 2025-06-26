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
import { PipelineTestResult, PipelineStep, formatPipelineStepName } from '../types/aiPipelineTest';

const { Title, Text, Paragraph } = Typography;


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
  console.log('🔍 AIPipelineTestResults rendering with result:', result);
  console.log('🔍 Available step results:', result?.results ? Object.keys(result.results) : 'No results');
  console.log('🔍 AIGeneration step data:', result?.results?.['AIGeneration']);

  // Define helper functions first to ensure they're available
  const getStepIcon = (step: PipelineStep) => {
    const icons = {
      [PipelineStep.BusinessContextAnalysis]: '🧠',
      [PipelineStep.TokenBudgetManagement]: '💰',
      [PipelineStep.SchemaRetrieval]: '📊',
      [PipelineStep.PromptBuilding]: '🔧',
      [PipelineStep.AIGeneration]: '🤖',
      [PipelineStep.SQLValidation]: '✅',
      [PipelineStep.SQLExecution]: '▶️',
      [PipelineStep.ResultsProcessing]: '📋'
    };
    return icons[step] || '⚙️';
  };

  const getStepDisplayName = (step: PipelineStep) => {
    const names = {
      [PipelineStep.BusinessContextAnalysis]: 'Business Context Analysis',
      [PipelineStep.TokenBudgetManagement]: 'Token Budget Management',
      [PipelineStep.SchemaRetrieval]: 'Schema Retrieval',
      [PipelineStep.PromptBuilding]: 'Prompt Building',
      [PipelineStep.AIGeneration]: 'AI Generation',
      [PipelineStep.SQLValidation]: 'SQL Validation',
      [PipelineStep.SQLExecution]: 'SQL Execution',
      [PipelineStep.ResultsProcessing]: 'Results Processing'
    };
    return names[step] || step;
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  const getStepStatus = (step: PipelineStep) => {
    const stepData = result.results[step];
    if (!stepData) return { status: 'default', text: 'Not executed' };

    // Handle both direct stepResult and wrapped stepResult structure
    const stepResult = stepData.stepResult || stepData;

    return stepResult.success
      ? { status: 'success', text: 'Success' }
      : { status: 'error', text: 'Error' };
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

  const renderStepResults = () => {
    console.log('🔥🔥🔥 renderStepResults CALLED! 🔥🔥🔥');
    console.log('🔍 Rendering step results. requestedSteps:', result.requestedSteps);
    console.log('🔍 Available results:', result.results);
    console.log('🔍 result.results type:', typeof result.results);
    console.log('🔍 result.results keys:', result.results ? Object.keys(result.results) : 'NO KEYS');

    // Debug each step's data
    result.requestedSteps.forEach(step => {
      console.log(`🔍 Step ${step}:`, result.results?.[step]);
      console.log(`🔍 Step ${step} has data:`, !!result.results?.[step]);
    });

    const collapseItems = result.requestedSteps.map(step => {
      const stepResult = result.results?.[step];
      // Special handling for steps that might have data but appear empty
      let hasData = !!stepResult;
      if (step === 'AIGeneration') {
        hasData = !!(stepResult.generatedSQL || stepResult.sqlLength || stepResult.success !== undefined);
      }

      console.log(`🔍 Processing step ${step}, hasData: ${hasData}`);



      const collapseItem = {
        key: step,
        label: (
          <div className="flex items-center justify-between">
            <span className="flex items-center gap-2">
              {getStepIcon(step)}
              <span className="font-medium">{getStepDisplayName(step)}</span>
              {hasData ? (
                <Tag color="green">Has Data</Tag>
              ) : (
                <Tag color="red">No Data</Tag>
              )}
            </span>
          </div>
        ),
        children: hasData ? renderStepSpecificResults(step, stepResult) : (
          <div style={{padding: '20px', backgroundColor: '#f5f5f5', textAlign: 'center'}}>
            <div>
              <Text type="secondary">No data available for this step</Text>
              <br />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                The step may have been skipped or failed to execute.
              </Text>
            </div>
          </div>
        )
      };



      return collapseItem;
    });

    // Always expand all steps that have data
    const stepsWithData = result.requestedSteps.filter(step => !!result.results?.[step]);
    const defaultActiveKeys = [...new Set(stepsWithData)]; // Remove duplicates

    console.log('🔍 Steps with data:', stepsWithData);
    console.log('🔍 Setting default active keys (no duplicates):', defaultActiveKeys);

    return (
      <Collapse
        className="mb-6"
        items={collapseItems}
        defaultActiveKey={defaultActiveKeys}
      />
    );
  };

  const renderStepSpecificResults = (step: string, stepResult: any) => {
    switch (step) {
      case PipelineStep.BusinessContextAnalysis:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="Intent" span={2}>
                {stepResult.intent || 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Domain">
                {stepResult.domain || 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Confidence Score" span={2}>
                <Progress
                  percent={Math.round((stepResult.confidenceScore || 0) * 100)}
                  size="small"
                  status={(stepResult.confidenceScore || 0) > 0.7 ? 'success' : 'normal'}
                />
              </Descriptions.Item>
              <Descriptions.Item label="Entities Extracted">
                {stepResult.extractedEntities || 0}
              </Descriptions.Item>
              <Descriptions.Item label="Success">
                <Tag color={stepResult.success ? 'green' : 'red'}>
                  {stepResult.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>



            {stepResult.businessProfile && (
              <div>
                <Text strong>Business Profile:</Text>
                <div className="bg-gray-50 p-3 rounded border mt-2" style={{ maxHeight: '300px', overflowY: 'auto' }}>
                  <pre style={{ fontSize: '12px', margin: 0 }}>
                    {JSON.stringify(stepResult.businessProfile, null, 2)}
                  </pre>
                </div>
              </div>
            )}

            {stepResult.error && (
              <div className="mt-4">
                <Text strong type="danger">Error:</Text>
                <div className="bg-red-50 p-3 rounded border border-red-200 mt-2">
                  <Text type="danger">{stepResult.error}</Text>
                </div>
              </div>
            )}
          </div>
        );

      case "TokenBudgetManagement":
      case PipelineStep.TokenBudgetManagement:

        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="Max Tokens">
                {stepResult.maxTokens || 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Available Context Tokens">
                {stepResult.availableContextTokens || 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Reserved Tokens">
                {stepResult.reservedTokens || 'N/A'}
              </Descriptions.Item>
              <Descriptions.Item label="Success">
                <Tag color={stepResult.success ? 'green' : 'red'}>
                  {stepResult.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>



            {stepResult.tokenBudget && (
              <div>
                <Text strong>Token Budget Details:</Text>
                <div className="bg-gray-50 p-3 rounded border mt-2">
                  <pre style={{ fontSize: '12px', margin: 0 }}>
                    {JSON.stringify(stepResult.tokenBudget, null, 2)}
                  </pre>
                </div>
              </div>
            )}

            {stepResult.error && (
              <div className="mt-4">
                <Text strong type="danger">Error:</Text>
                <div className="bg-red-50 p-3 rounded border border-red-200 mt-2">
                  <Text type="danger">{stepResult.error}</Text>
                </div>
              </div>
            )}
          </div>
        );

      case PipelineStep.SchemaRetrieval:
        return (
          <div>
            <Descriptions bordered size="small" column={2} className="mb-4">
              <Descriptions.Item label="Tables Retrieved" span={1}>
                <strong>{stepResult.tablesRetrieved || 0}</strong>
              </Descriptions.Item>
              <Descriptions.Item label="Relevance Score" span={1}>
                <Progress
                  percent={Math.round((stepResult.relevanceScore || 0) * 100)}
                  size="small"
                  status={(stepResult.relevanceScore || 0) > 0.7 ? 'success' : 'normal'}
                />
              </Descriptions.Item>
              <Descriptions.Item label="Success">
                <Tag color={stepResult.success ? 'green' : 'red'}>
                  {stepResult.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>



            {(() => {
              // Handle tableNames that might be a JSON string or array
              let tableNames: string[] = [];
              if (stepResult.tableNames) {
                if (Array.isArray(stepResult.tableNames)) {
                  tableNames = stepResult.tableNames;
                } else if (typeof stepResult.tableNames === 'string') {
                  try {
                    tableNames = JSON.parse(stepResult.tableNames);
                  } catch {
                    // If parsing fails, treat as single table name
                    tableNames = [stepResult.tableNames];
                  }
                }
              }

              return tableNames.length > 0 && (
                <div className="mb-4">
                  <Text strong>Retrieved Tables:</Text>
                  <div className="mt-2">
                    {tableNames.map((tableName: string) => (
                      <Tag key={tableName} color="blue" className="mb-1">{tableName}</Tag>
                    ))}
                  </div>
                </div>
              );
            })()}

            {stepResult.schemaMetadata && (
              <div>
                <Text strong>Schema Metadata:</Text>
                <div className="bg-gray-50 p-3 rounded border mt-2" style={{ maxHeight: '300px', overflowY: 'auto' }}>
                  <pre style={{ fontSize: '12px', margin: 0 }}>
                    {JSON.stringify(stepResult.schemaMetadata, null, 2)}
                  </pre>
                </div>
              </div>
            )}

            {stepResult.error && (
              <div className="mt-4">
                <Text strong type="danger">Error:</Text>
                <div className="bg-red-50 p-3 rounded border border-red-200 mt-2">
                  <Text type="danger">{stepResult.error}</Text>
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
                  <Space>
                    <Button
                      type="text"
                      size="small"
                      icon={<CopyOutlined />}
                      onClick={() => copyToClipboard(stepResult.prompt)}
                    >
                      Copy
                    </Button>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {stepResult.prompt.length} characters
                    </Text>
                  </Space>
                </div>
                <div className="bg-gray-50 p-3 rounded border" style={{ maxHeight: '800px', overflowY: 'auto' }}>
                  <Text code className="whitespace-pre-wrap" style={{ fontSize: '12px' }}>
                    {stepResult.prompt}
                  </Text>
                </div>
              </div>
            )}
          </div>
        );

      case "AIGeneration":
      case PipelineStep.AIGeneration:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="SQL Length">
                {stepResult.sqlLength || 0} characters
              </Descriptions.Item>
              <Descriptions.Item label="Estimated Cost">
                ${stepResult.estimatedCost?.toFixed(4) || '0.0000'}
              </Descriptions.Item>
              <Descriptions.Item label="Success">
                <Tag color={stepResult.success ? 'green' : 'red'}>
                  {stepResult.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
            </Descriptions>

            {stepResult.generatedSQL && (
              <div>
                <div className="flex items-center justify-between mb-2">
                  <Text strong>Generated SQL:</Text>
                  <Space>
                    <Button
                      type="text"
                      size="small"
                      icon={<CopyOutlined />}
                      onClick={() => copyToClipboard(stepResult.generatedSQL)}
                    >
                      Copy
                    </Button>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {stepResult.generatedSQL.length} characters
                    </Text>
                  </Space>
                </div>
                <div className="bg-gray-50 p-3 rounded border" style={{ maxHeight: '400px', overflowY: 'auto' }}>
                  <Text code className="whitespace-pre-wrap" style={{ fontSize: '12px' }}>
                    {stepResult.generatedSQL}
                  </Text>
                </div>
              </div>
            )}

            {stepResult.error && (
              <div className="mt-4">
                <Text strong type="danger">Error:</Text>
                <div className="bg-red-50 p-3 rounded border border-red-200 mt-2">
                  <Text type="danger">{stepResult.error}</Text>
                </div>
              </div>
            )}
          </div>
        );

      case PipelineStep.SQLValidation:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="Validation Status">
                <Tag color={stepResult.isValid ? 'green' : 'red'}>
                  {stepResult.isValid ? 'Valid' : 'Invalid'}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Validation Time">
                {stepResult.validationTime}ms
              </Descriptions.Item>
            </Descriptions>
            {stepResult.validationErrors && stepResult.validationErrors.length > 0 && (
              <div className="mt-3">
                <Text strong className="text-red-600">Validation Errors:</Text>
                <ul className="mt-2">
                  {stepResult.validationErrors.map((error: string, index: number) => (
                    <li key={index} className="text-red-600">{error}</li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        );

      case PipelineStep.SQLExecution:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="Execution Status">
                <Tag color={stepResult.success ? 'green' : 'red'}>
                  {stepResult.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Execution Time">
                {stepResult.executionTime}ms
              </Descriptions.Item>
              <Descriptions.Item label="Rows Returned">
                {stepResult.rowCount || 0}
              </Descriptions.Item>
            </Descriptions>
            {stepResult.error && (
              <div className="mt-3">
                <Text strong className="text-red-600">Execution Error:</Text>
                <div className="bg-red-50 p-3 rounded mt-2">
                  <Text code className="text-red-600">{stepResult.error}</Text>
                </div>
              </div>
            )}
          </div>
        );

      case PipelineStep.ResultsProcessing:
        return (
          <div>
            <Descriptions bordered size="small" className="mb-4">
              <Descriptions.Item label="Processing Status">
                <Tag color={stepResult.success ? 'green' : 'red'}>
                  {stepResult.success ? 'Success' : 'Failed'}
                </Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Processing Time">
                {stepResult.processingTime}ms
              </Descriptions.Item>
              <Descriptions.Item label="Final Result Size">
                {stepResult.resultSize || 0} bytes
              </Descriptions.Item>
            </Descriptions>
            {stepResult.finalResults && (
              <div className="mt-3">
                <Text strong>Final Results:</Text>
                <div className="bg-gray-50 p-3 rounded mt-2 max-h-60 overflow-y-auto">
                  <Text code className="whitespace-pre-wrap">
                    {JSON.stringify(stepResult.finalResults, null, 2)}
                  </Text>
                </div>
              </div>
            )}
          </div>
        );

      default:
        console.log(`🔍 [DEFAULT-CASE] Rendering default for step: ${step}`);
        console.log(`🔍 [DEFAULT-CASE] stepResult:`, stepResult);
        return (
          <div className="bg-yellow-50 p-3 rounded border border-yellow-200">
            <div className="mb-2">
              <Text strong className="text-orange-600">⚠️ No specific renderer for step: {step}</Text>
            </div>
            <Text code className="whitespace-pre-wrap">{JSON.stringify(stepResult, null, 2)}</Text>
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
