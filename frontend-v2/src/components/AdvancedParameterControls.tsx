import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  InputNumber,
  Switch,
  Slider,
  Select,
  Button,
  Tooltip,
  Alert,
  Divider,
  Space,
  Tag,
  Collapse,
  Form,
  Typography,
  Progress,
  Badge
} from 'antd';
import {
  SettingOutlined,
  ReloadOutlined,
  SaveOutlined,
  ThunderboltOutlined,
  ExperimentOutlined,
  BulbOutlined,
  WarningOutlined,
  CheckCircleOutlined
} from '@ant-design/icons';
import { PipelineTestParameters, ParameterValidationResult } from '../types/aiPipelineTest';

const { Option } = Select;
const { Text, Title } = Typography;

interface AdvancedParameterControlsProps {
  parameters: PipelineTestParameters;
  onParametersChange: (parameters: PipelineTestParameters) => void;
  validationResult?: ParameterValidationResult | null;
  onValidate?: () => void;
  isValidating?: boolean;
}

interface ParameterPreset {
  name: string;
  description: string;
  category: 'performance' | 'cost' | 'accuracy' | 'debugging';
  parameters: Partial<PipelineTestParameters>;
  icon: string;
}

const PARAMETER_PRESETS: ParameterPreset[] = [
  {
    name: 'High Performance',
    description: 'Optimized for speed and throughput',
    category: 'performance',
    icon: '⚡',
    parameters: {
      maxTokens: 2000,
      reservedResponseTokens: 300,
      confidenceThreshold: 0.6,
      maxTables: 5,
      temperature: 0.1,
      includeExamples: false,
      includeBusinessRules: false,
      enableAIGeneration: false
    }
  },
  {
    name: 'Cost Optimized',
    description: 'Minimal token usage and API calls',
    category: 'cost',
    icon: '💰',
    parameters: {
      maxTokens: 1500,
      reservedResponseTokens: 200,
      confidenceThreshold: 0.8,
      maxTables: 3,
      temperature: 0.0,
      includeExamples: false,
      includeBusinessRules: false,
      enableAIGeneration: false
    }
  },
  {
    name: 'High Accuracy',
    description: 'Maximum context and examples for best results',
    category: 'accuracy',
    icon: '🎯',
    parameters: {
      maxTokens: 6000,
      reservedResponseTokens: 800,
      confidenceThreshold: 0.9,
      maxTables: 15,
      temperature: 0.2,
      includeExamples: true,
      includeBusinessRules: true,
      enableAIGeneration: false
    }
  },
  {
    name: 'Debug Mode',
    description: 'Detailed analysis with comprehensive context',
    category: 'debugging',
    icon: '🔍',
    parameters: {
      maxTokens: 4000,
      reservedResponseTokens: 500,
      confidenceThreshold: 0.5,
      maxTables: 20,
      temperature: 0.3,
      includeExamples: true,
      includeBusinessRules: true,
      enableAIGeneration: false
    }
  }
];

