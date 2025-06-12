/**
 * Ant Design Theme Configuration
 * 
 * Standardized theme configuration for Ant Design components
 * that aligns with our design system tokens.
 */

import { theme } from 'antd';
import type { ThemeConfig } from 'antd';

// Design system color tokens
const designTokens = {
  colors: {
    primary: '#3b82f6',
    primaryHover: '#2563eb',
    primaryActive: '#1d4ed8',
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6',
  },
  spacing: {
    xs: 4,
    sm: 8,
    md: 12,
    lg: 16,
    xl: 20,
    xxl: 24,
  },
  borderRadius: {
    sm: 6,
    base: 8,
    md: 12,
    lg: 16,
    xl: 20,
  },
  fontSize: {
    xs: 12,
    sm: 14,
    base: 16,
    lg: 18,
    xl: 20,
    xxl: 24,
  },
  fontWeight: {
    normal: 400,
    medium: 500,
    semibold: 600,
    bold: 700,
  },
  shadows: {
    sm: '0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)',
    base: '0 4px 6px rgba(0, 0, 0, 0.07), 0 2px 4px rgba(0, 0, 0, 0.06)',
    md: '0 10px 15px rgba(0, 0, 0, 0.1), 0 4px 6px rgba(0, 0, 0, 0.05)',
    lg: '0 20px 25px rgba(0, 0, 0, 0.1), 0 10px 10px rgba(0, 0, 0, 0.04)',
  },
};

// Light theme configuration
export const lightTheme: ThemeConfig = {
  algorithm: theme.defaultAlgorithm,
  token: {
    // Brand Colors
    colorPrimary: designTokens.colors.primary,
    colorSuccess: designTokens.colors.success,
    colorWarning: designTokens.colors.warning,
    colorError: designTokens.colors.error,
    colorInfo: designTokens.colors.info,

    // Typography
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    fontSize: designTokens.fontSize.base,
    fontSizeHeading1: 36,
    fontSizeHeading2: 30,
    fontSizeHeading3: 24,
    fontSizeHeading4: 20,
    fontSizeHeading5: 18,
    fontSizeHeading6: 16,

    // Layout & Spacing
    borderRadius: designTokens.borderRadius.base,
    borderRadiusLG: designTokens.borderRadius.lg,
    borderRadiusSM: designTokens.borderRadius.sm,
    borderRadiusXS: designTokens.borderRadius.sm,
    
    // Spacing
    padding: designTokens.spacing.lg,
    paddingLG: designTokens.spacing.xl,
    paddingSM: designTokens.spacing.md,
    paddingXS: designTokens.spacing.sm,
    
    margin: designTokens.spacing.lg,
    marginLG: designTokens.spacing.xl,
    marginSM: designTokens.spacing.md,
    marginXS: designTokens.spacing.sm,

    // Shadows
    boxShadow: designTokens.shadows.base,
    boxShadowSecondary: designTokens.shadows.sm,

    // Surface Colors
    colorBgContainer: '#ffffff',
    colorBgElevated: '#ffffff',
    colorBgLayout: '#f9fafb',
    colorBgSpotlight: '#ffffff',
    colorBgMask: 'rgba(0, 0, 0, 0.45)',

    // Text Colors
    colorText: '#111827',
    colorTextSecondary: '#6b7280',
    colorTextTertiary: '#9ca3af',
    colorTextQuaternary: '#d1d5db',

    // Border Colors
    colorBorder: '#e5e7eb',
    colorBorderSecondary: '#f3f4f6',

    // Control Heights
    controlHeight: 40,
    controlHeightLG: 48,
    controlHeightSM: 32,
    controlHeightXS: 24,

    // Line Heights
    lineHeight: 1.5,
    lineHeightHeading: 1.25,

    // Motion
    motionDurationFast: '0.1s',
    motionDurationMid: '0.2s',
    motionDurationSlow: '0.3s',
    motionEaseInOut: 'cubic-bezier(0.4, 0, 0.2, 1)',
    motionEaseOut: 'cubic-bezier(0, 0, 0.2, 1)',
    motionEaseIn: 'cubic-bezier(0.4, 0, 1, 1)',

    // Z-Index
    zIndexBase: 0,
    zIndexPopupBase: 1000,
  },
  components: {
    // Button Component
    Button: {
      borderRadius: designTokens.borderRadius.base,
      controlHeight: 40,
      controlHeightLG: 48,
      controlHeightSM: 32,
      fontWeight: designTokens.fontWeight.medium,
      primaryShadow: '0 2px 4px rgba(59, 130, 246, 0.2)',
    },

    // Input Component
    Input: {
      borderRadius: designTokens.borderRadius.base,
      controlHeight: 40,
      controlHeightLG: 48,
      controlHeightSM: 32,
      paddingInline: designTokens.spacing.md,
    },

    // Card Component
    Card: {
      borderRadiusLG: designTokens.borderRadius.md,
      paddingLG: designTokens.spacing.xl,
      boxShadowTertiary: designTokens.shadows.sm,
    },

    // Table Component
    Table: {
      borderRadius: designTokens.borderRadius.base,
      headerBg: '#f9fafb',
      headerColor: '#374151',
      headerSortActiveBg: '#f3f4f6',
      headerSortHoverBg: '#f9fafb',
      rowHoverBg: '#f9fafb',
    },

    // Menu Component
    Menu: {
      borderRadius: designTokens.borderRadius.base,
      itemBorderRadius: designTokens.borderRadius.sm,
      itemHeight: 40,
      itemPaddingInline: designTokens.spacing.md,
      iconSize: 16,
    },

    // Modal Component
    Modal: {
      borderRadiusLG: designTokens.borderRadius.lg,
      paddingLG: designTokens.spacing.xl,
    },

    // Drawer Component
    Drawer: {
      borderRadiusLG: designTokens.borderRadius.lg,
      paddingLG: designTokens.spacing.xl,
    },

    // Notification Component
    Notification: {
      borderRadiusLG: designTokens.borderRadius.md,
      paddingLG: designTokens.spacing.lg,
    },

    // Message Component
    Message: {
      borderRadiusLG: designTokens.borderRadius.md,
      paddingLG: designTokens.spacing.md,
    },

    // Tabs Component
    Tabs: {
      borderRadius: designTokens.borderRadius.base,
      itemActiveColor: designTokens.colors.primary,
      itemHoverColor: designTokens.colors.primaryHover,
      itemSelectedColor: designTokens.colors.primary,
    },

    // Progress Component
    Progress: {
      borderRadius: designTokens.borderRadius.base,
    },

    // Badge Component
    Badge: {
      borderRadiusSM: designTokens.borderRadius.sm,
      fontWeight: designTokens.fontWeight.medium,
    },

    // Tag Component
    Tag: {
      borderRadiusSM: designTokens.borderRadius.sm,
      fontWeight: designTokens.fontWeight.medium,
    },

    // Alert Component
    Alert: {
      borderRadiusLG: designTokens.borderRadius.base,
      paddingLG: designTokens.spacing.lg,
    },

    // Tooltip Component
    Tooltip: {
      borderRadius: designTokens.borderRadius.sm,
      paddingSM: designTokens.spacing.sm,
    },

    // Popover Component
    Popover: {
      borderRadiusOuter: designTokens.borderRadius.base,
      paddingLG: designTokens.spacing.lg,
    },

    // Select Component
    Select: {
      borderRadius: designTokens.borderRadius.base,
      controlHeight: 40,
      controlHeightLG: 48,
      controlHeightSM: 32,
    },

    // DatePicker Component
    DatePicker: {
      borderRadius: designTokens.borderRadius.base,
      controlHeight: 40,
      controlHeightLG: 48,
      controlHeightSM: 32,
    },

    // Switch Component
    Switch: {
      borderRadius: designTokens.borderRadius.base,
    },

    // Slider Component
    Slider: {
      borderRadius: designTokens.borderRadius.base,
    },

    // Upload Component
    Upload: {
      borderRadiusLG: designTokens.borderRadius.base,
    },

    // Form Component
    Form: {
      itemMarginBottom: designTokens.spacing.lg,
      verticalLabelPadding: `0 0 ${designTokens.spacing.sm}px`,
    },
  },
};

