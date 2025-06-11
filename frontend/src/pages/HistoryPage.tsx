/**
 * History Page - Dedicated page for browsing and managing query history
 */

import React from 'react';
import {
  HomeOutlined,
  HistoryOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';
import { QueryHistory } from '../components/QueryInterface/QueryHistory';
import { PageLayout, PageSection } from '../components/ui/PageLayout';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';

const HistoryPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery, queryHistory } = useQueryContext();

  const handleQuerySelect = (selectedQuery: string) => {
    setQuery(selectedQuery);
    navigate('/', { state: { selectedQuery } });
  };

  return (
    <PageLayout
      title="Query History"
      subtitle={`Browse and reuse your previous queries (${queryHistory.length} saved)`}
      breadcrumbs={[
        { title: 'Home', href: '/', icon: <HomeOutlined /> },
        { title: 'Query History', icon: <HistoryOutlined /> }
      ]}
      maxWidth="lg"
    >
      <PageSection
        title="Recent Queries"
        subtitle="Your most recent query history"
        background="card"
        padding="lg"
      >
        {queryHistory.length > 100 ? (
          <div style={{
            height: '600px',
            overflow: 'auto',
            border: '1px solid var(--border-primary)',
            borderRadius: 'var(--radius-md)'
          }}>
            {queryHistory.map((item, index) => (
              <div
                key={index}
                style={{
                  padding: 'var(--space-4)',
                  borderBottom: '1px solid var(--border-secondary)',
                  cursor: 'pointer',
                  transition: 'background-color var(--transition-fast)'
                }}
                onClick={() => handleQuerySelect(item.query)}
                onMouseEnter={(e) => {
                  e.currentTarget.style.backgroundColor = 'var(--bg-tertiary)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.backgroundColor = 'transparent';
                }}
              >
                <div style={{
                  fontWeight: 'var(--font-weight-semibold)',
                  marginBottom: 'var(--space-1)',
                  color: 'var(--text-primary)'
                }}>
                  {item.query.substring(0, 100)}...
                </div>
                <div style={{
                  fontSize: 'var(--text-sm)',
                  color: 'var(--text-tertiary)'
                }}>
                  {item.timestamp}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <QueryHistory onQuerySelect={handleQuerySelect} />
        )}
      </PageSection>

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
            onClick={() => navigate('/suggestions')}
          >
            Smart Suggestions
          </Button>
        </div>
      </PageSection>
    </PageLayout>
  );
};

export const HistoryPage: React.FC = () => {
  return (
    <QueryProvider>
      <HistoryPageContent />
    </QueryProvider>
  );
};

// Default export for lazy loading
export default HistoryPage;
