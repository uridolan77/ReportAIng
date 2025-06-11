/**
 * Modern Layout Components
 * 
 * Provides comprehensive layout components that replace all scattered layout implementations.
 * Includes AppLayout, PageLayout, ContentLayout, and SidebarLayout with modern patterns.
 */

import React, { forwardRef, useState, createContext, useContext } from 'react';
import { Layout as AntLayout } from 'antd';
import { designTokens } from './design-system';
import { Menu } from './Navigation';
import type { MenuItem } from './Navigation';

const { Header, Sider, Content, Footer } = AntLayout;

// Layout Context
interface LayoutContextValue {
  sidebarCollapsed: boolean;
  setSidebarCollapsed: (collapsed: boolean) => void;
}

const LayoutContext = createContext<LayoutContextValue | null>(null);

export const useLayout = () => {
  const context = useContext(LayoutContext);
  if (!context) {
    throw new Error('useLayout must be used within a Layout component');
  }
  return context;
};

// Types
export interface AppLayoutProps {
  children: React.ReactNode;
  header?: React.ReactNode;
  sidebar?: React.ReactNode;
  footer?: React.ReactNode;
  sidebarCollapsible?: boolean;
  sidebarDefaultCollapsed?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export interface PageLayoutProps {
  children: React.ReactNode;
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  breadcrumb?: React.ReactNode;
  actions?: React.ReactNode;
  tabs?: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export interface ContentLayoutProps {
  children: React.ReactNode;
  sidebar?: React.ReactNode;
  sidebarWidth?: number;
  sidebarPosition?: 'left' | 'right';
  className?: string;
  style?: React.CSSProperties;
}

export interface SidebarLayoutProps {
  children: React.ReactNode;
  menuItems: MenuItem[];
  logo?: React.ReactNode;
  title?: string;
  collapsed?: boolean;
  onCollapse?: (collapsed: boolean) => void;
  width?: number;
  collapsedWidth?: number;
  className?: string;
  style?: React.CSSProperties;
}

// App Layout Component
export const AppLayout = forwardRef<HTMLDivElement, AppLayoutProps>(
  ({ 
    children, 
    header, 
    sidebar, 
    footer,
    sidebarCollapsible = true,
    sidebarDefaultCollapsed = false,
    className, 
    style 
  }, ref) => {
    const [sidebarCollapsed, setSidebarCollapsed] = useState(sidebarDefaultCollapsed);

    const layoutStyle = {
      minHeight: '100vh',
      backgroundColor: designTokens.colors.background,
      ...style,
    };

    const contextValue: LayoutContextValue = {
      sidebarCollapsed,
      setSidebarCollapsed,
    };

    return (
      <LayoutContext.Provider value={contextValue}>
        <AntLayout ref={ref} className={className} style={layoutStyle}>
          {sidebar && (
            <Sider
              collapsible={sidebarCollapsible}
              collapsed={sidebarCollapsed}
              onCollapse={setSidebarCollapsed}
              trigger={null}
              width={280}
              collapsedWidth={80}
              style={{
                backgroundColor: designTokens.colors.white,
                borderRight: `1px solid ${designTokens.colors.border}`,
                boxShadow: designTokens.shadows.small,
              }}
            >
              {sidebar}
            </Sider>
          )}
          <AntLayout>
            {header && (
              <Header
                style={{
                  backgroundColor: designTokens.colors.white,
                  borderBottom: `1px solid ${designTokens.colors.border}`,
                  padding: '0 24px',
                  height: '64px',
                  lineHeight: '64px',
                  boxShadow: designTokens.shadows.small,
                }}
              >
                {header}
              </Header>
            )}
            <Content
              style={{
                padding: '24px',
                backgroundColor: designTokens.colors.backgroundSecondary,
                minHeight: 'calc(100vh - 64px)',
              }}
            >
              {children}
            </Content>
            {footer && (
              <Footer
                style={{
                  backgroundColor: designTokens.colors.white,
                  borderTop: `1px solid ${designTokens.colors.border}`,
                  textAlign: 'center',
                  padding: '16px 24px',
                }}
              >
                {footer}
              </Footer>
            )}
          </AntLayout>
        </AntLayout>
      </LayoutContext.Provider>
    );
  }
);

AppLayout.displayName = 'AppLayout';

// Page Layout Component
export const PageLayout = forwardRef<HTMLDivElement, PageLayoutProps>(
  ({ 
    children, 
    title, 
    subtitle, 
    breadcrumb, 
    actions, 
    tabs,
    className, 
    style 
  }, ref) => {
    const pageStyle = {
      backgroundColor: designTokens.colors.white,
      borderRadius: designTokens.borderRadius.large,
      boxShadow: designTokens.shadows.small,
      overflow: 'hidden',
      ...style,
    };

    return (
      <div ref={ref} className={className} style={pageStyle}>
        {(title || subtitle || breadcrumb || actions) && (
          <div
            style={{
              padding: `${designTokens.spacing.lg} ${designTokens.spacing.xl}`,
              borderBottom: `1px solid ${designTokens.colors.border}`,
              backgroundColor: designTokens.colors.white,
            }}
          >
            {breadcrumb && (
              <div style={{ marginBottom: designTokens.spacing.md }}>
                {breadcrumb}
              </div>
            )}
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                gap: designTokens.spacing.lg,
              }}
            >
              <div>
                {title && (
                  <h1
                    style={{
                      margin: 0,
                      fontSize: designTokens.typography.fontSize['3xl'],
                      fontWeight: designTokens.typography.fontWeight.bold,
                      color: designTokens.colors.text,
                      lineHeight: designTokens.typography.lineHeight.tight,
                    }}
                  >
                    {title}
                  </h1>
                )}
                {subtitle && (
                  <p
                    style={{
                      margin: title ? `${designTokens.spacing.sm} 0 0 0` : '0',
                      fontSize: designTokens.typography.fontSize.lg,
                      color: designTokens.colors.textSecondary,
                      lineHeight: designTokens.typography.lineHeight.normal,
                    }}
                  >
                    {subtitle}
                  </p>
                )}
              </div>
              {actions && (
                <div style={{ flexShrink: 0 }}>
                  {actions}
                </div>
              )}
            </div>
          </div>
        )}
        {tabs && (
          <div
            style={{
              borderBottom: `1px solid ${designTokens.colors.border}`,
              backgroundColor: designTokens.colors.white,
            }}
          >
            {tabs}
          </div>
        )}
        <div
          style={{
            padding: designTokens.spacing.xl,
          }}
        >
          {children}
        </div>
      </div>
    );
  }
);

