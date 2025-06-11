/**
 * LLM Debug Page
 * 
 * Simple debug page to test LLM Management service and identify issues.
 */

import React, { useState } from 'react';
import { Card, Button, Space, Typography, Alert, Divider } from 'antd';
import { BugOutlined, PlayCircleOutlined } from '@ant-design/icons';
import { llmManagementService } from '../../services/llmManagementService';

const { Title, Text, Paragraph } = Typography;

const LLMDebugPage: React.FC = () => {
  const [results, setResults] = useState<any[]>([]);
  const [testing, setTesting] = useState(false);

  const runTest = async (testName: string, testFunction: () => Promise<any>) => {
    setTesting(true);
    const startTime = Date.now();
    
    try {
      const result = await testFunction();
      const endTime = Date.now();
      
      setResults(prev => [...prev, {
        name: testName,
        success: true,
        result,
        duration: endTime - startTime,
        timestamp: new Date().toISOString(),
      }]);
    } catch (error) {
      const endTime = Date.now();
      
      setResults(prev => [...prev, {
        name: testName,
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error',
        duration: endTime - startTime,
        timestamp: new Date().toISOString(),
      }]);
    } finally {
      setTesting(false);
    }
  };

  const runAllTests = async () => {
    setResults([]);
    
    await runTest('Dashboard Summary', () => llmManagementService.getDashboardSummary());
    await runTest('Get Providers', () => llmManagementService.getProviders());
    await runTest('Get Models', () => llmManagementService.getModels());
    await runTest('Provider Health', () => llmManagementService.getProviderHealth());
  };

  const clearResults = () => {
    setResults([]);
  };

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>
          <BugOutlined /> LLM Management Debug
        </Title>
        <Paragraph>
          This page tests the LLM Management service to identify any issues with the backend connection.
        </Paragraph>
      </div>

      <Card title="Service Tests" style={{ marginBottom: '24px' }}>
        <Space>
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={runAllTests}
            loading={testing}
          >
            Run All Tests
          </Button>
          
          <Button onClick={clearResults}>
            Clear Results
          </Button>
        </Space>
      </Card>

      {results.length > 0 && (
        <Card title="Test Results">
          <Space direction="vertical" style={{ width: '100%' }}>
            {results.map((result, index) => (
              <Card key={index} size="small" style={{ 
                background: result.success ? '#f6ffed' : '#fff2f0',
                border: `1px solid ${result.success ? '#b7eb8f' : '#ffccc7'}`
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div>
                    <Text strong style={{ color: result.success ? '#52c41a' : '#ff4d4f' }}>
                      {result.success ? '✅' : '❌'} {result.name}
                    </Text>
                    <div style={{ marginTop: '8px', fontSize: '12px', color: '#666' }}>
                      Duration: {result.duration}ms | {new Date(result.timestamp).toLocaleTimeString()}
                    </div>
                  </div>
                </div>
                
                <Divider style={{ margin: '12px 0' }} />
                
                {result.success ? (
                  <div>
                    <Text strong>Result:</Text>
                    <pre style={{ 
                      background: '#f5f5f5', 
                      padding: '8px', 
                      borderRadius: '4px',
                      fontSize: '11px',
                      maxHeight: '200px',
                      overflow: 'auto',
                      marginTop: '8px'
                    }}>
                      {JSON.stringify(result.result, null, 2)}
                    </pre>
                  </div>
                ) : (
                  <Alert
                    message="Error"
                    description={result.error}
                    type="error"
                    size="small"
                  />
                )}
              </Card>
            ))}
          </Space>
        </Card>
      )}

      {results.length === 0 && (
        <Card>
          <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
            No test results yet. Click "Run All Tests" to start testing the LLM Management service.
          </div>
        </Card>
      )}
    </div>
  );
};

export default LLMDebugPage;
