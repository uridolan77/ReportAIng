import React, { useState, useCallback, useRef } from 'react';
import { Button, Card, Progress, Typography, Space, Alert, Statistic, Row, Col, Switch, InputNumber } from 'antd';
import { PlayCircleOutlined, StopOutlined, DownloadOutlined } from '@ant-design/icons';
import { StreamingProgressUpdate, AdvancedStreamingRequest, StreamingQueryChunk } from '../../types/query';

const { Text } = Typography;

interface AdvancedStreamingQueryProps {
  onStreamingComplete?: (data: any[]) => void;
  onError?: (error: string) => void;
}

export const AdvancedStreamingQuery: React.FC<AdvancedStreamingQueryProps> = ({
  onStreamingComplete,
  onError
}) => {
  const [isStreaming, setIsStreaming] = useState(false);
  const [progress, setProgress] = useState<StreamingProgressUpdate | null>(null);
  const [streamingData, setStreamingData] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [question, setQuestion] = useState('');
  const [streamingConfig, setStreamingConfig] = useState({
    maxRows: 10000,
    timeoutSeconds: 300,
    chunkSize: 1000,
    enableProgressReporting: true,
    useBackpressure: true
  });

  const abortControllerRef = useRef<AbortController | null>(null);
  const accumulatedDataRef = useRef<any[]>([]);

  const startStreaming = useCallback(async () => {
    if (!question.trim()) {
      setError('Please enter a question');
      return;
    }

    setIsStreaming(true);
    setError(null);
    setProgress(null);
    setStreamingData([]);
    accumulatedDataRef.current = [];

    abortControllerRef.current = new AbortController();

    try {
      const endpoint = streamingConfig.useBackpressure
        ? '/api/streaming-query/stream-backpressure'
        : '/api/streaming-query/stream-progress';

      const request: AdvancedStreamingRequest = {
        question,
        maxRows: streamingConfig.maxRows,
        timeoutSeconds: streamingConfig.timeoutSeconds,
        chunkSize: streamingConfig.chunkSize,
        enableProgressReporting: streamingConfig.enableProgressReporting
      };

      const response = await fetch(`${process.env.REACT_APP_API_URL}${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(request),
        signal: abortControllerRef.current.signal
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const reader = response.body?.getReader();
      if (!reader) {
        throw new Error('No response body reader available');
      }

      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();

        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n');
        buffer = lines.pop() || '';

        for (const line of lines) {
          if (line.trim()) {
            try {
              if (streamingConfig.useBackpressure) {
                const chunk: StreamingQueryChunk = JSON.parse(line);
                accumulatedDataRef.current.push(...chunk.data);
                setStreamingData([...accumulatedDataRef.current]);

                // Update progress based on chunk info
                setProgress({
                  rowsProcessed: accumulatedDataRef.current.length,
                  estimatedTotalRows: 0, // Not available in chunk mode
                  progressPercentage: chunk.isLastChunk ? 100 : 0,
                  elapsedTime: `${chunk.processingTimeMs}ms`,
                  estimatedTimeRemaining: '0ms',
                  rowsPerSecond: 0,
                  status: chunk.isLastChunk ? 'Completed' : 'Processing',
                  isCompleted: chunk.isLastChunk,
                  currentRow: chunk.data[0]
                });

                if (chunk.isLastChunk) {
                  break;
                }
              } else {
                const progressUpdate: StreamingProgressUpdate = JSON.parse(line);
                setProgress(progressUpdate);

                if (progressUpdate.currentRow) {
                  accumulatedDataRef.current.push(progressUpdate.currentRow);
                  setStreamingData([...accumulatedDataRef.current]);
                }

                if (progressUpdate.isCompleted) {
                  break;
                }
              }
            } catch (parseError) {
              console.warn('Failed to parse streaming data:', parseError);
            }
          }
        }
      }

      onStreamingComplete?.(accumulatedDataRef.current);
    } catch (err: any) {
      if (err.name !== 'AbortError') {
        const errorMessage = err.message || 'Streaming failed';
        setError(errorMessage);
        onError?.(errorMessage);
      }
    } finally {
      setIsStreaming(false);
    }
  }, [question, streamingConfig, onStreamingComplete, onError]);

  const stopStreaming = useCallback(() => {
    abortControllerRef.current?.abort();
    setIsStreaming(false);
  }, []);

  const downloadData = useCallback(() => {
    if (accumulatedDataRef.current.length === 0) return;

    const csv = convertToCSV(accumulatedDataRef.current);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `streaming_query_results_${Date.now()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }, []);

  const convertToCSV = (data: any[]): string => {
    if (data.length === 0) return '';

    const headers = Object.keys(data[0]);
    const csvContent = [
      headers.join(','),
      ...data.map(row =>
        headers.map(header =>
          JSON.stringify(row[header] ?? '')
        ).join(',')
      )
    ].join('\n');

    return csvContent;
  };

  const formatTime = (timeString: string): string => {
    if (timeString.includes(':')) {
      return timeString; // Already formatted
    }
    const ms = parseInt(timeString.replace('ms', ''));
    if (ms < 1000) return `${ms}ms`;
    if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
    return `${(ms / 60000).toFixed(1)}m`;
  };

  return (
    <Card title="Advanced Streaming Query" style={{ margin: '16px 0' }}>
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* Configuration */}
        <Card size="small" title="Streaming Configuration">
          <Row gutter={16}>
            <Col span={6}>
              <Text>Max Rows:</Text>
              <InputNumber
                value={streamingConfig.maxRows}
                onChange={(value) => setStreamingConfig(prev => ({ ...prev, maxRows: value || 10000 }))}
                min={1000}
                max={100000}
                step={1000}
                style={{ width: '100%' }}
              />
            </Col>
            <Col span={6}>
              <Text>Chunk Size:</Text>
              <InputNumber
                value={streamingConfig.chunkSize}
                onChange={(value) => setStreamingConfig(prev => ({ ...prev, chunkSize: value || 1000 }))}
                min={100}
                max={5000}
                step={100}
                style={{ width: '100%' }}
              />
            </Col>
            <Col span={6}>
              <Text>Timeout (seconds):</Text>
              <InputNumber
                value={streamingConfig.timeoutSeconds}
                onChange={(value) => setStreamingConfig(prev => ({ ...prev, timeoutSeconds: value || 300 }))}
                min={30}
                max={600}
                step={30}
                style={{ width: '100%' }}
              />
            </Col>
            <Col span={6}>
              <Space direction="vertical">
                <Switch
                  checked={streamingConfig.useBackpressure}
                  onChange={(checked) => setStreamingConfig(prev => ({ ...prev, useBackpressure: checked }))}
                />
                <Text>Use Backpressure</Text>
              </Space>
            </Col>
          </Row>
        </Card>

        {/* Query Input */}
        <div>
          <Text>Question:</Text>
          <textarea
            value={question}
            onChange={(e) => setQuestion(e.target.value)}
            placeholder="Enter your question (e.g., 'Show me all customer data')"
            style={{ width: '100%', minHeight: '60px', marginTop: '8px' }}
            disabled={isStreaming}
          />
        </div>

        {/* Controls */}
        <Space>
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={startStreaming}
            disabled={isStreaming || !question.trim()}
            loading={isStreaming}
          >
            Start Streaming
          </Button>
          <Button
            icon={<StopOutlined />}
            onClick={stopStreaming}
            disabled={!isStreaming}
          >
            Stop
          </Button>
          <Button
            icon={<DownloadOutlined />}
            onClick={downloadData}
            disabled={streamingData.length === 0}
          >
            Download CSV
          </Button>
        </Space>

        {/* Progress Display */}
        {progress && (
          <Card size="small" title="Streaming Progress">
            <Row gutter={16}>
              <Col span={6}>
                <Statistic title="Rows Processed" value={progress.rowsProcessed} />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Progress"
                  value={progress.progressPercentage}
                  suffix="%"
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Speed"
                  value={progress.rowsPerSecond.toFixed(0)}
                  suffix="rows/sec"
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Elapsed Time"
                  value={formatTime(progress.elapsedTime)}
                />
              </Col>
            </Row>

            <Progress
              percent={Math.round(progress.progressPercentage)}
              status={progress.isCompleted ? 'success' : 'active'}
              style={{ marginTop: '16px' }}
            />

            <Text type="secondary">Status: {progress.status}</Text>
            {progress.estimatedTimeRemaining !== '0ms' && (
              <Text type="secondary" style={{ marginLeft: '16px' }}>
                ETA: {formatTime(progress.estimatedTimeRemaining)}
              </Text>
            )}
          </Card>
        )}

        {/* Error Display */}
        {error && (
          <Alert
            message="Streaming Error"
            description={error}
            type="error"
            closable
            onClose={() => setError(null)}
          />
        )}

        {/* Data Preview */}
        {streamingData.length > 0 && (
          <Card size="small" title={`Data Preview (${streamingData.length} rows)`}>
            <div style={{ maxHeight: '300px', overflow: 'auto' }}>
              <pre style={{ fontSize: '12px' }}>
                {JSON.stringify(streamingData.slice(0, 5), null, 2)}
              </pre>
              {streamingData.length > 5 && (
                <Text type="secondary">... and {streamingData.length - 5} more rows</Text>
              )}
            </div>
          </Card>
        )}
      </Space>
    </Card>
  );
};

export default AdvancedStreamingQuery;
