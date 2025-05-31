import React from 'react';
import { Layout as AntLayout, Button, Space, Typography, Avatar, Dropdown } from 'antd';
import {
  RobotOutlined,
  LogoutOutlined,
  UserOutlined,
  SettingOutlined,
  QuestionCircleOutlined
} from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';
import { DatabaseStatusIndicator } from './DatabaseStatusIndicator';
import { AppNavigation } from '../Navigation/AppNavigation';

const { Header, Content } = AntLayout;
const { Text } = Typography;

interface LayoutProps {
  children: React.ReactNode;
}

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { user, logout, isAdmin } = useAuthStore();

  const userMenuItems = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: 'Profile',
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: 'Settings',
    },
    {
      type: 'divider' as const,
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Logout',
      onClick: logout,
    },
  ];

  return (
    <AntLayout style={{ minHeight: '100vh' }}>
      <Header style={{
        background: '#ffffff',
        padding: '0 24px',
        boxShadow: '0 1px 4px rgba(0, 0, 0, 0.08)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        borderBottom: '1px solid #f0f0f0',
        height: '64px'
      }}>
        {/* Left side - Menu and Logo */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
          <AppNavigation isAdmin={isAdmin} />

          <div style={{ display: 'flex', alignItems: 'center' }}>
            <RobotOutlined style={{
              fontSize: '24px',
              color: '#667eea',
              marginRight: '12px'
            }} />
            <Text strong style={{
              fontSize: '18px',
              color: '#262626',
              fontWeight: 600
            }}>
              BI Reporting Copilot
            </Text>
          </div>
        </div>

        {/* Right side - Status and User */}
        <Space size="medium">
          <DatabaseStatusIndicator />

          <Button
            type="text"
            icon={<QuestionCircleOutlined />}
            onClick={() => window.open('/docs', '_blank')}
            style={{
              color: '#595959',
              border: '1px solid #d9d9d9',
              borderRadius: '6px',
              height: '32px'
            }}
          >
            Help
          </Button>

          <Dropdown
            menu={{ items: userMenuItems }}
            placement="bottomRight"
            trigger={['click']}
          >
            <Button
              type="text"
              style={{
                display: 'flex',
                alignItems: 'center',
                color: 'white',
                border: '1px solid rgba(255, 255, 255, 0.3)',
                borderRadius: '8px',
                background: 'rgba(255, 255, 255, 0.1)',
                backdropFilter: 'blur(10px)',
                padding: '4px 12px'
              }}
            >
              <Avatar
                size="small"
                icon={<UserOutlined />}
                style={{
                  marginRight: '8px',
                  background: 'rgba(255, 255, 255, 0.2)',
                  border: '1px solid rgba(255, 255, 255, 0.3)'
                }}
              />
              <Text style={{ color: 'white' }}>{user?.displayName || 'User'}</Text>
            </Button>
          </Dropdown>
        </Space>
      </Header>

      <Content style={{
        padding: '0',
        background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
        minHeight: 'calc(100vh - 64px)'
      }}>
        {children}
      </Content>
    </AntLayout>
  );
};
