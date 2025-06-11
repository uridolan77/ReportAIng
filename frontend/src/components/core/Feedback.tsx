/**
 * Modern Feedback Components
 * 
 * Provides comprehensive feedback components including alerts, notifications,
 * progress indicators, skeletons, and spinners.
 */

import React, { forwardRef } from 'react';
import { Alert as AntAlert, Progress as AntProgress, Skeleton as AntSkeleton, Spin } from 'antd';
import type { AlertProps as AntAlertProps, ProgressProps as AntProgressProps } from 'antd';
import { designTokens } from './design-system';

// Types
export interface AlertProps extends Omit<AntAlertProps, 'type'> {
  variant?: 'info' | 'success' | 'warning' | 'error';
  size?: 'small' | 'medium' | 'large';
  icon?: React.ReactNode;
  closable?: boolean;
  onClose?: () => void;
}

export interface NotificationProps {
  title: string;
  message?: string;
  variant?: 'info' | 'success' | 'warning' | 'error';
  duration?: number;
  placement?: 'topLeft' | 'topRight' | 'bottomLeft' | 'bottomRight';
  onClose?: () => void;
}

export interface ProgressProps extends AntProgressProps {
  variant?: 'line' | 'circle' | 'dashboard';
  size?: 'small' | 'medium' | 'large';
  showInfo?: boolean;
  status?: 'normal' | 'exception' | 'active' | 'success';
}

export interface SkeletonProps {
  variant?: 'text' | 'circular' | 'rectangular' | 'article' | 'list';
  width?: string | number;
  height?: string | number;
  lines?: number;
  animation?: 'pulse' | 'wave' | false;
  className?: string;
  style?: React.CSSProperties;
}

export interface SpinnerProps {
  size?: 'small' | 'medium' | 'large';
  variant?: 'default' | 'dots' | 'bars';
  color?: string;
  className?: string;
  style?: React.CSSProperties;
}

// Alert Component
export const Alert = forwardRef<HTMLDivElement, AlertProps>(
  ({ 
    variant = 'info', 
    size = 'medium', 
    icon, 
    closable = false,
    onClose,
    className, 
    style, 
    ...props 
  }, ref) => {
    const getSizeStyles = () => {
      const sizes = {
        small: {
          padding: `${designTokens.spacing.sm} ${designTokens.spacing.md}`,
          fontSize: designTokens.typography.fontSize.sm,
        },
        medium: {
          padding: `${designTokens.spacing.md} ${designTokens.spacing.lg}`,
          fontSize: designTokens.typography.fontSize.base,
        },
        large: {
          padding: `${designTokens.spacing.lg} ${designTokens.spacing.xl}`,
          fontSize: designTokens.typography.fontSize.lg,
        },
      };
      return sizes[size];
    };

    const alertStyle = {
      ...getSizeStyles(),
      borderRadius: designTokens.borderRadius.medium,
      ...style,
    };

    return (
      <AntAlert
        ref={ref}
        type={variant}
        icon={icon}
        closable={closable}
        onClose={onClose}
        className={className}
        style={alertStyle}
        {...props}
      />
    );
  }
);

Alert.displayName = 'Alert';

// Notification Component (Static implementation)
export const Notification: React.FC<NotificationProps> = ({
  title,
  message,
  variant = 'info',
  duration = 4500,
  placement = 'topRight',
  onClose,
}) => {
  // This would typically integrate with a notification system like react-toastify
  // For now, we'll provide a basic implementation
  return (
    <div
      style={{
        position: 'fixed',
        top: placement.includes('top') ? '20px' : 'auto',
        bottom: placement.includes('bottom') ? '20px' : 'auto',
        left: placement.includes('Left') ? '20px' : 'auto',
        right: placement.includes('Right') ? '20px' : 'auto',
        backgroundColor: designTokens.colors.white,
        border: `1px solid ${designTokens.colors.border}`,
        borderRadius: designTokens.borderRadius.medium,
        boxShadow: designTokens.shadows.large,
        padding: designTokens.spacing.lg,
        minWidth: '300px',
        zIndex: designTokens.zIndex.toast,
      }}
    >
      <div
        style={{
          fontWeight: designTokens.typography.fontWeight.semibold,
          marginBottom: message ? designTokens.spacing.sm : 0,
          color: designTokens.colors.text,
        }}
      >
        {title}
      </div>
      {message && (
        <div
          style={{
            color: designTokens.colors.textSecondary,
            fontSize: designTokens.typography.fontSize.sm,
          }}
        >
          {message}
        </div>
      )}
    </div>
  );
};

