/**
 * UI Component Types
 * Comprehensive type definitions for the UI component system
 */

import React from 'react';

// Base component props
export interface BaseComponentProps {
  className?: string;
  style?: React.CSSProperties;
  children?: React.ReactNode;
  id?: string;
  'data-testid'?: string;
}

// Size variants
export type Size = 'small' | 'medium' | 'large';
export type ExtendedSize = Size | 'extra-small' | 'extra-large';

// Color variants
export type Color = 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';
export type ColorVariant = Color | 'default' | 'inherit';

// Common variants
export type Variant = 'default' | 'outlined' | 'filled' | 'ghost' | 'text';
export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'outline' | 'default';
export type CardVariant = 'default' | 'outlined' | 'elevated' | 'flat';
export type AlertVariant = 'default' | 'filled' | 'outlined';

// Layout props
export interface LayoutProps extends BaseComponentProps {
  padding?: string | number;
  margin?: string | number;
  gap?: string | number;
  direction?: 'row' | 'column';
  align?: 'start' | 'center' | 'end' | 'stretch';
  justify?: 'start' | 'center' | 'end' | 'between' | 'around' | 'evenly';
  wrap?: boolean;
}

// Responsive props
export interface ResponsiveProps {
  xs?: number | boolean;
  sm?: number | boolean;
  md?: number | boolean;
  lg?: number | boolean;
  xl?: number | boolean;
  xxl?: number | boolean;
}

// Interactive props
export interface InteractiveProps {
  disabled?: boolean;
  loading?: boolean;
  onClick?: (event: React.MouseEvent) => void;
  onFocus?: (event: React.FocusEvent) => void;
  onBlur?: (event: React.FocusEvent) => void;
  onKeyDown?: (event: React.KeyboardEvent) => void;
}

// Form props
export interface FormProps extends BaseComponentProps {
  name?: string;
  value?: any;
  defaultValue?: any;
  onChange?: (value: any) => void;
  onBlur?: (event: React.FocusEvent) => void;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  error?: string | boolean;
  helperText?: string;
}

// Button props
export interface ButtonProps extends BaseComponentProps, InteractiveProps {
  variant?: ButtonVariant;
  size?: Size;
  fullWidth?: boolean;
  icon?: React.ReactNode;
  iconPosition?: 'start' | 'end';
  href?: string;
  target?: string;
  type?: 'button' | 'submit' | 'reset';
}

// Card props
export interface CardProps extends BaseComponentProps {
  variant?: CardVariant;
  padding?: 'none' | 'small' | 'medium' | 'large';
  hover?: boolean;
  clickable?: boolean;
  onClick?: (event: React.MouseEvent) => void;
}

// Modal props
export interface ModalProps extends BaseComponentProps {
  open?: boolean;
  onClose?: () => void;
  title?: React.ReactNode;
  footer?: React.ReactNode;
  size?: Size | 'extra-large';
  variant?: 'default' | 'fullscreen' | 'compact' | 'centered';
  closable?: boolean;
  maskClosable?: boolean;
}

// Table props
export interface TableColumn {
  key: string;
  title: React.ReactNode;
  dataIndex?: string;
  width?: number | string;
  align?: 'left' | 'center' | 'right';
  sortable?: boolean;
  filterable?: boolean;
  render?: (value: any, record: any, index: number) => React.ReactNode;
}

export interface TableProps extends BaseComponentProps {
  columns: TableColumn[];
  data: any[];
  loading?: boolean;
  pagination?: boolean | object;
  selection?: boolean | object;
  variant?: 'default' | 'bordered' | 'striped' | 'compact';
  size?: Size;
  onRowClick?: (record: any, index: number) => void;
  onSelectionChange?: (selectedRows: any[]) => void;
}

// Navigation props
export interface MenuItemProps {
  key: string;
  label: React.ReactNode;
  icon?: React.ReactNode;
  disabled?: boolean;
  children?: MenuItemProps[];
  onClick?: () => void;
}

export interface MenuProps extends BaseComponentProps {
  items: MenuItemProps[];
  mode?: 'horizontal' | 'vertical' | 'inline';
  selectedKeys?: string[];
  openKeys?: string[];
  onSelect?: (selectedKeys: string[]) => void;
  onOpenChange?: (openKeys: string[]) => void;
}

// Form field props
export interface InputProps extends FormProps {
  type?: 'text' | 'password' | 'email' | 'number' | 'tel' | 'url' | 'search';
  size?: Size;
  variant?: 'default' | 'filled' | 'borderless';
  prefix?: React.ReactNode;
  suffix?: React.ReactNode;
  addonBefore?: React.ReactNode;
  addonAfter?: React.ReactNode;
  maxLength?: number;
  showCount?: boolean;
}

export interface SelectProps extends FormProps {
  options: Array<{
    label: React.ReactNode;
    value: any;
    disabled?: boolean;
    group?: string;
  }>;
  multiple?: boolean;
  searchable?: boolean;
  clearable?: boolean;
  size?: Size;
  variant?: 'default' | 'filled' | 'borderless';
  placeholder?: string;
  loading?: boolean;
  onSearch?: (value: string) => void;
}

