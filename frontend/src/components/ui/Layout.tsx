/**
 * Layout Components
 * Flexible layout components for consistent page structure
 */

import React from 'react';

// Container Component
export interface ContainerProps {
  children: React.ReactNode;
  size?: 'small' | 'medium' | 'large' | 'full';
  className?: string;
  style?: React.CSSProperties;
}

export const Container: React.FC<ContainerProps> = ({
  children,
  size = 'large',
  className,
  style
}) => {
  const getMaxWidth = () => {
    switch (size) {
      case 'small': return '640px';
      case 'medium': return '768px';
      case 'large': return '1024px';
      case 'full': return '100%';
      default: return '1024px';
    }
  };

  return (
    <div
      className={`ui-container ${className || ''}`}
      style={{
        width: '100%',
        maxWidth: getMaxWidth(),
        margin: '0 auto',
        padding: '0 var(--space-4)',
        ...style,
      }}
    >
      {children}
    </div>
  );
};

// Flex Container Component
export interface FlexContainerProps {
  children: React.ReactNode;
  direction?: 'row' | 'column' | 'row-reverse' | 'column-reverse';
  justify?: 'start' | 'center' | 'end' | 'between' | 'around' | 'evenly';
  align?: 'start' | 'center' | 'end' | 'stretch' | 'baseline';
  wrap?: 'nowrap' | 'wrap' | 'wrap-reverse';
  gap?: string | number;
  className?: string;
  style?: React.CSSProperties;
}

export const FlexContainer: React.FC<FlexContainerProps> = ({
  children,
  direction = 'row',
  justify = 'start',
  align = 'start',
  wrap = 'nowrap',
  gap = 0,
  className,
  style
}) => {
  const getJustifyContent = () => {
    switch (justify) {
      case 'center': return 'center';
      case 'end': return 'flex-end';
      case 'between': return 'space-between';
      case 'around': return 'space-around';
      case 'evenly': return 'space-evenly';
      default: return 'flex-start';
    }
  };

  const getAlignItems = () => {
    switch (align) {
      case 'center': return 'center';
      case 'end': return 'flex-end';
      case 'stretch': return 'stretch';
      case 'baseline': return 'baseline';
      default: return 'flex-start';
    }
  };

  const getGap = () => {
    if (typeof gap === 'number') return `${gap}px`;
    return gap;
  };

  return (
    <div
      className={`ui-flex-container ${className || ''}`}
      style={{
        display: 'flex',
        flexDirection: direction,
        justifyContent: getJustifyContent(),
        alignItems: getAlignItems(),
        flexWrap: wrap,
        gap: getGap(),
        ...style,
      }}
    >
      {children}
    </div>
  );
};

// Grid Container Component
export interface GridContainerProps {
  children: React.ReactNode;
  columns?: number | string;
  rows?: number | string;
  gap?: string | number;
  columnGap?: string | number;
  rowGap?: string | number;
  responsive?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export const GridContainer: React.FC<GridContainerProps> = ({
  children,
  columns = 1,
  rows = 'auto',
  gap,
  columnGap,
  rowGap,
  responsive = false,
  className,
  style
}) => {
  const getColumns = () => {
    if (typeof columns === 'number') {
      return `repeat(${columns}, 1fr)`;
    }
    return columns;
  };

  const getRows = () => {
    if (typeof rows === 'number') {
      return `repeat(${rows}, 1fr)`;
    }
    return rows;
  };

  const getGap = (value?: string | number) => {
    if (typeof value === 'number') return `${value}px`;
    return value || 'var(--space-4)';
  };

  return (
    <div
      className={`ui-grid-container ${className || ''}`}
      style={{
        display: 'grid',
        gridTemplateColumns: getColumns(),
        gridTemplateRows: getRows(),
        gap: gap ? getGap(gap) : undefined,
        columnGap: columnGap ? getGap(columnGap) : undefined,
        rowGap: rowGap ? getGap(rowGap) : undefined,
        ...(responsive && {
          '@media (max-width: 768px)': {
            gridTemplateColumns: '1fr',
          }
        }),
        ...style,
      }}
    >
      {children}
    </div>
  );
};

// Stack Component (Vertical Layout)
export interface StackProps {
  children: React.ReactNode;
  spacing?: string | number;
  align?: 'start' | 'center' | 'end' | 'stretch';
  className?: string;
  style?: React.CSSProperties;
}

export const Stack: React.FC<StackProps> = ({
  children,
  spacing = 'var(--space-4)',
  align = 'stretch',
  className,
  style
}) => {
  const getSpacing = () => {
    if (typeof spacing === 'number') return `${spacing}px`;
    return spacing;
  };

  const getAlignItems = () => {
    switch (align) {
      case 'center': return 'center';
      case 'end': return 'flex-end';
      case 'stretch': return 'stretch';
      default: return 'flex-start';
    }
  };

  return (
    <div
      className={`ui-stack ${className || ''}`}
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: getAlignItems(),
        gap: getSpacing(),
        ...style,
      }}
    >
      {children}
    </div>
  );
};

