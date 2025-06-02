import React, { useState } from 'react';
import {
  Card,
  Input,
  Button,
  Typography,
  Alert,
  Spin,
  Row,
  Col,
  Tag,
  List,
  Progress,
  Space,
  Flex,
} from 'antd';
import {
  SwapOutlined as CompareIcon,
  BulbOutlined as PsychologyIcon,
  RiseOutlined as TrendingUpIcon,
  CheckCircleOutlined as CheckCircleIcon,
  WarningOutlined as WarningIcon,
  CloseCircleOutlined as ErrorIcon,
  BulbOutlined as LightbulbIcon,
} from '@ant-design/icons';
import { ApiService, SimilarityRequest, SimilarityResponse, SimilarQueryResponse } from '../../services/api';

const QuerySimilarityAnalyzer: React.FC = () => {
  const [query1, setQuery1] = useState('');
  const [query2, setQuery2] = useState('');
  const [similarityResult, setSimilarityResult] = useState<SimilarityResponse | null>(null);
  const [similarQueries, setSimilarQueries] = useState<SimilarQueryResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingSimilar, setLoadingSimilar] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCompareSimilarity = async () => {
    if (!query1.trim() || !query2.trim()) {
      setError('Please enter both queries to compare');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const request: SimilarityRequest = {
        query1: query1.trim(),
        query2: query2.trim(),
      };

      const result = await ApiService.calculateSimilarity(request);
      setSimilarityResult(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to calculate similarity');
    } finally {
      setLoading(false);
    }
  };

  const handleFindSimilar = async () => {
    if (!query1.trim()) {
      setError('Please enter a query to find similar ones');
      return;
    }

    setLoadingSimilar(true);
    setError(null);

    try {
      const result = await ApiService.findSimilarQueries(query1.trim(), 5);
      setSimilarQueries(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to find similar queries');
    } finally {
      setLoadingSimilar(false);
    }
  };

  const getSimilarityColor = (score: number) => {
    if (score >= 0.8) return 'success';
    if (score >= 0.6) return 'warning';
    if (score >= 0.4) return 'processing';
    return 'error';
  };

  const getSimilarityIcon = (score: number) => {
    if (score >= 0.8) return <CheckCircleIcon style={{ color: '#52c41a' }} />;
    if (score >= 0.6) return <WarningIcon style={{ color: '#faad14' }} />;
    return <ErrorIcon style={{ color: '#ff4d4f' }} />;
  };

  const getSimilarityDescription = (score: number) => {
    if (score >= 0.9) return 'Nearly Identical';
    if (score >= 0.8) return 'Very Similar';
    if (score >= 0.6) return 'Similar';
    if (score >= 0.4) return 'Somewhat Similar';
    if (score >= 0.2) return 'Slightly Similar';
    return 'Very Different';
  };

  const sampleQueries = [
    "Show me total deposits for yesterday",
    "Top 10 players by deposits in the last 7 days",
    "Show me daily revenue for the last week",
    "Count of active players yesterday",
    "Show me casino vs sports betting revenue for last week",
    "Show me player activity for the last 3 days",
  ];

  return (
    <div>
      <Typography.Title level={3}>
        <CompareIcon style={{ marginRight: 8 }} />
        Query Similarity Analyzer
      </Typography.Title>
      <Typography.Text type="secondary">
        Compare queries semantically and find similar patterns in your query history
      </Typography.Text>

      {/* Query Comparison */}
      <Card style={{ marginBottom: 24 }}>
        <Typography.Title level={4}>
          Compare Two Queries
        </Typography.Title>

        <Row gutter={16}>
          <Col xs={24} md={12}>
            <Input.TextArea
              rows={3}
              placeholder="Enter your first query..."
              value={query1}
              onChange={(e) => setQuery1(e.target.value)}
            />
          </Col>
          <Col xs={24} md={12}>
            <Input.TextArea
              rows={3}
              placeholder="Enter your second query..."
              value={query2}
              onChange={(e) => setQuery2(e.target.value)}
            />
          </Col>
        </Row>

        <Space style={{ marginTop: 16 }}>
          <Button
            type="primary"
            icon={loading ? <Spin size="small" /> : <CompareIcon />}
            onClick={handleCompareSimilarity}
            disabled={loading || !query1.trim() || !query2.trim()}
            loading={loading}
          >
            {loading ? 'Comparing...' : 'Compare Similarity'}
          </Button>

          <Button
            icon={loadingSimilar ? <Spin size="small" /> : <PsychologyIcon />}
            onClick={handleFindSimilar}
            disabled={loadingSimilar || !query1.trim()}
            loading={loadingSimilar}
          >
            {loadingSimilar ? 'Finding...' : 'Find Similar Queries'}
          </Button>
        </Space>
      </Card>

      {/* Sample Queries */}
      <Card style={{ marginBottom: 24 }}>
        <Typography.Title level={4}>
          <LightbulbIcon style={{ marginRight: 8 }} />
          Sample Queries to Try
        </Typography.Title>
        <Space wrap>
          {sampleQueries.map((sample, index) => (
            <Tag
              key={index}
              style={{ cursor: 'pointer' }}
              onClick={() => {
                if (!query1.trim()) {
                  setQuery1(sample);
                } else if (!query2.trim()) {
                  setQuery2(sample);
                } else {
                  setQuery1(sample);
                  setQuery2('');
                }
              }}
            >
              {sample}
            </Tag>
          ))}
        </Space>
      </Card>

      {error && (
        <Alert
          message={typeof error === 'string' ? error : error?.message || 'An error occurred'}
          type="error"
          style={{ marginBottom: 16 }}
          showIcon
        />
      )}

      {/* Similarity Results */}
      {similarityResult && (
        <Card style={{ marginBottom: 24 }}>
          <Typography.Title level={4}>
            Similarity Analysis Results
          </Typography.Title>

          <div style={{ marginBottom: 24 }}>
            <Flex align="center" style={{ marginBottom: 16 }}>
              {getSimilarityIcon(similarityResult.similarityScore)}
              <Typography.Title level={2} style={{ margin: '0 16px' }}>
                {(similarityResult.similarityScore * 100).toFixed(1)}%
              </Typography.Title>
              <Tag color={getSimilarityColor(similarityResult.similarityScore)}>
                {getSimilarityDescription(similarityResult.similarityScore)}
              </Tag>
            </Flex>

            <Progress
              percent={similarityResult.similarityScore * 100}
              status={getSimilarityColor(similarityResult.similarityScore) as any}
              strokeWidth={8}
            />
          </div>

          <Typography.Text>
            <strong>Analysis:</strong> {similarityResult.analysis}
          </Typography.Text>

          <Row gutter={16} style={{ marginTop: 16 }}>
            {similarityResult.commonEntities.length > 0 && (
              <Col xs={24} md={12}>
                <Typography.Title level={5}>
                  Common Entities
                </Typography.Title>
                <Space wrap>
                  {similarityResult.commonEntities.map((entity, index) => (
                    <Tag
                      key={index}
                      color="blue"
                    >
                      {entity}
                    </Tag>
                  ))}
                </Space>
              </Col>
            )}

            {similarityResult.commonKeywords.length > 0 && (
              <Col xs={24} md={12}>
                <Typography.Title level={5}>
                  Common Keywords
                </Typography.Title>
                <Space wrap>
                  {similarityResult.commonKeywords.map((keyword, index) => (
                    <Tag
                      key={index}
                      color="purple"
                    >
                      {keyword}
                    </Tag>
                  ))}
                </Space>
              </Col>
            )}
          </Row>
        </Card>
      )}

      {/* Similar Queries */}
      {similarQueries.length > 0 && (
        <Card>
          <Typography.Title level={4}>
            <TrendingUpIcon style={{ marginRight: 8 }} />
            Similar Queries Found
          </Typography.Title>

          <List
            dataSource={similarQueries}
            renderItem={(similar, index) => (
              <List.Item>
                <List.Item.Meta
                  avatar={<PsychologyIcon style={{ color: '#1890ff' }} />}
                  title={
                    <Flex justify="space-between" align="center">
                      <Typography.Text style={{ flex: 1 }}>
                        {similar.explanation}
                      </Typography.Text>
                      <Space>
                        <Tag color={getSimilarityColor(similar.confidence)}>
                          {(similar.confidence * 100).toFixed(1)}%
                        </Tag>
                        <Tag>
                          {similar.classification}
                        </Tag>
                      </Space>
                    </Flex>
                  }
                  description={
                    <div style={{
                      padding: 8,
                      marginTop: 8,
                      backgroundColor: '#f5f5f5',
                      borderRadius: 4,
                      fontFamily: 'monospace',
                      whiteSpace: 'pre-wrap'
                    }}>
                      {similar.sql}
                    </div>
                  }
                />
              </List.Item>
            )}
          />
        </Card>
      )}

      {/* Empty State */}
      {!similarityResult && similarQueries.length === 0 && !loading && !loadingSimilar && (
        <Card>
          <div style={{ textAlign: 'center', padding: '32px 0' }}>
            <CompareIcon style={{ fontSize: 64, color: '#bfbfbf', marginBottom: 16 }} />
            <Typography.Title level={4}>
              Analyze Query Similarity
            </Typography.Title>
            <Typography.Text type="secondary">
              Enter queries above to compare their semantic similarity or find similar patterns in your history.
            </Typography.Text>
          </div>
        </Card>
      )}
    </div>
  );
};

export default QuerySimilarityAnalyzer;
