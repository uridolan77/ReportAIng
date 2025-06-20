import React, { useRef, useEffect, useState } from 'react'
import { Card, Button, Space, Tooltip, Typography } from 'antd'
import { PlayCircleOutlined, CopyOutlined, FormatPainterOutlined, FullscreenOutlined } from '@ant-design/icons'
import Editor, { Monaco } from '@monaco-editor/react'
import type { editor } from 'monaco-editor'

const { Text } = Typography

interface SqlEditorProps {
  value?: string
  onChange?: (value: string) => void
  onExecute?: (sql: string) => void
  height?: number
  readOnly?: boolean
  showExecuteButton?: boolean
  showFormatButton?: boolean
  showCopyButton?: boolean
  theme?: 'light' | 'dark'
  placeholder?: string
}

export const SqlEditor: React.FC<SqlEditorProps> = ({
  value = '',
  onChange,
  onExecute,
  height = 300,
  readOnly = false,
  showExecuteButton = true,
  showFormatButton = true,
  showCopyButton = true,
  theme = 'light',
  placeholder = 'Enter your SQL query here...',
}) => {
  const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null)
  const monacoRef = useRef<Monaco | null>(null)
  const [isFullscreen, setIsFullscreen] = useState(false)

  const handleEditorDidMount = (editor: editor.IStandaloneCodeEditor, monaco: Monaco) => {
    editorRef.current = editor
    monacoRef.current = monaco

    // Configure SQL language features
    monaco.languages.setLanguageConfiguration('sql', {
      comments: {
        lineComment: '--',
        blockComment: ['/*', '*/'],
      },
      brackets: [
        ['(', ')'],
        ['[', ']'],
      ],
      autoClosingPairs: [
        { open: '(', close: ')' },
        { open: '[', close: ']' },
        { open: "'", close: "'" },
        { open: '"', close: '"' },
      ],
      surroundingPairs: [
        { open: '(', close: ')' },
        { open: '[', close: ']' },
        { open: "'", close: "'" },
        { open: '"', close: '"' },
      ],
    })

    // Add SQL keywords and functions for better autocomplete
    monaco.languages.setMonarchTokensProvider('sql', {
      tokenizer: {
        root: [
          [/\b(?:SELECT|FROM|WHERE|JOIN|INNER|LEFT|RIGHT|FULL|OUTER|ON|GROUP|BY|ORDER|HAVING|UNION|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP|TABLE|INDEX|VIEW|DATABASE|SCHEMA)\b/i, 'keyword'],
          [/\b(?:COUNT|SUM|AVG|MIN|MAX|DISTINCT|AS|AND|OR|NOT|IN|LIKE|BETWEEN|IS|NULL|TRUE|FALSE)\b/i, 'keyword'],
          [/\b(?:INT|INTEGER|VARCHAR|CHAR|TEXT|DATE|DATETIME|TIMESTAMP|DECIMAL|FLOAT|DOUBLE|BOOLEAN)\b/i, 'type'],
          [/'([^'\\]|\\.)*'/, 'string'],
          [/"([^"\\]|\\.)*"/, 'string'],
          [/\d+/, 'number'],
          [/--.*$/, 'comment'],
          [/\/\*/, 'comment', '@comment'],
        ],
        comment: [
          [/[^\/*]+/, 'comment'],
          [/\*\//, 'comment', '@pop'],
          [/[\/*]/, 'comment'],
        ],
      },
    })

    // Add keyboard shortcuts
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, () => {
      handleExecute()
    })

    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.KeyF, () => {
      handleFormat()
    })

    // Set placeholder
    if (!value && placeholder) {
      editor.setValue(placeholder)
      editor.setSelection(new monaco.Selection(1, 1, 1, placeholder.length))
    }
  }

  const handleExecute = () => {
    const sql = editorRef.current?.getValue() || ''
    if (sql.trim() && onExecute) {
      onExecute(sql)
    }
  }

  const handleFormat = () => {
    if (editorRef.current && monacoRef.current) {
      editorRef.current.getAction('editor.action.formatDocument')?.run()
    }
  }

  const handleCopy = async () => {
    const sql = editorRef.current?.getValue() || ''
    try {
      await navigator.clipboard.writeText(sql)
      // You could add a notification here
    } catch (error) {
      console.error('Failed to copy SQL:', error)
    }
  }

  const toggleFullscreen = () => {
    setIsFullscreen(!isFullscreen)
  }

  const editorOptions: editor.IStandaloneEditorConstructionOptions = {
    minimap: { enabled: false },
    scrollBeyondLastLine: false,
    fontSize: 14,
    lineHeight: 20,
    readOnly,
    wordWrap: 'on',
    automaticLayout: true,
    suggestOnTriggerCharacters: true,
    quickSuggestions: true,
    tabSize: 2,
    insertSpaces: true,
    folding: true,
    lineNumbers: 'on',
    glyphMargin: false,
    contextmenu: true,
    mouseWheelZoom: true,
    smoothScrolling: true,
    cursorBlinking: 'blink',
    cursorSmoothCaretAnimation: true,
    renderLineHighlight: 'line',
    selectOnLineNumbers: true,
    roundedSelection: false,
    renderIndentGuides: true,
    colorDecorators: true,
    codeLens: false,
    matchBrackets: 'always',
    showFoldingControls: 'mouseover',
  }

  const containerStyle = isFullscreen
    ? {
        position: 'fixed' as const,
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        zIndex: 1000,
        backgroundColor: 'white',
      }
    : {}

  return (
    <div style={containerStyle}>
      <Card
        size="small"
        title={
          <Space>
            <Text strong>SQL Editor</Text>
            {!readOnly && (
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Ctrl+Enter to execute, Ctrl+Shift+F to format
              </Text>
            )}
          </Space>
        }
        extra={
          <Space>
            {showCopyButton && (
              <Tooltip title="Copy SQL">
                <Button size="small" icon={<CopyOutlined />} onClick={handleCopy} />
              </Tooltip>
            )}
            {showFormatButton && !readOnly && (
              <Tooltip title="Format SQL (Ctrl+Shift+F)">
                <Button size="small" icon={<FormatPainterOutlined />} onClick={handleFormat} />
              </Tooltip>
            )}
            {showExecuteButton && !readOnly && (
              <Tooltip title="Execute Query (Ctrl+Enter)">
                <Button
                  type="primary"
                  size="small"
                  icon={<PlayCircleOutlined />}
                  onClick={handleExecute}
                >
                  Execute
                </Button>
              </Tooltip>
            )}
            <Tooltip title={isFullscreen ? 'Exit Fullscreen' : 'Fullscreen'}>
              <Button size="small" icon={<FullscreenOutlined />} onClick={toggleFullscreen} />
            </Tooltip>
          </Space>
        }
        style={{ height: isFullscreen ? '100vh' : height + 60 }}
      >
        <Editor
          height={isFullscreen ? 'calc(100vh - 120px)' : height}
          defaultLanguage="sql"
          value={value}
          onChange={(newValue) => onChange?.(newValue || '')}
          onMount={handleEditorDidMount}
          theme={theme === 'dark' ? 'vs-dark' : 'vs'}
          options={editorOptions}
        />
      </Card>
    </div>
  )
}
