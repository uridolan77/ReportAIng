import React, { useState } from 'react';
import { Button, Input, Space, Dropdown, Modal, Upload, message, Badge, Tooltip } from 'antd';
import {
  SearchOutlined,
  DownloadOutlined,
  FilterOutlined,
  SettingOutlined,
  ReloadOutlined,
  FullscreenOutlined,
  FullscreenExitOutlined,
  PrinterOutlined,
  GroupOutlined,
  SaveOutlined,
  FolderOpenOutlined,
  DeleteOutlined,
  ExportOutlined,
  ImportOutlined,
  DashboardOutlined
} from '@ant-design/icons';
import { useToken } from 'antd/es/theme/internal';

interface DataTableToolbarProps {
  enabledFeatures: {
    searching: boolean;
    filtering: boolean;
    grouping: boolean;
    export: boolean;
    print: boolean;
    columnChooser: boolean;
    fullscreen: boolean;
    saveState?: boolean;
  };
  searchText: string;
  onSearchChange: (value: string) => void;
  filterCount: number;
  onToggleFilterPanel: () => void;
  onExport: () => void;
  onPrint: () => void;
  onColumnChooser: () => void;
  onRefresh: () => void;
  isFullscreen: boolean;
  onToggleFullscreen: () => void;
  onGroupBy: () => void;
  onSaveState?: () => void;
  onLoadState?: () => void;
  onClearState?: () => void;
  onExportState?: () => string | null;
  onImportState?: (stateJson: string) => boolean;
  performanceMetrics?: any;
}

export const DataTableToolbar: React.FC<DataTableToolbarProps> = ({
  enabledFeatures,
  searchText,
  onSearchChange,
  filterCount,
  onToggleFilterPanel,
  onExport,
  onPrint,
  onColumnChooser,
  onRefresh,
  isFullscreen,
  onToggleFullscreen,
  onGroupBy,
  onSaveState,
  onLoadState,
  onClearState,
  onExportState,
  onImportState,
  performanceMetrics
}) => {
  const [, token] = useToken();
  const [showImportModal, setShowImportModal] = useState(false);
  const [importText, setImportText] = useState('');

  const handleExportState = () => {
    const stateJson = onExportState?.();
    if (stateJson) {
      const blob = new Blob([stateJson], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `datatable-state-${new Date().toISOString().split('T')[0]}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      message.success('State exported successfully');
    }
  };

  const handleImportState = () => {
    if (importText.trim()) {
      const success = onImportState?.(importText);
      if (success) {
        message.success('State imported successfully');
        setShowImportModal(false);
        setImportText('');
      } else {
        message.error('Failed to import state');
      }
    }
  };
  const stateMenuItems = [
    {
      key: 'save',
      label: 'Save Current State',
      icon: <SaveOutlined />,
      onClick: () => {
        onSaveState?.();
        message.success('State saved');
      }
    },
    {
      key: 'load',
      label: 'Load Saved State',
      icon: <FolderOpenOutlined />,
      onClick: onLoadState
    },
    {
      type: 'divider' as const
    },
    {
      key: 'export',
      label: 'Export State',
      icon: <ExportOutlined />,
      onClick: handleExportState
    },
    {
      key: 'import',
      label: 'Import State',
      icon: <ImportOutlined />,
      onClick: () => setShowImportModal(true)
    },
    {
      type: 'divider' as const
    },
    {
      key: 'clear',
      label: 'Clear Saved State',
      icon: <DeleteOutlined />,
      danger: true,
      onClick: () => {
        Modal.confirm({
          title: 'Clear Saved State',
          content: 'Are you sure you want to clear the saved state? This action cannot be undone.',
          onOk: () => {
            onClearState?.();
            message.success('State cleared');
          }
        });
      }
    }
  ];

  return (
    <div style={{
      padding: '16px',
      background: token.colorBgContainer,
      borderBottom: `1px solid ${token.colorBorderSecondary}`,
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      flexWrap: 'wrap',
      gap: '16px'
    }}>
      <Space wrap>
        {enabledFeatures.searching && (
          <Input
            id="datatable-search"
            placeholder="Search..."
            prefix={<SearchOutlined />}
            value={searchText}
            onChange={e => onSearchChange(e.target.value)}
            style={{ width: 250 }}
            allowClear
          />
        )}
        
        {enabledFeatures.filtering && (
          <Button
            icon={<FilterOutlined />}
            onClick={onToggleFilterPanel}
          >
            Filters {filterCount > 0 && `(${filterCount})`}
          </Button>
        )}
        
        {enabledFeatures.grouping && (
          <Button icon={<GroupOutlined />} onClick={onGroupBy}>
            Group By
          </Button>
        )}
      </Space>
      
      <Space wrap>
        {enabledFeatures.export && (
          <Button
            icon={<DownloadOutlined />}
            onClick={onExport}
          >
            Export
          </Button>
        )}
        
        {enabledFeatures.print && (
          <Button
            icon={<PrinterOutlined />}
            onClick={onPrint}
          >
            Print
          </Button>
        )}
        
        {enabledFeatures.columnChooser && (
          <Button
            icon={<SettingOutlined />}
            onClick={onColumnChooser}
          >
            Columns
          </Button>
        )}
        
        {enabledFeatures.fullscreen && (
          <Button
            icon={isFullscreen ? <FullscreenExitOutlined /> : <FullscreenOutlined />}
            onClick={onToggleFullscreen}
          />
        )}
          <Button
          icon={<ReloadOutlined />}
          onClick={onRefresh}
        />
        
        {enabledFeatures.saveState && (
          <Dropdown 
            menu={{ items: stateMenuItems }}
            trigger={['click']}
          >
            <Button icon={<SaveOutlined />}>
              State
            </Button>
          </Dropdown>
        )}

        {performanceMetrics && (
          <Tooltip title={
            <div>
              <div>Render Time: {performanceMetrics.renderTime?.toFixed(2) || 0}ms</div>
              <div>Visible Rows: {performanceMetrics.visibleRows || 0}</div>
              <div>Total Rows: {performanceMetrics.totalRows || 0}</div>
              <div>Scroll Position: {performanceMetrics.scrollTop?.toFixed(0) || 0}px</div>
            </div>
          }>
            <Badge count={performanceMetrics.renderTime ? Math.round(performanceMetrics.renderTime) : 0}>
              <Button icon={<DashboardOutlined />} size="small" />
            </Badge>
          </Tooltip>
        )}
      </Space>

      <Modal
        title="Import Table State"
        open={showImportModal}
        onOk={handleImportState}
        onCancel={() => {
          setShowImportModal(false);
          setImportText('');
        }}
        okText="Import"
        cancelText="Cancel"
      >
        <div style={{ marginBottom: 16 }}>
          <p>Paste the exported JSON state below:</p>
          <Input.TextArea
            rows={10}
            value={importText}
            onChange={e => setImportText(e.target.value)}
            placeholder="Paste JSON state content here..."
          />
        </div>
        <Upload
          accept=".json"
          beforeUpload={(file) => {
            const reader = new FileReader();
            reader.onload = (e) => {
              try {
                const content = e.target?.result as string;
                setImportText(content);
              } catch (error) {
                message.error('Failed to read file');
              }
            };
            reader.readAsText(file);
            return false; // Prevent automatic upload
          }}
          showUploadList={false}
        >
          <Button>Upload JSON File</Button>
        </Upload>
      </Modal>
    </div>
  );
};
