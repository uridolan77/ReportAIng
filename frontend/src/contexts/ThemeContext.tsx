import React, { createContext, useContext, useEffect, useState } from 'react';
import { theme } from 'antd';

export type ThemeMode = 'light' | 'dark' | 'auto';

interface ThemeContextType {
  themeMode: ThemeMode;
  isDarkMode: boolean;
  toggleTheme: () => void;
  setThemeMode: (mode: ThemeMode) => void;
  antdTheme: any;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

interface ThemeProviderProps {
  children: React.ReactNode;
}

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  const [themeMode, setThemeMode] = useState<ThemeMode>(() => {
    const saved = localStorage.getItem('bi-reporting-theme-mode');
    return (saved as ThemeMode) || 'light';
  });

  const [systemPrefersDark, setSystemPrefersDark] = useState(() => 
    window.matchMedia('(prefers-color-scheme: dark)').matches
  );

  // Determine if dark mode should be active
  const isDarkMode = themeMode === 'dark' || (themeMode === 'auto' && systemPrefersDark);

  // Listen for system theme changes
  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const handleChange = (e: MediaQueryListEvent) => {
      setSystemPrefersDark(e.matches);
    };

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);

  // Update document class and localStorage when theme changes
  useEffect(() => {
    console.log('Theme changing to:', themeMode, 'isDarkMode:', isDarkMode);

    document.documentElement.classList.toggle('dark-theme', isDarkMode);
    document.documentElement.setAttribute('data-theme', isDarkMode ? 'dark' : 'light');
    document.body.classList.toggle('dark-theme', isDarkMode);
    document.body.setAttribute('data-theme', isDarkMode ? 'dark' : 'light');

    // Force style recalculation
    if (isDarkMode) {
      document.body.style.backgroundColor = '#1f2937';
      document.body.style.color = '#f9fafb';

      // Force all major containers to dark theme
      const containers = document.querySelectorAll('.ant-layout-content, .main-content-area, .query-tabs, .ant-card, .ant-tabs-content');
      containers.forEach(container => {
        if (container instanceof HTMLElement) {
          container.style.backgroundColor = '#111827';
          container.style.color = '#f9fafb';
        }
      });

      // Force all text elements to light color
      const textElements = document.querySelectorAll('h1, h2, h3, h4, h5, h6, p, span, div, .ant-typography');
      textElements.forEach(element => {
        if (element instanceof HTMLElement) {
          element.style.color = '#f9fafb';
        }
      });

    } else {
      document.body.style.backgroundColor = '';
      document.body.style.color = '';

      // Reset containers
      const containers = document.querySelectorAll('.ant-layout-content, .main-content-area, .query-tabs, .ant-card, .ant-tabs-content');
      containers.forEach(container => {
        if (container instanceof HTMLElement) {
          container.style.backgroundColor = '';
          container.style.color = '';
        }
      });

      // Reset text elements
      const textElements = document.querySelectorAll('h1, h2, h3, h4, h5, h6, p, span, div, .ant-typography');
      textElements.forEach(element => {
        if (element instanceof HTMLElement) {
          element.style.color = '';
        }
      });
    }

    localStorage.setItem('bi-reporting-theme-mode', themeMode);

    console.log('Applied classes:', {
      htmlClasses: document.documentElement.className,
      bodyClasses: document.body.className,
      htmlDataTheme: document.documentElement.getAttribute('data-theme'),
      bodyDataTheme: document.body.getAttribute('data-theme')
    });
  }, [isDarkMode, themeMode]);

  const toggleTheme = () => {
    setThemeMode(current => {
      if (current === 'light') return 'dark';
      if (current === 'dark') return 'auto';
      return 'light';
    });

    // Force a re-render by adding a temporary class
    document.body.classList.add('theme-changing');
    setTimeout(() => {
      document.body.classList.remove('theme-changing');
    }, 100);
  };

  // Enhanced Ant Design theme configuration
  const antdTheme = {
    algorithm: isDarkMode ? theme.darkAlgorithm : theme.defaultAlgorithm,
    token: {
      // Modern Primary Colors
      colorPrimary: '#3b82f6',
      colorSuccess: '#10b981',
      colorWarning: '#f59e0b',
      colorError: '#ef4444',
      colorInfo: '#3b82f6',

      // Enhanced Typography
      fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif",
      fontSize: 16,
      fontSizeHeading1: 48,
      fontSizeHeading2: 36,
      fontSizeHeading3: 30,
      fontSizeHeading4: 24,
      fontSizeHeading5: 20,
      fontSizeHeading6: 18,

      // Modern Layout
      borderRadius: 12,
      borderRadiusLG: 20,
      borderRadiusSM: 8,
      borderRadiusXS: 6,

      // 8-Point Grid Spacing
      padding: 16,
      paddingLG: 24,
      paddingSM: 12,
      paddingXS: 8,
      margin: 16,
      marginLG: 24,
      marginSM: 12,
      marginXS: 8,

      // Enhanced Colors
      colorBgContainer: isDarkMode ? '#1e1e1e' : '#ffffff',
      colorBgElevated: isDarkMode ? '#2a2a2a' : '#ffffff',
      colorBgLayout: isDarkMode ? '#121212' : '#f7fafc',
      colorBorder: isDarkMode ? '#334155' : '#e2e8f0',
      colorBorderSecondary: isDarkMode ? '#1e293b' : '#edf2f7',

      // Text Colors
      colorText: isDarkMode ? '#e2e8f0' : '#1a202c',
      colorTextSecondary: isDarkMode ? '#94a3b8' : '#4a5568',
      colorTextTertiary: isDarkMode ? '#64748b' : '#718096',
      colorTextDisabled: isDarkMode ? '#475569' : '#a0aec0',

      // Dark mode specific adjustments
      ...(isDarkMode && {
        colorBgContainer: '#111827',
        colorBgElevated: '#374151',
        colorBgLayout: '#1f2937',
        colorBgBase: '#1f2937',
        colorBgSpotlight: '#111827',
        colorBorder: '#4b5563',
        colorBorderSecondary: '#374151',
        colorText: '#f9fafb',
        colorTextBase: '#f9fafb',
        colorTextSecondary: '#d1d5db',
        colorTextTertiary: '#9ca3af',
        colorTextQuaternary: '#6b7280',
        colorFill: '#374151',
        colorFillSecondary: '#4b5563',
        colorFillTertiary: '#6b7280',
        colorFillQuaternary: '#9ca3af',
      })
    },
    components: {
      Button: {
        borderRadius: 12,
        fontWeight: 600,
        paddingContentHorizontal: 24,
        paddingContentVertical: 12,
        primaryShadow: isDarkMode
          ? '0 10px 15px -3px rgba(59, 130, 246, 0.4), 0 4px 6px -2px rgba(59, 130, 246, 0.3)'
          : '0 10px 15px -3px rgba(59, 130, 246, 0.1), 0 4px 6px -2px rgba(59, 130, 246, 0.05)',
        defaultShadow: isDarkMode
          ? '0 1px 3px 0 rgba(0, 0, 0, 0.4), 0 1px 2px 0 rgba(0, 0, 0, 0.3)'
          : '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
      },
      Input: {
        borderRadius: 12,
        fontSize: 16,
        paddingBlock: 16,
        paddingInline: 20,
        borderWidth: 2,
        activeBorderColor: '#3b82f6',
        hoverBorderColor: '#93c5fd',
      },
      TextArea: {
        borderRadius: 12,
        fontSize: 16,
        paddingBlock: 16,
        paddingInline: 20,
        borderWidth: 2,
      },
      Card: {
        borderRadius: 20,
        paddingLG: 32,
        boxShadow: isDarkMode
          ? '0 10px 15px -3px rgba(0, 0, 0, 0.4), 0 4px 6px -2px rgba(0, 0, 0, 0.3)'
          : '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
        boxShadowTertiary: isDarkMode
          ? '0 1px 3px 0 rgba(0, 0, 0, 0.4), 0 1px 2px 0 rgba(0, 0, 0, 0.3)'
          : '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
      },
      Menu: {
        borderRadius: 12,
        itemBorderRadius: 12,
        itemPadding: '12px 16px',
        itemMarginBlock: 4,
        itemMarginInline: 8,
        subMenuItemBorderRadius: 8,
      },
      Tabs: {
        borderRadius: 12,
        cardBg: isDarkMode ? '#1e1e1e' : '#ffffff',
        itemColor: isDarkMode ? '#94a3b8' : '#4a5568',
        itemSelectedColor: isDarkMode ? '#e2e8f0' : '#1a202c',
        itemHoverColor: isDarkMode ? '#cbd5e1' : '#2d3748',
        inkBarColor: '#3b82f6',
        cardPadding: 24,
      },
      Layout: {
        siderBg: isDarkMode ? '#1e1e1e' : '#ffffff',
        bodyBg: isDarkMode
          ? 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)'
          : 'linear-gradient(135deg, #f7fafc 0%, #edf2f7 100%)',
        headerBg: isDarkMode ? '#1e1e1e' : '#ffffff',
      },
    },
  };

  const contextValue: ThemeContextType = {
    themeMode,
    isDarkMode,
    toggleTheme,
    setThemeMode,
    antdTheme,
  };

  return (
    <ThemeContext.Provider value={contextValue}>
      {children}
    </ThemeContext.Provider>
  );
};

export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};
