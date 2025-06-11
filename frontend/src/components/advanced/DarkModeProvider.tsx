/**
 * Dark Mode Provider
 * 
 * Advanced dark mode implementation with system preference detection,
 * smooth transitions, and comprehensive theme management.
 */

import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { ConfigProvider, theme } from 'antd';

// Types
export type ThemeMode = 'light' | 'dark' | 'system';

export interface DarkModeContextValue {
  mode: ThemeMode;
  isDark: boolean;
  setMode: (mode: ThemeMode) => void;
  toggleMode: () => void;
  systemPreference: 'light' | 'dark';
}

export interface DarkModeProviderProps {
  children: React.ReactNode;
  defaultMode?: ThemeMode;
  enableTransitions?: boolean;
  storageKey?: string;
}

// Context
const DarkModeContext = createContext<DarkModeContextValue | null>(null);

// Hook
export const useDarkMode = () => {
  const context = useContext(DarkModeContext);
  if (!context) {
    throw new Error('useDarkMode must be used within a DarkModeProvider');
  }
  return context;
};

// Provider Component
export const DarkModeProvider: React.FC<DarkModeProviderProps> = ({
  children,
  defaultMode = 'system',
  enableTransitions = true,
  storageKey = 'theme-mode',
}) => {
  const [mode, setModeState] = useState<ThemeMode>(defaultMode);
  const [systemPreference, setSystemPreference] = useState<'light' | 'dark'>('light');

  // Detect system preference
  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    setSystemPreference(mediaQuery.matches ? 'dark' : 'light');

    const handleChange = (e: MediaQueryListEvent) => {
      setSystemPreference(e.matches ? 'dark' : 'light');
    };

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);

  // Load saved preference
  useEffect(() => {
    try {
      const saved = localStorage.getItem(storageKey);
      if (saved && ['light', 'dark', 'system'].includes(saved)) {
        setModeState(saved as ThemeMode);
      }
    } catch (error) {
      console.warn('Failed to load theme preference:', error);
    }
  }, [storageKey]);

  // Calculate actual dark mode state
  const isDark = mode === 'dark' || (mode === 'system' && systemPreference === 'dark');

  // Set mode with persistence
  const setMode = useCallback((newMode: ThemeMode) => {
    setModeState(newMode);
    try {
      localStorage.setItem(storageKey, newMode);
    } catch (error) {
      console.warn('Failed to save theme preference:', error);
    }
  }, [storageKey]);

  // Toggle between light and dark (skips system)
  const toggleMode = useCallback(() => {
    if (mode === 'system') {
      setMode(systemPreference === 'dark' ? 'light' : 'dark');
    } else {
      setMode(mode === 'light' ? 'dark' : 'light');
    }
  }, [mode, systemPreference, setMode]);

  // Apply CSS custom properties for smooth transitions
  useEffect(() => {
    const root = document.documentElement;
    
    if (enableTransitions) {
      root.style.setProperty('--theme-transition', 'all 0.3s ease');
    }

    // Set CSS custom properties based on theme
    if (isDark) {
      root.style.setProperty('--bg-primary', '#141414');
      root.style.setProperty('--bg-secondary', '#1f1f1f');
      root.style.setProperty('--bg-tertiary', '#262626');
      root.style.setProperty('--text-primary', '#ffffff');
      root.style.setProperty('--text-secondary', '#a6a6a6');
      root.style.setProperty('--text-tertiary', '#737373');
      root.style.setProperty('--border-color', '#404040');
      root.style.setProperty('--shadow', '0 2px 8px rgba(0, 0, 0, 0.3)');
    } else {
      root.style.setProperty('--bg-primary', '#ffffff');
      root.style.setProperty('--bg-secondary', '#fafafa');
      root.style.setProperty('--bg-tertiary', '#f5f5f5');
      root.style.setProperty('--text-primary', '#000000');
      root.style.setProperty('--text-secondary', '#666666');
      root.style.setProperty('--text-tertiary', '#999999');
      root.style.setProperty('--border-color', '#d9d9d9');
      root.style.setProperty('--shadow', '0 2px 8px rgba(0, 0, 0, 0.1)');
    }

    // Add theme class to body
    document.body.className = document.body.className.replace(/theme-\w+/g, '');
    document.body.classList.add(`theme-${isDark ? 'dark' : 'light'}`);
  }, [isDark, enableTransitions]);

  // Antd theme configuration
  const antdTheme = {
    algorithm: isDark ? theme.darkAlgorithm : theme.defaultAlgorithm,
    token: {
      colorPrimary: '#667eea',
      colorSuccess: '#10b981',
      colorWarning: '#f59e0b',
      colorError: '#ef4444',
      colorInfo: '#3b82f6',
      borderRadius: 8,
      ...(isDark ? {
        colorBgContainer: '#1f1f1f',
        colorBgElevated: '#262626',
        colorBgLayout: '#141414',
        colorText: '#ffffff',
        colorTextSecondary: '#a6a6a6',
        colorBorder: '#404040',
      } : {
        colorBgContainer: '#ffffff',
        colorBgElevated: '#ffffff',
        colorBgLayout: '#f5f5f5',
        colorText: '#000000',
        colorTextSecondary: '#666666',
        colorBorder: '#d9d9d9',
      }),
    },
    components: {
      Layout: {
        bodyBg: isDark ? '#141414' : '#f5f5f5',
        headerBg: isDark ? '#1f1f1f' : '#ffffff',
        siderBg: isDark ? '#1f1f1f' : '#ffffff',
      },
      Menu: {
        itemBg: 'transparent',
        itemSelectedBg: isDark ? '#262626' : '#f0f0f0',
        itemHoverBg: isDark ? '#262626' : '#f5f5f5',
      },
      Card: {
        colorBgContainer: isDark ? '#1f1f1f' : '#ffffff',
      },
      Table: {
        colorBgContainer: isDark ? '#1f1f1f' : '#ffffff',
        headerBg: isDark ? '#262626' : '#fafafa',
      },
    },
  };

  const contextValue: DarkModeContextValue = {
    mode,
    isDark,
    setMode,
    toggleMode,
    systemPreference,
  };

  return (
    <DarkModeContext.Provider value={contextValue}>
      <ConfigProvider theme={antdTheme}>
        {children}
      </ConfigProvider>
    </DarkModeContext.Provider>
  );
};

