import React, { useEffect, useState } from 'react';
import { Modal, Typography, List, Tag, Space, Button, Tooltip } from 'antd';
import {
  ControlOutlined,
  EyeOutlined
} from '@ant-design/icons';

const { Title, Text } = Typography;

interface KeyboardShortcut {
  key: string;
  description: string;
  category: 'navigation' | 'query' | 'results' | 'general';
}

interface AccessibilityFeaturesProps {
  onShortcutTriggered?: (action: string) => void;
}

export const AccessibilityFeatures: React.FC<AccessibilityFeaturesProps> = ({
  onShortcutTriggered
}) => {
  const [showShortcuts, setShowShortcuts] = useState(false);
  const [announcements, setAnnouncements] = useState<string[]>([]);

  const shortcuts: KeyboardShortcut[] = [
    // Navigation
    { key: 'Tab', description: 'Navigate between elements', category: 'navigation' },
    { key: 'Shift + Tab', description: 'Navigate backwards', category: 'navigation' },
    { key: 'Alt + 1', description: 'Focus query input', category: 'navigation' },
    { key: 'Alt + 2', description: 'Focus results area', category: 'navigation' },
    { key: 'Alt + 3', description: 'Focus insights panel', category: 'navigation' },
    
    // Query Actions
    { key: 'Ctrl + Enter', description: 'Execute query', category: 'query' },
    { key: 'Ctrl + K', description: 'Open query wizard', category: 'query' },
    { key: 'Ctrl + L', description: 'Clear query input', category: 'query' },
    { key: 'Ctrl + Z', description: 'Undo last query change', category: 'query' },
    { key: 'Ctrl + Y', description: 'Redo query change', category: 'query' },
    
    // Results
    { key: 'Ctrl + E', description: 'Export results', category: 'results' },
    { key: 'Ctrl + S', description: 'Save query', category: 'results' },
    { key: 'F11', description: 'Toggle fullscreen view', category: 'results' },
    { key: 'Ctrl + F', description: 'Search in results', category: 'results' },
    
    // General
    { key: 'Ctrl + ?', description: 'Show keyboard shortcuts', category: 'general' },
    { key: 'Escape', description: 'Close modals/cancel actions', category: 'general' },
    { key: 'Ctrl + H', description: 'Show help', category: 'general' }
  ];

  // Screen reader announcements
  const announce = (message: string) => {
    setAnnouncements(prev => [...prev, message]);
    
    // Create live region for screen readers
    const liveRegion = document.createElement('div');
    liveRegion.setAttribute('aria-live', 'polite');
    liveRegion.setAttribute('aria-atomic', 'true');
    liveRegion.style.position = 'absolute';
    liveRegion.style.left = '-10000px';
    liveRegion.style.width = '1px';
    liveRegion.style.height = '1px';
    liveRegion.style.overflow = 'hidden';
    liveRegion.textContent = message;
    
    document.body.appendChild(liveRegion);
    
    setTimeout(() => {
      document.body.removeChild(liveRegion);
    }, 1000);
  };

  // Keyboard event handler
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      const { key, ctrlKey, altKey, shiftKey } = event;
      
      // Prevent default for our custom shortcuts
      const shortcutKey = [
        ctrlKey && 'Ctrl',
        altKey && 'Alt', 
        shiftKey && 'Shift',
        key
      ].filter(Boolean).join(' + ');

      switch (shortcutKey) {
        case 'Ctrl + Enter':
          event.preventDefault();
          onShortcutTriggered?.('execute-query');
          announce('Executing query');
          break;
          
        case 'Ctrl + k':
        case 'Ctrl + K':
          event.preventDefault();
          onShortcutTriggered?.('open-wizard');
          announce('Opening query wizard');
          break;
          
        case 'Ctrl + l':
        case 'Ctrl + L':
          event.preventDefault();
          onShortcutTriggered?.('clear-query');
          announce('Query input cleared');
          break;
          
        case 'Alt + 1':
          event.preventDefault();
          const queryInput = document.querySelector('[data-testid="query-input"]') as HTMLElement;
          queryInput?.focus();
          announce('Query input focused');
          break;
          
        case 'Alt + 2':
          event.preventDefault();
          const resultsArea = document.querySelector('[data-testid="results-area"]') as HTMLElement;
          resultsArea?.focus();
          announce('Results area focused');
          break;
          
        case 'Alt + 3':
          event.preventDefault();
          const insightsPanel = document.querySelector('[data-testid="insights-panel"]') as HTMLElement;
          insightsPanel?.focus();
          announce('Insights panel focused');
          break;
          
        case 'Ctrl + e':
        case 'Ctrl + E':
          event.preventDefault();
          onShortcutTriggered?.('export-results');
          announce('Opening export options');
          break;
          
        case 'Ctrl + s':
        case 'Ctrl + S':
          event.preventDefault();
          onShortcutTriggered?.('save-query');
          announce('Saving query');
          break;
          
        case 'Ctrl + ?':
          event.preventDefault();
          setShowShortcuts(true);
          announce('Keyboard shortcuts dialog opened');
          break;
          
        case 'F11':
          event.preventDefault();
          onShortcutTriggered?.('toggle-fullscreen');
          announce('Toggling fullscreen view');
          break;
          
        case 'Escape':
          if (showShortcuts) {
            setShowShortcuts(false);
            announce('Keyboard shortcuts dialog closed');
          }
          break;
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onShortcutTriggered, showShortcuts]);

  // Focus management
  useEffect(() => {
    // Ensure proper focus indicators
    const style = document.createElement('style');
    style.textContent = `
      .accessibility-focus:focus {
        outline: 3px solid #3b82f6 !important;
        outline-offset: 2px !important;
        box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2) !important;
      }
      
      .accessibility-skip-link {
        position: absolute;
        top: -40px;
        left: 6px;
        background: #3b82f6;
        color: white;
        padding: 8px;
        text-decoration: none;
        border-radius: 4px;
        z-index: 1000;
        transition: top 0.3s;
      }
      
      .accessibility-skip-link:focus {
        top: 6px;
      }
    `;
    document.head.appendChild(style);

    return () => {
      document.head.removeChild(style);
    };
  }, []);

  const groupedShortcuts = shortcuts.reduce((acc, shortcut) => {
    if (!acc[shortcut.category]) {
      acc[shortcut.category] = [];
    }
    acc[shortcut.category].push(shortcut);
    return acc;
  }, {} as Record<string, KeyboardShortcut[]>);

  const categoryLabels = {
    navigation: 'Navigation',
    query: 'Query Actions',
    results: 'Results',
    general: 'General'
  };

  const categoryColors = {
    navigation: 'blue',
    query: 'green',
    results: 'orange',
    general: 'purple'
  };

  return (
    <>
      {/* Skip Links for Screen Readers */}
      <a href="#query-input" className="accessibility-skip-link">
        Skip to query input
      </a>
      <a href="#results-area" className="accessibility-skip-link">
        Skip to results
      </a>
      <a href="#insights-panel" className="accessibility-skip-link">
        Skip to insights
      </a>

      {/* Keyboard Shortcuts Help Button */}
      <Tooltip title="Keyboard shortcuts (Ctrl + ?)">
        <Button
          icon={<ControlOutlined />}
          onClick={() => setShowShortcuts(true)}
          style={{
            position: 'fixed',
            bottom: '24px',
            right: '24px',
            zIndex: 1000,
            borderRadius: '50%',
            width: '48px',
            height: '48px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
          }}
          aria-label="Show keyboard shortcuts"
        />
      </Tooltip>

      {/* Keyboard Shortcuts Modal */}
      <Modal
        title={
          <Space>
            <ControlOutlined />
            <span>Keyboard Shortcuts</span>
          </Space>
        }
        open={showShortcuts}
        onCancel={() => setShowShortcuts(false)}
        footer={[
          <Button key="close" onClick={() => setShowShortcuts(false)}>
            Close
          </Button>
        ]}
        width={700}
      >
        <div style={{ maxHeight: '60vh', overflowY: 'auto' }}>
          {Object.entries(groupedShortcuts).map(([category, categoryShortcuts]) => (
            <div key={category} style={{ marginBottom: '24px' }}>
              <Title level={4} style={{ marginBottom: '12px' }}>
                <Tag color={categoryColors[category as keyof typeof categoryColors]}>
                  {categoryLabels[category as keyof typeof categoryLabels]}
                </Tag>
              </Title>
              <List
                size="small"
                dataSource={categoryShortcuts}
                renderItem={(shortcut) => (
                  <List.Item style={{ padding: '8px 0' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', width: '100%' }}>
                      <Text>{shortcut.description}</Text>
                      <Tag
                        style={{
                          fontFamily: 'monospace',
                          fontSize: '12px',
                          padding: '2px 8px'
                        }}
                      >
                        {shortcut.key}
                      </Tag>
                    </div>
                  </List.Item>
                )}
              />
            </div>
          ))}
        </div>
        
        <div style={{ marginTop: '16px', padding: '12px', background: '#f6f8fa', borderRadius: '8px' }}>
          <Space>
            <EyeOutlined />
            <Text strong>Accessibility Features:</Text>
          </Space>
          <ul style={{ marginTop: '8px', marginBottom: 0 }}>
            <li>Screen reader support with live announcements</li>
            <li>High contrast mode compatibility</li>
            <li>Keyboard-only navigation</li>
            <li>Focus indicators for all interactive elements</li>
            <li>Skip links for quick navigation</li>
          </ul>
        </div>
      </Modal>

      {/* Live Region for Announcements (Hidden) */}
      <div
        aria-live="polite"
        aria-atomic="true"
        style={{
          position: 'absolute',
          left: '-10000px',
          width: '1px',
          height: '1px',
          overflow: 'hidden'
        }}
      >
        {announcements[announcements.length - 1]}
      </div>
    </>
  );
};

// Hook for managing focus
export const useFocusManagement = () => {
  const [focusedElement, setFocusedElement] = useState<string | null>(null);

  const focusElement = (selector: string) => {
    const element = document.querySelector(selector) as HTMLElement;
    if (element) {
      element.focus();
      setFocusedElement(selector);
    }
  };

  const trapFocus = (containerSelector: string) => {
    const container = document.querySelector(containerSelector) as HTMLElement;
    if (!container) return;

    const focusableElements = container.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    
    const firstElement = focusableElements[0] as HTMLElement;
    const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Tab') {
        if (e.shiftKey) {
          if (document.activeElement === firstElement) {
            e.preventDefault();
            lastElement.focus();
          }
        } else {
          if (document.activeElement === lastElement) {
            e.preventDefault();
            firstElement.focus();
          }
        }
      }
    };

    container.addEventListener('keydown', handleKeyDown);
    firstElement?.focus();

    return () => {
      container.removeEventListener('keydown', handleKeyDown);
    };
  };

  return {
    focusedElement,
    focusElement,
    trapFocus
  };
};

// ARIA Live Region Component
export const LiveRegion: React.FC<{
  message: string;
  priority?: 'polite' | 'assertive';
}> = ({ message, priority = 'polite' }) => {
  return (
    <div
      aria-live={priority}
      aria-atomic="true"
      style={{
        position: 'absolute',
        left: '-10000px',
        width: '1px',
        height: '1px',
        overflow: 'hidden'
      }}
    >
      {message}
    </div>
  );
};
