/**
 * Corner Status Panel
 *
 * Sliding panel from top-right corner with minimize/maximize functionality
 * Shows only bottom-left portion when minimized
 */

import React, { useState } from 'react';
import { Space, Button } from 'antd';
import {
  ExpandAltOutlined,
  CompressOutlined,
  DownOutlined,
  UpOutlined
} from '@ant-design/icons';
import { DatabaseStatusIndicator } from './DatabaseStatusIndicator';
import { CompactLLMIndicator } from '../AI/CompactLLMIndicator';
import { CompactUserPanel } from '../AI/CompactUserPanel';

interface CornerStatusPanelProps {
  onLogout?: () => void;
}

export const CornerStatusPanel: React.FC<CornerStatusPanelProps> = ({ onLogout }) => {
  const [isExpanded, setIsExpanded] = useState(true);

  const handleLogout = () => {
    // Clear authentication data
    localStorage.removeItem('token');
    localStorage.removeItem('userName');
    localStorage.removeItem('userRole');

    // Call parent logout handler if provided
    if (onLogout) {
      onLogout();
    } else {
      // Default logout behavior - reload page to trigger auth check
      window.location.reload();
    }
  };

  return (
    <div
      className="corner-status-panel"
      style={{
        position: 'fixed',
        top: isExpanded ? '0' : '-15px',
        right: isExpanded ? '0' : '-30px',
        zIndex: 1050,
        width: isExpanded ? '360px' : '100px',
        height: isExpanded ? '140px' : '50px',
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
      {/* Professional Toggle Handle */}
      <div
        onClick={() => setIsExpanded(!isExpanded)}
        style={{
          position: 'absolute',
          bottom: isExpanded ? '12px' : '6px',
          left: isExpanded ? '12px' : '6px',
          width: '20px',
          height: '20px',
          borderRadius: '3px',
          background: '#f5f5f5',
          border: '1px solid #d9d9d9',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          cursor: 'pointer',
          transition: 'all 0.2s ease',
          fontSize: '12px',
          color: '#666',
          zIndex: 10,
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.background = '#e6f7ff';
          e.currentTarget.style.borderColor = '#91d5ff';
          e.currentTarget.style.color = '#1890ff';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.background = '#f5f5f5';
          e.currentTarget.style.borderColor = '#d9d9d9';
          e.currentTarget.style.color = '#666';
        }}
      >
        {isExpanded ? '−' : '+'}
      </div>

      {/* Professional Content Layout */}
      <div
        style={{
          padding: isExpanded ? '12px 12px 32px 32px' : '10px 10px 26px 26px',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          transition: 'all 0.3s ease',
        }}
      >
        {isExpanded ? (
          <>
            {/* Status Indicators Row */}
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                marginBottom: '12px',
              }}
            >
              <DatabaseStatusIndicator />
              <CompactLLMIndicator />
            </div>

            {/* User Panel */}
            <div
              style={{
                borderTop: '1px solid #f0f0f0',
                paddingTop: '12px',
              }}
            >
              <CompactUserPanel onLogout={handleLogout} />
            </div>
          </>
        ) : (
          /* Minimized State - Only Dot Indicators */
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              justifyContent: 'center',
            }}
          >
            {/* DB Status Dot */}
            <div
              style={{
                width: '12px',
                height: '12px',
                borderRadius: '50%',
                background: '#52c41a', // Green for connected, will be dynamic
                border: '2px solid #fff',
                boxShadow: '0 0 0 1px #52c41a',
                cursor: 'pointer',
                transition: 'all 0.3s ease',
              }}
              title="Database: Connected"
              onClick={() => setIsExpanded(true)}
            />

            {/* LLM Status Dot */}
            <div
              style={{
                width: '12px',
                height: '12px',
                borderRadius: '50%',
                background: '#faad14', // Orange for ready, will be dynamic
                border: '2px solid #fff',
                boxShadow: '0 0 0 1px #faad14',
                cursor: 'pointer',
                transition: 'all 0.3s ease',
              }}
              title="LLM: Ready"
              onClick={() => setIsExpanded(true)}
            />

            {/* User Dot */}
            <div
              style={{
                width: '12px',
                height: '12px',
                borderRadius: '50%',
                background: '#1890ff',
                border: '2px solid #fff',
                boxShadow: '0 0 0 1px #1890ff',
                cursor: 'pointer',
                transition: 'all 0.3s ease',
              }}
              title="User Panel - Click to expand"
              onClick={() => setIsExpanded(true)}
            />
          </div>
        )}
      </div>
    </div>
  );
};
