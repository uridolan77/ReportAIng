/**
 * App Navigation Component
 * Provides organized navigation for all features
 */

import React, { useState } from 'react';
import {
  Menu,
  Drawer,
  Button,
  Space,
  Typography,
  Badge,
  Divider
} from 'antd';
import {
  MenuOutlined,
  HomeOutlined,
  BarChartOutlined,
  HistoryOutlined,
  ThunderboltOutlined,
  DashboardOutlined,
  InteractionOutlined,
  BookOutlined,
  SettingOutlined,
  SafetyOutlined,
  RocketOutlined,
  BulbOutlined,
  CodeOutlined
} from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';

const { Text } = Typography;

interface AppNavigationProps {
  isAdmin?: boolean;
}

export const AppNavigation: React.FC<AppNavigationProps> = ({ isAdmin = false }) => {
  const [drawerVisible, setDrawerVisible] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  // Get query history from localStorage instead of context
  const getQueryHistoryCount = () => {
    try {
      const stored = localStorage.getItem('query-history');
      return stored ? JSON.parse(stored).length : 0;
    } catch {
      return 0;
    }
  };

  // Check if there are current results
  const hasCurrentResult = () => {
    try {
      const stored = localStorage.getItem('current-query-result');
      return !!stored;
    } catch {
      return false;
    }
  };

  const queryHistoryCount = getQueryHistoryCount();
  const currentResult = hasCurrentResult();

  const menuItems = [
    {
      key: 'main',
      label: 'Main',
      type: 'group',
      children: [
        {
          key: '/',
          icon: <HomeOutlined />,
          label: 'Query Interface',
          description: 'Ask questions about your data'
        }
      ]
    },
    {
      key: 'analytics',
      label: 'Analytics & Visualization',
      type: 'group',
      children: [
        {
          key: '/results',
          icon: <BarChartOutlined />,
          label: 'Results & Charts',
          description: 'View query results and basic charts',
          disabled: !currentResult
        },
        {
          key: '/dashboard',
          icon: <DashboardOutlined />,
          label: 'Dashboard View',
          description: 'Interactive dashboard builder',
          disabled: !currentResult
        },
        {
          key: '/interactive',
          icon: <InteractionOutlined />,
          label: 'Interactive Viz',
          description: 'Advanced interactive visualizations',
          disabled: !currentResult
        },
        {
          key: '/advanced-viz',
          icon: <RocketOutlined />,
          label: 'AI-Powered Charts',
          description: 'AI-generated advanced visualizations',
          disabled: !currentResult
        }
      ]
    },
    {
      key: 'tools',
      label: 'Query Tools',
      type: 'group',
      children: [
        {
          key: '/history',
          icon: <HistoryOutlined />,
          label: (
            <Space>
              Query History
              {queryHistoryCount > 0 && (
                <Badge count={queryHistoryCount} style={{ fontSize: '11px' }} />
              )}
            </Space>
          ),
          description: 'Browse and reuse past queries'
        },
        {
          key: '/templates',
          icon: <BookOutlined />,
          label: 'Query Templates',
          description: 'Pre-built query templates'
        },
        {
          key: '/suggestions',
          icon: <BulbOutlined />,
          label: 'Smart Suggestions',
          description: 'AI-powered query suggestions'
        },
        {
          key: '/streaming',
          icon: <ThunderboltOutlined />,
          label: 'Streaming Queries',
          description: 'Real-time data streaming'
        },
        {
          key: '/enhanced-query',
          icon: <CodeOutlined />,
          label: 'Query Builder',
          description: 'Advanced query building tools'
        }
      ]
    }
  ];

  // Add admin items if user is admin
  if (isAdmin) {
    menuItems.push({
      key: 'admin',
      label: 'Administration',
      type: 'group',
      children: [
        {
          key: '/admin/tuning',
          icon: <SettingOutlined />,
          label: 'AI Tuning',
          description: 'Configure AI models and prompts'
        },
        {
          key: '/admin/cache',
          icon: <ThunderboltOutlined />,
          label: 'Cache Manager',
          description: 'Manage query caching'
        },
        {
          key: '/admin/security',
          icon: <SafetyOutlined />,
          label: 'Security Dashboard',
          description: 'Security monitoring and settings'
        }
      ]
    });
  }

  const handleMenuClick = ({ key }: { key: string }) => {
    navigate(key);
    setDrawerVisible(false);
  };

  const getCurrentKey = () => {
    const path = location.pathname;
    // Map current path to menu key
    if (path === '/') return '/';
    if (path.startsWith('/admin/')) return path;
    return path;
  };

  const MenuContent = () => (
    <div style={{ padding: '0 16px' }}>
      <Menu
        mode="inline"
        selectedKeys={[getCurrentKey()]}
        onClick={handleMenuClick}
        style={{
          border: 'none',
          background: 'transparent'
        }}
      >
        {menuItems.map(group => (
          <Menu.ItemGroup
            key={group.key}
            title={
              <Text
                strong
                style={{
                  color: '#8c8c8c',
                  fontSize: '12px',
                  textTransform: 'uppercase',
                  letterSpacing: '0.5px'
                }}
              >
                {group.label}
              </Text>
            }
          >
            {group.children.map(item => (
              <Menu.Item
                key={item.key}
                icon={item.icon}
                disabled={item.disabled}
                style={{
                  marginBottom: '2px',
                  borderRadius: '6px',
                  height: 'auto',
                  lineHeight: 'normal',
                  padding: '8px 12px'
                }}
              >
                <div style={{ minHeight: '20px' }}>
                  <div style={{
                    fontSize: '14px',
                    fontWeight: 500,
                    color: '#262626'
                  }}>
                    {item.label}
                  </div>
                  {item.description && (
                    <Text
                      type="secondary"
                      style={{
                        fontSize: '12px',
                        display: 'block',
                        marginTop: '2px',
                        lineHeight: '1.3'
                      }}
                    >
                      {item.description}
                    </Text>
                  )}
                </div>
              </Menu.Item>
            ))}
            <Divider style={{ margin: '12px 0 16px 0' }} />
          </Menu.ItemGroup>
        ))}
      </Menu>
    </div>
  );

  return (
    <>
      {/* Menu Button */}
      <Button
        type="text"
        icon={<MenuOutlined />}
        onClick={() => setDrawerVisible(true)}
        style={{
          color: '#595959',
          border: '1px solid #d9d9d9',
          borderRadius: '6px',
          height: '32px',
          display: 'flex',
          alignItems: 'center',
          gap: '6px'
        }}
      >
        Menu
      </Button>

      {/* Navigation Drawer */}
      <Drawer
        title={
          <Space>
            <RocketOutlined style={{ color: '#667eea' }} />
            <Text strong style={{ color: '#262626', fontSize: '16px' }}>
              Navigation
            </Text>
          </Space>
        }
        placement="left"
        onClose={() => setDrawerVisible(false)}
        open={drawerVisible}
        width={280}
        styles={{
          body: { padding: '8px 0' },
          header: {
            borderBottom: '1px solid #f0f0f0',
            padding: '16px 24px'
          }
        }}
      >
        <MenuContent />
      </Drawer>
    </>
  );
};
