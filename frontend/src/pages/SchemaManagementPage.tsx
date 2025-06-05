import React from 'react';
import { SchemaManagementDashboard } from '../components/SchemaManagement/SchemaManagementDashboard';

const SchemaManagementPage: React.FC = () => {
  return (
    <div style={{ padding: '24px', background: '#f9fafb', minHeight: '100vh' }}>
      <SchemaManagementDashboard />
    </div>
  );
};

export default SchemaManagementPage;
