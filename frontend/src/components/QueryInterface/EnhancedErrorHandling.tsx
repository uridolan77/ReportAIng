import React, { useState } from 'react';
import {
  Alert,
  Card,
  Button,
  Space,
  Typography,
  Collapse,
  Steps,
  Tag,
  Tooltip,
  Divider,
  List,
  Input,
  Modal
} from 'antd';
import {
  ExclamationCircleOutlined,
  BugOutlined,
  ReloadOutlined,
  EditOutlined,
  BulbOutlined,
  QuestionCircleOutlined,
  RobotOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  WarningOutlined
} from '@ant-design/icons';

const { Title, Text, Paragraph } = Typography;
const { Panel } = Collapse;
const { Step } = Steps;
const { TextArea } = Input;

interface QueryError {
  type: 'syntax' | 'permission' | 'timeout' | 'data' | 'connection' | 'validation';
  message: string;
  details?: string;
  code?: string;
  line?: number;
  column?: number;
  suggestions?: string[];
  fixable?: boolean;
}

interface ErrorRecoveryProps {
  error: QueryError;
  originalQuery: string;
  onRetry: () => void;
  onQueryFix: (fixedQuery: string) => void;
  onGetHelp: () => void;
}

interface QueryValidationResult {
  isValid: boolean;
  errors: QueryError[];
  warnings: string[];
  suggestions: string[];
  confidence: number;
}

