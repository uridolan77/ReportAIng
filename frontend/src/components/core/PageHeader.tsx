/**
 * Enhanced Page Header Component
 *
 * Consistent page header with breadcrumbs, title, subtitle, and actions.
 * Ensures uniform styling and layout across all pages with enhanced visual design.
 *
 * Features:
 * - Responsive design with mobile-first approach
 * - Gradient backgrounds and shadow effects
 * - Flexible action button layouts
 * - Enhanced breadcrumb functionality
 * - Accessibility improvements
 * - Dark mode support
 * - Multiple variants and customization options
 * - Animation and transition effects
 */

import React, { forwardRef } from 'react';
import { Button, Space, Divider, Tooltip } from 'antd';
import { designTokens } from './design-system';
import { Breadcrumb, BreadcrumbItem } from './Navigation';

export interface PageHeaderAction {
  key: string;
  label: React.ReactNode;
  icon?: React.ReactNode;
  onClick: () => void;
  type?: 'primary' | 'default' | 'dashed' | 'link' | 'text';
  size?: 'small' | 'middle' | 'large';
  loading?: boolean;
  disabled?: boolean;
  danger?: boolean;
  ghost?: boolean;
  tooltip?: string;
  href?: string;
  target?: '_blank' | '_self' | '_parent' | '_top';
}

export interface PageHeaderProps {
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  breadcrumbItems?: BreadcrumbItem[];
  actions?: React.ReactNode | PageHeaderAction[];
  className?: string;
  style?: React.CSSProperties;

  // Enhanced styling options
  variant?: 'default' | 'gradient' | 'minimal' | 'elevated' | 'glassmorphism';
  size?: 'small' | 'medium' | 'large';
  showDivider?: boolean;
  responsive?: boolean;

  // Background and visual effects
  background?: string;
  backgroundGradient?: boolean | string;
  shadow?: boolean | 'small' | 'medium' | 'large';
  blur?: boolean;

  // Layout options
  align?: 'left' | 'center' | 'right';
  actionsAlign?: 'start' | 'center' | 'end';
  direction?: 'horizontal' | 'vertical';

  // Accessibility
  titleLevel?: 1 | 2 | 3 | 4 | 5 | 6;
  ariaLabel?: string;
  
  // Animation
  animated?: boolean;
  animationDelay?: number;

  // Mobile responsiveness
  mobileCollapse?: boolean;
  mobileStackActions?: boolean;
}

