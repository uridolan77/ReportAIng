/**
 * Navigation Components
 * Advanced navigation components including menus, breadcrumbs, and tabs
 */

import React from 'react';
import { 
  Menu as AntMenu,
  MenuProps as AntMenuProps,
  Breadcrumb as AntBreadcrumb,
  BreadcrumbProps as AntBreadcrumbProps,
  Tabs as AntTabs,
  TabsProps as AntTabsProps,
  Steps as AntSteps,
  StepsProps as AntStepsProps,
  Pagination as AntPagination,
  PaginationProps as AntPaginationProps,
  Anchor as AntAnchor,
  AnchorProps as AntAnchorProps
} from 'antd';

const { Item: MenuItem, SubMenu, ItemGroup: MenuItemGroup } = AntMenu;
const { Item: BreadcrumbItem } = AntBreadcrumb;
const { TabPane } = AntTabs;
const { Step } = AntSteps;
const { Link: AnchorLink } = AntAnchor;

// Menu Component
export interface MenuProps extends AntMenuProps {
  variant?: 'default' | 'horizontal' | 'vertical' | 'inline';
  size?: 'small' | 'medium' | 'large';
}

export const Menu: React.FC<MenuProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getMode = () => {
    switch (variant) {
      case 'horizontal': return 'horizontal';
      case 'vertical': return 'vertical';
      case 'inline': return 'inline';
      default: return 'vertical';
    }
  };

  const getSizeStyle = () => {
    switch (size) {
      case 'small': return { fontSize: 'var(--font-size-sm)' };
      case 'large': return { fontSize: 'var(--font-size-lg)' };
      default: return {};
    }
  };

  return (
    <AntMenu
      {...props}
      mode={getMode()}
      className={`ui-menu ui-menu-${variant} ui-menu-${size} ${className || ''}`}
      style={{
        border: 'none',
        backgroundColor: 'transparent',
        ...getSizeStyle(),
        ...style,
      }}
    />
  );
};

// Breadcrumb Component
export interface BreadcrumbProps extends AntBreadcrumbProps {
  variant?: 'default' | 'compact' | 'detailed';
}

export const Breadcrumb: React.FC<BreadcrumbProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'compact':
        return {
          fontSize: 'var(--font-size-sm)',
          padding: 'var(--space-1) 0',
        };
      case 'detailed':
        return {
          fontSize: 'var(--font-size-md)',
          padding: 'var(--space-3) 0',
        };
      default:
        return {
          fontSize: 'var(--font-size-sm)',
          padding: 'var(--space-2) 0',
        };
    }
  };

  return (
    <AntBreadcrumb
      {...props}
      className={`ui-breadcrumb ui-breadcrumb-${variant} ${className || ''}`}
      style={{
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Tabs Component
export interface TabsProps extends AntTabsProps {
  variant?: 'default' | 'card' | 'editable-card' | 'line';
  size?: 'small' | 'large';
}

export const Tabs: React.FC<TabsProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getType = () => {
    switch (variant) {
      case 'card': return 'card';
      case 'editable-card': return 'editable-card';
      case 'line': return 'line';
      default: return 'line';
    }
  };

  const getSize = () => {
    switch (size) {
      case 'small': return 'small';
      case 'large': return 'large';
      default: return 'middle';
    }
  };

  return (
    <AntTabs
      {...props}
      type={getType()}
      size={getSize()}
      className={`ui-tabs ui-tabs-${variant} ui-tabs-${size} ${className || ''}`}
      style={{
        ...style,
      }}
    />
  );
};

// Steps Component
export interface StepsProps extends AntStepsProps {
  variant?: 'default' | 'navigation' | 'dot';
  size?: 'small' | 'default';
}

export const Steps: React.FC<StepsProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getType = () => {
    switch (variant) {
      case 'navigation': return 'navigation';
      case 'dot': return 'inline'; // Map 'dot' to 'inline' since 'dot' is not supported
      default: return 'default';
    }
  };

  const getSize = () => {
    switch (size) {
      case 'small': return 'small';
      case 'large': return 'default';
      default: return 'default';
    }
  };

  return (
    <AntSteps
      {...props}
      type={getType()}
      size={getSize()}
      className={`ui-steps ui-steps-${variant} ui-steps-${size} ${className || ''}`}
      style={style}
    />
  );
};

// Pagination Component
export interface PaginationProps extends AntPaginationProps {
  variant?: 'default' | 'simple' | 'mini';
}

export const Pagination: React.FC<PaginationProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantProps = () => {
    switch (variant) {
      case 'simple':
        return {
          simple: true,
          showSizeChanger: false,
          showQuickJumper: false,
        };
      case 'mini':
        return {
          size: 'small' as const,
          showSizeChanger: false,
          showQuickJumper: false,
          showTotal: undefined,
        };
      default:
        return {};
    }
  };

  return (
    <AntPagination
      {...getVariantProps()}
      {...props}
      className={`ui-pagination ui-pagination-${variant} ${className || ''}`}
      style={style}
    />
  );
};

