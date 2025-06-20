// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\DataTableMain.tsx
import React, { useRef, useEffect } from 'react';
import { ConfigProvider, theme, Spin, Alert, Empty } from 'antd';
import dayjs from 'dayjs';
import isBetween from 'dayjs/plugin/isBetween';
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';
// Import new UI components
import { PerformanceMonitor, VirtualList } from '../ui';

import { DataTableProps } from './types';
import { useDataTableState } from './hooks/useDataTableState';
import { useDataProcessing } from './hooks/useDataProcessing';
import { useDataTableHandlers } from './hooks/useDataTableHandlers';
import { useContextMenuHandlers } from './hooks/useContextMenuHandlers';
import { useKeyboardShortcuts } from './hooks/useKeyboardShortcuts';
import { useEnhancedVirtualization } from './hooks/useEnhancedVirtualization';
import { useEnhancedColumns } from './hooks/useEnhancedColumns';

// Import sub-components
import { DataTableToolbar } from './components/DataTableToolbar';
import { FilterPanel } from './components/FilterPanel';
import { ColumnChooserModal, ExportModal } from './components/DataTableModals';
import { AggregationRow } from './components/AggregationRow';
import { VirtualizedTable, StandardTable } from './components/DataTableRenderer';
import { ContextMenuProvider } from './services/ContextMenuService';
import { VirtualizationService } from './services/VirtualizationService';

// Large dataset threshold for virtual scrolling
const VIRTUAL_SCROLL_THRESHOLD = 1000;

const { useToken } = theme;

// Initialize dayjs plugins
dayjs.extend(isBetween);
dayjs.extend(isSameOrAfter);
dayjs.extend(isSameOrBefore);

