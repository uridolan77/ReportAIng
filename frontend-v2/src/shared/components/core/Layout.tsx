import React from 'react'
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
  ToolOutlined
} from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAppSelector, useAppDispatch } from '../../hooks'
import { selectUser, selectIsAdmin, authActions } from '../../store/auth'
import { selectSidebarCollapsed, uiActions } from '../../store/ui'
import { ApiModeToggle } from './ApiModeToggle'

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

  const handleLogout = () => {
    dispatch(authActions.logout())
    navigate('/login')
  }

  const toggleSidebar = () => {
    dispatch(uiActions.setSidebarCollapsed(!sidebarCollapsed))
  }

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
    if (path === '/chat' || path === '/chat/') return ['chat']
    if (path === '/chat/history') return ['chat-history']
    if (path.startsWith('/chat/results')) return ['chat-results']
    if (path === '/admin' || path === '/admin/') return ['admin-dashboard']
    if (path === '/admin/business-metadata') return ['admin-metadata']
    if (path === '/admin/analytics') return ['admin-analytics']
    if (path === '/admin/cost-management') return ['admin-cost']
    if (path === '/admin/performance') return ['admin-performance']
    if (path === '/admin/users') return ['admin-users']
    if (path === '/admin/system-config') return ['admin-config']
    return []
  }

  const menuItems = [
    {
      key: 'chat-section',
      label: 'Chat & Queries',
      type: 'group',
    },
    {
      key: 'chat',
      icon: <MessageOutlined />,
      label: 'Chat Interface',
      onClick: () => navigate('/chat'),
    },
    {
      key: 'chat-history',
      icon: <HistoryOutlined />,
      label: 'Query History',
      onClick: () => navigate('/chat/history'),
    },
    {
      key: 'chat-results',
      icon: <FileSearchOutlined />,
      label: 'Query Results',
      onClick: () => navigate('/chat/results'),
    },
    ...(isAdmin ? [
      {
        key: 'admin-section',
        label: 'Administration',
        type: 'group',
      },
      {
        key: 'admin-dashboard',
        icon: <DashboardOutlined />,
        label: 'Dashboard',
        onClick: () => navigate('/admin'),
      },
      {
        key: 'admin-metadata',
        icon: <DatabaseOutlined />,
        label: 'Business Metadata',
        onClick: () => navigate('/admin/business-metadata'),
      },
      {
        key: 'admin-analytics',
        icon: <BarChartOutlined />,
        label: 'Analytics',
        onClick: () => navigate('/admin/analytics'),
      },
      {
        key: 'admin-cost',
        icon: <DollarOutlined />,
        label: 'Cost Management',
        onClick: () => navigate('/admin/cost-management'),
      },
      {
        key: 'admin-performance',
        icon: <ThunderboltOutlined />,
        label: 'Performance',
        onClick: () => navigate('/admin/performance'),
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
