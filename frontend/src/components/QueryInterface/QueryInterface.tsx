import React from 'react';
import { Card, Typography } from 'antd';
import { DatabaseConnectionBanner } from '../Layout/DatabaseConnectionBanner';
import { QueryProvider } from './QueryProvider';
import { QueryEditor } from './QueryEditor';
import { QueryTabs } from './QueryTabs';
import { QueryModals } from './QueryModals';
import { ScreenReaderAnnouncer } from '../../hooks/useKeyboardNavigation';

const { Title } = Typography;

interface QueryInterfaceProps {
  className?: string;
}

export const QueryInterface: React.FC<QueryInterfaceProps> = ({ className }) => {
  return (
    <QueryProvider>
      <div className={`query-interface ${className || ''}`}>
        <DatabaseConnectionBanner />
        <Card className="query-card">
          <div className="query-header">
            <Title level={3}>BI Reporting Copilot</Title>
          </div>

          <QueryEditor />
          <QueryTabs />
        </Card>

        <QueryModals />
        <ScreenReaderAnnouncer />
      </div>
    </QueryProvider>
  );
};


