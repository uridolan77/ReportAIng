import React, { useState } from 'react';
import { Card, Form, Input, Button, Typography, message, Space, Collapse } from 'antd';
import { UserOutlined, LockOutlined, SettingOutlined } from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';
import ConnectionStatus from '../Debug/ConnectionStatus';
import DatabaseStatus from '../Debug/DatabaseStatus';
import KeyVaultStatus from '../Debug/KeyVaultStatus';

const { Text } = Typography;

export const Login: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [showSystemStatus, setShowSystemStatus] = useState(false);
  const { login } = useAuthStore();

  const onFinish = async (values: { username: string; password: string }) => {
    setLoading(true);
    try {
      const success = await login(values.username, values.password);
      if (success) {
        message.success('Login successful!');
      } else {
        message.error('Login failed. Please check your credentials.');
      }
    } catch (error) {
      message.error('An error occurred during login.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      {showSystemStatus && (
        <div style={{ maxWidth: '800px', margin: '0 auto', padding: '20px' }}>
          <Collapse
            items={[
              {
                key: 'system-status',
                label: (
                  <Space>
                    <SettingOutlined />
                    <Text strong>System Status & Diagnostics</Text>
                  </Space>
                ),
                children: (
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <ConnectionStatus />
                    <DatabaseStatus />
                    <KeyVaultStatus />
                  </Space>
                ),
              },
            ]}
            defaultActiveKey={['system-status']}
            style={{ marginBottom: 16 }}
          />
        </div>
      )}
      <Card className="login-card">

        <Form
          name="login"
          onFinish={onFinish}
          autoComplete="off"
          size="large"
          className="login-form"
        >
          <div className="login-form-inputs">
            <Form.Item
              name="username"
              rules={[
                {
                  required: true,
                  message: 'Please input your username!',
                },
              ]}
            >
              <Input
                prefix={<UserOutlined />}
                placeholder="Username"
              />
            </Form.Item>

            <Form.Item
              name="password"
              rules={[
                {
                  required: true,
                  message: 'Please input your password!',
                },
              ]}
            >
              <Input.Password
                prefix={<LockOutlined />}
                placeholder="Password"
              />
            </Form.Item>
          </div>

          <Form.Item style={{ margin: 0 }}>
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              className="login-button"
            >
              Sign In
            </Button>
          </Form.Item>
        </Form>

        <div style={{ textAlign: 'center', marginTop: '16px' }}>
          <Space direction="vertical" size="small">
            <Text type="secondary" style={{ fontSize: '12px' }}>
              For development: Use any username and password
            </Text>
            <Button
              type="link"
              size="small"
              icon={<SettingOutlined />}
              onClick={() => setShowSystemStatus(!showSystemStatus)}
              style={{ fontSize: '12px' }}
            >
              {showSystemStatus ? 'Hide System Status' : 'Show System Status'}
            </Button>
          </Space>
        </div>
      </Card>
    </div>
  );
};
