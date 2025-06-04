// Context Menu Service for DataTable
import React, { useState, useCallback, useEffect, useRef } from 'react';
import { Menu, Dropdown } from 'antd';
import type { MenuProps } from 'antd';
import {
  CopyOutlined,
  EditOutlined,
  DeleteOutlined,
  PlusOutlined,
  FilterOutlined,
  SortAscendingOutlined,
  SortDescendingOutlined,
  EyeInvisibleOutlined,
  ColumnWidthOutlined,
  ExportOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import { DataTableColumn } from '../DataTable';

export interface ContextMenuAction {
  key: string;
  label: string;
  icon?: React.ReactNode;
  disabled?: boolean;
  danger?: boolean;
  children?: ContextMenuAction[];
  onClick?: (context: ContextMenuContext) => void;
}

export interface ContextMenuContext {
  type: 'cell' | 'row' | 'header' | 'table';
  record?: any;
  column?: DataTableColumn;
  value?: any;
  rowIndex?: number;
  columnIndex?: number;
  selectedRows?: any[];
  event: React.MouseEvent;
}

interface ContextMenuServiceProps {
  children: React.ReactNode;
  onAction?: (action: string, context: ContextMenuContext) => void;
  customActions?: ContextMenuAction[];
  enabledFeatures?: {
    copy?: boolean;
    edit?: boolean;
    delete?: boolean;
    insert?: boolean;
    filter?: boolean;
    sort?: boolean;
    export?: boolean;
    columnOperations?: boolean;
  };
}

const defaultActions: Record<string, ContextMenuAction[]> = {
  cell: [
    {
      key: 'copy',
      label: 'Copy Cell',
      icon: <CopyOutlined />,
      onClick: (context) => {
        if (context.value != null) {
          navigator.clipboard.writeText(String(context.value));
        }
      }
    },
    {
      key: 'edit',
      label: 'Edit Cell',
      icon: <EditOutlined />
    },
    { key: 'divider1', label: 'divider' },
    {
      key: 'filter',
      label: 'Filter by Value',
      icon: <FilterOutlined />
    },
    {
      key: 'sort',
      label: 'Sort',
      icon: <SortAscendingOutlined />,
      children: [
        {
          key: 'sort-asc',
          label: 'Sort Ascending',
          icon: <SortAscendingOutlined />
        },
        {
          key: 'sort-desc',
          label: 'Sort Descending',
          icon: <SortDescendingOutlined />
        }
      ]
    }
  ],
  row: [
    {
      key: 'copy-row',
      label: 'Copy Row',
      icon: <CopyOutlined />
    },
    {
      key: 'edit-row',
      label: 'Edit Row',
      icon: <EditOutlined />
    },
    {
      key: 'delete-row',
      label: 'Delete Row',
      icon: <DeleteOutlined />,
      danger: true
    },
    { key: 'divider1', label: 'divider' },
    {
      key: 'insert-above',
      label: 'Insert Row Above',
      icon: <PlusOutlined />
    },
    {
      key: 'insert-below',
      label: 'Insert Row Below',
      icon: <PlusOutlined />
    }
  ],
  header: [
    {
      key: 'sort-asc',
      label: 'Sort Ascending',
      icon: <SortAscendingOutlined />
    },
    {
      key: 'sort-desc',
      label: 'Sort Descending',
      icon: <SortDescendingOutlined />
    },
    { key: 'divider1', label: 'divider' },
    {
      key: 'filter-column',
      label: 'Filter Column',
      icon: <FilterOutlined />
    },
    {
      key: 'hide-column',
      label: 'Hide Column',
      icon: <EyeInvisibleOutlined />
    },
    {
      key: 'resize-column',
      label: 'Auto Resize',
      icon: <ColumnWidthOutlined />
    },
    { key: 'divider2', label: 'divider' },
    {
      key: 'move-left',
      label: 'Move Left',
      disabled: false
    },
    {
      key: 'move-right',
      label: 'Move Right',
      disabled: false
    }
  ],
  table: [
    {
      key: 'refresh',
      label: 'Refresh',
      icon: <ReloadOutlined />
    },
    {
      key: 'export',
      label: 'Export',
      icon: <ExportOutlined />,
      children: [
        { key: 'export-csv', label: 'Export as CSV' },
        { key: 'export-excel', label: 'Export as Excel' },
        { key: 'export-pdf', label: 'Export as PDF' }
      ]
    },
    { key: 'divider1', label: 'divider' },
    {
      key: 'select-all',
      label: 'Select All'
    },
    {
      key: 'clear-selection',
      label: 'Clear Selection'
    }
  ]
};

export const ContextMenuProvider: React.FC<ContextMenuServiceProps> = ({
  children,
  onAction,
  customActions = [],
  enabledFeatures = {}
}) => {
  const [contextMenu, setContextMenu] = useState<{
    visible: boolean;
    x: number;
    y: number;
    context: ContextMenuContext | null;
  }>({
    visible: false,
    x: 0,
    y: 0,
    context: null
  });

  const containerRef = useRef<HTMLDivElement>(null);

  const features = {
    copy: true,
    edit: true,
    delete: true,
    insert: true,
    filter: true,
    sort: true,
    export: true,
    columnOperations: true,
    ...enabledFeatures
  };

  const filterActions = useCallback((actions: ContextMenuAction[], context: ContextMenuContext) => {
    return actions.filter(action => {
      switch (action.key) {
        case 'copy':
        case 'copy-row':
          return features.copy;
        case 'edit':
        case 'edit-row':
          return features.edit;
        case 'delete-row':
          return features.delete;
        case 'insert-above':
        case 'insert-below':
          return features.insert;
        case 'filter':
        case 'filter-column':
          return features.filter;
        case 'sort':
        case 'sort-asc':
        case 'sort-desc':
          return features.sort;
        case 'export':
        case 'export-csv':
        case 'export-excel':
        case 'export-pdf':
          return features.export;
        case 'hide-column':
        case 'resize-column':
        case 'move-left':
        case 'move-right':
          return features.columnOperations;
        default:
          return true;
      }
    }).map(action => ({
      ...action,
      children: action.children ? filterActions(action.children, context) : undefined
    }));
  }, [features]);

  const buildMenuItems = useCallback((actions: ContextMenuAction[], context: ContextMenuContext): MenuProps['items'] => {
    const filteredActions = filterActions([...actions, ...customActions], context);
    
    return filteredActions.map(action => {
      if (action.label === 'divider') {
        return { type: 'divider', key: action.key };
      }

      return {
        key: action.key,
        label: action.label,
        icon: action.icon,
        disabled: action.disabled,
        danger: action.danger,
        children: action.children ? buildMenuItems(action.children, context) : undefined,
        onClick: () => {
          action.onClick?.(context);
          onAction?.(action.key, context);
          setContextMenu(prev => ({ ...prev, visible: false }));
        }
      };
    });
  }, [filterActions, customActions, onAction]);

  const handleContextMenu = useCallback((event: React.MouseEvent, context: Omit<ContextMenuContext, 'event'>) => {
    event.preventDefault();
    event.stopPropagation();

    const fullContext: ContextMenuContext = { ...context, event };
    
    setContextMenu({
      visible: true,
      x: event.clientX,
      y: event.clientY,
      context: fullContext
    });
  }, []);

  const handleClickOutside = useCallback((event: MouseEvent) => {
    if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
      setContextMenu(prev => ({ ...prev, visible: false }));
    }
  }, []);

  useEffect(() => {
    if (contextMenu.visible) {
      document.addEventListener('click', handleClickOutside);
      return () => document.removeEventListener('click', handleClickOutside);
    }
  }, [contextMenu.visible, handleClickOutside]);

  const getMenuItems = () => {
    if (!contextMenu.context) return [];
    
    const { type } = contextMenu.context;
    const baseActions = defaultActions[type] || [];
    
    return buildMenuItems(baseActions, contextMenu.context);
  };

  return (
    <div ref={containerRef} style={{ position: 'relative', height: '100%' }}>
      {React.cloneElement(children as React.ReactElement, {
        onContextMenu: handleContextMenu
      })}
      
      {contextMenu.visible && (
        <div
          style={{
            position: 'fixed',
            top: contextMenu.y,
            left: contextMenu.x,
            zIndex: 9999
          }}
        >
          <Menu
            items={getMenuItems()}
            style={{
              border: '1px solid #d9d9d9',
              borderRadius: 6,
              boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
            }}
          />
        </div>
      )}
    </div>
  );
};

