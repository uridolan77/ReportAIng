/**
 * Standardized Page Layout Component
 * Provides consistent page structure across the application
 */

import React, { useEffect } from 'react';
import { Typography, Breadcrumb, Space, Divider } from 'antd';
import { HomeOutlined } from '@ant-design/icons';
import { useScrollAnimation, useStaggeredAnimation } from '../../hooks/useAnimations';

const { Title, Text } = Typography;

export interface PageLayoutProps {
  children: React.ReactNode;
  title?: string;
  subtitle?: string;
  breadcrumbs?: Array<{
    title: string;
    href?: string;
    icon?: React.ReactNode;
  }>;
  actions?: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | 'full';
  padding?: 'none' | 'sm' | 'md' | 'lg';
}

const getMaxWidth = (size: PageLayoutProps['maxWidth']) => {
  switch (size) {
    case 'sm': return '640px';
    case 'md': return '768px';
    case 'lg': return '1024px';
    case 'xl': return '1280px';
    case 'full': return '100%';
    default: return '1200px';
  }
};

const getPadding = (size: PageLayoutProps['padding']) => {
  switch (size) {
    case 'none': return '0';
    case 'sm': return 'var(--space-4)';
    case 'md': return 'var(--space-6)';
    case 'lg': return 'var(--space-8)';
    default: return 'var(--space-6)';
  }
};

export const PageLayout: React.FC<PageLayoutProps> = ({
  children,
  title,
  subtitle,
  breadcrumbs,
  actions,
  className,
  style,
  maxWidth = 'lg',
  padding = 'md'
}) => {
  const { elementRef: pageRef, isVisible } = useScrollAnimation(0.1);
  const { startStagger, isItemVisible } = useStaggeredAnimation(3, 150);

  useEffect(() => {
    if (isVisible) {
      startStagger();
    }
  }, [isVisible, startStagger]);

  return (
    <div
      ref={pageRef}
      className={`page-layout ${className || ''} ${isVisible ? 'staggered-content' : ''}`}
      style={{
        minHeight: '100%',
        background: 'var(--bg-secondary)',
        ...style
      }}
    >
      <div
        className="page-container"
        style={{
          maxWidth: getMaxWidth(maxWidth),
          margin: '0 auto',
          padding: getPadding(padding),
          width: '100%'
        }}
      >
        {/* Breadcrumbs */}
        {breadcrumbs && breadcrumbs.length > 0 && (
          <div
            className={`page-breadcrumbs ${isItemVisible(0) ? 'breadcrumb-enter breadcrumb-enter-active' : ''}`}
            style={{ marginBottom: 'var(--space-4)' }}
          >
            <Breadcrumb>
              <Breadcrumb.Item href="/">
                <HomeOutlined />
              </Breadcrumb.Item>
              {breadcrumbs.map((crumb, index) => (
                <Breadcrumb.Item key={index} href={crumb.href}>
                  {crumb.icon && <span style={{ marginRight: 'var(--space-1)' }}>{crumb.icon}</span>}
                  {crumb.title}
                </Breadcrumb.Item>
              ))}
            </Breadcrumb>
          </div>
        )}

        {/* Page Header */}
        {(title || subtitle || actions) && (
          <div
            className={`page-header ${isItemVisible(1) ? 'section-enter section-enter-active' : ''}`}
            style={{ marginBottom: 'var(--space-6)' }}
          >
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                gap: 'var(--space-4)',
                marginBottom: subtitle ? 'var(--space-2)' : 0
              }}
            >
              {title && (
                <Title
                  level={1}
                  style={{
                    margin: 0,
                    fontSize: 'var(--text-3xl)',
                    fontWeight: 'var(--font-weight-bold)',
                    color: 'var(--text-primary)'
                  }}
                >
                  {title}
                </Title>
              )}
              {actions && (
                <div className="page-actions">
                  <Space size="middle">{actions}</Space>
                </div>
              )}
            </div>
            {subtitle && (
              <Text
                style={{
                  fontSize: 'var(--text-lg)',
                  color: 'var(--text-secondary)',
                  display: 'block',
                  marginTop: 'var(--space-2)'
                }}
              >
                {subtitle}
              </Text>
            )}
            <Divider style={{ margin: 'var(--space-4) 0 0 0' }} />
          </div>
        )}

        {/* Page Content */}
        <div
          className={`page-content ${isItemVisible(2) ? 'section-enter section-enter-active' : ''}`}
        >
          {children}
        </div>
      </div>
    </div>
  );
};

