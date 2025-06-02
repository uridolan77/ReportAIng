import React from 'react';
import { Typography } from 'antd';
import { DatabaseConnectionBanner } from '../Layout/DatabaseConnectionBanner';
import { QueryProvider } from './QueryProvider';
import { MinimalQueryInterface } from './MinimalQueryInterface';
import { QueryModals } from './QueryModals';
import { ScreenReaderAnnouncer } from '../../hooks/useKeyboardNavigation';

// Title not used in this component

interface QueryInterfaceProps {
  className?: string;
}

export const QueryInterface: React.FC<QueryInterfaceProps> = ({ className }) => {
  return (
    <QueryProvider>
      <div className={`query-interface ${className || ''}`}>
        <DatabaseConnectionBanner />

        {/* Minimal, Clean Interface */}
        <MinimalQueryInterface />

        <QueryModals />
        <ScreenReaderAnnouncer />
      </div>
    </QueryProvider>
  );
};


