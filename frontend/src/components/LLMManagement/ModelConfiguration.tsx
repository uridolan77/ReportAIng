/**
 * Model Configuration Component
 * 
 * Manages LLM model configurations including parameters, use cases,
 * cost settings, and model capabilities.
 */

import React from 'react';
import { Card, Alert, Button, Space } from 'antd';
import { SettingOutlined, RobotOutlined } from '@ant-design/icons';

export const ModelConfiguration: React.FC = () => {
  return (
    <div style={{ padding: '24px' }}>
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <RobotOutlined style={{ fontSize: '64px', color: '#1890ff', marginBottom: '16px' }} />
          <h3>Model Configuration</h3>
          <p style={{ color: '#666', marginBottom: '24px' }}>
            Configure model parameters, use cases, and capabilities for each LLM provider.
          </p>
          
          <Alert
            message="Coming Soon"
            description="Model configuration interface is under development. This will include model selection, parameter tuning, use case assignment, and cost configuration."
            type="info"
            showIcon
            style={{ marginBottom: '24px' }}
          />
          
          <Space>
            <Button type="primary" icon={<SettingOutlined />}>
              Configure Models
            </Button>
            <Button icon={<RobotOutlined />}>
              Model Capabilities
            </Button>
          </Space>
        </div>
      </Card>
    </div>
  );
};
