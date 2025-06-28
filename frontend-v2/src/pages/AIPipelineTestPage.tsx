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
import { ComprehensiveSignalRMonitor } from '../components/ComprehensiveSignalRMonitor';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;

const AIPipelineTestPage: React.FC = () => {
  const [form] = Form.useForm();
  const [selectedSteps, setSelectedSteps] = useState<PipelineStep[]>([
    PipelineStep.BusinessContextAnalysis,
    PipelineStep.TokenBudgetManagement,
    PipelineStep.SchemaRetrieval,
    PipelineStep.PromptBuilding,
    PipelineStep.AIGeneration,
    PipelineStep.SQLValidation,
    PipelineStep.SQLExecution,
    PipelineStep.ResultsProcessing
  ]);
  const [testQuery, setTestQuery] = useState('Top 10 depositors yesterday from UK');
  const [isRunning, setIsRunning] = useState(false);
  const [testResult, setTestResult] = useState<PipelineTestResult | null>(null);
  const [availableSteps, setAvailableSteps] = useState<PipelineStepInfo[]>([]);
  const [parameters, setParameters] = useState<Record<string, any>>({});
  const [currentTestId, setCurrentTestId] = useState<string | null>(null);
  const [showMonitoring, setShowMonitoring] = useState(false);
  const [useEnhancedMonitoring, setUseEnhancedMonitoring] = useState(true);
  const [showComprehensiveMonitor, setShowComprehensiveMonitor] = useState(true);

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
          // Override specific defaults
          if (param.name === 'useAI' || param.name === 'enableExecution') {
            defaultParams[param.name] = true;
          } else {
            defaultParams[param.name] = parseParameterValue(param.defaultValue, param.type);
          }
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

  const handleSelectAllSteps = () => {
    const allSteps = availableSteps.map(step => step.step);
    setSelectedSteps(allSteps);
  };

  const handleDeselectAllSteps = () => {
    setSelectedSteps([]);
  };

  const isAllStepsSelected = availableSteps.length > 0 && selectedSteps.length === availableSteps.length;

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
      console.log('🔍 [FRONTEND-DEBUG] Frontend code updated - step data merging enabled');
      message.info('Starting pipeline test...');

      const result = await testPipelineSteps(request);
      console.log('✅ Received test result:', result);
      console.log('🔍 Setting testResult state...');

      // Wait a moment for any remaining SignalR events to arrive
      await new Promise(resolve => setTimeout(resolve, 500));

      // Merge real-time step data from monitoring into the final result
      const enhancedResult = { ...result };
      console.log('🔍 Current stepProgress state:', stepProgress);
      console.log('🔍 stepProgress keys:', stepProgress ? Object.keys(stepProgress) : 'stepProgress is null/undefined');

      if (stepProgress && Object.keys(stepProgress).length > 0) {
        console.log('🔄 Merging real-time step data into result:', stepProgress);
        enhancedResult.results = {};

        // Convert stepProgress data to the expected results format
        Object.entries(stepProgress).forEach(([stepName, progress]) => {
          console.log(`🔍 Processing step ${stepName}:`, progress);
          console.log(`🔍 Step ${stepName} - status: ${progress.status}, has details: ${!!progress.details}`);

          if (stepName === 'AIGeneration') {
            console.log(`🤖 [AI-GENERATION-DEBUG] Step details:`, progress.details);
            console.log(`🤖 [AI-GENERATION-DEBUG] Step status:`, progress.status);
            console.log(`🤖 [AI-GENERATION-DEBUG] Full progress object:`, progress);
          }

          if (progress.details && progress.status === 'completed') {
            console.log(`✅ Adding step ${stepName} details:`, progress.details);
            enhancedResult.results[stepName] = {
              ...progress.details,
              durationMs: progress.endTime && progress.startTime
                ? new Date(progress.endTime).getTime() - new Date(progress.startTime).getTime()
                : progress.details.durationMs || 0
            };

            if (stepName === 'AIGeneration') {
              console.log(`🤖 [AI-GENERATION-DEBUG] Added to enhancedResult.results:`, enhancedResult.results[stepName]);
            }
          } else {
            console.log(`⚠️ Skipping step ${stepName} - status: ${progress.status}, has details: ${!!progress.details}`);

            if (stepName === 'AIGeneration') {
              console.log(`🤖 [AI-GENERATION-DEBUG] AIGeneration step was skipped!`);
              console.log(`🤖 [AI-GENERATION-DEBUG] Reason - status: ${progress.status}, has details: ${!!progress.details}`);
            }
          }
        });

        console.log('✅ Enhanced result with real-time data:', enhancedResult);
      } else {
        console.log('⚠️ No stepProgress data available for merging');
      }

      setTestResult(enhancedResult);

      console.log('✅ State updated, testResult should now be:', enhancedResult);

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

    // Generate test ID immediately and join session for real-time monitoring
    const testId = `mock_test_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    setIsRunning(true);
    setTestResult(null);
    setCurrentTestId(testId);
    setShowMonitoring(true);
    message.info('Starting enhanced mock pipeline test with real-time monitoring...');

    // Join test session immediately for real-time updates
    if (isConnected) {
      await joinTestSession(testId);
    }

    // Create detailed mock results for all selected steps
    const stepResults: Record<string, any> = {};
    const startTime = new Date();

    for (let i = 0; i < selectedSteps.length; i++) {
      const step = selectedSteps[i];
      const stepName = formatPipelineStepName(step);
      const processingTime = 500 + Math.random() * 1500; // 0.5-2 seconds per step

      console.log(`🔄 Mock Step Started: ${stepName}`);

      // Simulate processing delay
      await new Promise(resolve => setTimeout(resolve, processingTime));

      // Base step result
      const baseStepResult = {
        success: true,
        durationMs: Math.round(processingTime),
        error: null,
        details: {
          stepName,
          mockData: `Detailed mock result for ${stepName}`,
          timestamp: new Date().toISOString(),
          stepIndex: i + 1,
          totalSteps: selectedSteps.length
        }
      };

      // Add step-specific properties
      if (step === PipelineStep.AIGeneration) {
        stepResults[step] = {
          ...baseStepResult,
          generatedSQL: `SELECT TOP 10 c.customer_id, c.name, SUM(d.amount) as total_deposits
FROM Customers c
JOIN Transactions t ON c.customer_id = t.customer_id
JOIN Deposits d ON t.transaction_id = d.transaction_id
WHERE c.country = 'UK' AND DATE(d.created_date) = DATEADD(day, -1, CAST(GETDATE() AS DATE))
GROUP BY c.customer_id, c.name
ORDER BY total_deposits DESC;`,
          sqlLength: 347,
          estimatedCost: 0.0023
        };
      } else if (step === PipelineStep.PromptBuilding) {
        stepResults[step] = {
          ...baseStepResult,
          prompt: `Generate SQL to find the top 10 depositors from UK yesterday. Use tables: Customers, Transactions, Deposits.
Context: Gaming platform analysis for financial reporting.
Requirements: Include customer names and total deposit amounts.`,
          promptLength: 187,
          estimatedTokens: 45
        };
      } else {
        stepResults[step] = baseStepResult;
      }

      console.log(`✅ Mock Step Completed: ${stepName} (${Math.round(processingTime)}ms)`);
    }

    // Create comprehensive mock result
    const mockResult: PipelineTestResult = {
      testId,
      query: testQuery,
      requestedSteps: selectedSteps,
      startTime: startTime.toISOString(),
      endTime: new Date().toISOString(),
      totalDurationMs: Object.values(stepResults).reduce((total: number, result: any) => total + (result.durationMs || 0), 0),
      success: true,
      error: null,
      results: stepResults,
      businessProfile: {
        intent: 'Aggregation',
        domain: 'Gaming',
        confidence: 0.91,
        entities: ['depositors', 'UK', 'yesterday', 'Top 10']
      },
      tokenBudget: {
        allocated: 4000,
        used: 2847,
        remaining: 1153
      },
      schemaMetadata: {
        tables: ['Customers', 'Transactions', 'Deposits'],
        columns: ['customer_id', 'amount', 'date', 'country'],
        relationships: ['Customers -> Transactions', 'Transactions -> Deposits']
      },
      generatedPrompt: `Generate SQL to find the top 10 depositors from UK yesterday. Use tables: Customers, Transactions, Deposits.`,
      generatedSQL: `SELECT TOP 10 c.customer_id, c.name, SUM(d.amount) as total_deposits
FROM Customers c
JOIN Transactions t ON c.customer_id = t.customer_id
JOIN Deposits d ON t.transaction_id = d.transaction_id
WHERE c.country = 'UK' AND DATE(d.created_date) = DATEADD(day, -1, CAST(GETDATE() AS DATE))
GROUP BY c.customer_id, c.name
ORDER BY total_deposits DESC;`
    };

    setTestResult(mockResult);
    setIsRunning(false);
    message.success(`Mock pipeline test completed! Executed ${selectedSteps.length} steps with detailed results.`);
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
    <Card
      title={
        <div className="flex items-center space-x-3">
          <Checkbox
            checked={isAllStepsSelected}
            indeterminate={selectedSteps.length > 0 && selectedSteps.length < availableSteps.length}
            onChange={(e) => {
              if (e.target.checked) {
                handleSelectAllSteps();
              } else {
                handleDeselectAllSteps();
              }
            }}
          >
            <SettingOutlined /> Step Configuration
          </Checkbox>
          <Tag color="blue">
            {selectedSteps.length}/{availableSteps.length} selected
          </Tag>
        </div>
      }
      className="mb-4"
    >
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

            {showMonitoring && (
              <>
                <Button
                  type={showComprehensiveMonitor ? "primary" : "default"}
                  icon={<BugOutlined />}
                  onClick={() => setShowComprehensiveMonitor(!showComprehensiveMonitor)}
                  size="small"
                >
                  {showComprehensiveMonitor ? 'Hide SignalR Monitor' : 'Show SignalR Monitor'}
                </Button>

                <Button
                  type={useEnhancedMonitoring ? "primary" : "default"}
                  icon={<SettingOutlined />}
                  onClick={() => setUseEnhancedMonitoring(!useEnhancedMonitoring)}
                  size="small"
                >
                  {useEnhancedMonitoring ? 'Enhanced Mode' : 'Basic Mode'}
                </Button>
              </>
            )}
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

        {parameters.enableExecution && (
          <Alert
            message="SQL Execution Enabled"
            description="This will execute generated SQL against the database. Ensure you have proper permissions and backups!"
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
              <Row gutter={[16, 16]}>
                {showComprehensiveMonitor && (
                  <Col span={24}>
                    <ComprehensiveSignalRMonitor testResult={testResult} />
                  </Col>
                )}
                <Col span={24}>
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
                </Col>
              </Row>
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
