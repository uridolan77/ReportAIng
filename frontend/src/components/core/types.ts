/**
 * Core Component Types
 * 
 * Comprehensive type definitions for all core UI components.
 * Provides consistent typing across the entire component system.
 */

import { ReactNode, CSSProperties, MouseEvent, ChangeEvent } from 'react';

// Base Types
export type Size = 'small' | 'medium' | 'large';
export type Variant = 'default' | 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger' | 'success';
export type ColorScheme = 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';
export type SpacingSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | '5xl';
export type BorderRadius = 'none' | 'small' | 'medium' | 'large' | 'xl' | 'full';
export type BoxShadow = 'none' | 'small' | 'medium' | 'large' | 'xl' | 'inner';
export type AccentColor = 'blue' | 'green' | 'red' | 'yellow' | 'purple' | 'pink' | 'gray';

// Component State Types
export type ComponentState = 'idle' | 'loading' | 'success' | 'error';
export type InteractionState = 'default' | 'hover' | 'active' | 'focus' | 'disabled';

// Event Handler Types
export type ClickHandler = (event: MouseEvent<HTMLElement>) => void;
export type ChangeHandler<T = any> = (value: T) => void;
export type InputChangeHandler = (event: ChangeEvent<HTMLInputElement>) => void;
export type SelectChangeHandler = (value: string | string[]) => void;

// Base Component Props
export interface BaseComponentProps {
  className?: string;
  style?: CSSProperties;
  children?: ReactNode;
  id?: string;
  'data-testid'?: string;
}

// Layout Props
export interface LayoutProps extends BaseComponentProps {
  padding?: SpacingSize | boolean;
  margin?: SpacingSize | boolean;
  width?: string | number;
  height?: string | number;
  maxWidth?: string | number;
  maxHeight?: string | number;
  minWidth?: string | number;
  minHeight?: string | number;
}

// Interactive Props
export interface InteractiveProps extends BaseComponentProps {
  disabled?: boolean;
  loading?: boolean;
  onClick?: ClickHandler;
  onFocus?: (event: React.FocusEvent) => void;
  onBlur?: (event: React.FocusEvent) => void;
  onKeyDown?: (event: React.KeyboardEvent) => void;
  tabIndex?: number;
  'aria-label'?: string;
  'aria-describedby'?: string;
  'aria-expanded'?: boolean;
  'aria-selected'?: boolean;
}

// Form Props
export interface FormFieldProps extends InteractiveProps {
  name?: string;
  value?: any;
  defaultValue?: any;
  onChange?: ChangeHandler;
  onValidate?: (value: any) => string | null;
  required?: boolean;
  placeholder?: string;
  error?: string;
  helperText?: string;
  label?: string;
}

// Visual Props
export interface VisualProps extends BaseComponentProps {
  variant?: Variant;
  size?: Size;
  color?: ColorScheme;
  backgroundColor?: string;
  borderColor?: string;
  borderRadius?: BorderRadius;
  shadow?: BoxShadow;
  opacity?: number;
}

// Animation Props
export interface AnimationProps {
  animate?: boolean;
  duration?: number;
  delay?: number;
  easing?: 'linear' | 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out';
  transition?: string;
}

// Responsive Props
export interface ResponsiveProps {
  responsive?: boolean;
  breakpoint?: 'sm' | 'md' | 'lg' | 'xl' | '2xl';
  hideOn?: 'mobile' | 'tablet' | 'desktop';
  showOn?: 'mobile' | 'tablet' | 'desktop';
}

// Accessibility Props
export interface AccessibilityProps {
  'aria-label'?: string;
  'aria-labelledby'?: string;
  'aria-describedby'?: string;
  'aria-expanded'?: boolean;
  'aria-selected'?: boolean;
  'aria-checked'?: boolean;
  'aria-disabled'?: boolean;
  'aria-hidden'?: boolean;
  'aria-live'?: 'off' | 'polite' | 'assertive';
  'aria-atomic'?: boolean;
  'aria-relevant'?: string;
  role?: string;
  tabIndex?: number;
}

// Theme Props
export interface ThemeProps {
  theme?: 'light' | 'dark' | 'auto';
  colorMode?: 'light' | 'dark';
  accentColor?: AccentColor;
  borderRadius?: BorderRadius;
  fontFamily?: string;
  fontSize?: string;
}

// Component-Specific Types

// Button Types
export interface ButtonBaseProps extends InteractiveProps, VisualProps, AnimationProps {
  type?: 'button' | 'submit' | 'reset';
  fullWidth?: boolean;
  icon?: ReactNode;
  iconPosition?: 'left' | 'right';
  href?: string;
  target?: '_blank' | '_self' | '_parent' | '_top';
}

