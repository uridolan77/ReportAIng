/**
 * Advanced Dev Tools Stories
 * 
 * Comprehensive Storybook stories for the Advanced Dev Tools component
 * showcasing debugging capabilities and developer experience features
 */

import type { Meta, StoryObj } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import { useState } from 'react';
import { Button, Space } from 'antd';
import { AdvancedDevTools } from '../components/DevTools/AdvancedDevTools';

const meta: Meta<typeof AdvancedDevTools> = {
  title: 'Developer Tools/Advanced Dev Tools',
  component: AdvancedDevTools,
  parameters: {
    layout: 'fullscreen',
    docs: {
      description: {
        component: 'Advanced development tools with console logging, performance monitoring, and state inspection capabilities.'
      }
    }
  },
  tags: ['autodocs'],
  argTypes: {
    visible: {
      control: 'boolean',
      description: 'Whether the dev tools drawer is visible'
    },
    onClose: {
      action: 'closed',
      description: 'Callback when dev tools are closed'
    }
  }
};

export default meta;
type Story = StoryObj<typeof meta>;

// Basic dev tools
export const Default: Story = {
  args: {
    visible: true,
    onClose: action('dev-tools-closed')
  }
};

// Interactive example with controls
export const Interactive: Story = {
  render: () => {
    const [visible, setVisible] = useState(false);
    
    const generateLogs = () => {
      console.log('Info: This is a sample log message');
      console.warn('Warning: This is a warning message');
      console.error('Error: This is an error message');
      console.debug('Debug: This is a debug message');
    };

    const generatePerformanceData = () => {
      performance.mark('test-start');
      setTimeout(() => {
        performance.mark('test-end');
        performance.measure('test-operation', 'test-start', 'test-end');
      }, 100);
    };

    return (
      <div style={{ padding: '20px' }}>
        <Space>
          <Button type="primary" onClick={() => setVisible(true)}>
            Open Dev Tools
          </Button>
          <Button onClick={generateLogs}>
            Generate Sample Logs
          </Button>
          <Button onClick={generatePerformanceData}>
            Generate Performance Data
          </Button>
        </Space>
        
        <AdvancedDevTools
          visible={visible}
          onClose={() => setVisible(false)}
        />
      </div>
    );
  }
};

// Console logging demonstration
export const ConsoleLogging: Story = {
  render: () => {
    const [visible, setVisible] = useState(true);
    
    const generateVariousLogs = () => {
      console.log('📊 Query executed successfully');
      console.log('User data:', { id: 1, name: 'John Doe', role: 'admin' });
      console.warn('⚠️ Cache miss for key: user_preferences');
      console.error('❌ Failed to connect to database');
      console.debug('🔍 Component rendered with props:', { visible: true });
      console.info('ℹ️ Application started in development mode');
    };

    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical">
          <h3>Console Logging Demo</h3>
          <p>Click the button below to generate various types of log messages:</p>
          <Button type="primary" onClick={generateVariousLogs}>
            Generate Sample Logs
          </Button>
        </Space>
        
        <AdvancedDevTools
          visible={visible}
          onClose={() => setVisible(false)}
        />
      </div>
    );
  }
};

// Performance monitoring demonstration
export const PerformanceMonitoring: Story = {
  render: () => {
    const [visible, setVisible] = useState(true);
    
    const simulateSlowOperation = () => {
      performance.mark('slow-operation-start');
      
      // Simulate a slow operation
      const start = Date.now();
      while (Date.now() - start < 200) {
        // Busy wait
      }
      
      performance.mark('slow-operation-end');
      performance.measure('slow-operation', 'slow-operation-start', 'slow-operation-end');
      console.log('Slow operation completed');
    };

    const simulateFastOperation = () => {
      performance.mark('fast-operation-start');
      
      setTimeout(() => {
        performance.mark('fast-operation-end');
        performance.measure('fast-operation', 'fast-operation-start', 'fast-operation-end');
        console.log('Fast operation completed');
      }, 10);
    };

    const simulateMemoryIntensiveOperation = () => {
      const largeArray = new Array(100000).fill(0).map((_, i) => ({ id: i, data: Math.random() }));
      console.log('Created large array with', largeArray.length, 'items');
      
      // Simulate processing
      const processed = largeArray.filter(item => item.data > 0.5);
      console.log('Processed array has', processed.length, 'items');
    };

    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical">
          <h3>Performance Monitoring Demo</h3>
          <p>Test different operations and monitor their performance:</p>
          <Space>
            <Button onClick={simulateSlowOperation}>
              Slow Operation (200ms)
            </Button>
            <Button onClick={simulateFastOperation}>
              Fast Operation (10ms)
            </Button>
            <Button onClick={simulateMemoryIntensiveOperation}>
              Memory Intensive Operation
            </Button>
          </Space>
        </Space>
        
        <AdvancedDevTools
          visible={visible}
          onClose={() => setVisible(false)}
        />
      </div>
    );
  }
};

