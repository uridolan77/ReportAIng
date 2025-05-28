import { Button as AntButton, ButtonProps as AntButtonProps } from 'antd';
import styled from '@emotion/styled';
import React from 'react';

// Unified Button component
export interface ButtonProps extends Omit<AntButtonProps, 'variant'> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
}

export const Button = styled(AntButton)<ButtonProps>`
  ${({ variant }: { variant?: string }) => {
    switch (variant) {
      case 'secondary':
        return `
          background: #f0f0f0;
          border-color: #d9d9d9;
          color: #262626;
          &:hover {
            background: #e8e8e8;
            border-color: #bfbfbf;
          }
        `;
      case 'ghost':
        return `
          background: transparent;
          border-color: transparent;
        `;
      case 'danger':
        return `
          background: #ff4d4f;
          border-color: #ff4d4f;
          color: white;
          &:hover {
            background: #ff7875;
            border-color: #ff7875;
          }
        `;
      default:
        return '';
    }
  }}
`;

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
export const Card = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 24px;
  margin-bottom: 16px;
`;

// Common spacing utilities
export const Spacer = styled.div<{ size?: 'small' | 'medium' | 'large' }>`
  height: ${({ size }: { size?: string }) => {
    switch (size) {
      case 'small': return '8px';
      case 'large': return '32px';
      default: return '16px';
    }
  }};
`;

// Flex utilities
export const FlexContainer = styled.div<{
  direction?: 'row' | 'column';
  justify?: 'flex-start' | 'center' | 'flex-end' | 'space-between' | 'space-around';
  align?: 'flex-start' | 'center' | 'flex-end' | 'stretch';
  gap?: string;
}>`
  display: flex;
  flex-direction: ${({ direction }: { direction?: string }) => direction || 'row'};
  justify-content: ${({ justify }: { justify?: string }) => justify || 'flex-start'};
  align-items: ${({ align }: { align?: string }) => align || 'flex-start'};
  gap: ${({ gap }: { gap?: string }) => gap || '0'};
`;

// Typography
export const Title = styled.h1<{ level?: 1 | 2 | 3 | 4 | 5 }>`
  font-size: ${({ level }: { level?: number }) => {
    switch (level) {
      case 1: return '2.5rem';
      case 2: return '2rem';
      case 3: return '1.5rem';
      case 4: return '1.25rem';
      case 5: return '1rem';
      default: return '2rem';
    }
  }};
  font-weight: 600;
  margin: 0 0 16px 0;
  color: #262626;
`;

export const Text = styled.p<{
  size?: 'small' | 'medium' | 'large';
  weight?: 'normal' | 'medium' | 'bold';
  color?: string;
}>`
  font-size: ${({ size }: { size?: string }) => {
    switch (size) {
      case 'small': return '0.875rem';
      case 'large': return '1.125rem';
      default: return '1rem';
    }
  }};
  font-weight: ${({ weight }: { weight?: string }) => {
    switch (weight) {
      case 'medium': return '500';
      case 'bold': return '600';
      default: return '400';
    }
  }};
  color: ${({ color }: { color?: string }) => color || '#595959'};
  margin: 0 0 8px 0;
  line-height: 1.5;
`;

// Status indicators
export const StatusBadge = styled.span<{
  status: 'success' | 'warning' | 'error' | 'info' | 'default'
}>`
  display: inline-flex;
  align-items: center;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;

  ${({ status }: { status: string }) => {
    switch (status) {
      case 'success':
        return `
          background: #f6ffed;
          color: #52c41a;
          border: 1px solid #b7eb8f;
        `;
      case 'warning':
        return `
          background: #fffbe6;
          color: #faad14;
          border: 1px solid #ffe58f;
        `;
      case 'error':
        return `
          background: #fff2f0;
          color: #ff4d4f;
          border: 1px solid #ffccc7;
        `;
      case 'info':
        return `
          background: #f0f5ff;
          color: #1890ff;
          border: 1px solid #adc6ff;
        `;
      default:
        return `
          background: #fafafa;
          color: #8c8c8c;
          border: 1px solid #d9d9d9;
        `;
    }
  }}
`;

// Grid system
export const Grid = styled.div<{
  columns?: number;
  gap?: string;
  responsive?: boolean;
}>`
  display: grid;
  grid-template-columns: ${({ columns }: { columns?: number }) => `repeat(${columns || 1}, 1fr)`};
  gap: ${({ gap }: { gap?: string }) => gap || '16px'};

  ${({ responsive }: { responsive?: boolean }) => responsive && `
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
  `}
`;

// Common animations
export const fadeIn = `
  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }
`;

export const slideIn = `
  @keyframes slideIn {
    from { transform: translateY(-10px); opacity: 0; }
    to { transform: translateY(0); opacity: 1; }
  }
`;

export const AnimatedContainer = styled.div<{ animation?: 'fadeIn' | 'slideIn' }>`
  ${({ animation }: { animation?: string }) => {
    switch (animation) {
      case 'fadeIn':
        return `
          ${fadeIn}
          animation: fadeIn 0.3s ease-in-out;
        `;
      case 'slideIn':
        return `
          ${slideIn}
          animation: slideIn 0.3s ease-in-out;
        `;
      default:
        return '';
    }
  }}
`;
