/**
 * UI Components Library - Advanced Features
 *
 * This module provides advanced UI components and re-exports core components.
 * Core components have been moved to /components/core/ for better organization.
 */

// Re-export all core components for backward compatibility
export * from '../core';

// Advanced UI Components (unique to this module)
export { ThemeToggle } from './ThemeToggle';
export { ThemeCustomization } from './ThemeCustomization';
export { AnimatedChart } from './AnimatedChart';
export { AnimationPresets } from './AnimationPresets';
export { AnimationPerformanceAnalytics } from './AnimationPerformanceAnalytics';

// Re-export types for backward compatibility
export type * from './types';
export type * from '../core/types';

// Legacy compatibility utilities (will be deprecated)
// All core components are now available from '../core'

// Performance monitoring for advanced features
export const PerformanceMonitor: React.FC<{
  children: React.ReactNode;
  onMetrics?: (metrics: any) => void;
}> = ({ children, onMetrics }) => {
  React.useEffect(() => {
    if (onMetrics && process.env.NODE_ENV === 'development') {
      const metrics = {
        timestamp: Date.now(),
        renderTime: performance.now(),
      };
      onMetrics(metrics);
    }
  }, [onMetrics]);

  return <>{children}</>;
};

// Bundle analyzer for development
export const BundleAnalyzer: React.FC<{
  onAnalysis?: (analysis: any) => void;
}> = ({ onAnalysis }) => {
  React.useEffect(() => {
    if (onAnalysis && process.env.NODE_ENV === 'development') {
      const analysis = {
        timestamp: Date.now(),
        bundleSize: 'N/A', // Would be calculated in real implementation
      };
      onAnalysis(analysis);
    }
  }, [onAnalysis]);

  return null;
};

// Dialog components (using Ant Design Modal)

export interface DialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: React.ReactNode;
}

export const Dialog: React.FC<DialogProps> = ({ open, onOpenChange, children }) => (
  <Modal
    open={open}
    onCancel={() => onOpenChange(false)}
    footer={null}
    destroyOnClose
  >
    {children}
  </Modal>
);

export const DialogContent: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const DialogHeader: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{ marginBottom: '16px', ...style }}>
    {children}
  </div>
);

export const DialogTitle: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <h2 className={className} style={{ margin: 0, fontSize: '18px', fontWeight: 600, ...style }}>
    {children}
  </h2>
);

export const DialogDescription: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <p className={className} style={{ margin: '8px 0 0 0', color: '#8c8c8c', fontSize: '14px', ...style }}>
    {children}
  </p>
);

export const DialogFooter: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{
    display: 'flex',
    justifyContent: 'flex-end',
    gap: '8px',
    marginTop: '24px',
    paddingTop: '16px',
    borderTop: '1px solid #f0f0f0',
    ...style
  }}>
    {children}
  </div>
);

// Accordion components (using Ant Design Collapse)

export const Accordion: React.FC<{
  children: React.ReactNode;
  type?: 'single' | 'multiple';
  collapsible?: boolean;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, type, collapsible, className, style }) => (
  <Collapse
    accordion={type === 'single'}
    className={className}
    style={style}
  >
    {children}
  </Collapse>
);

export const AccordionItem: React.FC<{
  children: React.ReactNode;
  value: string;
  key?: string;
}> = ({ children, value, key }) => (
  <Collapse.Panel header="" key={key || value}>
    {children}
  </Collapse.Panel>
);

export const AccordionTrigger: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const AccordionContent: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

// Alert Dialog components (using Ant Design Modal)
export interface AlertDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: React.ReactNode;
}

export const AlertDialog: React.FC<AlertDialogProps> = ({ open, onOpenChange, children }) => (
  <Modal
    open={open}
    onCancel={() => onOpenChange(false)}
    footer={null}
    destroyOnClose
    centered
  >
    {children}
  </Modal>
);

export const AlertDialogContent: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const AlertDialogHeader: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{ marginBottom: '16px', ...style }}>
    {children}
  </div>
);