const AdvancedParameterControls: React.FC<AdvancedParameterControlsProps> = ({
  parameters,
  onParametersChange,
  validationResult,
  onValidate,
  isValidating = false
}) => {
  const [activePreset, setActivePreset] = useState<string | null>(null);
  const [customParameters, setCustomParameters] = useState<PipelineTestParameters>(parameters);

  useEffect(() => {
    setCustomParameters(parameters);
  }, [parameters]);

  const handleParameterChange = (key: keyof PipelineTestParameters, value: any) => {
    const newParameters = { ...customParameters, [key]: value };
    setCustomParameters(newParameters);
    onParametersChange(newParameters);
    setActivePreset(null); // Clear preset when manually changing parameters
  };

  const applyPreset = (preset: ParameterPreset) => {
    const newParameters = { ...customParameters, ...preset.parameters };
    setCustomParameters(newParameters);
    onParametersChange(newParameters);
    setActivePreset(preset.name);
  };

  const resetToDefaults = () => {
    const defaultParameters: PipelineTestParameters = {
      maxTokens: 4000,
      reservedResponseTokens: 500,
      confidenceThreshold: 0.7,
      maxTables: 10,
      temperature: 0.1,
      includeExamples: true,
      includeBusinessRules: true,
      enableAIGeneration: false
    };
    setCustomParameters(defaultParameters);
    onParametersChange(defaultParameters);
    setActivePreset(null);
  };

  const getPresetCategoryColor = (category: string) => {
    const colors = {
      performance: 'blue',
      cost: 'green',
      accuracy: 'purple',
      debugging: 'orange'
    };
    return colors[category as keyof typeof colors] || 'default';
  };

  const calculateTokenCost = () => {
    const inputTokens = (customParameters.maxTokens || 4000) - (customParameters.reservedResponseTokens || 500);
    const outputTokens = customParameters.reservedResponseTokens || 500;
    
    // Rough cost estimation (GPT-4 pricing as example)
    const inputCost = (inputTokens / 1000) * 0.03; // $0.03 per 1K input tokens
    const outputCost = (outputTokens / 1000) * 0.06; // $0.06 per 1K output tokens
    
    return {
      inputTokens,
      outputTokens,
      estimatedCost: inputCost + outputCost
    };
  };

  const renderPresets = () => (
    <Card title={<><ExperimentOutlined /> Parameter Presets</>} size="small" className="mb-4">
      <Row gutter={[8, 8]}>
        {PARAMETER_PRESETS.map(preset => (
          <Col span={6} key={preset.name}>
            <Card
              size="small"
              hoverable
              className={`cursor-pointer transition-all ${
                activePreset === preset.name ? 'border-blue-500 bg-blue-50' : ''
              }`}
              onClick={() => applyPreset(preset)}
            >
              <div className="text-center">
                <div className="text-2xl mb-1">{preset.icon}</div>
                <Text strong className="block">{preset.name}</Text>
                <Text type="secondary" className="text-xs block mt-1">
                  {preset.description}
                </Text>
                <Tag 
                  color={getPresetCategoryColor(preset.category)} 
                  size="small" 
                  className="mt-2"
                >
                  {preset.category}
                </Tag>
              </div>
            </Card>
          </Col>
        ))}
      </Row>
      
      <div className="mt-3 text-center">
        <Button 
          size="small" 
          icon={<ReloadOutlined />} 
          onClick={resetToDefaults}
        >
          Reset to Defaults
        </Button>
      </div>
    </Card>
  );

  const renderTokenManagement = () => {
    const tokenInfo = calculateTokenCost();
    const tokenUsagePercent = Math.round(((tokenInfo.inputTokens + tokenInfo.outputTokens) / 8000) * 100);
    
    return (
      <Card title={<><ThunderboltOutlined /> Token Management</>} size="small" className="mb-4">
        <Row gutter={[16, 16]}>
          <Col span={12}>
            <div className="space-y-3">
              <div>
                <div className="flex justify-between items-center mb-1">
                  <Text strong>Max Tokens</Text>
                  <Text type="secondary">{customParameters.maxTokens}</Text>
                </div>
                <Slider
                  min={500}
                  max={8000}
                  step={100}
                  value={customParameters.maxTokens}
                  onChange={(value) => handleParameterChange('maxTokens', value)}
                  tooltip={{ formatter: (value) => `${value} tokens` }}
                />
              </div>
              
              <div>
                <div className="flex justify-between items-center mb-1">
                  <Text strong>Reserved Response Tokens</Text>
                  <Text type="secondary">{customParameters.reservedResponseTokens}</Text>
                </div>
                <Slider
                  min={100}
                  max={2000}
                  step={50}
                  value={customParameters.reservedResponseTokens}
                  onChange={(value) => handleParameterChange('reservedResponseTokens', value)}
                  tooltip={{ formatter: (value) => `${value} tokens` }}
                />
              </div>
            </div>
          </Col>
          
          <Col span={12}>
            <div className="space-y-3">
              <div>
                <Text strong className="block mb-2">Token Usage Overview</Text>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <Text>Input Tokens:</Text>
                    <Text>{tokenInfo.inputTokens.toLocaleString()}</Text>
                  </div>
                  <div className="flex justify-between">
                    <Text>Output Tokens:</Text>
                    <Text>{tokenInfo.outputTokens.toLocaleString()}</Text>
                  </div>
                  <div className="flex justify-between">
                    <Text strong>Estimated Cost:</Text>
                    <Text strong>${tokenInfo.estimatedCost.toFixed(4)}</Text>
                  </div>
                </div>
              </div>
              
              <div>
                <Text strong className="block mb-1">Token Limit Usage</Text>
                <Progress 
                  percent={tokenUsagePercent} 
                  size="small"
                  status={tokenUsagePercent > 90 ? 'exception' : 'active'}
                  format={() => `${tokenUsagePercent}%`}
                />
              </div>
            </div>
          </Col>
        </Row>
      </Card>
    );
  };

  const renderQualityControls = () => (
    <Card title={<><BulbOutlined /> Quality & Accuracy Controls</>} size="small" className="mb-4">
      <Row gutter={[16, 16]}>
        <Col span={12}>
          <div className="space-y-4">
            <div>
              <div className="flex justify-between items-center mb-1">
                <Text strong>Confidence Threshold</Text>
                <Text type="secondary">{(customParameters.confidenceThreshold || 0.7) * 100}%</Text>
              </div>
              <Slider
                min={0}
                max={1}
                step={0.1}
                value={customParameters.confidenceThreshold}
                onChange={(value) => handleParameterChange('confidenceThreshold', value)}
                tooltip={{ formatter: (value) => `${Math.round((value || 0) * 100)}%` }}
              />
            </div>
            
            <div>
              <div className="flex justify-between items-center mb-1">
                <Text strong>Max Tables</Text>
                <Text type="secondary">{customParameters.maxTables}</Text>
              </div>
              <Slider
                min={1}
                max={50}
                step={1}
                value={customParameters.maxTables}
                onChange={(value) => handleParameterChange('maxTables', value)}
                tooltip={{ formatter: (value) => `${value} tables` }}
              />
            </div>
          </div>
        </Col>
        
        <Col span={12}>
          <div className="space-y-4">
            <div>
              <div className="flex justify-between items-center mb-1">
                <Text strong>AI Temperature</Text>
                <Text type="secondary">{customParameters.temperature}</Text>
              </div>
              <Slider
                min={0}
                max={2}
                step={0.1}
                value={customParameters.temperature}
                onChange={(value) => handleParameterChange('temperature', value)}
                tooltip={{ formatter: (value) => `${value} (${value === 0 ? 'Deterministic' : value < 0.5 ? 'Conservative' : value < 1 ? 'Balanced' : 'Creative'})` }}
              />
            </div>
            
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Text strong>Include Examples</Text>
                <Switch
                  checked={customParameters.includeExamples}
                  onChange={(checked) => handleParameterChange('includeExamples', checked)}
                />
              </div>
              
              <div className="flex items-center justify-between">
                <Text strong>Include Business Rules</Text>
                <Switch
                  checked={customParameters.includeBusinessRules}
                  onChange={(checked) => handleParameterChange('includeBusinessRules', checked)}
                />
              </div>
              
              <div className="flex items-center justify-between">
                <Text strong className="text-red-600">Enable AI Generation</Text>
                <Switch
                  checked={customParameters.enableAIGeneration}
                  onChange={(checked) => handleParameterChange('enableAIGeneration', checked)}
                />
              </div>

              <div className="flex items-center justify-between">
                <Text strong className="text-orange-600">Enable SQL Execution</Text>
                <Switch
                  checked={customParameters.enableExecution}
                  onChange={(checked) => handleParameterChange('enableExecution', checked)}
                />
              </div>
            </div>
          </div>
        </Col>
      </Row>
    </Card>
  );

  const renderValidationResults = () => {
    if (!validationResult) return null;

    return (
      <Card title={<><CheckCircleOutlined /> Parameter Validation</>} size="small" className="mb-4">
        <div className="space-y-3">
          {!validationResult.isValid && validationResult.errors.length > 0 && (
            <Alert
              message="Validation Errors"
              description={
                <ul className="list-disc list-inside mb-0">
                  {validationResult.errors.map((error, index) => (
                    <li key={index}>{error}</li>
                  ))}
                </ul>
              }
              type="error"
              showIcon
            />
          )}

          {validationResult.warnings.length > 0 && (
            <Alert
              message="Warnings"
              description={
                <ul className="list-disc list-inside mb-0">
                  {validationResult.warnings.map((warning, index) => (
                    <li key={index}>{warning}</li>
                  ))}
                </ul>
              }
              type="warning"
              showIcon
            />
          )}

          {validationResult.suggestions.length > 0 && (
            <Alert
              message="Suggestions"
              description={
                <ul className="list-disc list-inside mb-0">
                  {validationResult.suggestions.map((suggestion, index) => (
                    <li key={index}>{suggestion}</li>
                  ))}
                </ul>
              }
              type="info"
              showIcon
            />
          )}

          {validationResult.isValid && validationResult.errors.length === 0 && (
            <Alert
              message="Parameters Valid"
              description="All parameters are properly configured and ready for testing."
              type="success"
              showIcon
            />
          )}
        </div>

        <div className="mt-3 text-center">
          <Button 
            type="primary" 
            icon={<CheckCircleOutlined />}
            onClick={onValidate}
            loading={isValidating}
          >
            {isValidating ? 'Validating...' : 'Validate Parameters'}
          </Button>
        </div>
      </Card>
    );
  };

  return (
    <div>
      {renderPresets()}
      
      <Collapse
        defaultActiveKey={['tokens', 'quality']}
        className="mb-4"
        items={[
          {
            key: 'tokens',
            label: 'Token Management',
            children: renderTokenManagement()
          },
          {
            key: 'quality',
            label: 'Quality & Accuracy Controls',
            children: renderQualityControls()
          }
        ]}
      />

      {renderValidationResults()}

      {customParameters.enableAIGeneration && (
        <Alert
          message="AI Generation Enabled"
          description="This configuration will make actual calls to OpenAI and incur costs. Monitor your usage carefully!"
          type="warning"
          showIcon
          icon={<WarningOutlined />}
          className="mb-4"
        />
      )}
    </div>
  );
};

export default AdvancedParameterControls;
