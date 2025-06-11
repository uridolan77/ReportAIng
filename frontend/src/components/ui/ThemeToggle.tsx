/**
 * Theme Toggle Component
 * Provides UI controls for switching between light, dark, and auto themes
 */

import React, { useState } from 'react';
import { Button, Dropdown, Tooltip, Space } from 'antd';
import type { MenuProps } from 'antd';
import {
  SunOutlined,
  MoonOutlined,
  DesktopOutlined,
  BulbOutlined,
  CheckOutlined
} from '@ant-design/icons';
import { useTheme } from '../../contexts/ThemeContext';

export interface ThemeToggleProps {
  size?: 'small' | 'middle' | 'large';
  type?: 'button' | 'dropdown' | 'switch';
  showLabel?: boolean;
  placement?: 'topLeft' | 'topCenter' | 'topRight' | 'bottomLeft' | 'bottomCenter' | 'bottomRight';
  className?: string;
  style?: React.CSSProperties;
}

export const ThemeToggle: React.FC<ThemeToggleProps> = ({
  size = 'middle',
  type = 'dropdown',
  showLabel = false,
  placement = 'bottomRight',
  className,
  style
}) => {
  const { themeMode, isDarkMode, setThemeMode, toggleTheme } = useTheme();
  const [dropdownOpen, setDropdownOpen] = useState(false);

  // Get current theme icon and label
  const getCurrentThemeInfo = () => {
    switch (themeMode) {
      case 'light':
        return {
          icon: <SunOutlined />,
          label: 'Light',
          description: 'Light theme'
        };
      case 'dark':
        return {
          icon: <MoonOutlined />,
          label: 'Dark',
          description: 'Dark theme'
        };
      case 'auto':
        return {
          icon: <DesktopOutlined />,
          label: 'Auto',
          description: `Auto (${isDarkMode ? 'Dark' : 'Light'})`
        };
      default:
        return {
          icon: <BulbOutlined />,
          label: 'Theme',
          description: 'Theme'
        };
    }
  };

  const currentTheme = getCurrentThemeInfo();

  // Dropdown menu items
  const menuItems: MenuProps['items'] = [
    {
      key: 'light',
      icon: <SunOutlined />,
      label: (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: '120px' }}>
          <span>Light</span>
          {themeMode === 'light' && <CheckOutlined style={{ color: 'var(--color-primary)' }} />}
        </div>
      ),
      onClick: () => setThemeMode('light')
    },
    {
      key: 'dark',
      icon: <MoonOutlined />,
      label: (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: '120px' }}>
          <span>Dark</span>
          {themeMode === 'dark' && <CheckOutlined style={{ color: 'var(--color-primary)' }} />}
        </div>
      ),
      onClick: () => setThemeMode('dark')
    },
    {
      key: 'auto',
      icon: <DesktopOutlined />,
      label: (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', minWidth: '120px' }}>
          <div>
            <div>Auto</div>
            <div style={{ fontSize: '12px', color: 'var(--text-tertiary)' }}>
              System ({isDarkMode ? 'Dark' : 'Light'})
            </div>
          </div>
          {themeMode === 'auto' && <CheckOutlined style={{ color: 'var(--color-primary)' }} />}
        </div>
      ),
      onClick: () => setThemeMode('auto')
    }
  ];

  // Simple toggle button (cycles through themes)
  if (type === 'button') {
    return (
      <Tooltip title={`Current: ${currentTheme.description}. Click to cycle themes.`}>
        <Button
          icon={currentTheme.icon}
          onClick={toggleTheme}
          size={size}
          className={className}
          style={style}
        >
          {showLabel && currentTheme.label}
        </Button>
      </Tooltip>
    );
  }

  // Switch-style toggle (only light/dark)
  if (type === 'switch') {
    return (
      <div 
        className={`theme-switch ${className || ''}`}
        style={{
          display: 'inline-flex',
          alignItems: 'center',
          gap: 'var(--space-2)',
          padding: 'var(--space-2)',
          borderRadius: 'var(--radius-full)',
          background: 'var(--bg-tertiary)',
          border: '1px solid var(--border-primary)',
          cursor: 'pointer',
          transition: 'all var(--transition-fast)',
          ...style
        }}
        onClick={() => setThemeMode(themeMode === 'dark' ? 'light' : 'dark')}
      >
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: '24px',
            height: '24px',
            borderRadius: '50%',
            background: themeMode === 'light' ? 'var(--color-warning)' : 'transparent',
            color: themeMode === 'light' ? 'white' : 'var(--text-tertiary)',
            transition: 'all var(--transition-fast)'
          }}
        >
          <SunOutlined style={{ fontSize: '14px' }} />
        </div>
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: '24px',
            height: '24px',
            borderRadius: '50%',
            background: themeMode === 'dark' ? 'var(--color-primary)' : 'transparent',
            color: themeMode === 'dark' ? 'white' : 'var(--text-tertiary)',
            transition: 'all var(--transition-fast)'
          }}
        >
          <MoonOutlined style={{ fontSize: '14px' }} />
        </div>
        {showLabel && (
          <span style={{ 
            fontSize: 'var(--text-sm)', 
            color: 'var(--text-secondary)',
            marginLeft: 'var(--space-1)'
          }}>
            {currentTheme.label}
          </span>
        )}
      </div>
    );
  }

  // Dropdown (default)
  return (
    <Dropdown
      menu={{ items: menuItems }}
      placement={placement}
      trigger={['click']}
      open={dropdownOpen}
      onOpenChange={setDropdownOpen}
    >
      <Button
        icon={currentTheme.icon}
        size={size}
        className={className}
        style={style}
      >
        {showLabel && (
          <Space>
            {currentTheme.label}
            {themeMode === 'auto' && (
              <span style={{ 
                fontSize: '12px', 
                color: 'var(--text-tertiary)',
                fontWeight: 'normal'
              }}>
                ({isDarkMode ? 'Dark' : 'Light'})
              </span>
            )}
          </Space>
        )}
      </Button>
    </Dropdown>
  );
};

