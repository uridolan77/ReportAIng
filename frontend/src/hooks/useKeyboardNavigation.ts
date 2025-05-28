import React, { useEffect, useCallback, useState } from 'react';
import { message } from 'antd';

interface KeyboardShortcut {
  key: string;
  ctrlKey?: boolean;
  shiftKey?: boolean;
  altKey?: boolean;
  metaKey?: boolean;
  action: () => void;
  description: string;
  category: string;
}

interface UseKeyboardNavigationOptions {
  onExecuteQuery?: () => void;
  onSaveQuery?: () => void;
  onUndo?: () => void;
  onRedo?: () => void;
  onToggleHelp?: () => void;
  onOpenCommandPalette?: () => void;
  onFocusQueryInput?: () => void;
  onClearQuery?: () => void;
  onExportResults?: () => void;
  onToggleFullscreen?: () => void;
  onNewQuery?: () => void;
  onOpenTemplates?: () => void;
  onToggleInsights?: () => void;
  onToggleHistory?: () => void;
}

export const useKeyboardNavigation = (options: UseKeyboardNavigationOptions = {}) => {
  const [isCommandPaletteOpen, setIsCommandPaletteOpen] = useState(false);
  const [focusedElement, setFocusedElement] = useState<string | null>(null);
  const [shortcuts, setShortcuts] = useState<KeyboardShortcut[]>([]);

  // Initialize shortcuts
  useEffect(() => {
    const defaultShortcuts: KeyboardShortcut[] = [
      // Query Operations
      {
        key: 'Enter',
        ctrlKey: true,
        action: () => options.onExecuteQuery?.(),
        description: 'Execute query',
        category: 'Query'
      },
      {
        key: 's',
        ctrlKey: true,
        action: () => {
          options.onSaveQuery?.();
          message.success('Query saved');
        },
        description: 'Save query',
        category: 'Query'
      },
      {
        key: 'n',
        ctrlKey: true,
        action: () => options.onNewQuery?.(),
        description: 'New query',
        category: 'Query'
      },
      {
        key: 'l',
        ctrlKey: true,
        action: () => options.onClearQuery?.(),
        description: 'Clear query',
        category: 'Query'
      },

      // Navigation
      {
        key: 'k',
        ctrlKey: true,
        action: () => {
          setIsCommandPaletteOpen(true);
          options.onOpenCommandPalette?.();
        },
        description: 'Open command palette',
        category: 'Navigation'
      },
      {
        key: 'i',
        ctrlKey: true,
        action: () => options.onFocusQueryInput?.(),
        description: 'Focus query input',
        category: 'Navigation'
      },
      {
        key: 't',
        ctrlKey: true,
        action: () => options.onOpenTemplates?.(),
        description: 'Open templates',
        category: 'Navigation'
      },
      {
        key: 'h',
        ctrlKey: true,
        action: () => options.onToggleHistory?.(),
        description: 'Toggle query history',
        category: 'Navigation'
      },

      // Edit Operations
      {
        key: 'z',
        ctrlKey: true,
        action: () => options.onUndo?.(),
        description: 'Undo',
        category: 'Edit'
      },
      {
        key: 'z',
        ctrlKey: true,
        shiftKey: true,
        action: () => options.onRedo?.(),
        description: 'Redo',
        category: 'Edit'
      },
      {
        key: 'y',
        ctrlKey: true,
        action: () => options.onRedo?.(),
        description: 'Redo (alternative)',
        category: 'Edit'
      },

      // View Operations
      {
        key: 'F11',
        action: () => options.onToggleFullscreen?.(),
        description: 'Toggle fullscreen',
        category: 'View'
      },
      {
        key: 'j',
        ctrlKey: true,
        action: () => options.onToggleInsights?.(),
        description: 'Toggle insights panel',
        category: 'View'
      },

      // Export/Share
      {
        key: 'e',
        ctrlKey: true,
        action: () => options.onExportResults?.(),
        description: 'Export results',
        category: 'Export'
      },

      // Help
      {
        key: 'F1',
        action: () => options.onToggleHelp?.(),
        description: 'Show help',
        category: 'Help'
      },
      {
        key: '?',
        ctrlKey: true,
        action: () => options.onToggleHelp?.(),
        description: 'Show keyboard shortcuts',
        category: 'Help'
      }
    ];

    setShortcuts(defaultShortcuts);
  }, [options]);

  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    // Don't trigger shortcuts when typing in input fields
    const target = event.target as HTMLElement;
    const isInputField = target.tagName === 'INPUT' ||
                        target.tagName === 'TEXTAREA' ||
                        target.contentEditable === 'true' ||
                        target.closest('.ant-select-dropdown') ||
                        target.closest('.ant-modal');

    // Allow Ctrl+Enter in input fields for query execution
    if (isInputField && !(event.ctrlKey && event.key === 'Enter')) {
      return;
    }

    // Find matching shortcut
    const matchingShortcut = shortcuts.find(shortcut => {
      return shortcut.key === event.key &&
             !!shortcut.ctrlKey === event.ctrlKey &&
             !!shortcut.shiftKey === event.shiftKey &&
             !!shortcut.altKey === event.altKey &&
             !!shortcut.metaKey === event.metaKey;
    });

    if (matchingShortcut) {
      event.preventDefault();
      event.stopPropagation();

      try {
        matchingShortcut.action();
      } catch (error) {
        console.error('Error executing keyboard shortcut:', error);
        message.error('Failed to execute shortcut');
      }
    }
  }, [shortcuts]);

  // Set up event listeners
  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);

  // Focus management
  const focusElement = useCallback((elementId: string) => {
    const element = document.getElementById(elementId);
    if (element) {
      element.focus();
      setFocusedElement(elementId);
    }
  }, []);

  const blurElement = useCallback(() => {
    const activeElement = document.activeElement as HTMLElement;
    if (activeElement && activeElement.blur) {
      activeElement.blur();
    }
    setFocusedElement(null);
  }, []);

  // Announce actions for screen readers
  const announceAction = useCallback((message: string) => {
    const announcement = document.createElement('div');
    announcement.setAttribute('aria-live', 'polite');
    announcement.setAttribute('aria-atomic', 'true');
    announcement.className = 'sr-only';
    announcement.textContent = message;

    document.body.appendChild(announcement);

    setTimeout(() => {
      document.body.removeChild(announcement);
    }, 1000);
  }, []);

  // Get shortcuts by category
  const getShortcutsByCategory = useCallback(() => {
    const categories: Record<string, KeyboardShortcut[]> = {};

    shortcuts.forEach(shortcut => {
      if (!categories[shortcut.category]) {
        categories[shortcut.category] = [];
      }
      categories[shortcut.category].push(shortcut);
    });

    return categories;
  }, [shortcuts]);

  // Format shortcut key combination
  const formatShortcut = useCallback((shortcut: KeyboardShortcut): string => {
    const parts: string[] = [];

    if (shortcut.ctrlKey) parts.push('Ctrl');
    if (shortcut.shiftKey) parts.push('Shift');
    if (shortcut.altKey) parts.push('Alt');
    if (shortcut.metaKey) parts.push('Cmd');

    parts.push(shortcut.key === ' ' ? 'Space' : shortcut.key);

    return parts.join(' + ');
  }, []);

  // Add custom shortcut
  const addShortcut = useCallback((shortcut: KeyboardShortcut) => {
    setShortcuts(prev => [...prev, shortcut]);
  }, []);

  // Remove shortcut
  const removeShortcut = useCallback((key: string, modifiers: Partial<Pick<KeyboardShortcut, 'ctrlKey' | 'shiftKey' | 'altKey' | 'metaKey'>> = {}) => {
    setShortcuts(prev => prev.filter(shortcut =>
      !(shortcut.key === key &&
        !!shortcut.ctrlKey === !!modifiers.ctrlKey &&
        !!shortcut.shiftKey === !!modifiers.shiftKey &&
        !!shortcut.altKey === !!modifiers.altKey &&
        !!shortcut.metaKey === !!modifiers.metaKey)
    ));
  }, []);

  // Navigation helpers
  const navigateToNextElement = useCallback(() => {
    const focusableElements = document.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );

    const currentIndex = Array.from(focusableElements).findIndex(
      el => el === document.activeElement
    );

    const nextIndex = (currentIndex + 1) % focusableElements.length;
    (focusableElements[nextIndex] as HTMLElement).focus();
  }, []);

  const navigateToPreviousElement = useCallback(() => {
    const focusableElements = document.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );

    const currentIndex = Array.from(focusableElements).findIndex(
      el => el === document.activeElement
    );

    const prevIndex = currentIndex <= 0 ? focusableElements.length - 1 : currentIndex - 1;
    (focusableElements[prevIndex] as HTMLElement).focus();
  }, []);

  return {
    // State
    isCommandPaletteOpen,
    setIsCommandPaletteOpen,
    focusedElement,
    shortcuts,

    // Actions
    focusElement,
    blurElement,
    announceAction,
    addShortcut,
    removeShortcut,
    navigateToNextElement,
    navigateToPreviousElement,

    // Utilities
    getShortcutsByCategory,
    formatShortcut
  };
};

// Screen reader announcer component
export const ScreenReaderAnnouncer = () => {
  const [announcement, setAnnouncement] = useState('');

  useEffect(() => {
    const handleAnnouncement = (event: CustomEvent) => {
      setAnnouncement(event.detail.message);

      // Clear announcement after a delay
      setTimeout(() => setAnnouncement(''), 1000);
    };

    window.addEventListener('announce' as any, handleAnnouncement);
    return () => window.removeEventListener('announce' as any, handleAnnouncement);
  }, []);

  return React.createElement('div', {
    role: 'status',
    'aria-live': 'polite',
    'aria-atomic': 'true',
    className: 'sr-only',
    style: {
      position: 'absolute',
      left: '-10000px',
      width: '1px',
      height: '1px',
      overflow: 'hidden'
    }
  }, announcement);
};

// Utility function to announce messages
export const announceToScreenReader = (message: string) => {
  const event = new CustomEvent('announce', {
    detail: { message }
  });
  window.dispatchEvent(event);
};
