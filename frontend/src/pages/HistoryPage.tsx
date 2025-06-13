/**
 * History Page - Dedicated page for browsing and managing query history
 */

import React from 'react';
import { HistoryOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';
import { QueryHistory } from '../components/QueryInterface/QueryHistory';
import { Button } from '../components/core/Button';

const HistoryPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery, queryHistory } = useQueryContext();

  const handleQuerySelect = (selectedQuery: string) => {
    setQuery(selectedQuery);
    navigate('/', { state: { selectedQuery } });
  };

  return (
    <div style={{ padding: '24px' }}>
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
          <HistoryOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Query History
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Browse and reuse your previous queries ({queryHistory.length} saved)
        </p>
      </div>

      <div style={{ marginBottom: '32px' }}>
        <h3 style={{ marginBottom: '16px', fontSize: '1.25rem', fontWeight: 600 }}>Recent Queries</h3>
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
      </div>

      <div style={{ marginTop: '32px' }}>
        <h3 style={{ marginBottom: '16px', fontSize: '1.25rem', fontWeight: 600 }}>Quick Actions</h3>
        <div style={{
          display: 'flex',
          gap: '16px',
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
      </div>
    </div>
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
