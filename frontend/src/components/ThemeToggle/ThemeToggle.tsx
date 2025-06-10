import React from 'react';
import { Button, Tooltip } from 'antd';
import { SunOutlined, MoonOutlined, DesktopOutlined } from '@ant-design/icons';
import { useTheme } from '../../contexts/ThemeContext';
import './ThemeToggle.css';

interface ThemeToggleProps {
  size?: 'small' | 'middle' | 'large';
  variant?: 'button' | 'icon' | 'compact';
  showLabel?: boolean;
  className?: string;
}

export const ThemeToggle: React.FC<ThemeToggleProps> = ({
  size = 'middle',
  variant = 'button',
  showLabel = false,
  className = ''
}) => {
  const { themeMode, isDarkMode, toggleTheme } = useTheme();

  const getIcon = () => {
    switch (themeMode) {
      case 'light':
        return <SunOutlined />;
      case 'dark':
        return <MoonOutlined />;
      case 'auto':
        return <DesktopOutlined />;
      default:
        return <SunOutlined />;
    }
  };

  const getTooltipText = () => {
    switch (themeMode) {
      case 'light':
        return `Switch to Dark Mode (Currently: Light${isDarkMode ? ' - System Dark' : ''})`;
      case 'dark':
        return `Switch to Auto Mode (Currently: Dark)`;
      case 'auto':
        return `Switch to Light Mode (Currently: Auto${isDarkMode ? ' - Dark' : ' - Light'})`;
      default:
        return 'Toggle Theme';
    }
  };

  const getLabel = () => {
    switch (themeMode) {
      case 'light':
        return 'Light';
      case 'dark':
        return 'Dark';
      case 'auto':
        return 'Auto';
      default:
        return 'Theme';
    }
  };

  if (variant === 'icon') {
    return (
      <Tooltip title={getTooltipText()} placement="bottom">
        <Button
          type="text"
          icon={getIcon()}
          size={size}
          onClick={toggleTheme}
          className={`theme-toggle-icon ${className} ${isDarkMode ? 'dark' : 'light'}`}
          style={{
            color: isDarkMode ? '#f9fafb' : '#64748b',
            border: '1px solid',
            borderColor: isDarkMode ? '#4b5563' : '#e2e8f0',
            borderRadius: '10px',
            height: size === 'small' ? '28px' : size === 'large' ? '40px' : '36px',
            width: size === 'small' ? '28px' : size === 'large' ? '40px' : '36px',
            transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
            background: isDarkMode 
              ? 'rgba(31, 41, 59, 0.8)' 
              : 'rgba(255, 255, 255, 0.8)',
            backdropFilter: 'blur(8px)',
            boxShadow: isDarkMode
              ? '0 1px 3px 0 rgba(0, 0, 0, 0.3), 0 1px 2px 0 rgba(0, 0, 0, 0.2)'
              : '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
          }}
        />
      </Tooltip>
    );
  }

  if (variant === 'compact') {
    return (
      <Tooltip title={getTooltipText()} placement="bottom">
        <Button
          type="text"
          icon={getIcon()}
          size={size}
          onClick={toggleTheme}
          className={`theme-toggle-compact ${className} ${isDarkMode ? 'dark' : 'light'}`}
          style={{
            color: isDarkMode ? '#f9fafb' : '#64748b',
            border: '1px solid',
            borderColor: isDarkMode ? '#4b5563' : '#e2e8f0',
            borderRadius: '10px',
            height: size === 'small' ? '28px' : size === 'large' ? '40px' : '36px',
            padding: '0 12px',
            fontWeight: 500,
            fontSize: size === 'small' ? '12px' : '14px',
            transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
            background: isDarkMode 
              ? 'rgba(31, 41, 59, 0.8)' 
              : 'rgba(255, 255, 255, 0.8)',
            backdropFilter: 'blur(8px)',
            boxShadow: isDarkMode
              ? '0 1px 3px 0 rgba(0, 0, 0, 0.3), 0 1px 2px 0 rgba(0, 0, 0, 0.2)'
              : '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
          }}
        >
          {showLabel && getLabel()}
        </Button>
      </Tooltip>
    );
  }

  return (
    <Tooltip title={getTooltipText()} placement="bottom">
      <Button
        type="text"
        icon={getIcon()}
        size={size}
        onClick={toggleTheme}
        className={`theme-toggle-button ${className} ${isDarkMode ? 'dark' : 'light'}`}
        style={{
          color: isDarkMode ? '#f9fafb' : '#64748b',
          border: '1px solid',
          borderColor: isDarkMode ? '#4b5563' : '#e2e8f0',
          borderRadius: '10px',
          height: size === 'small' ? '28px' : size === 'large' ? '40px' : '36px',
          padding: '0 14px',
          fontWeight: 500,
          fontSize: size === 'small' ? '12px' : '14px',
          transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
          background: isDarkMode 
            ? 'rgba(31, 41, 59, 0.8)' 
            : 'rgba(255, 255, 255, 0.8)',
          backdropFilter: 'blur(8px)',
          boxShadow: isDarkMode
            ? '0 1px 3px 0 rgba(0, 0, 0, 0.3), 0 1px 2px 0 rgba(0, 0, 0, 0.2)'
            : '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)'
        }}
      >
        {showLabel ? getLabel() : 'Theme'}
      </Button>
    </Tooltip>
  );
};
