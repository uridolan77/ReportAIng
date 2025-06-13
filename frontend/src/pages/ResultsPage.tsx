/**
 * Results Page - Dedicated page for viewing query results and basic charts
 */

import React from 'react';
import {
  Empty
} from 'antd';
import {
  BarChartOutlined,
  ArrowLeftOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useCurrentResult } from '../hooks/useCurrentResult';
import { QueryResult } from '../components/QueryInterface/QueryResult';
import { DataInsightsPanel } from '../components/Insights/DataInsightsPanel';
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
      <div style={{ padding: '24px' }}>
        <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
          <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
            <BarChartOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
            Query Results & Analysis
          </h1>
          <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
            Detailed view of your query results with insights and analysis
          </p>
        </div>

        <div style={{ textAlign: 'center', padding: '64px 0', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <div style={{ marginBottom: '24px' }}>
                <div style={{
                  fontSize: '18px',
                  color: '#666',
                  marginBottom: '8px'
                }}>
                  No query results to display
                </div>
                <div style={{
                  fontSize: '16px',
                  color: '#999'
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
                gap: '8px'
              }}
            >
              <ArrowLeftOutlined />
              Go to Query Interface
            </Button>
          </Empty>
        </div>
      </div>
    );
  }

  return (
    <div style={{ padding: '24px' }}>
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
          <BarChartOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Query Results & Analysis
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Detailed view of your query results with insights and analysis
        </p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: '24px', marginBottom: '32px' }}>
        <div style={{ padding: '24px', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
          <h3 style={{ marginBottom: '16px', fontSize: '1.25rem', fontWeight: 600 }}>Query Results</h3>
          <QueryResult
            result={currentResult}
            query={query}
            onRequery={handleRequery}
            onSuggestionClick={(suggestion) => {
              // Handle suggestion click - could navigate back to main page with the suggestion
              navigate('/', { state: { suggestedQuery: suggestion } });
            }}
          />
        </div>

        <div style={{ padding: '24px', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
          <h3 style={{ marginBottom: '16px', fontSize: '1.25rem', fontWeight: 600 }}>Data Insights</h3>
          <p style={{ marginBottom: '16px', color: '#666' }}>AI-generated insights and analysis</p>
          <DataInsightsPanel
            queryResult={currentResult}
            onInsightAction={(action) => {
              console.log('Insight action:', action);
              // Handle insight actions like drill-down, filtering, etc.
            }}
            autoGenerate={true}
          />
        </div>
      </div>

      {/* Quick Actions */}
      <div style={{ padding: '24px', backgroundColor: '#fff', borderRadius: '8px', border: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h3 style={{ marginBottom: '16px', fontSize: '1.25rem', fontWeight: 600 }}>Quick Actions</h3>
        <p style={{ marginBottom: '16px', color: '#666' }}>Navigate to related tools and features</p>
        <div style={{
          display: 'flex',
          gap: '16px',
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
      </div>
    </div>
  );
};

export const ResultsPage: React.FC = () => {
  return <ResultsPageContent />;
};

// Default export for lazy loading
export default ResultsPage;
