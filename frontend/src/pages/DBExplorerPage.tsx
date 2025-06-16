/**
 * Modern Database Explorer Page
 * 
 * Consolidated database exploration interface with schema browsing,
 * table preview, and data exploration capabilities.
 */

import React, { useState, useCallback } from 'react';
import {
  Tabs
} from '../components/core';
import { DatabaseOutlined } from '@ant-design/icons';
import { DBExplorer } from '../components/DBExplorer/DBExplorer';
import { SchemaManagementDashboard } from '../components/SchemaManagement/SchemaManagementDashboard';

const DBExplorerPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('database-explorer');

  const handleTabChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  const tabItems = [
    {
      key: 'database-explorer',
      label: '📊 Database Explorer',
      children: (
        <div style={{ height: 'calc(100vh - 180px)', overflow: 'hidden' }}>
          <DBExplorer />
        </div>
      ),
    },
    {
      key: 'schema-management',
      label: '🛠️ Schema Management',
      children: (
        <div style={{ height: 'calc(100vh - 180px)', overflow: 'auto', padding: '16px' }}>
          <SchemaManagementDashboard />
        </div>
      ),
    },
  ];

  return (
    <div style={{ padding: '16px', height: '100vh', display: 'flex', flexDirection: 'column' }}>
      <div className="modern-page-header" style={{ marginBottom: '16px', paddingBottom: '16px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)', flexShrink: 0 }}>
        <h1 className="modern-page-title" style={{ fontSize: '2rem', fontWeight: 600, margin: 0, marginBottom: '4px', color: '#1a1a1a' }}>
          <DatabaseOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Database Management
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1rem', color: '#666', margin: 0, lineHeight: 1.4 }}>
          Comprehensive database exploration and schema management interface
        </p>
      </div>

      <div style={{ flex: 1, minHeight: 0 }}>
        <Tabs
          variant="line"
          size="large"
          activeKey={activeTab}
          onChange={handleTabChange}
          items={tabItems}
          style={{ height: '100%' }}
        />
      </div>
    </div>
  );
};

export default DBExplorerPage;
