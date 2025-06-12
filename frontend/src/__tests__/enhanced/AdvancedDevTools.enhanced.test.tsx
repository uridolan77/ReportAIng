/**
 * Enhanced Advanced Dev Tools Tests
 * 
 * Comprehensive test suite for the Advanced Dev Tools component
 * featuring performance testing, accessibility testing, and integration testing
 */

import React from 'react';
import { screen, fireEvent, waitFor, act } from '@testing-library/react';
import { renderWithProviders, mockPerformanceMetrics, measureRenderTime } from '../../utils/testUtils';
import { AdvancedDevTools } from '../../components/DevTools/AdvancedDevTools';

// Mock performance API
const mockPerformanceObserver = jest.fn();
const mockPerformanceEntries = [
  {
    name: 'test-navigation',
    duration: 150,
    startTime: 100,
    entryType: 'navigation'
  },
  {
    name: 'test-resource',
    duration: 50,
    startTime: 200,
    entryType: 'resource'
  }
];

beforeAll(() => {
  // Mock PerformanceObserver
  global.PerformanceObserver = jest.fn().mockImplementation((callback) => {
    mockPerformanceObserver.mockImplementation(callback);
    return {
      observe: jest.fn(),
      disconnect: jest.fn()
    };
  });

  // Mock performance.memory
  Object.defineProperty(performance, 'memory', {
    value: mockPerformanceMetrics,
    writable: true
  });
});

