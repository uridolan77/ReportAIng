/**
 * Performance Monitoring Page
 *
 * Real-time system performance monitoring and optimization insights
 */

import React from 'react';
import { DashboardOutlined } from '@ant-design/icons';
import { PerformanceMonitoringDashboard } from '../components/Performance/PerformanceMonitoringDashboard';

const PerformancePage: React.FC = () => {
  return (
    <div style={{ padding: '24px' }}>
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
          <DashboardOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Performance Monitoring
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Real-time system performance and optimization insights
        </p>
      </div>

      <PerformanceMonitoringDashboard />
    </div>
  );
};

export default PerformancePage;
