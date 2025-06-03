import React, { useState, useEffect } from 'react';
import {
  Modal,
  Steps,
  Card,
  Row,
  Col,
  Button,
  Space,
  Typography,
  Select,
  Input,
  Checkbox,
  Radio,
  Tag,
  Tooltip,
  Progress,
  Alert,
  Divider
} from 'antd';
import {
  RocketOutlined,
  DatabaseOutlined,
  BarChartOutlined,
  FilterOutlined,
  SortAscendingOutlined,
  CheckCircleOutlined,
  BulbOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';

const { Title, Text, Paragraph } = Typography;
const { Step } = Steps;
const { Option } = Select;
const { TextArea } = Input;

interface WizardStep {
  title: string;
  description: string;
  icon: React.ReactNode;
  completed: boolean;
}

interface QueryGoal {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  complexity: 'beginner' | 'intermediate' | 'advanced';
  examples: string[];
}

interface DataSource {
  id: string;
  name: string;
  description: string;
  tables: string[];
  estimatedRows: number;
}

interface GuidedQueryWizardProps {
  visible: boolean;
  onClose: () => void;
  onQueryGenerated: (query: string, metadata: any) => void;
}

export const GuidedQueryWizard: React.FC<GuidedQueryWizardProps> = ({
  visible,
  onClose,
  onQueryGenerated
}) => {
  const [currentStep, setCurrentStep] = useState(0);
  const [selectedGoal, setSelectedGoal] = useState<string>('');
  const [selectedDataSources, setSelectedDataSources] = useState<string[]>([]);
  const [selectedColumns, setSelectedColumns] = useState<string[]>([]);
  const [filters, setFilters] = useState<any[]>([]);
  const [groupBy, setGroupBy] = useState<string[]>([]);
  const [orderBy, setOrderBy] = useState<string>('');
  const [limit, setLimit] = useState<number>(100);
  const [generatedQuery, setGeneratedQuery] = useState<string>('');
  const [generating, setGenerating] = useState(false);

  const queryGoals: QueryGoal[] = [
    {
      id: 'analyze_trends',
      title: 'Analyze Trends',
      description: 'Discover patterns and trends in your data over time',
      icon: <BarChartOutlined style={{ fontSize: '24px', color: '#1890ff' }} />,
      complexity: 'beginner',
      examples: ['Revenue trends by month', 'User growth over time', 'Product performance trends']
    },
    {
      id: 'compare_segments',
      title: 'Compare Segments',
      description: 'Compare different groups or categories in your data',
      icon: <FilterOutlined style={{ fontSize: '24px', color: '#52c41a' }} />,
      complexity: 'intermediate',
      examples: ['Revenue by country', 'Performance by product type', 'User behavior by device']
    },
    {
      id: 'find_top_performers',
      title: 'Find Top Performers',
      description: 'Identify highest or lowest performing items',
      icon: <SortAscendingOutlined style={{ fontSize: '24px', color: '#faad14' }} />,
      complexity: 'beginner',
      examples: ['Top 10 customers', 'Best selling products', 'Most active users']
    },
    {
      id: 'advanced_analytics',
      title: 'Advanced Analytics',
      description: 'Complex calculations and statistical analysis',
      icon: <ThunderboltOutlined style={{ fontSize: '24px', color: '#722ed1' }} />,
      complexity: 'advanced',
      examples: ['Correlation analysis', 'Cohort analysis', 'Predictive modeling']
    }
  ];

  const dataSources: DataSource[] = [
    {
      id: 'users',
      name: 'Users',
      description: 'User accounts and profile information',
      tables: ['users', 'user_profiles'],
      estimatedRows: 50000
    },
    {
      id: 'transactions',
      name: 'Transactions',
      description: 'Financial transactions and payments',
      tables: ['transactions', 'payments', 'deposits'],
      estimatedRows: 500000
    },
    {
      id: 'sessions',
      name: 'Sessions',
      description: 'User sessions and activity logs',
      tables: ['sessions', 'activity_logs'],
      estimatedRows: 1000000
    },
    {
      id: 'products',
      name: 'Products',
      description: 'Product catalog and performance data',
      tables: ['products', 'product_performance'],
      estimatedRows: 10000
    }
  ];

  const steps: WizardStep[] = [
    {
      title: 'Goal',
      description: 'What do you want to discover?',
      icon: <BulbOutlined />,
      completed: !!selectedGoal
    },
    {
      title: 'Data',
      description: 'Select your data sources',
      icon: <DatabaseOutlined />,
      completed: selectedDataSources.length > 0
    },
    {
      title: 'Columns',
      description: 'Choose what to analyze',
      icon: <BarChartOutlined />,
      completed: selectedColumns.length > 0
    },
    {
      title: 'Filters',
      description: 'Refine your data',
      icon: <FilterOutlined />,
      completed: true // Optional step
    },
    {
      title: 'Generate',
      description: 'Create your query',
      icon: <RocketOutlined />,
      completed: !!generatedQuery
    }
  ];

  const generateQuery = async () => {
    setGenerating(true);
    
    // Simulate AI query generation
    setTimeout(() => {
      const goal = queryGoals.find(g => g.id === selectedGoal);
      const sources = dataSources.filter(ds => selectedDataSources.includes(ds.id));
      
      let query = '';
      
      if (selectedGoal === 'analyze_trends') {
        query = `SELECT DATE_TRUNC('month', created_at) as month, COUNT(*) as count
FROM ${sources[0]?.tables[0] || 'users'}
WHERE created_at >= NOW() - INTERVAL '12 months'
GROUP BY month
ORDER BY month`;
      } else if (selectedGoal === 'find_top_performers') {
        query = `SELECT ${selectedColumns[0] || 'name'}, SUM(${selectedColumns[1] || 'amount'}) as total
FROM ${sources[0]?.tables[0] || 'transactions'}
GROUP BY ${selectedColumns[0] || 'name'}
ORDER BY total DESC
LIMIT ${limit}`;
      } else {
        query = `SELECT ${selectedColumns.join(', ')}
FROM ${sources[0]?.tables[0] || 'users'}
${filters.length > 0 ? 'WHERE ' + filters.map(f => `${f.column} ${f.operator} '${f.value}'`).join(' AND ') : ''}
${groupBy.length > 0 ? 'GROUP BY ' + groupBy.join(', ') : ''}
${orderBy ? 'ORDER BY ' + orderBy : ''}
LIMIT ${limit}`;
      }
      
      setGeneratedQuery(query);
      setGenerating(false);
    }, 2000);
  };

  const handleNext = () => {
    if (currentStep < steps.length - 1) {
      setCurrentStep(currentStep + 1);
    }
  };

  const handlePrev = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleFinish = () => {
    onQueryGenerated(generatedQuery, {
      goal: selectedGoal,
      dataSources: selectedDataSources,
      columns: selectedColumns,
      filters,
      groupBy,
      orderBy,
      limit
    });
    onClose();
  };

  const renderStepContent = () => {
    switch (currentStep) {
      case 0:
        return (
          <div>
            <Title level={4} style={{ textAlign: 'center', marginBottom: '24px' }}>
              What would you like to discover?
            </Title>
            <Row gutter={[16, 16]}>
              {queryGoals.map(goal => (
                <Col xs={24} sm={12} key={goal.id}>
                  <Card
                    hoverable
                    className={selectedGoal === goal.id ? 'selected-goal' : ''}
                    onClick={() => setSelectedGoal(goal.id)}
                    style={{
                      border: selectedGoal === goal.id ? '2px solid #1890ff' : '1px solid #f0f0f0',
                      borderRadius: '12px'
                    }}
                  >
                    <div style={{ textAlign: 'center', marginBottom: '16px' }}>
                      {goal.icon}
                    </div>
                    <Title level={5} style={{ textAlign: 'center', marginBottom: '8px' }}>
                      {goal.title}
                    </Title>
                    <Text type="secondary" style={{ display: 'block', textAlign: 'center', marginBottom: '12px' }}>
                      {goal.description}
                    </Text>
                    <div style={{ textAlign: 'center' }}>
                      <Tag color={goal.complexity === 'beginner' ? 'green' : goal.complexity === 'intermediate' ? 'orange' : 'red'}>
                        {goal.complexity}
                      </Tag>
                    </div>
                  </Card>
                </Col>
              ))}
            </Row>
          </div>
        );

      case 1:
        return (
          <div>
            <Title level={4} style={{ textAlign: 'center', marginBottom: '24px' }}>
              Select your data sources
            </Title>
            <Row gutter={[16, 16]}>
              {dataSources.map(source => (
                <Col xs={24} sm={12} key={source.id}>
                  <Card
                    hoverable
                    className={selectedDataSources.includes(source.id) ? 'selected-source' : ''}
                    onClick={() => {
                      if (selectedDataSources.includes(source.id)) {
                        setSelectedDataSources(selectedDataSources.filter(id => id !== source.id));
                      } else {
                        setSelectedDataSources([...selectedDataSources, source.id]);
                      }
                    }}
                    style={{
                      border: selectedDataSources.includes(source.id) ? '2px solid #1890ff' : '1px solid #f0f0f0',
                      borderRadius: '12px'
                    }}
                  >
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                      <div>
                        <Title level={5} style={{ marginBottom: '8px' }}>
                          {source.name}
                        </Title>
                        <Text type="secondary" style={{ display: 'block', marginBottom: '8px' }}>
                          {source.description}
                        </Text>
                        <Text style={{ fontSize: '12px', color: '#666' }}>
                          ~{source.estimatedRows.toLocaleString()} rows
                        </Text>
                      </div>
                      {selectedDataSources.includes(source.id) && (
                        <CheckCircleOutlined style={{ color: '#1890ff', fontSize: '20px' }} />
                      )}
                    </div>
                  </Card>
                </Col>
              ))}
            </Row>
          </div>
        );

      case 2:
        return (
          <div>
            <Title level={4} style={{ textAlign: 'center', marginBottom: '24px' }}>
              Choose columns to analyze
            </Title>
            <Select
              mode="multiple"
              placeholder="Select columns"
              style={{ width: '100%', marginBottom: '16px' }}
              value={selectedColumns}
              onChange={setSelectedColumns}
            >
              <Option value="id">ID</Option>
              <Option value="name">Name</Option>
              <Option value="email">Email</Option>
              <Option value="created_at">Created Date</Option>
              <Option value="amount">Amount</Option>
              <Option value="status">Status</Option>
              <Option value="country">Country</Option>
              <Option value="device_type">Device Type</Option>
            </Select>
            
            <Alert
              message="Tip"
              description="Select the columns that are most relevant to your analysis goal. You can always modify this later."
              type="info"
              showIcon
              style={{ marginTop: '16px' }}
            />
          </div>
        );

      case 3:
        return (
          <div>
            <Title level={4} style={{ textAlign: 'center', marginBottom: '24px' }}>
              Add filters (optional)
            </Title>
            <Card>
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text>Add conditions to filter your data:</Text>
                <Input placeholder="e.g., status = 'active'" />
                <Input placeholder="e.g., created_at > '2023-01-01'" />
                <Button type="dashed" style={{ width: '100%' }}>
                  + Add another filter
                </Button>
              </Space>
            </Card>
          </div>
        );

      case 4:
        return (
          <div>
            <Title level={4} style={{ textAlign: 'center', marginBottom: '24px' }}>
              Generate your query
            </Title>
            {generating ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Progress type="circle" percent={75} />
                <div style={{ marginTop: '16px' }}>
                  <Text>AI is generating your optimized query...</Text>
                </div>
              </div>
            ) : generatedQuery ? (
              <Card>
                <Title level={5}>Generated Query:</Title>
                <TextArea
                  value={generatedQuery}
                  rows={8}
                  style={{ fontFamily: 'monospace', fontSize: '14px' }}
                  readOnly
                />
                <div style={{ marginTop: '16px', textAlign: 'center' }}>
                  <Button type="primary" size="large" onClick={handleFinish}>
                    Use This Query
                  </Button>
                </div>
              </Card>
            ) : (
              <div style={{ textAlign: 'center' }}>
                <Button type="primary" size="large" onClick={generateQuery}>
                  Generate Query with AI
                </Button>
              </div>
            )}
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <Modal
      title={
        <Space>
          <RocketOutlined />
          <span>Guided Query Builder</span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      footer={null}
      width={800}
      style={{ top: 20 }}
    >
      <Steps current={currentStep} style={{ marginBottom: '32px' }}>
        {steps.map((step, index) => (
          <Step
            key={index}
            title={step.title}
            description={step.description}
            icon={step.icon}
            status={step.completed ? 'finish' : index === currentStep ? 'process' : 'wait'}
          />
        ))}
      </Steps>

      <div style={{ minHeight: '400px', marginBottom: '24px' }}>
        {renderStepContent()}
      </div>

      <div style={{ textAlign: 'center' }}>
        <Space>
          {currentStep > 0 && (
            <Button onClick={handlePrev}>
              Previous
            </Button>
          )}
          {currentStep < steps.length - 1 && (
            <Button type="primary" onClick={handleNext} disabled={!steps[currentStep].completed}>
              Next
            </Button>
          )}
        </Space>
      </div>
    </Modal>
  );
};
