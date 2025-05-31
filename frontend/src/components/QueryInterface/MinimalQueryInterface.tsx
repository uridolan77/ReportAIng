/**
 * Minimal Query Interface - Clean, focused main page
 * Features only the essential query input and immediate results
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Typography,
  Space,
  Button,
  Row,
  Col,
  Divider,
  Empty,
  Tag
} from 'antd';
import {
  HistoryOutlined,
  BookOutlined,
  BarChartOutlined,
  ThunderboltOutlined,
  RocketOutlined
} from '@ant-design/icons';
import { useQueryContext } from './QueryProvider';
import { EnhancedQueryInput } from './EnhancedQueryInput';
import { QueryResult } from './QueryResult';
import { OnboardingTour } from '../Onboarding/OnboardingTour';
import { TipTooltip, ShortcutTooltip, FeatureTooltip } from '../Onboarding/ContextTooltip';

const { Title, Text, Paragraph } = Typography;

export const MinimalQueryInterface: React.FC = () => {
  const {
    query,
    setQuery,
    currentResult,
    isLoading,
    isConnected,
    handleSubmitQuery,
    setShowTemplateLibrary,
    setActiveTab,
    queryHistory
  } = useQueryContext();

  const [showQuickActions, setShowQuickActions] = useState(true);
  const [isFirstVisit, setIsFirstVisit] = useState(false);

  // Check if this is user's first visit
  useEffect(() => {
    const hasVisited = localStorage.getItem('has-visited-app');
    if (!hasVisited) {
      setIsFirstVisit(true);
      localStorage.setItem('has-visited-app', 'true');
    }
  }, []);

  const quickActions = [
    {
      key: 'templates',
      icon: <BookOutlined />,
      title: 'Query Templates',
      description: 'Browse pre-built queries',
      action: () => setShowTemplateLibrary(true),
      color: '#1890ff'
    },
    {
      key: 'history',
      icon: <HistoryOutlined />,
      title: 'Recent Queries',
      description: `${queryHistory.length} saved queries`,
      action: () => setActiveTab('history'),
      color: '#52c41a'
    },
    {
      key: 'examples',
      icon: <RocketOutlined />,
      title: 'Quick Examples',
      description: 'Try sample queries',
      action: () => setQuery('Show me total deposits for yesterday'),
      color: '#722ed1'
    }
  ];

  const exampleQueries = [
    "Show me total deposits for yesterday",
    "Top 10 players by deposits in the last 7 days",
    "Show me daily revenue for the last week",
    "Count of active players yesterday",
    "Show me casino vs sports betting revenue for last week",
    "Revenue breakdown by country for last week"
  ];

  return (
    <div style={{ maxWidth: '1000px', margin: '0 auto', padding: '60px 24px' }}>
      {/* Hero Section */}
      <div style={{ textAlign: 'center', marginBottom: '48px' }}>
        <Title
          level={1}
          style={{
            fontSize: '2.5rem',
            fontWeight: 700,
            margin: '0 0 16px 0',
            color: '#262626',
            lineHeight: '1.2'
          }}
        >
          Ask Your Data Anything
        </Title>
        <Paragraph
          style={{
            fontSize: '16px',
            color: '#595959',
            maxWidth: '500px',
            margin: '0 auto 24px',
            lineHeight: '1.5'
          }}
        >
          Get instant insights from your business data using natural language.
          No SQL knowledge required.
        </Paragraph>

        {!isConnected && (
          <Tag
            color="orange"
            style={{
              fontSize: '13px',
              padding: '4px 12px',
              borderRadius: '12px'
            }}
          >
            Working in offline mode
          </Tag>
        )}
      </div>

      {/* Main Query Input */}
      <div
        style={{
          marginBottom: '40px',
          background: '#ffffff',
          border: '1px solid #e8e8e8',
          borderRadius: '8px',
          padding: '24px',
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)'
        }}
      >
        <EnhancedQueryInput
          value={query}
          onChange={setQuery}
          onSubmit={handleSubmitQuery}
          loading={isLoading}
          placeholder="Ask a question about your data... (e.g., 'Show me revenue by country last month')"
          showShortcuts={false}
          autoHeight={true}
          maxRows={4}
        />
      </div>

      {/* Quick Actions */}
      {showQuickActions && !currentResult && (
        <Row gutter={[16, 16]} style={{ marginBottom: '40px' }}>
          {quickActions.map((action, index) => (
            <Col xs={24} sm={8} key={action.key}>
              <div
                style={{
                  padding: '20px',
                  border: '1px solid #f0f0f0',
                  borderRadius: '8px',
                  background: '#fafafa',
                  cursor: 'pointer',
                  transition: 'all 0.2s ease',
                  textAlign: 'center'
                }}
                onClick={action.action}
                onMouseEnter={(e) => {
                  e.currentTarget.style.borderColor = action.color;
                  e.currentTarget.style.background = '#ffffff';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.borderColor = '#f0f0f0';
                  e.currentTarget.style.background = '#fafafa';
                }}
              >
                <div style={{
                  fontSize: '20px',
                  color: action.color,
                  marginBottom: '12px'
                }}>
                  {action.icon}
                </div>
                <Text strong style={{
                  fontSize: '14px',
                  display: 'block',
                  marginBottom: '4px'
                }}>
                  {action.title}
                </Text>
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  {action.description}
                </Text>
              </div>
            </Col>
          ))}
        </Row>
      )}

      {/* Example Queries */}
      {!currentResult && (
        <div style={{ marginBottom: '40px' }}>
          <div style={{
            marginBottom: '16px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <ThunderboltOutlined style={{ color: '#667eea', fontSize: '16px' }} />
            <Text strong style={{ color: '#262626', fontSize: '16px' }}>
              Try These Examples
            </Text>
          </div>

          <Row gutter={[12, 12]}>
            {exampleQueries.map((example, index) => (
              <Col xs={24} sm={12} lg={8} key={index}>
                <Button
                  type="text"
                  onClick={() => setQuery(example)}
                  style={{
                    width: '100%',
                    textAlign: 'left',
                    height: 'auto',
                    padding: '12px 16px',
                    background: '#f8f9fa',
                    border: '1px solid #e9ecef',
                    borderRadius: '6px',
                    color: '#495057',
                    whiteSpace: 'normal',
                    lineHeight: '1.4',
                    fontSize: '13px',
                    transition: 'all 0.2s ease'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = '#667eea';
                    e.currentTarget.style.color = 'white';
                    e.currentTarget.style.borderColor = '#667eea';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = '#f8f9fa';
                    e.currentTarget.style.color = '#495057';
                    e.currentTarget.style.borderColor = '#e9ecef';
                  }}
                >
                  {example}
                </Button>
              </Col>
            ))}
          </Row>
        </div>
      )}

      {/* Results Section */}
      {currentResult && (
        <div
          style={{
            background: '#ffffff',
            border: '1px solid #e8e8e8',
            borderRadius: '8px',
            padding: '24px',
            boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)'
          }}
        >
          <QueryResult
            result={currentResult}
            query={query}
            onRequery={handleSubmitQuery}
            onSuggestionClick={(suggestion) => {
              setQuery(suggestion);
              handleSubmitQuery();
            }}
          />
        </div>
      )}

      {/* Empty State */}
      {!currentResult && !isLoading && (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <Space direction="vertical">
                <Text type="secondary" style={{ fontSize: '16px' }}>
                  Ready to explore your data
                </Text>
                <Text type="secondary" style={{ fontSize: '14px' }}>
                  Type a question above or try one of the examples
                </Text>
              </Space>
            }
          />
        </div>
      )}

      {/* Onboarding Tour */}
      <OnboardingTour
        isFirstVisit={isFirstVisit}
        onComplete={() => setIsFirstVisit(false)}
      />
    </div>
  );
};
