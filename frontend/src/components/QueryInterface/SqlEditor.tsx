import React, { useState, useRef, useEffect } from 'react';
import { Card, Button, Space, Typography, message, Tooltip, Alert } from 'antd';
import { PlayCircleOutlined, CopyOutlined, FormatPainterOutlined, ReloadOutlined } from '@ant-design/icons';
import { ApiService, SqlExecutionRequest } from '../../services/api';
import { FrontendQueryResponse } from '../../types/api';

const { Text } = Typography;

interface SqlEditorProps {
  initialSql: string;
  onExecute: (result: FrontendQueryResponse) => void;
  onClose?: () => void;
  sessionId?: string;
  disabled?: boolean;
}

export const SqlEditor: React.FC<SqlEditorProps> = ({
  initialSql,
  onExecute,
  onClose,
  sessionId,
  disabled = false
}) => {
  const [sql, setSql] = useState(initialSql);
  const [isExecuting, setIsExecuting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    setSql(initialSql);
  }, [initialSql]);

  const handleExecute = async () => {
    if (!sql.trim()) {
      message.warning('Please enter a SQL query');
      return;
    }

    setIsExecuting(true);
    setError(null);

    try {
      const request: SqlExecutionRequest = {
        sql: sql.trim(),
        sessionId: sessionId || Date.now().toString(),
        options: {
          maxRows: 1000,
          timeoutSeconds: 30
        }
      };

      if (process.env.NODE_ENV === 'development') {
        console.log('🔍 SQL Editor - Executing SQL:', request);
      }
      const result = await ApiService.executeRawSQL(request);
      if (process.env.NODE_ENV === 'development') {
        console.log('🔍 SQL Editor - Result:', result);
      }

      if (result.success) {
        message.success('SQL executed successfully');
        onExecute(result);
      } else {
        setError(result.error || 'SQL execution failed');
        message.error('SQL execution failed');
      }
    } catch (err: any) {
      const errorMessage = err.message || 'Failed to execute SQL';
      setError(errorMessage);
      message.error(errorMessage);
      console.error('SQL Editor - Execution error:', err);
    } finally {
      setIsExecuting(false);
    }
  };

  const handleCopy = () => {
    navigator.clipboard.writeText(sql);
    message.success('SQL copied to clipboard');
  };

  const handleFormat = () => {
    // Basic SQL formatting
    const formatted = sql
      .replace(/\s+/g, ' ')
      .replace(/,\s*/g, ',\n    ')
      .replace(/\bSELECT\b/gi, 'SELECT')
      .replace(/\bFROM\b/gi, '\nFROM')
      .replace(/\bWHERE\b/gi, '\nWHERE')
      .replace(/\bAND\b/gi, '\n  AND')
      .replace(/\bOR\b/gi, '\n  OR')
      .replace(/\bORDER BY\b/gi, '\nORDER BY')
      .replace(/\bGROUP BY\b/gi, '\nGROUP BY')
      .replace(/\bHAVING\b/gi, '\nHAVING')
      .replace(/\bJOIN\b/gi, '\nJOIN')
      .replace(/\bLEFT JOIN\b/gi, '\nLEFT JOIN')
      .replace(/\bRIGHT JOIN\b/gi, '\nRIGHT JOIN')
      .replace(/\bINNER JOIN\b/gi, '\nINNER JOIN')
      .trim();
    
    setSql(formatted);
    message.success('SQL formatted');
  };

  const handleReset = () => {
    setSql(initialSql);
    setError(null);
    message.info('SQL reset to original');
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.ctrlKey && e.key === 'Enter') {
      e.preventDefault();
      handleExecute();
    }
  };

  return (
    <Card
      title={
        <Space>
          <Text strong style={{ color: '#3b82f6' }}>SQL Editor</Text>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            Press Ctrl+Enter to execute
          </Text>
        </Space>
      }
      extra={
        <Space>
          <Tooltip title="Format SQL">
            <Button
              size="small"
              icon={<FormatPainterOutlined />}
              onClick={handleFormat}
              disabled={disabled || isExecuting}
            />
          </Tooltip>
          <Tooltip title="Copy SQL">
            <Button
              size="small"
              icon={<CopyOutlined />}
              onClick={handleCopy}
              disabled={disabled}
            />
          </Tooltip>
          <Tooltip title="Reset to original">
            <Button
              size="small"
              icon={<ReloadOutlined />}
              onClick={handleReset}
              disabled={disabled || isExecuting}
            />
          </Tooltip>
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={handleExecute}
            loading={isExecuting}
            disabled={disabled}
            style={{ backgroundColor: '#3b82f6', borderColor: '#3b82f6' }}
          >
            Execute
          </Button>
          {onClose && (
            <Button size="small" onClick={onClose} disabled={isExecuting}>
              Close
            </Button>
          )}
        </Space>
      }
      style={{
        marginBottom: '16px',
        borderRadius: '12px',
        border: '1px solid #e5e7eb',
        boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)'
      }}
      bodyStyle={{ padding: '16px' }}
    >
      {error && (
        <Alert
          message="SQL Execution Error"
          description={error}
          type="error"
          showIcon
          closable
          onClose={() => setError(null)}
          style={{ marginBottom: '16px' }}
        />
      )}
      
      <textarea
        ref={textareaRef}
        value={sql}
        onChange={(e) => setSql(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Enter your SQL query here..."
        disabled={disabled || isExecuting}
        style={{
          width: '100%',
          minHeight: '200px',
          padding: '12px',
          border: '1px solid #d1d5db',
          borderRadius: '8px',
          fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
          fontSize: '14px',
          lineHeight: '1.5',
          resize: 'vertical',
          backgroundColor: disabled || isExecuting ? '#f9fafb' : '#ffffff',
          color: '#374151'
        }}
      />
      
      <div style={{ marginTop: '8px', fontSize: '12px', color: '#6b7280' }}>
        <Text type="secondary">
          Only SELECT statements are allowed for security reasons. 
          Use Ctrl+Enter to execute or click the Execute button.
        </Text>
      </div>
    </Card>
  );
};

export default SqlEditor;
