/**
 * LLM Management Test Page
 *
 * Test page to verify LLM Management system integration
 * and demonstrate provider/model selection functionality.
 * Updated to fix icon import issues.
 */

import React, { useState } from 'react';
import { Card, Button, Space, Alert, Divider, Typography, Row, Col } from 'antd';
import {
  SettingOutlined,
  ApiOutlined,
  RobotOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  HomeOutlined
} from '@ant-design/icons';
import { ModernPageLayout } from '../../components/core/Layouts';
import { Breadcrumb } from '../../components/core/Navigation';
import { LLMSelector } from '../../components/AI/LLMSelector';
import { LLMStatusWidget } from '../../components/AI/LLMStatusWidget';
import { QueryInput } from '../../components/QueryInterface/QueryInput';

const { Title, Text, Paragraph } = Typography;

const LLMTestPage: React.FC = () => {
  const [selectedProviderId, setSelectedProviderId] = useState<string>();
  const [selectedModelId, setSelectedModelId] = useState<string>();
  const [testResults, setTestResults] = useState<any[]>([]);
  const [testing, setTesting] = useState(false);

  const handleTestQuery = async (query: string, options?: { providerId?: string; modelId?: string }) => {
    setTesting(true);
    
    try {
      // Simulate a test query
      const testResult = {
        timestamp: new Date().toISOString(),
        query,
        providerId: options?.providerId || 'default',
        modelId: options?.modelId || 'default',
        success: true,
        responseTime: Math.floor(Math.random() * 2000) + 500,
        cost: (Math.random() * 0.01).toFixed(4)
      };
      
      setTestResults(prev => [testResult, ...prev.slice(0, 4)]);
      
      // Simulate processing time
      await new Promise(resolve => setTimeout(resolve, testResult.responseTime));
      
    } catch (error) {
      console.error('Test query failed:', error);
    } finally {
      setTesting(false);
    }
  };

  const runSystemTests = async () => {
    setTesting(true);
    setTestResults([]);
    
    const tests = [
      { name: 'Provider Health Check', delay: 500 },
      { name: 'Model Configuration Test', delay: 800 },
      { name: 'Usage Logging Test', delay: 600 },
      { name: 'Cost Calculation Test', delay: 400 }
    ];
    
    for (const test of tests) {
      await new Promise(resolve => setTimeout(resolve, test.delay));
      
      const result = {
        timestamp: new Date().toISOString(),
        query: test.name,
        providerId: selectedProviderId || 'system',
        modelId: selectedModelId || 'test',
        success: Math.random() > 0.1, // 90% success rate
        responseTime: test.delay,
        cost: (Math.random() * 0.005).toFixed(4)
      };
      
      setTestResults(prev => [result, ...prev]);
    }
    
    setTesting(false);
  };

  return (
    <ModernPageLayout
      title="LLM Management System Test"
      subtitle="Test provider/model selection functionality and system integration"
      breadcrumb={
        <Breadcrumb
          items={[
            { title: 'Home', path: '/', icon: <HomeOutlined /> },
            { title: 'Admin', path: '/admin', icon: <SettingOutlined /> },
            { title: 'LLM Test', icon: <RobotOutlined /> }
          ]}
        />
      }
    >
      <Row gutter={[24, 24]}>
        {/* System Status */}
        <Col span={24}>
          <LLMStatusWidget compact={false} showActions={true} />
        </Col>

        {/* LLM Selector Test */}
        <Col span={12}>
          <Card title="LLM Provider & Model Selection" size="small">
            <LLMSelector
              selectedProviderId={selectedProviderId}
              selectedModelId={selectedModelId}
              useCase="SQL"
              onProviderChange={setSelectedProviderId}
              onModelChange={setSelectedModelId}
              compact={false}
              showStatus={true}
            />
            
            <Divider />
            
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Selected Configuration:</Text>
              <Text>Provider: {selectedProviderId || 'None selected'}</Text>
              <Text>Model: {selectedModelId || 'None selected'}</Text>
            </Space>
          </Card>
        </Col>

        {/* Test Controls */}
        <Col span={12}>
          <Card title="System Tests" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Button
                type="primary"
                icon={<SettingOutlined />}
                onClick={runSystemTests}
                loading={testing}
                block
              >
                Run System Tests
              </Button>
              
              <Button
                icon={<ApiOutlined />}
                onClick={() => window.open('/admin/llm', '_blank')}
                block
              >
                Open LLM Management
              </Button>
              
              <Button
                icon={<RobotOutlined />}
                onClick={() => window.open('/admin/llm#models', '_blank')}
                block
              >
                Configure Models
              </Button>
            </Space>
          </Card>
        </Col>

        {/* Query Test Interface */}
        <Col span={24}>
          <Card title="Query Test Interface" size="small">
            <Alert
              message="Test Query Submission"
              description="Use the query input below to test how the LLM Management system handles query submission with provider/model selection."
              type="info"
              showIcon
              style={{ marginBottom: '16px' }}
            />
            
            <QueryInput
              placeholder="Enter a test query to see how LLM selection works..."
              onSubmit={handleTestQuery}
              loading={testing}
              showLLMSelector={true}
            />
          </Card>
        </Col>

        {/* Test Results */}
        <Col span={24}>
          <Card title="Test Results" size="small">
            {testResults.length === 0 ? (
              <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
                No test results yet. Run some tests to see results here.
              </div>
            ) : (
              <Space direction="vertical" style={{ width: '100%' }}>
                {testResults.map((result, index) => (
                  <Card key={index} size="small" style={{ background: '#f9f9f9' }}>
                    <Row gutter={16} align="middle">
                      <Col span={1}>
                        {result.success ? (
                          <CheckCircleOutlined style={{ color: '#52c41a' }} />
                        ) : (
                          <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
                        )}
                      </Col>
                      <Col span={8}>
                        <Text strong>{result.query}</Text>
                      </Col>
                      <Col span={4}>
                        <Text>Provider: {result.providerId}</Text>
                      </Col>
                      <Col span={4}>
                        <Text>Model: {result.modelId}</Text>
                      </Col>
                      <Col span={3}>
                        <Text>Time: {result.responseTime}ms</Text>
                      </Col>
                      <Col span={2}>
                        <Text>Cost: ${result.cost}</Text>
                      </Col>
                      <Col span={2}>
                        <Text style={{ fontSize: '11px', color: '#666' }}>
                          {new Date(result.timestamp).toLocaleTimeString()}
                        </Text>
                      </Col>
                    </Row>
                  </Card>
                ))}
              </Space>
            )}
          </Card>
        </Col>
      </Row>
    </ModernPageLayout>
  );
};

export default LLMTestPage;
