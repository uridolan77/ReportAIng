/**
 * Data Components
 * Advanced data display components including tables, lists, and data visualization
 */

import React from 'react';
import { 
  Table as AntTable,
  TableProps as AntTableProps,
  List as AntList,
  ListProps as AntListProps,
  Tree as AntTree,
  TreeProps as AntTreeProps,
  Transfer as AntTransfer,
  TransferProps as AntTransferProps,
  Tag as AntTag,
  TagProps as AntTagProps,
  Badge as AntBadge,
  BadgeProps as AntBadgeProps,
  Avatar as AntAvatar,
  AvatarProps as AntAvatarProps,
  Statistic as AntStatistic,
  StatisticProps as AntStatisticProps
} from 'antd';

const { Item: ListItem, Item: { Meta: ListItemMeta } } = AntList;
const { TreeNode } = AntTree;
const { Group: AvatarGroup } = AntAvatar;

// Table Component
export interface TableProps extends AntTableProps<any> {
  variant?: 'default' | 'bordered' | 'striped' | 'compact';
  size?: 'small' | 'medium' | 'large';
}

export const Table: React.FC<TableProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getVariantProps = () => {
    switch (variant) {
      case 'bordered':
        return {
          bordered: true,
        };
      case 'striped':
        return {
          className: 'ui-table-striped',
        };
      case 'compact':
        return {
          size: 'small' as const,
        };
      default:
        return {};
    }
  };

  const getSize = () => {
    switch (size) {
      case 'small': return 'small';
      case 'large': return 'default';
      default: return 'middle';
    }
  };

  return (
    <AntTable
      {...getVariantProps()}
      {...props}
      size={getSize()}
      className={`ui-table ui-table-${variant} ui-table-${size} ${className || ''}`}
      style={{
        ...style,
      }}
    />
  );
};

// List Component
export interface ListProps extends AntListProps<any> {
  variant?: 'default' | 'card' | 'bordered' | 'split';
  size?: 'small' | 'medium' | 'large';
}

export const List: React.FC<ListProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getVariantProps = () => {
    switch (variant) {
      case 'card':
        return {
          bordered: true,
          style: { backgroundColor: 'var(--bg-secondary)' },
        };
      case 'bordered':
        return {
          bordered: true,
        };
      case 'split':
        return {
          split: true,
        };
      default:
        return {};
    }
  };

  const getSize = () => {
    switch (size) {
      case 'small': return 'small';
      case 'large': return 'large';
      default: return 'default';
    }
  };

  return (
    <AntList
      {...getVariantProps()}
      {...props}
      size={getSize()}
      className={`ui-list ui-list-${variant} ui-list-${size} ${className || ''}`}
      style={{
        ...style,
      }}
    />
  );
};

// Tree Component
export interface TreeProps extends AntTreeProps {
  variant?: 'default' | 'directory' | 'line';
  size?: 'small' | 'medium' | 'large';
}

export const Tree: React.FC<TreeProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getVariantProps = () => {
    switch (variant) {
      case 'directory':
        return {
          showIcon: true,
          showLine: true,
        };
      case 'line':
        return {
          showLine: true,
        };
      default:
        return {};
    }
  };

  const getSizeStyle = () => {
    switch (size) {
      case 'small': return { fontSize: 'var(--font-size-sm)' };
      case 'large': return { fontSize: 'var(--font-size-lg)' };
      default: return {};
    }
  };

  return (
    <AntTree
      {...getVariantProps()}
      {...props}
      className={`ui-tree ui-tree-${variant} ui-tree-${size} ${className || ''}`}
      style={{
        ...getSizeStyle(),
        ...style,
      }}
    />
  );
};

// Tag Component
export interface TagProps extends AntTagProps {
  variant?: 'default' | 'outlined' | 'filled' | 'rounded';
  size?: 'small' | 'medium' | 'large';
}