export const ErrorRecoveryPanel: React.FC<ErrorRecoveryProps> = ({
  error,
  originalQuery,
  onRetry,
  onQueryFix,
  onGetHelp
}) => {
  const [showFixModal, setShowFixModal] = useState(false);
  const [fixedQuery, setFixedQuery] = useState(originalQuery);
  const [fixingInProgress, setFixingInProgress] = useState(false);

  const getErrorIcon = (type: string) => {
    switch (type) {
      case 'syntax': return <BugOutlined style={{ color: '#ff4d4f' }} />;
      case 'permission': return <ExclamationCircleOutlined style={{ color: '#faad14' }} />;
      case 'timeout': return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
      case 'data': return <WarningOutlined style={{ color: '#faad14' }} />;
      case 'connection': return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      case 'validation': return <QuestionCircleOutlined style={{ color: '#1890ff' }} />;
      default: return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
    }
  };

  const getErrorColor = (type: string) => {
    switch (type) {
      case 'syntax': return 'error';
      case 'permission': return 'warning';
      case 'timeout': return 'error';
      case 'data': return 'warning';
      case 'connection': return 'error';
      case 'validation': return 'info';
      default: return 'error';
    }
  };

  const getRecoverySteps = (errorType: string) => {
    switch (errorType) {
      case 'syntax':
        return [
          { title: 'Check SQL syntax', description: 'Review your query for syntax errors' },
          { title: 'Validate column names', description: 'Ensure all column names exist in the tables' },
          { title: 'Check table references', description: 'Verify table names are correct' },
          { title: 'Test with AI fix', description: 'Let AI suggest corrections' }
        ];
      case 'permission':
        return [
          { title: 'Check access rights', description: 'Verify you have permission to access the data' },
          { title: 'Contact administrator', description: 'Request access to the required tables' },
          { title: 'Use alternative data', description: 'Try querying different tables you have access to' }
        ];
      case 'timeout':
        return [
          { title: 'Simplify query', description: 'Reduce complexity or add filters' },
          { title: 'Add time filters', description: 'Limit the date range to reduce data volume' },
          { title: 'Use sampling', description: 'Query a subset of the data first' },
          { title: 'Optimize joins', description: 'Review and optimize table joins' }
        ];
      default:
        return [
          { title: 'Review error details', description: 'Understand the specific issue' },
          { title: 'Try suggested fixes', description: 'Apply recommended solutions' },
          { title: 'Get help', description: 'Contact support or use AI assistance' }
        ];
    }
  };

  const handleAIFix = async () => {
    setFixingInProgress(true);
    
    // Simulate AI fixing the query
    setTimeout(() => {
      let fixed = originalQuery;
      
      if (error.type === 'syntax') {
        // Example fixes for common syntax errors
        fixed = fixed.replace(/SELCT/gi, 'SELECT');
        fixed = fixed.replace(/FORM/gi, 'FROM');
        fixed = fixed.replace(/WHRE/gi, 'WHERE');
        fixed = fixed.replace(/GROPU BY/gi, 'GROUP BY');
      }
      
      setFixedQuery(fixed);
      setFixingInProgress(false);
      setShowFixModal(true);
    }, 2000);
  };

  const recoverySteps = getRecoverySteps(error.type);

  return (
    <Card
      style={{
        borderRadius: '12px',
        border: `2px solid ${error.type === 'syntax' ? '#ff4d4f' : error.type === 'permission' ? '#faad14' : '#ff4d4f'}`,
        background: 'linear-gradient(135deg, #fff5f5 0%, #fef2f2 100%)'
      }}
    >
      <Alert
        message={
          <Space>
            {getErrorIcon(error.type)}
            <Text strong>Query Error: {error.type.charAt(0).toUpperCase() + error.type.slice(1)}</Text>
          </Space>
        }
        description={error.message}
        type={getErrorColor(error.type) as any}
        showIcon={false}
        style={{ marginBottom: '16px', border: 'none', background: 'transparent' }}
      />

      {error.details && (
        <Collapse ghost style={{ marginBottom: '16px' }}>
          <Panel header="Error Details" key="details">
            <Text code style={{ fontSize: '12px' }}>
              {error.details}
            </Text>
            {error.line && (
              <div style={{ marginTop: '8px' }}>
                <Text type="secondary">
                  Line {error.line}{error.column && `, Column ${error.column}`}
                </Text>
              </div>
            )}
          </Panel>
        </Collapse>
      )}

      {error.suggestions && error.suggestions.length > 0 && (
        <div style={{ marginBottom: '16px' }}>
          <Title level={5}>💡 Suggestions:</Title>
          <List
            size="small"
            dataSource={error.suggestions}
            renderItem={(suggestion, index) => (
              <List.Item>
                <Space>
                  <Tag color="blue">{index + 1}</Tag>
                  <Text>{suggestion}</Text>
                </Space>
              </List.Item>
            )}
          />
        </div>
      )}

      <div style={{ marginBottom: '16px' }}>
        <Title level={5}>🔧 Recovery Steps:</Title>
        <Steps direction="vertical" size="small" current={-1}>
          {recoverySteps.map((step, index) => (
            <Step
              key={index}
              title={step.title}
              description={step.description}
              status="wait"
            />
          ))}
        </Steps>
      </div>

      <Divider />

      <Space wrap>
        <Button
          type="primary"
          icon={<ReloadOutlined />}
          onClick={onRetry}
        >
          Try Again
        </Button>
        
        {error.fixable && (
          <Button
            icon={<RobotOutlined />}
            onClick={handleAIFix}
            loading={fixingInProgress}
          >
            AI Fix
          </Button>
        )}
        
        <Button
          icon={<EditOutlined />}
          onClick={() => setShowFixModal(true)}
        >
          Edit Query
        </Button>
        
        <Button
          icon={<QuestionCircleOutlined />}
          onClick={onGetHelp}
        >
          Get Help
        </Button>
      </Space>

      <Modal
        title="Fix Your Query"
        open={showFixModal}
        onCancel={() => setShowFixModal(false)}
        onOk={() => {
          onQueryFix(fixedQuery);
          setShowFixModal(false);
        }}
        width={700}
      >
        <div style={{ marginBottom: '16px' }}>
          <Text strong>Original Query:</Text>
          <TextArea
            value={originalQuery}
            rows={4}
            style={{ marginTop: '8px', fontFamily: 'monospace' }}
            readOnly
          />
        </div>
        
        <div>
          <Text strong>Fixed Query:</Text>
          <TextArea
            value={fixedQuery}
            onChange={(e) => setFixedQuery(e.target.value)}
            rows={6}
            style={{ marginTop: '8px', fontFamily: 'monospace' }}
          />
        </div>
        
        <Alert
          message="AI Suggestions Applied"
          description="The AI has automatically fixed common syntax errors. Review and modify as needed."
          type="success"
          showIcon
          style={{ marginTop: '16px' }}
        />
      </Modal>
    </Card>
  );
};

