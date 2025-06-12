/**
 * Query Feature Module
 * 
 * Consolidated query-related components with modern patterns.
 * Combines QueryInterface, QueryBuilder, QueryHistory, and related components.
 */

// Core Query Components
export { QueryInterface } from '../../QueryInterface/QueryInterface';
export { QueryBuilder } from '../../QueryInterface/QueryBuilder';
export { QueryHistory } from '../../QueryInterface/QueryHistory';
export { QuerySuggestions } from '../../QueryInterface/QuerySuggestions';
export { QueryEditor } from '../../QueryInterface/QueryEditor';
export { SqlEditor } from '../../QueryInterface/SqlEditor';

// Advanced Query Components
export { QueryInterface } from '../../QueryInterface/QueryInterface';
export { QueryInput } from '../../QueryInterface/QueryInput';
export { AdvancedStreamingQuery } from '../../QueryInterface/AdvancedStreamingQuery';
export { GuidedQueryWizard } from '../../QueryInterface/GuidedQueryWizard';

// Query Results & Display
export { QueryResult } from '../../QueryInterface/QueryResult';
export { InteractiveResultsDisplay } from '../../QueryInterface/InteractiveResultsDisplay';
export { ExportModal } from '../../QueryInterface/ExportModal';

// Query Support Components
// MockDataToggle removed - database connection always required
export { QueryShortcuts } from '../../QueryInterface/QueryShortcuts';
export { QueryModals } from '../../QueryInterface/QueryModals';
export { QueryTabs } from '../../QueryInterface/QueryTabs';
export { LoadingStates } from '../../QueryInterface/LoadingStates';

// AI & Processing
export { AIProcessingFeedback } from '../../QueryInterface/AIProcessingFeedback';
export { ProactiveSuggestions } from '../../QueryInterface/ProactiveSuggestions';
export { PromptDetailsPanel } from '../../QueryInterface/PromptDetailsPanel';
export { QueryProcessingViewer } from '../../QueryInterface/QueryProcessingViewer';

// Templates & Patterns
export { QueryTemplateLibrary } from '../../QueryTemplates/QueryTemplateLibrary';

// Accessibility
export { AccessibilityFeatures } from '../../QueryInterface/AccessibilityFeatures';

// Types
export type * from '../../../types/query';