export const Tag: React.FC<TagProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  children,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'outlined':
        return {
          backgroundColor: 'transparent',
          border: '1px solid currentColor',
        };
      case 'filled':
        return {
          border: 'none',
          fontWeight: '500',
        };
      case 'rounded':
        return {
          borderRadius: '50px',
        };
      default:
        return {};
    }
  };

  const getSizeStyle = () => {
    switch (size) {
      case 'small':
        return {
          fontSize: 'var(--font-size-xs)',
          padding: '2px 6px',
          lineHeight: '16px',
        };
      case 'large':
        return {
          fontSize: 'var(--font-size-md)',
          padding: '6px 12px',
          lineHeight: '24px',
        };
      default:
        return {
          fontSize: 'var(--font-size-sm)',
          padding: '4px 8px',
          lineHeight: '20px',
        };
    }
  };

  return (
    <AntTag
      {...props}
      className={`ui-tag ui-tag-${variant} ui-tag-${size} ${className || ''}`}
      style={{
        ...getVariantStyle(),
        ...getSizeStyle(),
        ...style,
      }}
    >
      {children}
    </AntTag>
  );
};

// Badge Component
export interface BadgeProps extends AntBadgeProps {
  variant?: 'default' | 'dot' | 'status' | 'ribbon';
  size?: 'small' | 'medium' | 'large';
}

export const Badge: React.FC<BadgeProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getVariantProps = () => {
    switch (variant) {
      case 'dot':
        return {
          dot: true,
        };
      case 'status':
        return {
          status: 'processing' as const,
        };
      case 'ribbon':
        return {
          ribbon: true,
        };
      default:
        return {};
    }
  };

  const getSizeProps = () => {
    switch (size) {
      case 'small':
        return {
          size: 'small' as const,
        };
      case 'large':
        return {
          size: 'default' as const,
        };
      default:
        return {
          size: 'default' as const,
        };
    }
  };

  return (
    <AntBadge
      {...getVariantProps()}
      {...getSizeProps()}
      {...props}
      className={`ui-badge ui-badge-${variant} ui-badge-${size} ${className || ''}`}
      style={style}
    />
  );
};

// Avatar Component
export interface AvatarProps extends AntAvatarProps {
  variant?: 'default' | 'square' | 'circle';
  size?: 'small' | 'medium' | 'large' | 'extra-large' | number;
}

export const Avatar: React.FC<AvatarProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getShape = () => {
    switch (variant) {
      case 'square': return 'square';
      case 'circle': return 'circle';
      default: return 'circle';
    }
  };

  const getSize = () => {
    if (typeof size === 'number') return size;
    switch (size) {
      case 'small': return 24;
      case 'large': return 48;
      case 'extra-large': return 64;
      default: return 32;
    }
  };

  return (
    <AntAvatar
      {...props}
      shape={getShape()}
      size={getSize()}
      className={`ui-avatar ui-avatar-${variant} ui-avatar-${size} ${className || ''}`}
      style={style}
    />
  );
};

// Statistic Component
export interface StatisticProps extends AntStatisticProps {
  variant?: 'default' | 'card' | 'inline';
  size?: 'small' | 'medium' | 'large';
}

export const Statistic: React.FC<StatisticProps> = ({ 
  variant = 'default',
  size = 'medium',
  className,
  style,
  ...props 
}) => {
  const getVariantStyle = () => {
    switch (variant) {
      case 'card':
        return {
          padding: 'var(--space-4)',
          backgroundColor: 'var(--bg-secondary)',
          borderRadius: 'var(--radius-lg)',
          border: '1px solid var(--border-primary)',
        };
      case 'inline':
        return {
          display: 'inline-block',
        };
      default:
        return {};
    }
  };

  const getSizeStyle = () => {
    switch (size) {
      case 'small':
        return {
          fontSize: 'var(--font-size-sm)',
        };
      case 'large':
        return {
          fontSize: 'var(--font-size-xl)',
        };
      default:
        return {
          fontSize: 'var(--font-size-lg)',
        };
    }
  };

  if (variant === 'card') {
    return (
      <div
        className={`ui-statistic ui-statistic-${variant} ui-statistic-${size} ${className || ''}`}
        style={{
          ...getVariantStyle(),
          ...style,
        }}
      >
        <AntStatistic
          {...props}
          style={getSizeStyle()}
        />
      </div>
    );
  }

  return (
    <AntStatistic
      {...props}
      className={`ui-statistic ui-statistic-${variant} ui-statistic-${size} ${className || ''}`}
      style={{
        ...getVariantStyle(),
        ...getSizeStyle(),
        ...style,
      }}
    />
  );
};

// Export sub-components
export { ListItem, ListItemMeta, TreeNode, AvatarGroup };

export default {
  Table,
  List,
  Tree,
  Tag,
  Badge,
  Avatar,
  Statistic,
  ListItem,
  ListItemMeta,
  TreeNode,
  AvatarGroup,
};
