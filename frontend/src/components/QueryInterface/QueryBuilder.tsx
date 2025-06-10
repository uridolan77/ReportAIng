/**
 * Unified Query Builder Component
 * Consolidates functionality from Enhanced and other query builders
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Button,
  Space,
  Typography,
  Tabs,
  Row,
  Col,
  Select,
  Input,
  Switch,
  Tooltip,
  Alert,
  Collapse,
  Tag,
  Progress,
  Spin
} from 'antd';
import {
  PlayCircleOutlined,
  SaveOutlined,
  ShareAltOutlined,
  HistoryOutlined,
  BulbOutlined,
  CodeOutlined,
  EyeOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  RobotOutlined
} from '@ant-design/icons';
import { QueryWizard } from './QueryWizard';
import { SqlEditor } from './SqlEditor';
import { QueryHistory } from './QueryHistory';
import { QuerySuggestions } from './QuerySuggestions';
import { ProactiveSuggestions } from './ProactiveSuggestions';
import { AIProcessingFeedback } from './AIProcessingFeedback';

const { Title, Text } = Typography;
const { TabPane } = Tabs;
const { Panel } = Collapse;
const { Option } = Select;

interface QueryTemplate {
  id: string;
  name: string;
  description: string;
  category: string;
  template: string;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  tags: string[];
}

interface QueryBuilderProps {
  onExecuteQuery: (query: string) => void;
  onSaveQuery?: (query: string, name: string) => void;
  loading?: boolean;
  initialQuery?: string;
  showWizard?: boolean;
  showTemplates?: boolean;
  showAIAssistance?: boolean;
}

export const QueryBuilder: React.FC<QueryBuilderProps> = ({
  onExecuteQuery,
  onSaveQuery,
  loading = false,
  initialQuery = '',
  showWizard = true,
  showTemplates = true,
  showAIAssistance = true
}) => {
  const [activeTab, setActiveTab] = useState('builder');
  const [query, setQuery] = useState(initialQuery);
  const [selectedTemplate, setSelectedTemplate] = useState<QueryTemplate | null>(null);
  const [aiMode, setAiMode] = useState(true);
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [queryHistory, setQueryHistory] = useState<string[]>([]);
  const [suggestions, setSuggestions] = useState<any[]>([]);
  const [aiProcessing, setAiProcessing] = useState(false);

  // Mock query templates
  const queryTemplates: QueryTemplate[] = [
    {
      id: '1',
      name: 'Player Revenue Analysis',
      description: 'Analyze player revenue patterns and trends',
      category: 'Revenue',
      template: 'SELECT player_id, SUM(deposit_amount) as total_revenue FROM tbl_Daily_actions WHERE action_date >= DATEADD(day, -30, GETDATE()) GROUP BY player_id ORDER BY total_revenue DESC',
      difficulty: 'Beginner',
      tags: ['revenue', 'players', 'analysis']
    },
    {
      id: '2',
      name: 'Country Performance Report',
      description: 'Compare performance metrics across different countries',
      category: 'Geography',
      template: 'SELECT c.country_name, COUNT(DISTINCT da.player_id) as active_players, SUM(da.deposit_amount) as total_deposits FROM tbl_Daily_actions da JOIN tbl_Countries c ON da.country_id = c.country_id GROUP BY c.country_name ORDER BY total_deposits DESC',
      difficulty: 'Intermediate',
      tags: ['countries', 'performance', 'deposits']
    },
    {
      id: '3',
      name: 'Advanced Player Segmentation',
      description: 'Complex player segmentation with multiple criteria',
      category: 'Segmentation',
      template: 'WITH player_metrics AS (SELECT player_id, COUNT(*) as session_count, SUM(deposit_amount) as total_deposits, AVG(session_duration) as avg_session FROM tbl_Daily_actions GROUP BY player_id) SELECT CASE WHEN total_deposits > 1000 THEN \'High Value\' WHEN total_deposits > 100 THEN \'Medium Value\' ELSE \'Low Value\' END as segment, COUNT(*) as player_count FROM player_metrics GROUP BY CASE WHEN total_deposits > 1000 THEN \'High Value\' WHEN total_deposits > 100 THEN \'Medium Value\' ELSE \'Low Value\' END',
      difficulty: 'Advanced',
      tags: ['segmentation', 'advanced', 'cte']
    }
  ];

  useEffect(() => {
    if (initialQuery) {
      setQuery(initialQuery);
    }
  }, [initialQuery]);

  const handleExecuteQuery = useCallback(() => {
    if (query.trim()) {
      setQueryHistory(prev => [query, ...prev.slice(0, 9)]); // Keep last 10 queries
      onExecuteQuery(query);
    }
  }, [query, onExecuteQuery]);

  const handleTemplateSelect = (template: QueryTemplate) => {
    setSelectedTemplate(template);
    setQuery(template.template);
    setActiveTab('sql');
  };

  const handleAIAssist = async () => {
    if (!query.trim()) return;
    
    setAiProcessing(true);
    try {
      // Mock AI processing
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      const mockSuggestions = [
        'Consider adding an index on action_date for better performance',
        'You might want to filter by active players only',
        'Adding NOLOCK hint could improve query speed',
        'Consider using a date range parameter for flexibility'
      ];
      
      setSuggestions(mockSuggestions);
    } catch (error) {
      console.error('AI assistance failed:', error);
    } finally {
      setAiProcessing(false);
    }
  };

  const renderTemplatesTab = () => (
    <div>
      <Row gutter={[16, 16]}>
        {queryTemplates.map(template => (
          <Col xs={24} sm={12} lg={8} key={template.id}>
            <Card
              size="small"
              hoverable
              onClick={() => handleTemplateSelect(template)}
              actions={[
                <Tooltip title="Use Template">
                  <Button type="link" icon={<CodeOutlined />} />
                </Tooltip>,
                <Tooltip title="Preview">
                  <Button type="link" icon={<EyeOutlined />} />
                </Tooltip>
              ]}
            >
              <Card.Meta
                title={template.name}
                description={
                  <Space direction="vertical" size="small" style={{ width: '100%' }}>
                    <Text type="secondary">{template.description}</Text>
                    <Space wrap>
                      <Tag color="blue">{template.category}</Tag>
                      <Tag color={
                        template.difficulty === 'Beginner' ? 'green' :
                        template.difficulty === 'Intermediate' ? 'orange' : 'red'
                      }>
                        {template.difficulty}
                      </Tag>
                    </Space>
                    <Space wrap>
                      {template.tags.map(tag => (
                        <Tag key={tag}>{tag}</Tag>
                      ))}
                    </Space>
                  </Space>
                }
              />
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  );

  const renderWizardTab = () => (
    <QueryWizard
      onQueryGenerated={(generatedQuery) => {
        setQuery(generatedQuery);
        setActiveTab('sql');
      }}
      onClose={() => setActiveTab('sql')}
    />
  );

  const renderSqlTab = () => (
    <div>
      <Space direction="vertical" style={{ width: '100%' }}>
        {showAIAssistance && (
          <Card size="small">
            <Row justify="space-between" align="middle">
              <Col>
                <Space>
                  <Switch
                    checked={aiMode}
                    onChange={setAiMode}
                    checkedChildren="AI"
                    unCheckedChildren="Manual"
                  />
                  <Text>AI-Powered Query Building</Text>
                </Space>
              </Col>
              <Col>
                <Space>
                  <Button
                    icon={<RobotOutlined />}
                    onClick={handleAIAssist}
                    loading={aiProcessing}
                    disabled={!query.trim()}
                  >
                    AI Assist
                  </Button>
                  <Button
                    icon={<BulbOutlined />}
                    onClick={() => setShowAdvanced(!showAdvanced)}
                  >
                    {showAdvanced ? 'Hide' : 'Show'} Advanced
                  </Button>
                </Space>
              </Col>
            </Row>
          </Card>
        )}

        <SqlEditor
          initialSql={query}
          onExecute={(result) => {
            // Handle the result from SQL execution
            console.log('SQL execution result:', result);
          }}
          disabled={loading}
        />

        {aiProcessing && (
          <AIProcessingFeedback
            isProcessing={true}
            currentStep="Analyzing query for optimization suggestions..."
            steps={[
              {
                id: 'analyze',
                title: 'Analyzing Query',
                description: 'Analyzing query for optimization suggestions...',
                status: 'processing',
                timestamp: new Date()
              }
            ]}
          />
        )}

        {suggestions.length > 0 && (
          <Card size="small" title="AI Suggestions">
            <Space direction="vertical" style={{ width: '100%' }}>
              {suggestions.map((suggestion, index) => (
                <Alert
                  key={index}
                  message={suggestion}
                  type="info"
                  showIcon
                  closable
                  onClose={() => setSuggestions(prev => prev.filter((_, i) => i !== index))}
                />
              ))}
            </Space>
          </Card>
        )}

        {showAdvanced && (
          <Collapse>
            <Panel header="Advanced Options" key="advanced">
              <Row gutter={[16, 16]}>
                <Col span={12}>
                  <Text strong>Query Timeout (seconds)</Text>
                  <Select defaultValue="30" style={{ width: '100%', marginTop: 4 }}>
                    <Option value="30">30</Option>
                    <Option value="60">60</Option>
                    <Option value="120">120</Option>
                    <Option value="300">300</Option>
                  </Select>
                </Col>
                <Col span={12}>
                  <Text strong>Result Limit</Text>
                  <Select defaultValue="1000" style={{ width: '100%', marginTop: 4 }}>
                    <Option value="100">100</Option>
                    <Option value="500">500</Option>
                    <Option value="1000">1000</Option>
                    <Option value="5000">5000</Option>
                  </Select>
                </Col>
                <Col span={24}>
                  <Space>
                    <Switch defaultChecked /> Include execution plan
                    <Switch defaultChecked /> Use NOLOCK hints
                    <Switch /> Enable query caching
                  </Space>
                </Col>
              </Row>
            </Panel>
          </Collapse>
        )}
      </Space>
    </div>
  );

  const renderHistoryTab = () => (
    <QueryHistory
      onQuerySelect={(selectedQuery) => {
        setQuery(selectedQuery);
        setActiveTab('sql');
      }}
    />
  );

  return (
    <Card
      title={
        <Space>
          <ThunderboltOutlined />
          Query Builder
          {selectedTemplate && (
            <Tag color="blue">{selectedTemplate.name}</Tag>
          )}
        </Space>
      }
      extra={
        <Space>
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={handleExecuteQuery}
            loading={loading}
            disabled={!query.trim()}
          >
            Execute
          </Button>
          <Button
            icon={<SaveOutlined />}
            onClick={() => onSaveQuery?.(query, 'Saved Query')}
            disabled={!query.trim()}
          >
            Save
          </Button>
          <Button icon={<ShareAltOutlined />}>
            Share
          </Button>
        </Space>
      }
    >
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        {showWizard && (
          <TabPane tab="Wizard" key="wizard">
            {renderWizardTab()}
          </TabPane>
        )}
        
        <TabPane tab="SQL Editor" key="sql">
          {renderSqlTab()}
        </TabPane>
        
        {showTemplates && (
          <TabPane tab="Templates" key="templates">
            {renderTemplatesTab()}
          </TabPane>
        )}
        
        <TabPane tab="History" key="history">
          {renderHistoryTab()}
        </TabPane>
      </Tabs>

      <ProactiveSuggestions
        onQuerySelect={(suggestion) => setQuery(suggestion)}
      />
    </Card>
  );
};
