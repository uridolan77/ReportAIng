/**
 * Modern Button Component System
 * 
 * Provides a comprehensive button system with variants, sizes, and compound patterns.
 * Replaces all scattered button implementations with a single, consistent system.
 */

import React, { forwardRef } from 'react';
import { Button as AntButton, Space } from 'antd';
import type { ButtonProps as AntButtonProps } from 'antd';
import { designTokens } from './design-system';

// Base Button Types
export interface ButtonProps extends Omit<AntButtonProps, 'type'> {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger' | 'success';
  size?: 'small' | 'medium' | 'large';
  fullWidth?: boolean;
  loading?: boolean;
  icon?: React.ReactNode;
  iconPosition?: 'left' | 'right';
}

export interface IconButtonProps extends Omit<ButtonProps, 'children'> {
  icon: React.ReactNode;
  'aria-label': string;
}

export interface ButtonGroupProps {
  children: React.ReactNode;
  size?: 'small' | 'medium' | 'large';
  variant?: 'horizontal' | 'vertical';
  spacing?: 'tight' | 'normal' | 'loose';
  className?: string;
  style?: React.CSSProperties;
}

// Button Component
export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ 
    variant = 'primary', 
    size = 'medium', 
    fullWidth = false,
    icon,
    iconPosition = 'left',
    children,
    style,
    className,
    ...props 
  }, ref) => {
    const getVariantStyles = () => {
      const variants = {
        primary: {
          backgroundColor: designTokens.colors.primary,
          borderColor: designTokens.colors.primary,
          color: 'white',
          '&:hover': {
            backgroundColor: designTokens.colors.primaryHover,
            borderColor: designTokens.colors.primaryHover,
          }
        },
        secondary: {
          backgroundColor: designTokens.colors.secondary,
          borderColor: designTokens.colors.secondary,
          color: designTokens.colors.text,
          '&:hover': {
            backgroundColor: designTokens.colors.secondaryHover,
            borderColor: designTokens.colors.secondaryHover,
          }
        },
        outline: {
          backgroundColor: 'transparent',
          borderColor: designTokens.colors.border,
          color: designTokens.colors.text,
          '&:hover': {
            backgroundColor: designTokens.colors.backgroundHover,
            borderColor: designTokens.colors.primary,
            color: designTokens.colors.primary,
          }
        },
        ghost: {
          backgroundColor: 'transparent',
          borderColor: 'transparent',
          color: designTokens.colors.text,
          '&:hover': {
            backgroundColor: designTokens.colors.backgroundHover,
          }
        },
        danger: {
          backgroundColor: designTokens.colors.danger,
          borderColor: designTokens.colors.danger,
          color: 'white',
          '&:hover': {
            backgroundColor: designTokens.colors.dangerHover,
            borderColor: designTokens.colors.dangerHover,
          }
        },
        success: {
          backgroundColor: designTokens.colors.success,
          borderColor: designTokens.colors.success,
          color: 'white',
          '&:hover': {
            backgroundColor: designTokens.colors.successHover,
            borderColor: designTokens.colors.successHover,
          }
        }
      };
      return variants[variant];
    };

    const getSizeStyles = () => {
      const sizes = {
        small: {
          height: '32px',
          padding: '0 12px',
          fontSize: '14px',
        },
        medium: {
          height: '40px',
          padding: '0 16px',
          fontSize: '14px',
        },
        large: {
          height: '48px',
          padding: '0 24px',
          fontSize: '16px',
        }
      };
      return sizes[size];
    };

    const buttonStyle = {
      ...getVariantStyles(),
      ...getSizeStyles(),
      width: fullWidth ? '100%' : 'auto',
      borderRadius: designTokens.borderRadius.medium,
      fontWeight: 500,
      transition: 'all 0.2s ease',
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      gap: icon && children ? '8px' : '0',
      ...style,
    };

    const content = (
      <>
        {icon && iconPosition === 'left' && icon}
        {children}
        {icon && iconPosition === 'right' && icon}
      </>
    );

    return (
      <AntButton
        ref={ref}
        className={className}
        style={buttonStyle}
        {...props}
      >
        {content}
      </AntButton>
    );
  }
);

Button.displayName = 'Button';

// Icon Button Component
export const IconButton = forwardRef<HTMLButtonElement, IconButtonProps>(
  ({ icon, size = 'medium', variant = 'ghost', style, ...props }, ref) => {
    const getSizeStyles = () => {
      const sizes = {
        small: { width: '32px', height: '32px', padding: '0' },
        medium: { width: '40px', height: '40px', padding: '0' },
        large: { width: '48px', height: '48px', padding: '0' }
      };
      return sizes[size];
    };

    return (
      <Button
        ref={ref}
        variant={variant}
        size={size}
        style={{
          ...getSizeStyles(),
          ...style,
        }}
        {...props}
      >
        {icon}
      </Button>
    );
  }
);

IconButton.displayName = 'IconButton';

// Button Group Component
export const ButtonGroup: React.FC<ButtonGroupProps> = ({
  children,
  size = 'medium',
  variant = 'horizontal',
  spacing = 'normal',
  className,
  style,
}) => {
  const getSpacing = () => {
    const spacings = {
      tight: '4px',
      normal: '8px',
      loose: '16px'
    };
    return spacings[spacing];
  };

  const groupStyle = {
    display: 'flex',
    flexDirection: variant === 'vertical' ? 'column' : 'row',
    gap: getSpacing(),
    ...style,
  };

  return (
    <Space.Compact
      direction={variant === 'vertical' ? 'vertical' : 'horizontal'}
      size={spacing === 'tight' ? 'small' : spacing === 'loose' ? 'large' : 'middle'}
      className={className}
      style={groupStyle}
    >
      {children}
    </Space.Compact>
  );
};

ButtonGroup.displayName = 'ButtonGroup';