export const PageHeader = forwardRef<HTMLDivElement, PageHeaderProps>(
  ({ 
    title, 
    subtitle, 
    breadcrumbItems, 
    actions,
    className, 
    style,
    variant = 'default',
    size = 'medium',
    showDivider = false,
    responsive = true,
    background,
    backgroundGradient = false,
    shadow = false,
    blur = false,
    align = 'left',
    actionsAlign = 'end',
    direction = 'horizontal',
    titleLevel = 1,
    ariaLabel,
    animated = false,
    animationDelay = 0,
    mobileCollapse = true,
    mobileStackActions = true
  }, ref) => {

    // Generate variant-based styles
    const getVariantStyles = () => {
      const baseStyle = {
        padding: `${designTokens.spacing.lg} 0 ${designTokens.spacing.xl} 0`,
        marginBottom: designTokens.spacing.lg,
        transition: animated ? 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)' : 'none',
        animationDelay: animated ? `${animationDelay}ms` : undefined,
      };

      switch (variant) {
        case 'gradient':
          return {
            ...baseStyle,
            background: typeof backgroundGradient === 'string' 
              ? backgroundGradient 
              : `linear-gradient(135deg, ${designTokens.colors.backgroundSecondary} 0%, ${designTokens.colors.background} 100%)`,
            borderRadius: designTokens.borderRadius.large,
            padding: designTokens.spacing.xl,
            boxShadow: shadow ? designTokens.shadows.medium : 'none',
          };

        case 'elevated':
          return {
            ...baseStyle,
            background: designTokens.colors.background,
            borderRadius: designTokens.borderRadius.medium,
            padding: designTokens.spacing.xl,
            boxShadow: typeof shadow === 'string' 
              ? designTokens.shadows[shadow as keyof typeof designTokens.shadows] 
              : shadow ? designTokens.shadows.large : designTokens.shadows.medium,
            border: `1px solid ${designTokens.colors.border}`,
          };

        case 'glassmorphism':
          return {
            ...baseStyle,
            background: blur 
              ? 'rgba(255, 255, 255, 0.25)' 
              : 'rgba(255, 255, 255, 0.1)',
            backdropFilter: blur ? 'blur(10px)' : 'none',
            borderRadius: designTokens.borderRadius.large,
            padding: designTokens.spacing.xl,
            border: '1px solid rgba(255, 255, 255, 0.18)',
            boxShadow: '0 8px 32px 0 rgba(31, 38, 135, 0.37)',
          };

        case 'minimal':
          return {
            ...baseStyle,
            padding: `${designTokens.spacing.md} 0`,
            borderBottom: showDivider ? `1px solid ${designTokens.colors.border}` : 'none',
          };

        default:
          return {
            ...baseStyle,
            background: background || 'transparent',
            boxShadow: shadow ? designTokens.shadows.small : 'none',
          };
      }
    };

    // Size-based styles
    const getSizeStyles = () => {
      switch (size) {
        case 'small':
          return {
            titleFontSize: designTokens.typography.fontSize['2xl'],
            subtitleFontSize: designTokens.typography.fontSize.base,
            spacing: designTokens.spacing.sm,
          };
        case 'large':
          return {
            titleFontSize: designTokens.typography.fontSize['4xl'],
            subtitleFontSize: designTokens.typography.fontSize.xl,
            spacing: designTokens.spacing.xl,
          };
        default: // medium
          return {
            titleFontSize: designTokens.typography.fontSize['3xl'],
            subtitleFontSize: designTokens.typography.fontSize.lg,
            spacing: designTokens.spacing.lg,
          };
      }
    };

    const sizeStyles = getSizeStyles();
    const headerStyle = {
      ...getVariantStyles(),
      textAlign: align,
      ...style,
    };

    const breadcrumbStyle = {
      marginBottom: designTokens.spacing.md,
      fontSize: designTokens.typography.fontSize.sm,
      color: designTokens.colors.textSecondary,
    };

    const titleStyle = {
      margin: 0,
      fontSize: sizeStyles.titleFontSize,
      fontWeight: designTokens.typography.fontWeight.bold,
      color: designTokens.colors.text,
      lineHeight: designTokens.typography.lineHeight.tight,
    };

    const subtitleStyle = {
      margin: title ? `${designTokens.spacing.sm} 0 0 0` : '0',
      fontSize: sizeStyles.subtitleFontSize,
      color: designTokens.colors.textSecondary,
      lineHeight: designTokens.typography.lineHeight.normal,
    };

    // Content container styles based on direction and responsive design
    const getContentContainerStyle = () => {
      const baseStyle = {
        display: 'flex',
        gap: sizeStyles.spacing,
      };

      if (direction === 'vertical') {
        return {
          ...baseStyle,
          flexDirection: 'column' as const,
          alignItems: align === 'center' ? 'center' : align === 'right' ? 'flex-end' : 'flex-start',
        };
      }

      return {
        ...baseStyle,
        justifyContent: 'space-between',
        alignItems: 'flex-start',
        flexWrap: responsive && mobileStackActions ? 'wrap' as const : 'nowrap' as const,
      };
    };

    const contentContainerStyle = getContentContainerStyle();

    const actionsStyle = {
      flexShrink: 0,
      display: 'flex',
      alignItems: 'center',
      justifyContent: actionsAlign === 'start' ? 'flex-start' : 
                     actionsAlign === 'center' ? 'center' : 'flex-end',
    };

    // Render actions properly
    const renderActions = () => {
      if (!actions) return null;

      // If actions is a React node, render directly
      if (React.isValidElement(actions) || typeof actions === 'string' || typeof actions === 'number') {
        return actions;
      }

      // If actions is an array of PageHeaderAction objects
      if (Array.isArray(actions)) {
        return (
          <Space size="middle" wrap={mobileStackActions}>
            {actions.map((action) => {
              const buttonProps = {
                key: action.key,
                type: action.type || 'default',
                size: action.size || (size === 'medium' ? 'middle' : size as 'small' | 'large'),
                icon: action.icon,
                onClick: action.onClick,
                style: {
                  borderRadius: designTokens.borderRadius.medium,
                  fontWeight: designTokens.typography.fontWeight.medium,
                },
                ...(action.loading !== undefined && { loading: action.loading }),
                ...(action.disabled !== undefined && { disabled: action.disabled }),
                ...(action.danger !== undefined && { danger: action.danger }),
                ...(action.ghost !== undefined && { ghost: action.ghost }),
                ...(action.href && { href: action.href }),
                ...(action.target && { target: action.target }),
              };

              const buttonContent = (
                <Button {...buttonProps}>
                  {action.label}
                </Button>
              );

              return action.tooltip ? (
                <Tooltip key={action.key} title={action.tooltip}>
                  {buttonContent}
                </Tooltip>
              ) : (
                buttonContent
              );
            })}
          </Space>
        );
      }

      return actions;
    };

    // Create the heading element based on titleLevel
    const HeadingTag = `h${titleLevel}` as keyof JSX.IntrinsicElements;

    // Ensure modern-page-header class is always included for consistency
    const combinedClassName = `modern-page-header page-header ${variant} ${className || ''}`.trim();

    return (
      <div
        ref={ref}
        className={combinedClassName}
        style={headerStyle}
        role="banner"
        aria-label={ariaLabel}
      >
        {/* Breadcrumbs */}
        {breadcrumbItems && breadcrumbItems.length > 0 && (
          <div style={breadcrumbStyle}>
            <Breadcrumb items={breadcrumbItems} />
          </div>
        )}
        
        {/* Title, Subtitle, and Actions */}
        {(title || subtitle || actions) && (
          <div style={contentContainerStyle}>
            <div style={{ flex: direction === 'horizontal' ? 1 : 'none' }}>
              {title && (
                <HeadingTag style={titleStyle}>
                  {title}
                </HeadingTag>
              )}
              {subtitle && (
                <p style={subtitleStyle}>
                  {subtitle}
                </p>
              )}
            </div>
            {actions && (
              <div style={actionsStyle}>
                {renderActions()}
              </div>
            )}
          </div>
        )}

        {/* Optional divider */}
        {showDivider && variant === 'default' && (
          <Divider style={{ margin: `${designTokens.spacing.md} 0 0 0` }} />
        )}

        {/* Responsive styles */}
        <style dangerouslySetInnerHTML={{
          __html: `
          @media (max-width: 768px) {
            .page-header {
              padding: ${designTokens.spacing.md} 0 !important;
            }
            
            ${mobileCollapse ? `
            .page-header h1, .page-header h2, .page-header h3 {
              font-size: ${designTokens.typography.fontSize['2xl']} !important;
            }
            
            .page-header p {
              font-size: ${designTokens.typography.fontSize.base} !important;
            }
            ` : ''}
            
            ${mobileStackActions ? `
            .page-header > div {
              flex-direction: column !important;
              align-items: flex-start !important;
              gap: ${designTokens.spacing.md} !important;
            }
            ` : ''}
          }

          ${animated ? `
          .page-header {
            animation: fadeInUp 0.6s ease-out;
          }

          @keyframes fadeInUp {
            from {
              opacity: 0;
              transform: translateY(20px);
            }
            to {
              opacity: 1;
              transform: translateY(0);
            }
          }
          ` : ''}
          `
        }} />
      </div>
    );
  }
);

