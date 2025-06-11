/**
 * Compact User Panel
 * 
 * A compact user context panel for the header showing user profile and quick access
 */

import React, { useState, useEffect } from 'react';
import { 
  Tooltip, 
  Space, 
  Typography, 
  Button, 
  Modal, 
  Avatar, 
  Tag, 
  Dropdown,
  Badge
} from 'antd';
import {
  UserOutlined,
  SettingOutlined,
  LogoutOutlined,
  BulbOutlined,
  TableOutlined,
  FilterOutlined,
  GlobalOutlined,
  DownOutlined
} from '@ant-design/icons';
import { ApiService, UserContextResponse } from '../../services/api';

const { Text } = Typography;

interface CompactUserPanelProps {
  onLogout?: () => void;
}

export const CompactUserPanel: React.FC<CompactUserPanelProps> = ({ onLogout }) => {
  const [userContext, setUserContext] = useState<UserContextResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showDetails, setShowDetails] = useState(false);

  const loadUserContext = async () => {
    setLoading(true);
    try {
      setError(null);
      const context = await ApiService.getUserContext();
      setUserContext(context);
    } catch (err: any) {
      console.warn('User Context Panel error:', err);
      setError(err.response?.data?.message || 'Failed to load user context');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Delay initial load to allow authentication to settle
    const initialTimeout = setTimeout(() => {
      loadUserContext();
    }, 2000);

    return () => clearTimeout(initialTimeout);
  }, []);

  const getDomainColor = (domain: string) => {
    switch (domain.toLowerCase()) {
      case 'sales': return 'success';
      case 'marketing': return 'blue';
      case 'finance': return 'orange';
      case 'hr': return 'cyan';
      case 'operations': return 'purple';
      default: return 'default';
    }
  };

  const getUserDisplayName = () => {
    // Try to get user name from localStorage or context
    const userName = localStorage.getItem('userName');
    const userRole = localStorage.getItem('userRole');

    // If we have a role, use it to determine display name
    if (userRole === 'admin' || userRole === 'administrator') {
      return 'Administrator';
    }

    // For this BI system, default to Administrator if no specific role is set
    if (!userRole && !userName) {
      return 'Administrator';
    }

    // Otherwise use the stored name or default to User
    return userName || 'User';
  };

  const getProfileSummary = () => {
    if (!userContext) return 'Building AI profile...';
    
    const totalItems = 
      userContext.preferredTables.length + 
      userContext.commonFilters.length + 
      userContext.recentPatterns.length;
    
    if (totalItems === 0) return 'New user - start asking questions!';
    
    return `${totalItems} learned patterns`;
  };

  const dropdownItems = [
    {
      key: 'profile',
      label: (
        <Space>
          <UserOutlined />
          View AI Profile
        </Space>
      ),
      onClick: () => setShowDetails(true)
    },
    {
      key: 'settings',
      label: (
        <Space>
          <SettingOutlined />
          Settings
        </Space>
      ),
      onClick: () => {
        // Navigate to settings page or open settings modal
        console.log('Settings clicked');
      }
    },
    {
      type: 'divider' as const
    },
    {
      key: 'logout',
      label: (
        <Space>
          <LogoutOutlined />
          Logout
        </Space>
      ),
      onClick: onLogout
    }
  ];

  return (
    <>
      <Dropdown
        menu={{ items: dropdownItems }}
        trigger={['click']}
        placement="bottomRight"
      >
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            padding: '6px 12px',
            borderRadius: '4px',
            cursor: 'pointer',
            background: 'rgba(24, 144, 255, 0.1)',
            border: '1px solid #1890ff',
            transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
            backdropFilter: 'blur(8px)',
            height: '36px',
            boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-1px)';
            e.currentTarget.style.boxShadow = '0 4px 12px rgba(24, 144, 255, 0.15)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)';
          }}
        >
          <Avatar 
            size={24} 
            style={{ backgroundColor: '#1890ff' }}
            icon={<UserOutlined />}
          />
          <Text style={{
            fontSize: '13px',
            color: '#1890ff',
            fontWeight: 600,
            fontFamily: "'Inter', sans-serif"
          }}>
            {getUserDisplayName()}
          </Text>
          <DownOutlined style={{ fontSize: '10px', color: '#1890ff' }} />
        </div>
      </Dropdown>

      <Modal
        title={
          <Space>
            <UserOutlined />
            Your AI Profile
          </Space>
        }
        open={showDetails}
        onCancel={() => setShowDetails(false)}
        footer={[
          <Button key="refresh" icon={<BulbOutlined />} onClick={loadUserContext} loading={loading}>
            Refresh Profile
          </Button>,
          <Button key="close" onClick={() => setShowDetails(false)}>
            Close
          </Button>
        ]}
        width={600}
        className="user-profile-modal"
      >
        {loading && (
          <div style={{ textAlign: 'center', padding: '32px' }}>
            <Text>Loading your AI profile...</Text>
          </div>
        )}

        {error && (
          <div style={{ 
            padding: '16px', 
            background: '#fff2f0', 
            border: '1px solid #ffccc7',
            borderRadius: '6px',
            marginBottom: '16px'
          }}>
            <Space>
              <UserOutlined style={{ color: '#ff4d4f' }} />
              <Text type="danger">
                Unable to load AI profile. {error}
              </Text>
            </Space>
          </div>
        )}

        {!loading && !error && userContext && (
          <div style={{ padding: '16px 0' }}>
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              {/* Domain Focus */}
              <div>
                <Text strong>Domain Focus: </Text>
                <Tag color={getDomainColor(userContext.domain)}>
                  <GlobalOutlined style={{ marginRight: '4px' }} />
                  {userContext.domain || 'General'}
                </Tag>
              </div>

              {/* Profile Summary */}
              <div>
                <Text strong>Profile Status: </Text>
                <Text>{getProfileSummary()}</Text>
              </div>

              {/* Preferred Tables */}
              {userContext.preferredTables.length > 0 && (
                <div>
                  <Text strong>Preferred Tables ({userContext.preferredTables.length}): </Text>
                  <div style={{ marginTop: '8px' }}>
                    <Space wrap>
                      {userContext.preferredTables.slice(0, 5).map((table, index) => (
                        <Tag key={index} icon={<TableOutlined />} color="blue">
                          {table}
                        </Tag>
                      ))}
                      {userContext.preferredTables.length > 5 && (
                        <Tag>+{userContext.preferredTables.length - 5} more</Tag>
                      )}
                    </Space>
                  </div>
                </div>
              )}

              {/* Common Filters */}
              {userContext.commonFilters.length > 0 && (
                <div>
                  <Text strong>Common Filters ({userContext.commonFilters.length}): </Text>
                  <div style={{ marginTop: '8px' }}>
                    <Space wrap>
                      {userContext.commonFilters.slice(0, 5).map((filter, index) => (
                        <Tag key={index} icon={<FilterOutlined />} color="purple">
                          {filter}
                        </Tag>
                      ))}
                      {userContext.commonFilters.length > 5 && (
                        <Tag>+{userContext.commonFilters.length - 5} more</Tag>
                      )}
                    </Space>
                  </div>
                </div>
              )}

              {/* Recent Patterns */}
              {userContext.recentPatterns.length > 0 && (
                <div>
                  <Text strong>Query Patterns ({userContext.recentPatterns.length}): </Text>
                  <div style={{ marginTop: '8px' }}>
                    <Space direction="vertical" style={{ width: '100%' }}>
                      {userContext.recentPatterns.slice(0, 3).map((pattern, index) => (
                        <div key={index} style={{ 
                          padding: '8px 12px', 
                          background: '#f5f5f5', 
                          borderRadius: '6px',
                          fontSize: '12px'
                        }}>
                          <Space>
                            <Badge count={pattern.frequency} size="small" />
                            <Text>{pattern.pattern}</Text>
                            <Tag size="small">{pattern.intent}</Tag>
                          </Space>
                        </div>
                      ))}
                      {userContext.recentPatterns.length > 3 && (
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          +{userContext.recentPatterns.length - 3} more patterns
                        </Text>
                      )}
                    </Space>
                  </div>
                </div>
              )}

              {/* Empty State */}
              {userContext.preferredTables.length === 0 &&
               userContext.commonFilters.length === 0 &&
               userContext.recentPatterns.length === 0 && (
                <div style={{ 
                  textAlign: 'center', 
                  padding: '32px',
                  background: '#f9f9f9',
                  borderRadius: '8px'
                }}>
                  <BulbOutlined style={{ fontSize: '32px', color: '#bfbfbf', marginBottom: '16px' }} />
                  <div>
                    <Text strong>Start Building Your AI Profile</Text>
                  </div>
                  <div style={{ marginTop: '8px' }}>
                    <Text type="secondary">
                      Ask questions and execute queries to help AI learn your preferences.
                    </Text>
                  </div>
                </div>
              )}

              {/* Last Updated */}
              {userContext.lastUpdated && (
                <div style={{ 
                  borderTop: '1px solid #f0f0f0', 
                  paddingTop: '16px',
                  fontSize: '12px',
                  color: '#666'
                }}>
                  Last updated: {new Date(userContext.lastUpdated).toLocaleString()}
                </div>
              )}
            </Space>
          </div>
        )}
      </Modal>
    </>
  );
};
