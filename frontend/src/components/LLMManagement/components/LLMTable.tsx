/**
 * Standardized LLM Table Component
 * 
 * Provides consistent table styling and functionality for all LLM management pages.
 * Supports different action sets for providers vs models with enhanced UI/UX.
 */

import React from 'react';
import { Table, Button, Space, Tooltip, Popconfirm, Badge, Tag } from 'antd';
import { 
  EditOutlined, 
  DeleteOutlined, 
  CheckCircleOutlined,
  SettingOutlined,
  EyeOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined
} from '@ant-design/icons';
import { designTokens } from '../../core/design-system';

export interface LLMTableAction {
  key: string;
  label: string;
  icon: React.ReactNode;
  onClick: (record: any) => void;
  type?: 'primary' | 'default' | 'danger';
  loading?: boolean;
  tooltip?: string;
  confirm?: {
    title: string;
    description?: string;
  };
}

export interface LLMTableColumn {
  title: string;
  dataIndex?: string;
  key: string;
  render?: (value: any, record: any, index: number) => React.ReactNode;
  width?: number | string;
  sorter?: boolean;
  filters?: Array<{ text: string; value: any }>;
}

export interface LLMTableProps {
  columns: LLMTableColumn[];
  dataSource: any[];
  loading?: boolean;
  rowKey: string;
  actions?: LLMTableAction[];
  actionColumnWidth?: number;
  size?: 'small' | 'middle' | 'large';
  pagination?: boolean | object;
  onRow?: (record: any) => object;
  className?: string;
}

const LLMTable: React.FC<LLMTableProps> = ({
  columns,
  dataSource,
  loading = false,
  rowKey,
  actions = [],
  actionColumnWidth = 120,
  size = 'middle',
  pagination = {
    pageSize: 10,
    showSizeChanger: true,
    showQuickJumper: true,
    showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
  },
  onRow,
  className = '',
}) => {
  // Enhanced table columns with actions
  const enhancedColumns = [
    ...columns,
    ...(actions.length > 0 ? [{
      title: 'Actions',
      key: 'actions',
      width: actionColumnWidth,
      render: (_: any, record: any) => (
        <Space size="small">
          {actions.map((action) => {
            const ButtonComponent = action.confirm ? (
              <Popconfirm
                key={action.key}
                title={action.confirm.title}
                description={action.confirm.description}
                onConfirm={() => action.onClick(record)}
                okText="Yes"
                cancelText="No"
                okButtonProps={{ 
                  danger: action.type === 'danger',
                  loading: action.loading 
                }}
              >
                <Button
                  type={action.type === 'primary' ? 'primary' : 'text'}
                  danger={action.type === 'danger'}
                  icon={action.icon}
                  loading={action.loading}
                  size="small"
                  style={{
                    borderRadius: designTokens.borderRadius.medium,
                    boxShadow: action.type === 'primary' ? designTokens.shadows.small : 'none',
                    minWidth: action.label ? 'auto' : '32px',
                    height: '32px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  {action.label}
                </Button>
              </Popconfirm>
            ) : (
              <Button
                key={action.key}
                type={action.type === 'primary' ? 'primary' : 'text'}
                danger={action.type === 'danger'}
                icon={action.icon}
                loading={action.loading}
                onClick={() => action.onClick(record)}
                size="small"
                style={{
                  borderRadius: designTokens.borderRadius.medium,
                  boxShadow: action.type === 'primary' ? designTokens.shadows.small : 'none',
                  minWidth: action.label ? 'auto' : '32px',
                  height: '32px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                {action.label}
              </Button>
            );

            return action.tooltip ? (
              <Tooltip key={action.key} title={action.tooltip}>
                {ButtonComponent}
              </Tooltip>
            ) : ButtonComponent;
          })}
        </Space>
      ),
    }] : [])
  ];

  return (
    <div 
      className={`llm-table ${className}`}
      style={{
        background: designTokens.colors.white,
        borderRadius: designTokens.borderRadius.large,
        boxShadow: designTokens.shadows.medium,
        overflow: 'hidden',
      }}
    >
      <Table
        columns={enhancedColumns}
        dataSource={dataSource}
        loading={loading}
        rowKey={rowKey}
        size={size}
        pagination={pagination}
        onRow={onRow}
        style={{
          '--table-header-bg': designTokens.colors.backgroundSecondary,
          '--table-row-hover-bg': designTokens.colors.backgroundHover,
        } as React.CSSProperties}
        className="enhanced-table"
      />
      
      <style jsx>{`
        .enhanced-table .ant-table-thead > tr > th {
          background: var(--table-header-bg) !important;
          border-bottom: 2px solid ${designTokens.colors.border};
          font-weight: ${designTokens.typography.fontWeight.semibold};
          color: ${designTokens.colors.text};
          padding: ${designTokens.spacing.md} ${designTokens.spacing.md};
        }
        
        .enhanced-table .ant-table-tbody > tr > td {
          padding: ${designTokens.spacing.md} ${designTokens.spacing.md};
          border-bottom: 1px solid ${designTokens.colors.border};
        }
        
        .enhanced-table .ant-table-tbody > tr:hover > td {
          background: var(--table-row-hover-bg) !important;
        }
        
        .enhanced-table .ant-table-pagination {
          padding: ${designTokens.spacing.md} ${designTokens.spacing.lg};
          background: ${designTokens.colors.backgroundSecondary};
          border-top: 1px solid ${designTokens.colors.border};
        }
      `}</style>
    </div>
  );
};

export default LLMTable;

// Predefined action sets for different entity types
export const providerActions = (
  onTest: (record: any) => void,
  onEdit: (record: any) => void,
  onDelete: (record: any) => void,
  testingId?: string
): LLMTableAction[] => [
  {
    key: 'test',
    label: '',
    icon: <CheckCircleOutlined />,
    onClick: onTest,
    type: 'primary',
    tooltip: 'Test provider connection',
    loading: testingId !== undefined,
  },
  {
    key: 'edit',
    label: '',
    icon: <EditOutlined />,
    onClick: onEdit,
    tooltip: 'Edit provider configuration',
  },
  {
    key: 'delete',
    label: '',
    icon: <DeleteOutlined />,
    onClick: onDelete,
    type: 'danger',
    tooltip: 'Delete provider',
    confirm: {
      title: 'Delete Provider',
      description: 'Are you sure you want to delete this provider? This action cannot be undone.',
    },
  },
];

export const modelActions = (
  onEdit: (record: any) => void,
  onDelete: (record: any) => void,
  onToggle?: (record: any) => void
): LLMTableAction[] => [
  ...(onToggle ? [{
    key: 'toggle',
    label: '',
    icon: <PlayCircleOutlined />,
    onClick: onToggle,
    tooltip: 'Enable/Disable model',
  }] : []),
  {
    key: 'edit',
    label: '',
    icon: <EditOutlined />,
    onClick: onEdit,
    tooltip: 'Edit model configuration',
  },
  {
    key: 'delete',
    label: '',
    icon: <DeleteOutlined />,
    onClick: onDelete,
    type: 'danger',
    tooltip: 'Delete model',
    confirm: {
      title: 'Delete Model',
      description: 'Are you sure you want to delete this model configuration?',
    },
  },
];
