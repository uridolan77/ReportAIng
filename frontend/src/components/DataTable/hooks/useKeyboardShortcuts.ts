// filepath: c:\dev\ReportAIng\frontend\src\components\DataTable\hooks\useKeyboardShortcuts.ts
import { useHotkeys } from 'react-hotkeys-hook';
import { DataTableFeatures } from '../types';

interface UseKeyboardShortcutsProps {
  enabledFeatures: DataTableFeatures;
  setShowExportModal: (show: boolean) => void;
  setShowColumnChooser: (show: boolean) => void;
  setShowFilterPanel: (show: boolean) => void;
  handlePrint: () => void;
  toggleFullscreen: () => void;
}

export const useKeyboardShortcuts = ({
  enabledFeatures,
  setShowExportModal,
  setShowColumnChooser,
  setShowFilterPanel,
  handlePrint,
  toggleFullscreen
}: UseKeyboardShortcutsProps) => {

  // Search shortcut
  useHotkeys('ctrl+f', (e) => {
    e.preventDefault();
    if (enabledFeatures.searching) {
      document.getElementById('datatable-search')?.focus();
    }
  }, [enabledFeatures.searching]);

  // Export shortcut
  useHotkeys('ctrl+e', (e) => {
    e.preventDefault();
    if (enabledFeatures.export) {
      setShowExportModal(true);
    }
  }, [enabledFeatures.export, setShowExportModal]);

  // Print shortcut
  useHotkeys('ctrl+p', (e) => {
    e.preventDefault();
    if (enabledFeatures.print) {
      handlePrint();
    }
  }, [enabledFeatures.print, handlePrint]);

  // Fullscreen shortcut
  useHotkeys('f11', (e) => {
    e.preventDefault();
    if (enabledFeatures.fullscreen) {
      toggleFullscreen();
    }
  }, [enabledFeatures.fullscreen, toggleFullscreen]);

  // Column chooser shortcut
  useHotkeys('ctrl+shift+c', (e) => {
    e.preventDefault();
    if (enabledFeatures.columnChooser) {
      setShowColumnChooser(true);
    }
  }, [enabledFeatures.columnChooser, setShowColumnChooser]);

  // Filter panel shortcut
  useHotkeys('ctrl+shift+f', (e) => {
    e.preventDefault();
    if (enabledFeatures.filtering) {
      setShowFilterPanel(true);
    }
  }, [enabledFeatures.filtering, setShowFilterPanel]);

  // Close modals shortcut
  useHotkeys('escape', () => {
    setShowColumnChooser(false);
    setShowFilterPanel(false);
    setShowExportModal(false);
  }, [setShowColumnChooser, setShowFilterPanel, setShowExportModal]);
};
