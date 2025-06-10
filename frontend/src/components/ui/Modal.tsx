/**
 * Modal Components
 * Advanced modal and overlay components including dialogs, drawers, and popups
 */

import React from 'react';
import { 
  Modal as AntModal,
  ModalProps as AntModalProps,
  Drawer as AntDrawer,
  DrawerProps as AntDrawerProps,
  Popover as AntPopover,
  PopoverProps as AntPopoverProps,
  Tooltip as AntTooltip,
  TooltipProps as AntTooltipProps,
  Popconfirm as AntPopconfirm,
  PopconfirmProps as AntPopconfirmProps
} from 'antd';

// Modal Component
export interface ModalProps extends AntModalProps {
  variant?: 'default' | 'fullscreen' | 'compact' | 'centered';
  size?: 'small' | 'medium' | 'large' | 'extra-large';
}

export const Modal: React.FC<ModalProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getSizeProps = () => {
    switch (size) {
      case 'small': return { width: 400 };
      case 'large': return { width: 800 };
      case 'extra-large': return { width: 1200 };
      default: return { width: 600 };
    }
  };

  const getVariantProps = () => {
    switch (variant) {
      case 'fullscreen':
        return {
          width: '100vw',
          style: { top: 0, paddingBottom: 0, maxWidth: 'none' },
          bodyStyle: { height: 'calc(100vh - 110px)', padding: 0 },
        };
      case 'compact':
        return {
          bodyStyle: { padding: 'var(--space-4)' },
        };
      case 'centered':
        return {
          centered: true,
        };
      default:
        return {};
    }
  };

  return (
    <AntModal
      {...getSizeProps()}
      {...getVariantProps()}
      {...props}
      className={`ui-modal ui-modal-${variant} ui-modal-${size} ${className || ''}`}
      style={{
        ...style,
      }}
    />
  );
};

// Drawer Component
export interface DrawerProps extends AntDrawerProps {
  variant?: 'default' | 'overlay' | 'push';
  size?: 'default' | 'large';
}

export const Drawer: React.FC<DrawerProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getSizeProps = () => {
    switch (size) {
      case 'small': return { width: 300 };
      case 'large': return { width: 600 };
      default: return { width: 400 };
    }
  };

  const getVariantProps = () => {
    switch (variant) {
      case 'overlay':
        return {
          mask: true,
          maskClosable: true,
        };
      case 'push':
        return {
          mask: false,
          maskClosable: false,
        };
      default:
        return {};
    }
  };

  return (
    <AntDrawer
      {...getSizeProps()}
      {...getVariantProps()}
      {...props}
      className={`ui-drawer ui-drawer-${variant} ui-drawer-${size} ${className || ''}`}
      style={style}
    />
  );
};

// Popover Component
export interface PopoverProps extends AntPopoverProps {
  variant?: 'default' | 'card' | 'tooltip';
  size?: 'small' | 'medium' | 'large';
}

export const Popover: React.FC<PopoverProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  overlayClassName,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'card':
        return 'ui-popover-card';
      case 'tooltip':
        return 'ui-popover-tooltip';
      default:
        return 'ui-popover-default';
    }
  };

  const getSizeClass = () => {
    switch (size) {
      case 'small': return 'ui-popover-small';
      case 'large': return 'ui-popover-large';
      default: return 'ui-popover-medium';
    }
  };

  return (
    <AntPopover
      {...props}
      className={`ui-popover ui-popover-${variant} ui-popover-${size} ${className || ''}`}
      overlayClassName={`${getVariantStyle()} ${getSizeClass()} ${overlayClassName || ''}`}
    />
  );
};

// Tooltip Component
export interface TooltipProps extends Omit<AntTooltipProps, 'className' | 'overlayClassName'> {
  variant?: 'default' | 'dark' | 'light' | 'colored';
  size?: 'small' | 'medium' | 'large';
  className?: string;
  overlayClassName?: string;
}