// Spacer Component
export interface SpacerProps {
  size?: string | number;
  direction?: 'horizontal' | 'vertical';
}

export const Spacer: React.FC<SpacerProps> = ({ 
  size = 'var(--space-4)', 
  direction = 'vertical' 
}) => {
  const getSize = () => {
    if (typeof size === 'number') return `${size}px`;
    return size;
  };

  return (
    <div
      className="ui-spacer"
      style={{
        width: direction === 'horizontal' ? getSize() : 'auto',
        height: direction === 'vertical' ? getSize() : 'auto',
        flexShrink: 0,
      }}
    />
  );
};

// Section Component
export interface SectionProps {
  children: React.ReactNode;
  padding?: 'none' | 'small' | 'medium' | 'large';
  background?: 'primary' | 'secondary' | 'tertiary' | 'transparent';
  className?: string;
  style?: React.CSSProperties;
}

export const Section: React.FC<SectionProps> = ({
  children,
  padding = 'medium',
  background = 'transparent',
  className,
  style
}) => {
  const getPadding = () => {
    switch (padding) {
      case 'none': return 0;
      case 'small': return 'var(--space-4) 0';
      case 'large': return 'var(--space-8) 0';
      default: return 'var(--space-6) 0';
    }
  };

  const getBackground = () => {
    switch (background) {
      case 'primary': return 'var(--bg-primary)';
      case 'secondary': return 'var(--bg-secondary)';
      case 'tertiary': return 'var(--bg-tertiary)';
      default: return 'transparent';
    }
  };

  return (
    <section
      className={`ui-section ${className || ''}`}
      style={{
        padding: getPadding(),
        background: getBackground(),
        ...style,
      }}
    >
      {children}
    </section>
  );
};

// Divider Component
export interface DividerProps {
  orientation?: 'horizontal' | 'vertical';
  variant?: 'solid' | 'dashed' | 'dotted';
  spacing?: 'small' | 'medium' | 'large';
  className?: string;
  style?: React.CSSProperties;
}

export const Divider: React.FC<DividerProps> = ({
  orientation = 'horizontal',
  variant = 'solid',
  spacing = 'medium',
  className,
  style
}) => {
  const getSpacing = () => {
    switch (spacing) {
      case 'small': return 'var(--space-2)';
      case 'large': return 'var(--space-6)';
      default: return 'var(--space-4)';
    }
  };

  const isHorizontal = orientation === 'horizontal';

  return (
    <div
      className={`ui-divider ${className || ''}`}
      style={{
        width: isHorizontal ? '100%' : '1px',
        height: isHorizontal ? '1px' : '100%',
        background: 'var(--border-primary)',
        borderStyle: variant,
        margin: isHorizontal ? `${getSpacing()} 0` : `0 ${getSpacing()}`,
        ...style,
      }}
    />
  );
};

// Center Component
export interface CenterProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export const Center: React.FC<CenterProps> = ({ children, className, style }) => (
  <div
    className={`ui-center ${className || ''}`}
    style={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      ...style,
    }}
  >
    {children}
  </div>
);

export default {
  Container,
  FlexContainer,
  GridContainer,
  Stack,
  Spacer,
  Section,
  Divider,
  Center,
};
