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
  MonitorOutlined,
  RocketOutlined
} from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAppSelector, useAppDispatch } from '../../hooks'
import { selectUser, selectIsAdmin, authActions } from '../../store/auth'
import { selectSidebarCollapsed, uiActions } from '../../store/ui'
import { SystemStatusIndicator } from './ApiModeToggle'

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
    if (path === '/admin' || path === '/admin/' || path === '/admin/dashboard') return ['admin-dashboard']
    if (path === '/admin/business-metadata') return ['business-intelligence']
    if (path === '/admin/business-metadata-enhanced') return ['business-intelligence']
    if (path === '/admin/analytics') return ['admin-dashboard'] // Redirect to consolidated dashboard

    // System Management
    if (path === '/admin/performance') return ['admin-ai-analytics'] // Redirect to consolidated AI analytics
    if (path === '/admin/cost-management') return ['admin-ai-analytics'] // Redirect to consolidated AI analytics
    if (path === '/admin/performance-cost') return ['admin-ai-analytics'] // Redirect to consolidated AI analytics
    if (path === '/admin/ai-analytics') return ['admin-ai-analytics']
    if (path === '/admin/llm-management') return ['admin-llm-management']
    if (path === '/admin/users') return ['admin-users']
    if (path === '/admin/system-config') return ['admin-config']

    // Advanced Features - AI Transparency (consolidated)
    if (path === '/admin/ai-transparency') return ['admin-ai-transparency']
    if (path === '/admin/transparency-dashboard') return ['admin-ai-transparency']
    if (path === '/admin/transparency-management') return ['admin-ai-transparency']
    if (path === '/admin/transparency-review') return ['admin-ai-transparency']
    if (path === '/admin/ai-transparency-analysis') return ['admin-ai-transparency']

    // Advanced Features - Template Analytics (consolidated)
    if (path === '/admin/template-analytics') return ['admin-template-analytics']
    if (path.startsWith('/admin/template-analytics')) return ['admin-template-analytics']

    return []
  }

  const menuItems = [
    // Main User Interface
    {
      key: 'main-section',
      label: sidebarCollapsed ? '' : 'Main Interface',
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
        label: sidebarCollapsed ? '' : 'Data & Analytics',
        type: 'group',
        icon: <LineChartOutlined />,
      },
      {
        key: 'admin-dashboard',
        icon: <DashboardOutlined />,
        label: 'Admin Dashboard',
        onClick: () => navigate('/admin/dashboard'),
      },
      {
        key: 'business-metadata',
        icon: <DatabaseOutlined />,
        label: 'Business Metadata',
        onClick: () => navigate('/admin/business-metadata'),
      },
      // System Management
      {
        key: 'system-section',
        label: sidebarCollapsed ? '' : 'System Management',
        type: 'group',
        icon: <ControlOutlined />,
      },
      {
        key: 'admin-ai-analytics',
        icon: <AreaChartOutlined />,
        label: 'AI Analytics',
        onClick: () => navigate('/admin/ai-analytics'),
      },
      {
        key: 'admin-llm-management',
        icon: <ApiOutlined />,
        label: 'LLM Management',
        onClick: () => navigate('/admin/llm-management'),
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
        label: sidebarCollapsed ? '' : 'Advanced Features',
        type: 'group',
        icon: <ExperimentOutlined />,
      },
      {
        key: 'admin-ai-transparency',
        icon: <EyeOutlined />,
        label: 'ProcessFlow Transparency',
        onClick: () => navigate('/admin/ai-transparency'),
      },

      {
        key: 'admin-template-analytics',
        icon: <FileTextOutlined />,
        label: 'Template Analytics',
        onClick: () => navigate('/admin/template-analytics'),
      },
    ] : []),
  ]

  return (
    <AntLayout style={{ minHeight: '100vh' }}>
      <Sider
        trigger={null}
        collapsible
        collapsed={sidebarCollapsed}
        width={sidebarWidth}
        collapsedWidth={80}
        style={{
          background: '#001529',
          position: 'relative',
          height: '100vh',
          overflow: 'hidden',
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
          {sidebarCollapsed ? (
            <RocketOutlined style={{ color: 'white', fontSize: '18px' }} />
          ) : (
            <Text style={{ color: 'white', fontWeight: 'bold' }}>
              BI Copilot
            </Text>
          )}
        </div>
        <div style={{
          height: 'calc(100vh - 64px)', // Account for brand section height
          display: 'flex',
          flexDirection: 'column'
        }}>
          <Menu
            theme="dark"
            mode="inline"
            selectedKeys={getSelectedKeys()}
            items={menuItems}
            style={{
              flex: 1,
              borderRight: 0,
              overflow: 'auto',
              minHeight: 0, // Important for flex child with overflow
            }}
          />

          {/* Bottom Section with User Info and Controls - Sticky to bottom */}
          <div
            style={{
              padding: sidebarCollapsed ? '12px 8px' : '16px',
              borderTop: '1px solid rgba(255, 255, 255, 0.1)',
              background: 'rgba(0, 0, 0, 0.2)',
              flexShrink: 0, // Prevent shrinking
            }}
          >
            {sidebarCollapsed ? (
              // Collapsed view - stacked icons
              <div style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                gap: '8px',
                width: '100%'
              }}>
                {/* User Avatar */}
                <Dropdown
                  menu={{ items: userMenuItems }}
                  placement="topRight"
                  arrow
                  overlayStyle={{
                    background: '#1f1f1f',
                    border: '1px solid #434343',
                    borderRadius: '6px',
                    boxShadow: '0 4px 12px rgba(0, 0, 0, 0.4)',
                  }}
                  overlayClassName="dark-dropdown"
                >
                  <Avatar
                    style={{ cursor: 'pointer' }}
                    icon={<UserOutlined />}
                    size="small"
                  />
                </Dropdown>

                {/* System Status Indicator - Collapsed Version */}
                <div style={{ width: '100%' }}>
                  <SystemStatusIndicator collapsed={true} />
                </div>
              </div>
            ) : (
              // Expanded view - full layout
              <div>
                {/* User Info */}
                <div style={{ marginBottom: '12px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
                    <Avatar
                      icon={<UserOutlined />}
                      size="small"
                    />
                    <div style={{ flex: 1, minWidth: 0 }}>
                      <div style={{ color: 'white', fontSize: '12px', fontWeight: 500, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {user?.displayName || user?.username}
                      </div>
                      <div style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: '10px' }}>
                        {isAdmin ? 'Administrator' : 'User'}
                      </div>
                    </div>
                    <Dropdown
                      menu={{ items: userMenuItems }}
                      placement="topRight"
                      arrow
                      overlayStyle={{
                        background: '#1f1f1f',
                        border: '1px solid #434343',
                        borderRadius: '6px',
                        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.4)',
                      }}
                      overlayClassName="dark-dropdown"
                    >
                      <Button
                        type="text"
                        icon={<SettingOutlined />}
                        size="small"
                        style={{ color: 'rgba(255, 255, 255, 0.6)' }}
                      />
                    </Dropdown>
                  </div>
                </div>

                {/* System Status Indicator */}
                <SystemStatusIndicator collapsed={false} />
              </div>
            )}
          </div>
        </div>

        {/* Resize Handle */}
        {!sidebarCollapsed && (
          <div
            onMouseDown={handleMouseDown}
            style={{
              position: 'absolute',
              top: 0,
              right: 0,
              width: 4,
              height: '100%',
              cursor: 'col-resize',
              backgroundColor: 'transparent',
              zIndex: 1000,
              transition: 'background-color 0.2s',
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.backgroundColor = 'rgba(255, 255, 255, 0.1)'
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.backgroundColor = 'transparent'
            }}
          />
        )}
      </Sider>
      
      <AntLayout>
        <Header
          style={{
            padding: '0 16px',
            background: '#fff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'flex-start',
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
