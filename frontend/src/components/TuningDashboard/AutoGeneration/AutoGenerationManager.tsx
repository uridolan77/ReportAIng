import React, { useState, useCallback } from 'react';
import { Card, Button, Space, Typography, Alert, Divider, Row, Col, Switch, InputNumber, message } from 'antd';
import { RobotOutlined, TableOutlined, BookOutlined, ShareAltOutlined, PlayCircleOutlined } from '@ant-design/icons';
import { tuningApi, AutoGenerationRequest, AutoGenerationResponse } from '../../../services/tuningApi';
import { AutoGenerationResults } from './AutoGenerationResults';
import { AutoGenerationProgress } from './AutoGenerationProgress';

const { Title, Text, Paragraph } = Typography;
// const { Option } = Select;

interface AutoGenerationManagerProps {
  onRefresh?: () => void;
}

export const AutoGenerationManager: React.FC<AutoGenerationManagerProps> = ({ onRefresh }) => {
  const [isGenerating, setIsGenerating] = useState(false);
  const [generationProgress, setGenerationProgress] = useState<number>(0);
  const [currentTask, setCurrentTask] = useState<string>('');
  const [results, setResults] = useState<AutoGenerationResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Generation options
  const [generateTableContexts, setGenerateTableContexts] = useState(true);
  const [generateGlossaryTerms, setGenerateGlossaryTerms] = useState(true);
  const [analyzeRelationships, setAnalyzeRelationships] = useState(true);
  const [overwriteExisting, setOverwriteExisting] = useState(false);
  const [confidenceThreshold, setConfidenceThreshold] = useState(0.6);

  const handleAutoGenerate = useCallback(async () => {
    if (!generateTableContexts && !generateGlossaryTerms && !analyzeRelationships) {
      message.warning('Please select at least one generation option');
      return;
    }

    setIsGenerating(true);
    setGenerationProgress(0);
    setCurrentTask('Initializing auto-generation...');
    setError(null);
    setResults(null);

    try {
      const request: AutoGenerationRequest = {
        generateTableContexts,
        generateGlossaryTerms,
        analyzeRelationships,
        overwriteExisting,
        minimumConfidenceThreshold: confidenceThreshold
      };

      // Simulate progress updates
      const progressInterval = setInterval(() => {
        setGenerationProgress(prev => {
          if (prev >= 90) {
            clearInterval(progressInterval);
            return prev;
          }
          return prev + Math.random() * 10;
        });
      }, 1000);

      // Update task descriptions
      setTimeout(() => setCurrentTask('Analyzing database schema...'), 500);
      setTimeout(() => setCurrentTask('Generating table business contexts...'), 2000);
      setTimeout(() => setCurrentTask('Creating business glossary terms...'), 4000);
      setTimeout(() => setCurrentTask('Analyzing table relationships...'), 6000);
      setTimeout(() => setCurrentTask('Finalizing results...'), 8000);

      const response = await tuningApi.autoGenerateBusinessContext(request);

      clearInterval(progressInterval);
      setGenerationProgress(100);
      setCurrentTask('Auto-generation completed successfully!');
      setResults(response);

      if (response.success) {
        message.success(`Auto-generation completed! Generated ${response.totalTablesProcessed} table contexts and ${response.totalTermsGenerated} glossary terms.`);
      } else {
        message.warning('Auto-generation completed with warnings. Please review the results.');
      }

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Auto-generation failed';
      setError(errorMessage);
      setCurrentTask('Auto-generation failed');
      message.error(errorMessage);
    } finally {
      setIsGenerating(false);
    }
  }, [generateTableContexts, generateGlossaryTerms, analyzeRelationships, overwriteExisting, confidenceThreshold]);

  const handleApplyResults = useCallback(async () => {
    if (!results) return;

    try {
      setIsGenerating(true);
      setCurrentTask('Applying auto-generated context...');

      await tuningApi.applyAutoGeneratedContext(results);

      message.success('Auto-generated context applied successfully!');
      setResults(null);

      if (onRefresh) {
        onRefresh();
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to apply auto-generated context';
      message.error(errorMessage);
    } finally {
      setIsGenerating(false);
      setCurrentTask('');
    }
  }, [results, onRefresh]);

  const handleDiscardResults = useCallback(() => {
    setResults(null);
    setError(null);
    setGenerationProgress(0);
    setCurrentTask('');
  }, []);

  return (
    <div style={{ padding: '24px' }}>
      <Card>
        <div style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <RobotOutlined style={{ marginRight: '8px', color: '#1890ff' }} />
            Auto-Generate Business Context
          </Title>
          <Paragraph type="secondary">
            Automatically analyze your database schema and generate intelligent business context,
            including table descriptions, business glossary terms, and relationship analysis.
            This feature uses AI to bootstrap your tuning process with smart defaults.
          </Paragraph>
        </div>

        {error && (
          <Alert
            message="Auto-Generation Error"
            description={typeof error === 'string' ? error : error?.message || 'An error occurred'}
            type="error"
            showIcon
            style={{ marginBottom: '24px' }}
            action={
              <Button size="small" onClick={() => setError(null)}>
                Dismiss
              </Button>
            }
          />
        )}

        {!results && !isGenerating && (
          <Card title="Generation Options" style={{ marginBottom: '24px' }}>
            <Row gutter={[16, 16]}>
              <Col span={24}>
                <Title level={5}>What to Generate</Title>
              </Col>

              <Col span={8}>
                <Card size="small" style={{ height: '100%' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Space>
                        <TableOutlined style={{ color: '#52c41a' }} />
                        <Text strong>Table Contexts</Text>
                      </Space>
                      <Switch
                        checked={generateTableContexts}
                        onChange={setGenerateTableContexts}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Generate business-friendly descriptions, use cases, and context for each database table
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={8}>
                <Card size="small" style={{ height: '100%' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Space>
                        <BookOutlined style={{ color: '#1890ff' }} />
                        <Text strong>Business Glossary</Text>
                      </Space>
                      <Switch
                        checked={generateGlossaryTerms}
                        onChange={setGenerateGlossaryTerms}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Create business glossary terms from column names and data patterns
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={8}>
                <Card size="small" style={{ height: '100%' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                      <Space>
                        <ShareAltOutlined style={{ color: '#722ed1' }} />
                        <Text strong>Relationships</Text>
                      </Space>
                      <Switch
                        checked={analyzeRelationships}
                        onChange={setAnalyzeRelationships}
                      />
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Analyze table relationships and identify business domains
                    </Text>
                  </Space>
                </Card>
              </Col>

              <Col span={24}>
                <Divider />
                <Title level={5}>Advanced Options</Title>
              </Col>

              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Confidence Threshold</Text>
                  <InputNumber
                    min={0.1}
                    max={1.0}
                    step={0.1}
                    value={confidenceThreshold}
                    onChange={(value) => setConfidenceThreshold(value || 0.6)}
                    style={{ width: '100%' }}
                    formatter={(value) => `${Math.round((value || 0) * 100)}%`}
                    parser={(value) => (parseFloat(value?.replace('%', '') || '60') / 100)}
                  />
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Minimum confidence score for generated content (higher = more selective)
                  </Text>
                </Space>
              </Col>

              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <Text strong>Overwrite Existing</Text>
                    <Switch
                      checked={overwriteExisting}
                      onChange={setOverwriteExisting}
                    />
                  </div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Replace existing business context with auto-generated content
                  </Text>
                </Space>
              </Col>
            </Row>

            <Divider />

            <div style={{ textAlign: 'center' }}>
              <Button
                type="primary"
                size="large"
                icon={<PlayCircleOutlined />}
                onClick={handleAutoGenerate}
                disabled={!generateTableContexts && !generateGlossaryTerms && !analyzeRelationships}
              >
                Start Auto-Generation
              </Button>
            </div>
          </Card>
        )}

        {isGenerating && (
          <AutoGenerationProgress
            progress={generationProgress}
            currentTask={currentTask}
          />
        )}

        {results && !isGenerating && (
          <AutoGenerationResults
            results={results}
            onApply={handleApplyResults}
            onDiscard={handleDiscardResults}
          />
        )}
      </Card>
    </div>
  );
};
