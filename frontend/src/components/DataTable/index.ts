// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable/index.ts
export { default } from './DataTableMain';
export type { DataTableProps, DataTableColumn, DataTableFeatures, DataTableConfig } from './types';

// Re-export components for external use
export { DataTableToolbar } from './components/DataTableToolbar';
export { FilterPanel } from './components/FilterPanel';
export { ColumnChooserModal, ExportModal } from './components/DataTableModals';
export { AggregationRow } from './components/AggregationRow';
export { VirtualizedTable, StandardTable } from './components/DataTableRenderer';

// Re-export services
export { ExportService } from './services/ExportService';
export { VirtualizationService } from './services/VirtualizationService';
export { StatePersistenceService } from './services/StatePersistenceService';
export { ContextMenuProvider, useContextMenuActions } from './services/ContextMenuService';

// Re-export hooks
export { useEnhancedVirtualization } from './hooks/useEnhancedVirtualization';
export { useDataTableState } from './hooks/useDataTableState';
export { useDataProcessing } from './hooks/useDataProcessing';
export { useDataTableHandlers } from './hooks/useDataTableHandlers';
export { useContextMenuHandlers } from './hooks/useContextMenuHandlers';
export { useKeyboardShortcuts } from './hooks/useKeyboardShortcuts';