// Card Types
export interface CardBaseProps extends LayoutProps, VisualProps, AnimationProps {
  interactive?: boolean;
  hoverable?: boolean;
  selectable?: boolean;
  selected?: boolean;
  onSelect?: ChangeHandler<boolean>;
}

// Input Types
export interface InputBaseProps extends FormFieldProps, VisualProps {
  type?: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'search';
  autoComplete?: string;
  autoFocus?: boolean;
  readOnly?: boolean;
  maxLength?: number;
  minLength?: number;
  pattern?: string;
  step?: number;
  min?: number;
  max?: number;
}

// Select Types
export interface SelectOption {
  label: string;
  value: string | number;
  disabled?: boolean;
  group?: string;
}

export interface SelectBaseProps extends FormFieldProps, VisualProps {
  options: SelectOption[];
  multiple?: boolean;
  searchable?: boolean;
  clearable?: boolean;
  placeholder?: string;
  noOptionsMessage?: string;
  loadingMessage?: string;
}

// Modal Types
export interface ModalBaseProps extends BaseComponentProps, AnimationProps {
  open: boolean;
  onClose: () => void;
  closeOnEscape?: boolean;
  closeOnOverlayClick?: boolean;
  showCloseButton?: boolean;
  size?: Size | 'xs' | 'xl' | '2xl' | '3xl' | '4xl' | '5xl' | 'full';
  centered?: boolean;
  scrollBehavior?: 'inside' | 'outside';
}

// Table Types
export interface TableColumn<T = any> {
  key: string;
  title: ReactNode;
  dataIndex?: string;
  render?: (value: any, record: T, index: number) => ReactNode;
  width?: string | number;
  align?: 'left' | 'center' | 'right';
  sortable?: boolean;
  filterable?: boolean;
  fixed?: 'left' | 'right';
}

export interface TableBaseProps<T = any> extends BaseComponentProps, VisualProps {
  columns: TableColumn<T>[];
  data: T[];
  loading?: boolean;
  pagination?: boolean | object;
  selection?: {
    type: 'checkbox' | 'radio';
    selectedRowKeys?: string[];
    onChange?: (selectedRowKeys: string[], selectedRows: T[]) => void;
  };
  onRowClick?: (record: T, index: number) => void;
  onSort?: (column: TableColumn<T>, direction: 'asc' | 'desc') => void;
  onFilter?: (filters: Record<string, any>) => void;
}

// Navigation Types
export interface NavigationItem {
  key: string;
  label: ReactNode;
  icon?: ReactNode;
  path?: string;
  children?: NavigationItem[];
  disabled?: boolean;
  badge?: ReactNode;
  description?: string;
}

export interface NavigationBaseProps extends BaseComponentProps, VisualProps {
  items: NavigationItem[];
  mode?: 'horizontal' | 'vertical' | 'inline';
  collapsed?: boolean;
  selectedKeys?: string[];
  openKeys?: string[];
  onSelect?: (selectedKeys: string[]) => void;
  onOpenChange?: (openKeys: string[]) => void;
}

// Chart Types
export interface ChartDataPoint {
  x: string | number;
  y: number;
  label?: string;
  color?: string;
  [key: string]: any;
}

export interface ChartBaseProps extends BaseComponentProps, VisualProps {
  data: ChartDataPoint[];
  type?: 'line' | 'bar' | 'pie' | 'area' | 'scatter' | 'donut';
  width?: number;
  height?: number;
  responsive?: boolean;
  animated?: boolean;
  legend?: boolean;
  tooltip?: boolean;
  grid?: boolean;
  theme?: 'light' | 'dark';
}

// Utility Types
export type PropsWithVariant<T, V extends string> = T & { variant?: V };
export type PropsWithSize<T> = T & { size?: Size };
export type PropsWithColor<T> = T & { color?: ColorScheme };
export type PropsWithChildren<T> = T & { children: ReactNode };
export type PropsWithOptionalChildren<T> = T & { children?: ReactNode };

// Generic Component Props
export type ComponentProps<T extends keyof JSX.IntrinsicElements> = 
  BaseComponentProps & 
  Omit<React.ComponentProps<T>, keyof BaseComponentProps>;

// Export utility type for component refs
export type ComponentRef<T> = React.Ref<T>;

// Export common prop combinations
export type StandardComponentProps = BaseComponentProps & VisualProps & InteractiveProps;
export type FormComponentProps = FormFieldProps & VisualProps;
export type LayoutComponentProps = LayoutProps & VisualProps;
export type NavigationComponentProps = NavigationBaseProps & ResponsiveProps;
