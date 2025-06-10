import React, { useState } from 'react';
import { Layout, Menu, Button, Typography, Space, Tooltip, Badge } from 'antd';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  HomeOutlined,
  BarChartOutlined,
  TableOutlined,
  HistoryOutlined,
  SettingOutlined,
  DatabaseOutlined,
  BulbOutlined,
  UserOutlined,
  TeamOutlined,
  FileTextOutlined,
  DashboardOutlined,
  LineChartOutlined,
  PieChartOutlined,
  RobotOutlined,
  LeftOutlined,
  RightOutlined,
  ThunderboltOutlined,
  BookOutlined,
  CodeOutlined,
  ShareAltOutlined,
  SafetyOutlined,
  RocketOutlined
} from '@ant-design/icons';
import { useApplicationState } from '../../hooks/useEnhancedState';
import { useTheme } from '../../contexts/ThemeContext';
import { useActiveResult } from '../../stores/activeResultStore';

const { Sider } = Layout;
const { Text } = Typography;

interface EnhancedNavigationProps {
  isAdmin?: boolean;
}

export const EnhancedNavigation: React.FC<EnhancedNavigationProps> = ({
  isAdmin = false
}) => {
  const { appState, updateAppState } = useApplicationState();
  const navigate = useNavigate();
  const location = useLocation();
  const { isDarkMode } = useTheme();

  // Use local state for testing, fallback to app state
  const [localCollapsed, setLocalCollapsed] = useState(appState.sidebarCollapsed);
  const collapsed = localCollapsed;

  const toggleSidebar = () => {
    const newCollapsed = !collapsed;
    setLocalCollapsed(newCollapsed);
    updateAppState({ sidebarCollapsed: newCollapsed });
  };

  // Get query history from localStorage
  const getQueryHistoryCount = () => {
    try {
      const stored = localStorage.getItem('query-history');
      return stored ? JSON.parse(stored).length : 0;
    } catch {
      return 0;
    }
  };

  // Use active result store
  const { hasResult } = useActiveResult();
  const queryHistoryCount = getQueryHistoryCount();

  const navigationItems = [
    {
      key: 'main',
      type: 'group',
      label: 'Main',
      children: [
        {
          key: '/',
          icon: <HomeOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Query Interface</span>,
          tooltip: 'Ask questions about your data'
        }
      ]
    },
    {
      key: 'analytics',
      type: 'group',
      label: 'Analytics & Visualization',
      children: [
        {
          key: '/results',
          icon: <BarChartOutlined className="enhanced-nav-icon" />,
          label: (
            <Badge dot={hasResult} offset={[8, 0]}>
              <span className="enhanced-nav-text">Results & Charts</span>
            </Badge>
          ),
          tooltip: 'View query results and basic charts',
          disabled: !hasResult
        },
        {
          key: '/dashboard',
          icon: <DashboardOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Dashboard Builder</span>,
          tooltip: 'Create and manage interactive dashboards'
        },
        {
          key: '/interactive',
          icon: <RocketOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Interactive Charts</span>,
          tooltip: 'Advanced interactive visualizations with AI-powered features'
        }
      ]
    },
    {
      key: 'tools',
      type: 'group',
      label: 'Query Tools',
      children: [
        {
          key: '/history',
          icon: <HistoryOutlined className="enhanced-nav-icon" />,
          label: (
            <Space>
              <span className="enhanced-nav-text">Query History</span>
              {queryHistoryCount > 0 && (
                <Badge count={queryHistoryCount} style={{ fontSize: '11px' }} />
              )}
            </Space>
          ),
          tooltip: 'Browse and reuse past queries'
        },
        {
          key: '/templates',
          icon: <BookOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Query Templates</span>,
          tooltip: 'Pre-built query templates'
        },
        {
          key: '/suggestions',
          icon: <BulbOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Smart Suggestions</span>,
          tooltip: 'AI-powered query suggestions'
        },
        {
          key: '/enhanced-query',
          icon: <CodeOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Query Builder</span>,
          tooltip: 'Advanced query building tools'
        }
      ]
    },
    {
      key: 'system',
      type: 'group',
      label: 'System & Tools',
      children: [
        {
          key: '/db-explorer',
          icon: <DatabaseOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Database Explorer</span>,
          tooltip: 'Explore database schema and preview table data'
        },
        {
          key: '/performance-monitoring',
          icon: <ThunderboltOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Performance Monitor</span>,
          tooltip: 'Real-time system performance and optimization'
        },
        {
          key: '/global-result-demo',
          icon: <ShareAltOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Global Result Demo</span>,
          tooltip: 'See how results work across pages'
        },
        {
          key: '/enhanced-demo',
          icon: <RocketOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Features Demo</span>,
          tooltip: 'Comprehensive demo of all enhanced features'
        }
      ]
    }
  ];

  // Add admin items if user is admin
  if (isAdmin) {
    navigationItems.push({
      key: 'admin',
      type: 'group',
      label: 'Administration',
      children: [
        {
          key: '/admin/tuning',
          icon: <SettingOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">AI Tuning</span>,
          tooltip: 'Configure AI models and prompts'
        },
        {
          key: '/admin/schemas',
          icon: <DatabaseOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Schema Management</span>,
          tooltip: 'Manage business context schemas'
        },
        {
          key: '/admin/cache',
          icon: <ThunderboltOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Cache Manager</span>,
          tooltip: 'Manage query caching'
        },
        {
          key: '/admin/security',
          icon: <SafetyOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Security Dashboard</span>,
          tooltip: 'Security monitoring and settings'
        },
        {
          key: '/admin/suggestions',
          icon: <BulbOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Query Suggestions</span>,
          tooltip: 'Manage AI query suggestions and categories'
        },
        {
          key: '/ui-demo',
          icon: <RocketOutlined className="enhanced-nav-icon" />,
          label: <span className="enhanced-nav-text">Enhanced UI Demo</span>,
          tooltip: 'Preview the new enhanced UI components'
        }
      ]
    });
  }



  const handleMenuClick = ({ key }: { key: string }) => {
    navigate(key);
  };

  const getCurrentKey = () => {
    const path = location.pathname;
    if (path === '/') return '/';
    if (path.startsWith('/admin/')) return path;
    return path;
  };

  const renderMenuItem = (item: any) => {
    const isActive = getCurrentKey() === item.key;
    const isDisabled = item.disabled;

    return (
      <div className="enhanced-nav-item" key={item.key}>
        <div
          className={`enhanced-nav-link ${isActive ? 'active' : ''} ${isDisabled ? 'disabled' : ''}`}
          onClick={() => !isDisabled && handleMenuClick({ key: item.key })}
          style={{
            cursor: isDisabled ? 'not-allowed' : 'pointer',
            opacity: isDisabled ? 0.5 : 1
          }}
        >
          {item.icon}
          {item.label}
          {collapsed && (
            <div className="nav-tooltip">
              {item.tooltip || (typeof item.label === 'string' ? item.label : 'Navigation Item')}
            </div>
          )}
        </div>
      </div>
    );
  };

  const renderMenuGroup = (group: any) => (
    <div key={group.key}>
      {!collapsed && (
        <div className="nav-section-title">
          {group.label}
        </div>
      )}
      {group.children.map(renderMenuItem)}
      <div className="nav-section-divider" />
    </div>
  );

  return (
    <Sider
      collapsible={false}
      collapsed={collapsed}
      trigger={null}
      width={collapsed ? 80 : 280}
      collapsedWidth={80}
      className={`enhanced-sidebar ${collapsed ? 'collapsed' : ''}`}
      style={{
        background: isDarkMode ? '#1e1e1e' : '#ffffff',
        borderRight: `1px solid ${isDarkMode ? '#334155' : '#e2e8f0'}`,
        boxShadow: isDarkMode
          ? '2px 0 8px rgba(0, 0, 0, 0.3)'
          : '2px 0 8px rgba(0, 0, 0, 0.06)',
        width: collapsed ? '80px !important' : '280px !important',
        minWidth: collapsed ? '80px !important' : '280px !important',
        maxWidth: collapsed ? '80px !important' : '280px !important'
      }}
    >
      {/* Sidebar Header */}
      <div className="enhanced-sidebar-header">
        <div className="sidebar-brand">
          <div className="sidebar-brand-icon">
            <RobotOutlined />
          </div>
          {!collapsed && (
            <Text className="sidebar-brand-text">
              BI Copilot
            </Text>
          )}
        </div>
        <Tooltip title={collapsed ? 'Expand Sidebar' : 'Collapse Sidebar'}>
          <Button
            type="text"
            icon={collapsed ? <RightOutlined /> : <LeftOutlined />}
            onClick={toggleSidebar}
            className="sidebar-toggle"
            style={{
              backgroundColor: collapsed ? '#f0f0f0' : '#e6f7ff',
              border: '1px solid #d9d9d9'
            }}
          />
        </Tooltip>
      </div>

      {/* Navigation Menu */}
      <div className="enhanced-nav-menu">
        {navigationItems.map(renderMenuGroup)}
      </div>
    </Sider>
  );
};

export default EnhancedNavigation;
