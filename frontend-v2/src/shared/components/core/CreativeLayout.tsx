import React, { useState, useEffect } from 'react'
import { Button, Typography, Space, Avatar, Dropdown, Badge, Tooltip } from 'antd'
import {
  MessageOutlined,
  HistoryOutlined,
  DatabaseOutlined,
  SettingOutlined,
  UserOutlined,
  LogoutOutlined,
  BellOutlined,
  SearchOutlined,
  PlusOutlined,
  ThunderboltOutlined,
  StarOutlined
} from '@ant-design/icons'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import { selectUser, authActions } from '@shared/store/auth'

const { Title, Text } = Typography

interface CreativeLayoutProps {
  children: React.ReactNode
  currentPath?: string
}

export const CreativeLayout: React.FC<CreativeLayoutProps> = ({ 
  children, 
  currentPath = '/' 
}) => {
  const [isFloating, setIsFloating] = useState(false)
  const [activeTab, setActiveTab] = useState(currentPath)
  const dispatch = useAppDispatch()
  const user = useAppSelector(selectUser)

  // Floating navigation on scroll
  useEffect(() => {
    const handleScroll = () => {
      setIsFloating(window.scrollY > 100)
    }
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  const navigationItems = [
    { key: '/', icon: MessageOutlined, label: 'Chat', color: '#3b82f6', gradient: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)' },
    { key: '/history', icon: HistoryOutlined, label: 'History', color: '#10b981', gradient: 'linear-gradient(135deg, #10b981 0%, #059669 100%)' },
    { key: '/explorer', icon: DatabaseOutlined, label: 'Explorer', color: '#f59e0b', gradient: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)' },
    { key: '/insights', icon: ThunderboltOutlined, label: 'Insights', color: '#8b5cf6', gradient: 'linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%)' },
    { key: '/favorites', icon: StarOutlined, label: 'Favorites', color: '#ef4444', gradient: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)' }
  ]

  const userMenuItems = [
    { key: 'profile', icon: <UserOutlined />, label: 'Profile' },
    { key: 'settings', icon: <SettingOutlined />, label: 'Settings' },
    { type: 'divider' as const },
    { key: 'logout', icon: <LogoutOutlined />, label: 'Logout', onClick: () => dispatch(authActions.logout()) }
  ]

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      position: 'relative',
      overflow: 'hidden'
    }}>
      {/* Animated Background Elements */}
      <div style={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: `
          radial-gradient(circle at 20% 80%, rgba(120, 119, 198, 0.3) 0%, transparent 50%),
          radial-gradient(circle at 80% 20%, rgba(255, 119, 198, 0.3) 0%, transparent 50%),
          radial-gradient(circle at 40% 40%, rgba(120, 219, 255, 0.3) 0%, transparent 50%)
        `,
        animation: 'float 20s ease-in-out infinite'
      }} />

      {/* Floating Navigation Bar */}
      <div style={{
        position: 'fixed',
        top: isFloating ? '20px' : '40px',
        left: '50%',
        transform: 'translateX(-50%)',
        zIndex: 1000,
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        background: isFloating 
          ? 'rgba(255, 255, 255, 0.95)' 
          : 'rgba(255, 255, 255, 0.1)',
        backdropFilter: 'blur(20px)',
        borderRadius: '24px',
        padding: '12px 20px',
        border: '1px solid rgba(255, 255, 255, 0.2)',
        boxShadow: isFloating 
          ? '0 20px 40px rgba(0, 0, 0, 0.1)' 
          : '0 8px 32px rgba(0, 0, 0, 0.1)'
      }}>
        <Space size="large" align="center">
          {/* Logo */}
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: '12px'
          }}>
            <div style={{
              width: '40px',
              height: '40px',
              borderRadius: '12px',
              background: 'linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'white',
              fontWeight: 'bold',
              fontSize: '18px',
              boxShadow: '0 8px 16px rgba(255, 107, 107, 0.3)'
            }}>
              🚀
            </div>
            <Title level={4} style={{ 
              margin: 0, 
              color: isFloating ? '#1f2937' : 'white',
              fontWeight: 700
            }}>
              ReportAI
            </Title>
          </div>

          {/* Navigation Pills */}
          <div style={{
            display: 'flex',
            gap: '8px',
            background: 'rgba(255, 255, 255, 0.1)',
            borderRadius: '16px',
            padding: '6px'
          }}>
            {navigationItems.map((item) => {
              const Icon = item.icon
              const isActive = activeTab === item.key
              
              return (
                <Tooltip key={item.key} title={item.label}>
                  <Button
                    type="text"
                    onClick={() => setActiveTab(item.key)}
                    style={{
                      width: '48px',
                      height: '48px',
                      borderRadius: '12px',
                      border: 'none',
                      background: isActive ? item.gradient : 'transparent',
                      color: isActive ? 'white' : (isFloating ? '#6b7280' : 'rgba(255, 255, 255, 0.8)'),
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      transition: 'all 0.3s ease',
                      transform: isActive ? 'scale(1.1)' : 'scale(1)',
                      boxShadow: isActive ? `0 8px 16px ${item.color}40` : 'none'
                    }}
                    onMouseEnter={(e) => {
                      if (!isActive) {
                        e.currentTarget.style.background = 'rgba(255, 255, 255, 0.2)'
                        e.currentTarget.style.transform = 'scale(1.05)'
                      }
                    }}
                    onMouseLeave={(e) => {
                      if (!isActive) {
                        e.currentTarget.style.background = 'transparent'
                        e.currentTarget.style.transform = 'scale(1)'
                      }
                    }}
                  >
                    <Icon style={{ fontSize: '20px' }} />
                  </Button>
                </Tooltip>
              )
            })}
          </div>

          {/* Quick Actions */}
          <Space>
            <Tooltip title="Quick Search">
              <Button
                type="text"
                icon={<SearchOutlined />}
                style={{
                  width: '40px',
                  height: '40px',
                  borderRadius: '12px',
                  color: isFloating ? '#6b7280' : 'rgba(255, 255, 255, 0.8)',
                  background: 'rgba(255, 255, 255, 0.1)'
                }}
              />
            </Tooltip>
            
            <Badge count={3} size="small">
              <Tooltip title="Notifications">
                <Button
                  type="text"
                  icon={<BellOutlined />}
                  style={{
                    width: '40px',
                    height: '40px',
                    borderRadius: '12px',
                    color: isFloating ? '#6b7280' : 'rgba(255, 255, 255, 0.8)',
                    background: 'rgba(255, 255, 255, 0.1)'
                  }}
                />
              </Tooltip>
            </Badge>

            {/* User Menu */}
            <Dropdown
              menu={{ items: userMenuItems }}
              placement="bottomRight"
              trigger={['click']}
            >
              <div style={{
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                padding: '6px 12px',
                borderRadius: '12px',
                background: 'rgba(255, 255, 255, 0.1)',
                cursor: 'pointer',
                transition: 'all 0.2s ease'
              }}>
                <Avatar
                  size={32}
                  icon={<UserOutlined />}
                  style={{ 
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    border: '2px solid rgba(255, 255, 255, 0.3)'
                  }}
                />
                <Text style={{ 
                  color: isFloating ? '#1f2937' : 'white',
                  fontWeight: 500,
                  fontSize: '14px'
                }}>
                  {user?.name || 'User'}
                </Text>
              </div>
            </Dropdown>
          </Space>
        </Space>
      </div>

      {/* Floating Action Button */}
      <Button
        type="primary"
        shape="circle"
        size="large"
        icon={<PlusOutlined />}
        style={{
          position: 'fixed',
          bottom: '30px',
          right: '30px',
          width: '60px',
          height: '60px',
          background: 'linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%)',
          border: 'none',
          boxShadow: '0 8px 24px rgba(255, 107, 107, 0.4)',
          zIndex: 1000,
          fontSize: '24px'
        }}
      />

      {/* Main Content Area */}
      <div style={{
        paddingTop: '140px',
        paddingBottom: '40px',
        paddingLeft: '40px',
        paddingRight: '40px',
        minHeight: '100vh',
        position: 'relative',
        zIndex: 1
      }}>
        <div style={{
          background: 'rgba(255, 255, 255, 0.95)',
          backdropFilter: 'blur(20px)',
          borderRadius: '24px',
          minHeight: 'calc(100vh - 180px)',
          border: '1px solid rgba(255, 255, 255, 0.2)',
          boxShadow: '0 20px 40px rgba(0, 0, 0, 0.1)',
          overflow: 'hidden'
        }}>
          {children}
        </div>
      </div>

      {/* CSS Animations */}
      <style jsx>{`
        @keyframes float {
          0%, 100% { transform: translateY(0px) rotate(0deg); }
          33% { transform: translateY(-20px) rotate(1deg); }
          66% { transform: translateY(-10px) rotate(-1deg); }
        }
        
        @keyframes pulse {
          0%, 100% { opacity: 0.8; }
          50% { opacity: 1; }
        }
      `}</style>
    </div>
  )
}
