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
  Flex,
  Form,
} from 'antd';
import {
  SendOutlined as SendIcon,
  DownOutlined as ExpandMoreIcon,
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
} from '@ant-design/icons';
import {
  ApiService,
  EnhancedQueryRequest,
  EnhancedQueryResponse,
  SemanticAnalysisResponse,
  ClassificationResponse,
} from '../../services/api';

const { Title, Text } = Typography;
const { TabPane } = Tabs;
const { Panel } = Collapse;

const EnhancedQueryBuilder: React.FC = () => {
  const [query, setQuery] = useState('');
  const [result, setResult] = useState<EnhancedQueryResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [loadingSuggestions, setLoadingSuggestions] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  const [analyzing, setAnalyzing] = useState(false);
  const [semanticAnalysis, setSemanticAnalysis] = useState<SemanticAnalysisResponse | null>(null);
  const [classification, setClassification] = useState<ClassificationResponse | null>(null);

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

  const loadEnhancedSuggestions = useCallback(async () => {
    try {
      setLoadingSuggestions(true);
      const apiSuggestions = await ApiService.getEnhancedQuerySuggestions();
      setSuggestions([...sampleQueries, ...apiSuggestions]);
    } catch (err) {
      console.error('Failed to load enhanced suggestions:', err);
      setSuggestions(sampleQueries);
    } finally {
      setLoadingSuggestions(false);
    }
  }, [sampleQueries]);

  useEffect(() => {
    loadEnhancedSuggestions();
  }, [loadEnhancedSuggestions]);

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
    <div>
      <Title level={2}>Enhanced AI Query Builder</Title>
      <Text type="secondary" style={{ display: 'block', marginBottom: 24 }}>
        Ask questions about your data with advanced AI analysis and insights
      </Text>

      <Card style={{ marginBottom: 24 }}>
        <form onSubmit={handleSubmit}>
          <AutoComplete
            style={{ width: '100%', marginBottom: 16 }}
            options={suggestions.map(s => ({ value: s }))}
            value={query}
            onSelect={handleSuggestionSelect}
            onSearch={setQuery}
            placeholder="Ask a question about your data with AI-powered insights... (e.g., 'Analyze customer behavior trends')"
          >
            <Input.TextArea
              rows={3}
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
          </AutoComplete>

          <Space wrap>
            <Button
              type="primary"
              htmlType="submit"
              icon={loading ? <Spin size="small" /> : <SendIcon />}
              disabled={loading || !query.trim()}
              size="large"
            >
              {loading ? 'Processing...' : 'Execute Enhanced Query'}
            </Button>

            <Button
              type="default"
              icon={analyzing ? <Spin size="small" /> : <PsychologyIcon />}
              disabled={analyzing || !query.trim()}
              onClick={handleAnalyzeQuery}
              size="large"
            >
              {analyzing ? 'Analyzing...' : 'Analyze Query'}
            </Button>
          </Space>
        </form>
      </Card>

      {error && (
        <Alert
          type="error"
          message={typeof error === 'string' ? error : error?.message || 'An error occurred'}
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Analysis Results */}
      {(semanticAnalysis || classification) && (
        <div style={{ marginBottom: 24 }}>
          {renderSemanticAnalysis()}
          {renderClassification()}
        </div>
      )}

      {/* Enhanced Results */}
      {result && (
        <div>
          <Tabs activeKey={tabValue.toString()} onChange={(key) => setTabValue(parseInt(key))}>
            <TabPane tab="Results" key="0">
              {result.queryResult && (
                <div>
                  <Row gutter={16} style={{ marginBottom: 16 }}>
                    <Col xs={24} sm={8}>
                      <Tag icon={<TimerIcon />} color="blue">
                        {result.queryResult.executionTimeMs}ms
                      </Tag>
                    </Col>
                    <Col xs={24} sm={8}>
                      <Tag icon={<TableRowsIcon />} color="green">
                        {result.queryResult.rowCount || result.queryResult.data?.length || 0} rows
                      </Tag>
                    </Col>
                    <Col xs={24} sm={8}>
                      <Tag color="cyan">
                        {result.queryResult.columns?.length || 0} columns
                      </Tag>
                    </Col>
                  </Row>

                  {result.processedQuery?.sql && (
                    <Collapse style={{ marginBottom: 16 }}>
                      <Panel
                        header={
                          <Space>
                            <Text strong>Generated SQL</Text>
                            <Tag color={getConfidenceColor(result.processedQuery.confidence)}>
                              {(result.processedQuery.confidence * 100).toFixed(1)}% confidence
                            </Tag>
                          </Space>
                        }
                        key="sql"
                      >
                        <div style={{
                          padding: 16,
                          backgroundColor: '#f5f5f5',
                          borderRadius: 6,
                          fontFamily: 'monospace',
                          whiteSpace: 'pre-wrap',
                          marginBottom: 16
                        }}>
                          {result.processedQuery.sql}
                        </div>
                        {result.processedQuery.explanation && (
                          <div>
                            <Text strong>Explanation</Text>
                            <div style={{ marginTop: 8 }}>
                              <Text>{result.processedQuery.explanation}</Text>
                            </div>
                          </div>
                        )}
                      </Panel>
                    </Collapse>
                  )}
                </div>
              )}
            </TabPane>

            <TabPane tab="AI Insights" key="1">
              {result.semanticAnalysis && (
                <Card style={{ marginBottom: 16 }}>
                  <Title level={5}>Semantic Analysis</Title>
                  <Text>Intent: {result.semanticAnalysis.intent}</Text><br />
                  <Text>Confidence: {(result.semanticAnalysis.confidence * 100).toFixed(1)}%</Text>
                </Card>
              )}

              {result.classification && (
                <Card>
                  <Title level={5}>Classification</Title>
                  <Text>Category: {result.classification.category}</Text><br />
                  <Text>Complexity: {result.classification.complexity}</Text><br />
                  <Text>Recommended Visualization: {result.classification.recommendedVisualization}</Text>
                </Card>
              )}
            </TabPane>

            <TabPane tab="Alternatives" key="2">
              {renderAlternatives()}
            </TabPane>
          </Tabs>
        </div>
      )}
    </div>
  );
};

export default EnhancedQueryBuilder;