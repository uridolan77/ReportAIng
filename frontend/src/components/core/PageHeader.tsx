/**
 * Page Header Component
 * 
 * Consistent page header with breadcrumbs, title, subtitle, and actions.
 * Ensures uniform styling and layout across all pages.
 */

import React, { forwardRef } from 'react';
import { designTokens } from './design-system';
import { Breadcrumb, BreadcrumbItem } from './Navigation';

export interface PageHeaderProps {
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  breadcrumbItems?: BreadcrumbItem[];
  actions?: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export const PageHeader = forwardRef<HTMLDivElement, PageHeaderProps>(
  ({ 
    title, 
    subtitle, 
    breadcrumbItems, 
    actions,
    className, 
    style 
  }, ref) => {
    const headerStyle = {
      padding: `${designTokens.spacing.lg} 0 ${designTokens.spacing.xl} 0`,
      marginBottom: designTokens.spacing.lg,
      ...style,
    };

    const breadcrumbStyle = {
      marginBottom: designTokens.spacing.md,
      fontSize: designTokens.typography.fontSize.sm,
      color: designTokens.colors.textSecondary,
    };

    const titleStyle = {
      margin: 0,
      fontSize: designTokens.typography.fontSize['3xl'],
      fontWeight: designTokens.typography.fontWeight.bold,
      color: designTokens.colors.text,
      lineHeight: designTokens.typography.lineHeight.tight,
    };

    const subtitleStyle = {
      margin: title ? `${designTokens.spacing.sm} 0 0 0` : '0',
      fontSize: designTokens.typography.fontSize.lg,
      color: designTokens.colors.textSecondary,
      lineHeight: designTokens.typography.lineHeight.normal,
    };

    const contentContainerStyle = {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'flex-start',
      gap: designTokens.spacing.lg,
    };

    const actionsStyle = {
      flexShrink: 0,
    };

    return (
      <div ref={ref} className={className} style={headerStyle}>
        {/* Breadcrumbs */}
        {breadcrumbItems && breadcrumbItems.length > 0 && (
          <div style={breadcrumbStyle}>
            <Breadcrumb items={breadcrumbItems} />
          </div>
        )}
        
        {/* Title, Subtitle, and Actions */}
        {(title || subtitle || actions) && (
          <div style={contentContainerStyle}>
            <div>
              {title && (
                <h1 style={titleStyle}>
                  {title}
                </h1>
              )}
              {subtitle && (
                <p style={subtitleStyle}>
                  {subtitle}
                </p>
              )}
            </div>
            {actions && (
              <div style={actionsStyle}>
                {actions}
              </div>
            )}
          </div>
        )}
      </div>
    );
  }
);

PageHeader.displayName = 'PageHeader';

// Convenience hook for creating common breadcrumb patterns
export const useBreadcrumbs = () => {
  const createHomeBreadcrumb = (): BreadcrumbItem => ({
    title: 'Home',
    path: '/',
    icon: <span>🏠</span>
  });

  const createAdminBreadcrumb = (): BreadcrumbItem => ({
    title: 'Admin',
    path: '/admin',
    icon: <span>⚙️</span>
  });

  const createBreadcrumbs = (items: Omit<BreadcrumbItem, 'key'>[]): BreadcrumbItem[] => {
    return [
      createHomeBreadcrumb(),
      ...items.map((item, index) => ({
        ...item,
        key: `breadcrumb-${index}`
      }))
    ];
  };

  const createAdminBreadcrumbs = (items: Omit<BreadcrumbItem, 'key'>[]): BreadcrumbItem[] => {
    return [
      createHomeBreadcrumb(),
      createAdminBreadcrumb(),
      ...items.map((item, index) => ({
        ...item,
        key: `admin-breadcrumb-${index}`
      }))
    ];
  };

  return {
    createHomeBreadcrumb,
    createAdminBreadcrumb,
    createBreadcrumbs,
    createAdminBreadcrumbs
  };
};

export default PageHeader;