export const QueryValidator: React.FC<{
  query: string;
  onValidationResult: (result: QueryValidationResult) => void;
}> = ({ query, onValidationResult }) => {
  const [validating, setValidating] = useState(false);

  const validateQuery = async () => {
    if (!query.trim()) return;
    
    setValidating(true);
    
    // Simulate validation
    setTimeout(() => {
      const errors: QueryError[] = [];
      const warnings: string[] = [];
      const suggestions: string[] = [];
      
      // Basic validation checks
      if (!query.toLowerCase().includes('select')) {
        errors.push({
          type: 'syntax',
          message: 'Query must start with SELECT statement',
          suggestions: ['Add SELECT clause at the beginning']
        });
      }
      
      if (!query.toLowerCase().includes('from')) {
        errors.push({
          type: 'syntax',
          message: 'Missing FROM clause',
          suggestions: ['Add FROM clause to specify table']
        });
      }
      
      if (query.includes('*') && query.toLowerCase().includes('group by')) {
        warnings.push('Using SELECT * with GROUP BY may cause issues');
        suggestions.push('Specify exact columns instead of using *');
      }
      
      if (query.length > 1000) {
        warnings.push('Query is very long and may be complex');
        suggestions.push('Consider breaking into smaller queries');
      }
      
      const result: QueryValidationResult = {
        isValid: errors.length === 0,
        errors,
        warnings,
        suggestions,
        confidence: errors.length === 0 ? (warnings.length === 0 ? 0.95 : 0.8) : 0.3
      };
      
      onValidationResult(result);
      setValidating(false);
    }, 1000);
  };

  React.useEffect(() => {
    if (query.trim()) {
      const debounceTimer = setTimeout(validateQuery, 500);
      return () => clearTimeout(debounceTimer);
    }
  }, [query]);

  return null; // This is a utility component that doesn't render anything
};

export const IterativeQueryBuilder: React.FC<{
  initialQuery: string;
  onQueryUpdate: (query: string) => void;
}> = ({ initialQuery, onQueryUpdate }) => {
  const [queryHistory, setQueryHistory] = useState<string[]>([initialQuery]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [suggestions, setSuggestions] = useState<string[]>([]);

  const addIteration = (newQuery: string) => {
    const newHistory = [...queryHistory.slice(0, currentIndex + 1), newQuery];
    setQueryHistory(newHistory);
    setCurrentIndex(newHistory.length - 1);
    onQueryUpdate(newQuery);
  };

  const goToVersion = (index: number) => {
    setCurrentIndex(index);
    onQueryUpdate(queryHistory[index]);
  };

  const generateSuggestions = () => {
    const currentQuery = queryHistory[currentIndex];
    const newSuggestions = [
      'Add ORDER BY clause for better sorting',
      'Include LIMIT to control result size',
      'Add WHERE clause to filter results',
      'Consider using GROUP BY for aggregation'
    ];
    setSuggestions(newSuggestions);
  };

  return (
    <Card title="Query Iterations" style={{ marginTop: '16px' }}>
      <div style={{ marginBottom: '16px' }}>
        <Text strong>Version History:</Text>
        <div style={{ marginTop: '8px' }}>
          {queryHistory.map((query, index) => (
            <Tag
              key={index}
              color={index === currentIndex ? 'blue' : 'default'}
              style={{ cursor: 'pointer', marginBottom: '4px' }}
              onClick={() => goToVersion(index)}
            >
              Version {index + 1}
            </Tag>
          ))}
        </div>
      </div>
      
      <Button onClick={generateSuggestions} icon={<BulbOutlined />}>
        Get Improvement Suggestions
      </Button>
      
      {suggestions.length > 0 && (
        <div style={{ marginTop: '16px' }}>
          <Text strong>Suggestions:</Text>
          <List
            size="small"
            dataSource={suggestions}
            renderItem={(suggestion) => (
              <List.Item>
                <Text>{suggestion}</Text>
              </List.Item>
            )}
          />
        </div>
      )}
    </Card>
  );
};