// Hook for context menu actions
export const useContextMenuActions = () => {
  const [lastAction, setLastAction] = useState<{
    action: string;
    context: ContextMenuContext;
    timestamp: number;
  } | null>(null);

  const handleAction = useCallback((action: string, context: ContextMenuContext) => {
    setLastAction({
      action,
      context,
      timestamp: Date.now()
    });

    // Built-in action handlers
    switch (action) {
      case 'copy':
        if (context.value != null) {
          navigator.clipboard.writeText(String(context.value));
        }
        break;
      
      case 'copy-row':
        if (context.record) {
          const rowText = Object.values(context.record).join('\t');
          navigator.clipboard.writeText(rowText);
        }
        break;
      
      case 'select-all':
        // Will be handled by parent component
        break;
      
      case 'clear-selection':
        // Will be handled by parent component
        break;
      
      default:
        // Custom actions handled by parent
        break;
    }
  }, []);

  return {
    lastAction,
    handleAction,
    clearLastAction: () => setLastAction(null)
  };
};

// Context for sharing context menu state
export const ContextMenuContext = React.createContext<{
  showContextMenu: (event: React.MouseEvent, context: Omit<ContextMenuContext, 'event'>) => void;
  hideContextMenu: () => void;
} | null>(null);

export const useContextMenu = () => {
  const context = React.useContext(ContextMenuContext);
  if (!context) {
    throw new Error('useContextMenu must be used within ContextMenuProvider');
  }
  return context;
};
