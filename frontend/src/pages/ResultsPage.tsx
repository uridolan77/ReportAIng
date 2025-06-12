/**
 * Results Page - Dedicated page for viewing query results and basic charts
 */

import React from 'react';
import {
  Empty,
  Space,
  Button as AntButton
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
import { ModernPageLayout, PageSection, PageGrid } from '../components/core/Layouts';
import { Card, CardContent } from '../components/core/Card';
import { Button } from '../components/core/Button';

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
      <ModernPageLayout
        title="Query Results & Analysis"
        subtitle="Detailed view of your query results with insights and analysis"
        breadcrumb={
          <div style={{ display: 'flex', gap: '8px', alignItems: 'center', marginBottom: '16px' }}>
            <HomeOutlined /> Home / <BarChartOutlined /> Results
          </div>
        }
        className="full-width-content"
      >
        <PageSection background="card" padding="lg">
          <div style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            padding: 'var(--space-16) 0',
            textAlign: 'center'
          }}>
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={
                <div style={{ marginBottom: 'var(--space-6)' }}>
                  <div style={{
                    fontSize: 'var(--text-lg)',
                    color: 'var(--text-secondary)',
                    marginBottom: 'var(--space-2)'
                  }}>
                    No query results to display
                  </div>
                  <div style={{
                    fontSize: 'var(--text-base)',
                    color: 'var(--text-tertiary)'
                  }}>
                    Run a query first to see results here
                  </div>
                </div>
              }
            >
              <Button
                variant="primary"
                size="large"
                onClick={() => navigate('/')}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 'var(--space-2)'
                }}
              >
                <ArrowLeftOutlined />
                Go to Query Interface
              </Button>
            </Empty>
          </div>
        </PageSection>
      </ModernPageLayout>
    );
  }

  return (
    <ModernPageLayout
      title="Query Results & Analysis"
      subtitle="Detailed view of your query results with insights and analysis"
      breadcrumb={
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center', marginBottom: '16px' }}>
          <HomeOutlined /> Home / <BarChartOutlined /> Results
        </div>
      }
      className="full-width-content"
    >
      <PageGrid columns={3} gap="lg">
        <div style={{ gridColumn: 'span 2' }}>
          <PageSection
            title="Query Results"
            background="card"
            padding="lg"
          >
            <QueryResult
              result={currentResult}
              query={query}
              onRequery={handleRequery}
              onSuggestionClick={(suggestion) => {
                // Handle suggestion click - could navigate back to main page with the suggestion
                navigate('/', { state: { suggestedQuery: suggestion } });
              }}
            />
          </PageSection>
        </div>

        <div>
          <PageSection
            title="Data Insights"
            subtitle="AI-generated insights and analysis"
            background="card"
            padding="lg"
            actions={<BarChartOutlined style={{ color: 'var(--color-primary)' }} />}
          >
            <DataInsightsPanel
              queryResult={currentResult}
              onInsightAction={(action) => {
                console.log('Insight action:', action);
                // Handle insight actions like drill-down, filtering, etc.
              }}
              autoGenerate={true}
            />
          </PageSection>
        </div>
      </PageGrid>

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
            onClick={() => navigate('/interactive')}
            disabled={!currentResult}
          >
            AI-Powered Charts
          </Button>
        </div>
      </PageSection>
    </ModernPageLayout>
  );
};

export const ResultsPage: React.FC = () => {
  return <ResultsPageContent />;
};

// Default export for lazy loading
export default ResultsPage;
