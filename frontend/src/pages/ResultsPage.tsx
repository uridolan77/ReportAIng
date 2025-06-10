/**
 * Results Page - Dedicated page for viewing query results and basic charts
 */

import React from 'react';
import {
  Empty,
  Space,
  Row,
  Col
} from 'antd';
import {
  HomeOutlined,
  BarChartOutlined,
  ArrowLeftOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useActiveResult } from '../stores/activeResultStore';
import { useCurrentResult } from '../hooks/useCurrentResult';
import { QueryResult } from '../components/QueryInterface/QueryResult';
import { DataInsightsPanel } from '../components/Insights/DataInsightsPanel';
// Import new UI components
import {
  Container,
  FlexContainer,
  GridContainer,
  Stack,
  Breadcrumb,
  BreadcrumbItem,
  InView,
  PerformanceMonitor,
  Card,
  Button
} from '../components/ui';
import type { ButtonProps } from '../components/ui/types';

const ResultsPageContent: React.FC = () => {
  const navigate = useNavigate();
  // Use global result system for cross-page availability
  const { result: currentResult, query, hasResult } = useCurrentResult('results');

  // Handle requery - navigate back to main page with the current query
  const handleRequery = () => {
    navigate('/', { state: { suggestedQuery: query } });
  };

  if (!hasResult || !currentResult) {
    return (
      <PerformanceMonitor onMetrics={(metrics) => console.log('Results page metrics:', metrics)}>
        <Container size="large">
          <Stack spacing="var(--space-6)">
            <Breadcrumb>
              <BreadcrumbItem>
                <FlexContainer align="center" gap="var(--space-2)">
                  <HomeOutlined />
                  <span
                    onClick={() => navigate('/')}
                    style={{ cursor: 'pointer' }}
                  >
                    Home
                  </span>
                </FlexContainer>
              </BreadcrumbItem>
              <BreadcrumbItem>
                <FlexContainer align="center" gap="var(--space-2)">
                  <BarChartOutlined />
                  Results
                </FlexContainer>
              </BreadcrumbItem>
            </Breadcrumb>

            <Card>
              <FlexContainer
                direction="column"
                align="center"
                justify="center"
                style={{ padding: '60px 0' }}
              >
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description={
                    <Stack spacing="var(--space-2)" style={{ textAlign: 'center' }}>
                      <span style={{ fontSize: '16px', color: '#8c8c8c' }}>
                        No query results to display
                      </span>
                      <span style={{ fontSize: '14px', color: '#8c8c8c' }}>
                        Run a query first to see results here
                      </span>
                    </Stack>
                  }
                >
                  <Button
                    variant="primary"
                    size="large"
                    onClick={() => navigate('/')}
                  >
                    <FlexContainer align="center" gap="var(--space-2)">
                      <ArrowLeftOutlined />
                      Go to Query Interface
                    </FlexContainer>
                  </Button>
                </Empty>
              </FlexContainer>
            </Card>
          </Stack>
        </Container>
      </PerformanceMonitor>
    );
  }

  return (
    <PerformanceMonitor onMetrics={(metrics) => console.log('Results page metrics:', metrics)}>
      <Container size="full">
        <Stack spacing="var(--space-6)">
          <Breadcrumb>
            <BreadcrumbItem>
              <FlexContainer align="center" gap="var(--space-2)">
                <HomeOutlined />
                <span
                  onClick={() => navigate('/')}
                  style={{ cursor: 'pointer' }}
                >
                  Home
                </span>
              </FlexContainer>
            </BreadcrumbItem>
            <BreadcrumbItem>
              <FlexContainer align="center" gap="var(--space-2)">
                <BarChartOutlined />
                Results
              </FlexContainer>
            </BreadcrumbItem>
          </Breadcrumb>

          <Stack spacing="var(--space-2)">
            <h2 style={{ margin: 0, color: '#667eea', fontSize: '2rem', fontWeight: 600 }}>
              Query Results & Analysis
            </h2>
            <p style={{ color: '#8c8c8c', fontSize: '16px', margin: 0 }}>
              Detailed view of your query results with insights and analysis
            </p>
          </Stack>

          <GridContainer columns={3} gap="var(--space-6)" responsive>
            <div style={{ gridColumn: 'span 2' }}>
              <InView threshold={0.3} triggerOnce>
                <QueryResult
                  result={currentResult}
                  query={query}
                  onRequery={handleRequery}
                  onSuggestionClick={(suggestion) => {
                    // Handle suggestion click - could navigate back to main page with the suggestion
                    navigate('/', { state: { suggestedQuery: suggestion } });
                  }}
                />
              </InView>
            </div>

            <div>
              <InView threshold={0.3} triggerOnce>
                <Card

                  style={{ padding: '16px' }}
                >
                  <Stack spacing="var(--space-4)">
                    <FlexContainer align="center" gap="var(--space-2)">
                      <BarChartOutlined style={{ color: '#667eea' }} />
                      <span style={{ color: '#667eea', fontWeight: 600 }}>Data Insights</span>
                    </FlexContainer>
                    <DataInsightsPanel
                      queryResult={currentResult}
                      onInsightAction={(action) => {
                        console.log('Insight action:', action);
                        // Handle insight actions like drill-down, filtering, etc.
                      }}
                      autoGenerate={true}
                    />
                  </Stack>
                </Card>
              </InView>
            </div>
          </GridContainer>

          {/* Quick Actions */}
          <Card

            style={{ padding: '16px' }}
          >
            <Stack spacing="var(--space-4)">
              <h3 style={{ margin: 0, fontWeight: 600 }}>Quick Actions</h3>
              <FlexContainer gap="var(--space-3)">
                <Button
                  variant="outline"
                  onClick={() => navigate('/')}
                >
                  New Query
                </Button>
                <Button
                  variant="secondary"
                  onClick={() => navigate('/dashboard')}
                  disabled={!currentResult}
                >
                  Create Dashboard
                </Button>
                <Button
                  variant="ghost"
                  onClick={() => navigate('/interactive')}
                  disabled={!currentResult}
                >
                  Interactive Visualization
                </Button>
                <Button
                  variant="outline"
                  onClick={() => navigate('/advanced-viz')}
                  disabled={!currentResult}
                >
                  AI-Powered Charts
                </Button>
              </FlexContainer>
            </Stack>
          </Card>
        </Stack>
      </Container>
    </PerformanceMonitor>
  );
};

export const ResultsPage: React.FC = () => {
  return <ResultsPageContent />;
};

// Default export for lazy loading
export default ResultsPage;
