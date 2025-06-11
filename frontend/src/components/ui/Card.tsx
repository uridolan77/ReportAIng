/**
 * Card Components
 * Flexible card components with consistent styling and layout
 */

import React from 'react';
import { Card as AntCard, CardProps as AntCardProps } from 'antd';

export interface CardProps extends Omit<AntCardProps, 'children'> {
  children: React.ReactNode;
  variant?: 'default' | 'outlined' | 'borderless';
  padding?: 'none' | 'small' | 'medium' | 'large';
  hover?: boolean;
}

export const Card: React.FC<CardProps> = ({ 
  children, 
  variant = 'default',
  padding = 'medium',
  hover = false,
  style, 
  className,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'outlined':
        return {
          background: 'var(--bg-primary)',
          border: '1px solid var(--border-primary)',
          boxShadow: 'none',
        };
      case 'elevated':
        return {
          background: 'var(--bg-primary)',
          border: 'none',
          boxShadow: 'var(--shadow-lg)',
        };
      case 'flat':
        return {
          background: 'var(--bg-primary)',
          border: 'none',
          boxShadow: 'none',
        };
      default:
        return {
          background: 'var(--bg-primary)',
          border: '1px solid var(--border-secondary)',
          boxShadow: 'var(--shadow-md)',
        };
    }
  };

  const getPaddingStyle = () => {
    switch (padding) {
      case 'none': return { padding: 0 };
      case 'small': return { padding: 'var(--space-3)' };
      case 'large': return { padding: 'var(--space-6)' };
      default: return { padding: 'var(--space-4)' };
    }
  };

  const getHoverStyle = () => {
    if (!hover) return {};
    return {
      transition: 'all var(--transition-normal)',
      cursor: 'pointer',
      ':hover': {
        transform: 'translateY(-2px)',
        boxShadow: 'var(--shadow-lg)',
      }
    };
  };

  return (
    <AntCard
      {...props}
      className={`ui-card ${className || ''}`}
      style={{
        borderRadius: 'var(--radius-lg)',
        overflow: 'hidden',
        ...getVariantStyle(),
        ...getPaddingStyle(),
        ...getHoverStyle(),
        ...style,
      }}
      bodyStyle={{ padding: 0 }}
    >
      {children}
    </AntCard>
  );
};

// Card Header Component
export interface CardHeaderProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  actions?: React.ReactNode;
}

export const CardHeader: React.FC<CardHeaderProps> = ({
  children,
  className,
  style,
  actions
}) => (
  <div 
    className={`ui-card-header ${className || ''}`}
    style={{
      padding: 'var(--space-4)',
      borderBottom: '1px solid var(--border-light)',
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      background: 'var(--bg-secondary)',
      ...style
    }}
  >
    <div>{children}</div>
    {actions && <div className="card-header-actions">{actions}</div>}
  </div>
);

// Card Content Component
export interface CardContentProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  padding?: 'none' | 'small' | 'medium' | 'large';
}

export const CardContent: React.FC<CardContentProps> = ({
  children,
  className,
  style,
  padding = 'medium'
}) => {
  const getPadding = () => {
    switch (padding) {
      case 'none': return 0;
      case 'small': return 'var(--space-3)';
      case 'large': return 'var(--space-6)';
      default: return 'var(--space-4)';
    }
  };

  return (
    <div 
      className={`ui-card-content ${className || ''}`}
      style={{
        padding: getPadding(),
        ...style
      }}
    >
      {children}
    </div>
  );
};

// Card Title Component
export interface CardTitleProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  level?: 1 | 2 | 3 | 4 | 5;
}

export const CardTitle: React.FC<CardTitleProps> = ({
  children,
  className,
  style,
  level = 3
}) => {
  const Tag = `h${level}` as keyof JSX.IntrinsicElements;
  
  return (
    <Tag 
      className={`ui-card-title ${className || ''}`}
      style={{
        margin: 0,
        fontSize: level === 1 ? 'var(--font-size-2xl)' : 
                 level === 2 ? 'var(--font-size-xl)' :
                 level === 3 ? 'var(--font-size-lg)' :
                 level === 4 ? 'var(--font-size-md)' : 'var(--font-size-sm)',
        fontWeight: 600,
        color: 'var(--text-primary)',
        lineHeight: 'var(--line-height-tight)',
        ...style
      }}
    >
      {children}
    </Tag>
  );
};

// Card Description Component
export interface CardDescriptionProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export const CardDescription: React.FC<CardDescriptionProps> = ({
  children,
  className,
  style
}) => (
  <p 
    className={`ui-card-description ${className || ''}`}
    style={{
      margin: 'var(--space-2) 0 0 0',
      color: 'var(--text-secondary)',
      fontSize: 'var(--font-size-sm)',
      lineHeight: 'var(--line-height-normal)',
      ...style
    }}
  >
    {children}
  </p>
);

// Card Footer Component
export interface CardFooterProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  justify?: 'start' | 'center' | 'end' | 'between';
}

export const CardFooter: React.FC<CardFooterProps> = ({
  children,
  className,
  style,
  justify = 'end'
}) => {
  const getJustifyContent = () => {
    switch (justify) {
      case 'start': return 'flex-start';
      case 'center': return 'center';
      case 'between': return 'space-between';
      default: return 'flex-end';
    }
  };

  return (
    <div 
      className={`ui-card-footer ${className || ''}`}
      style={{
        padding: 'var(--space-4)',
        borderTop: '1px solid var(--border-light)',
        display: 'flex',
        justifyContent: getJustifyContent(),
        alignItems: 'center',
        gap: 'var(--space-3)',
        background: 'var(--bg-secondary)',
        ...style
      }}
    >
      {children}
    </div>
  );
};

// Stats Card Component
export interface StatsCardProps {
  title: string;
  value: string | number;
  change?: {
    value: string | number;
    type: 'increase' | 'decrease' | 'neutral';
  };
  icon?: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export const StatsCard: React.FC<StatsCardProps> = ({
  title,
  value,
  change,
  icon,
  className,
  style
}) => {
  const getChangeColor = () => {
    if (!change) return 'var(--text-secondary)';
    switch (change.type) {
      case 'increase': return 'var(--color-success)';
      case 'decrease': return 'var(--color-error)';
      default: return 'var(--text-secondary)';
    }
  };

  return (
    <Card className={`ui-stats-card ${className || ''}`} style={style}>
      <CardContent>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <div style={{ flex: 1 }}>
            <p style={{ 
              margin: 0, 
              fontSize: 'var(--font-size-sm)', 
              color: 'var(--text-secondary)',
              fontWeight: 500
            }}>
              {title}
            </p>
            <p style={{ 
              margin: 'var(--space-2) 0 0 0', 
              fontSize: 'var(--font-size-2xl)', 
              fontWeight: 700,
              color: 'var(--text-primary)',
              lineHeight: 1
            }}>
              {value}
            </p>
            {change && (
              <p style={{ 
                margin: 'var(--space-1) 0 0 0', 
                fontSize: 'var(--font-size-sm)', 
                color: getChangeColor(),
                fontWeight: 500
              }}>
                {change.type === 'increase' ? '↗' : change.type === 'decrease' ? '↘' : '→'} {change.value}
              </p>
            )}
          </div>
          {icon && (
            <div style={{ 
              fontSize: 'var(--font-size-2xl)', 
              color: 'var(--color-primary)',
              opacity: 0.7
            }}>
              {icon}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
};

export default Card;
