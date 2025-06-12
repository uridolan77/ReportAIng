/**
 * Performance Optimization Stories
 * 
 * Comprehensive Storybook stories for Performance Optimization components
 * showcasing React 18 concurrent features, memory management, and bundle optimization
 */

import type { Meta, StoryObj } from '@storybook/react';
import { useState, Suspense } from 'react';
import { Button, Space, Card, Alert } from 'antd';
import { ConcurrentSuspense } from '../components/Performance/ConcurrentSuspense';
import { PerformanceOptimizationSummary } from '../components/Performance/PerformanceOptimizationSummary';

// Mock component for testing Suspense
const SlowComponent = ({ delay = 2000 }: { delay?: number }) => {
  const [data, setData] = useState<string | null>(null);
  
  if (!data) {
    // Simulate async data loading
    setTimeout(() => {
      setData(`Data loaded after ${delay}ms`);
    }, delay);
    throw new Promise(resolve => setTimeout(resolve, delay));
  }
  
  return (
    <Card>
      <h3>Slow Component Loaded!</h3>
      <p>{data}</p>
    </Card>
  );
};

const meta: Meta = {
  title: 'Performance/Optimization Components',
  parameters: {
    layout: 'fullscreen',
    docs: {
      description: {
        component: 'Advanced performance optimization components featuring React 18 concurrent features, memory management, and bundle optimization.'
      }
    }
  },
  tags: ['autodocs']
};

export default meta;

// Concurrent Suspense Stories
export const ConcurrentSuspenseBasic: StoryObj = {
  render: () => {
    const [showComponent, setShowComponent] = useState(false);
    
    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <h2>Concurrent Suspense Demo</h2>
          <p>Test React 18 concurrent features with enhanced Suspense boundaries:</p>
          
          <Button 
            type="primary" 
            onClick={() => setShowComponent(!showComponent)}
          >
            {showComponent ? 'Hide' : 'Load'} Slow Component
          </Button>
          
          {showComponent && (
            <ConcurrentSuspense
              enableProgressiveLoading={true}
              priority="normal"
              timeout={5000}
            >
              <SlowComponent delay={2000} />
            </ConcurrentSuspense>
          )}
        </Space>
      </div>
    );
  }
};

export const ConcurrentSuspenseWithRetry: StoryObj = {
  render: () => {
    const [key, setKey] = useState(0);
    const [showComponent, setShowComponent] = useState(false);
    
    const FailingComponent = () => {
      if (Math.random() > 0.5) {
        throw new Error('Random component failure');
      }
      return <Card><h3>Component loaded successfully!</h3></Card>;
    };
    
    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <h2>Concurrent Suspense with Error Recovery</h2>
          <p>Test error handling and retry functionality:</p>
          
          <Space>
            <Button 
              type="primary" 
              onClick={() => {
                setShowComponent(true);
                setKey(prev => prev + 1);
              }}
            >
              Load Component (50% chance of failure)
            </Button>
            <Button onClick={() => setShowComponent(false)}>
              Reset
            </Button>
          </Space>
          
          {showComponent && (
            <ConcurrentSuspense
              key={key}
              enableProgressiveLoading={true}
              priority="high"
              timeout={3000}
              errorFallback={
                <Alert
                  message="Component Failed to Load"
                  description="The component encountered an error. Try loading it again."
                  type="error"
                  showIcon
                />
              }
            >
              <FailingComponent />
            </ConcurrentSuspense>
          )}
        </Space>
      </div>
    );
  }
};

export const ConcurrentSuspensePriorities: StoryObj = {
  render: () => {
    const [components, setComponents] = useState<Array<{ id: number; priority: 'high' | 'normal' | 'low' }>>([]);
    
    const addComponent = (priority: 'high' | 'normal' | 'low') => {
      setComponents(prev => [...prev, { id: Date.now(), priority }]);
    };
    
    const clearComponents = () => {
      setComponents([]);
    };
    
    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <h2>Concurrent Suspense Priority Demo</h2>
          <p>Test different loading priorities with React 18 concurrent features:</p>
          
          <Space>
            <Button type="primary" onClick={() => addComponent('high')}>
              Add High Priority Component
            </Button>
            <Button onClick={() => addComponent('normal')}>
              Add Normal Priority Component
            </Button>
            <Button onClick={() => addComponent('low')}>
              Add Low Priority Component
            </Button>
            <Button danger onClick={clearComponents}>
              Clear All
            </Button>
          </Space>
          
          <div style={{ display: 'grid', gap: '16px', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))' }}>
            {components.map(({ id, priority }) => (
              <ConcurrentSuspense
                key={id}
                enableProgressiveLoading={true}
                priority={priority}
                timeout={5000}
              >
                <Card title={`${priority.toUpperCase()} Priority Component`}>
                  <SlowComponent delay={priority === 'high' ? 1000 : priority === 'normal' ? 2000 : 3000} />
                </Card>
              </ConcurrentSuspense>
            ))}
          </div>
        </Space>
      </div>
    );
  }
};

