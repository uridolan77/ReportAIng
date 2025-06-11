/**
 * Storybook Preview Configuration
 * 
 * Global configuration for Storybook including decorators, parameters,
 * and theme setup for consistent component documentation.
 */

import React from 'react';
import type { Preview } from '@storybook/react';
import { ConfigProvider } from 'antd';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '../src/contexts/ThemeContext';
import { DarkModeProvider } from '../src/components/advanced/DarkModeProvider';
import { designTokens } from '../src/components/core/design-system';
import '../src/App.css';

// Create a query client for Storybook
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
      staleTime: Infinity,
    },
  },
});

// Global decorator to wrap all stories with providers
const withProviders = (Story: any, context: any) => {
  const { globals } = context;
  const darkMode = globals.theme === 'dark';

  return (
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <ConfigProvider>
          <ThemeProvider>
            <DarkModeProvider defaultMode={darkMode ? 'dark' : 'light'}>
              <div style={{ 
                padding: '20px',
                minHeight: '100vh',
                backgroundColor: 'var(--bg-primary)',
                color: 'var(--text-primary)',
                transition: 'all 0.3s ease',
              }}>
                <Story />
              </div>
            </DarkModeProvider>
          </ThemeProvider>
        </ConfigProvider>
      </QueryClientProvider>
    </BrowserRouter>
  );
};

const preview: Preview = {
  parameters: {
    actions: { argTypesRegex: '^on[A-Z].*' },
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/,
      },
      expanded: true,
      sort: 'requiredFirst',
    },
    docs: {
      theme: {
        base: 'light',
        brandTitle: 'BI Reporting Copilot - Component Library',
        brandUrl: '/',
        brandImage: undefined,
        brandTarget: '_self',
        
        colorPrimary: designTokens.colors.primary,
        colorSecondary: designTokens.colors.secondary,
        
        // UI
        appBg: designTokens.colors.background,
        appContentBg: designTokens.colors.white,
        appBorderColor: designTokens.colors.border,
        appBorderRadius: parseInt(designTokens.borderRadius.medium),
        
        // Typography
        fontBase: designTokens.typography.fontFamily.sans,
        fontCode: designTokens.typography.fontFamily.mono,
        
        // Text colors
        textColor: designTokens.colors.text,
        textInverseColor: designTokens.colors.textInverse,
        
        // Toolbar default and active colors
        barTextColor: designTokens.colors.textSecondary,
        barSelectedColor: designTokens.colors.primary,
        barBg: designTokens.colors.backgroundSecondary,
        
        // Form colors
        inputBg: designTokens.colors.white,
        inputBorder: designTokens.colors.border,
        inputTextColor: designTokens.colors.text,
        inputBorderRadius: parseInt(designTokens.borderRadius.medium),
      },
      source: {
        state: 'open',
      },
      canvas: {
        sourceState: 'shown',
      },
    },
    backgrounds: {
      default: 'light',
      values: [
        {
          name: 'light',
          value: designTokens.colors.background,
        },
        {
          name: 'dark',
          value: '#141414',
        },
        {
          name: 'gray',
          value: designTokens.colors.backgroundSecondary,
        },
      ],
    },
    viewport: {
      viewports: {
        mobile: {
          name: 'Mobile',
          styles: {
            width: '375px',
            height: '667px',
          },
        },
        tablet: {
          name: 'Tablet',
          styles: {
            width: '768px',
            height: '1024px',
          },
        },
        desktop: {
          name: 'Desktop',
          styles: {
            width: '1440px',
            height: '900px',
          },
        },
        wide: {
          name: 'Wide Screen',
          styles: {
            width: '1920px',
            height: '1080px',
          },
        },
      },
    },
    layout: 'centered',
    options: {
      storySort: {
        order: [
          'Introduction',
          'Design System',
          ['Colors', 'Typography', 'Spacing', 'Components'],
          'Core Components',
          ['Button', 'Card', 'Layout', 'Form', 'Navigation', 'Data', 'Feedback'],
          'Advanced Components',
          ['Dark Mode', 'Animations', 'Performance'],
          'Pages',
          ['Query Page', 'Dashboard Page', 'Visualization Page'],
          'Examples',
          '*',
        ],
      },
    },
  },
  
  globalTypes: {
    theme: {
      description: 'Global theme for components',
      defaultValue: 'light',
      toolbar: {
        title: 'Theme',
        icon: 'paintbrush',
        items: [
          { value: 'light', title: 'Light', icon: 'sun' },
          { value: 'dark', title: 'Dark', icon: 'moon' },
        ],
        dynamicTitle: true,
      },
    },
    locale: {
      description: 'Internationalization locale',
      defaultValue: 'en',
      toolbar: {
        title: 'Locale',
        icon: 'globe',
        items: [
          { value: 'en', title: 'English', right: '🇺🇸' },
          { value: 'es', title: 'Español', right: '🇪🇸' },
          { value: 'fr', title: 'Français', right: '🇫🇷' },
        ],
      },
    },
  },
  
  decorators: [withProviders],
  
  tags: ['autodocs'],
};

export default preview;