PageHeader.displayName = 'PageHeader';

// Preset configurations for common page header patterns
export const PageHeaderPresets = {
  dashboard: {
    variant: 'gradient' as const,
    size: 'large' as const,
    backgroundGradient: true,
    shadow: 'medium' as const,
    animated: true,
  },
  
  admin: {
    variant: 'elevated' as const,
    size: 'medium' as const,
    shadow: 'large' as const,
    showDivider: true,
  },
  
  minimal: {
    variant: 'minimal' as const,
    size: 'medium' as const,
    showDivider: true,
    mobileCollapse: true,
  },
  
  glassmorphism: {
    variant: 'glassmorphism' as const,
    size: 'large' as const,
    blur: true,
    animated: true,
    shadow: false,
  },
  
  mobile: {
    variant: 'default' as const,
    size: 'small' as const,
    mobileCollapse: true,
    mobileStackActions: true,
    direction: 'vertical' as const,
  }
} as const;

// Utility functions for creating common action patterns
export const createHeaderActions = {
  refresh: (onClick: () => void, loading = false): PageHeaderAction => ({
    key: 'refresh',
    label: 'Refresh',
    icon: '🔄',
    onClick,
    loading,
    tooltip: 'Refresh data',
  }),

  save: (onClick: () => void, loading = false): PageHeaderAction => ({
    key: 'save',
    label: 'Save',
    icon: '💾',
    onClick,
    loading,
    type: 'primary',
    tooltip: 'Save changes',
  }),

  export: (onClick: () => void): PageHeaderAction => ({
    key: 'export',
    label: 'Export',
    icon: '📤',
    onClick,
    tooltip: 'Export data',
  }),

  settings: (onClick: () => void): PageHeaderAction => ({
    key: 'settings',
    label: 'Settings',
    icon: '⚙️',
    onClick,
    tooltip: 'Open settings',
  }),

  add: (onClick: () => void, label = 'Add New'): PageHeaderAction => ({
    key: 'add',
    label,
    icon: '➕',
    onClick,
    type: 'primary',
    tooltip: `${label}`,
  }),

  help: (onClick: () => void): PageHeaderAction => ({
    key: 'help',
    label: 'Help',
    icon: '❓',
    onClick,
    type: 'link',
    tooltip: 'Get help',
  }),
};

