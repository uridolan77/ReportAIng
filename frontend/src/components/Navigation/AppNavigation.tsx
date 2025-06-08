/**
 * App Navigation Component
 * Provides organized navigation for all features
 */

import React from 'react';
import {
  Menu,
  Layout,
  Button,
  Space,
  Typography,
  Badge,
  Divider
} from 'antd';
import {
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
  CodeOutlined,
  LeftOutlined,
  RightOutlined,
  DatabaseOutlined,
  RobotOutlined
} from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';
import { useApplicationState } from '../../hooks/useEnhancedState';
import { useActiveResult } from '../../stores/activeResultStore';

const { Sider } = Layout;

const { Text } = Typography;

interface AppNavigationProps {
  isAdmin?: boolean;
}

export const AppNavigation: React.FC<AppNavigationProps> = ({ isAdmin = false }) => {
  const { appState, updateAppState } = useApplicationState();
  const navigate = useNavigate();
  const location = useLocation();

  // Use application state for sidebar visibility
  const collapsed = appState.sidebarCollapsed;

  const toggleSidebar = () => {
    updateAppState({ sidebarCollapsed: !collapsed });
  };

  // Get query history from localStorage instead of context
  const getQueryHistoryCount = () => {
    try {
      const stored = localStorage.getItem('query-history');
      return stored ? JSON.parse(stored).length : 0;
    } catch {
      return 0;
    }
  };

  // Use active result store instead of localStorage check
  const { hasResult } = useActiveResult();
  const queryHistoryCount = getQueryHistoryCount();

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
          label: (
            <Badge dot={hasResult} offset={[8, 0]}>
              Results & Charts
            </Badge>
          ),
          description: 'View query results and basic charts',
          disabled: !hasResult
        },
        {
          key: '/dashboard',
          icon: <DashboardOutlined />,
          label: 'Dashboard View',
          description: 'Interactive dashboard builder',
          disabled: false
        },
        {
          key: '/interactive',
          icon: <InteractionOutlined />,
          label: 'Interactive Viz',
          description: 'Advanced interactive visualizations',
          disabled: false
        },
        {
          key: '/advanced-viz',
          icon: <RocketOutlined />,
          label: 'AI-Powered Charts',
          description: 'AI-generated advanced visualizations',
          disabled: false
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
        },
        {
          key: '/enhanced-ai',
          icon: <RobotOutlined />,
          label: 'Enhanced AI Interface',
          description: 'Next-gen AI with real-time streaming'
        },
        {
          key: '/enhanced-demo',
          icon: <RocketOutlined />,
          label: 'Enhanced Features Demo',
          description: 'Comprehensive demo of all enhanced features'
        },
        {
          key: '/enhanced-dashboard',
          icon: <DashboardOutlined />,
          label: 'Multi-Modal Dashboards',
          description: 'AI-powered dashboard creation and management'
        },
        {
          key: '/enhanced-visualization',
          icon: <BarChartOutlined />,
          label: 'Advanced Visualizations',
          description: 'D3.js-powered interactive charts and graphs'
        },
        {
          key: '/performance-monitoring',
          icon: <ThunderboltOutlined />,
          label: 'Performance Monitoring',
          description: 'Real-time system performance and optimization'
        },
        {
          key: '/db-explorer',
          icon: <DatabaseOutlined />,
          label: 'DB Explorer',
          description: 'Explore database schema and preview table data'
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
          key: '/admin/schemas',
          icon: <DatabaseOutlined />,
          label: 'Schema Management',
          description: 'Manage business context schemas'
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
        },
        {
          key: '/admin/suggestions',
          icon: <BulbOutlined />,
          label: 'Query Suggestions',
          description: 'Manage AI query suggestions and categories'
        }
      ]
    });
  }

  const handleMenuClick = ({ key }: { key: string }) => {
    navigate(key);
    // Don't close sidebar on navigation - it stays persistent
  };

  const getCurrentKey = () => {
    const path = location.pathname;
    // Map current path to menu key
    if (path === '/') return '/';
    if (path.startsWith('/admin/')) return path;
    return path;
  };

  const MenuContent = () => (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Sidebar Header */}
      <div style={{
        padding: '16px',
        borderBottom: '1px solid #f0f0f0',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between'
      }}>
        {!collapsed && (
          <Space>
            <RocketOutlined style={{ color: '#667eea', fontSize: '18px' }} />
            <Text strong style={{ color: '#262626', fontSize: '16px' }}>
              Navigation
            </Text>
          </Space>
        )}
        <Button
          type="text"
          icon={collapsed ? <RightOutlined /> : <LeftOutlined />}
          onClick={toggleSidebar}
          style={{
            color: '#595959',
            border: 'none',
            borderRadius: '6px',
            height: '32px',
            width: '32px'
          }}
        />
      </div>

      {/* Menu Content */}
      <div style={{ flex: 1, overflow: 'auto', padding: collapsed ? '8px 0' : '8px 16px' }}>
        <Menu
          mode="inline"
          selectedKeys={[getCurrentKey()]}
          onClick={handleMenuClick}
          inlineCollapsed={collapsed}
          style={{
            border: 'none',
            background: 'transparent'
          }}
        >
          {menuItems.map(group => (
            <Menu.ItemGroup
              key={group.key}
              title={
                !collapsed ? (
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
                ) : null
              }
            >
              {group.children.map(item => (
                <Menu.Item
                  key={item.key}
                  icon={item.icon}
                  disabled={item.disabled}
                  title={collapsed ? item.label : undefined}
                  style={{
                    marginBottom: '2px',
                    borderRadius: '6px',
                    height: 'auto',
                    lineHeight: 'normal',
                    padding: collapsed ? '8px 24px' : '8px 12px'
                  }}
                >
                  {!collapsed ? (
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
                  ) : (
                    <span>{item.label}</span>
                  )}
                </Menu.Item>
              ))}
              {!collapsed && <Divider style={{ margin: '12px 0 16px 0' }} />}
            </Menu.ItemGroup>
          ))}
        </Menu>
      </div>
    </div>
  );

  return (
    <Sider
      collapsible
      collapsed={collapsed}
      onCollapse={toggleSidebar}
      trigger={null}
      width={280}
      collapsedWidth={80}
      style={{
        background: '#ffffff',
        borderRight: '1px solid #f0f0f0',
        boxShadow: '2px 0 8px rgba(0, 0, 0, 0.06)',
        height: '100vh',
        position: 'sticky',
        top: 0,
        left: 0,
        zIndex: 100
      }}
    >
      <MenuContent />
    </Sider>
  );
};