PageLayout.displayName = 'PageLayout';

// Content Layout Component
export const ContentLayout = forwardRef<HTMLDivElement, ContentLayoutProps>(
  ({ 
    children, 
    sidebar, 
    sidebarWidth = 280, 
    sidebarPosition = 'left',
    className, 
    style 
  }, ref) => {
    const layoutStyle = {
      display: 'flex',
      gap: designTokens.spacing.lg,
      ...style,
    };

    return (
      <div ref={ref} className={className} style={layoutStyle}>
        {sidebar && sidebarPosition === 'left' && (
          <div
            style={{
              width: sidebarWidth,
              flexShrink: 0,
            }}
          >
            {sidebar}
          </div>
        )}
        <div style={{ flex: 1, minWidth: 0 }}>
          {children}
        </div>
        {sidebar && sidebarPosition === 'right' && (
          <div
            style={{
              width: sidebarWidth,
              flexShrink: 0,
            }}
          >
            {sidebar}
          </div>
        )}
      </div>
    );
  }
);

ContentLayout.displayName = 'ContentLayout';

// Sidebar Layout Component
export const SidebarLayout = forwardRef<HTMLDivElement, SidebarLayoutProps>(
  ({ 
    children, 
    menuItems, 
    logo, 
    title,
    collapsed = false,
    onCollapse,
    width = 280,
    collapsedWidth = 80,
    className, 
    style 
  }, ref) => {
    const sidebarStyle = {
      width: collapsed ? collapsedWidth : width,
      backgroundColor: designTokens.colors.white,
      borderRight: `1px solid ${designTokens.colors.border}`,
      boxShadow: designTokens.shadows.small,
      transition: 'width 0.2s ease',
      display: 'flex',
      flexDirection: 'column' as const,
      ...style,
    };

    return (
      <div ref={ref} className={className} style={sidebarStyle}>
        {(logo || title) && (
          <div
            style={{
              padding: designTokens.spacing.lg,
              borderBottom: `1px solid ${designTokens.colors.border}`,
              display: 'flex',
              alignItems: 'center',
              gap: designTokens.spacing.md,
            }}
          >
            {logo}
            {!collapsed && title && (
              <span
                style={{
                  fontSize: designTokens.typography.fontSize.lg,
                  fontWeight: designTokens.typography.fontWeight.semibold,
                  color: designTokens.colors.text,
                }}
              >
                {title}
              </span>
            )}
          </div>
        )}
        <div style={{ flex: 1, overflow: 'auto' }}>
          <Menu
            variant="sidebar"
            items={menuItems}
            style={{
              border: 'none',
              backgroundColor: 'transparent',
            }}
          />
        </div>
        {children}
      </div>
    );
  }
);

SidebarLayout.displayName = 'SidebarLayout';
