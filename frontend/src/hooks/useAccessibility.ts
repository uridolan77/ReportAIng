import { useEffect, useRef, useState } from 'react';

interface AccessibilityOptions {
  enableKeyboardNavigation?: boolean;
  enableScreenReaderSupport?: boolean;
  enableHighContrast?: boolean;
  ariaLabel?: string;
  ariaDescribedBy?: string;
}

export const useAccessibility = (options: AccessibilityOptions = {}) => {
  const [announcement, setAnnouncement] = useState<string>('');
  const announcerRef = useRef<HTMLDivElement>(null);

  // Screen reader announcements
  const announce = (message: string, priority: 'polite' | 'assertive' = 'polite') => {
    setAnnouncement(message);
    if (announcerRef.current) {
      announcerRef.current.setAttribute('aria-live', priority);
    }
  };

  // Keyboard navigation hook
  const useKeyboardNavigation = (
    containerRef: React.RefObject<HTMLElement>,
    handlers: Record<string, () => void>
  ) => {
    useEffect(() => {
      const handleKeyDown = (event: KeyboardEvent) => {
        const handler = handlers[event.key];
        if (handler) {
          event.preventDefault();
          handler();
        }
      };

      const container = containerRef.current;
      if (container) {
        container.addEventListener('keydown', handleKeyDown);
        return () => container.removeEventListener('keydown', handleKeyDown);
      }
    }, [containerRef, handlers]);
  };

  // High contrast mode
  useEffect(() => {
    if (options.enableHighContrast) {
      document.body.classList.add('high-contrast');
      return () => document.body.classList.remove('high-contrast');
    }
  }, [options.enableHighContrast]);

  // Focus management
  const useFocusManagement = () => {
    const focusableElements = 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])';
    
    const trapFocus = (container: HTMLElement) => {
      const focusableContent = container.querySelectorAll(focusableElements);
      const firstFocusableElement = focusableContent[0] as HTMLElement;
      const lastFocusableElement = focusableContent[focusableContent.length - 1] as HTMLElement;

      const handleTabKey = (e: KeyboardEvent) => {
        if (e.key === 'Tab') {
          if (e.shiftKey) {
            if (document.activeElement === firstFocusableElement) {
              lastFocusableElement.focus();
              e.preventDefault();
            }
          } else {
            if (document.activeElement === lastFocusableElement) {
              firstFocusableElement.focus();
              e.preventDefault();
            }
          }
        }
      };

      container.addEventListener('keydown', handleTabKey);
      return () => container.removeEventListener('keydown', handleTabKey);
    };

    const restoreFocus = (previousActiveElement: Element | null) => {
      if (previousActiveElement && 'focus' in previousActiveElement) {
        (previousActiveElement as HTMLElement).focus();
      }
    };

    return { trapFocus, restoreFocus };
  };

  // Skip links for screen readers
  const useSkipLinks = () => {
    const createSkipLink = (targetId: string, text: string) => {
      const skipLink = document.createElement('a');
      skipLink.href = `#${targetId}`;
      skipLink.textContent = text;
      skipLink.className = 'skip-link';
      skipLink.style.cssText = `
        position: absolute;
        top: -40px;
        left: 6px;
        background: #000;
        color: #fff;
        padding: 8px;
        text-decoration: none;
        z-index: 1000;
        transition: top 0.3s;
      `;
      
      skipLink.addEventListener('focus', () => {
        skipLink.style.top = '6px';
      });
      
      skipLink.addEventListener('blur', () => {
        skipLink.style.top = '-40px';
      });

      return skipLink;
    };

    return { createSkipLink };
  };

  // ARIA live region management
  const useAriaLiveRegion = () => {
    const createLiveRegion = (politeness: 'polite' | 'assertive' = 'polite') => {
      const liveRegion = document.createElement('div');
      liveRegion.setAttribute('aria-live', politeness);
      liveRegion.setAttribute('aria-atomic', 'true');
      liveRegion.style.cssText = `
        position: absolute;
        left: -10000px;
        width: 1px;
        height: 1px;
        overflow: hidden;
      `;
      document.body.appendChild(liveRegion);
      return liveRegion;
    };

    const announceToLiveRegion = (element: HTMLElement, message: string) => {
      element.textContent = message;
      // Clear after announcement to allow repeated announcements
      setTimeout(() => {
        element.textContent = '';
      }, 1000);
    };

    return { createLiveRegion, announceToLiveRegion };
  };

  return {
    announce,
    announcerRef,
    announcement,
    useKeyboardNavigation,
    useFocusManagement,
    useSkipLinks,
    useAriaLiveRegion
  };
};

// Utility function to generate chart descriptions for screen readers
export const generateChartDescription = (data: any[], config: any): string => {
  const dataPoints = data.length;
  const chartType = config.type || 'chart';
  const title = config.title || 'Untitled chart';
  
  let description = `${title}. This is a ${chartType} with ${dataPoints} data points.`;
  
  if (data.length > 0) {
    const firstPoint = data[0];
    const lastPoint = data[data.length - 1];
    
    if (typeof firstPoint.value === 'number' && typeof lastPoint.value === 'number') {
      const min = Math.min(...data.map(d => d.value));
      const max = Math.max(...data.map(d => d.value));
      description += ` Values range from ${min} to ${max}.`;
    }
  }
  
  return description;
};

// Utility function to generate data table descriptions
export const generateDataTableDescription = (data: any[]): string => {
  if (data.length === 0) return 'No data available.';
  
  const columns = Object.keys(data[0]);
  const rowCount = data.length;
  
  return `Data table with ${rowCount} rows and ${columns.length} columns: ${columns.join(', ')}.`;
};
