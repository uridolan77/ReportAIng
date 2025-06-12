/**
 * Corner Status Panel
 *
 * Sliding panel from top-right corner with minimize/maximize functionality
 * Shows only bottom-left portion when minimized
 */

import React, { useState, useEffect } from 'react';
import { Dropdown, message } from 'antd';
import {
  ReloadOutlined,
  DatabaseOutlined,
  RobotOutlined,
  UserOutlined,
  SettingOutlined,
  InfoCircleOutlined,
  LogoutOutlined,
  DashboardOutlined
} from '@ant-design/icons';
import { useAuthStore } from '../../stores/authStore';
import { llmManagementService } from '../../services/llmManagementService';
// import { CompactUserPanel } from '../AI/CompactUserPanel'; // Not used anymore

interface CornerStatusPanelProps {
  onLogout?: () => void;
}

interface StatusState {
  isConnected: boolean | null;
  loading: boolean;
  retries: number;
  lastChecked?: Date;
  error?: string;
  details?: any;
}

interface LLMStatusState extends StatusState {
  status: string;
  providers?: any;
}

export const CornerStatusPanel: React.FC<CornerStatusPanelProps> = ({ onLogout }) => {
  const [isExpanded, setIsExpanded] = useState(true);
  const [dbStatus, setDbStatus] = useState<StatusState>({ isConnected: null, loading: true, retries: 0 });
  const [llmStatus, setLlmStatus] = useState<LLMStatusState>({
    isConnected: null,
    status: 'loading',
    loading: true,
    retries: 0
  });
  const { logout, user } = useAuthStore();

  // Enhanced status checking with proper API endpoints
  const checkDatabaseStatus = async () => {
    try {
      setDbStatus(prev => ({ ...prev, loading: true }));

      // Try to fetch the health endpoint
      const response = await fetch('http://localhost:55243/health');

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      // Check if response is JSON
      const contentType = response.headers.get('content-type');
      if (!contentType || !contentType.includes('application/json')) {
        const text = await response.text();
        throw new Error(`Expected JSON, got ${contentType}. Response: ${text.substring(0, 100)}...`);
      }

      const data = await response.json();
      setDbStatus({
        isConnected: data.status === 'Healthy',
        loading: false,
        retries: 0,
        lastChecked: new Date(),
        details: data
      });
    } catch (error) {
      let errorMessage = 'Unknown error';

      if (error instanceof TypeError && error.message.includes('fetch')) {
        errorMessage = 'Backend server not reachable (check if running on port 55243)';
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }

      setDbStatus(prev => ({
        ...prev,
        isConnected: false,
        loading: false,
        retries: prev.retries + 1,
        lastChecked: new Date(),
        error: errorMessage
      }));
    }
  };

  const checkLLMStatus = async () => {
    try {
      setLlmStatus(prev => ({ ...prev, loading: true }));
      const summary = await llmManagementService.getDashboardSummary();

      const hasEnabledProviders = summary.providers.enabled > 0;
      const allHealthy = summary.providers.healthy === summary.providers.total;

      setLlmStatus({
        isConnected: hasEnabledProviders,
        status: allHealthy && hasEnabledProviders ? 'success' : hasEnabledProviders ? 'warning' : 'error',
        loading: false,
        retries: 0,
        lastChecked: new Date(),
        providers: summary.providers,
        details: summary
      });
    } catch (error) {
      setLlmStatus(prev => ({
        ...prev,
        isConnected: false,
        status: 'error',
        loading: false,
        retries: prev.retries + 1,
        lastChecked: new Date(),
        error: error instanceof Error ? error.message : 'Unknown error'
      }));
    }
  };

  const checkAllStatuses = async () => {
    await Promise.all([
      checkDatabaseStatus(),
      checkLLMStatus()
    ]);
  };

  useEffect(() => {
    // Initial check after a short delay
    setTimeout(checkAllStatuses, 1000);

    // Check every 30 seconds
    const interval = setInterval(checkAllStatuses, 30000);
    return () => clearInterval(interval);
  }, []);

  const handleLogout = () => {
    console.log('🚪 Corner panel logout clicked');
    logout();
    if (onLogout) {
      onLogout();
    }
  };

  // Context menu handlers
  const handleRefreshDB = async () => {
    message.loading('Refreshing database status...', 1);
    await checkDatabaseStatus();
    message.success('Database status refreshed');
  };

  const handleRefreshLLM = async () => {
    message.loading('Refreshing LLM status...', 1);
    await checkLLMStatus();
    message.success('LLM status refreshed');
  };

  const handleRefreshAll = async () => {
    message.loading('Refreshing all statuses...', 1);
    await checkAllStatuses();
    message.success('All statuses refreshed');
  };

  // Context menus with proper styling and content
  const getDatabaseMenuItems = () => [
    {
      key: 'status',
      icon: <InfoCircleOutlined />,
      label: (
        <div style={{ minWidth: '200px', padding: '4px 0' }}>
          <div style={{ fontWeight: 600, marginBottom: '4px' }}>Database Status</div>
          <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
            Status: <span style={{
              color: dbStatus.isConnected ? '#52c41a' : '#ff4d4f',
              fontWeight: 500
            }}>
              {dbStatus.isConnected ? 'Connected' : 'Disconnected'}
            </span>
          </div>
          {dbStatus.lastChecked && (
            <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
              Last checked: {dbStatus.lastChecked.toLocaleTimeString()}
            </div>
          )}
          {dbStatus.error && (
            <div style={{ fontSize: '12px', color: '#ff4d4f', marginTop: '4px' }}>
              Error: {dbStatus.error}
            </div>
          )}
        </div>
      ),
      disabled: true,
    },
    { type: 'divider' as const },
    {
      key: 'refresh',
      icon: <ReloadOutlined />,
      label: 'Refresh Status',
      onClick: handleRefreshDB,
    },
    {
      key: 'admin',
      icon: <SettingOutlined />,
      label: 'Database Admin',
      onClick: () => window.open('/admin/database', '_blank'),
    },
  ];

  const getLLMMenuItems = () => [
    {
      key: 'status',
      icon: <InfoCircleOutlined />,
      label: (
        <div style={{ minWidth: '220px', padding: '4px 0' }}>
          <div style={{ fontWeight: 600, marginBottom: '4px' }}>LLM System Status</div>
          <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
            Providers: <span style={{
              color: (llmStatus.providers?.healthy || 0) > 0 ? '#52c41a' : '#ff4d4f',
              fontWeight: 500
            }}>
              {llmStatus.providers?.healthy || 0}/{llmStatus.providers?.total || 0} healthy
            </span>
          </div>
          <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
            Enabled: <span style={{
              color: (llmStatus.providers?.enabled || 0) > 0 ? '#52c41a' : '#ff4d4f',
              fontWeight: 500
            }}>
              {llmStatus.providers?.enabled || 0}
            </span>
          </div>
          {llmStatus.lastChecked && (
            <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
              Last checked: {llmStatus.lastChecked.toLocaleTimeString()}
            </div>
          )}
          {llmStatus.error && (
            <div style={{ fontSize: '12px', color: '#ff4d4f', marginTop: '4px' }}>
              Error: {llmStatus.error}
            </div>
          )}
        </div>
      ),
      disabled: true,
    },
    { type: 'divider' as const },
    {
      key: 'refresh',
      icon: <ReloadOutlined />,
      label: 'Refresh Status',
      onClick: handleRefreshLLM,
    },
    {
      key: 'admin',
      icon: <SettingOutlined />,
      label: 'LLM Management',
      onClick: () => window.open('/admin/llm', '_blank'),
    },
    {
      key: 'dashboard',
      icon: <DashboardOutlined />,
      label: 'LLM Dashboard',
      onClick: () => window.open('/admin/llm/dashboard', '_blank'),
    },
  ];

  const getUserMenuItems = () => [
    {
      key: 'user-info',
      icon: <UserOutlined />,
      label: (
        <div style={{ minWidth: '180px', padding: '4px 0' }}>
          <div style={{ fontWeight: 600, marginBottom: '4px', color: '#1890ff' }}>
            {user?.username || 'User'}
          </div>
          <div style={{ fontSize: '12px', color: '#666', marginBottom: '2px' }}>
            Role: <span style={{ fontWeight: 500 }}>{user?.roles?.[0] || 'Unknown'}</span>
          </div>
          <div style={{ fontSize: '12px', color: '#666' }}>
            Session: Active
          </div>
        </div>
      ),
      disabled: true,
    },
    { type: 'divider' as const },
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: 'My Profile',
      onClick: () => {
        message.info('Profile page coming soon');
        // window.open('/profile', '_blank');
      },
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: 'Account Settings',
      onClick: () => {
        message.info('Settings page coming soon');
        // window.open('/settings', '_blank');
      },
    },
    { type: 'divider' as const },
    {
      key: 'refresh-all',
      icon: <ReloadOutlined />,
      label: 'Refresh All Status',
      onClick: handleRefreshAll,
    },
    { type: 'divider' as const },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Sign Out',
      onClick: handleLogout,
      danger: true,
    },
  ];

  return (
    <div
      className="corner-status-panel"
      style={{
        position: 'fixed',
        top: '0',
        right: isExpanded ? '0' : '10px',
        zIndex: 1050,
        width: isExpanded ? '500px' : '140px',
        height: '60px',
        background: '#ffffff',
        borderRadius: isExpanded ? '0 0 0 6px' : '0 0 0 4px',
        boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06), 0 1px 2px rgba(0, 0, 0, 0.04)',
        border: '1px solid #e8e8e8',
        borderTop: 'none',
        borderRight: 'none',
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        overflow: 'hidden',
        fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
      }}
    >
      {/* Toggle Arrow - No Background */}
      <div
        onClick={() => setIsExpanded(!isExpanded)}
        style={{
          position: 'absolute',
          top: '50%',
          left: '12px',
          transform: 'translateY(-50%)',
          width: '16px',
          height: '16px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          cursor: 'pointer',
          transition: 'all 0.2s ease',
          fontSize: '14px',
          color: '#666',
          zIndex: 10,
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.color = '#1890ff';
          e.currentTarget.style.transform = 'translateY(-50%) scale(1.2)';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.color = '#666';
          e.currentTarget.style.transform = 'translateY(-50%) scale(1)';
        }}
      >
        {isExpanded ? '→' : '←'}
      </div>

      {/* Professional Content Layout */}
      <div
        style={{
          padding: '0 16px',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          transition: 'all 0.3s ease',
        }}
      >
        {/* Single Line Layout - Perfectly Aligned */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: isExpanded ? '8px' : '12px',
            justifyContent: 'flex-end',
            height: '100%',
            paddingRight: isExpanded ? '40px' : '20px', // Space for toggle button
            paddingLeft: isExpanded ? '40px' : '30px', // Space for toggle button
          }}
        >
          {isExpanded ? (
            <>
              {/* Database Status with Context Menu */}
              <div style={{
                width: '130px',
                height: '32px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}>
                <Dropdown
                  menu={{ items: getDatabaseMenuItems() }}
                  trigger={['hover']}
                  placement="bottomLeft"
                  overlayStyle={{ minWidth: '240px' }}
                >
                  <div
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '6px',
                      padding: '6px 12px',
                      borderRadius: '4px',
                      cursor: 'pointer',
                      background: dbStatus.isConnected === null ? 'rgba(209, 213, 219, 0.1)' :
                                 dbStatus.isConnected ? 'rgba(34, 197, 94, 0.1)' : 'rgba(239, 68, 68, 0.1)',
                      border: `1px solid ${dbStatus.isConnected === null ? '#d1d5db' :
                                          dbStatus.isConnected ? '#22c55e' : '#ef4444'}`,
                      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                      backdropFilter: 'blur(8px)',
                      height: '32px',
                      width: '120px',
                      boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
                    }}
                    onDoubleClick={handleRefreshDB}
                    title={`Database: ${dbStatus.isConnected === null ? 'Connecting...' :
                                       dbStatus.isConnected ? 'Connected' : 'Offline'} • Hover for options • Double-click to refresh`}
                  >
                    <DatabaseOutlined style={{
                      fontSize: '12px',
                      animation: dbStatus.loading ? 'spin 2s linear infinite' : 'none',
                    }} />
                    <span style={{
                      fontSize: '11px',
                      color: dbStatus.isConnected === null ? '#6b7280' :
                             dbStatus.isConnected ? '#22c55e' : '#ef4444',
                      fontWeight: 600,
                      fontFamily: "'Inter', sans-serif"
                    }}>
                      {dbStatus.isConnected === null ? 'DB Connecting...' :
                       dbStatus.isConnected ? 'DB Connected' : 'DB Offline'}
                    </span>
                  </div>
                </Dropdown>
              </div>
              {/* LLM Status with Context Menu */}
              <div style={{
                width: '130px',
                height: '32px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}>
                <Dropdown
                  menu={{ items: getLLMMenuItems() }}
                  trigger={['hover']}
                  placement="bottomLeft"
                  overlayStyle={{ minWidth: '260px' }}
                >
                  <div
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '6px',
                      padding: '6px 12px',
                      borderRadius: '4px',
                      cursor: 'pointer',
                      background: llmStatus.isConnected === null ? 'rgba(209, 213, 219, 0.1)' :
                                 llmStatus.isConnected ?
                                   (llmStatus.status === 'success' ? 'rgba(34, 197, 94, 0.1)' : 'rgba(250, 173, 20, 0.1)') :
                                   'rgba(239, 68, 68, 0.1)',
                      border: `1px solid ${llmStatus.isConnected === null ? '#d1d5db' :
                                          llmStatus.isConnected ?
                                            (llmStatus.status === 'success' ? '#22c55e' : '#faad14') : '#ef4444'}`,
                      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                      backdropFilter: 'blur(8px)',
                      height: '32px',
                      width: '120px',
                      boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
                    }}
                    onDoubleClick={handleRefreshLLM}
                    title={`LLM: ${llmStatus.isConnected === null ? 'Connecting...' :
                                   llmStatus.isConnected ? 'Ready' : 'Offline'} • Hover for options • Double-click to refresh`}
                  >
                    <RobotOutlined style={{
                      fontSize: '12px',
                      animation: llmStatus.loading ? 'spin 2s linear infinite' : 'none',
                    }} />
                    <span style={{
                      fontSize: '11px',
                      color: llmStatus.isConnected === null ? '#6b7280' :
                             llmStatus.isConnected ?
                               (llmStatus.status === 'success' ? '#22c55e' : '#faad14') : '#ef4444',
                      fontWeight: 600,
                      fontFamily: "'Inter', sans-serif"
                    }}>
                      {llmStatus.isConnected === null ? 'LLM Connecting...' :
                       llmStatus.isConnected ? 'LLM Ready' : 'LLM Offline'}
                    </span>
                  </div>
                </Dropdown>
              </div>
              {/* User Panel with Context Menu */}
              <div style={{
                width: '130px',
                height: '32px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}>
                <Dropdown
                  menu={{ items: getUserMenuItems() }}
                  trigger={['hover']}
                  placement="bottomLeft"
                  overlayStyle={{ minWidth: '200px' }}
                >
                  <div
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '6px',
                      padding: '6px 12px',
                      borderRadius: '4px',
                      cursor: 'pointer',
                      background: 'rgba(24, 144, 255, 0.1)',
                      border: '1px solid #1890ff',
                      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                      backdropFilter: 'blur(8px)',
                      height: '32px',
                      width: '120px',
                      boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
                    }}
                    onDoubleClick={handleRefreshAll}
                    title={`User: ${user?.username || 'User'} • Hover for user menu • Double-click to refresh all`}
                  >
                    <UserOutlined style={{
                      fontSize: '12px',
                      color: '#1890ff',
                    }} />
                    <span style={{
                      fontSize: '11px',
                      color: '#1890ff',
                      fontWeight: 600,
                      fontFamily: "'Inter', sans-serif"
                    }}>
                      {user?.username || 'User'}
                    </span>
                  </div>
                </Dropdown>
              </div>
            </>
          ) : (
            /* Collapsed State - Status-Based Animated Icons with Context Menus */
            <>
              {/* DB Status Icon */}
              <Dropdown
                menu={{ items: getDatabaseMenuItems() }}
                trigger={['hover']}
                placement="bottomLeft"
                overlayStyle={{ minWidth: '240px' }}
              >
                <div
                  style={{
                    width: '20px',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    cursor: 'pointer',
                    transition: 'all 0.3s ease',
                  }}
                  title={`Database: ${dbStatus.isConnected === null ? 'Connecting...' :
                                     dbStatus.isConnected ? 'Connected' : 'Offline'} (Hover for options)`}
                  onDoubleClick={handleRefreshDB}
                >
                  <DatabaseOutlined style={{
                    fontSize: '16px',
                    color: dbStatus.isConnected === null ? '#6b7280' :
                           dbStatus.isConnected ? '#22c55e' : '#ef4444',
                    animation: dbStatus.loading ? 'spin 2s linear infinite' : 'none',
                  }} />
                </div>
              </Dropdown>

              {/* LLM Status Icon */}
              <Dropdown
                menu={{ items: getLLMMenuItems() }}
                trigger={['hover']}
                placement="bottomLeft"
                overlayStyle={{ minWidth: '260px' }}
              >
                <div
                  style={{
                    width: '20px',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    cursor: 'pointer',
                    transition: 'all 0.3s ease',
                  }}
                  title={`LLM: ${llmStatus.isConnected === null ? 'Connecting...' :
                                 llmStatus.isConnected ? 'Ready' : 'Offline'} (Hover for options)`}
                  onDoubleClick={handleRefreshLLM}
                >
                  <RobotOutlined style={{
                    fontSize: '16px',
                    color: llmStatus.isConnected === null ? '#6b7280' :
                           llmStatus.isConnected ?
                             (llmStatus.status === 'success' ? '#22c55e' : '#faad14') : '#ef4444',
                    animation: llmStatus.loading ? 'spin 2s linear infinite' : 'none',
                  }} />
                </div>
              </Dropdown>

              {/* User Status Icon */}
              <Dropdown
                menu={{ items: getUserMenuItems() }}
                trigger={['hover']}
                placement="bottomLeft"
                overlayStyle={{ minWidth: '200px' }}
              >
                <div
                  style={{
                    width: '20px',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    cursor: 'pointer',
                    transition: 'all 0.3s ease',
                  }}
                  title="User Panel - Hover for user menu"
                  onDoubleClick={handleRefreshAll}
                >
                  <UserOutlined style={{
                    fontSize: '16px',
                    color: '#1890ff',
                  }} />
                </div>
              </Dropdown>
            </>
          )}
        </div>
      </div>

      {/* Enhanced CSS for corner panel and dropdowns */}
      <style>{`
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }

        /* Enhanced dropdown styling */
        .ant-dropdown {
          z-index: 1060 !important;
        }

        .ant-dropdown-menu {
          border-radius: 8px !important;
          box-shadow: 0 6px 16px rgba(0, 0, 0, 0.12), 0 3px 6px rgba(0, 0, 0, 0.08) !important;
          border: 1px solid #e8e8e8 !important;
          padding: 4px 0 !important;
        }

        .ant-dropdown-menu-item {
          padding: 8px 16px !important;
          margin: 0 4px !important;
          border-radius: 4px !important;
          transition: all 0.2s ease !important;
        }

        .ant-dropdown-menu-item:hover {
          background-color: #f5f5f5 !important;
        }

        .ant-dropdown-menu-item-disabled {
          cursor: default !important;
          background-color: transparent !important;
        }

        .ant-dropdown-menu-item-disabled:hover {
          background-color: transparent !important;
        }

        .ant-dropdown-menu-item-danger {
          color: #ff4d4f !important;
        }

        .ant-dropdown-menu-item-danger:hover {
          background-color: #fff2f0 !important;
          color: #ff4d4f !important;
        }

        .ant-dropdown-menu-item-divider {
          margin: 4px 8px !important;
          background-color: #f0f0f0 !important;
        }

        /* Corner panel hover effects */
        .corner-status-panel [data-tooltip]:hover {
          transform: translateY(-1px);
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15) !important;
        }
      `}</style>
    </div>
  );
};
