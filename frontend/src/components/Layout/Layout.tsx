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

const { Header, Content } = AntLayout;
const { Text } = Typography;

interface LayoutProps {
  children: React.ReactNode;
}

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { user, logout } = useAuthStore();

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
        background: '#fff',
        padding: '0 24px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between'
      }}>
        <div style={{ display: 'flex', alignItems: 'center' }}>
          <RobotOutlined style={{ fontSize: '24px', color: '#1890ff', marginRight: '12px' }} />
          <Text strong style={{ fontSize: '18px', color: '#1890ff' }}>
            BI Reporting Copilot
          </Text>
        </div>

        <Space>
          <DatabaseStatusIndicator />

          <Button
            type="text"
            icon={<QuestionCircleOutlined />}
            onClick={() => window.open('/docs', '_blank')}
          >
            Help
          </Button>

          <Dropdown
            menu={{ items: userMenuItems }}
            placement="bottomRight"
            trigger={['click']}
          >
            <Button type="text" style={{ display: 'flex', alignItems: 'center' }}>
              <Avatar size="small" icon={<UserOutlined />} style={{ marginRight: '8px' }} />
              <Text>{user?.displayName || 'User'}</Text>
            </Button>
          </Dropdown>
        </Space>
      </Header>

      <Content style={{ padding: '0', background: '#f5f5f5' }}>
        {children}
      </Content>
    </AntLayout>
  );
};