// Feedback props
export interface AlertProps extends BaseComponentProps {
  type?: 'success' | 'info' | 'warning' | 'error';
  variant?: AlertVariant;
  title?: React.ReactNode;
  description?: React.ReactNode;
  closable?: boolean;
  showIcon?: boolean;
  onClose?: () => void;
}

export interface ToastOptions {
  type?: 'success' | 'info' | 'warning' | 'error';
  title: string;
  description?: string;
  duration?: number;
  placement?: 'top' | 'bottom' | 'topLeft' | 'topRight' | 'bottomLeft' | 'bottomRight';
  onClose?: () => void;
}

// Data display props
export interface TagProps extends BaseComponentProps {
  color?: ColorVariant;
  variant?: 'default' | 'outlined' | 'filled' | 'rounded';
  size?: Size;
  closable?: boolean;
  onClose?: () => void;
}

export interface BadgeProps extends BaseComponentProps {
  count?: number;
  text?: string;
  color?: ColorVariant;
  variant?: 'default' | 'dot' | 'status';
  size?: Size;
  showZero?: boolean;
  overflowCount?: number;
}

export interface AvatarProps extends BaseComponentProps {
  src?: string;
  alt?: string;
  size?: Size | number;
  variant?: 'default' | 'square' | 'circle';
  icon?: React.ReactNode;
  fallback?: React.ReactNode;
}

// Layout component props
export interface ContainerProps extends BaseComponentProps {
  size?: 'small' | 'medium' | 'large' | 'full';
  centered?: boolean;
  fluid?: boolean;
}

export interface GridProps extends BaseComponentProps, ResponsiveProps {
  container?: boolean;
  item?: boolean;
  spacing?: number;
  direction?: 'row' | 'column';
  justify?: 'start' | 'center' | 'end' | 'between' | 'around' | 'evenly';
  align?: 'start' | 'center' | 'end' | 'stretch';
  wrap?: 'nowrap' | 'wrap' | 'wrap-reverse';
}

export interface FlexProps extends BaseComponentProps {
  direction?: 'row' | 'column' | 'row-reverse' | 'column-reverse';
  justify?: 'start' | 'center' | 'end' | 'between' | 'around' | 'evenly';
  align?: 'start' | 'center' | 'end' | 'stretch' | 'baseline';
  wrap?: 'nowrap' | 'wrap' | 'wrap-reverse';
  gap?: string | number;
  inline?: boolean;
}

// Theme props
export interface ThemeProps {
  mode?: 'light' | 'dark' | 'auto';
  primaryColor?: string;
  borderRadius?: number;
  fontSize?: number;
  fontFamily?: string;
}

// Animation props
export interface AnimationProps {
  duration?: number;
  easing?: string;
  delay?: number;
  direction?: 'normal' | 'reverse' | 'alternate' | 'alternate-reverse';
  fillMode?: 'none' | 'forwards' | 'backwards' | 'both';
  iterationCount?: number | 'infinite';
  playState?: 'running' | 'paused';
}

// Accessibility props
export interface AccessibilityProps {
  'aria-label'?: string;
  'aria-labelledby'?: string;
  'aria-describedby'?: string;
  'aria-expanded'?: boolean;
  'aria-hidden'?: boolean;
  'aria-disabled'?: boolean;
  'aria-selected'?: boolean;
  'aria-checked'?: boolean;
  'aria-pressed'?: boolean;
  'aria-current'?: boolean | 'page' | 'step' | 'location' | 'date' | 'time';
  role?: string;
  tabIndex?: number;
}

// Event handler types
export type ClickHandler = (event: React.MouseEvent) => void;
export type ChangeHandler<T = any> = (value: T) => void;
export type FocusHandler = (event: React.FocusEvent) => void;
export type KeyboardHandler = (event: React.KeyboardEvent) => void;
export type FormSubmitHandler = (values: any) => void;

// Utility types
export type Omit<T, K extends keyof T> = Pick<T, Exclude<keyof T, K>>;
export type Partial<T> = { [P in keyof T]?: T[P] };
export type Required<T> = { [P in keyof T]-?: T[P] };

// Component ref types
export type ButtonRef = HTMLButtonElement;
export type InputRef = HTMLInputElement;
export type TextareaRef = HTMLTextAreaElement;
export type SelectRef = HTMLSelectElement;
export type FormRef = HTMLFormElement;
export type DivRef = HTMLDivElement;

// Generic component props
export interface ComponentProps<T = HTMLElement> extends BaseComponentProps {
  ref?: React.Ref<T>;
}

// Additional type definitions for UI system
export type ColorScheme = 'light' | 'dark' | 'auto';
export type SpacingSize = 'none' | 'small' | 'medium' | 'large' | 'extra-large';
export type BorderRadius = 'none' | 'small' | 'medium' | 'large' | 'full';
export type BoxShadow = 'none' | 'small' | 'medium' | 'large' | 'extra-large';
export type AccentColor = 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';