// Section Component for consistent content sections
export interface PageSectionProps {
  children: React.ReactNode;
  title?: string;
  subtitle?: string;
  actions?: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  background?: 'default' | 'card' | 'transparent';
  padding?: 'none' | 'sm' | 'md' | 'lg';
}

export const PageSection: React.FC<PageSectionProps> = ({
  children,
  title,
  subtitle,
  actions,
  className,
  style,
  background = 'card',
  padding = 'md'
}) => {
  const getBackground = () => {
    switch (background) {
      case 'card': return 'var(--bg-primary)';
      case 'transparent': return 'transparent';
      default: return 'var(--bg-secondary)';
    }
  };

  const getSectionPadding = () => {
    switch (padding) {
      case 'none': return '0';
      case 'sm': return 'var(--space-4)';
      case 'md': return 'var(--space-6)';
      case 'lg': return 'var(--space-8)';
      default: return 'var(--space-6)';
    }
  };

  return (
    <div
      className={`page-section ${className || ''}`}
      style={{
        background: getBackground(),
        borderRadius: background === 'card' ? 'var(--radius-lg)' : 0,
        border: background === 'card' ? '1px solid var(--border-primary)' : 'none',
        boxShadow: background === 'card' ? 'var(--shadow-sm)' : 'none',
        padding: getSectionPadding(),
        marginBottom: 'var(--space-6)',
        ...style
      }}
    >
      {/* Section Header */}
      {(title || subtitle || actions) && (
        <div className="section-header" style={{ marginBottom: 'var(--space-4)' }}>
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-start',
              gap: 'var(--space-4)'
            }}
          >
            <div>
              {title && (
                <Title
                  level={3}
                  style={{
                    margin: 0,
                    fontSize: 'var(--text-xl)',
                    fontWeight: 'var(--font-weight-semibold)',
                    color: 'var(--text-primary)',
                    marginBottom: subtitle ? 'var(--space-1)' : 0
                  }}
                >
                  {title}
                </Title>
              )}
              {subtitle && (
                <Text
                  style={{
                    fontSize: 'var(--text-base)',
                    color: 'var(--text-secondary)'
                  }}
                >
                  {subtitle}
                </Text>
              )}
            </div>
            {actions && (
              <div className="section-actions">
                <Space size="small">{actions}</Space>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Section Content */}
      <div className="section-content">
        {children}
      </div>
    </div>
  );
};

// Grid Layout Component for consistent layouts
export interface PageGridProps {
  children: React.ReactNode;
  columns?: 1 | 2 | 3 | 4;
  gap?: 'sm' | 'md' | 'lg';
  className?: string;
  style?: React.CSSProperties;
}

export const PageGrid: React.FC<PageGridProps> = ({
  children,
  columns = 2,
  gap = 'md',
  className,
  style
}) => {
  const getGap = () => {
    switch (gap) {
      case 'sm': return 'var(--space-4)';
      case 'md': return 'var(--space-6)';
      case 'lg': return 'var(--space-8)';
      default: return 'var(--space-6)';
    }
  };

  return (
    <div
      className={`page-grid ${className || ''}`}
      style={{
        display: 'grid',
        gridTemplateColumns: `repeat(${columns}, 1fr)`,
        gap: getGap(),
        '@media (max-width: 768px)': {
          gridTemplateColumns: '1fr'
        },
        ...style
      }}
    >
      {children}
    </div>
  );
};
