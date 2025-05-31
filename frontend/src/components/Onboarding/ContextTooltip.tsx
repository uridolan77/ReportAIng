/**
 * Context Tooltip Component
 * Provides contextual help and tips throughout the interface
 */

import React from 'react';
import {
  Tooltip,
  Typography,
  Space,
  Tag
} from 'antd';
import {
  InfoCircleOutlined,
  BulbOutlined,
  ThunderboltOutlined,
  RocketOutlined
} from '@ant-design/icons';

const { Text } = Typography;

interface ContextTooltipProps {
  title: string;
  content: React.ReactNode;
  type?: 'info' | 'tip' | 'shortcut' | 'feature';
  children: React.ReactNode;
  placement?: 'top' | 'bottom' | 'left' | 'right' | 'topLeft' | 'topRight' | 'bottomLeft' | 'bottomRight';
  trigger?: 'hover' | 'click' | 'focus';
}

export const ContextTooltip: React.FC<ContextTooltipProps> = ({
  title,
  content,
  type = 'info',
  children,
  placement = 'top',
  trigger = 'hover'
}) => {
  const getIcon = () => {
    switch (type) {
      case 'tip':
        return <BulbOutlined style={{ color: '#faad14' }} />;
      case 'shortcut':
        return <ThunderboltOutlined style={{ color: '#722ed1' }} />;
      case 'feature':
        return <RocketOutlined style={{ color: '#52c41a' }} />;
      default:
        return <InfoCircleOutlined style={{ color: '#1890ff' }} />;
    }
  };

  const getTypeLabel = () => {
    switch (type) {
      case 'tip':
        return <Tag color="orange" style={{ fontSize: '11px' }}>Tip</Tag>;
      case 'shortcut':
        return <Tag color="purple" style={{ fontSize: '11px' }}>Shortcut</Tag>;
      case 'feature':
        return <Tag color="green" style={{ fontSize: '11px' }}>Feature</Tag>;
      default:
        return <Tag color="blue" style={{ fontSize: '11px' }}>Info</Tag>;
    }
  };

  const tooltipContent = (
    <div style={{ maxWidth: '300px' }}>
      <Space direction="vertical" size="small" style={{ width: '100%' }}>
        <Space>
          {getIcon()}
          <Text strong style={{ color: 'white' }}>{title}</Text>
          {getTypeLabel()}
        </Space>
        <div style={{ color: 'rgba(255, 255, 255, 0.85)' }}>
          {content}
        </div>
      </Space>
    </div>
  );

  return (
    <Tooltip
      title={tooltipContent}
      placement={placement}
      trigger={trigger}
      styles={{
        root: {
          maxWidth: '350px'
        },
        inner: {
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          border: 'none',
          borderRadius: '8px',
          boxShadow: '0 8px 32px rgba(0, 0, 0, 0.15)'
        }
      }}
    >
      {children}
    </Tooltip>
  );
};

// Pre-configured tooltip components for common use cases
export const TipTooltip: React.FC<Omit<ContextTooltipProps, 'type'>> = (props) => (
  <ContextTooltip {...props} type="tip" />
);

export const ShortcutTooltip: React.FC<Omit<ContextTooltipProps, 'type'>> = (props) => (
  <ContextTooltip {...props} type="shortcut" />
);

export const FeatureTooltip: React.FC<Omit<ContextTooltipProps, 'type'>> = (props) => (
  <ContextTooltip {...props} type="feature" />
);

export const InfoTooltip: React.FC<Omit<ContextTooltipProps, 'type'>> = (props) => (
  <ContextTooltip {...props} type="info" />
);
