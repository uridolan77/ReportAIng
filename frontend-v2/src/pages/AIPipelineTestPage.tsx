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
  Tooltip
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
import { PipelineStep, PipelineTestRequest, PipelineTestResult, PipelineStepInfo, PipelineTestConfiguration } from '../types/aiPipelineTest';
import AIPipelineTestResults from '../components/AIPipelineTestResults';
import PipelineTestMonitoringDashboard from '../components/PipelineTestMonitoringDashboard';
import EnhancedMonitoringDashboard from '../components/EnhancedMonitoringDashboard';
import PipelineTestConfigurationManager from '../components/PipelineTestConfigurationManager';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;
const { Panel } = Collapse;

const AIPipelineTestPage: React.FC = () => {
  const [form] = Form.useForm();
  const [selectedSteps, setSelectedSteps] = useState<PipelineStep[]>([]);
  const [testQuery, setTestQuery] = useState('Show me total sales by region for last quarter');
  const [isRunning, setIsRunning] = useState(false);
  const [testResult, setTestResult] = useState<PipelineTestResult | null>(null);
  const [availableSteps, setAvailableSteps] = useState<PipelineStepInfo[]>([]);
  const [parameters, setParameters] = useState<Record<string, any>>({});
  const [currentTestId, setCurrentTestId] = useState<string | null>(null);
  const [showMonitoring, setShowMonitoring] = useState(false);
  const [useEnhancedMonitoring, setUseEnhancedMonitoring] = useState(true);

  const { testPipelineSteps, getAvailableSteps } = useAIPipelineTestApi();

  useEffect(() => {
    loadAvailableSteps();
  }, []);

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
      return;
    }

    setIsRunning(true);
    setTestResult(null);
    setShowMonitoring(true);

    try {
      const request: PipelineTestRequest = {
        query: testQuery,
        steps: selectedSteps,
        parameters
      };

      const result = await testPipelineSteps(request);
      setTestResult(result);
      setCurrentTestId(result.testId);
    } catch (error) {
      console.error('Test failed:', error);
    } finally {
      setIsRunning(false);
    }
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

            {/* Configuration Manager */}
            <PipelineTestConfigurationManager
              currentSteps={selectedSteps}
              currentParameters={parameters}
              onLoadConfiguration={handleLoadConfiguration}
              onParametersChange={setParameters}
            />

            {renderStepConfiguration()}
          </div>
        </Col>

        {showMonitoring && (
          <Col span={12}>
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
        )}
      </Row>

      {/* Test Results */}
      {testResult && (
        <AIPipelineTestResults
          result={testResult}
          onExport={() => {
            // TODO: Implement export functionality
            console.log('Export results:', testResult);
          }}
          onViewDetails={(step) => {
            // TODO: Implement detailed view
            console.log('View details for step:', step);
          }}
        />
      )}
    </div>
  );
};

export default AIPipelineTestPage;
