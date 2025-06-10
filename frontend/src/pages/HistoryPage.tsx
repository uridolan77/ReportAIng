/**
 * History Page - Dedicated page for browsing and managing query history
 */

import React from 'react';
import {
  Space
} from 'antd';
import {
  HomeOutlined,
  HistoryOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { QueryProvider } from '../components/QueryInterface/QueryProvider';
import { useQueryContext } from '../components/QueryInterface/QueryProvider';
import { QueryHistory } from '../components/QueryInterface/QueryHistory';
// Import new UI components
import {
  Container,
  FlexContainer,
  Stack,
  Breadcrumb,
  BreadcrumbItem,
  PerformanceMonitor,
  VirtualList,
  Card,
  Button
} from '../components/ui';
import type { ButtonProps } from '../components/ui/types';

const HistoryPageContent: React.FC = () => {
  const navigate = useNavigate();
  const { setQuery, queryHistory } = useQueryContext();

  const handleQuerySelect = (selectedQuery: string) => {
    setQuery(selectedQuery);
    navigate('/', { state: { selectedQuery } });
  };

  return (
    <PerformanceMonitor onMetrics={(metrics) => console.log('History page metrics:', metrics)}>
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
                <HistoryOutlined />
                Query History
              </FlexContainer>
            </BreadcrumbItem>
          </Breadcrumb>

          <Stack spacing="var(--space-2)">
            <h2 style={{ margin: 0, color: '#667eea', fontSize: '2rem', fontWeight: 600 }}>
              Query History
            </h2>
            <p style={{ color: '#8c8c8c', fontSize: '16px', margin: 0 }}>
              Browse and reuse your previous queries ({queryHistory.length} saved)
            </p>
          </Stack>

          <Card variant="elevated" padding="medium">
            {queryHistory.length > 100 ? (
              <VirtualList
                items={queryHistory}
                itemHeight={80}
                containerHeight={600}
                renderItem={(item, index) => (
                  <div
                    key={index}
                    style={{
                      padding: '12px 16px',
                      borderBottom: '1px solid #f0f0f0',
                      cursor: 'pointer'
                    }}
                    onClick={() => handleQuerySelect(item.query)}
                  >
                    <div style={{ fontWeight: 600, marginBottom: '4px' }}>
                      {item.query.substring(0, 100)}...
                    </div>
                    <div style={{ fontSize: '12px', color: '#8c8c8c' }}>
                      {item.timestamp}
                    </div>
                  </div>
                )}
                overscan={5}
              />
            ) : (
              <QueryHistory onQuerySelect={handleQuerySelect} />
            )}
          </Card>

          {/* Quick Actions */}
          <Card variant="elevated" padding="medium">
            <Stack spacing="var(--space-4)">
              <h3 style={{ margin: 0, fontWeight: 600 }}>Quick Actions</h3>
              <FlexContainer gap="var(--space-3)" wrap>
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
              </FlexContainer>
            </Stack>
          </Card>
        </Stack>
      </Container>
    </PerformanceMonitor>
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
