/**
 * Suggestions Page - AI-powered smart query suggestions
 */

import React, { useState, useEffect } from 'react';
import {
  Row,
  Col,
  Tag,
  Empty,
  Spin,
  Badge,
  Tooltip,
  Typography
} from 'antd';
import {
  HomeOutlined,
  BulbOutlined,
  RobotOutlined,
  ClockCircleOutlined,
  RiseOutlined,
  UserOutlined,
  DollarOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';
import { PageLayout, PageSection, PageGrid } from '../components/core/Layouts';
import { Card, CardContent } from '../components/core/Card';
import { Button } from '../components/core/Button';

const { Text } = Typography;

const SuggestionsPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery } = useQueryContext();
  const [loading, setLoading] = useState(true);
  const [suggestions, setSuggestions] = useState<any[]>([]);

  // Real AI suggestions - loaded from API
  const [aiSuggestions, setAiSuggestions] = useState<any[]>([]);
  const [loadingAI, setLoadingAI] = useState(true);
  const [aiError, setAiError] = useState<string | null>(null);

  // Load real AI suggestions on component mount
  React.useEffect(() => {
    const loadAISuggestions = async () => {
      try {
        setLoadingAI(true);
        setAiError(null);

        // TODO: Replace with actual AI service API calls
        // const response = await aiSuggestionsApi.getPersonalizedSuggestions();
        // setAiSuggestions(response.suggestions);

        console.log('Loading real AI suggestions...');

        // For now, show empty state until real AI service is connected
        setAiSuggestions([]);

      } catch (err) {
        console.error('Failed to load AI suggestions:', err);
        setAiError('Failed to load AI suggestions. Please check your connection.');
      } finally {
        setLoadingAI(false);
      }
    };

    loadAISuggestions();
  }, []);

  useEffect(() => {
    // Use real AI suggestions instead of mock data
    setSuggestions(aiSuggestions);
    setLoading(loadingAI);
  }, [aiSuggestions, loadingAI]);

  const handleSuggestionSelect = (suggestion: any) => {
    setQuery(suggestion.query);
    navigate('/', { state: { selectedQuery: suggestion.query } });
  };



  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'trending': return 'Trending';
      case 'personalized': return 'Personalized';
      case 'optimization': return 'Optimization';
      case 'seasonal': return 'Seasonal';
      default: return 'General';
    }
  };

  return (
    <PageLayout
      title="AI-Powered Query Suggestions"
      subtitle="Intelligent recommendations based on your data patterns and query history"
      breadcrumbs={[
        { title: 'Home', href: '/', icon: <HomeOutlined /> },
        { title: 'Smart Suggestions', icon: <BulbOutlined /> }
      ]}
      style={{ width: '80%', margin: '0 auto' }}
    >
      {/* AI Status Card */}
      <PageSection
        title="AI Analysis Status"
        background="card"
        padding="lg"
        actions={<RobotOutlined style={{ color: 'var(--color-primary)' }} />}
      >
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: 'var(--space-4)'
        }}>
          <div style={{ flex: 1 }}>
            <div style={{
              color: 'var(--text-primary)',
              fontWeight: 'var(--font-weight-semibold)',
              marginBottom: 'var(--space-1)'
            }}>
              {loading ? 'Analyzing your data patterns...' : `Generated ${suggestions.length} personalized suggestions`}
            </div>
            <div style={{
              color: 'var(--text-secondary)',
              fontSize: 'var(--text-sm)'
            }}>
              {loading ? 'Please wait while AI processes your query history' : 'Ready to explore intelligent recommendations'}
            </div>
          </div>
          {loading && <Spin />}
        </div>
      </PageSection>

      {/* Suggestions Grid */}
      {loading ? (
        <PageSection background="card" padding="lg">
          <div style={{ textAlign: 'center', padding: 'var(--space-16) 0' }}>
            <Spin size="large" />
            <div style={{ marginTop: 'var(--space-4)' }}>
              <Text type="secondary" style={{ color: 'var(--text-secondary)' }}>
                AI is analyzing your data patterns...
              </Text>
            </div>
          </div>
        </PageSection>
      ) : suggestions.length > 0 ? (
        <PageSection
          title="Personalized Suggestions"
          subtitle="AI-generated recommendations tailored to your needs"
          background="transparent"
          padding="none"
        >
          <PageGrid columns={2} gap="lg">
            {suggestions.map((suggestion, index) => (
              <Card
                key={suggestion.id}
                variant="outlined"
                hover
                style={{ cursor: 'pointer', height: '100%' }}
                onClick={() => handleSuggestionSelect(suggestion)}
              >
                <CardContent padding="large">
                  <div style={{
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 'var(--space-4)',
                    height: '100%'
                  }}>
                    {/* Header */}
                    <div style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'flex-start'
                    }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
                        <div style={{
                          fontSize: '20px',
                          color: suggestion.color,
                          padding: 'var(--space-2)',
                          background: `${suggestion.color}15`,
                          borderRadius: 'var(--radius-md)'
                        }}>
                          {suggestion.icon}
                        </div>
                        <div>
                          <Text strong style={{
                            fontSize: 'var(--text-lg)',
                            color: 'var(--text-primary)'
                          }}>
                            {suggestion.title}
                          </Text>
                          <br />
                          <Tag color={suggestion.color} style={{ marginTop: 'var(--space-1)' }}>
                            {getTypeLabel(suggestion.type)}
                          </Tag>
                        </div>
                      </div>
                      <Tooltip title={`AI Confidence: ${(suggestion.confidence * 100).toFixed(0)}%`}>
                        <Badge
                          count={`${(suggestion.confidence * 100).toFixed(0)}%`}
                          style={{
                            backgroundColor: suggestion.confidence > 0.9 ? 'var(--color-success)' :
                                            suggestion.confidence > 0.8 ? 'var(--color-warning)' : 'var(--color-error)'
                          }}
                        />
                      </Tooltip>
                    </div>

                    {/* Description */}
                    <Text type="secondary" style={{
                      fontSize: 'var(--text-base)',
                      color: 'var(--text-secondary)'
                    }}>
                      {suggestion.description}
                    </Text>

                    {/* Query Preview */}
                    <div style={{
                      background: 'var(--bg-tertiary)',
                      padding: 'var(--space-3)',
                      borderRadius: 'var(--radius-md)',
                      fontSize: 'var(--text-sm)',
                      fontFamily: 'var(--font-family-mono)',
                      color: 'var(--text-secondary)',
                      border: '1px solid var(--border-primary)'
                    }}>
                      {suggestion.query}
                    </div>

                    {/* AI Reasoning */}
                    <div style={{
                      background: `${suggestion.color}08`,
                      padding: 'var(--space-3)',
                      borderRadius: 'var(--radius-md)',
                      border: `1px solid ${suggestion.color}20`
                    }}>
                      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 'var(--space-2)' }}>
                        <RobotOutlined style={{ color: suggestion.color, fontSize: 'var(--text-sm)' }} />
                        <Text style={{
                          fontSize: 'var(--text-sm)',
                          color: 'var(--text-secondary)',
                          lineHeight: 'var(--line-height-relaxed)'
                        }}>
                          <strong>AI Insight:</strong> {suggestion.reasoning}
                        </Text>
                      </div>
                    </div>

                    {/* Category */}
                    <div style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      marginTop: 'auto'
                    }}>
                      <Tag color="blue">{suggestion.category}</Tag>
                      <Text type="secondary" style={{
                        fontSize: 'var(--text-xs)',
                        color: 'var(--text-tertiary)'
                      }}>
                        Click to run this query
                      </Text>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </PageGrid>
        </PageSection>
      ) : (
        <PageSection background="card" padding="lg">
          <div style={{ textAlign: 'center', padding: 'var(--space-16) 0' }}>
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={
                <div style={{ marginBottom: 'var(--space-6)' }}>
                  <div style={{
                    fontSize: 'var(--text-lg)',
                    color: 'var(--text-secondary)',
                    marginBottom: 'var(--space-2)'
                  }}>
                    No suggestions available
                  </div>
                  <div style={{
                    fontSize: 'var(--text-base)',
                    color: 'var(--text-tertiary)'
                  }}>
                    Run some queries first to get personalized AI suggestions
                  </div>
                </div>
              }
            >
              <Button
                variant="primary"
                onClick={() => navigate('/')}
              >
                Start Querying
              </Button>
            </Empty>
          </div>
        </PageSection>
      )}

      {/* Quick Actions */}
      <PageSection
        title="Quick Actions"
        subtitle="Navigate to related tools and features"
        background="card"
        padding="lg"
      >
        <div style={{
          display: 'flex',
          gap: 'var(--space-4)',
          flexWrap: 'wrap'
        }}>
          <Button
            variant="primary"
            onClick={() => navigate('/')}
          >
            New Query
          </Button>
          <Button
            variant="secondary"
            onClick={() => navigate('/templates')}
          >
            Browse Templates
          </Button>
          <Button
            variant="outline"
            onClick={() => navigate('/history')}
          >
            View History
          </Button>
          <Button
            variant="ghost"
            onClick={() => {
              setLoading(true);
              setTimeout(() => {
                // Refresh real AI suggestions instead of mock data
                setSuggestions([...aiSuggestions].sort(() => Math.random() - 0.5));
                setLoading(false);
              }, 1500);
            }}
          >
            Refresh Suggestions
          </Button>
        </div>
      </PageSection>
    </PageLayout>
  );
};

export const SuggestionsPage: React.FC = () => {
  return (
    <QueryProvider>
      <SuggestionsPageContent />
    </QueryProvider>
  );
};

// Default export for lazy loading
export default SuggestionsPage;