// Dark Mode Toggle Component
export const DarkModeToggle: React.FC<{
  size?: 'small' | 'medium' | 'large';
  showLabel?: boolean;
  className?: string;
  style?: React.CSSProperties;
}> = ({ size = 'medium', showLabel = false, className, style }) => {
  const { mode, isDark, toggleMode, setMode } = useDarkMode();

  const getSizeStyles = () => {
    const sizes = {
      small: { width: '32px', height: '32px', fontSize: '14px' },
      medium: { width: '40px', height: '40px', fontSize: '16px' },
      large: { width: '48px', height: '48px', fontSize: '18px' },
    };
    return sizes[size];
  };

  const sizeStyles = getSizeStyles();

  return (
    <div className={className} style={{ display: 'flex', alignItems: 'center', gap: '8px', ...style }}>
      <button
        onClick={toggleMode}
        style={{
          ...sizeStyles,
          border: '1px solid var(--border-color)',
          borderRadius: '50%',
          backgroundColor: 'var(--bg-secondary)',
          color: 'var(--text-primary)',
          cursor: 'pointer',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          transition: 'var(--theme-transition)',
        }}
        title={`Switch to ${isDark ? 'light' : 'dark'} mode`}
      >
        {isDark ? '☀️' : '🌙'}
      </button>
      
      {showLabel && (
        <span style={{ fontSize: '14px', color: 'var(--text-secondary)' }}>
          {mode === 'system' ? `System (${isDark ? 'Dark' : 'Light'})` : 
           mode === 'dark' ? 'Dark' : 'Light'}
        </span>
      )}
      
      {mode === 'system' && (
        <button
          onClick={() => setMode(isDark ? 'light' : 'dark')}
          style={{
            padding: '4px 8px',
            border: '1px solid var(--border-color)',
            borderRadius: '4px',
            backgroundColor: 'transparent',
            color: 'var(--text-secondary)',
            fontSize: '12px',
            cursor: 'pointer',
          }}
          title="Override system preference"
        >
          Override
        </button>
      )}
    </div>
  );
};
