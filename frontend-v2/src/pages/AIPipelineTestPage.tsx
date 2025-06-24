import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Input,
  Checkbox,
  Form,
  Typography,
  Space,
  Divider,
  Alert,
  Spin,
  Collapse,
  Tag,
  Progress,
  Statistic,
  Switch,
  InputNumber,
  Tooltip,
  message
} from 'antd';
import {
  PlayCircleOutlined,
  SettingOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  BugOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';
import { useAIPipelineTestApi } from '../hooks/useAIPipelineTestApi';
import { usePipelineTestMonitoring } from '../hooks/usePipelineTestMonitoring';
import { PipelineStep, PipelineTestRequest, PipelineTestResult, PipelineStepInfo, PipelineTestConfiguration, formatPipelineStepName } from '../types/aiPipelineTest';
import AIPipelineTestResults from '../components/AIPipelineTestResults';
import '../utils/testEnumNormalization'; // Import to run tests
import PipelineTestMonitoringDashboard from '../components/PipelineTestMonitoringDashboard';
import EnhancedMonitoringDashboard from '../components/EnhancedMonitoringDashboard';
import PipelineTestConfigurationManager from '../components/PipelineTestConfigurationManager';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;
const { Panel } = Collapse;

const AIPipelineTestPage: React.FC = () => {
  const [form] = Form.useForm();
  const [selectedSteps, setSelectedSteps] = useState<PipelineStep[]>([]);
  const [testQuery, setTestQuery] = useState('Top 10 depositors yesterday from UK');
  const [isRunning, setIsRunning] = useState(false);
  const [testResult, setTestResult] = useState<PipelineTestResult | null>(null);
  const [availableSteps, setAvailableSteps] = useState<PipelineStepInfo[]>([]);
  const [parameters, setParameters] = useState<Record<string, any>>({});
  const [currentTestId, setCurrentTestId] = useState<string | null>(null);
  const [showMonitoring, setShowMonitoring] = useState(false);
  const [useEnhancedMonitoring, setUseEnhancedMonitoring] = useState(true);

  const { testPipelineSteps, getAvailableSteps } = useAIPipelineTestApi();
  const {
    isConnected,
    currentSession,
    stepProgress,
    connectionError,
    connect,
    disconnect,
    joinTestSession,
    leaveTestSession
  } = usePipelineTestMonitoring();

  useEffect(() => {
    loadAvailableSteps();
  }, []);

  // Connect to monitoring when component mounts
  useEffect(() => {
    if (showMonitoring && !isConnected) {
      connect();
    }
    return () => {
      if (isConnected) {
        disconnect();
      }
    };
  }, [showMonitoring, isConnected, connect, disconnect]);

  // Join test session when test starts
  useEffect(() => {
    if (currentTestId && isConnected && showMonitoring) {
      joinTestSession(currentTestId);
    }
  }, [currentTestId, isConnected, showMonitoring, joinTestSession]);

  // Listen for test completion
  useEffect(() => {
    if (currentSession?.status === 'completed' && currentSession.testId === currentTestId) {
      // Test completed, refresh the result
      console.log('🎉 Test completed via SignalR, refreshing result...');
      // The monitoring dashboard should handle this, but we could also fetch the final result here
    }
  }, [currentSession, currentTestId]);

  useEffect(() => {
    console.log('🔍 testResult state changed:', testResult);
  }, [testResult]);

  const loadAvailableSteps = async () => {
    try {
      const steps = await getAvailableSteps();
      setAvailableSteps(steps);
      
      // Set default parameters
      const defaultParams: Record<string, any> = {};
      steps.forEach(step => {
        step.parameters.forEach(param => {
          defaultParams[param.name] = parseParameterValue(param.defaultValue, param.type);
        });
      });
      setParameters(defaultParams);
    } catch (error) {
      console.error('Failed to load available steps:', error);
    }
  };

  const parseParameterValue = (value: string, type: string): any => {
    switch (type) {
      case 'bool':
        return value.toLowerCase() === 'true';
      case 'int':
        return parseInt(value, 10);
      case 'decimal':
        return parseFloat(value);
      default:
        return value;
    }
  };

  const handleStepToggle = (step: PipelineStep, checked: boolean) => {
    if (checked) {
      setSelectedSteps(prev => [...prev, step]);
    } else {
      setSelectedSteps(prev => prev.filter(s => s !== step));
    }
  };

  const handleParameterChange = (paramName: string, value: any) => {
    setParameters(prev => ({
      ...prev,
      [paramName]: value
    }));
  };

  const runTest = async () => {
    if (selectedSteps.length === 0) {
      message.warning('Please select at least one step to test');
      return;
    }

    // Generate test ID immediately and join session for real-time monitoring
    const testId = `test_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    setIsRunning(true);
    setTestResult(null);
    setCurrentTestId(testId); // Set test ID immediately
    setShowMonitoring(true);

    // Join test session immediately for real-time updates
    if (isConnected) {
      await joinTestSession(testId);
      console.log(`🔗 Joined test session: ${testId}`);
    }

    try {
      const request: PipelineTestRequest = {
        testId, // Pass the test ID to backend
        query: testQuery,
        steps: selectedSteps,
        parameters
      };

      console.log('🚀 Sending test request:', request);
      message.info('Starting pipeline test...');

      const result = await testPipelineSteps(request);
      console.log('✅ Received test result:', result);
      console.log('🔍 Setting testResult state...');

      setTestResult(result);

      console.log('✅ State updated, testResult should now be:', result);

      if (result.success) {
        message.success('Pipeline test completed successfully!');
      } else {
        message.error(`Pipeline test failed: ${result.error || 'Unknown error'}`);
      }
    } catch (error) {
      console.error('❌ Test failed:', error);
      message.error('Failed to execute pipeline test');
    } finally {
      setIsRunning(false);
    }
  };

  const runMockTest = async () => {
    if (selectedSteps.length === 0) {
      message.warning('Please select at least one step to test');
      return;
    }

    setIsRunning(true);
    setTestResult(null);
    setShowMonitoring(true);
    message.info('Starting mock pipeline test...');

    // Simulate test execution
    await new Promise(resolve => setTimeout(resolve, 3000));

    // Create mock result
    const mockResult: PipelineTestResult = {
      testId: 'mock-test-' + Date.now(),
      query: testQuery,
      requestedSteps: selectedSteps,
      startTime: new Date().toISOString(),
      endTime: new Date().toISOString(),
      totalDurationMs: 3000,
      success: true,
      error: null,
      results: {
        [selectedSteps[0]]: {
          success: true,
          durationMs: 1500,
          error: null
        }
      },
      businessProfile: null,
      tokenBudget: null,
      schemaMetadata: null,
      generatedPrompt: null,
      generatedSQL: null
    };

    setTestResult(mockResult);
    setCurrentTestId(mockResult.testId);
    setIsRunning(false);
    message.success('Mock pipeline test completed successfully!');
  };

  const handleLoadConfiguration = (config: PipelineTestConfiguration) => {
    setSelectedSteps(config.steps);
    setParameters(config.parameters);
    setTestQuery(config.description || testQuery);
  };

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
    if (!testResult) return 'default';
    
    const stepResult = testResult.results[step];
    if (!stepResult) return 'default';
    
    return stepResult.success ? 'success' : 'error';
  };

  const renderStepConfiguration = () => (
    <Card title={<><SettingOutlined /> Step Configuration</>} className="mb-4">
      <Row gutter={[16, 16]}>
        {availableSteps.map(stepInfo => (
          <Col span={24} key={stepInfo.step}>
            <Card 
              size="small" 
              className={selectedSteps.includes(stepInfo.step) ? 'border-blue-400' : ''}
            >
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center space-x-2">
                  <span className="text-lg">{getStepIcon(stepInfo.step)}</span>
                  <Checkbox
                    checked={selectedSteps.includes(stepInfo.step)}
                    onChange={(e) => handleStepToggle(stepInfo.step, e.target.checked)}
                  >
                    <Text strong>{stepInfo.name}</Text>
                  </Checkbox>
                  {testResult && (
                    <Tag color={getStepStatus(stepInfo.step)}>
                      {getStepStatus(stepInfo.step) === 'success' ? 'Success' : 'Error'}
                    </Tag>
                  )}
                </div>
              </div>
              
              <Paragraph className="text-sm text-gray-600 mb-3">
                {stepInfo.description}
              </Paragraph>

              {selectedSteps.includes(stepInfo.step) && stepInfo.parameters.length > 0 && (
                <div className="bg-gray-50 p-3 rounded">
                  <Text strong className="block mb-2">Parameters:</Text>
                  <Row gutter={[8, 8]}>
                    {stepInfo.parameters.map(param => (
                      <Col span={12} key={param.name}>
                        <div>
                          <Text className="text-xs text-gray-600">{param.name}</Text>
                          <Tooltip title={param.description}>
                            {param.type === 'bool' ? (
                              <Switch
                                size="small"
                                checked={parameters[param.name] || false}
                                onChange={(checked) => handleParameterChange(param.name, checked)}
                                className="ml-2"
                              />
                            ) : param.type === 'int' || param.type === 'decimal' ? (
                              <InputNumber
                                size="small"
                                value={parameters[param.name]}
                                onChange={(value) => handleParameterChange(param.name, value)}
                                className="w-full"
                                step={param.type === 'decimal' ? 0.1 : 1}
                              />
                            ) : (
                              <Input
                                size="small"
                                value={parameters[param.name] || ''}
                                onChange={(e) => handleParameterChange(param.name, e.target.value)}
                              />
                            )}
                          </Tooltip>
                        </div>
                      </Col>
                    ))}
                  </Row>
                </div>
              )}
            </Card>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderTestControls = () => (
    <Card title={<><BugOutlined /> Test Controls</>} className="mb-4">
      <Space direction="vertical" className="w-full">
        <div>
          <Text strong>Test Query:</Text>
          <TextArea
            value={testQuery}
            onChange={(e) => setTestQuery(e.target.value)}
            placeholder="Enter your test query here..."
            rows={3}
            className="mt-2"
          />
        </div>
        
        <div className="flex justify-between items-center">
          <div className="flex items-center space-x-4">
            <div>
              <Text>Selected Steps: </Text>
              <Text strong>{selectedSteps.length}</Text>
            </div>

            <Button
              type={showMonitoring ? "primary" : "default"}
              icon={<ThunderboltOutlined />}
              onClick={() => setShowMonitoring(!showMonitoring)}
              size="small"
            >
              {showMonitoring ? 'Hide Monitoring' : 'Show Real-time Monitoring'}
            </Button>
          </div>

          <Space>
            <Button
              type="primary"
              icon={<PlayCircleOutlined />}
              onClick={runTest}
              loading={isRunning}
              disabled={selectedSteps.length === 0}
              size="large"
            >
              {isRunning ? 'Running Test...' : 'Run Pipeline Test'}
            </Button>

            <Button
              type="default"
              icon={<PlayCircleOutlined />}
              onClick={runMockTest}
              loading={isRunning}
              disabled={selectedSteps.length === 0}
              size="large"
            >
              {isRunning ? 'Running Mock Test...' : 'Run Mock Test'}
            </Button>
          </Space>
        </div>

        {parameters.enableAIGeneration && (
          <Alert
            message="AI Generation Enabled"
            description="This will make actual calls to OpenAI and incur costs. Use carefully!"
            type="warning"
            showIcon
            icon={<ExclamationCircleOutlined />}
          />
        )}

        {isRunning && (
          <Alert
            message="Pipeline Test Running"
            description={`Testing ${selectedSteps.length} step(s): ${selectedSteps.map(step => formatPipelineStepName(step)).join(', ')}`}
            type="info"
            showIcon
            icon={<Spin size="small" />}
          />
        )}
      </Space>
    </Card>
  );

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <div className="mb-6">
        <Title level={2}>
          <ThunderboltOutlined className="mr-2" />
          AI Pipeline Testing & Monitoring
        </Title>
        <Paragraph>
          Test individual steps of the AI processing pipeline to debug and optimize performance.
          Select the steps you want to test, configure parameters, and monitor the results in real-time.
        </Paragraph>
      </div>

      <Row gutter={[24, 24]}>
        <Col span={showMonitoring ? 12 : 24}>
          <div>
            {renderTestControls()}

            {/* Step Selection - Moved above configuration */}
            {renderStepConfiguration()}

            {/* Configuration Manager */}
            <PipelineTestConfigurationManager
              currentSteps={selectedSteps}
              currentParameters={parameters}
              onLoadConfiguration={handleLoadConfiguration}
              onParametersChange={setParameters}
            />
          </div>
        </Col>

        {showMonitoring && (
          <Col span={12}>
            <div className="space-y-4">
              {/* Debug Panel */}
              <Card title="Test Results Debug" size="small">
                <div style={{ padding: 8, background: testResult ? '#f6ffed' : '#fff2e8', borderRadius: 4 }}>
                  <Text strong>Status: {testResult ? 'Test Result Available' : 'No Test Result'}</Text>
                  {testResult && (
                    <>
                      <br />
                      <Text>Test ID: {testResult.testId}</Text>
                      <br />
                      <Text>Success: {testResult.success ? 'Yes' : 'No'}</Text>
                      <br />
                      <Text>Steps: {JSON.stringify(testResult.requestedSteps)}</Text>
                      <br />
                      <Text>Results: {JSON.stringify(Object.keys(testResult.results || {}))}</Text>
                    </>
                  )}
                </div>
              </Card>

              {/* Monitoring Dashboard */}
              {useEnhancedMonitoring ? (
                <EnhancedMonitoringDashboard
                  testId={currentTestId || undefined}
                  autoJoinSession={true}
                  showAnalytics={true}
                  showLogs={true}
                />
              ) : (
                <PipelineTestMonitoringDashboard
                  testId={currentTestId || undefined}
                  autoJoinSession={true}
                />
              )}
            </div>
          </Col>
        )}
      </Row>

      {/* Test Results */}
      {testResult && (
        <AIPipelineTestResults
          result={testResult}
          onExport={() => {
            console.log('Export results:', testResult);
          }}
          onViewDetails={(step) => {
            console.log('View details for step:', step);
          }}
        />
      )}
    </div>
  );
};

export default AIPipelineTestPage;
