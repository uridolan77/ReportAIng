import React, { useState, useCallback } from 'react'
import { Layout as AntLayout, Menu, Avatar, Dropdown, Space, Typography, Button, Divider } from 'antd'
import {
  UserOutlined,
  LogoutOutlined,
  SettingOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  MessageOutlined,
  HistoryOutlined,
  FileSearchOutlined,
  DashboardOutlined,
  DatabaseOutlined,
  BarChartOutlined,
  DollarOutlined,
  ThunderboltOutlined,
  TeamOutlined,
  ToolOutlined,
  RobotOutlined,
  EyeOutlined,
  ExperimentOutlined,
  BulbOutlined,
  AppstoreOutlined,
  LineChartOutlined,
  ControlOutlined,
  StarOutlined,
  CommentOutlined,
  SearchOutlined,
  FundOutlined,
  ApiOutlined,
  SafetyOutlined,
  FileTextOutlined,
  AreaChartOutlined,
  BranchesOutlined,
  MonitorOutlined
} from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAppSelector, useAppDispatch } from '../../hooks'
import { selectUser, selectIsAdmin, authActions } from '../../store/auth'
import { selectSidebarCollapsed, uiActions } from '../../store/ui'
import { ApiModeToggle } from './ApiModeToggle'
import { AIStatusIndicator } from './AIStatusIndicator'

const { Header, Sider, Content } = AntLayout
const { Text } = Typography

interface AppLayoutProps {
  children: React.ReactNode
}

