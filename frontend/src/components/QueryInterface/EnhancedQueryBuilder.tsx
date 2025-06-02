import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Card,
  Input,
  Button,
  Typography,
  Alert,
  Spin,
  Tag,
  AutoComplete,
  Row,
  Col,
  Collapse,
  Tabs,
  List,
  Tooltip,
  Space,
  // Form and Steps removed - not used in current implementation
  Badge,
  Progress,
  Empty,
  Switch,
} from 'antd';
import {
  SendOutlined as SendIcon,
  ClockCircleOutlined as TimerIcon,
  TableOutlined as TableRowsIcon,
  BulbOutlined as PsychologyIcon,
  AppstoreOutlined as CategoryIcon,
  RiseOutlined as TrendingUpIcon,
  BulbOutlined as LightbulbIcon,
  SwapOutlined as CompareIcon,
  DashboardOutlined as SpeedIcon,
  StarOutlined as StarIcon,
  WarningOutlined as WarningIcon,
  ToolOutlined as WandOutlined,
  CodeOutlined,
  HistoryOutlined,
  BookOutlined,
  RocketOutlined,
  ThunderboltOutlined,
  EyeOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons';
import {
  ApiService,
  EnhancedQueryRequest,
  EnhancedQueryResponse,
  SemanticAnalysisResponse,
  ClassificationResponse,
} from '../../services/api';
import { QueryWizard, QueryBuilderData } from './QueryWizard';
import './EnhancedQueryBuilder.css';

const { Title, Text } = Typography;
const { Panel } = Collapse;

interface QueryTemplate {
  id: string;
  name: string;
  description: string;
  category: string;
  template: string;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  tags: string[];
}

const EnhancedQueryBuilder: React.FC = () => {
  // Core query state
  const [query, setQuery] = useState('');
  const [result, setResult] = useState<EnhancedQueryResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // UI state
  const [activeTab, setActiveTab] = useState('freeform');
  const [builderMode, setBuilderMode] = useState<'guided' | 'advanced'>('guided');
  const [showTemplates, setShowTemplates] = useState(false);

  // Analysis state
  const [suggestions, setSuggestions] = useState<string[]>([]);
  // loadingSuggestions state removed - not used in current implementation
  const [analyzing, setAnalyzing] = useState(false);
  const [semanticAnalysis, setSemanticAnalysis] = useState<SemanticAnalysisResponse | null>(null);
  const [classification, setClassification] = useState<ClassificationResponse | null>(null);

  // Wizard state
  const [wizardData, setWizardData] = useState<QueryBuilderData>({});
  const [queryHistory, setQueryHistory] = useState<string[]>([]);
  const [favorites, setFavorites] = useState<string[]>([]);

  // Real-time validation
  const [queryValidation, setQueryValidation] = useState<{
    isValid: boolean;
    confidence: number;
    suggestions: string[];
    warnings: string[];
  } | null>(null);

  // Enhanced sample queries with AI insights - memoized to prevent re-creation
  const sampleQueries = useMemo(() => [
    "Show me total deposits for yesterday",
    "Top 10 players by deposits in the last 7 days",
    "Show me daily revenue for the last week",
    "Count of active players yesterday",
    "Show me casino vs sports betting revenue for last week",
    "Show me player activity for the last 3 days",
    "Total bets and wins for yesterday",
    "Revenue breakdown by country for last week",
  ], []);

  // Query templates for common use cases
  const queryTemplates = useMemo((): QueryTemplate[] => [
    {
      id: 'revenue-analysis',
      name: 'Revenue Analysis',
      description: 'Analyze revenue trends and patterns',
      category: 'Financial',
      template: 'Show me {metric} revenue for {time_period} grouped by {dimension}',
      difficulty: 'Beginner',
      tags: ['revenue', 'financial', 'trends']
    },
    {
      id: 'player-behavior',
      name: 'Player Behavior Analysis',
      description: 'Understand player gaming patterns and preferences',
      category: 'Analytics',
      template: 'Analyze player {behavior_type} for {segment} in {time_period}',
      difficulty: 'Intermediate',
      tags: ['players', 'behavior', 'segmentation']
    },
    {
      id: 'performance-kpis',
      name: 'Performance KPIs',
      description: 'Track key performance indicators',
      category: 'KPIs',
      template: 'Show me {kpi_metric} performance compared to {comparison_period}',
      difficulty: 'Beginner',
      tags: ['kpi', 'performance', 'comparison']
    },
    {
      id: 'cohort-analysis',
      name: 'Cohort Analysis',
      description: 'Analyze user cohorts and retention',
      category: 'Advanced Analytics',
      template: 'Perform cohort analysis for players registered in {period} tracking {metric}',
      difficulty: 'Advanced',
      tags: ['cohort', 'retention', 'advanced']
    },
    {
      id: 'geographic-analysis',
      name: 'Geographic Analysis',
      description: 'Analyze performance by geographic regions',
      category: 'Geographic',
      template: 'Show me {metric} breakdown by {geographic_level} for {time_period}',
      difficulty: 'Intermediate',
      tags: ['geographic', 'regional', 'location']
    }
  ], []);

  const loadEnhancedSuggestions = useCallback(async () => {
    try {
      // Loading state removed - suggestions load instantly
      const apiSuggestions = await ApiService.getEnhancedQuerySuggestions();
      setSuggestions([...sampleQueries, ...apiSuggestions]);
    } catch (err) {
      console.error('Failed to load enhanced suggestions:', err);
      setSuggestions(sampleQueries);
    }
    // Loading state cleanup removed
  }, [sampleQueries]);

  useEffect(() => {
    loadEnhancedSuggestions();
  }, [loadEnhancedSuggestions]);

  // Real-time query validation
  const validateQuery = useCallback(async (queryText: string) => {
    if (!queryText.trim() || queryText.length < 10) {
      setQueryValidation(null);
      return;
    }

    try {
      // Simulate API call for query validation
      const validation = {
        isValid: queryText.length > 5,
        confidence: Math.min(0.9, queryText.length / 50),
        suggestions: queryText.includes('yesterday') ? ['Consider using specific dates for better performance'] : [],
        warnings: queryText.toLowerCase().includes('select *') ? ['Avoid SELECT * for better performance'] : []
      };
      setQueryValidation(validation);
    } catch (error) {
      console.error('Query validation error:', error);
    }
  }, []);

  // Debounced query validation
  useEffect(() => {
    const timer = setTimeout(() => {
      validateQuery(query);
    }, 500);

    return () => clearTimeout(timer);
  }, [query, validateQuery]);

  // Load query history from localStorage
  useEffect(() => {
    const savedHistory = localStorage.getItem('queryHistory');
    if (savedHistory) {
      setQueryHistory(JSON.parse(savedHistory));
    }

    const savedFavorites = localStorage.getItem('queryFavorites');
    if (savedFavorites) {
      setFavorites(JSON.parse(savedFavorites));
    }
  }, []);

  // Save query to history
  const saveToHistory = useCallback((queryText: string) => {
    const newHistory = [queryText, ...queryHistory.filter(q => q !== queryText)].slice(0, 10);
    setQueryHistory(newHistory);
    localStorage.setItem('queryHistory', JSON.stringify(newHistory));
  }, [queryHistory]);

  // Toggle favorite
  const toggleFavorite = useCallback((queryText: string) => {
    const newFavorites = favorites.includes(queryText)
      ? favorites.filter(q => q !== queryText)
      : [...favorites, queryText];
    setFavorites(newFavorites);
    localStorage.setItem('queryFavorites', JSON.stringify(newFavorites));
  }, [favorites]);

  const handleAnalyzeQuery = async () => {
    if (!query.trim()) return;

    setAnalyzing(true);
    try {
      const [analysisResult, classificationResult] = await Promise.all([
        ApiService.analyzeQuery(query),
        ApiService.classifyQuery(query),
      ]);

      setSemanticAnalysis(analysisResult);
      setClassification(classificationResult);
    } catch (err) {
      console.error('Failed to analyze query:', err);
    } finally {
      setAnalyzing(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!query.trim()) return;

    setLoading(true);
    setError(null);
    setResult(null);

    try {
      // Save to history
      saveToHistory(query);

      const request: EnhancedQueryRequest = {
        query: query,
        executeQuery: true,
        includeAlternatives: true,
        includeSemanticAnalysis: true,
      };

      const queryResult = await ApiService.processEnhancedQuery(request);
      setResult(queryResult);

      if (!queryResult.success) {
        setError(queryResult.errorMessage || 'Query execution failed');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'An error occurred while executing the query');
    } finally {
      setLoading(false);
    }
  };

  // Handle wizard completion
  const handleWizardComplete = useCallback((generatedQuery: string, data: QueryBuilderData) => {
    setQuery(generatedQuery);
    setWizardData(data);
    setActiveTab('freeform');
    // Auto-analyze the generated query
    setTimeout(() => {
      handleAnalyzeQuery();
    }, 500);
  }, []);

  // Handle template selection
  const handleTemplateSelect = useCallback((template: QueryTemplate) => {
    setQuery(template.template);
    setShowTemplates(false);
    setActiveTab('freeform');
  }, []);

  const handleSuggestionSelect = (suggestion: string | null) => {
    if (suggestion) {
      setQuery(suggestion);
    }
  };

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return 'green';
    if (confidence >= 0.6) return 'orange';
    return 'red';
  };

  const getComplexityColor = (complexity: string) => {
    switch (complexity.toLowerCase()) {
      case 'low': return 'green';
      case 'medium': return 'orange';
      case 'high': return 'red';
      default: return 'default';
    }
  };

  const renderSemanticAnalysis = () => {
    if (!semanticAnalysis) return null;

    return (
      <Card style={{ marginBottom: 16 }}>
        <Title level={5}>
          <Space>
            <PsychologyIcon />
            Semantic Analysis
          </Space>
        </Title>

        <Row gutter={16}>
          <Col xs={24} md={12}>
            <Text strong>Intent</Text>
            <div style={{ marginTop: 8 }}>
              <Tag color="blue" icon={<CategoryIcon />}>
                {semanticAnalysis.intent}
              </Tag>
            </div>
          </Col>
          <Col xs={24} md={12}>
            <Text strong>Confidence</Text>
            <div style={{ marginTop: 8 }}>
              <Tag color={getConfidenceColor(semanticAnalysis.confidence)} icon={<SpeedIcon />}>
                {`${(semanticAnalysis.confidence * 100).toFixed(1)}%`}
              </Tag>
            </div>
          </Col>
        </Row>

        {semanticAnalysis.entities.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong>Detected Entities</Text>
            <div style={{ marginTop: 8, display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {semanticAnalysis.entities.map((entity, index) => (
                <Tooltip key={index} title={`Type: ${entity.type}, Confidence: ${(entity.confidence * 100).toFixed(1)}%`}>
                  <Tag color={getConfidenceColor(entity.confidence)}>
                    {entity.text}
                  </Tag>
                </Tooltip>
              ))}
            </div>
          </div>
        )}

        {semanticAnalysis.keywords.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong>Keywords</Text>
            <div style={{ marginTop: 8, display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {semanticAnalysis.keywords.map((keyword, index) => (
                <Tag key={index}>{keyword}</Tag>
              ))}
            </div>
          </div>
        )}
      </Card>
    );
  };

  const renderClassification = () => {
    if (!classification) return null;

    return (
      <Card style={{ marginBottom: 16 }}>
        <Title level={5}>
          <Space>
            <CategoryIcon />
            Query Classification
          </Space>
        </Title>

        <Row gutter={16}>
          <Col xs={24} md={8}>
            <Text strong>Category</Text>
            <div style={{ marginTop: 8 }}>
              <Tag color="blue">{classification.category}</Tag>
            </div>
          </Col>
          <Col xs={24} md={8}>
            <Text strong>Complexity</Text>
            <div style={{ marginTop: 8 }}>
              <Tag color={getComplexityColor(classification.complexity)}>
                {classification.complexity}
              </Tag>
            </div>
          </Col>
          <Col xs={24} md={8}>
            <Text strong>Recommended Visualization</Text>
            <div style={{ marginTop: 8 }}>
              <Tag color="cyan">{classification.recommendedVisualization}</Tag>
            </div>
          </Col>
        </Row>

        {classification.predictedTables.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong>Predicted Tables</Text>
            <div style={{ marginTop: 8, display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {classification.predictedTables.map((table, index) => (
                <Tag key={index}>{table}</Tag>
              ))}
            </div>
          </div>
        )}

        {classification.optimizationSuggestions.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong>
              <Space>
                <LightbulbIcon />
                Optimization Suggestions
              </Space>
            </Text>
            <List
              style={{ marginTop: 8 }}
              size="small"
              dataSource={classification.optimizationSuggestions}
              renderItem={(suggestion, index) => (
                <List.Item key={index}>
                  <Space>
                    <TrendingUpIcon style={{ color: '#1890ff' }} />
                    <Text>{suggestion}</Text>
                  </Space>
                </List.Item>
              )}
            />
          </div>
        )}
      </Card>
    );
  };

  const renderAlternatives = () => {
    if (!result?.alternatives || result.alternatives.length === 0) return null;

    return (
      <Card style={{ marginBottom: 16 }}>
        <Title level={5}>
          <Space>
            <CompareIcon />
            Alternative Queries
          </Space>
        </Title>

        <Collapse>
          {result.alternatives.map((alternative, index) => (
            <Panel
              key={index}
              header={
                <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                  <Text strong>Alternative {index + 1}</Text>
                  <Tag color={getConfidenceColor(alternative.score)}>
                    Score: {(alternative.score * 100).toFixed(1)}%
                  </Tag>
                </Space>
              }
            >
              <div style={{ marginBottom: 16 }}>
                <Text strong>Reasoning:</Text> {alternative.reasoning}
              </div>

              <div style={{
                padding: 16,
                backgroundColor: '#f5f5f5',
                borderRadius: 6,
                marginBottom: 16,
                fontFamily: 'monospace',
                whiteSpace: 'pre-wrap'
              }}>
                {alternative.sql}
              </div>

              <Row gutter={16}>
                <Col xs={24} md={12}>
                  <Text strong style={{ color: '#52c41a' }}>
                    <Space>
                      <StarIcon />
                      Strengths
                    </Space>
                  </Text>
                  <List
                    style={{ marginTop: 8 }}
                    size="small"
                    dataSource={alternative.strengths}
                    renderItem={(strength, idx) => (
                      <List.Item key={idx}>
                        <Text>{strength}</Text>
                      </List.Item>
                    )}
                  />
                </Col>
                <Col xs={24} md={12}>
                  <Text strong style={{ color: '#faad14' }}>
                    <Space>
                      <WarningIcon />
                      Considerations
                    </Space>
                  </Text>
                  <List
                    style={{ marginTop: 8 }}
                    size="small"
                    dataSource={alternative.weaknesses}
                    renderItem={(weakness, idx) => (
                      <List.Item key={idx}>
                        <Text>{weakness}</Text>
                      </List.Item>
                    )}
                  />
                </Col>
              </Row>
            </Panel>
          ))}
        </Collapse>
      </Card>
    );
  };

  return (
    <div className="enhanced-query-builder">
      {/* Header Section */}
      <div className="query-builder-header">
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={2} style={{ margin: 0, color: '#1890ff' }}>
              <RocketOutlined style={{ marginRight: '12px' }} />
              Enhanced AI Query Builder
            </Title>
            <Text type="secondary" style={{ fontSize: '16px' }}>
              Build powerful queries with AI assistance, guided wizards, and smart templates
            </Text>
          </Col>
          <Col>
            <Space>
              <Switch
                checkedChildren="Advanced"
                unCheckedChildren="Guided"
                checked={builderMode === 'advanced'}
                onChange={(checked) => setBuilderMode(checked ? 'advanced' : 'guided')}
              />
              <Button
                icon={<BookOutlined />}
                onClick={() => setShowTemplates(!showTemplates)}
                type={showTemplates ? 'primary' : 'default'}
              >
                Templates
              </Button>
            </Space>
          </Col>
        </Row>
      </div>

      {/* Query Builder Tabs */}
      <Card style={{ marginBottom: '24px' }}>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          size="large"
          className="enhanced-query-builder-tabs"
        >
          <Tabs.TabPane
            tab={
              <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
                <ThunderboltOutlined style={{ fontSize: '18px' }} />
                Smart
                <Badge count="Smart" style={{ backgroundColor: '#52c41a', fontSize: '10px' }} />
              </span>
            }
            key="freeform"
          >
            {/* Freeform Query Builder */}
            <div>
              <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: '16px' }}>
                  <AutoComplete
                    style={{ width: '100%' }}
                    options={[
                      ...suggestions.map(s => ({ value: s, label: s })),
                      ...queryHistory.map(q => ({ value: q, label: `🕒 ${q}` })),
                      ...favorites.map(f => ({ value: f, label: `⭐ ${f}` }))
                    ]}
                    value={query}
                    onSelect={handleSuggestionSelect}
                    onSearch={setQuery}
                    placeholder="Ask a question about your data with AI-powered insights... (e.g., 'Show me revenue trends by country')"
                    size="large"
                  >
                    <Input.TextArea
                      rows={4}
                      value={query}
                      onChange={(e) => setQuery(e.target.value)}
                      style={{
                        fontSize: '16px',
                        borderRadius: '8px',
                        border: queryValidation?.isValid === false ? '2px solid #ff4d4f' : undefined
                      }}
                    />
                  </AutoComplete>
                </div>

                {/* Real-time Query Validation */}
                {queryValidation && (
                  <div style={{ marginBottom: '16px' }}>
                    <Row gutter={16}>
                      <Col span={12}>
                        <Space>
                          {queryValidation.isValid ? (
                            <CheckCircleOutlined style={{ color: '#52c41a' }} />
                          ) : (
                            <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
                          )}
                          <Text strong>
                            Query Confidence: {(queryValidation.confidence * 100).toFixed(0)}%
                          </Text>
                        </Space>
                        <Progress
                          percent={queryValidation.confidence * 100}
                          size="small"
                          status={queryValidation.confidence > 0.7 ? 'success' : 'normal'}
                          showInfo={false}
                        />
                      </Col>
                      <Col span={12}>
                        <Space>
                          <Button
                            icon={<StarIcon />}
                            size="small"
                            type={favorites.includes(query) ? 'primary' : 'default'}
                            onClick={() => toggleFavorite(query)}
                            disabled={!query.trim()}
                          >
                            {favorites.includes(query) ? 'Favorited' : 'Add to Favorites'}
                          </Button>
                        </Space>
                      </Col>
                    </Row>

                    {queryValidation.warnings.length > 0 && (
                      <Alert
                        type="warning"
                        message="Query Optimization Suggestions"
                        description={
                          <ul style={{ margin: 0, paddingLeft: '20px' }}>
                            {queryValidation.warnings.map((warning, idx) => (
                              <li key={idx}>{warning}</li>
                            ))}
                          </ul>
                        }
                        style={{ marginTop: '8px' }}
                        showIcon
                      />
                    )}
                  </div>
                )}

                {/* Action Buttons */}
                <Row gutter={16}>
                  <Col>
                    <Button
                      type="primary"
                      htmlType="submit"
                      icon={loading ? <Spin size="small" /> : <SendIcon />}
                      disabled={loading || !query.trim()}
                      size="large"
                      style={{ minWidth: '160px' }}
                    >
                      {loading ? 'Processing...' : 'Execute Query'}
                    </Button>
                  </Col>
                  <Col>
                    <Button
                      icon={analyzing ? <Spin size="small" /> : <PsychologyIcon />}
                      disabled={analyzing || !query.trim()}
                      onClick={handleAnalyzeQuery}
                      size="large"
                    >
                      {analyzing ? 'Analyzing...' : 'Analyze Query'}
                    </Button>
                  </Col>
                  <Col>
                    <Button
                      icon={<EyeOutlined />}
                      disabled={!query.trim()}
                      size="large"
                    >
                      Preview SQL
                    </Button>
                  </Col>
                </Row>
              </form>
            </div>
          </Tabs.TabPane>

          <Tabs.TabPane
            tab={
              <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
                <WandOutlined style={{ fontSize: '18px' }} />
                Guided Wizard
                <Badge count="Step-by-step" style={{ backgroundColor: '#1890ff', fontSize: '10px' }} />
              </span>
            }
            key="wizard"
          >
            <QueryWizard
              onQueryGenerated={handleWizardComplete}
              onClose={() => setActiveTab('freeform')}
              initialData={wizardData}
            />
          </Tabs.TabPane>

          <Tabs.TabPane
            tab={
              <span style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '15px', fontWeight: '500' }}>
                <HistoryOutlined style={{ fontSize: '18px' }} />
                History & Favorites
              </span>
            }
            key="history"
          >
            <Row gutter={24}>
              <Col span={12}>
                <Card title="Recent Queries" size="small">
                  {queryHistory.length === 0 ? (
                    <Empty description="No query history yet" />
                  ) : (
                    <List
                      size="small"
                      dataSource={queryHistory}
                      renderItem={(item, index) => (
                        <List.Item
                          actions={[
                            <Button
                              type="link"
                              size="small"
                              onClick={() => setQuery(item)}
                            >
                              Use
                            </Button>
                          ]}
                        >
                          <Text ellipsis style={{ maxWidth: '300px' }}>
                            {index + 1}. {item}
                          </Text>
                        </List.Item>
                      )}
                    />
                  )}
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Favorite Queries" size="small">
                  {favorites.length === 0 ? (
                    <Empty description="No favorite queries yet" />
                  ) : (
                    <List
                      size="small"
                      dataSource={favorites}
                      renderItem={(item) => (
                        <List.Item
                          actions={[
                            <Button
                              type="link"
                              size="small"
                              onClick={() => setQuery(item)}
                            >
                              Use
                            </Button>,
                            <Button
                              type="link"
                              size="small"
                              danger
                              onClick={() => toggleFavorite(item)}
                            >
                              Remove
                            </Button>
                          ]}
                        >
                          <Text ellipsis style={{ maxWidth: '300px' }}>
                            ⭐ {item}
                          </Text>
                        </List.Item>
                      )}
                    />
                  )}
                </Card>
              </Col>
            </Row>
          </Tabs.TabPane>
        </Tabs>
      </Card>

      {/* Query Templates Modal */}
      {showTemplates && (
        <Card style={{ marginBottom: '24px' }}>
          <div style={{ marginBottom: '16px' }}>
            <Title level={4}>
              <BookOutlined style={{ marginRight: '8px' }} />
              Query Templates
            </Title>
            <Text type="secondary">
              Choose from pre-built templates to get started quickly
            </Text>
          </div>

          <Row gutter={[16, 16]}>
            {queryTemplates.map((template) => (
              <Col xs={24} sm={12} md={8} key={template.id}>
                <Card
                  hoverable
                  size="small"
                  onClick={() => handleTemplateSelect(template)}
                  style={{ cursor: 'pointer', height: '100%' }}
                >
                  <Card.Meta
                    title={
                      <Space direction="vertical" size="small" style={{ width: '100%' }}>
                        <Text strong>{template.name}</Text>
                        <Space>
                          <Tag color="blue">{template.category}</Tag>
                          <Tag color={
                            template.difficulty === 'Beginner' ? 'green' :
                            template.difficulty === 'Intermediate' ? 'orange' : 'red'
                          }>
                            {template.difficulty}
                          </Tag>
                        </Space>
                      </Space>
                    }
                    description={
                      <Space direction="vertical" size="small" style={{ width: '100%' }}>
                        <Text>{template.description}</Text>
                        <div>
                          {template.tags.map(tag => (
                            <Tag key={tag} size="small">{tag}</Tag>
                          ))}
                        </div>
                        <Text code style={{ fontSize: '12px' }}>
                          {template.template}
                        </Text>
                      </Space>
                    }
                  />
                </Card>
              </Col>
            ))}
          </Row>
        </Card>
      )}

      {/* Error Display */}
      {error && (
        <Alert
          type="error"
          message={typeof error === 'string' ? error : error?.message || 'An error occurred'}
          style={{ marginBottom: '16px' }}
          showIcon
          closable
          onClose={() => setError(null)}
        />
      )}

      {/* Analysis Results */}
      {(semanticAnalysis || classification) && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={4}>
            <PsychologyIcon style={{ marginRight: '8px' }} />
            AI Analysis Results
          </Title>
          <Row gutter={24}>
            <Col span={12}>
              {renderSemanticAnalysis()}
            </Col>
            <Col span={12}>
              {renderClassification()}
            </Col>
          </Row>
        </Card>
      )}

      {/* Enhanced Results */}
      {result && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={4}>
            <CheckCircleOutlined style={{ marginRight: '8px', color: '#52c41a' }} />
            Query Results
          </Title>

          <Tabs defaultActiveKey="results" size="large">
            <Tabs.TabPane
              tab={
                <Space>
                  <TableRowsIcon />
                  Results
                  {result.queryResult && (
                    <Badge
                      count={result.queryResult.rowCount || result.queryResult.data?.length || 0}
                      style={{ backgroundColor: '#52c41a' }}
                    />
                  )}
                </Space>
              }
              key="results"
            >
              {result.queryResult && (
                <div>
                  <Row gutter={16} style={{ marginBottom: '16px' }}>
                    <Col xs={24} sm={8}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <TimerIcon style={{ fontSize: '24px', color: '#1890ff', marginBottom: '8px' }} />
                        <div>
                          <Text strong style={{ fontSize: '18px' }}>
                            {result.queryResult.executionTimeMs}ms
                          </Text>
                          <br />
                          <Text type="secondary">Execution Time</Text>
                        </div>
                      </Card>
                    </Col>
                    <Col xs={24} sm={8}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <TableRowsIcon style={{ fontSize: '24px', color: '#52c41a', marginBottom: '8px' }} />
                        <div>
                          <Text strong style={{ fontSize: '18px' }}>
                            {result.queryResult.rowCount || result.queryResult.data?.length || 0}
                          </Text>
                          <br />
                          <Text type="secondary">Rows Returned</Text>
                        </div>
                      </Card>
                    </Col>
                    <Col xs={24} sm={8}>
                      <Card size="small" style={{ textAlign: 'center' }}>
                        <CategoryIcon style={{ fontSize: '24px', color: '#722ed1', marginBottom: '8px' }} />
                        <div>
                          <Text strong style={{ fontSize: '18px' }}>
                            {result.queryResult.columns?.length || 0}
                          </Text>
                          <br />
                          <Text type="secondary">Columns</Text>
                        </div>
                      </Card>
                    </Col>
                  </Row>

                  {result.processedQuery?.sql && (
                    <Collapse style={{ marginBottom: '16px' }}>
                      <Panel
                        header={
                          <Space>
                            <CodeOutlined />
                            <Text strong>Generated SQL Query</Text>
                            <Tag color={getConfidenceColor(result.processedQuery.confidence)}>
                              {(result.processedQuery.confidence * 100).toFixed(1)}% confidence
                            </Tag>
                          </Space>
                        }
                        key="sql"
                      >
                        <div style={{
                          padding: '16px',
                          backgroundColor: '#f6f8fa',
                          borderRadius: '8px',
                          fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
                          fontSize: '14px',
                          whiteSpace: 'pre-wrap',
                          marginBottom: '16px',
                          border: '1px solid #e1e4e8'
                        }}>
                          {result.processedQuery.sql}
                        </div>
                        {result.processedQuery.explanation && (
                          <div>
                            <Text strong>AI Explanation:</Text>
                            <div style={{
                              marginTop: '8px',
                              padding: '12px',
                              backgroundColor: '#f0f9ff',
                              borderRadius: '6px',
                              border: '1px solid #bae6fd'
                            }}>
                              <Text>{result.processedQuery.explanation}</Text>
                            </div>
                          </div>
                        )}
                      </Panel>
                    </Collapse>
                  )}
                </div>
              )}
            </Tabs.TabPane>

            <Tabs.TabPane
              tab={
                <Space>
                  <PsychologyIcon />
                  AI Insights
                </Space>
              }
              key="insights"
            >
              <Row gutter={24}>
                <Col span={12}>
                  {result.semanticAnalysis && (
                    <Card size="small" title="Semantic Analysis">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div>
                          <Text strong>Intent:</Text> {result.semanticAnalysis.intent}
                        </div>
                        <div>
                          <Text strong>Confidence:</Text>
                          <Progress
                            percent={result.semanticAnalysis.confidence * 100}
                            size="small"
                            status={result.semanticAnalysis.confidence > 0.7 ? 'success' : 'normal'}
                          />
                        </div>
                      </Space>
                    </Card>
                  )}
                </Col>
                <Col span={12}>
                  {result.classification && (
                    <Card size="small" title="Query Classification">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <div>
                          <Text strong>Category:</Text>
                          <Tag color="blue" style={{ marginLeft: '8px' }}>
                            {result.classification.category}
                          </Tag>
                        </div>
                        <div>
                          <Text strong>Complexity:</Text>
                          <Tag
                            color={getComplexityColor(result.classification.complexity)}
                            style={{ marginLeft: '8px' }}
                          >
                            {result.classification.complexity}
                          </Tag>
                        </div>
                        <div>
                          <Text strong>Recommended Visualization:</Text>
                          <Tag color="cyan" style={{ marginLeft: '8px' }}>
                            {result.classification.recommendedVisualization}
                          </Tag>
                        </div>
                      </Space>
                    </Card>
                  )}
                </Col>
              </Row>
            </Tabs.TabPane>

            <Tabs.TabPane
              tab={
                <Space>
                  <CompareIcon />
                  Alternative Queries
                  {result.alternatives && (
                    <Badge count={result.alternatives.length} style={{ backgroundColor: '#fa8c16' }} />
                  )}
                </Space>
              }
              key="alternatives"
            >
              {renderAlternatives()}
            </Tabs.TabPane>
          </Tabs>
        </Card>
      )}
    </div>
  );
};

export default EnhancedQueryBuilder;