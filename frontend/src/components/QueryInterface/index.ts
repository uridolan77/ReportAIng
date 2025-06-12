/**
 * Query Interface Components
 * Consolidated exports for all query interface functionality
 */

// Import styles
import '../styles/query-interface.css';

// Main Components (most commonly used)
export { QueryInterface } from './QueryInterface';
export { SimpleQueryInterface } from './SimpleQueryInterface';
export { QueryInput } from './QueryInput';
export { QueryBuilder } from './QueryBuilder';
export { MinimalQueryInterface } from './MinimalQueryInterface';
export { QueryEditor } from './QueryEditor';
export { SqlEditor } from './SqlEditor';
export { QueryResult } from './QueryResult';

// Provider & Context
export { QueryProvider, useQueryContext } from './QueryProvider';

// UI Components
export { QueryTabs } from './QueryTabs';
export { QueryModals } from './QueryModals';
export { ExportModal } from './ExportModal';
export { LoadingStates } from './LoadingStates';

// History & Suggestions
export { QueryHistory } from './QueryHistory';
export { QuerySuggestions } from './QuerySuggestions';
export { ProactiveSuggestions } from './ProactiveSuggestions';

// Wizard Components
export { QueryWizard } from './QueryWizard';
export { GuidedQueryWizard } from './GuidedQueryWizard';
export * from './WizardSteps';

// AI & Processing
export { AIProcessingFeedback } from './AIProcessingFeedback';
export { QueryProcessingViewer } from './QueryProcessingViewer';
export { PromptDetailsPanel } from './PromptDetailsPanel';

// Advanced Features
export { AdvancedStreamingQuery } from './AdvancedStreamingQuery';
export { InteractiveResultsDisplay } from './InteractiveResultsDisplay';
export { AccessibilityFeatures } from './AccessibilityFeatures';
export { QueryShortcuts } from './QueryShortcuts';

// All components also available through sub-components export
export * from './components';