// Compact theme toggle for headers/toolbars
export const CompactThemeToggle: React.FC<Omit<ThemeToggleProps, 'type' | 'showLabel'>> = (props) => {
  return <ThemeToggle {...props} type="button" showLabel={false} size="small" />;
};

// Theme toggle with label for settings pages
export const LabeledThemeToggle: React.FC<Omit<ThemeToggleProps, 'showLabel'>> = (props) => {
  return <ThemeToggle {...props} showLabel={true} />;
};

// Switch-style theme toggle
export const ThemeSwitch: React.FC<Omit<ThemeToggleProps, 'type'>> = (props) => {
  return <ThemeToggle {...props} type="switch" />;
};

// Theme status indicator (read-only)
export const ThemeIndicator: React.FC<{ showSystemPreference?: boolean }> = ({ 
  showSystemPreference = false 
}) => {
  const { themeMode, isDarkMode } = useTheme();
  const currentTheme = getCurrentThemeInfo();

  function getCurrentThemeInfo() {
    switch (themeMode) {
      case 'light':
        return { icon: <SunOutlined />, label: 'Light', color: 'var(--color-warning)' };
      case 'dark':
        return { icon: <MoonOutlined />, label: 'Dark', color: 'var(--color-primary)' };
      case 'auto':
        return { 
          icon: <DesktopOutlined />, 
          label: `Auto (${isDarkMode ? 'Dark' : 'Light'})`, 
          color: 'var(--color-info)' 
        };
      default:
        return { icon: <BulbOutlined />, label: 'Unknown', color: 'var(--text-tertiary)' };
    }
  }

  return (
    <div style={{
      display: 'inline-flex',
      alignItems: 'center',
      gap: 'var(--space-2)',
      padding: 'var(--space-2) var(--space-3)',
      borderRadius: 'var(--radius-md)',
      background: 'var(--bg-tertiary)',
      border: '1px solid var(--border-primary)',
      fontSize: 'var(--text-sm)',
      color: 'var(--text-secondary)'
    }}>
      <span style={{ color: currentTheme.color }}>
        {currentTheme.icon}
      </span>
      <span>{currentTheme.label}</span>
      {showSystemPreference && themeMode === 'auto' && (
        <span style={{ 
          fontSize: 'var(--text-xs)', 
          color: 'var(--text-tertiary)',
          marginLeft: 'var(--space-1)'
        }}>
          (System)
        </span>
      )}
    </div>
  );
};