// Anchor Component
export interface AnchorProps extends AntAnchorProps {
  variant?: 'default' | 'fixed' | 'inline';
}

export const Anchor: React.FC<AnchorProps> = ({ 
  variant = 'default',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'fixed':
        return {
          position: 'fixed' as const,
          top: '50%',
          right: 'var(--space-4)',
          transform: 'translateY(-50%)',
          zIndex: 1000,
        };
      case 'inline':
        return {
          position: 'relative' as const,
          display: 'inline-block',
        };
      default:
        return {};
    }
  };

  return (
    <AntAnchor
      {...props}
      className={`ui-anchor ui-anchor-${variant} ${className || ''}`}
      style={{
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Navigation Bar Component
export interface NavBarProps {
  logo?: React.ReactNode;
  title?: string;
  menu?: React.ReactNode;
  actions?: React.ReactNode;
  variant?: 'default' | 'transparent' | 'elevated';
  className?: string;
  style?: React.CSSProperties;
}

export const NavBar: React.FC<NavBarProps> = ({
  logo,
  title,
  menu,
  actions,
  variant = 'default',
  className,
  style,
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'transparent':
        return {
          backgroundColor: 'transparent',
          boxShadow: 'none',
          borderBottom: '1px solid var(--border-primary)',
        };
      case 'elevated':
        return {
          backgroundColor: 'var(--bg-primary)',
          boxShadow: 'var(--shadow-lg)',
          borderBottom: 'none',
        };
      default:
        return {
          backgroundColor: 'var(--bg-primary)',
          boxShadow: 'var(--shadow-sm)',
          borderBottom: '1px solid var(--border-primary)',
        };
    }
  };

  return (
    <nav
      className={`ui-navbar ui-navbar-${variant} ${className || ''}`}
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 var(--space-6)',
        height: '64px',
        position: 'sticky',
        top: 0,
        zIndex: 1000,
        ...getVariantStyle(),
        ...style,
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)' }}>
        {logo && (
          <div className="ui-navbar-logo">
            {logo}
          </div>
        )}
        {title && (
          <h1 style={{ 
            margin: 0, 
            fontSize: 'var(--font-size-xl)', 
            fontWeight: 600,
            color: 'var(--text-primary)'
          }}>
            {title}
          </h1>
        )}
      </div>
      
      {menu && (
        <div className="ui-navbar-menu" style={{ flex: 1, marginLeft: 'var(--space-8)' }}>
          {menu}
        </div>
      )}
      
      {actions && (
        <div className="ui-navbar-actions">
          {actions}
        </div>
      )}
    </nav>
  );
};

// Export sub-components
export { MenuItem, SubMenu, MenuItemGroup, BreadcrumbItem, TabPane, Step, AnchorLink };

export default {
  Menu,
  Breadcrumb,
  Tabs,
  Steps,
  Pagination,
  Anchor,
  NavBar,
  MenuItem,
  SubMenu,
  MenuItemGroup,
  BreadcrumbItem,
  TabPane,
  Step,
  AnchorLink,
};