export const Tooltip: React.FC<TooltipProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  overlayClassName,
  ...props 
}) => {
  const getVariantClass = () => {
    switch (variant) {
      case 'dark': return 'ui-tooltip-dark';
      case 'light': return 'ui-tooltip-light';
      case 'colored': return 'ui-tooltip-colored';
      default: return 'ui-tooltip-default';
    }
  };

  const getSizeClass = () => {
    switch (size) {
      case 'small': return 'ui-tooltip-small';
      case 'large': return 'ui-tooltip-large';
      default: return 'ui-tooltip-medium';
    }
  };

  return (
    <AntTooltip
      {...props}
      className={`ui-tooltip ui-tooltip-${variant} ui-tooltip-${size} ${className || ''}`}
      overlayClassName={`${getVariantClass()} ${getSizeClass()} ${overlayClassName || ''}`}
    />
  );
};

// Popconfirm Component
export interface PopconfirmProps extends AntPopconfirmProps {
  variant?: 'default' | 'danger' | 'warning' | 'info';
}

export const Popconfirm: React.FC<PopconfirmProps> = ({ 
  variant = 'default',
  className,
  overlayClassName,
  ...props 
}) => {
  const getVariantClass = () => {
    switch (variant) {
      case 'danger': return 'ui-popconfirm-danger';
      case 'warning': return 'ui-popconfirm-warning';
      case 'info': return 'ui-popconfirm-info';
      default: return 'ui-popconfirm-default';
    }
  };

  return (
    <AntPopconfirm
      {...props}
      className={`ui-popconfirm ui-popconfirm-${variant} ${className || ''}`}
      overlayClassName={`${getVariantClass()} ${overlayClassName || ''}`}
    />
  );
};

// Confirmation Dialog Component
export interface ConfirmDialogProps {
  title: string;
  content: React.ReactNode;
  onConfirm: () => void;
  onCancel?: () => void;
  confirmText?: string;
  cancelText?: string;
  variant?: 'default' | 'danger' | 'warning' | 'info';
  loading?: boolean;
}

export const ConfirmDialog = {
  show: ({
    title,
    content,
    onConfirm,
    onCancel,
    confirmText = 'Confirm',
    cancelText = 'Cancel',
    variant = 'default',
    loading = false,
  }: ConfirmDialogProps) => {
    const getVariantProps = () => {
      switch (variant) {
        case 'danger':
          return {
            okType: 'danger' as const,
            okText: confirmText,
          };
        case 'warning':
          return {
            okType: 'primary' as const,
            okText: confirmText,
          };
        case 'info':
          return {
            okType: 'primary' as const,
            okText: confirmText,
          };
        default:
          return {
            okType: 'primary' as const,
            okText: confirmText,
          };
      }
    };

    return AntModal.confirm({
      title,
      content,
      onOk: onConfirm,
      onCancel,
      cancelText,

      className: `ui-confirm-dialog ui-confirm-dialog-${variant}`,
      ...getVariantProps(),
    });
  },
};

// Notification Component
export interface NotificationOptions {
  title: string;
  description?: React.ReactNode;
  type?: 'success' | 'info' | 'warning' | 'error';
  duration?: number;
  placement?: 'topLeft' | 'topRight' | 'bottomLeft' | 'bottomRight';
  onClick?: () => void;
  onClose?: () => void;
}

export const notification = {
  success: (options: Omit<NotificationOptions, 'type'>) => {
    // Implementation would use Ant Design notification
    console.log('Success notification:', options);
  },
  info: (options: Omit<NotificationOptions, 'type'>) => {
    console.log('Info notification:', options);
  },
  warning: (options: Omit<NotificationOptions, 'type'>) => {
    console.log('Warning notification:', options);
  },
  error: (options: Omit<NotificationOptions, 'type'>) => {
    console.log('Error notification:', options);
  },
  open: (options: NotificationOptions) => {
    console.log('Open notification:', options);
  },
};

// Backdrop Component
export interface BackdropProps {
  open: boolean;
  onClick?: () => void;
  className?: string;
  style?: React.CSSProperties;
  children?: React.ReactNode;
}

export const Backdrop: React.FC<BackdropProps> = ({
  open,
  onClick,
  className,
  style,
  children,
}) => {
  if (!open) return null;

  return (
    <div
      className={`ui-backdrop ${className || ''}`}
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        zIndex: 1000,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        ...style,
      }}
      onClick={onClick}
    >
      {children}
    </div>
  );
};

export default {
  Modal,
  Drawer,
  Popover,
  Tooltip,
  Popconfirm,
  ConfirmDialog,
  notification,
  Backdrop,
};
