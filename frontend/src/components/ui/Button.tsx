/**
 * Button Component
 * Enhanced button component with multiple variants and consistent styling
 */

import React from 'react';
import { Button as AntButton, ButtonProps as AntButtonProps } from 'antd';

export interface ButtonProps extends Omit<AntButtonProps, 'variant'> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger' | 'outline' | 'default';
  size?: 'small' | 'large';
  fullWidth?: boolean;
}

export const Button: React.FC<ButtonProps> = ({ 
  variant = 'default', 
  size = 'medium',
  fullWidth = false,
  style, 
  className,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'primary':
        return {
          background: 'var(--color-primary)',
          borderColor: 'var(--color-primary)',
          color: 'white',
        };
      case 'secondary':
        return {
          background: 'var(--bg-secondary)',
          borderColor: 'var(--border-primary)',
          color: 'var(--text-primary)',
        };
      case 'ghost':
        return {
          background: 'transparent',
          borderColor: 'transparent',
          color: 'var(--color-primary)',
        };
      case 'danger':
        return {
          background: 'var(--color-error)',
          borderColor: 'var(--color-error)',
          color: 'white',
        };
      case 'outline':
        return {
          background: 'transparent',
          borderColor: 'var(--border-primary)',
          color: 'var(--text-primary)',
        };
      default:
        return {
          background: 'var(--color-primary)',
          borderColor: 'var(--color-primary)',
          color: 'white',
        };
    }
  };

  const getSizeStyle = () => {
    switch (size) {
      case 'small':
        return {
          height: '32px',
          padding: '0 var(--space-3)',
          fontSize: 'var(--font-size-sm)',
        };
      case 'large':
        return {
          height: '48px',
          padding: '0 var(--space-6)',
          fontSize: 'var(--font-size-lg)',
        };
      default:
        return {
          height: '40px',
          padding: '0 var(--space-4)',
          fontSize: 'var(--font-size-md)',
        };
    }
  };

  return (
    <AntButton
      {...props}
      className={`ui-button ${className || ''}`}
      style={{
        borderRadius: 'var(--radius-md)',
        fontWeight: '500',
        transition: 'all var(--transition-fast)',
        border: '1px solid',
        cursor: 'pointer',
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 'var(--space-2)',
        width: fullWidth ? '100%' : 'auto',
        ...getVariantStyle(),
        ...getSizeStyle(),
        ...style,
      }}
    />
  );
};

// Button Group Component
export interface ButtonGroupProps {
  children: React.ReactNode;
  orientation?: 'horizontal' | 'vertical';
  spacing?: 'small' | 'medium' | 'large';
  className?: string;
  style?: React.CSSProperties;
}

export const ButtonGroup: React.FC<ButtonGroupProps> = ({
  children,
  orientation = 'horizontal',
  spacing = 'medium',
  className,
  style
}) => {
  const getSpacing = () => {
    switch (spacing) {
      case 'small': return 'var(--space-2)';
      case 'large': return 'var(--space-4)';
      default: return 'var(--space-3)';
    }
  };

  return (
    <div
      className={`ui-button-group ${className || ''}`}
      style={{
        display: 'flex',
        flexDirection: orientation === 'vertical' ? 'column' : 'row',
        gap: getSpacing(),
        ...style,
      }}
    >
      {children}
    </div>
  );
};

// Icon Button Component
export interface IconButtonProps extends Omit<ButtonProps, 'children'> {
  icon: React.ReactNode;
  'aria-label': string;
}

export const IconButton: React.FC<IconButtonProps> = ({
  icon,
  size = 'medium',
  variant = 'ghost',
  style,
  ...props
}) => {
  const getIconSize = () => {
    switch (size) {
      case 'small': return '32px';
      case 'large': return '48px';
      default: return '40px';
    }
  };

  const getButtonSize = () => {
    switch (size) {
      case 'small': return 'small';
      case 'large': return 'large';
      default: return 'large'; // Default to large since 'medium' is not supported
    }
  };

  return (
    <Button
      {...props}
      variant={variant}
      size={getButtonSize()}
      style={{
        width: getIconSize(),
        height: getIconSize(),
        padding: 0,
        minWidth: 'auto',
        ...style,
      }}
    >
      {icon}
    </Button>
  );
};

// Loading Button Component
export interface LoadingButtonProps extends ButtonProps {
  loading?: boolean;
  loadingText?: string;
}

export const LoadingButton: React.FC<LoadingButtonProps> = ({
  loading = false,
  loadingText = 'Loading...',
  children,
  disabled,
  ...props
}) => {
  return (
    <Button
      {...props}
      disabled={disabled || loading}
      style={{
        opacity: loading ? 0.7 : 1,
        cursor: loading ? 'not-allowed' : 'pointer',
        ...props.style,
      }}
    >
      {loading ? (
        <>
          <span style={{ marginRight: 'var(--space-2)' }}>⏳</span>
          {loadingText}
        </>
      ) : (
        children
      )}
    </Button>
  );
};

export default Button;
