/**
 * Modern Navigation Component System
 * 
 * Provides a comprehensive navigation system that consolidates all navigation components.
 * Replaces AppNavigation, EnhancedNavigation, and other scattered navigation implementations.
 */

import React, { forwardRef, createContext, useContext } from 'react';
import { Menu as AntMenu, Breadcrumb as AntBreadcrumb, Tabs as AntTabs, Steps as AntSteps, Pagination as AntPagination } from 'antd';
import type { MenuProps as AntMenuProps, BreadcrumbProps as AntBreadcrumbProps, TabsProps as AntTabsProps } from 'antd';
import { useNavigate, useLocation } from 'react-router-dom';
import { designTokens } from './design-system';

// Navigation Context
interface NavigationContextValue {
  variant: string;
  size: string;
}

const NavigationContext = createContext<NavigationContextValue | null>(null);

// Types
export interface MenuProps extends Omit<AntMenuProps, 'mode'> {
  variant?: 'horizontal' | 'vertical' | 'inline' | 'sidebar';
  size?: 'small' | 'medium' | 'large';
  items: MenuItem[];
  onNavigate?: (path: string) => void;
  className?: string;
  style?: React.CSSProperties;
}

export interface MenuItem {
  key: string;
  label: React.ReactNode;
  icon?: React.ReactNode;
  path?: string;
  children?: MenuItem[];
  disabled?: boolean;
  description?: string;
  badge?: React.ReactNode;
}

export interface BreadcrumbProps extends AntBreadcrumbProps {
  items: BreadcrumbItem[];
  variant?: 'default' | 'detailed' | 'compact';
  showHome?: boolean;
}

export interface BreadcrumbItem {
  title: React.ReactNode;
  path?: string;
  icon?: React.ReactNode;
}

export interface TabsProps extends AntTabsProps {
  variant?: 'line' | 'card' | 'editable-card' | 'borderless';
  size?: 'small' | 'medium' | 'large';
}

// Menu Component
export const Menu = forwardRef<HTMLDivElement, MenuProps>(
  ({ 
    variant = 'vertical', 
    size = 'medium', 
    items, 
    onNavigate,
    className, 
    style, 
    ...props 
  }, ref) => {
    const navigate = useNavigate();
    const location = useLocation();

    const getMode = () => {
      const modeMap = {
        horizontal: 'horizontal' as const,
        vertical: 'vertical' as const,
        inline: 'inline' as const,
        sidebar: 'inline' as const,
      };
      return modeMap[variant];
    };

    const getSizeStyles = () => {
      const sizes = {
        small: {
          fontSize: designTokens.typography.fontSize.sm,
          padding: '8px 12px',
        },
        medium: {
          fontSize: designTokens.typography.fontSize.base,
          padding: '12px 16px',
        },
        large: {
          fontSize: designTokens.typography.fontSize.lg,
          padding: '16px 20px',
        },
      };
      return sizes[size];
    };

    const handleMenuClick = ({ key }: { key: string }) => {
      const item = findMenuItem(items, key);
      if (item?.path) {
        if (onNavigate) {
          onNavigate(item.path);
        } else {
          navigate(item.path);
        }
      }
    };

    const findMenuItem = (menuItems: MenuItem[], key: string): MenuItem | null => {
      for (const item of menuItems) {
        if (item.key === key) return item;
        if (item.children) {
          const found = findMenuItem(item.children, key);
          if (found) return found;
        }
      }
      return null;
    };

    const convertToAntdItems = (menuItems: MenuItem[]) => {
      return menuItems.map(item => ({
        key: item.key,
        label: (
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            {item.icon}
            <span>{item.label}</span>
            {item.badge}
          </div>
        ),
        disabled: item.disabled,
        children: item.children ? convertToAntdItems(item.children) : undefined,
      }));
    };

    const menuStyle = {
      ...getSizeStyles(),
      border: 'none',
      backgroundColor: 'transparent',
      ...style,
    };

    return (
      <AntMenu
        ref={ref}
        mode={getMode()}
        selectedKeys={[location.pathname]}
        items={convertToAntdItems(items)}
        onClick={handleMenuClick}
        className={className}
        style={menuStyle}
        {...props}
      />
    );
  }
);

Menu.displayName = 'Menu';

// Breadcrumb Component
export const Breadcrumb = forwardRef<HTMLDivElement, BreadcrumbProps>(
  ({ items, variant = 'default', showHome = true, className, style, ...props }, ref) => {
    const navigate = useNavigate();

    const handleItemClick = (path?: string) => {
      if (path) {
        navigate(path);
      }
    };

    const breadcrumbItems = [
      ...(showHome ? [{
        title: (
          <span 
            onClick={() => handleItemClick('/')}
            style={{ cursor: 'pointer' }}
          >
            Home
          </span>
        ),
      }] : []),
      ...items.map(item => ({
        title: item.path ? (
          <span 
            onClick={() => handleItemClick(item.path)}
            style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '4px' }}
          >
            {item.icon}
            {item.title}
          </span>
        ) : (
          <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
            {item.icon}
            {item.title}
          </span>
        ),
      })),
    ];

    const breadcrumbStyle = {
      ...(variant === 'detailed' && {
        padding: designTokens.spacing.md,
        backgroundColor: designTokens.colors.backgroundSecondary,
        borderRadius: designTokens.borderRadius.medium,
      }),
      ...(variant === 'compact' && {
        fontSize: designTokens.typography.fontSize.sm,
      }),
      ...style,
    };

    return (
      <AntBreadcrumb
        ref={ref}
        items={breadcrumbItems}
        className={className}
        style={breadcrumbStyle}
        {...props}
      />
    );
  }
);

Breadcrumb.displayName = 'Breadcrumb';

// Tabs Component
export const Tabs = forwardRef<HTMLDivElement, TabsProps>(
  ({ variant = 'line', size = 'medium', className, style, ...props }, ref) => {
    const getSizeStyles = () => {
      const sizes = {
        small: {
          fontSize: designTokens.typography.fontSize.sm,
        },
        medium: {
          fontSize: designTokens.typography.fontSize.base,
        },
        large: {
          fontSize: designTokens.typography.fontSize.lg,
        },
      };
      return sizes[size];
    };

    const tabsStyle = {
      ...getSizeStyles(),
      ...style,
    };

    return (
      <AntTabs
        ref={ref}
        type={variant}
        size={size}
        className={className}
        style={tabsStyle}
        {...props}
      />
    );
  }
);

Tabs.displayName = 'Tabs';

// Steps Component
export const Steps = forwardRef<HTMLDivElement, any>(
  ({ variant = 'default', size = 'medium', className, style, ...props }, ref) => {
    return (
      <AntSteps
        ref={ref}
        size={size}
        className={className}
        style={style}
        {...props}
      />
    );
  }
);

Steps.displayName = 'Steps';

// Pagination Component
export const Pagination = forwardRef<HTMLDivElement, any>(
  ({ variant = 'default', size = 'medium', className, style, ...props }, ref) => {
    return (
      <AntPagination
        ref={ref}
        size={size}
        className={className}
        style={style}
        {...props}
      />
    );
  }
);

Pagination.displayName = 'Pagination';
