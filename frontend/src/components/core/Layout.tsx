/**
 * Modern Layout Component System
 * 
 * Provides a comprehensive layout system with modern CSS Grid and Flexbox patterns.
 * Replaces all scattered layout implementations with a single, consistent system.
 */

import React, { forwardRef } from 'react';
import { designTokens } from './design-system';

// Types
export interface ContainerProps {
  children: React.ReactNode;
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | '2xl' | 'full';
  padding?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export interface GridProps {
  children: React.ReactNode;
  columns?: number | string;
  rows?: number | string;
  gap?: keyof typeof designTokens.spacing;
  columnGap?: keyof typeof designTokens.spacing;
  rowGap?: keyof typeof designTokens.spacing;
  responsive?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export interface FlexProps {
  children: React.ReactNode;
  direction?: 'row' | 'column' | 'row-reverse' | 'column-reverse';
  justify?: 'start' | 'end' | 'center' | 'between' | 'around' | 'evenly';
  align?: 'start' | 'end' | 'center' | 'baseline' | 'stretch';
  wrap?: 'nowrap' | 'wrap' | 'wrap-reverse';
  gap?: keyof typeof designTokens.spacing;
  className?: string;
  style?: React.CSSProperties;
}

export interface StackProps {
  children: React.ReactNode;
  spacing?: keyof typeof designTokens.spacing;
  direction?: 'vertical' | 'horizontal';
  align?: 'start' | 'center' | 'end' | 'stretch';
  className?: string;
  style?: React.CSSProperties;
}

export interface SpacerProps {
  size?: keyof typeof designTokens.spacing;
  direction?: 'horizontal' | 'vertical';
}

export interface DividerProps {
  orientation?: 'horizontal' | 'vertical';
  variant?: 'solid' | 'dashed' | 'dotted';
  spacing?: keyof typeof designTokens.spacing;
  className?: string;
  style?: React.CSSProperties;
}

// Container Component
export const Container = forwardRef<HTMLDivElement, ContainerProps>(
  ({ children, maxWidth = 'xl', padding = true, className, style }, ref) => {
    const getMaxWidth = () => {
      const widths = {
        sm: '640px',
        md: '768px',
        lg: '1024px',
        xl: '1280px',
        '2xl': '1536px',
        full: '100%',
      };
      return widths[maxWidth];
    };

    const containerStyle = {
      width: '100%',
      maxWidth: getMaxWidth(),
      marginLeft: 'auto',
      marginRight: 'auto',
      ...(padding && {
        paddingLeft: designTokens.spacing.lg,
        paddingRight: designTokens.spacing.lg,
      }),
      ...style,
    };

    return (
      <div ref={ref} className={className} style={containerStyle}>
        {children}
      </div>
    );
  }
);

Container.displayName = 'Container';

// Grid Component
export const Grid = forwardRef<HTMLDivElement, GridProps>(
  ({ 
    children, 
    columns = 1, 
    rows = 'auto',
    gap = 'md',
    columnGap,
    rowGap,
    responsive = false,
    className, 
    style 
  }, ref) => {
    const gridStyle = {
      display: 'grid',
      gridTemplateColumns: typeof columns === 'number' ? `repeat(${columns}, 1fr)` : columns,
      gridTemplateRows: typeof rows === 'number' ? `repeat(${rows}, 1fr)` : rows,
      gap: designTokens.spacing[gap],
      ...(columnGap && { columnGap: designTokens.spacing[columnGap] }),
      ...(rowGap && { rowGap: designTokens.spacing[rowGap] }),
      ...(responsive && {
        '@media (max-width: 768px)': {
          gridTemplateColumns: '1fr',
        },
      }),
      ...style,
    };

    return (
      <div ref={ref} className={className} style={gridStyle}>
        {children}
      </div>
    );
  }
);

Grid.displayName = 'Grid';

// Flex Component
export const Flex = forwardRef<HTMLDivElement, FlexProps>(
  ({ 
    children, 
    direction = 'row', 
    justify = 'start', 
    align = 'start', 
    wrap = 'nowrap',
    gap = 'md',
    className, 
    style 
  }, ref) => {
    const getJustifyContent = () => {
      const justifyMap = {
        start: 'flex-start',
        end: 'flex-end',
        center: 'center',
        between: 'space-between',
        around: 'space-around',
        evenly: 'space-evenly',
      };
      return justifyMap[justify];
    };

    const getAlignItems = () => {
      const alignMap = {
        start: 'flex-start',
        end: 'flex-end',
        center: 'center',
        baseline: 'baseline',
        stretch: 'stretch',
      };
      return alignMap[align];
    };

    const flexStyle = {
      display: 'flex',
      flexDirection: direction,
      justifyContent: getJustifyContent(),
      alignItems: getAlignItems(),
      flexWrap: wrap,
      gap: designTokens.spacing[gap],
      ...style,
    };

    return (
      <div ref={ref} className={className} style={flexStyle}>
        {children}
      </div>
    );
  }
);

Flex.displayName = 'Flex';

// Stack Component
export const Stack = forwardRef<HTMLDivElement, StackProps>(
  ({ 
    children, 
    spacing = 'md', 
    direction = 'vertical', 
    align = 'stretch',
    className, 
    style 
  }, ref) => {
    const stackStyle = {
      display: 'flex',
      flexDirection: direction === 'vertical' ? 'column' : 'row',
      alignItems: align === 'stretch' ? 'stretch' : 
                  align === 'center' ? 'center' :
                  align === 'end' ? 'flex-end' : 'flex-start',
      gap: designTokens.spacing[spacing],
      ...style,
    };

    return (
      <div ref={ref} className={className} style={stackStyle}>
        {children}
      </div>
    );
  }
);

Stack.displayName = 'Stack';

// Spacer Component
export const Spacer: React.FC<SpacerProps> = ({ 
  size = 'md', 
  direction = 'vertical' 
}) => {
  const spacerStyle = {
    ...(direction === 'vertical' && {
      height: designTokens.spacing[size],
      width: '100%',
    }),
    ...(direction === 'horizontal' && {
      width: designTokens.spacing[size],
      height: '100%',
    }),
  };

  return <div style={spacerStyle} />;
};

Spacer.displayName = 'Spacer';

// Divider Component
export const Divider = forwardRef<HTMLDivElement, DividerProps>(
  ({ 
    orientation = 'horizontal', 
    variant = 'solid', 
    spacing = 'md',
    className, 
    style 
  }, ref) => {
    const dividerStyle = {
      ...(orientation === 'horizontal' && {
        width: '100%',
        height: '1px',
        marginTop: designTokens.spacing[spacing],
        marginBottom: designTokens.spacing[spacing],
      }),
      ...(orientation === 'vertical' && {
        height: '100%',
        width: '1px',
        marginLeft: designTokens.spacing[spacing],
        marginRight: designTokens.spacing[spacing],
      }),
      backgroundColor: designTokens.colors.border,
      border: 'none',
      ...(variant === 'dashed' && {
        borderTop: orientation === 'horizontal' ? `1px dashed ${designTokens.colors.border}` : 'none',
        borderLeft: orientation === 'vertical' ? `1px dashed ${designTokens.colors.border}` : 'none',
        backgroundColor: 'transparent',
      }),
      ...(variant === 'dotted' && {
        borderTop: orientation === 'horizontal' ? `1px dotted ${designTokens.colors.border}` : 'none',
        borderLeft: orientation === 'vertical' ? `1px dotted ${designTokens.colors.border}` : 'none',
        backgroundColor: 'transparent',
      }),
      ...style,
    };

    return <div ref={ref} className={className} style={dividerStyle} />;
  }
);

Divider.displayName = 'Divider';
