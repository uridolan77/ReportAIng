import React, { useRef, useEffect, useState } from 'react'
import { Card, Button, Space, Tooltip, message } from 'antd'
import { 
  PlayCircleOutlined, 
  CopyOutlined, 
  FormatPainterOutlined,
  FullscreenOutlined,
  CompressOutlined
} from '@ant-design/icons'

// Monaco Editor types (would be installed via npm install monaco-editor)
interface MonacoEditor {
  getValue(): string
  setValue(value: string): void
  focus(): void
  getAction(id: string): any
  trigger(source: string, handlerId: string, payload?: any): void
}

interface MonacoSQLEditorProps {
  value?: string
  onChange?: (value: string) => void
  onExecute?: (sql: string) => void
  height?: number
  readOnly?: boolean
  theme?: 'vs-dark' | 'vs-light'
  showToolbar?: boolean
  placeholder?: string
}

export const MonacoSQLEditor: React.FC<MonacoSQLEditorProps> = ({
  value = '',
  onChange,
  onExecute,
  height = 300,
  readOnly = false,
  theme = 'vs-dark',
  showToolbar = true,
  placeholder = 'Enter your SQL query here...'
}) => {
  const editorRef = useRef<HTMLDivElement>(null)
  const monacoRef = useRef<MonacoEditor | null>(null)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    // Initialize Monaco Editor
    const initializeEditor = async () => {
      try {
        // In a real implementation, you would import monaco-editor
        // import * as monaco from 'monaco-editor'
        
        // For now, we'll simulate the Monaco Editor interface
        // This would be replaced with actual Monaco Editor initialization
        
        const mockEditor = {
          getValue: () => value,
          setValue: (newValue: string) => {
            onChange?.(newValue)
          },
          focus: () => {},
          getAction: (id: string) => ({ run: () => {} }),
          trigger: (source: string, handlerId: string, payload?: any) => {}
        }

        monacoRef.current = mockEditor
        setIsLoading(false)

        // Configure SQL language support
        // monaco.languages.registerCompletionItemProvider('sql', {
        //   provideCompletionItems: () => ({
        //     suggestions: getSQLCompletions()
        //   })
        // })

      } catch (error) {
        console.error('Failed to initialize Monaco Editor:', error)
        setIsLoading(false)
      }
    }

    if (editorRef.current) {
      initializeEditor()
    }

    return () => {
      // Cleanup Monaco Editor
      monacoRef.current = null
    }
  }, [])

  const handleExecute = () => {
    if (monacoRef.current && onExecute) {
      const sql = monacoRef.current.getValue()
      if (sql.trim()) {
        onExecute(sql)
      } else {
        message.warning('Please enter a SQL query')
      }
    }
  }

  const handleCopy = async () => {
    if (monacoRef.current) {
      const sql = monacoRef.current.getValue()
      try {
        await navigator.clipboard.writeText(sql)
        message.success('SQL copied to clipboard')
      } catch (error) {
        message.error('Failed to copy SQL')
      }
    }
  }

  const handleFormat = () => {
    if (monacoRef.current) {
      // Trigger Monaco's format action
      monacoRef.current.trigger('editor', 'editor.action.formatDocument')
      message.success('SQL formatted')
    }
  }

  const toggleFullscreen = () => {
    setIsFullscreen(!isFullscreen)
  }

  const getSQLCompletions = () => {
    // SQL keyword completions
    const keywords = [
      'SELECT', 'FROM', 'WHERE', 'JOIN', 'INNER JOIN', 'LEFT JOIN', 'RIGHT JOIN',
      'GROUP BY', 'ORDER BY', 'HAVING', 'INSERT', 'UPDATE', 'DELETE', 'CREATE',
      'ALTER', 'DROP', 'INDEX', 'TABLE', 'DATABASE', 'DISTINCT', 'COUNT', 'SUM',
      'AVG', 'MIN', 'MAX', 'AS', 'AND', 'OR', 'NOT', 'NULL', 'IS', 'LIKE',
      'BETWEEN', 'IN', 'EXISTS', 'CASE', 'WHEN', 'THEN', 'ELSE', 'END'
    ]

    return keywords.map(keyword => ({
      label: keyword,
      kind: 14, // monaco.languages.CompletionItemKind.Keyword
      insertText: keyword,
      documentation: `SQL keyword: ${keyword}`
    }))
  }

  const editorStyle = {
    height: isFullscreen ? '80vh' : height,
    width: '100%',
    border: '1px solid #d9d9d9',
    borderRadius: '6px',
    position: isFullscreen ? 'fixed' as const : 'relative' as const,
    top: isFullscreen ? '10vh' : 'auto',
    left: isFullscreen ? '10vw' : 'auto',
    right: isFullscreen ? '10vw' : 'auto',
    bottom: isFullscreen ? '10vh' : 'auto',
    zIndex: isFullscreen ? 1000 : 'auto',
    backgroundColor: theme === 'vs-dark' ? '#1e1e1e' : '#ffffff'
  }

  return (
    <Card 
      size="small"
      style={{ 
        position: 'relative',
        ...(isFullscreen && {
          position: 'fixed',
          top: '5vh',
          left: '5vw',
          right: '5vw',
          bottom: '5vh',
          zIndex: 1001,
          backgroundColor: 'white'
        })
      }}
    >
      {showToolbar && (
        <div style={{ 
          marginBottom: '8px', 
          display: 'flex', 
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <Space>
            <Tooltip title="Execute SQL (Ctrl+Enter)">
              <Button 
                type="primary" 
                icon={<PlayCircleOutlined />}
                onClick={handleExecute}
                disabled={readOnly}
                size="small"
              >
                Execute
              </Button>
            </Tooltip>
            <Tooltip title="Copy SQL">
              <Button 
                icon={<CopyOutlined />}
                onClick={handleCopy}
                size="small"
              >
                Copy
              </Button>
            </Tooltip>
            <Tooltip title="Format SQL">
              <Button 
                icon={<FormatPainterOutlined />}
                onClick={handleFormat}
                disabled={readOnly}
                size="small"
              >
                Format
              </Button>
            </Tooltip>
          </Space>
          
          <Tooltip title={isFullscreen ? "Exit Fullscreen" : "Fullscreen"}>
            <Button 
              icon={isFullscreen ? <CompressOutlined /> : <FullscreenOutlined />}
              onClick={toggleFullscreen}
              size="small"
            />
          </Tooltip>
        </div>
      )}

      <div 
        ref={editorRef}
        style={editorStyle}
      >
        {isLoading ? (
          <div style={{ 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            height: '100%',
            color: '#666'
          }}>
            Loading Monaco Editor...
          </div>
        ) : (
          <div style={{ 
            height: '100%', 
            width: '100%',
            fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
            fontSize: '14px',
            lineHeight: '1.5',
            padding: '12px',
            backgroundColor: theme === 'vs-dark' ? '#1e1e1e' : '#ffffff',
            color: theme === 'vs-dark' ? '#d4d4d4' : '#000000',
            border: 'none',
            outline: 'none',
            resize: 'none',
            overflow: 'auto'
          }}>
            {/* Fallback textarea for when Monaco isn't available */}
            <textarea
              value={value}
              onChange={(e) => onChange?.(e.target.value)}
              placeholder={placeholder}
              readOnly={readOnly}
              style={{
                width: '100%',
                height: '100%',
                border: 'none',
                outline: 'none',
                resize: 'none',
                backgroundColor: 'transparent',
                color: 'inherit',
                fontFamily: 'inherit',
                fontSize: 'inherit',
                lineHeight: 'inherit'
              }}
              onKeyDown={(e) => {
                if (e.ctrlKey && e.key === 'Enter') {
                  e.preventDefault()
                  handleExecute()
                }
              }}
            />
          </div>
        )}
      </div>

      {/* Fullscreen overlay */}
      {isFullscreen && (
        <div 
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            zIndex: 999
          }}
          onClick={toggleFullscreen}
        />
      )}
    </Card>
  )
}

// Hook for Monaco Editor integration
export const useMonacoSQL = () => {
  const [sql, setSql] = useState('')
  const [isValid, setIsValid] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  const validateSQL = (sqlText: string) => {
    // Basic SQL validation
    const trimmed = sqlText.trim()
    if (!trimmed) {
      setIsValid(true)
      setErrors([])
      return true
    }

    const errors: string[] = []
    
    // Check for basic SQL structure
    if (!trimmed.toLowerCase().match(/^(select|insert|update|delete|create|alter|drop)/)) {
      errors.push('SQL must start with a valid keyword (SELECT, INSERT, UPDATE, etc.)')
    }

    // Check for balanced parentheses
    const openParens = (trimmed.match(/\(/g) || []).length
    const closeParens = (trimmed.match(/\)/g) || []).length
    if (openParens !== closeParens) {
      errors.push('Unbalanced parentheses')
    }

    setErrors(errors)
    setIsValid(errors.length === 0)
    return errors.length === 0
  }

  const handleSQLChange = (newSQL: string) => {
    setSql(newSQL)
    validateSQL(newSQL)
  }

  return {
    sql,
    setSql: handleSQLChange,
    isValid,
    errors,
    validateSQL
  }
}
