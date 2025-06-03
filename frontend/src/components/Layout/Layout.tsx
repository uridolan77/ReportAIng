import React from 'react';
import { Layout as AntLayout, Button, Typography, Avatar, Dropdown } from 'antd';
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
import './Header.css';

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
      <Header
        className="app-header"
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          height: '64px',
          position: 'sticky',
          top: 0,
          zIndex: 1000
        }}
      >
        {/* Left side - Logo */}
        <div
          className="app-logo-container"
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            minWidth: '280px',
            overflow: 'visible'
          }}
        >
          <div
            className="app-logo-icon"
            style={{
              width: '40px',
              height: '40px',
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 4px 12px rgba(59, 130, 246, 0.3)'
            }}
          >
            <RobotOutlined style={{
              fontSize: '20px',
              color: 'white'
            }} />
          </div>
          <div style={{
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            minWidth: '200px',
            overflow: 'visible'
          }}>
            <Text strong style={{
              fontSize: '18px',
              color: '#1e293b',
              fontWeight: 700,
              fontFamily: "'Poppins', sans-serif",
              lineHeight: 1.2,
              display: 'block',
              whiteSpace: 'nowrap'
            }}>
              BI Reporting Copilot
            </Text>
            <Text style={{
              fontSize: '12px',
              color: '#64748b',
              fontWeight: 500,
              fontFamily: "'Inter', sans-serif",
              lineHeight: 1.2,
              whiteSpace: 'nowrap'
            }}>
              AI-Powered Analytics
            </Text>
          </div>
        </div>

        {/* Right side - Status and User */}
        <div
          className="header-right-section"
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '16px'
          }}
        >
          <DatabaseStatusIndicator />

          <Button
            type="text"
            className="header-button"
            icon={<QuestionCircleOutlined />}
            onClick={() => window.open('/docs', '_blank')}
            style={{
              color: '#64748b',
              border: '1px solid #e2e8f0',
              borderRadius: '10px',
              height: '36px',
              padding: '0 14px',
              fontWeight: 500,
              fontSize: '14px',
              transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
              background: 'rgba(255, 255, 255, 0.8)',
              backdropFilter: 'blur(8px)',
              boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.borderColor = '#3b82f6';
              e.currentTarget.style.color = '#3b82f6';
              e.currentTarget.style.transform = 'translateY(-1px)';
              e.currentTarget.style.boxShadow = '0 4px 12px rgba(59, 130, 246, 0.15)';
              e.currentTarget.style.background = 'rgba(255, 255, 255, 0.95)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.borderColor = '#e2e8f0';
              e.currentTarget.style.color = '#64748b';
              e.currentTarget.style.transform = 'translateY(0)';
              e.currentTarget.style.boxShadow = '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)';
              e.currentTarget.style.background = 'rgba(255, 255, 255, 0.8)';
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
              className="user-dropdown-button"
              style={{
                display: 'flex',
                alignItems: 'center',
                color: '#1e293b',
                border: '1px solid #e2e8f0',
                borderRadius: '10px',
                background: 'rgba(255, 255, 255, 0.8)',
                backdropFilter: 'blur(8px)',
                padding: '6px 12px',
                height: '36px',
                fontWeight: 500,
                fontSize: '14px',
                transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.borderColor = '#3b82f6';
                e.currentTarget.style.transform = 'translateY(-1px)';
                e.currentTarget.style.boxShadow = '0 4px 12px rgba(59, 130, 246, 0.15)';
                e.currentTarget.style.background = 'rgba(255, 255, 255, 0.95)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.borderColor = '#e2e8f0';
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.boxShadow = '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)';
                e.currentTarget.style.background = 'rgba(255, 255, 255, 0.8)';
              }}
            >
              <Avatar
                size={24}
                icon={<UserOutlined />}
                style={{
                  marginRight: '8px',
                  background: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
                  border: 'none',
                  color: 'white',
                  boxShadow: '0 2px 8px rgba(59, 130, 246, 0.3)',
                  fontSize: '12px'
                }}
              />
              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start' }}>
                <Text style={{
                  color: '#1e293b',
                  fontWeight: 600,
                  fontFamily: "'Inter', sans-serif",
                  fontSize: '13px',
                  lineHeight: 1.2
                }}>
                  {user?.displayName || 'System Administrator'}
                </Text>
                <Text style={{
                  color: '#64748b',
                  fontWeight: 400,
                  fontFamily: "'Inter', sans-serif",
                  fontSize: '11px',
                  lineHeight: 1
                }}>
                  {isAdmin ? 'Admin' : 'User'}
                </Text>
              </div>
            </Button>
          </Dropdown>
        </div>
      </Header>

      <AntLayout>
        {/* Sidebar */}
        <AppNavigation isAdmin={isAdmin} />

        {/* Main Content */}
        <Content style={{
          padding: '0',
          background: '#f9fafb',
          minHeight: 'calc(100vh - 64px)',
          overflow: 'auto',
          position: 'relative'
        }}>
          {children}
        </Content>
      </AntLayout>
    </AntLayout>
  );
};
