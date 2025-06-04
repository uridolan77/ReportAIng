import React from 'react';
import { Button, Input, Space } from 'antd';
import {
  SearchOutlined,
  DownloadOutlined,
  FilterOutlined,
  SettingOutlined,
  ReloadOutlined,
  FullscreenOutlined,
  FullscreenExitOutlined,
  PrinterOutlined,
  GroupOutlined
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
  onGroupBy
}) => {
  const [, token] = useToken();

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
      </Space>
    </div>
  );
};
