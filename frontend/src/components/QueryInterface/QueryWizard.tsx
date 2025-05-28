import React, { useState, useEffect } from 'react';
import { Steps, Card, Button, Space, Typography, Row, Col, message } from 'antd';
import {
  DatabaseOutlined,
  BarChartOutlined,
  FilterOutlined,
  EyeOutlined,
  ArrowLeftOutlined,
  ArrowRightOutlined,
  PlayCircleOutlined
} from '@ant-design/icons';
import { DataSourceSelector } from './WizardSteps/DataSourceSelector';
import { MetricSelector } from './WizardSteps/MetricSelector';
import { FilterBuilder } from './WizardSteps/FilterBuilder';
import { VisualizationPicker } from './WizardSteps/VisualizationPicker';
import { QueryPreview } from './WizardSteps/QueryPreview';

const { Title, Text } = Typography;
const { Step } = Steps;

export interface QueryBuilderData {
  dataSource?: {
    table: string;
    description: string;
  };
  metrics?: {
    selected: string[];
    aggregations: Record<string, string>;
  };
  filters?: {
    conditions: Array<{
      column: string;
      operator: string;
      value: any;
      type: 'and' | 'or';
    }>;
  };
  visualization?: {
    type: string;
    config: Record<string, any>;
  };
  timeRange?: {
    column?: string;
    start?: string;
    end?: string;
    period?: string;
  };
}

interface QueryWizardStep {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  component: React.ComponentType<any>;
  validation?: (data: QueryBuilderData) => { valid: boolean; message?: string };
}

interface QueryWizardProps {
  onQueryGenerated: (query: string, data: QueryBuilderData) => void;
  onClose: () => void;
  initialData?: QueryBuilderData;
}

export const QueryWizard: React.FC<QueryWizardProps> = ({
  onQueryGenerated,
  onClose,
  initialData = {}
}) => {
  const [currentStep, setCurrentStep] = useState(0);
  const [queryData, setQueryData] = useState<QueryBuilderData>(initialData);
  const [generatedQuery, setGeneratedQuery] = useState<string>('');

  const steps: QueryWizardStep[] = [
    {
      id: 'data-source',
      title: 'Data Source',
      description: 'Choose which data you want to analyze',
      icon: <DatabaseOutlined />,
      component: DataSourceSelector,
      validation: (data) => ({
        valid: !!data.dataSource?.table,
        message: 'Please select a data source'
      })
    },
    {
      id: 'metrics',
      title: 'Metrics',
      description: 'Select what you want to measure',
      icon: <BarChartOutlined />,
      component: MetricSelector,
      validation: (data) => ({
        valid: !!data.metrics?.selected && data.metrics.selected.length > 0,
        message: 'Please select at least one metric'
      })
    },
    {
      id: 'filters',
      title: 'Filters',
      description: 'Narrow down your data (optional)',
      icon: <FilterOutlined />,
      component: FilterBuilder,
      validation: () => ({ valid: true }) // Filters are optional
    },
    {
      id: 'visualization',
      title: 'Visualization',
      description: 'Choose how to display your data',
      icon: <EyeOutlined />,
      component: VisualizationPicker,
      validation: (data) => ({
        valid: !!data.visualization?.type,
        message: 'Please select a visualization type'
      })
    }
  ];

  const updateQueryData = (stepData: Partial<QueryBuilderData>) => {
    setQueryData(prev => ({ ...prev, ...stepData }));
  };

  const validateCurrentStep = (): boolean => {
    const currentStepConfig = steps[currentStep];
    if (currentStepConfig.validation) {
      const validation = currentStepConfig.validation(queryData);
      if (!validation.valid) {
        message.warning(validation.message || 'Please complete this step');
        return false;
      }
    }
    return true;
  };

  const handleNext = () => {
    if (validateCurrentStep()) {
      if (currentStep < steps.length - 1) {
        setCurrentStep(currentStep + 1);
      } else {
        handleFinish();
      }
    }
  };

  const handlePrevious = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleFinish = () => {
    if (validateCurrentStep()) {
      const query = generateNaturalLanguageQuery(queryData);
      setGeneratedQuery(query);
      onQueryGenerated(query, queryData);
    }
  };

  const generateNaturalLanguageQuery = (data: QueryBuilderData): string => {
    let query = '';

    // Start with action
    if (data.metrics?.selected && data.metrics.selected.length > 0) {
      const aggregations = data.metrics.aggregations || {};
      const metricParts = data.metrics.selected.map(metric => {
        const agg = aggregations[metric];
        if (agg && agg !== 'none') {
          return `${agg} of ${metric}`;
        }
        return metric;
      });
      query += `Show me ${metricParts.join(', ')}`;
    }

    // Add data source
    if (data.dataSource?.table) {
      query += ` from ${data.dataSource.table}`;
    }

    // Add filters
    if (data.filters?.conditions && data.filters.conditions.length > 0) {
      const filterParts = data.filters.conditions.map(condition => {
        return `${condition.column} ${condition.operator} ${condition.value}`;
      });
      query += ` where ${filterParts.join(' and ')}`;
    }

    // Add time range
    if (data.timeRange?.column && (data.timeRange.start || data.timeRange.period)) {
      if (data.timeRange.period) {
        query += ` for the last ${data.timeRange.period}`;
      } else if (data.timeRange.start && data.timeRange.end) {
        query += ` between ${data.timeRange.start} and ${data.timeRange.end}`;
      }
    }

    // Add visualization preference
    if (data.visualization?.type) {
      query += `. Display as ${data.visualization.type} chart`;
    }

    return query;
  };

  const getCurrentStepComponent = () => {
    const currentStepConfig = steps[currentStep];
    const Component = currentStepConfig.component;
    
    return (
      <Component
        data={queryData}
        onChange={updateQueryData}
        onNext={handleNext}
        onPrevious={handlePrevious}
      />
    );
  };

  const isLastStep = currentStep === steps.length - 1;
  const isFirstStep = currentStep === 0;

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <Card>
        <div style={{ marginBottom: '24px' }}>
          <Title level={3}>Query Builder Wizard</Title>
          <Text type="secondary">
            Build your query step by step with our guided wizard
          </Text>
        </div>

        <Steps current={currentStep} style={{ marginBottom: '32px' }}>
          {steps.map((step, index) => (
            <Step
              key={step.id}
              title={step.title}
              description={step.description}
              icon={step.icon}
              status={
                index < currentStep ? 'finish' :
                index === currentStep ? 'process' : 'wait'
              }
            />
          ))}
        </Steps>

        <Card style={{ minHeight: '400px', marginBottom: '24px' }}>
          {getCurrentStepComponent()}
        </Card>

        <Row justify="space-between" align="middle">
          <Col>
            <Button onClick={onClose}>
              Cancel
            </Button>
          </Col>
          
          <Col>
            <Space>
              <Button
                icon={<ArrowLeftOutlined />}
                onClick={handlePrevious}
                disabled={isFirstStep}
              >
                Previous
              </Button>
              
              <Button
                type="primary"
                icon={isLastStep ? <PlayCircleOutlined /> : <ArrowRightOutlined />}
                onClick={handleNext}
              >
                {isLastStep ? 'Generate Query' : 'Next'}
              </Button>
            </Space>
          </Col>
        </Row>

        {generatedQuery && (
          <Card style={{ marginTop: '16px' }} title="Generated Query">
            <QueryPreview
              query={generatedQuery}
              data={queryData}
              onExecute={() => onQueryGenerated(generatedQuery, queryData)}
              onEdit={() => setGeneratedQuery('')}
            />
          </Card>
        )}
      </Card>
    </div>
  );
};