// Performance Optimization Summary
export const PerformanceOptimizationSummaryStory: StoryObj = {
  render: () => (
    <div style={{ minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
      <PerformanceOptimizationSummary />
    </div>
  )
};

// Memory Management Demo
export const MemoryManagementDemo: StoryObj = {
  render: () => {
    const [components, setComponents] = useState<number[]>([]);
    const [memoryData, setMemoryData] = useState<any>(null);
    
    const addComponent = () => {
      setComponents(prev => [...prev, Date.now()]);
    };
    
    const removeComponent = () => {
      setComponents(prev => prev.slice(0, -1));
    };
    
    const clearAll = () => {
      setComponents([]);
    };
    
    const checkMemory = () => {
      if ('memory' in performance) {
        const memory = (performance as any).memory;
        setMemoryData({
          usedJSHeapSize: (memory.usedJSHeapSize / 1024 / 1024).toFixed(2),
          totalJSHeapSize: (memory.totalJSHeapSize / 1024 / 1024).toFixed(2),
          jsHeapSizeLimit: (memory.jsHeapSizeLimit / 1024 / 1024).toFixed(2)
        });
      } else {
        setMemoryData({ error: 'Memory API not available' });
      }
    };
    
    const MemoryIntensiveComponent = ({ id }: { id: number }) => {
      // Simulate memory usage
      const data = new Array(10000).fill(0).map((_, i) => ({ id: i, value: Math.random() }));
      
      return (
        <Card size="small" title={`Component ${id}`}>
          <p>Memory intensive component with {data.length} data points</p>
          <p>Sample data: {data[0].value.toFixed(4)}</p>
        </Card>
      );
    };
    
    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <h2>Memory Management Demo</h2>
          <p>Test memory usage and cleanup with dynamic components:</p>
          
          <Space>
            <Button type="primary" onClick={addComponent}>
              Add Memory Intensive Component
            </Button>
            <Button onClick={removeComponent} disabled={components.length === 0}>
              Remove Last Component
            </Button>
            <Button danger onClick={clearAll} disabled={components.length === 0}>
              Clear All Components
            </Button>
            <Button onClick={checkMemory}>
              Check Memory Usage
            </Button>
          </Space>
          
          {memoryData && (
            <Card title="Memory Usage" size="small">
              {memoryData.error ? (
                <p>{memoryData.error}</p>
              ) : (
                <Space direction="vertical">
                  <p>Used JS Heap Size: {memoryData.usedJSHeapSize} MB</p>
                  <p>Total JS Heap Size: {memoryData.totalJSHeapSize} MB</p>
                  <p>JS Heap Size Limit: {memoryData.jsHeapSizeLimit} MB</p>
                </Space>
              )}
            </Card>
          )}
          
          <div style={{ display: 'grid', gap: '16px', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))' }}>
            {components.map(id => (
              <MemoryIntensiveComponent key={id} id={id} />
            ))}
          </div>
          
          {components.length === 0 && (
            <Alert
              message="No Components"
              description="Add some memory intensive components to test memory management."
              type="info"
              showIcon
            />
          )}
        </Space>
      </div>
    );
  }
};

// Bundle Optimization Demo
export const BundleOptimizationDemo: StoryObj = {
  render: () => {
    const [loadedChunks, setLoadedChunks] = useState<string[]>([]);
    
    const simulateChunkLoading = async (chunkName: string) => {
      console.log(`Loading chunk: ${chunkName}`);
      
      // Simulate chunk loading delay
      await new Promise(resolve => setTimeout(resolve, Math.random() * 1000 + 500));
      
      setLoadedChunks(prev => [...prev, chunkName]);
      console.log(`Chunk loaded: ${chunkName}`);
    };
    
    const clearChunks = () => {
      setLoadedChunks([]);
    };
    
    return (
      <div style={{ padding: '20px' }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <h2>Bundle Optimization Demo</h2>
          <p>Simulate intelligent code splitting and chunk loading:</p>
          
          <Space wrap>
            <Button onClick={() => simulateChunkLoading('dashboard-page')}>
              Load Dashboard Chunk
            </Button>
            <Button onClick={() => simulateChunkLoading('visualization-page')}>
              Load Visualization Chunk
            </Button>
            <Button onClick={() => simulateChunkLoading('admin-panel')}>
              Load Admin Panel Chunk
            </Button>
            <Button onClick={() => simulateChunkLoading('reports-module')}>
              Load Reports Module
            </Button>
            <Button danger onClick={clearChunks}>
              Clear Loaded Chunks
            </Button>
          </Space>
          
          <Card title="Loaded Chunks" size="small">
            {loadedChunks.length > 0 ? (
              <Space direction="vertical">
                {loadedChunks.map((chunk, index) => (
                  <div key={index} style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{ color: '#52c41a' }}>✓</span>
                    <span>{chunk}</span>
                  </div>
                ))}
              </Space>
            ) : (
              <p>No chunks loaded yet. Click the buttons above to simulate chunk loading.</p>
            )}
          </Card>
          
          <Alert
            message="Bundle Optimization Features"
            description={
              <ul style={{ margin: 0, paddingLeft: '20px' }}>
                <li>Intelligent code splitting based on user behavior</li>
                <li>Predictive preloading of likely-needed chunks</li>
                <li>Runtime bundle analysis and optimization</li>
                <li>Automatic cleanup of unused resources</li>
              </ul>
            }
            type="info"
            showIcon
          />
        </Space>
      </div>
    );
  }
};
