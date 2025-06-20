import React, { useRef, useEffect, useState, useCallback } from 'react'
import { Card, Button, Space, Tooltip, Typography, Dropdown, Tag, Alert, Spin } from 'antd'
import {
  PlayCircleOutlined,
  CopyOutlined,
  FormatPainterOutlined,
  FullscreenOutlined,
  SettingOutlined,
  SaveOutlined,
  HistoryOutlined,
  BulbOutlined,
  DatabaseOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import Editor, { Monaco } from '@monaco-editor/react'
import type { editor } from 'monaco-editor'
import { useAppSelector } from '@shared/hooks'
import { selectChatSettings } from '@shared/store/chat'
import { useGetBusinessTablesQuery } from '@shared/store/api/businessApi'
import { useGetBusinessContextQuery } from '@shared/store/api/chatApi'

const { Text } = Typography

interface EnhancedSqlEditorProps {
  value?: string
  onChange?: (value: string) => void
  onExecute?: (sql: string) => void
  onSave?: (sql: string, name?: string) => void
  height?: number
  readOnly?: boolean
  showExecuteButton?: boolean
  showFormatButton?: boolean
  showCopyButton?: boolean
  showSaveButton?: boolean
  theme?: 'light' | 'dark'
  placeholder?: string
  enableBusinessContext?: boolean
  enableValidation?: boolean
  enableAutocomplete?: boolean
}

interface ValidationError {
  line: number
  column: number
  message: string
  severity: 'error' | 'warning' | 'info'
}

export const EnhancedSqlEditor: React.FC<EnhancedSqlEditorProps> = ({
  value = '',
  onChange,
  onExecute,
  onSave,
  height = 300,
  readOnly = false,
  showExecuteButton = true,
  showFormatButton = true,
  showCopyButton = true,
  showSaveButton = false,
  theme = 'light',
  placeholder = 'Enter your SQL query here...',
  enableBusinessContext = true,
  enableValidation = true,
  enableAutocomplete = true,
}) => {
  const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null)
  const monacoRef = useRef<Monaco | null>(null)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const [validationErrors, setValidationErrors] = useState<ValidationError[]>([])
  const [isValidating, setIsValidating] = useState(false)
  const [suggestions, setSuggestions] = useState<any[]>([])

  const settings = useAppSelector(selectChatSettings)
  const { data: businessTables, isLoading: tablesLoading } = useGetBusinessTablesQuery()
  const { data: businessContext } = useGetBusinessContextQuery(
    { query: value },
    { skip: !enableBusinessContext || value.length < 3 }
  )

  const handleEditorDidMount = useCallback((editor: editor.IStandaloneCodeEditor, monaco: Monaco) => {
    editorRef.current = editor
    monacoRef.current = monaco

    // Configure SQL language with enhanced features
    setupSqlLanguage(monaco)
    
    // Set up business context autocomplete
    if (enableAutocomplete) {
      setupAutocomplete(monaco, editor)
    }

    // Set up validation
    if (enableValidation) {
      setupValidation(monaco, editor)
    }

    // Add keyboard shortcuts
    setupKeyboardShortcuts(editor, monaco)

    // Set placeholder
    if (!value && placeholder) {
      editor.setValue(placeholder)
      editor.setSelection(new monaco.Selection(1, 1, 1, placeholder.length))
    }
  }, [value, placeholder, enableAutocomplete, enableValidation])

  const setupSqlLanguage = (monaco: Monaco) => {
    // Enhanced SQL language configuration
    monaco.languages.setLanguageConfiguration('sql', {
      comments: {
        lineComment: '--',
        blockComment: ['/*', '*/'],
      },
      brackets: [
        ['(', ')'],
        ['[', ']'],
        ['{', '}'],
      ],
      autoClosingPairs: [
        { open: '(', close: ')' },
        { open: '[', close: ']' },
        { open: '{', close: '}' },
        { open: "'", close: "'" },
        { open: '"', close: '"' },
        { open: '`', close: '`' },
      ],
      surroundingPairs: [
        { open: '(', close: ')' },
        { open: '[', close: ']' },
        { open: '{', close: '}' },
        { open: "'", close: "'" },
        { open: '"', close: '"' },
        { open: '`', close: '`' },
      ],
      folding: {
        markers: {
          start: new RegExp('^\\s*--\\s*#region\\b'),
          end: new RegExp('^\\s*--\\s*#endregion\\b'),
        },
      },
    })

    // Enhanced tokenization with business context
    monaco.languages.setMonarchTokensProvider('sql', {
      tokenizer: {
        root: [
          // SQL Keywords
          [/\b(?:SELECT|FROM|WHERE|JOIN|INNER|LEFT|RIGHT|FULL|OUTER|ON|GROUP|BY|ORDER|HAVING|UNION|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP|TABLE|INDEX|VIEW|DATABASE|SCHEMA|AS|AND|OR|NOT|IN|LIKE|BETWEEN|IS|NULL|TRUE|FALSE|DISTINCT|COUNT|SUM|AVG|MIN|MAX|CASE|WHEN|THEN|ELSE|END)\b/i, 'keyword'],
          
          // Data types
          [/\b(?:INT|INTEGER|VARCHAR|CHAR|TEXT|DATE|DATETIME|TIMESTAMP|DECIMAL|FLOAT|DOUBLE|BOOLEAN|BIGINT|SMALLINT|TINYINT|BLOB|CLOB)\b/i, 'type'],
          
          // Functions
          [/\b(?:COALESCE|ISNULL|NULLIF|CAST|CONVERT|SUBSTRING|TRIM|UPPER|LOWER|LENGTH|ROUND|FLOOR|CEIL|ABS|POWER|SQRT|NOW|GETDATE|DATEADD|DATEDIFF)\b/i, 'predefined'],
          
          // Business table names (dynamic based on metadata)
          ...(businessTables?.map(table => [
            new RegExp(`\\b${table.schemaName}\\.${table.tableName}\\b`, 'i'),
            'entity.name.class'
          ]) || []),
          
          // String literals
          [/'([^'\\]|\\.)*'/, 'string'],
          [/"([^"\\]|\\.)*"/, 'string'],
          [/`([^`\\]|\\.)*`/, 'string'],
          
          // Numbers
          [/\d+(\.\d+)?/, 'number'],
          
          // Comments
          [/--.*$/, 'comment'],
          [/\/\*/, 'comment', '@comment'],
          
          // Operators
          [/[=<>!]+/, 'operator'],
          [/[+\-*\/]/, 'operator'],
        ],
        comment: [
          [/[^\/*]+/, 'comment'],
          [/\*\//, 'comment', '@pop'],
          [/[\/*]/, 'comment'],
        ],
      },
    })
  }

  const setupAutocomplete = (monaco: Monaco, editor: editor.IStandaloneCodeEditor) => {
    // Register completion provider with business context
    monaco.languages.registerCompletionItemProvider('sql', {
      provideCompletionItems: (model, position) => {
        const word = model.getWordUntilPosition(position)
        const range = {
          startLineNumber: position.lineNumber,
          endLineNumber: position.lineNumber,
          startColumn: word.startColumn,
          endColumn: word.endColumn,
        }

        const suggestions: any[] = []
        const lineText = model.getLineContent(position.lineNumber)
        const beforeCursor = lineText.substring(0, position.column - 1)

        // Context-aware suggestions based on SQL structure
        const isAfterSelect = /\bSELECT\s+$/i.test(beforeCursor)
        const isAfterFrom = /\bFROM\s+$/i.test(beforeCursor)
        const isAfterWhere = /\bWHERE\s+$/i.test(beforeCursor)
        const isAfterJoin = /\b(JOIN|INNER\s+JOIN|LEFT\s+JOIN|RIGHT\s+JOIN)\s+$/i.test(beforeCursor)

        // SQL Keywords with context awareness
        const sqlKeywords = [
          { keyword: 'SELECT', contexts: ['start'] },
          { keyword: 'FROM', contexts: ['after_select'] },
          { keyword: 'WHERE', contexts: ['after_from'] },
          { keyword: 'JOIN', contexts: ['after_from'] },
          { keyword: 'INNER JOIN', contexts: ['after_from'] },
          { keyword: 'LEFT JOIN', contexts: ['after_from'] },
          { keyword: 'RIGHT JOIN', contexts: ['after_from'] },
          { keyword: 'GROUP BY', contexts: ['after_where'] },
          { keyword: 'ORDER BY', contexts: ['after_where'] },
          { keyword: 'HAVING', contexts: ['after_group'] },
          { keyword: 'LIMIT', contexts: ['after_order'] },
          { keyword: 'UNION', contexts: ['after_complete'] },
          { keyword: 'DISTINCT', contexts: ['after_select'] },
          { keyword: 'COUNT', contexts: ['after_select'] },
          { keyword: 'SUM', contexts: ['after_select'] },
          { keyword: 'AVG', contexts: ['after_select'] },
          { keyword: 'MIN', contexts: ['after_select'] },
          { keyword: 'MAX', contexts: ['after_select'] }
        ]

        sqlKeywords.forEach(({ keyword }) => {
          suggestions.push({
            label: keyword,
            kind: monaco.languages.CompletionItemKind.Keyword,
            insertText: keyword,
            range,
            sortText: '0' + keyword, // Prioritize keywords
          })
        })

        // Business tables and columns with enhanced context
        if (businessTables) {
          businessTables.forEach(table => {
            const tableFullName = `${table.schemaName}.${table.tableName}`

            // Table suggestions (prioritize after FROM/JOIN)
            if (isAfterFrom || isAfterJoin || !isAfterSelect) {
              suggestions.push({
                label: tableFullName,
                kind: monaco.languages.CompletionItemKind.Class,
                insertText: tableFullName,
                detail: `Table: ${table.businessPurpose}`,
                documentation: {
                  value: `**Business Purpose:** ${table.businessPurpose}\n\n**Domain:** ${table.domainClassification}\n\n**Row Count:** ${table.estimatedRowCount || 'Unknown'}\n\n**Last Updated:** ${table.lastUpdated || 'Unknown'}`,
                },
                range,
                sortText: isAfterFrom || isAfterJoin ? '1' + tableFullName : '3' + tableFullName,
              })

              // Table alias suggestion
              suggestions.push({
                label: `${tableFullName} AS ${table.tableName.toLowerCase()}`,
                kind: monaco.languages.CompletionItemKind.Class,
                insertText: `${tableFullName} AS ${table.tableName.toLowerCase()}`,
                detail: `Table with alias`,
                range,
                sortText: '2' + tableFullName,
              })
            }

            // Column suggestions (prioritize after SELECT/WHERE)
            table.columns?.forEach(column => {
              const columnDetail = `${table.tableName}.${column.columnName} (${column.businessDataType})`

              suggestions.push({
                label: column.columnName,
                kind: monaco.languages.CompletionItemKind.Field,
                insertText: column.columnName,
                detail: columnDetail,
                documentation: {
                  value: `**Business Meaning:** ${column.businessMeaning}\n\n**Data Type:** ${column.businessDataType}\n\n**Nullable:** ${column.isNullable ? 'Yes' : 'No'}\n\n**Sample Values:** ${column.sampleValues?.join(', ') || 'N/A'}`,
                },
                range,
                sortText: isAfterSelect ? '1' + column.columnName : '2' + column.columnName,
              })

              // Qualified column name
              suggestions.push({
                label: `${table.tableName}.${column.columnName}`,
                kind: monaco.languages.CompletionItemKind.Field,
                insertText: `${table.tableName}.${column.columnName}`,
                detail: columnDetail,
                range,
                sortText: '3' + column.columnName,
              })

              // Common aggregations for numeric columns
              if (column.businessDataType?.toLowerCase().includes('number') ||
                  column.businessDataType?.toLowerCase().includes('decimal') ||
                  column.businessDataType?.toLowerCase().includes('int')) {
                ['COUNT', 'SUM', 'AVG', 'MIN', 'MAX'].forEach(func => {
                  suggestions.push({
                    label: `${func}(${column.columnName})`,
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: `${func}(${column.columnName})`,
                    detail: `${func} of ${column.columnName}`,
                    range,
                    sortText: '4' + func + column.columnName,
                  })
                })
              }
            })
          })
        }

        // Business context suggestions
        if (businessContext) {
          businessContext.glossaryTerms?.forEach(term => {
            suggestions.push({
              label: term.term,
              kind: monaco.languages.CompletionItemKind.Reference,
              insertText: `'${term.term}'`,
              detail: 'Business Term',
              documentation: {
                value: `**Definition:** ${term.definition}`,
              },
              range,
              sortText: '5' + term.term,
            })
          })
        }

        // SQL Functions and operators
        const sqlFunctions = [
          'COALESCE', 'ISNULL', 'NULLIF', 'CAST', 'CONVERT', 'SUBSTRING', 'TRIM',
          'UPPER', 'LOWER', 'LENGTH', 'ROUND', 'FLOOR', 'CEIL', 'ABS', 'POWER',
          'SQRT', 'NOW', 'GETDATE', 'DATEADD', 'DATEDIFF', 'YEAR', 'MONTH', 'DAY'
        ]

        sqlFunctions.forEach(func => {
          suggestions.push({
            label: func,
            kind: monaco.languages.CompletionItemKind.Function,
            insertText: `${func}($1)`,
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            detail: `SQL Function`,
            range,
            sortText: '6' + func,
          })
        })

        // Common SQL patterns as snippets
        const sqlSnippets = [
          {
            label: 'SELECT with JOIN',
            insertText: 'SELECT $1\nFROM $2\nJOIN $3 ON $4 = $5',
            detail: 'SELECT statement with JOIN',
          },
          {
            label: 'GROUP BY with COUNT',
            insertText: 'SELECT $1, COUNT(*) as count\nFROM $2\nGROUP BY $1\nORDER BY count DESC',
            detail: 'GROUP BY with count',
          },
          {
            label: 'Date range filter',
            insertText: "WHERE $1 BETWEEN '$2' AND '$3'",
            detail: 'Date range WHERE clause',
          }
        ]

        sqlSnippets.forEach(snippet => {
          suggestions.push({
            label: snippet.label,
            kind: monaco.languages.CompletionItemKind.Snippet,
            insertText: snippet.insertText,
            insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
            detail: snippet.detail,
            range,
            sortText: '7' + snippet.label,
          })
        })

        return { suggestions }
      },
    })

    // Register hover provider for business context
    monaco.languages.registerHoverProvider('sql', {
      provideHover: (model, position) => {
        const word = model.getWordAtPosition(position)
        if (!word) return null

        // Find business context for the word
        const table = businessTables?.find(t =>
          t.tableName.toLowerCase() === word.word.toLowerCase() ||
          t.columns?.some(c => c.columnName.toLowerCase() === word.word.toLowerCase())
        )

        if (table) {
          const column = table.columns?.find(c =>
            c.columnName.toLowerCase() === word.word.toLowerCase()
          )

          if (column) {
            return {
              range: new monaco.Range(
                position.lineNumber,
                word.startColumn,
                position.lineNumber,
                word.endColumn
              ),
              contents: [
                { value: `**${table.tableName}.${column.columnName}**` },
                { value: `**Business Meaning:** ${column.businessMeaning}` },
                { value: `**Data Type:** ${column.businessDataType}` },
                { value: `**Nullable:** ${column.isNullable ? 'Yes' : 'No'}` },
                ...(column.sampleValues ? [{ value: `**Sample Values:** ${column.sampleValues.join(', ')}` }] : [])
              ]
            }
          } else if (table.tableName.toLowerCase() === word.word.toLowerCase()) {
            return {
              range: new monaco.Range(
                position.lineNumber,
                word.startColumn,
                position.lineNumber,
                word.endColumn
              ),
              contents: [
                { value: `**${table.schemaName}.${table.tableName}**` },
                { value: `**Business Purpose:** ${table.businessPurpose}` },
                { value: `**Domain:** ${table.domainClassification}` },
                { value: `**Estimated Rows:** ${table.estimatedRowCount || 'Unknown'}` }
              ]
            }
          }
        }

        return null
      }
    })
  }

  const setupValidation = (monaco: Monaco, editor: editor.IStandaloneCodeEditor) => {
    // Set up real-time validation
    const validateSQL = async (sql: string) => {
      if (!sql.trim()) {
        setValidationErrors([])
        return
      }

      setIsValidating(true)
      
      try {
        // Basic SQL syntax validation
        const errors: ValidationError[] = []
        
        // Check for common SQL errors
        const lines = sql.split('\n')
        lines.forEach((line, index) => {
          // Check for missing semicolon at end of statements
          if (line.trim().match(/^(SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP)/i) && 
              !line.trim().endsWith(';') && 
              index === lines.length - 1) {
            errors.push({
              line: index + 1,
              column: line.length + 1,
              message: 'Consider adding a semicolon at the end of the statement',
              severity: 'info',
            })
          }
          
          // Check for potential SQL injection patterns
          if (line.includes("'") && line.includes('+')) {
            errors.push({
              line: index + 1,
              column: line.indexOf("'") + 1,
              message: 'Potential SQL injection vulnerability detected',
              severity: 'warning',
            })
          }
        })

        setValidationErrors(errors)
        
        // Set Monaco markers
        const markers = errors.map(error => ({
          startLineNumber: error.line,
          startColumn: error.column,
          endLineNumber: error.line,
          endColumn: error.column + 1,
          message: error.message,
          severity: error.severity === 'error' ? monaco.MarkerSeverity.Error :
                   error.severity === 'warning' ? monaco.MarkerSeverity.Warning :
                   monaco.MarkerSeverity.Info,
        }))
        
        monaco.editor.setModelMarkers(editor.getModel()!, 'sql-validation', markers)
        
      } catch (error) {
        console.error('Validation error:', error)
      } finally {
        setIsValidating(false)
      }
    }

    // Debounced validation
    let validationTimeout: NodeJS.Timeout
    editor.onDidChangeModelContent(() => {
      clearTimeout(validationTimeout)
      validationTimeout = setTimeout(() => {
        validateSQL(editor.getValue())
      }, 500)
    })
  }

  const setupKeyboardShortcuts = (editor: editor.IStandaloneCodeEditor, monaco: Monaco) => {
    // Execute query (Ctrl+Enter)
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, () => {
      handleExecute()
    })

    // Format SQL (Ctrl+Shift+F)
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyMod.Shift | monaco.KeyCode.KeyF, () => {
      handleFormat()
    })

    // Save query (Ctrl+S)
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => {
      if (onSave) {
        onSave(editor.getValue())
      }
    })

    // Toggle fullscreen (F11)
    editor.addCommand(monaco.KeyCode.F11, () => {
      setIsFullscreen(!isFullscreen)
    })
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
    } catch (error) {
      console.error('Failed to copy SQL:', error)
    }
  }

  const handleSave = () => {
    const sql = editorRef.current?.getValue() || ''
    if (onSave) {
      onSave(sql)
    }
  }

  const toggleFullscreen = () => {
    setIsFullscreen(!isFullscreen)
  }

  const getValidationSummary = () => {
    const errors = validationErrors.filter(e => e.severity === 'error').length
    const warnings = validationErrors.filter(e => e.severity === 'warning').length
    const infos = validationErrors.filter(e => e.severity === 'info').length

    if (errors > 0) {
      return { type: 'error' as const, count: errors, message: `${errors} error(s)` }
    }
    if (warnings > 0) {
      return { type: 'warning' as const, count: warnings, message: `${warnings} warning(s)` }
    }
    if (infos > 0) {
      return { type: 'info' as const, count: infos, message: `${infos} suggestion(s)` }
    }
    return { type: 'success' as const, count: 0, message: 'No issues' }
  }

  const editorOptions: editor.IStandaloneEditorConstructionOptions = {
    minimap: { enabled: height > 400 },
    scrollBeyondLastLine: false,
    fontSize: settings.fontSize === 'small' ? 12 : settings.fontSize === 'large' ? 16 : 14,
    lineHeight: 20,
    readOnly,
    wordWrap: 'on',
    automaticLayout: true,
    suggestOnTriggerCharacters: enableAutocomplete,
    quickSuggestions: enableAutocomplete,
    tabSize: 2,
    insertSpaces: true,
    folding: true,
    lineNumbers: 'on',
    glyphMargin: true,
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
    acceptSuggestionOnEnter: 'on',
    acceptSuggestionOnCommitCharacter: true,
    snippetSuggestions: 'top',
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

  const validationSummary = getValidationSummary()

  return (
    <div style={containerStyle}>
      <Card
        size="small"
        title={
          <Space>
            <DatabaseOutlined />
            <Text strong>Enhanced SQL Editor</Text>
            {enableBusinessContext && businessContext && (
              <Tag color="blue">
                {businessContext.tables.length} tables available
              </Tag>
            )}
            {isValidating && <Spin size="small" />}
            {!isValidating && validationErrors.length > 0 && (
              <Tag color={validationSummary.type === 'error' ? 'red' : 
                          validationSummary.type === 'warning' ? 'orange' : 'blue'}>
                {validationSummary.message}
              </Tag>
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
            {showSaveButton && (
              <Tooltip title="Save Query (Ctrl+S)">
                <Button size="small" icon={<SaveOutlined />} onClick={handleSave} />
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
            <Tooltip title={isFullscreen ? 'Exit Fullscreen (F11)' : 'Fullscreen (F11)'}>
              <Button size="small" icon={<FullscreenOutlined />} onClick={toggleFullscreen} />
            </Tooltip>
          </Space>
        }
        style={{ height: isFullscreen ? '100vh' : height + 60 }}
      >
        {tablesLoading && (
          <Alert
            message="Loading business context..."
            type="info"
            showIcon
            style={{ marginBottom: 8 }}
          />
        )}
        
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