export const AlertDialogTitle: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <h2 className={className} style={{ margin: 0, fontSize: '18px', fontWeight: 600, color: '#ff4d4f', ...style }}>
    {children}
  </h2>
);

export const AlertDialogDescription: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <p className={className} style={{ margin: '8px 0 0 0', color: '#8c8c8c', fontSize: '14px', ...style }}>
    {children}
  </p>
);

export const AlertDialogFooter: React.FC<{
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties
}> = ({ children, className, style }) => (
  <div className={className} style={{
    display: 'flex',
    justifyContent: 'flex-end',
    gap: '8px',
    marginTop: '24px',
    paddingTop: '16px',
    borderTop: '1px solid #f0f0f0',
    ...style
  }}>
    {children}
  </div>
);

export const AlertDialogAction: React.FC<ButtonProps> = (props) => (
  <Button {...props} />
);

export const AlertDialogCancel: React.FC<ButtonProps> = (props) => (
  <Button variant="outline" {...props} />
);

// Dropdown Menu components (using Ant Design Dropdown)

export const DropdownMenu: React.FC<{
  children: React.ReactNode;
}> = ({ children }) => (
  <div>{children}</div>
);

export const DropdownMenuTrigger: React.FC<{
  children: React.ReactNode;
  asChild?: boolean;
}> = ({ children, asChild }) => (
  <div>{children}</div>
);

export const DropdownMenuContent: React.FC<{
  children: React.ReactNode;
  align?: 'start' | 'center' | 'end';
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, align, className, style }) => (
  <div className={className} style={style}>
    {children}
  </div>
);

export const DropdownMenuItem: React.FC<{
  children: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
  className?: string;
  style?: React.CSSProperties;
}> = ({ children, onClick, disabled, className, style }) => (
  <div
    className={className}
    style={{
      padding: '8px 12px',
      cursor: disabled ? 'not-allowed' : 'pointer',
      opacity: disabled ? 0.5 : 1,
      ...style
    }}
    onClick={disabled ? undefined : onClick}
  >
    {children}
  </div>
);

export const DropdownMenuSeparator: React.FC<{
  className?: string;
  style?: React.CSSProperties;
}> = ({ className, style }) => (
  <div className={className} style={{ height: '1px', background: '#f0f0f0', margin: '4px 0', ...style }} />
);

// Loading Fallback Component
export const LoadingFallback: React.FC = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '200px'
  }}>
    <div>Loading...</div>
  </div>
);



// Common spacing utilities
interface SpacerProps {
  size?: 'small' | 'medium' | 'large';
}

export const Spacer: React.FC<SpacerProps> = ({ size = 'medium' }) => {
  const getHeight = () => {
    switch (size) {
      case 'small': return 8;
      case 'large': return 32;
      default: return 16;
    }
  };

  return <div style={{ height: getHeight() }} />;
};

// Flex utilities
interface FlexContainerProps {
  children: React.ReactNode;
  direction?: 'row' | 'column';
  justify?: 'flex-start' | 'center' | 'flex-end' | 'space-between' | 'space-around';
  align?: 'flex-start' | 'center' | 'flex-end' | 'stretch';
  gap?: string;
  style?: React.CSSProperties;
}

export const FlexContainer: React.FC<FlexContainerProps> = ({
  children,
  direction = 'row',
  justify = 'flex-start',
  align = 'flex-start',
  gap = '0',
  style
}) => (
  <div style={{
    display: 'flex',
    flexDirection: direction,
    justifyContent: justify,
    alignItems: align,
    gap,
    ...style,
  }}>
    {children}
  </div>
);

// Typography
interface TitleProps {
  children: React.ReactNode;
  level?: 1 | 2 | 3 | 4 | 5;
  style?: React.CSSProperties;
}

