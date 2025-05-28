import { Button as AntButton, ButtonProps as AntButtonProps } from 'antd';
import React from 'react';

// Unified Button component
export interface ButtonProps extends Omit<AntButtonProps, 'variant'> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
}

export const Button: React.FC<ButtonProps> = ({ variant, style, ...props }) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'secondary':
        return {
          background: '#f0f0f0',
          borderColor: '#d9d9d9',
          color: '#262626',
        };
      case 'ghost':
        return {
          background: 'transparent',
          borderColor: 'transparent',
        };
      case 'danger':
        return {
          background: '#ff4d4f',
          borderColor: '#ff4d4f',
          color: 'white',
        };
      default:
        return {};
    }
  };

  return (
    <AntButton
      {...props}
      style={{
        ...getVariantStyle(),
        ...style,
      }}
    />
  );
};

// Loading Fallback Component
export const LoadingFallback: React.FC = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '200px'
  }}>
    <div>Loading...</div>
  </div>
);

// Common Card wrapper
interface CardProps {
  children: React.ReactNode;
  style?: React.CSSProperties;
}

export const Card: React.FC<CardProps> = ({ children, style }) => (
  <div style={{
    background: 'white',
    borderRadius: 8,
    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
    padding: 24,
    marginBottom: 16,
    ...style,
  }}>
    {children}
  </div>
);

// Common spacing utilities
interface SpacerProps {
  size?: 'small' | 'medium' | 'large';
}

export const Spacer: React.FC<SpacerProps> = ({ size = 'medium' }) => {
  const getHeight = () => {
    switch (size) {
      case 'small': return 8;
      case 'large': return 32;
      default: return 16;
    }
  };

  return <div style={{ height: getHeight() }} />;
};

// Flex utilities
interface FlexContainerProps {
  children: React.ReactNode;
  direction?: 'row' | 'column';
  justify?: 'flex-start' | 'center' | 'flex-end' | 'space-between' | 'space-around';
  align?: 'flex-start' | 'center' | 'flex-end' | 'stretch';
  gap?: string;
  style?: React.CSSProperties;
}

export const FlexContainer: React.FC<FlexContainerProps> = ({
  children,
  direction = 'row',
  justify = 'flex-start',
  align = 'flex-start',
  gap = '0',
  style
}) => (
  <div style={{
    display: 'flex',
    flexDirection: direction,
    justifyContent: justify,
    alignItems: align,
    gap,
    ...style,
  }}>
    {children}
  </div>
);

// Typography
interface TitleProps {
  children: React.ReactNode;
  level?: 1 | 2 | 3 | 4 | 5;
  style?: React.CSSProperties;
}

export const Title: React.FC<TitleProps> = ({ children, level = 2, style }) => {
  const getFontSize = () => {
    switch (level) {
      case 1: return '2.5rem';
      case 2: return '2rem';
      case 3: return '1.5rem';
      case 4: return '1.25rem';
      case 5: return '1rem';
      default: return '2rem';
    }
  };

  const Tag = `h${level}` as keyof JSX.IntrinsicElements;

  return (
    <Tag style={{
      fontSize: getFontSize(),
      fontWeight: 600,
      margin: '0 0 16px 0',
      color: '#262626',
      ...style,
    }}>
      {children}
    </Tag>
  );
};

interface TextProps {
  children: React.ReactNode;
  size?: 'small' | 'medium' | 'large';
  weight?: 'normal' | 'medium' | 'bold';
  color?: string;
  style?: React.CSSProperties;
}

export const Text: React.FC<TextProps> = ({
  children,
  size = 'medium',
  weight = 'normal',
  color = '#595959',
  style
}) => {
  const getFontSize = () => {
    switch (size) {
      case 'small': return '0.875rem';
      case 'large': return '1.125rem';
      default: return '1rem';
    }
  };

  const getFontWeight = () => {
    switch (weight) {
      case 'medium': return '500';
      case 'bold': return '600';
      default: return '400';
    }
  };

  return (
    <p style={{
      fontSize: getFontSize(),
      fontWeight: getFontWeight(),
      color,
      margin: '0 0 8px 0',
      lineHeight: 1.5,
      ...style,
    }}>
      {children}
    </p>
  );
};

// Status indicators
interface StatusBadgeProps {
  children: React.ReactNode;
  status: 'success' | 'warning' | 'error' | 'info' | 'default';
  style?: React.CSSProperties;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ children, status, style }) => {
  const getStatusStyle = () => {
    switch (status) {
      case 'success':
        return {
          background: '#f6ffed',
          color: '#52c41a',
          border: '1px solid #b7eb8f',
        };
      case 'warning':
        return {
          background: '#fffbe6',
          color: '#faad14',
          border: '1px solid #ffe58f',
        };
      case 'error':
        return {
          background: '#fff2f0',
          color: '#ff4d4f',
          border: '1px solid #ffccc7',
        };
      case 'info':
        return {
          background: '#f0f5ff',
          color: '#1890ff',
          border: '1px solid #adc6ff',
        };
      default:
        return {
          background: '#fafafa',
          color: '#8c8c8c',
          border: '1px solid #d9d9d9',
        };
    }
  };

  return (
    <span style={{
      display: 'inline-flex',
      alignItems: 'center',
      padding: '4px 8px',
      borderRadius: 4,
      fontSize: '0.75rem',
      fontWeight: 500,
      textTransform: 'uppercase',
      letterSpacing: '0.5px',
      ...getStatusStyle(),
      ...style,
    }}>
      {children}
    </span>
  );
};

// Grid system
interface GridProps {
  children: React.ReactNode;
  columns?: number;
  gap?: string;
  responsive?: boolean;
  style?: React.CSSProperties;
}

export const Grid: React.FC<GridProps> = ({
  children,
  columns = 1,
  gap = '16px',
  responsive = false,
  style
}) => (
  <div style={{
    display: 'grid',
    gridTemplateColumns: `repeat(${columns}, 1fr)`,
    gap,
    ...(responsive && {
      '@media (max-width: 768px)': {
        gridTemplateColumns: '1fr',
      }
    }),
    ...style,
  }}>
    {children}
  </div>
);

// Common animations
interface AnimatedContainerProps {
  children: React.ReactNode;
  animation?: 'fadeIn' | 'slideIn';
  style?: React.CSSProperties;
}

export const AnimatedContainer: React.FC<AnimatedContainerProps> = ({
  children,
  animation,
  style
}) => {
  const getAnimationStyle = () => {
    switch (animation) {
      case 'fadeIn':
        return {
          animation: 'fadeIn 0.3s ease-in-out',
        };
      case 'slideIn':
        return {
          animation: 'slideIn 0.3s ease-in-out',
        };
      default:
        return {};
    }
  };

  return (
    <div style={{
      ...getAnimationStyle(),
      ...style,
    }}>
      {children}
    </div>
  );
};