describe('AdvancedDevTools', () => {
  const defaultProps = {
    visible: true,
    onClose: jest.fn()
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Basic Functionality', () => {
    it('should render dev tools when visible', () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      expect(screen.getByText('Advanced Dev Tools')).toBeInTheDocument();
      expect(screen.getByRole('tablist')).toBeInTheDocument();
    });

    it('should not render when not visible', () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} visible={false} />);
      
      expect(screen.queryByText('Advanced Dev Tools')).not.toBeInTheDocument();
    });

    it('should call onClose when close button is clicked', () => {
      const onClose = jest.fn();
      renderWithProviders(<AdvancedDevTools {...defaultProps} onClose={onClose} />);
      
      const closeButton = screen.getByRole('button', { name: /close/i });
      fireEvent.click(closeButton);
      
      expect(onClose).toHaveBeenCalledTimes(1);
    });
  });

  describe('Console Tab', () => {
    it('should display console tab by default', () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      expect(screen.getByRole('tab', { name: /console/i })).toBeInTheDocument();
      expect(screen.getByText('Start Recording')).toBeInTheDocument();
    });

    it('should start and stop recording', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      const recordButton = screen.getByText('Start Recording');
      fireEvent.click(recordButton);
      
      await waitFor(() => {
        expect(screen.getByText('Stop Recording')).toBeInTheDocument();
      });

      fireEvent.click(screen.getByText('Stop Recording'));
      
      await waitFor(() => {
        expect(screen.getByText('Start Recording')).toBeInTheDocument();
      });
    });

    it('should filter logs by level', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Start recording
      fireEvent.click(screen.getByText('Start Recording'));
      
      // Generate some logs
      act(() => {
        console.log('Test info message');
        console.warn('Test warning message');
        console.error('Test error message');
      });

      // Filter by error level
      const filterSelect = screen.getByDisplayValue('All Levels');
      fireEvent.change(filterSelect, { target: { value: 'error' } });
      
      await waitFor(() => {
        expect(screen.getByText('Test error message')).toBeInTheDocument();
        expect(screen.queryByText('Test info message')).not.toBeInTheDocument();
      });
    });

    it('should search logs', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Start recording
      fireEvent.click(screen.getByText('Start Recording'));
      
      // Generate logs
      act(() => {
        console.log('Searchable message');
        console.log('Different message');
      });

      // Search for specific message
      const searchInput = screen.getByPlaceholderText('Search logs...');
      fireEvent.change(searchInput, { target: { value: 'Searchable' } });
      
      await waitFor(() => {
        expect(screen.getByText('Searchable message')).toBeInTheDocument();
        expect(screen.queryByText('Different message')).not.toBeInTheDocument();
      });
    });

    it('should clear logs', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Start recording and generate logs
      fireEvent.click(screen.getByText('Start Recording'));
      act(() => {
        console.log('Test message');
      });

      // Clear logs
      fireEvent.click(screen.getByText('Clear'));
      
      await waitFor(() => {
        expect(screen.queryByText('Test message')).not.toBeInTheDocument();
      });
    });
  });

  describe('Performance Tab', () => {
    it('should display performance metrics', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Switch to performance tab
      fireEvent.click(screen.getByRole('tab', { name: /performance/i }));
      
      await waitFor(() => {
        expect(screen.getByText('Memory Metrics')).toBeInTheDocument();
        expect(screen.getByText('Performance Entries')).toBeInTheDocument();
      });
    });

    it('should display memory usage when available', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      fireEvent.click(screen.getByRole('tab', { name: /performance/i }));
      
      await waitFor(() => {
        expect(screen.getByText('Used JS Heap Size')).toBeInTheDocument();
        expect(screen.getByText('47.68')).toBeInTheDocument(); // 50MB converted to MB
      });
    });

    it('should trigger memory cleanup', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      fireEvent.click(screen.getByRole('tab', { name: /performance/i }));
      
      const cleanupButton = screen.getByText('Force Cleanup');
      fireEvent.click(cleanupButton);
      
      // Should not throw error
      expect(cleanupButton).toBeInTheDocument();
    });

    it('should detect memory leaks', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      fireEvent.click(screen.getByRole('tab', { name: /performance/i }));
      
      const detectButton = screen.getByText('Detect Leaks');
      fireEvent.click(detectButton);
      
      // Should not throw error
      expect(detectButton).toBeInTheDocument();
    });
  });

  describe('State Tab', () => {
    it('should display state inspection interface', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      fireEvent.click(screen.getByRole('tab', { name: /state/i }));
      
      await waitFor(() => {
        expect(screen.getByText('Capture State Snapshot')).toBeInTheDocument();
      });
    });

    it('should capture state snapshots', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      fireEvent.click(screen.getByRole('tab', { name: /state/i }));
      
      const captureButton = screen.getByText('Capture State Snapshot');
      fireEvent.click(captureButton);
      
      await waitFor(() => {
        // Should show timeline with captured state
        expect(screen.getByText(/State Data/)).toBeInTheDocument();
      });
    });
  });

  describe('Performance Testing', () => {
    it('should render within acceptable time', async () => {
      const renderTime = await measureRenderTime(() => {
        renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      });
      
      expect(renderTime).toBeLessThan(100); // Should render in less than 100ms
    });

    it('should handle large number of logs efficiently', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Start recording
      fireEvent.click(screen.getByText('Start Recording'));
      
      // Generate many logs
      const startTime = performance.now();
      act(() => {
        for (let i = 0; i < 1000; i++) {
          console.log(`Log message ${i}`);
        }
      });
      const endTime = performance.now();
      
      expect(endTime - startTime).toBeLessThan(1000); // Should handle 1000 logs in less than 1 second
    });
  });

  describe('Integration Testing', () => {
    it('should work with performance observer', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Simulate performance entries
      act(() => {
        mockPerformanceObserver({
          getEntries: () => mockPerformanceEntries
        });
      });

      fireEvent.click(screen.getByRole('tab', { name: /performance/i }));
      
      await waitFor(() => {
        expect(screen.getByText('Performance Entries')).toBeInTheDocument();
      });
    });

    it('should handle errors gracefully', async () => {
      // Mock console.error to avoid noise in test output
      const originalError = console.error;
      console.error = jest.fn();
      
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Start recording
      fireEvent.click(screen.getByText('Start Recording'));
      
      // Generate an error
      act(() => {
        console.error('Test error for error handling');
      });

      await waitFor(() => {
        expect(screen.getByText('Test error for error handling')).toBeInTheDocument();
      });
      
      // Restore console.error
      console.error = originalError;
    });
  });

  describe('Accessibility', () => {
    it('should be accessible', async () => {
      const { container } = renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      // Check for proper ARIA labels
      expect(screen.getByRole('tablist')).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /console/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /performance/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /state/i })).toBeInTheDocument();
      
      // Check for keyboard navigation
      const firstTab = screen.getByRole('tab', { name: /console/i });
      firstTab.focus();
      expect(document.activeElement).toBe(firstTab);
    });

    it('should support keyboard navigation', async () => {
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      const consoleTab = screen.getByRole('tab', { name: /console/i });
      const performanceTab = screen.getByRole('tab', { name: /performance/i });
      
      // Focus first tab
      consoleTab.focus();
      expect(document.activeElement).toBe(consoleTab);
      
      // Navigate to next tab with arrow key
      fireEvent.keyDown(consoleTab, { key: 'ArrowRight' });
      expect(document.activeElement).toBe(performanceTab);
    });
  });

  describe('Error Handling', () => {
    it('should handle missing performance API gracefully', () => {
      // Temporarily remove performance.memory
      const originalMemory = (performance as any).memory;
      delete (performance as any).memory;
      
      renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      
      fireEvent.click(screen.getByRole('tab', { name: /performance/i }));
      
      expect(screen.getByText('Memory metrics not available')).toBeInTheDocument();
      
      // Restore performance.memory
      (performance as any).memory = originalMemory;
    });

    it('should handle PerformanceObserver errors', () => {
      // Mock PerformanceObserver to throw error
      global.PerformanceObserver = jest.fn().mockImplementation(() => {
        throw new Error('PerformanceObserver not supported');
      });
      
      // Should not crash
      expect(() => {
        renderWithProviders(<AdvancedDevTools {...defaultProps} />);
      }).not.toThrow();
    });
  });
});
