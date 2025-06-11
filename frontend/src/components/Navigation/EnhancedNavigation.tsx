import React, { useState } from 'react'; // Updated with icons and toggle
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
import '../styles/enhanced-navigation.css';

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
      label: 'MAIN',
      children: [
        {
          key: '/',
          icon: <RobotOutlined style={{ fontSize: '20px', color: '#3b82f6' }} />,
          label: <span className="enhanced-nav-text">Query Interface</span>,
          tooltip: 'Ask questions about your data in natural language'
        }
      ]
    },
    {
      key: 'analytics',
      type: 'group',
      label: 'ANALYTICS & VISUALIZATION',
      children: [
        {
          key: '/results',
          icon: <BarChartOutlined style={{ fontSize: '20px', color: '#10b981' }} />,
          label: (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
              <span className="enhanced-nav-text">Results & Charts</span>
              {hasResult && <Badge dot status="success" />}
            </div>
          ),
          tooltip: 'View query results and basic charts',
          disabled: !hasResult
        },
        {
          key: '/dashboard',
          icon: <DashboardOutlined style={{ fontSize: '20px', color: '#8b5cf6' }} />,
          label: <span className="enhanced-nav-text">Dashboard Builder</span>,
          tooltip: 'Create and manage interactive dashboards'
        },
        {
          key: '/interactive',
          icon: <LineChartOutlined style={{ fontSize: '20px', color: '#f59e0b' }} />,
          label: <span className="enhanced-nav-text">Interactive Charts</span>,
          tooltip: 'Advanced interactive visualizations with AI-powered features'
        }
      ]
    },
    {
      key: 'tools',
      type: 'group',
      label: 'QUERY TOOLS',
      children: [
        {
          key: '/history',
          icon: <HistoryOutlined style={{ fontSize: '20px', color: '#6b7280' }} />,
          label: (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
              <span className="enhanced-nav-text">Query History</span>
              {queryHistoryCount > 0 && (
                <Badge count={queryHistoryCount} size="small" style={{ fontSize: '10px', minWidth: '16px', height: '16px' }} />
              )}
            </div>
          ),
          tooltip: 'Browse and reuse past queries'
        },
        {
          key: '/templates',
          icon: <FileTextOutlined style={{ fontSize: '20px', color: '#6b7280' }} />,
          label: <span className="enhanced-nav-text">Query Templates</span>,
          tooltip: 'Pre-built query templates for common scenarios'
        },
        {
          key: '/suggestions',
          icon: <BulbOutlined style={{ fontSize: '20px', color: '#6b7280' }} />,
          label: <span className="enhanced-nav-text">Smart Suggestions</span>,
          tooltip: 'AI-powered query suggestions'
        },
        {
          key: '/enhanced-query',
          icon: <CodeOutlined style={{ fontSize: '20px', color: '#6b7280' }} />,
          label: <span className="enhanced-nav-text">Query Builder</span>,
          tooltip: 'Advanced visual query building tools'
        }
      ]
    },
    {
      key: 'system',
      type: 'group',
      label: 'SYSTEM & TOOLS',
      children: [
        {
          key: '/db-explorer',
          icon: <DatabaseOutlined style={{ fontSize: '20px', color: '#059669' }} />,
          label: <span className="enhanced-nav-text">Database Explorer</span>,
          tooltip: 'Explore database schema and preview table data'
        },
        {
          key: '/performance-monitoring',
          icon: <ThunderboltOutlined style={{ fontSize: '20px', color: '#dc2626' }} />,
          label: <span className="enhanced-nav-text">Performance Monitor</span>,
          tooltip: 'Real-time system performance and optimization'
        },
        {
          key: '/global-result-demo',
          icon: <ShareAltOutlined style={{ fontSize: '20px', color: '#7c3aed' }} />,
          label: <span className="enhanced-nav-text">Global Result Demo</span>,
          tooltip: 'See how results work across pages'
        },
        {
          key: '/enhanced-demo',
          icon: <RocketOutlined style={{ fontSize: '20px', color: '#ea580c' }} />,
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
      label: 'ADMINISTRATION',
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
            opacity: isDisabled ? 0.5 : 1,
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            padding: '12px 16px',
            color: '#64748b',
            textDecoration: 'none',
            fontSize: '14px',
            fontWeight: '500',
            transition: 'all 0.3s ease',
            borderRadius: '10px'
          }}
        >
          <span style={{
            fontSize: '20px',
            minWidth: '20px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#3b82f6',
            background: 'rgba(59, 130, 246, 0.1)',
            borderRadius: '6px',
            padding: '4px'
          }}>
            {item.icon || '🔧'}
          </span>
          {!collapsed && (
            <span style={{
              whiteSpace: 'nowrap',
              flex: 1
            }}>
              {item.label}
            </span>
          )}
          {collapsed && (
            <div className="nav-tooltip" style={{
              position: 'absolute',
              left: '100%',
              top: '50%',
              transform: 'translateY(-50%)',
              background: '#ffffff',
              color: '#1f2937',
              padding: '8px 12px',
              borderRadius: '8px',
              fontSize: '14px',
              fontWeight: '500',
              whiteSpace: 'nowrap',
              boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
              border: '1px solid #e2e8f0',
              opacity: 0,
              visibility: 'hidden',
              transition: 'all 0.3s ease',
              zIndex: 1000,
              marginLeft: '12px'
            }}>
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
        <div style={{
          padding: '12px 16px 8px 16px',
          color: '#9ca3af',
          fontSize: '11px',
          fontWeight: '600',
          textTransform: 'uppercase',
          letterSpacing: '0.8px',
          fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, sans-serif'
        }}>
          {group.label}
        </div>
      )}
      {group.children.map(renderMenuItem)}
      <div style={{
        height: '1px',
        background: 'linear-gradient(90deg, transparent, #e2e8f0, transparent)',
        margin: '24px 16px'
      }} />
    </div>
  );

  return (
    <>
      <style>{`
        .enhanced-nav-item:hover .nav-tooltip {
          opacity: 1 !important;
          visibility: visible !important;
        }
        .enhanced-nav-item:hover .enhanced-nav-link {
          background: #f1f5f9 !important;
          color: #1f2937 !important;
          transform: translateX(2px) !important;
        }
        .enhanced-nav-item .enhanced-nav-link.active {
          background: linear-gradient(135deg, rgba(59, 130, 246, 0.12) 0%, rgba(59, 130, 246, 0.06) 100%) !important;
          color: #2563eb !important;
          font-weight: 600 !important;
        }
      `}</style>
      <Sider
        collapsible={false}
        collapsed={collapsed}
        trigger={null}
        width={collapsed ? 70 : 260}
        collapsedWidth={70}
        className={`enhanced-sidebar ${collapsed ? 'collapsed' : ''}`}
        style={{
          background: isDarkMode ? '#1e1e1e' : 'linear-gradient(180deg, #ffffff 0%, #fafbfc 100%)',
          borderRight: `1px solid ${isDarkMode ? '#334155' : '#e2e8f0'}`,
          boxShadow: isDarkMode
            ? '2px 0 8px rgba(0, 0, 0, 0.3)'
            : '2px 0 12px rgba(0, 0, 0, 0.08)',
          width: collapsed ? '70px !important' : '260px !important',
          minWidth: collapsed ? '70px !important' : '260px !important',
          maxWidth: collapsed ? '70px !important' : '260px !important',
          flex: `0 0 ${collapsed ? '70px' : '260px'} !important`
        }}
      >
      {/* Sidebar Header */}
      <div style={{
        padding: '20px 16px',
        borderBottom: '1px solid #e2e8f0',
        display: 'flex',
        alignItems: 'center',
        justifyContent: collapsed ? 'center' : 'space-between',
        height: '80px',
        background: 'linear-gradient(135deg, #ffffff 0%, #f8fafc 100%)',
        position: 'relative'
      }}>
        {!collapsed && (
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <div style={{
              width: '36px',
              height: '36px',
              borderRadius: '10px',
              background: 'linear-gradient(135deg, #3b82f6, #2563eb)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'white',
              fontSize: '20px',
              boxShadow: '0 4px 12px rgba(59, 130, 246, 0.25)'
            }}>
              <RobotOutlined />
            </div>
            <div style={{ color: '#1f2937', fontSize: '16px', fontWeight: '600' }}>
              BI Copilot
            </div>
          </div>
        )}
        <Tooltip title={collapsed ? 'Expand Sidebar' : 'Collapse Sidebar'}>
          <Button
            type="text"
            icon={collapsed ? <RightOutlined /> : <LeftOutlined />}
            onClick={toggleSidebar}
            style={{
              backgroundColor: '#f8fafc',
              border: '1px solid #e2e8f0',
              color: '#64748b',
              width: '32px',
              height: '32px',
              borderRadius: '8px',
              display: 'flex !important',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              position: collapsed ? 'absolute' : 'static',
              top: collapsed ? '24px' : 'auto',
              right: collapsed ? '19px' : 'auto',
              zIndex: 10
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.backgroundColor = '#e2e8f0';
              e.currentTarget.style.transform = 'scale(1.05)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.backgroundColor = '#f8fafc';
              e.currentTarget.style.transform = 'scale(1)';
            }}
          />
        </Tooltip>
      </div>

        {/* Navigation Menu */}
        <div style={{
          padding: '16px 0',
          height: 'calc(100vh - 80px)',
          overflowY: 'auto',
          overflowX: 'hidden'
        }}>
          {navigationItems.map(renderMenuGroup)}
        </div>
      </Sider>
    </>
  );
};

export default EnhancedNavigation;
