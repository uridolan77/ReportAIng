/**
 * Modern Card Component System
 * 
 * Provides a comprehensive card system with compound components and variants.
 * Replaces all scattered card implementations with a single, consistent system.
 */

import React, { forwardRef, createContext, useContext } from 'react';
import { Card as AntCard } from 'antd';
import type { CardProps as AntCardProps } from 'antd';
import { designTokens } from './design-system';

// Card Context for compound components
interface CardContextValue {
  variant: CardVariant;
  size: CardSize;
}

const CardContext = createContext<CardContextValue | null>(null);

const useCardContext = () => {
  const context = useContext(CardContext);
  if (!context) {
    throw new Error('Card compound components must be used within a Card component');
  }
  return context;
};

// Types
export type CardVariant = 'default' | 'outlined' | 'elevated' | 'filled' | 'interactive';
export type CardSize = 'small' | 'medium' | 'large';

export interface CardProps extends Omit<AntCardProps, 'size'> {
  variant?: CardVariant;
  size?: CardSize;
  interactive?: boolean;
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export interface CardHeaderProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export interface CardContentProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export interface CardFooterProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

// Main Card Component
export const Card = forwardRef<HTMLDivElement, CardProps>(
  ({ 
    variant = 'default', 
    size = 'medium', 
    interactive = false,
    children, 
    className, 
    style, 
    ...props 
  }, ref) => {
    const getVariantStyles = () => {
      const variants = {
        default: {
          backgroundColor: designTokens.colors.white,
          border: `1px solid ${designTokens.colors.border}`,
          boxShadow: designTokens.shadows.small,
        },
        outlined: {
          backgroundColor: designTokens.colors.white,
          border: `2px solid ${designTokens.colors.border}`,
          boxShadow: 'none',
        },
        elevated: {
          backgroundColor: designTokens.colors.white,
          border: 'none',
          boxShadow: designTokens.shadows.large,
        },
        filled: {
          backgroundColor: designTokens.colors.backgroundSecondary,
          border: 'none',
          boxShadow: 'none',
        },
        interactive: {
          backgroundColor: designTokens.colors.white,
          border: `1px solid ${designTokens.colors.border}`,
          boxShadow: designTokens.shadows.small,
          cursor: 'pointer',
          transition: 'all 0.2s ease',
          '&:hover': {
            boxShadow: designTokens.shadows.medium,
            borderColor: designTokens.colors.borderHover,
            transform: 'translateY(-2px)',
          },
        },
      };
      return variants[variant];
    };

    const getSizeStyles = () => {
      const sizes = {
        small: {
          borderRadius: designTokens.borderRadius.small,
        },
        medium: {
          borderRadius: designTokens.borderRadius.medium,
        },
        large: {
          borderRadius: designTokens.borderRadius.large,
        },
      };
      return sizes[size];
    };

    const cardStyle = {
      ...getVariantStyles(),
      ...getSizeStyles(),
      ...(interactive && {
        '&:hover': {
          boxShadow: designTokens.shadows.medium,
          borderColor: designTokens.colors.borderHover,
          transform: 'translateY(-2px)',
        },
      }),
      ...style,
    };

    const contextValue: CardContextValue = {
      variant,
      size,
    };

    return (
      <CardContext.Provider value={contextValue}>
        <AntCard
          ref={ref}
          className={className}
          style={cardStyle}
          bordered={false}
          {...props}
        >
          {children}
        </AntCard>
      </CardContext.Provider>
    );
  }
);

Card.displayName = 'Card';

// Card Header Component
export const CardHeader = forwardRef<HTMLDivElement, CardHeaderProps>(
  ({ children, className, style }, ref) => {
    const { size } = useCardContext();
    
    const getSizeStyles = () => {
      const sizes = {
        small: {
          padding: `${designTokens.spacing.sm} ${designTokens.spacing.md}`,
        },
        medium: {
          padding: `${designTokens.spacing.md} ${designTokens.spacing.lg}`,
        },
        large: {
          padding: `${designTokens.spacing.lg} ${designTokens.spacing.xl}`,
        },
      };
      return sizes[size];
    };

    const headerStyle = {
      ...getSizeStyles(),
      borderBottom: `1px solid ${designTokens.colors.border}`,
      marginBottom: 0,
      ...style,
    };

    return (
      <div ref={ref} className={className} style={headerStyle}>
        {children}
      </div>
    );
  }
);

CardHeader.displayName = 'CardHeader';

// Card Content Component
export const CardContent = forwardRef<HTMLDivElement, CardContentProps>(
  ({ children, className, style }, ref) => {
    const { size } = useCardContext();
    
    const getSizeStyles = () => {
      const sizes = {
        small: {
          padding: designTokens.spacing.md,
        },
        medium: {
          padding: designTokens.spacing.lg,
        },
        large: {
          padding: designTokens.spacing.xl,
        },
      };
      return sizes[size];
    };

    const contentStyle = {
      ...getSizeStyles(),
      ...style,
    };

    return (
      <div ref={ref} className={className} style={contentStyle}>
        {children}
      </div>
    );
  }
);

CardContent.displayName = 'CardContent';

// Card Footer Component
export const CardFooter = forwardRef<HTMLDivElement, CardFooterProps>(
  ({ children, className, style }, ref) => {
    const { size } = useCardContext();
    
    const getSizeStyles = () => {
      const sizes = {
        small: {
          padding: `${designTokens.spacing.sm} ${designTokens.spacing.md}`,
        },
        medium: {
          padding: `${designTokens.spacing.md} ${designTokens.spacing.lg}`,
        },
        large: {
          padding: `${designTokens.spacing.lg} ${designTokens.spacing.xl}`,
        },
      };
      return sizes[size];
    };

    const footerStyle = {
      ...getSizeStyles(),
      borderTop: `1px solid ${designTokens.colors.border}`,
      marginTop: 0,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'flex-end',
      gap: designTokens.spacing.sm,
      ...style,
    };

    return (
      <div ref={ref} className={className} style={footerStyle}>
        {children}
      </div>
    );
  }
);

CardFooter.displayName = 'CardFooter';

// Compound Component Pattern
Card.Header = CardHeader;
Card.Content = CardContent;
Card.Footer = CardFooter;