export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  const navigate = useNavigate()
  const location = useLocation()
  const dispatch = useAppDispatch()

  const user = useAppSelector(selectUser)
  const isAdmin = useAppSelector(selectIsAdmin)
  const sidebarCollapsed = useAppSelector(selectSidebarCollapsed)

  // Sidebar width state
  const [sidebarWidth, setSidebarWidth] = useState(280)
  const [isResizing, setIsResizing] = useState(false)

  const handleLogout = () => {
    dispatch(authActions.logout())
    navigate('/login')
  }

  const toggleSidebar = () => {
    dispatch(uiActions.setSidebarCollapsed(!sidebarCollapsed))
  }

  // Sidebar resize handlers
  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    e.preventDefault()
    setIsResizing(true)
  }, [])

  const handleMouseMove = useCallback((e: MouseEvent) => {
    if (!isResizing) return

    const newWidth = e.clientX
    if (newWidth >= 200 && newWidth <= 400) {
      setSidebarWidth(newWidth)
    }
  }, [isResizing])

  const handleMouseUp = useCallback(() => {
    setIsResizing(false)
  }, [])

  React.useEffect(() => {
    if (isResizing) {
      document.addEventListener('mousemove', handleMouseMove)
      document.addEventListener('mouseup', handleMouseUp)
      document.body.style.cursor = 'col-resize'
      document.body.style.userSelect = 'none'
    } else {
      document.removeEventListener('mousemove', handleMouseMove)
      document.removeEventListener('mouseup', handleMouseUp)
      document.body.style.cursor = ''
      document.body.style.userSelect = ''
    }

    return () => {
      document.removeEventListener('mousemove', handleMouseMove)
      document.removeEventListener('mouseup', handleMouseUp)
      document.body.style.cursor = ''
      document.body.style.userSelect = ''
    }
  }, [isResizing, handleMouseMove, handleMouseUp])

  const userMenuItems = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: 'Profile',
      onClick: () => navigate('/profile'),
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: 'Settings',
      onClick: () => navigate('/settings'),
    },
    {
      type: 'divider' as const,
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Logout',
      onClick: handleLogout,
    },
  ]

  const getSelectedKeys = () => {
    const path = location.pathname

    // Main Interface
    if (path === '/chat' || path === '/chat/') return ['chat-standard']
    if (path.startsWith('/chat/enhanced')) return ['chat-enhanced']
    if (path === '/chat/history') return ['chat-history']
    if (path === '/admin/business-intelligence') return ['business-intelligence']

    // Data & Analytics
    if (path === '/admin' || path === '/admin/') return ['admin-dashboard']
    if (path === '/admin/business-metadata') return ['admin-metadata-standard']
    if (path === '/admin/business-metadata-enhanced') return ['admin-metadata-enhanced']
    if (path === '/admin/analytics') return ['admin-analytics']

    // System Management
    if (path === '/admin/performance') return ['admin-performance-metrics']
    if (path === '/admin/cost-management') return ['admin-cost']
    if (path === '/admin/ai-analytics') return ['admin-ai-analytics']
    if (path === '/admin/llm-management') return ['admin-llm-management']
    if (path === '/admin/users') return ['admin-users']
    if (path === '/admin/system-config') return ['admin-config']

    // Advanced Features - AI Transparency
    if (path === '/admin/transparency-dashboard') return ['admin-ai-transparency-dashboard']
    if (path === '/admin/transparency-management') return ['admin-transparency-management']
    if (path === '/admin/transparency-review') return ['admin-transparency-review']
    if (path === '/admin/ai-transparency-analysis') return ['admin-ai-transparency-analysis']

    // Advanced Features - Template Analytics
    if (path === '/admin/template-analytics/performance') return ['admin-template-performance']
    if (path === '/admin/template-analytics/ab-testing') return ['admin-template-ab-testing']
    if (path === '/admin/template-analytics/management') return ['admin-template-management']
    if (path === '/admin/template-analytics/analytics') return ['admin-template-advanced']
    if (path.startsWith('/admin/template-analytics')) return ['admin-template-analytics']

    return []
  }

  const menuItems = [
    // Main User Interface
    {
      key: 'main-section',
      label: 'Main Interface',
      type: 'group',
      icon: <AppstoreOutlined />,
    },
    {
      key: 'chat',
      icon: <MessageOutlined />,
      label: 'AI Chat',
      children: [
        {
          key: 'chat-standard',
          icon: <CommentOutlined />,
          label: 'Standard Chat',
          onClick: () => navigate('/chat'),
        },
        {
          key: 'chat-enhanced',
          icon: <StarOutlined />,
          label: 'Enhanced Chat',
          onClick: () => navigate('/chat/enhanced'),
        },
      ],
    },
    {
      key: 'business-intelligence',
      icon: <BulbOutlined />,
      label: 'Business Intelligence',
      onClick: () => navigate('/admin/business-intelligence'),
    },
    {
      key: 'chat-history',
      icon: <HistoryOutlined />,
      label: 'Query History',
      onClick: () => navigate('/chat/history'),
    },
    ...(isAdmin ? [
      // Data & Analytics
      {
        key: 'data-section',
        label: 'Data & Analytics',
        type: 'group',
        icon: <LineChartOutlined />,
      },
      {
        key: 'admin-dashboard',
        icon: <DashboardOutlined />,
        label: 'Admin Dashboard',
        onClick: () => navigate('/admin'),
      },
      {
        key: 'admin-metadata',
        icon: <DatabaseOutlined />,
        label: 'Business Metadata',
        children: [
          {
            key: 'admin-metadata-standard',
            icon: <FileTextOutlined />,
            label: 'Standard View',
            onClick: () => navigate('/admin/business-metadata'),
          },
          {
            key: 'admin-metadata-enhanced',
            icon: <StarOutlined />,
            label: 'Enhanced View',
            onClick: () => navigate('/admin/business-metadata-enhanced'),
          },
        ],
      },
      {
        key: 'admin-analytics',
        icon: <BarChartOutlined />,
        label: 'Analytics & Reports',
        onClick: () => navigate('/admin/analytics'),
      },
      // System Management
      {
        key: 'system-section',
        label: 'System Management',
        type: 'group',
      },
      {
        key: 'admin-performance',
        icon: <ThunderboltOutlined />,
        label: 'Performance & Cost',
        children: [
          {
            key: 'admin-performance-metrics',
            label: 'Performance Metrics',
            onClick: () => navigate('/admin/performance'),
          },
          {
            key: 'admin-cost',
            label: 'Cost Management',
            onClick: () => navigate('/admin/cost-management'),
          },
        ],
      },
      {
        key: 'admin-ai-management',
        icon: <RobotOutlined />,
        label: 'AI Management',
        children: [
          {
            key: 'admin-ai-analytics',
            label: 'AI Analytics',
            onClick: () => navigate('/admin/ai-analytics'),
          },
          {
            key: 'admin-llm-management',
            label: 'LLM Management',
            onClick: () => navigate('/admin/llm-management'),
          },
        ],
      },
      {
        key: 'admin-users',
        icon: <TeamOutlined />,
        label: 'User Management',
        onClick: () => navigate('/admin/users'),
      },
      {
        key: 'admin-config',
        icon: <ToolOutlined />,
        label: 'System Config',
        onClick: () => navigate('/admin/system-config'),
      },
      // Advanced Features
      {
        key: 'advanced-section',
        label: 'Advanced Features',
        type: 'group',
      },
      {
        key: 'admin-ai-transparency',
        icon: <EyeOutlined />,
        label: 'AI Transparency',
        children: [
          {
            key: 'admin-ai-transparency-dashboard',
            label: 'Dashboard',
            onClick: () => navigate('/admin/transparency-dashboard'),
          },
          {
            key: 'admin-transparency-management',
            label: 'Management',
            onClick: () => navigate('/admin/transparency-management'),
          },
          {
            key: 'admin-transparency-review',
            label: 'Review & Analysis',
            onClick: () => navigate('/admin/transparency-review'),
          },
          {
            key: 'admin-ai-transparency-analysis',
            label: 'Interactive Analysis',
            onClick: () => navigate('/admin/ai-transparency-analysis'),
          },
        ],
      },
      {
        key: 'admin-template-analytics',
        icon: <ExperimentOutlined />,
        label: 'Template Analytics',
        children: [
          {
            key: 'admin-template-performance',
            label: 'Performance Dashboard',
            onClick: () => navigate('/admin/template-analytics/performance'),
          },
          {
            key: 'admin-template-ab-testing',
            label: 'A/B Testing',
            onClick: () => navigate('/admin/template-analytics/ab-testing'),
          },
          {
            key: 'admin-template-management',
            label: 'Template Management',
            onClick: () => navigate('/admin/template-analytics/management'),
          },
          {
            key: 'admin-template-advanced',
            label: 'Advanced Analytics',
            onClick: () => navigate('/admin/template-analytics/analytics'),
          },
        ],
      },
    ] : []),
  ]

  return (
    <AntLayout style={{ minHeight: '100vh' }}>
      <Sider
        trigger={null}
        collapsible
        collapsed={sidebarCollapsed}
        style={{
          background: '#001529',
        }}
      >
        <div
          style={{
            height: 32,
            margin: 16,
            background: 'rgba(255, 255, 255, 0.3)',
            borderRadius: 6,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          {!sidebarCollapsed && (
            <Text style={{ color: 'white', fontWeight: 'bold' }}>
              BI Copilot
            </Text>
          )}
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={getSelectedKeys()}
          items={menuItems}
        />
      </Sider>
      
      <AntLayout>
        <Header
          style={{
            padding: '0 16px',
            background: '#fff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: '1px solid #f0f0f0',
          }}
        >
          <Button
            type="text"
            icon={sidebarCollapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={toggleSidebar}
            style={{
              fontSize: '16px',
              width: 64,
              height: 64,
            }}
          />
          
          <Space size="large">
            {/* AI Status Indicator */}
            <AIStatusIndicator />

            <Divider type="vertical" style={{ height: '24px' }} />

            {/* API Mode Toggle */}
            <ApiModeToggle />

            <Divider type="vertical" style={{ height: '24px' }} />

            {/* User Info and Menu */}
            <Space>
              <Text>Welcome, {user?.displayName || user?.username}</Text>
              <Dropdown
                menu={{ items: userMenuItems }}
                placement="bottomRight"
                arrow
              >
                <Avatar
                  style={{ cursor: 'pointer' }}
                  icon={<UserOutlined />}
                />
              </Dropdown>
            </Space>
          </Space>
        </Header>
        
        <Content
          style={{
            margin: '24px 16px',
            padding: 24,
            minHeight: 280,
            background: '#fff',
            borderRadius: 6,
          }}
        >
          {children}
        </Content>
      </AntLayout>
    </AntLayout>
  )
}

// Simple page layout for content areas
interface PageLayoutProps {
  title?: string
  subtitle?: string
  extra?: React.ReactNode
  children: React.ReactNode
}

export const PageLayout: React.FC<PageLayoutProps> = ({
  title,
  subtitle,
  extra,
  children,
}) => {
  return (
    <div>
      {(title || subtitle || extra) && (
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'flex-start',
            marginBottom: 24,
            paddingBottom: 16,
            borderBottom: '1px solid #f0f0f0',
          }}
        >
          <div>
            {title && (
              <Typography.Title level={2} style={{ margin: 0 }}>
                {title}
              </Typography.Title>
            )}
            {subtitle && (
              <Typography.Text type="secondary">
                {subtitle}
              </Typography.Text>
            )}
          </div>
          {extra && <div>{extra}</div>}
        </div>
      )}
      {children}
    </div>
  )
}
