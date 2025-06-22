import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Input,
  Select,
  Steps,
  Progress,
  Statistic,
  Alert,
  Tabs,
  Typography,
  Space,
  Tag,
  Divider,
  notification
} from 'antd';
import {
  PlayCircleOutlined,
  CheckCircleOutlined,
  LoadingOutlined,
  ClockCircleOutlined,
  DollarOutlined,
  DatabaseOutlined,
  EyeOutlined,
  BugOutlined
} from '@ant-design/icons';

const { TextArea } = Input;
const { Option } = Select;
const { Step } = Steps;
const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;

interface TransparencyData {
  traceId: string;
  businessContext: {
    intent: string;
    confidence: number;
    domain: string;
    entities: string[];
  };
  tokenUsage: {
    allocatedTokens: number;
    estimatedCost: number;
    provider: string;
  };
  processingSteps: number;
}

interface TestMetadata {
  startTime: string;
  endTime: string;
  totalDurationMs: number;
  userId: string;
  environment: string;
}

interface QueryResult {
  traceId: string;
  success: boolean;
  query: string;
  generatedSql?: string;
  results?: any;
  transparencyData?: TransparencyData;
  error?: string;
  testMetadata?: TestMetadata;
}

const TransparencyTestPage: React.FC = () => {
  // State management
  const [authToken, setAuthToken] = useState<string>('');
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [currentStep, setCurrentStep] = useState(0);
  const [isProcessing, setIsProcessing] = useState(false);
  const [query, setQuery] = useState('');
  const [queryResult, setQueryResult] = useState<QueryResult | null>(null);
  const [progress, setProgress] = useState(0);

  // Sample queries for testing
  const sampleQueries = [
    'Show me sales data for Q3 2024',
    'What are the top performing products this year?',
    'Compare revenue trends by region',
    'Find anomalies in customer behavior',
    'Generate monthly sales report for last 6 months',
    'Which customers have the highest lifetime value?',
    'Show inventory levels by product category',
    'Analyze seasonal trends in sales data'
  ];

  // Auto-authentication on component mount
  useEffect(() => {
    authenticateUser();
  }, []);

  const authenticateUser = async () => {
    try {
      const response = await fetch('/api/test/auth/token', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          userId: 'admin@bireporting.com',
          userName: 'Test User'
        }),
      });

      if (response.ok) {
        const tokenData = await response.json();
        setAuthToken(tokenData.token);
        setIsAuthenticated(true);
        notification.success({
          message: 'Authentication Successful',
          description: `Authenticated as ${tokenData.userName}`,
          duration: 3,
        });
      } else {
        throw new Error('Authentication failed');
      }
    } catch (error) {
      notification.error({
        message: 'Authentication Failed',
        description: 'Could not obtain test authentication token',
        duration: 5,
      });
    }
  };

  const executeQuery = async () => {
    if (!query.trim()) {
      notification.warning({
        message: 'Query Required',
        description: 'Please enter a query to test',
      });
      return;
    }

    setIsProcessing(true);
    setCurrentStep(0);
    setProgress(0);
    setQueryResult(null);

    try {
      // Simulate progress steps
      const steps = [
        'Authenticating...',
        'Analyzing business context...',
        'Managing token budget...',
        'Constructing prompt...',
        'Generating SQL...',
        'Executing query...',
        'Saving transparency data...'
      ];

      for (let i = 0; i < steps.length; i++) {
        setCurrentStep(i);
        setProgress((i + 1) * (100 / steps.length));
        await new Promise(resolve => setTimeout(resolve, 500));
      }

      const response = await fetch('/api/test/query/enhanced', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: query,
          executeQuery: true,
          includeAlternatives: true,
          includeSemanticAnalysis: true,
        }),
      });

      if (response.ok) {
        const result = await response.json();
        setQueryResult(result);
        setCurrentStep(steps.length);
        setProgress(100);
        
        notification.success({
          message: 'Query Executed Successfully',
          description: `Trace ID: ${result.traceId}`,
          duration: 5,
        });
      } else {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
    } catch (error) {
      notification.error({
        message: 'Query Execution Failed',
        description: error instanceof Error ? error.message : 'Unknown error occurred',
        duration: 5,
      });
      setQueryResult({
        traceId: 'error',
        success: false,
        query: query,
        error: error instanceof Error ? error.message : 'Unknown error occurred'
      });
    } finally {
      setIsProcessing(false);
    }
  };

  const resetTest = () => {
    setCurrentStep(0);
    setProgress(0);
    setQueryResult(null);
    setQuery('');
  };

  return (
    <div style={{ padding: '24px', maxWidth: '1400px', margin: '0 auto' }}>
      <Title level={2}>
        <BugOutlined style={{ marginRight: '8px', color: '#1890ff' }} />
        AI Transparency Foundation - Test Page
      </Title>
      
      <Paragraph>
        This test page demonstrates the complete AI transparency flow: Question → Business Context → Token Budget → Prompt → Query → Result.
        All transparency data is saved to the database and can be verified in real-time.
      </Paragraph>

      {/* Authentication Status */}
      <Card size="small" style={{ marginBottom: '16px' }}>
        <Row align="middle">
          <Col span={12}>
            <Space>
              <CheckCircleOutlined style={{ color: isAuthenticated ? '#52c41a' : '#d9d9d9' }} />
              <Text strong>Authentication Status:</Text>
              <Tag color={isAuthenticated ? 'green' : 'red'}>
                {isAuthenticated ? 'Authenticated' : 'Not Authenticated'}
              </Tag>
            </Space>
          </Col>
          <Col span={12} style={{ textAlign: 'right' }}>
            {!isAuthenticated && (
              <Button type="primary" onClick={authenticateUser}>
                Authenticate
              </Button>
            )}
          </Col>
        </Row>
      </Card>

      <Row gutter={[16, 16]}>
        {/* Left Column - Query Input and Progress */}
        <Col span={12}>
          <Card title="Query Input & Execution" style={{ height: '600px' }}>
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {/* Query Input */}
              <div>
                <Text strong>Test Query:</Text>
                <TextArea
                  rows={4}
                  value={query}
                  onChange={(e) => setQuery(e.target.value)}
                  placeholder="Enter your test query here..."
                  disabled={isProcessing}
                />
              </div>

              {/* Sample Queries */}
              <div>
                <Text strong>Sample Queries:</Text>
                <Select
                  style={{ width: '100%' }}
                  placeholder="Select a sample query"
                  onChange={(value) => setQuery(value)}
                  disabled={isProcessing}
                >
                  {sampleQueries.map((sample, index) => (
                    <Option key={index} value={sample}>
                      {sample}
                    </Option>
                  ))}
                </Select>
              </div>

              {/* Action Buttons */}
              <Space>
                <Button
                  type="primary"
                  icon={<PlayCircleOutlined />}
                  onClick={executeQuery}
                  disabled={!isAuthenticated || isProcessing || !query.trim()}
                  loading={isProcessing}
                >
                  Execute Test Query
                </Button>
                <Button onClick={resetTest} disabled={isProcessing}>
                  Reset
                </Button>
              </Space>

              {/* Progress */}
              {(isProcessing || queryResult) && (
                <div>
                  <Text strong>Progress:</Text>
                  <Progress percent={progress} status={isProcessing ? 'active' : 'success'} />
                  
                  <Steps
                    current={currentStep}
                    direction="vertical"
                    size="small"
                    style={{ marginTop: '16px' }}
                  >
                    <Step title="Authentication" icon={isProcessing && currentStep === 0 ? <LoadingOutlined /> : undefined} />
                    <Step title="Business Context Analysis" icon={isProcessing && currentStep === 1 ? <LoadingOutlined /> : undefined} />
                    <Step title="Token Budget Management" icon={isProcessing && currentStep === 2 ? <LoadingOutlined /> : undefined} />
                    <Step title="Prompt Construction" icon={isProcessing && currentStep === 3 ? <LoadingOutlined /> : undefined} />
                    <Step title="SQL Generation" icon={isProcessing && currentStep === 4 ? <LoadingOutlined /> : undefined} />
                    <Step title="Query Execution" icon={isProcessing && currentStep === 5 ? <LoadingOutlined /> : undefined} />
                    <Step title="Transparency Logging" icon={isProcessing && currentStep === 6 ? <LoadingOutlined /> : undefined} />
                  </Steps>
                </div>
              )}
            </Space>
          </Card>
        </Col>

        {/* Right Column - Results and Transparency Data */}
        <Col span={12}>
          <Card title="Results & Transparency Data" style={{ height: '600px' }}>
            {queryResult ? (
              <Tabs defaultActiveKey="1" style={{ height: '100%' }}>
                <TabPane tab="Overview" key="1">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Alert
                      message={queryResult.success ? 'Query Executed Successfully' : 'Query Execution Failed'}
                      type={queryResult.success ? 'success' : 'error'}
                      showIcon
                    />
                    
                    <Row gutter={16}>
                      <Col span={12}>
                        <Statistic
                          title="Trace ID"
                          value={queryResult.traceId}
                          valueStyle={{ fontSize: '14px' }}
                        />
                      </Col>
                      <Col span={12}>
                        <Statistic
                          title="Duration"
                          value={queryResult.testMetadata?.totalDurationMs || 0}
                          suffix="ms"
                          prefix={<ClockCircleOutlined />}
                        />
                      </Col>
                    </Row>

                    {queryResult.error && (
                      <Alert message="Error Details" description={queryResult.error} type="error" />
                    )}
                  </Space>
                </TabPane>

                <TabPane tab="Transparency" key="2">
                  {queryResult.transparencyData ? (
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <div>
                        <Text strong>Business Context:</Text>
                        <div style={{ marginLeft: '16px' }}>
                          <Text>Intent: <Tag>{queryResult.transparencyData.businessContext.intent}</Tag></Text><br />
                          <Text>Confidence: {(queryResult.transparencyData.businessContext.confidence * 100).toFixed(1)}%</Text><br />
                          <Text>Domain: <Tag color="blue">{queryResult.transparencyData.businessContext.domain}</Tag></Text>
                        </div>
                      </div>

                      <Divider />

                      <div>
                        <Text strong>Token Usage:</Text>
                        <Row gutter={16} style={{ marginTop: '8px' }}>
                          <Col span={8}>
                            <Statistic
                              title="Allocated"
                              value={queryResult.transparencyData.tokenUsage.allocatedTokens}
                              suffix="tokens"
                            />
                          </Col>
                          <Col span={8}>
                            <Statistic
                              title="Cost"
                              value={queryResult.transparencyData.tokenUsage.estimatedCost}
                              prefix={<DollarOutlined />}
                              precision={4}
                            />
                          </Col>
                          <Col span={8}>
                            <Statistic
                              title="Steps"
                              value={queryResult.transparencyData.processingSteps}
                            />
                          </Col>
                        </Row>
                      </div>
                    </Space>
                  ) : (
                    <Alert message="No transparency data available" type="info" />
                  )}
                </TabPane>

                <TabPane tab="SQL" key="3">
                  {queryResult.generatedSql ? (
                    <div>
                      <Text strong>Generated SQL:</Text>
                      <pre style={{ 
                        background: '#f5f5f5', 
                        padding: '12px', 
                        borderRadius: '4px',
                        marginTop: '8px',
                        fontSize: '12px'
                      }}>
                        {queryResult.generatedSql}
                      </pre>
                    </div>
                  ) : (
                    <Alert message="No SQL generated" type="info" />
                  )}
                </TabPane>

                <TabPane tab="Database" key="4">
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Alert
                      message="Database Verification"
                      description="This tab will show what data was saved to the transparency tables"
                      type="info"
                      icon={<DatabaseOutlined />}
                    />
                    <Button icon={<EyeOutlined />} type="dashed">
                      Verify Database Records
                    </Button>
                  </Space>
                </TabPane>
              </Tabs>
            ) : (
              <div style={{ textAlign: 'center', padding: '60px 0' }}>
                <Text type="secondary">Execute a query to see results and transparency data</Text>
              </div>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default TransparencyTestPage;