// State inspection demonstration
export const StateInspection: Story = {
  render: () => {
    const [visible, setVisible] = useState(true);
    const [counter, setCounter] = useState(0);
    const [user, setUser] = useState({ name: 'John', role: 'admin' });
    
    const updateCounter = () => {
      setCounter(prev => prev + 1);
      console.log('Counter updated to:', counter + 1);
    };

    const updateUser = () => {
      const newUser = { 
        name: user.name === 'John' ? 'Jane' : 'John', 
        role: user.role === 'admin' ? 'user' : 'admin' 
      };
      setUser(newUser);
      console.log('User updated to:', newUser);
    };

    const resetState = () => {
      setCounter(0);
      setUser({ name: 'John', role: 'admin' });
      console.log('State reset');
    };

    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical">
          <h3>State Inspection Demo</h3>
          <p>Current State:</p>
          <ul>
            <li>Counter: {counter}</li>
            <li>User: {user.name} ({user.role})</li>
          </ul>
          <Space>
            <Button onClick={updateCounter}>
              Increment Counter
            </Button>
            <Button onClick={updateUser}>
              Toggle User
            </Button>
            <Button onClick={resetState}>
              Reset State
            </Button>
          </Space>
        </Space>
        
        <AdvancedDevTools
          visible={visible}
          onClose={() => setVisible(false)}
        />
      </div>
    );
  }
};

// Error handling demonstration
export const ErrorHandling: Story = {
  render: () => {
    const [visible, setVisible] = useState(true);
    
    const triggerError = () => {
      try {
        throw new Error('This is a sample error for testing');
      } catch (error) {
        console.error('Caught error:', error);
      }
    };

    const triggerWarning = () => {
      console.warn('This is a sample warning message');
    };

    const triggerNetworkError = () => {
      fetch('/api/nonexistent-endpoint')
        .catch(error => {
          console.error('Network error:', error);
        });
    };

    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical">
          <h3>Error Handling Demo</h3>
          <p>Test error logging and handling:</p>
          <Space>
            <Button danger onClick={triggerError}>
              Trigger Error
            </Button>
            <Button onClick={triggerWarning}>
              Trigger Warning
            </Button>
            <Button onClick={triggerNetworkError}>
              Trigger Network Error
            </Button>
          </Space>
        </Space>
        
        <AdvancedDevTools
          visible={visible}
          onClose={() => setVisible(false)}
        />
      </div>
    );
  }
};

// Development workflow demonstration
export const DevelopmentWorkflow: Story = {
  render: () => {
    const [visible, setVisible] = useState(true);
    
    const simulateQueryExecution = () => {
      console.log('🚀 Starting query execution...');
      performance.mark('query-start');
      
      setTimeout(() => {
        console.log('📊 Query executed successfully');
        console.log('Results:', { rows: 150, executionTime: '245ms' });
        performance.mark('query-end');
        performance.measure('query-execution', 'query-start', 'query-end');
      }, 250);
    };

    const simulateDataProcessing = () => {
      console.log('⚙️ Processing data...');
      performance.mark('processing-start');
      
      const data = Array.from({ length: 1000 }, (_, i) => ({ id: i, value: Math.random() }));
      console.log('Generated', data.length, 'data points');
      
      const filtered = data.filter(item => item.value > 0.5);
      console.log('Filtered to', filtered.length, 'items');
      
      performance.mark('processing-end');
      performance.measure('data-processing', 'processing-start', 'processing-end');
    };

    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical">
          <h3>Development Workflow Demo</h3>
          <p>Simulate real development scenarios:</p>
          <Space>
            <Button type="primary" onClick={simulateQueryExecution}>
              Execute Query
            </Button>
            <Button onClick={simulateDataProcessing}>
              Process Data
            </Button>
          </Space>
        </Space>
        
        <AdvancedDevTools
          visible={visible}
          onClose={() => setVisible(false)}
        />
      </div>
    );
  }
};
