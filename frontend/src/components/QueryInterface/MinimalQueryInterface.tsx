/**
 * Minimal Query Interface - Clean, focused main page
 * Features only the essential query input and immediate results
 */

import React, { useState, useEffect } from 'react';
import {
  Typography,
  Space,
  Button,
  Row,
  Col,
  Empty,
  Tag
} from 'antd';
import {
  HistoryOutlined,
  BookOutlined,
  ThunderboltOutlined,
  RocketOutlined
} from '@ant-design/icons';
import { useLocation } from 'react-router-dom';
import { useQueryContext } from './QueryProvider';
import { EnhancedQueryInput } from './EnhancedQueryInput';
import { QueryResult } from './QueryResult';
import { OnboardingTour } from '../Onboarding/OnboardingTour';

const { Title, Text, Paragraph } = Typography;

export const MinimalQueryInterface: React.FC = () => {
  const location = useLocation();
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

  const [showQuickActions] = useState(true);
  const [isFirstVisit, setIsFirstVisit] = useState(false);

  // Handle navigation state (suggested queries from other pages)
  useEffect(() => {
    const state = location.state as { suggestedQuery?: string } | null;
    if (state?.suggestedQuery) {
      setQuery(state.suggestedQuery);
      // Clear the navigation state to prevent re-setting on subsequent renders
      window.history.replaceState({}, document.title);
    }
  }, [location.state, setQuery]);

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
    <div style={{ maxWidth: '1200px', margin: '0 auto', padding: '40px 24px' }}>
      {/* Hero Section */}
      <div style={{ textAlign: 'center', marginBottom: '56px' }}>
        <Title
          level={1}
          style={{
            fontSize: '3.2rem',
            fontWeight: 800,
            margin: '0 0 20px 0',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            backgroundClip: 'text',
            lineHeight: '1.1'
          }}
        >
          Ask Your Data Anything
        </Title>
        <Paragraph
          style={{
            fontSize: '18px',
            color: '#6b7280',
            maxWidth: '600px',
            margin: '0 auto 28px',
            lineHeight: '1.6',
            fontWeight: 400
          }}
        >
          Get instant insights from your business data using natural language.
          No SQL knowledge required.
        </Paragraph>

        {!isConnected && (
          <Tag
            color="orange"
            style={{
              fontSize: '14px',
              padding: '6px 16px',
              borderRadius: '20px',
              fontWeight: 500
            }}
          >
            Working in offline mode
          </Tag>
        )}
      </div>

      {/* Main Query Input */}
      <div
        style={{
          marginBottom: '48px',
          background: '#ffffff',
          border: '2px solid #f1f5f9',
          borderRadius: '16px',
          padding: '32px',
          boxShadow: '0 4px 20px rgba(0, 0, 0, 0.08)',
          transition: 'all 0.3s ease'
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.borderColor = '#e2e8f0';
          e.currentTarget.style.boxShadow = '0 8px 32px rgba(0, 0, 0, 0.12)';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.borderColor = '#f1f5f9';
          e.currentTarget.style.boxShadow = '0 4px 20px rgba(0, 0, 0, 0.08)';
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
        <div style={{ marginBottom: '56px' }}>
          <div style={{
            textAlign: 'center',
            marginBottom: '32px'
          }}>
            <Text style={{
              fontSize: '20px',
              fontWeight: 600,
              color: '#374151',
              display: 'block',
              marginBottom: '8px'
            }}>
              Quick Actions
            </Text>
            <Text type="secondary" style={{ fontSize: '16px' }}>
              Get started with these common tasks
            </Text>
          </div>

          <Row gutter={[24, 24]} justify="center">
            {quickActions.map((action, index) => (
              <Col xs={24} sm={8} key={action.key}>
                <div
                  style={{
                    padding: '28px 24px',
                    background: 'linear-gradient(135deg, #ffffff 0%, #f8fafc 100%)',
                    borderRadius: '16px',
                    cursor: 'pointer',
                    transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                    textAlign: 'center',
                    border: '1px solid #e2e8f0',
                    position: 'relative',
                    overflow: 'hidden'
                  }}
                  onClick={action.action}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-4px)';
                    e.currentTarget.style.boxShadow = '0 12px 40px rgba(0, 0, 0, 0.15)';
                    e.currentTarget.style.borderColor = action.color;
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = 'none';
                    e.currentTarget.style.borderColor = '#e2e8f0';
                  }}
                >
                  <div style={{
                    fontSize: '28px',
                    color: action.color,
                    marginBottom: '16px'
                  }}>
                    {action.icon}
                  </div>
                  <Text strong style={{
                    fontSize: '16px',
                    display: 'block',
                    marginBottom: '8px',
                    color: '#1f2937'
                  }}>
                    {action.title}
                  </Text>
                  <Text type="secondary" style={{
                    fontSize: '14px',
                    lineHeight: '1.5'
                  }}>
                    {action.description}
                  </Text>
                </div>
              </Col>
            ))}
          </Row>
        </div>
      )}

      {/* Example Queries */}
      {!currentResult && (
        <div style={{ marginBottom: '56px' }}>
          <div style={{
            textAlign: 'center',
            marginBottom: '32px'
          }}>
            <div style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '12px',
              marginBottom: '8px'
            }}>
              <ThunderboltOutlined style={{ color: '#667eea', fontSize: '20px' }} />
              <Text style={{
                fontSize: '20px',
                fontWeight: 600,
                color: '#374151'
              }}>
                Try These Examples
              </Text>
            </div>
            <Text type="secondary" style={{ fontSize: '16px' }}>
              Click any example to get started
            </Text>
          </div>

          <Row gutter={[16, 16]} justify="center">
            {exampleQueries.map((example, index) => (
              <Col xs={24} sm={12} lg={8} key={index}>
                <Button
                  type="text"
                  onClick={() => setQuery(example)}
                  style={{
                    width: '100%',
                    textAlign: 'left',
                    height: 'auto',
                    padding: '20px 24px',
                    background: 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)',
                    border: '1px solid #e2e8f0',
                    borderRadius: '12px',
                    color: '#374151',
                    whiteSpace: 'normal',
                    lineHeight: '1.5',
                    fontSize: '15px',
                    fontWeight: 500,
                    transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                    minHeight: '80px',
                    display: 'flex',
                    alignItems: 'center'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
                    e.currentTarget.style.color = 'white';
                    e.currentTarget.style.borderColor = '#667eea';
                    e.currentTarget.style.transform = 'translateY(-2px)';
                    e.currentTarget.style.boxShadow = '0 8px 25px rgba(102, 126, 234, 0.25)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = 'linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%)';
                    e.currentTarget.style.color = '#374151';
                    e.currentTarget.style.borderColor = '#e2e8f0';
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = 'none';
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
            border: '2px solid #f1f5f9',
            borderRadius: '16px',
            padding: '32px',
            boxShadow: '0 4px 20px rgba(0, 0, 0, 0.08)'
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
        <div style={{ textAlign: 'center', padding: '60px 0' }}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <Space direction="vertical" size="small">
                <Text style={{
                  fontSize: '18px',
                  color: '#6b7280',
                  fontWeight: 500
                }}>
                  Ready to explore your data
                </Text>
                <Text type="secondary" style={{ fontSize: '16px' }}>
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