const DataTable: React.FC<DataTableProps> = (props) => {
  const {
    data = [],
    columns = [],
    keyField = 'id',
    loading = false,
    error = null,
    autoDetectTypes = false,
    autoGenerateFilterOptions = true,

    features = {},
    config = {},
    components = {},
    className,
    style,
    tableStyle,
    rowStyle
  } = props;

  const { token } = useToken();
  const tableRef = useRef<HTMLDivElement>(null);

  // Enhanced columns with automatic type detection
  const { enhancedColumns, columnAnalysis } = useEnhancedColumns({
    data,
    columns,
    autoDetectTypes,
    sampleSize: 100
  });
  // Use enhanced columns if auto-detection is enabled, otherwise use original columns
  const finalColumns = autoDetectTypes ? enhancedColumns : columns;

  // Custom hooks for state management
  const {
    state,
    actions,
    enabledFeatures,
    visibleColumns,
    debouncedSearchText,
    virtualizationServiceRef
  } = useDataTableState({ columns: finalColumns, features, config });

  // Data processing
  const { processedData, paginatedData, totalRecords } = useDataProcessing({
    data,
    debouncedSearchText,
    sortConfig: state.sortConfig,
    filterConfig: state.filterConfig,
    visibleColumns,
    enabledFeatures,
    currentPage: state.currentPage,
    pageSize: state.pageSize,
    hiddenRows: state.hiddenRows,
    keyField
  });

  // Event handlers
  const handlers = useDataTableHandlers({
    state,
    actions,
    enabledFeatures,
    processedData,
    visibleColumns,
    config,
    tableRef,
    keyField,
    props,
    data,
    debouncedSearchText
  });

  // Context menu handlers
  const { handleTableContextMenu } = useContextMenuHandlers({
    enabledFeatures,
    visibleColumns,
    processedData,
    selectedRows: state.selectedRows,
    onSelectionChange: props.onSelectionChange,
    onCellClick: props.onCellClick,
    handleExport: handlers.handleExport,
    handleRefresh: handlers.handleRefresh,
    handleCopy: handlers.handleCopy,
    handleHideRow: handlers.handleHideRow,
    handleHideSelectedRows: handlers.handleHideSelectedRows,
    handleShowAllHiddenRows: handlers.handleShowAllHiddenRows
  });

  // Keyboard shortcuts
  useKeyboardShortcuts({
    enabledFeatures,
    setShowExportModal: actions.setShowExportModal,
    setShowColumnChooser: actions.setShowColumnChooser,
    setShowFilterPanel: actions.setShowFilterPanel,
    handlePrint: handlers.handlePrint,
    toggleFullscreen: handlers.toggleFullscreen
  });

  // Initialize virtualization service
  useEffect(() => {
    if (enabledFeatures.advancedVirtualization && !virtualizationServiceRef.current) {
      virtualizationServiceRef.current = new VirtualizationService({
        itemHeight: config.virtualization?.itemHeight || 50,
        containerHeight: typeof config.maxHeight === 'number' ? config.maxHeight : 500,
        overscan: config.virtualization?.overscan || 5,
        bufferSize: config.virtualization?.bufferSize || 10,
        scrollThreshold: 100,
        estimatedItemSize: config.virtualization?.estimatedItemSize || config.virtualization?.itemHeight || 50,
        dynamicHeight: config.virtualization?.enableDynamicSizing || enabledFeatures.dynamicRowHeight
      });
    }
  }, [enabledFeatures.advancedVirtualization, enabledFeatures.dynamicRowHeight, config, virtualizationServiceRef]);
  // Enhanced virtualization hook
  const { visibleData } = useEnhancedVirtualization({
    data: paginatedData,
    itemHeight: config.virtualization?.itemHeight || 50,
    containerHeight: typeof config.maxHeight === 'number' ? config.maxHeight : 500,
    enableDynamicHeight: config.virtualization?.enableDynamicSizing || enabledFeatures.dynamicRowHeight,
    enablePerformanceMonitoring: enabledFeatures.performanceMonitoring,
    overscan: config.virtualization?.overscan || 5,
    bufferSize: config.virtualization?.bufferSize || 10
  });

  // Update columns when prop changes
  useEffect(() => {
    actions.setDisplayColumns(finalColumns);
  }, [finalColumns, actions]);

  // Use enhanced virtualization data when advanced virtualization is enabled
  const finalData = enabledFeatures.advancedVirtualization ? visibleData : paginatedData;

  // Error handling
  if (error) {
    return components.errorState ? (
      <components.errorState error={error} />
    ) : (
      <Alert
        message="Error loading data"
        description={error.message}
        type="error"
        showIcon
        style={{ margin: '20px' }}
      />
    );
  }

  // Loading state
  if (loading) {
    return components.loadingIndicator ? (
      <components.loadingIndicator />
    ) : (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    );
  }

  // Empty state
  if (!loading && finalData.length === 0) {
    return components.emptyState ? (
      <components.emptyState />
    ) : (
      <Empty description="No data to display" style={{ padding: '50px' }} />
    );
  }

  return (    <PerformanceMonitor
      onMetrics={(metrics) => {
        // Performance metrics handled internally by PerformanceMonitor
      }}
    >
      <ConfigProvider theme={{ token }}>
        <ContextMenuProvider>
          <div
            ref={tableRef}
            className={`data-table-container ${className || ''}`}
            style={{
              ...style,
              height: '100%',
              width: '100%',
              overflow: 'hidden',
              position: 'relative'
            }}
          >
          {/* Toolbar */}          {enabledFeatures.searching || enabledFeatures.export || enabledFeatures.columnChooser ? (
            components.toolbar ? (
              <components.toolbar />
            ) : (
              <DataTableToolbar
                enabledFeatures={enabledFeatures}
                searchText={state.searchText}
                onSearchChange={handlers.handleSearch}
                filterCount={Object.keys(state.filterConfig).length}
                onToggleFilterPanel={() => actions.setShowFilterPanel(true)}
                onExport={() => actions.setShowExportModal(true)}
                onPrint={handlers.handlePrint}
                onColumnChooser={() => actions.setShowColumnChooser(true)}
                onRefresh={handlers.handleRefresh}
                isFullscreen={state.isFullscreen}
                onToggleFullscreen={handlers.toggleFullscreen}
                onGroupBy={() => {}}
                selectedRowsCount={state.selectedRows.length}
                hiddenRowsCount={state.hiddenRows.length}
                onHideSelectedRows={handlers.handleHideSelectedRows}
                onShowAllHiddenRows={handlers.handleShowAllHiddenRows}
              />
            )
          ) : null}
          {/* Filter Panel */}
          {state.showFilterPanel && (
            <FilterPanel
              visible={state.showFilterPanel}
              columns={finalColumns.filter(col => !col.hidden)}
              filterConfig={state.filterConfig}
              onFilterChange={handlers.handleFilter}
              onClose={() => actions.setShowFilterPanel(false)}
              onClearAll={() => actions.setFilterConfig({})}
            />
          )}

          {/* Table Container */}
          <div style={{ 
            flex: 1, 
            overflow: 'auto',
            ...tableStyle 
          }}>
            {/* Aggregation Row - Top */}
            {config.showAggregationRow && enabledFeatures.aggregation && (
              <AggregationRow
                columns={visibleColumns}
                data={processedData}
                enableSelection={enabledFeatures.selection}
                position="top"
              />
            )}

            {/* Main Table */}
            {enabledFeatures.virtualScroll || enabledFeatures.advancedVirtualization || finalData.length > VIRTUAL_SCROLL_THRESHOLD ? (
              // Use VirtualizedTable if available, otherwise fallback to VirtualList
              typeof VirtualizedTable !== 'undefined' ? (
                <VirtualizedTable
                  data={finalData}
                  columns={visibleColumns}
                  keyField={keyField}
                  height={tableRef.current?.clientHeight || 600}
                  width={tableRef.current?.clientWidth || 1200}
                  onRowClick={props.onRowClick}
                  onContextMenu={handleTableContextMenu}
                  rowSelection={enabledFeatures.selection ? {
                    selectedRowKeys: state.selectedRows.map(row => row[keyField]),
                    onChange: handlers.handleSelectionChange
                  } : undefined}
                  rowStyle={rowStyle}
                  virtualizationService={virtualizationServiceRef.current}
                  enableDynamicHeight={enabledFeatures.dynamicRowHeight}
                  enablePerformanceMonitoring={enabledFeatures.performanceMonitoring}
                />
              ) : (
                <VirtualList
                  items={finalData}
                  itemHeight={50}
                  containerHeight={tableRef.current?.clientHeight || 600}
                  renderItem={(item, index) => (
                    <div
                      key={item[keyField]}
                      style={{
                        display: 'flex',
                        padding: '8px 16px',
                        borderBottom: '1px solid #f0f0f0',
                        cursor: 'pointer'
                      }}
                      onClick={() => props.onRowClick?.(item, index)}
                    >
                      {visibleColumns.map(col => (
                        <div key={col.key} style={{ flex: 1, marginRight: '16px' }}>
                          {item[col.key]}
                        </div>
                      ))}
                    </div>
                  )}
                  overscan={5}
                />
              )
            ) : (
              <StandardTable
                data={finalData}
                columns={visibleColumns}
                keyField={keyField}
                pagination={enabledFeatures.pagination ? {
                  current: state.currentPage,
                  pageSize: state.pageSize,
                  total: totalRecords,
                  onChange: handlers.handlePageChange
                } : false}
                rowSelection={enabledFeatures.selection ? {
                  selectedRowKeys: state.selectedRows.map(row => row[keyField]),
                  onChange: handlers.handleSelectionChange
                } : undefined}
                onRow={(record, index) => ({
                  onClick: () => props.onRowClick?.(record, index || 0),
                  onDoubleClick: () => props.onRowDoubleClick?.(record, index || 0),
                  onContextMenu: (e) => handleTableContextMenu(e, { type: 'row', record, column: undefined })
                })}
                style={{ height: '100%', width: '100%' }}
                loading={loading}
              />
            )}

            {/* Aggregation Row - Bottom */}
            {config.showAggregationRow && enabledFeatures.aggregation && (
              <AggregationRow
                columns={visibleColumns}
                data={processedData}
                enableSelection={enabledFeatures.selection}
                position="bottom"
              />
            )}
          </div>

          {/* Footer */}
          {components.footer && <components.footer />}

          {/* Modals */}
          <ColumnChooserModal
            visible={state.showColumnChooser}
            columns={state.displayColumns}
            onColumnToggle={(columnKey, visible) => {
              const updated = state.displayColumns.map(col =>
                col.key === columnKey ? { ...col, hidden: !visible } : col
              );
              actions.setDisplayColumns(updated);
            }}
            onColumnReorder={handlers.handleColumnReorder}
            onClose={() => actions.setShowColumnChooser(false)}
          />

          <ExportModal
            visible={state.showExportModal}
            exportFormats={[...enabledFeatures.exportFormats || []]}
            onExport={handlers.handleExport}
            onClose={() => actions.setShowExportModal(false)}
          />
        </div>
      </ContextMenuProvider>
    </ConfigProvider>
    </PerformanceMonitor>
  );
};

export default DataTable;
export type { DataTableProps } from './types';
export type { DataTableColumn } from './types';