// Enhanced breadcrumb creation utilities
export const createEnhancedBreadcrumbs = {
  home: (): BreadcrumbItem => ({
    title: 'Home',
    path: '/',
    icon: '🏠'
  }),

  admin: (): BreadcrumbItem => ({
    title: 'Admin',
    path: '/admin',
    icon: '⚙️'
  }),

  dashboard: (): BreadcrumbItem => ({
    title: 'Dashboard',
    path: '/dashboard',
    icon: '📊'
  }),

  reports: (): BreadcrumbItem => ({
    title: 'Reports',
    path: '/reports',
    icon: '📋'
  }),

  analytics: (): BreadcrumbItem => ({
    title: 'Analytics',
    path: '/analytics',
    icon: '📈'
  }),

  settings: (): BreadcrumbItem => ({
    title: 'Settings',
    path: '/settings',
    icon: '⚙️'
  }),

  // Dynamic breadcrumb creation
  create: (title: string, path?: string, icon?: React.ReactNode): BreadcrumbItem => ({
    title,
    ...(path && { path }),
    icon: icon || '📄'
  }),

  // Create a complete breadcrumb trail
  trail: (...items: BreadcrumbItem[]): BreadcrumbItem[] => [
    createEnhancedBreadcrumbs.home(),
    ...items
  ],

  // Admin breadcrumb trail
  adminTrail: (...items: BreadcrumbItem[]): BreadcrumbItem[] => [
    createEnhancedBreadcrumbs.home(),
    createEnhancedBreadcrumbs.admin(),
    ...items
  ]
};

// Hook for responsive page header behavior
export const usePageHeaderResponsive = (breakpoint = 768) => {
  const [isMobile, setIsMobile] = React.useState(false);

  React.useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth <= breakpoint);
    };

    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, [breakpoint]);

  return {
    isMobile,
    mobileProps: isMobile ? {
      size: 'small' as const,
      mobileCollapse: true,
      mobileStackActions: true,
      direction: 'vertical' as const,
    } : {},
  };
};

// Theme-aware page header hook
export const usePageHeaderTheme = (darkMode = false) => {
  return darkMode ? {
    variant: 'glassmorphism' as const,
    background: 'linear-gradient(135deg, rgba(0, 0, 0, 0.25) 0%, rgba(0, 0, 0, 0.1) 100%)',
    blur: true,
  } : {
    variant: 'default' as const,
    background: 'transparent',
  };
};

// Utility function to ensure modern page header styling is applied consistently
export const ensureModernPageHeader = (className?: string): string => {
  const classes = ['modern-page-header'];
  if (className) {
    // Split existing className and filter out duplicates
    const existingClasses = className.split(' ').filter(cls => cls.trim() && cls !== 'modern-page-header');
    classes.push(...existingClasses);
  }
  return classes.join(' ');
};

// Hook to get standardized page header props for consistency
export const useStandardPageHeader = (options?: {
  variant?: PageHeaderProps['variant'];
  size?: PageHeaderProps['size'];
  animated?: boolean;
}) => {
  const { variant = 'default', size = 'medium', animated = false } = options || {};

  return {
    variant,
    size,
    animated,
    className: ensureModernPageHeader(),
    responsive: true,
    mobileCollapse: true,
    mobileStackActions: true,
  };
};

export const useBreadcrumbs = () => {
  const createHomeBreadcrumb = (): BreadcrumbItem => ({
    title: 'Home',
    path: '/',
    icon: '🏠'
  });

  const createAdminBreadcrumb = (): BreadcrumbItem => ({
    title: 'Admin',
    path: '/admin',
    icon: '⚙️'
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