// Progress Component
export const Progress = forwardRef<HTMLDivElement, ProgressProps>(
  ({ 
    variant = 'line', 
    size = 'medium', 
    showInfo = true,
    status = 'normal',
    className, 
    style, 
    ...props 
  }, ref) => {
    const getSizeStyles = () => {
      const sizes = {
        small: { strokeWidth: 6 },
        medium: { strokeWidth: 8 },
        large: { strokeWidth: 10 },
      };
      return sizes[size];
    };

    return (
      <AntProgress
        ref={ref}
        type={variant}
        size={size}
        showInfo={showInfo}
        status={status}
        className={className}
        style={style}
        {...getSizeStyles()}
        {...props}
      />
    );
  }
);

Progress.displayName = 'Progress';

// Skeleton Component
export const Skeleton = forwardRef<HTMLDivElement, SkeletonProps>(
  ({ 
    variant = 'text', 
    width, 
    height, 
    lines = 3,
    animation = 'pulse',
    className, 
    style 
  }, ref) => {
    const getVariantProps = () => {
      switch (variant) {
        case 'text':
          return {
            paragraph: { rows: lines, width: width || '100%' },
            title: false,
            avatar: false,
          };
        case 'circular':
          return {
            avatar: { shape: 'circle', size: width || 40 },
            paragraph: false,
            title: false,
          };
        case 'rectangular':
          return {
            paragraph: false,
            title: false,
            avatar: false,
            children: (
              <div
                style={{
                  width: width || '100%',
                  height: height || '200px',
                  backgroundColor: designTokens.colors.backgroundSecondary,
                  borderRadius: designTokens.borderRadius.medium,
                }}
              />
            ),
          };
        case 'article':
          return {
            avatar: true,
            paragraph: { rows: 4 },
            title: true,
          };
        case 'list':
          return {
            avatar: true,
            paragraph: { rows: 2 },
            title: false,
          };
        default:
          return {};
      }
    };

    return (
      <AntSkeleton
        ref={ref}
        active={animation !== false}
        className={className}
        style={style}
        {...getVariantProps()}
      />
    );
  }
);

Skeleton.displayName = 'Skeleton';

// Spinner Component
export const Spinner = forwardRef<HTMLDivElement, SpinnerProps>(
  ({ 
    size = 'medium', 
    variant = 'default',
    color = designTokens.colors.primary,
    className, 
    style 
  }, ref) => {
    const getSizeValue = () => {
      const sizes = {
        small: 16,
        medium: 24,
        large: 32,
      };
      return sizes[size];
    };

    const getSpinnerContent = () => {
      const sizeValue = getSizeValue();
      
      switch (variant) {
        case 'dots':
          return (
            <div
              style={{
                display: 'flex',
                gap: '4px',
                alignItems: 'center',
              }}
            >
              {[0, 1, 2].map((i) => (
                <div
                  key={i}
                  style={{
                    width: sizeValue / 4,
                    height: sizeValue / 4,
                    backgroundColor: color,
                    borderRadius: '50%',
                    animation: `pulse 1.4s ease-in-out ${i * 0.16}s infinite both`,
                  }}
                />
              ))}
            </div>
          );
        case 'bars':
          return (
            <div
              style={{
                display: 'flex',
                gap: '2px',
                alignItems: 'center',
                height: sizeValue,
              }}
            >
              {[0, 1, 2, 3].map((i) => (
                <div
                  key={i}
                  style={{
                    width: sizeValue / 8,
                    height: '100%',
                    backgroundColor: color,
                    animation: `bars 1.2s ease-in-out ${i * 0.1}s infinite`,
                  }}
                />
              ))}
            </div>
          );
        default:
          return (
            <div
              style={{
                width: sizeValue,
                height: sizeValue,
                border: `2px solid ${designTokens.colors.border}`,
                borderTop: `2px solid ${color}`,
                borderRadius: '50%',
                animation: 'spin 1s linear infinite',
              }}
            />
          );
      }
    };

    return (
      <div
        ref={ref}
        className={className}
        style={{
          display: 'inline-flex',
          alignItems: 'center',
          justifyContent: 'center',
          ...style,
        }}
      >
        {getSpinnerContent()}
      </div>
    );
  }
);

Spinner.displayName = 'Spinner';
