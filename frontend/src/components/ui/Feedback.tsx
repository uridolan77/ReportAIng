/**
 * Feedback Components
 * User feedback components including alerts, notifications, and loading states
 */

import React from 'react';
import { 
  Alert as AntAlert,
  AlertProps as AntAlertProps,
  Spin as AntSpin,
  SpinProps as AntSpinProps,
  Progress as AntProgress,
  ProgressProps as AntProgressProps,
  Result as AntResult,
  ResultProps as AntResultProps,
  Skeleton as AntSkeleton,
  SkeletonProps as AntSkeletonProps,
  Empty as AntEmpty,
  EmptyProps as AntEmptyProps
} from 'antd';

// Alert Component
export interface AlertProps extends AntAlertProps {
  variant?: 'default' | 'filled' | 'outlined';
}

export const Alert: React.FC<AlertProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'filled':
        return {
          border: 'none',
          fontWeight: '500',
        };
      case 'outlined':
        return {
          backgroundColor: 'transparent',
          border: '2px solid',
        };
      default:
        return {};
    }
  };

  return (
    <AntAlert
      {...props}
      className={`ui-alert ui-alert-${variant} ${className || ''}`}
      style={{
        borderRadius: 'var(--radius-lg)',
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Loading Spinner Component
export interface SpinProps extends AntSpinProps {
  variant?: 'default' | 'overlay' | 'inline';
}

export const Spin: React.FC<SpinProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'overlay':
        return {
          position: 'absolute' as const,
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(255, 255, 255, 0.8)',
          zIndex: 1000,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        };
      case 'inline':
        return {
          display: 'inline-flex',
          alignItems: 'center',
          gap: 'var(--space-2)',
        };
      default:
        return {};
    }
  };

  return (
    <AntSpin
      {...props}
      className={`ui-spin ui-spin-${variant} ${className || ''}`}
      style={{
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Progress Component
export interface ProgressProps extends AntProgressProps {
  variant?: 'default' | 'circle' | 'dashboard';
  color?: 'primary' | 'success' | 'warning' | 'error';
}

export const Progress: React.FC<ProgressProps> = ({ 
  variant = 'default',
  color = 'primary',
  className,
  style,
  ...props 
}) => {
  const getColorStyle = () => {
    switch (color) {
      case 'success': return { strokeColor: 'var(--color-success)' };
      case 'warning': return { strokeColor: 'var(--color-warning)' };
      case 'error': return { strokeColor: 'var(--color-error)' };
      default: return { strokeColor: 'var(--color-primary)' };
    }
  };

  const getType = () => {
    switch (variant) {
      case 'circle': return 'circle';
      case 'dashboard': return 'dashboard';
      default: return 'line';
    }
  };

  return (
    <AntProgress
      {...props}
      type={getType()}
      className={`ui-progress ui-progress-${variant} ${className || ''}`}
      style={style}
      {...getColorStyle()}
    />
  );
};

// Result Component
export interface ResultProps extends AntResultProps {
  variant?: 'default' | 'compact' | 'detailed';
}

export const Result: React.FC<ResultProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'compact':
        return {
          padding: 'var(--space-4)',
        };
      case 'detailed':
        return {
          padding: 'var(--space-8)',
        };
      default:
        return {
          padding: 'var(--space-6)',
        };
    }
  };

  return (
    <AntResult
      {...props}
      className={`ui-result ui-result-${variant} ${className || ''}`}
      style={{
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Skeleton Component
export interface SkeletonProps extends AntSkeletonProps {
  variant?: 'default' | 'card' | 'list' | 'article';
}

export const Skeleton: React.FC<SkeletonProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantProps = () => {
    switch (variant) {
      case 'card':
        return {
          avatar: true,
          paragraph: { rows: 2 },
          title: true,
        };
      case 'list':
        return {
          avatar: true,
          paragraph: { rows: 1 },
          title: false,
        };
      case 'article':
        return {
          avatar: false,
          paragraph: { rows: 4 },
          title: true,
        };
      default:
        return {};
    }
  };

  return (
    <AntSkeleton
      {...getVariantProps()}
      {...props}
      className={`ui-skeleton ui-skeleton-${variant} ${className || ''}`}
      style={style}
    />
  );
};

// Empty State Component
export interface EmptyProps extends AntEmptyProps {
  variant?: 'default' | 'simple' | 'detailed';
  icon?: React.ReactNode;
  title?: string;
  description?: React.ReactNode;
  actions?: React.ReactNode;
}

export const Empty: React.FC<EmptyProps> = ({ 
  variant = 'default',
  icon,
  title,
  description,
  actions,
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'simple':
        return {
          padding: 'var(--space-4)',
        };
      case 'detailed':
        return {
          padding: 'var(--space-8)',
        };
      default:
        return {
          padding: 'var(--space-6)',
        };
    }
  };

  return (
    <div
      className={`ui-empty ui-empty-${variant} ${className || ''}`}
      style={{
        textAlign: 'center' as const,
        ...getVariantStyle(),
        ...style,
      }}
    >
      {icon && (
        <div style={{ 
          fontSize: '48px', 
          color: 'var(--text-tertiary)', 
          marginBottom: 'var(--space-4)' 
        }}>
          {icon}
        </div>
      )}
      
      {title && (
        <h3 style={{ 
          margin: '0 0 var(--space-2) 0',
          color: 'var(--text-primary)',
          fontSize: 'var(--font-size-lg)',
          fontWeight: 600
        }}>
          {title}
        </h3>
      )}
      
      {description && (
        <div style={{ 
          color: 'var(--text-secondary)',
          fontSize: 'var(--font-size-sm)',
          marginBottom: actions ? 'var(--space-4)' : 0
        }}>
          {description}
        </div>
      )}
      
      {actions && (
        <div style={{ marginTop: 'var(--space-4)' }}>
          {actions}
        </div>
      )}
      
      {!icon && !title && !description && !actions && (
        <AntEmpty {...props} />
      )}
    </div>
  );
};

// Toast Notification (using Ant Design message)
export interface ToastOptions {
  type?: 'success' | 'error' | 'warning' | 'info' | 'loading';
  content: React.ReactNode;
  duration?: number;
  onClose?: () => void;
}

export const toast = {
  success: (content: React.ReactNode, duration?: number) => {
    // Implementation would use Ant Design message
    console.log('Success toast:', content);
  },
  error: (content: React.ReactNode, duration?: number) => {
    console.log('Error toast:', content);
  },
  warning: (content: React.ReactNode, duration?: number) => {
    console.log('Warning toast:', content);
  },
  info: (content: React.ReactNode, duration?: number) => {
    console.log('Info toast:', content);
  },
  loading: (content: React.ReactNode, duration?: number) => {
    console.log('Loading toast:', content);
  },
};

export default {
  Alert,
  Spin,
  Progress,
  Result,
  Skeleton,
  Empty,
  toast,
};