export const Title: React.FC<TitleProps> = ({ children, level = 2, style }) => {
  const getFontSize = () => {
    switch (level) {
      case 1: return '2.5rem';
      case 2: return '2rem';
      case 3: return '1.5rem';
      case 4: return '1.25rem';
      case 5: return '1rem';
      default: return '2rem';
    }
  };

  const Tag = `h${level}` as keyof JSX.IntrinsicElements;

  return (
    <Tag style={{
      fontSize: getFontSize(),
      fontWeight: 600,
      margin: '0 0 16px 0',
      color: '#262626',
      ...style,
    }}>
      {children}
    </Tag>
  );
};

interface TextProps {
  children: React.ReactNode;
  size?: 'small' | 'medium' | 'large';
  weight?: 'normal' | 'medium' | 'bold';
  color?: string;
  style?: React.CSSProperties;
}

export const Text: React.FC<TextProps> = ({
  children,
  size = 'medium',
  weight = 'normal',
  color = '#595959',
  style
}) => {
  const getFontSize = () => {
    switch (size) {
      case 'small': return '0.875rem';
      case 'large': return '1.125rem';
      default: return '1rem';
    }
  };

  const getFontWeight = () => {
    switch (weight) {
      case 'medium': return '500';
      case 'bold': return '600';
      default: return '400';
    }
  };

  return (
    <p style={{
      fontSize: getFontSize(),
      fontWeight: getFontWeight(),
      color,
      margin: '0 0 8px 0',
      lineHeight: 1.5,
      ...style,
    }}>
      {children}
    </p>
  );
};

// Status indicators
interface StatusBadgeProps {
  children: React.ReactNode;
  status: 'success' | 'warning' | 'error' | 'info' | 'default';
  style?: React.CSSProperties;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ children, status, style }) => {
  const getStatusStyle = () => {
    switch (status) {
      case 'success':
        return {
          background: '#f6ffed',
          color: '#52c41a',
          border: '1px solid #b7eb8f',
        };
      case 'warning':
        return {
          background: '#fffbe6',
          color: '#faad14',
          border: '1px solid #ffe58f',
        };
      case 'error':
        return {
          background: '#fff2f0',
          color: '#ff4d4f',
          border: '1px solid #ffccc7',
        };
      case 'info':
        return {
          background: '#f0f5ff',
          color: '#1890ff',
          border: '1px solid #adc6ff',
        };
      default:
        return {
          background: '#fafafa',
          color: '#8c8c8c',
          border: '1px solid #d9d9d9',
        };
    }
  };

  return (
    <span style={{
      display: 'inline-flex',
      alignItems: 'center',
      padding: '4px 8px',
      borderRadius: 4,
      fontSize: '0.75rem',
      fontWeight: 500,
      textTransform: 'uppercase',
      letterSpacing: '0.5px',
      ...getStatusStyle(),
      ...style,
    }}>
      {children}
    </span>
  );
};

// Grid system
interface GridProps {
  children: React.ReactNode;
  columns?: number;
  gap?: string;
  responsive?: boolean;
  style?: React.CSSProperties;
}

export const Grid: React.FC<GridProps> = ({
  children,
  columns = 1,
  gap = '16px',
  responsive = false,
  style
}) => (
  <div style={{
    display: 'grid',
    gridTemplateColumns: `repeat(${columns}, 1fr)`,
    gap,
    ...(responsive && {
      '@media (max-width: 768px)': {
        gridTemplateColumns: '1fr',
      }
    }),
    ...style,
  }}>
    {children}
  </div>
);

// Common animations
interface AnimatedContainerProps {
  children: React.ReactNode;
  animation?: 'fadeIn' | 'slideIn';
  style?: React.CSSProperties;
}

export const AnimatedContainer: React.FC<AnimatedContainerProps> = ({
  children,
  animation,
  style
}) => {
  const getAnimationStyle = () => {
    switch (animation) {
      case 'fadeIn':
        return {
          animation: 'fadeIn 0.3s ease-in-out',
        };
      case 'slideIn':
        return {
          animation: 'slideIn 0.3s ease-in-out',
        };
      default:
        return {};
    }
  };

  return (
    <div style={{
      ...getAnimationStyle(),
      ...style,
    }}>
      {children}
    </div>
  );
};
