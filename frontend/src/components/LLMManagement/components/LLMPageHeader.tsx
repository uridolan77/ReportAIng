/**
 * Standardized LLM Page Header Component
 * 
 * Provides consistent header styling and layout for all LLM management pages.
 * Includes title, description, and action buttons with enhanced visual design.
 */

import React from 'react';
import { Card, Button, Space, Flex } from 'antd';
import { ReloadOutlined, PlusOutlined } from '@ant-design/icons';
import { designTokens } from '../../core/design-system';

export interface LLMPageHeaderAction {
  key: string;
  label: string;
  icon?: React.ReactNode;
  onClick: () => void;
  type?: 'primary' | 'default';
  loading?: boolean;
}

export interface LLMPageHeaderProps {
  title: string;
  description: string;
  actions?: LLMPageHeaderAction[];
  onRefresh?: () => void;
  refreshLoading?: boolean;
  className?: string;
}

const LLMPageHeader: React.FC<LLMPageHeaderProps> = ({
  title,
  description,
  actions = [],
  onRefresh,
  refreshLoading = false,
  className = '',
}) => {
  const defaultActions: LLMPageHeaderAction[] = [
    ...(onRefresh ? [{
      key: 'refresh',
      label: 'Refresh',
      icon: <ReloadOutlined />,
      onClick: onRefresh,
      loading: refreshLoading,
    }] : []),
    ...actions,
  ];

  return (
    <Card 
      size="small" 
      className={`llm-page-header ${className}`}
      style={{ 
        marginBottom: designTokens.spacing.md,
        background: `linear-gradient(135deg, ${designTokens.colors.white} 0%, ${designTokens.colors.backgroundSecondary} 100%)`,
        border: `1px solid ${designTokens.colors.border}`,
        borderRadius: designTokens.borderRadius.large,
        boxShadow: designTokens.shadows.medium,
      }}
    >
      <Flex justify="between" align="center">
        <div style={{ flex: 1 }}>
          <h4 style={{ 
            margin: 0, 
            fontSize: designTokens.typography.fontSize.xl,
            fontWeight: designTokens.typography.fontWeight.semibold,
            color: designTokens.colors.text,
            marginBottom: designTokens.spacing.xs,
          }}>
            {title}
          </h4>
          <p style={{ 
            margin: 0, 
            color: designTokens.colors.textSecondary,
            fontSize: designTokens.typography.fontSize.sm,
            lineHeight: designTokens.typography.lineHeight.relaxed,
          }}>
            {description}
          </p>
        </div>
        
        {defaultActions.length > 0 && (
          <Space size="middle">
            {defaultActions.map((action) => (
              <Button
                key={action.key}
                type={action.type || 'default'}
                icon={action.icon}
                onClick={action.onClick}
                loading={action.loading}
                style={{
                  borderRadius: designTokens.borderRadius.medium,
                  fontWeight: designTokens.typography.fontWeight.medium,
                  boxShadow: action.type === 'primary' ? designTokens.shadows.small : 'none',
                  background: action.type === 'primary' 
                    ? `linear-gradient(135deg, ${designTokens.colors.primary} 0%, ${designTokens.colors.primaryHover} 100%)`
                    : undefined,
                  border: action.type === 'primary' ? 'none' : undefined,
                }}
              >
                {action.label}
              </Button>
            ))}
          </Space>
        )}
      </Flex>
    </Card>
  );
};

export default LLMPageHeader;

// Predefined action sets for common scenarios
export const createProviderHeaderActions = (
  onAdd: () => void,
  addLoading?: boolean
): LLMPageHeaderAction[] => [
  {
    key: 'add',
    label: 'Add Provider',
    icon: <PlusOutlined />,
    onClick: onAdd,
    type: 'primary',
    loading: addLoading,
  },
];

export const createModelHeaderActions = (
  onAdd: () => void,
  addLoading?: boolean
): LLMPageHeaderAction[] => [
  {
    key: 'add',
    label: 'Add Model',
    icon: <PlusOutlined />,
    onClick: onAdd,
    type: 'primary',
    loading: addLoading,
  },
];
