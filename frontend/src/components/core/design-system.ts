/**
 * Design System Tokens
 * 
 * Centralized design tokens for consistent theming across all components.
 * Replaces scattered CSS variables and provides TypeScript support.
 */

// Color Palette
export const colors = {
  // Primary Colors
  primary: '#667eea',
  primaryHover: '#5a6fd8',
  primaryActive: '#4c63d2',
  primaryLight: '#e8ecff',
  
  // Secondary Colors
  secondary: '#f7fafc',
  secondaryHover: '#edf2f7',
  secondaryActive: '#e2e8f0',
  
  // Neutral Colors
  white: '#ffffff',
  black: '#000000',
  gray50: '#f9fafb',
  gray100: '#f3f4f6',
  gray200: '#e5e7eb',
  gray300: '#d1d5db',
  gray400: '#9ca3af',
  gray500: '#6b7280',
  gray600: '#4b5563',
  gray700: '#374151',
  gray800: '#1f2937',
  gray900: '#111827',
  
  // Semantic Colors
  success: '#10b981',
  successHover: '#059669',
  successLight: '#d1fae5',
  
  warning: '#f59e0b',
  warningHover: '#d97706',
  warningLight: '#fef3c7',
  
  danger: '#ef4444',
  dangerHover: '#dc2626',
  dangerLight: '#fee2e2',
  
  info: '#3b82f6',
  infoHover: '#2563eb',
  infoLight: '#dbeafe',
  
  // Text Colors
  text: '#1f2937',
  textSecondary: '#6b7280',
  textMuted: '#9ca3af',
  textInverse: '#ffffff',
  
  // Background Colors
  background: '#ffffff',
  backgroundSecondary: '#f9fafb',
  backgroundHover: '#f3f4f6',
  
  // Border Colors
  border: '#e5e7eb',
  borderHover: '#d1d5db',
  borderFocus: '#667eea',
} as const;

// Spacing Scale
export const spacing = {
  xs: '4px',
  sm: '8px',
  md: '16px',
  lg: '24px',
  xl: '32px',
  '2xl': '48px',
  '3xl': '64px',
  '4xl': '96px',
  '5xl': '128px',
} as const;

// Typography Scale
export const typography = {
  fontFamily: {
    sans: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
    mono: 'SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace',
  },
  fontSize: {
    xs: '12px',
    sm: '14px',
    base: '16px',
    lg: '18px',
    xl: '20px',
    '2xl': '24px',
    '3xl': '30px',
    '4xl': '36px',
    '5xl': '48px',
  },
  fontWeight: {
    normal: 400,
    medium: 500,
    semibold: 600,
    bold: 700,
  },
  lineHeight: {
    tight: 1.25,
    normal: 1.5,
    relaxed: 1.75,
  },
} as const;

// Border Radius
export const borderRadius = {
  none: '0',
  small: '4px',
  medium: '8px',
  large: '12px',
  xl: '16px',
  full: '9999px',
} as const;

// Box Shadows
export const shadows = {
  none: 'none',
  small: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
  medium: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
  large: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
  xl: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
  inner: 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.06)',
} as const;

// Z-Index Scale
export const zIndex = {
  hide: -1,
  auto: 'auto',
  base: 0,
  docked: 10,
  dropdown: 1000,
  sticky: 1100,
  banner: 1200,
  overlay: 1300,
  modal: 1400,
  popover: 1500,
  skipLink: 1600,
  toast: 1700,
  tooltip: 1800,
} as const;

// Animation Durations
export const animations = {
  duration: {
    fast: '150ms',
    normal: '300ms',
    slow: '500ms',
  },
  easing: {
    linear: 'linear',
    easeIn: 'cubic-bezier(0.4, 0, 1, 1)',
    easeOut: 'cubic-bezier(0, 0, 0.2, 1)',
    easeInOut: 'cubic-bezier(0.4, 0, 0.2, 1)',
  },
} as const;

// Breakpoints
export const breakpoints = {
  sm: '640px',
  md: '768px',
  lg: '1024px',
  xl: '1280px',
  '2xl': '1536px',
} as const;

// Component Sizes
export const componentSizes = {
  small: {
    height: '32px',
    padding: '0 12px',
    fontSize: typography.fontSize.sm,
  },
  medium: {
    height: '40px',
    padding: '0 16px',
    fontSize: typography.fontSize.base,
  },
  large: {
    height: '48px',
    padding: '0 24px',
    fontSize: typography.fontSize.lg,
  },
} as const;

// Consolidated Design Tokens
export const designTokens = {
  colors,
  spacing,
  typography,
  borderRadius,
  shadows,
  zIndex,
  animations,
  breakpoints,
  componentSizes,
} as const;

// CSS Custom Properties Generator
export const generateCSSVariables = () => {
  const cssVars: Record<string, string> = {};
  
  // Colors
  Object.entries(colors).forEach(([key, value]) => {
    cssVars[`--color-${key}`] = value;
  });
  
  // Spacing
  Object.entries(spacing).forEach(([key, value]) => {
    cssVars[`--spacing-${key}`] = value;
  });
  
  // Typography
  Object.entries(typography.fontSize).forEach(([key, value]) => {
    cssVars[`--font-size-${key}`] = value;
  });
  
  // Border Radius
  Object.entries(borderRadius).forEach(([key, value]) => {
    cssVars[`--border-radius-${key}`] = value;
  });
  
  // Shadows
  Object.entries(shadows).forEach(([key, value]) => {
    cssVars[`--shadow-${key}`] = value;
  });
  
  return cssVars;
};

// Type Definitions
export type ColorKey = keyof typeof colors;
export type SpacingKey = keyof typeof spacing;
export type FontSizeKey = keyof typeof typography.fontSize;
export type BorderRadiusKey = keyof typeof borderRadius;
export type ShadowKey = keyof typeof shadows;
export type ComponentSize = keyof typeof componentSizes;
