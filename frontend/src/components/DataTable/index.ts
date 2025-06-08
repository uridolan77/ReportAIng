// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable/index.ts
export { default } from './DataTableMain';
export type { DataTableProps, DataTableColumn, DataTableFeatures, DataTableConfig } from './types';

// Re-export components for external use
export { DataTableToolbar } from './components/DataTableToolbar';
// Note: FilterPanel is internal to DataTable and not exported to prevent circular dependencies

// Re-export enhanced functionality
export { useEnhancedColumns, useAutoColumns, createColumnsFromData } from './hooks/useEnhancedColumns';
export { DataTypeDetector, enhanceColumnsWithTypeDetection } from './utils/dataTypeDetection';
export type { DataTypeAnalysis, ColumnAnalysis } from './utils/dataTypeDetection';

// Note: Demo component is not exported to prevent circular dependencies
// Import directly from './demo/EnhancedFilteringDemo' if needed
export { ColumnChooserModal, ExportModal } from './components/DataTableModals';
export { AggregationRow } from './components/AggregationRow';
export { VirtualizedTable, StandardTable } from './components/DataTableRenderer';

// Re-export services
export { ExportService } from './services/ExportService';
export { VirtualizationService } from './services/VirtualizationService';
export { ContextMenuProvider, useContextMenuActions } from './services/ContextMenuService';

// Re-export hooks
export { useEnhancedVirtualization } from './hooks/useEnhancedVirtualization';
export { useDataTableState } from './hooks/useDataTableState';
export { useDataProcessing } from './hooks/useDataProcessing';
export { useDataTableHandlers } from './hooks/useDataTableHandlers';
export { useContextMenuHandlers } from './hooks/useContextMenuHandlers';
export { useKeyboardShortcuts } from './hooks/useKeyboardShortcuts';
