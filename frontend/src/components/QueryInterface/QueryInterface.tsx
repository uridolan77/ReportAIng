import React from 'react';

import { DatabaseConnectionBanner } from '../Layout/DatabaseConnectionBanner';
import { QueryProvider } from './QueryProvider';
import { MinimalQueryInterface } from './MinimalQueryInterface';
import { QueryModals } from './QueryModals';
import { ScreenReaderAnnouncer } from '../../hooks/useKeyboardNavigation';
// Import new UI components
import { PerformanceMonitor } from '../ui';

interface QueryInterfaceProps {
  className?: string;
}

export const QueryInterface: React.FC<QueryInterfaceProps> = ({ className }) => {
  return (
    <PerformanceMonitor onMetrics={(metrics) => console.log('QueryInterface metrics:', metrics)}>
      <QueryProvider>
        <div className={`query-interface ${className || ''}`}>
          <DatabaseConnectionBanner />

          {/* Minimal, Clean Interface */}
          <MinimalQueryInterface />

          <QueryModals />
          <ScreenReaderAnnouncer />
        </div>
      </QueryProvider>
    </PerformanceMonitor>
  );
};

// Default export for lazy loading
export default QueryInterface;


