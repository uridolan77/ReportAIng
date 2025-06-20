import React from 'react'
import { Layout as AntLayout, Menu, Avatar, Dropdown, Space, Typography, Button } from 'antd'
import { UserOutlined, LogoutOutlined, SettingOutlined, MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAppSelector, useAppDispatch } from '../../hooks'
import { selectUser, selectIsAdmin, authActions } from '../../store/auth'
import { selectSidebarCollapsed, uiActions } from '../../store/ui'

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
    if (path.startsWith('/chat')) return ['chat']
    if (path.startsWith('/admin')) return ['admin']
    return []
  }

  const menuItems = [
    {
      key: 'chat',
      icon: <UserOutlined />,
      label: 'Chat Interface',
      onClick: () => navigate('/chat'),
    },
    ...(isAdmin ? [
      {
        key: 'admin',
        icon: <SettingOutlined />,
        label: 'Admin Dashboard',
        onClick: () => navigate('/admin'),
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