// Dark theme configuration
export const darkTheme: ThemeConfig = {
  algorithm: theme.darkAlgorithm,
  token: {
    ...lightTheme.token,
    
    // Surface Colors - Dark Mode
    colorBgContainer: '#1a1a1a',
    colorBgElevated: '#2d2d2d',
    colorBgLayout: '#141414',
    colorBgSpotlight: '#262626',
    colorBgMask: 'rgba(0, 0, 0, 0.8)',

    // Text Colors - Dark Mode
    colorText: '#ffffff',
    colorTextSecondary: '#a3a3a3',
    colorTextTertiary: '#737373',
    colorTextQuaternary: '#525252',

    // Border Colors - Dark Mode
    colorBorder: '#404040',
    colorBorderSecondary: '#525252',

    // Enhanced shadows for dark mode
    boxShadow: '0 4px 6px rgba(0, 0, 0, 0.4), 0 2px 4px rgba(0, 0, 0, 0.3)',
    boxShadowSecondary: '0 1px 3px rgba(0, 0, 0, 0.4), 0 1px 2px rgba(0, 0, 0, 0.3)',
  },
  components: {
    ...lightTheme.components,
    
    // Dark mode specific overrides
    Table: {
      ...lightTheme.components?.Table,
      headerBg: '#262626',
      headerColor: '#ffffff',
      headerSortActiveBg: '#404040',
      headerSortHoverBg: '#2d2d2d',
      rowHoverBg: '#262626',
    },

    Button: {
      ...lightTheme.components?.Button,
      primaryShadow: '0 2px 4px rgba(59, 130, 246, 0.4)',
    },
  },
};

// Theme configuration factory
export const createTheme = (isDark: boolean): ThemeConfig => {
  return isDark ? darkTheme : lightTheme;
};

// Export design tokens for use in components
export { designTokens };
