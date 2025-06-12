import React, { createContext, useContext, useEffect, useState } from 'react';
import { createTheme } from '../styles/design-system/antd-theme';

export type ThemeMode = 'light' | 'dark' | 'auto';

interface ThemeContextType {
  themeMode: ThemeMode;
  isDarkMode: boolean;
  actualTheme: 'light' | 'dark'; // Added missing actualTheme property
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

    // Clean approach using CSS custom properties
    const root = document.documentElement;
    const body = document.body;

    // Remove existing theme classes and attributes
    root.classList.remove('light-theme', 'dark-theme');
    body.classList.remove('light-theme', 'dark-theme');

    // Set data-theme attribute for CSS targeting
    const actualTheme = isDarkMode ? 'dark' : 'light';
    root.setAttribute('data-theme', actualTheme);
    body.setAttribute('data-theme', actualTheme);

    // Add theme class for compatibility
    root.classList.add(`${actualTheme}-theme`);
    body.classList.add(`${actualTheme}-theme`);

    // Set color-scheme for better browser integration
    body.style.colorScheme = actualTheme;

    // Update meta theme-color for mobile browsers
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
      metaThemeColor.setAttribute(
        'content',
        isDarkMode ? '#111827' : '#ffffff'
      );
    }

    // Persist theme preference
    localStorage.setItem('bi-reporting-theme-mode', themeMode);

    console.log('Applied theme:', {
      themeMode,
      actualTheme,
      htmlDataTheme: root.getAttribute('data-theme'),
      bodyDataTheme: body.getAttribute('data-theme'),
      colorScheme: body.style.colorScheme
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

  // Enhanced Ant Design theme configuration using standardized design system
  const antdTheme = createTheme(isDarkMode);

  const contextValue: ThemeContextType = {
    themeMode,
    isDarkMode,
    actualTheme: isDarkMode ? 'dark' : 'light', // Added actualTheme calculation
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
