import React from 'react';
import { DatabaseOutlined } from '@ant-design/icons';
import { SchemaManagementDashboard } from '../components/SchemaManagement/SchemaManagementDashboard';

const SchemaManagementPage: React.FC = () => {
  return (
    <div style={{ padding: '24px' }}>
      <div className="modern-page-header" style={{ marginBottom: '32px', paddingBottom: '24px', borderBottom: '1px solid rgba(0, 0, 0, 0.06)' }}>
        <h1 className="modern-page-title" style={{ fontSize: '2.5rem', fontWeight: 600, margin: 0, marginBottom: '8px', color: '#1a1a1a' }}>
          <DatabaseOutlined style={{ color: '#1890ff', marginRight: '12px' }} />
          Schema Management
        </h1>
        <p className="modern-page-subtitle" style={{ fontSize: '1.125rem', color: '#666', margin: 0, lineHeight: 1.5 }}>
          Manage database schemas, business context, and data relationships
        </p>
      </div>

      <SchemaManagementDashboard />
    </div>
  );
};

export default SchemaManagementPage;
