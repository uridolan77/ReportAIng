import React from 'react';
import { useQueryContext } from './QueryProvider';
import { ExportModal } from './ExportModal';
import { QueryWizard } from './QueryWizard';
import { QueryTemplateLibrary } from '../QueryTemplates/QueryTemplateLibrary';
import { CommandPalette } from '../CommandPalette/CommandPalette';

export const QueryModals: React.FC = () => {
  const {
    showExportModal,
    setShowExportModal,
    showWizard,
    setShowWizard,
    showTemplateLibrary,
    setShowTemplateLibrary,
    showCommandPalette,
    setShowCommandPalette,
    currentResult,
    query,
    handleWizardQueryGenerated,
    handleTemplateApply
  } = useQueryContext();

  return (
    <>
      {/* Export Modal */}
      <ExportModal
        visible={showExportModal}
        onClose={() => setShowExportModal(false)}
        result={currentResult}
        query={query}
      />

      {/* Query Wizard Modal */}
      {showWizard && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          zIndex: 1000,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}>
          <div style={{
            backgroundColor: 'white',
            borderRadius: '8px',
            maxWidth: '95vw',
            maxHeight: '95vh',
            overflow: 'auto',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
          }}>
            <QueryWizard
              onQueryGenerated={handleWizardQueryGenerated}
              onClose={() => setShowWizard(false)}
            />
          </div>
        </div>
      )}

      {/* Query Template Library Modal */}
      {showTemplateLibrary && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          zIndex: 1000,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}>
          <div style={{
            backgroundColor: 'white',
            borderRadius: '8px',
            maxWidth: '95vw',
            maxHeight: '95vh',
            overflow: 'auto',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
          }}>
            <QueryTemplateLibrary
              onApplyTemplate={handleTemplateApply}
              onClose={() => setShowTemplateLibrary(false)}
            />
          </div>
        </div>
      )}

      {/* Command Palette */}
      <CommandPalette
        visible={showCommandPalette}
        onClose={() => setShowCommandPalette(false)}
      />
    </>
  );
};
