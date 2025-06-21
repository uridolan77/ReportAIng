import React, { useState, useEffect } from 'react'
import { Input, Button, Space, Typography, Alert, Tooltip } from 'antd'
import { EditOutlined, EyeOutlined, CheckOutlined, CloseOutlined, FormatPainterOutlined } from '@ant-design/icons'

const { TextArea } = Input
const { Text } = Typography

interface JsonEditorProps {
  value?: string
  onChange?: (value: string) => void
  placeholder?: string
  rows?: number
  disabled?: boolean
  allowArrays?: boolean
  allowObjects?: boolean
  label?: string
  allowInlineStringEdit?: boolean
}

export const JsonEditor: React.FC<JsonEditorProps> = ({
  value = '',
  onChange,
  placeholder = 'Enter JSON data...',
  rows = 6,
  disabled = false,
  allowArrays = true,
  allowObjects = true,
  label,
  allowInlineStringEdit = false
}) => {
  const [isEditing, setIsEditing] = useState(false)
  const [editValue, setEditValue] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [inlineEditPath, setInlineEditPath] = useState<string | null>(null)
  const [inlineEditValue, setInlineEditValue] = useState('')
  const [parsedData, setParsedData] = useState<any>(null)

  useEffect(() => {
    if (value) {
      try {
        const parsed = JSON.parse(value)
        setParsedData(parsed)
        setError(null)
      } catch (e) {
        // If it's not valid JSON, try to parse as comma-separated values
        if (typeof value === 'string' && value.trim()) {
          const items = value.split(',').map(item => item.trim()).filter(Boolean)
          if (items.length > 0) {
            setParsedData(items)
            setError(null)
          } else {
            setParsedData(null)
            setError('Invalid JSON format')
          }
        } else {
          setParsedData(null)
          setError(null)
        }
      }
    } else {
      setParsedData(null)
      setError(null)
    }
  }, [value])

  const handleEdit = () => {
    setEditValue(value || '')
    setIsEditing(true)
    setError(null)
  }

  const handleSave = () => {
    try {
      if (editValue.trim()) {
        // Try to parse as JSON first
        const parsed = JSON.parse(editValue)
        
        // Validate type if restrictions are set
        if (!allowArrays && Array.isArray(parsed)) {
          setError('Arrays are not allowed for this field')
          return
        }
        if (!allowObjects && typeof parsed === 'object' && !Array.isArray(parsed)) {
          setError('Objects are not allowed for this field')
          return
        }
        
        onChange?.(editValue)
        setError(null)
      } else {
        onChange?.('')
        setError(null)
      }
      setIsEditing(false)
    } catch (e) {
      // If JSON parsing fails, try comma-separated values for arrays
      if (allowArrays && editValue.includes(',')) {
        try {
          const items = editValue.split(',').map(item => item.trim()).filter(Boolean)
          const jsonArray = JSON.stringify(items)
          onChange?.(jsonArray)
          setError(null)
          setIsEditing(false)
        } catch (e2) {
          setError('Invalid format. Please enter valid JSON or comma-separated values.')
        }
      } else {
        setError('Invalid JSON format. Please check your syntax.')
      }
    }
  }

  const handleCancel = () => {
    setIsEditing(false)
    setEditValue('')
    setError(null)
  }

  const handleFormat = () => {
    try {
      if (editValue.trim()) {
        const parsed = JSON.parse(editValue)
        const formatted = JSON.stringify(parsed, null, 2)
        setEditValue(formatted)
        setError(null)
      }
    } catch (e) {
      // Try to format as array from comma-separated values
      if (editValue.includes(',')) {
        try {
          const items = editValue.split(',').map(item => item.trim()).filter(Boolean)
          const formatted = JSON.stringify(items, null, 2)
          setEditValue(formatted)
          setError(null)
        } catch (e2) {
          setError('Cannot format: Invalid JSON syntax')
        }
      } else {
        setError('Cannot format: Invalid JSON syntax')
      }
    }
  }

  const handleFullEdit = () => {
    setEditValue(value || '')
    setIsEditing(true)
    setError(null)
  }

  const handleInlineStringEdit = (path: string, currentValue: string) => {
    console.log('handleInlineStringEdit called:', { path, currentValue, allowInlineStringEdit, parsedData })
    if (allowInlineStringEdit) {
      setInlineEditPath(path)
      setInlineEditValue(currentValue)
    } else {
      handleFullEdit()
    }
  }

  const handleInlineStringSave = () => {
    if (!inlineEditPath) return

    try {
      // Special case: if path is 'value', replace the entire data
      if (inlineEditPath === 'value') {
        onChange?.(JSON.stringify(inlineEditValue))
        setInlineEditPath(null)
        setInlineEditValue('')
        return
      }

      if (!parsedData) return

      const newData = JSON.parse(JSON.stringify(parsedData))

      // For simple object keys (no dots), just update directly
      if (!inlineEditPath.includes('.') && !inlineEditPath.startsWith('[')) {
        newData[inlineEditPath] = inlineEditValue
      } else {
        // Handle complex paths
        const pathParts = inlineEditPath.split('.')

        let current = newData
        for (let i = 0; i < pathParts.length - 1; i++) {
          const part = pathParts[i]
          if (part.startsWith('[') && part.endsWith(']')) {
            const index = parseInt(part.slice(1, -1))
            current = current[index]
          } else {
            current = current[part]
          }
        }

        const lastPart = pathParts[pathParts.length - 1]
        if (lastPart.startsWith('[') && lastPart.endsWith(']')) {
          const index = parseInt(lastPart.slice(1, -1))
          current[index] = inlineEditValue
        } else {
          current[lastPart] = inlineEditValue
        }
      }

      const newJsonString = JSON.stringify(newData, null, 2)
      onChange?.(newJsonString)
      setInlineEditPath(null)
      setInlineEditValue('')
    } catch (err) {
      console.error('Error updating inline value:', err)
    }
  }

  const handleInlineStringCancel = () => {
    setInlineEditPath(null)
    setInlineEditValue('')
  }

  const renderViewer = () => {
    const containerStyle = {
      background: '#f5f5f5',
      padding: 12,
      borderRadius: 4,
      fontSize: '12px',
      height: '200px',
      border: '1px solid #d9d9d9',
      cursor: allowInlineStringEdit ? 'default' : 'pointer',
      overflow: 'auto',
      position: 'relative' as const
    }

    const handleContainerClick = (e: React.MouseEvent) => {
      // Only open full edit if clicking on empty space, not on interactive elements
      const target = e.target as HTMLElement;
      if (target.classList.contains('editable-string') ||
          target.closest('.editable-string') ||
          target.tagName === 'INPUT' ||
          target.tagName === 'BUTTON' ||
          target.closest('button') ||
          target.closest('.ant-space')) {
        return;
      }
      handleFullEdit();
    }

    if (!parsedData) {
      return (
        <div style={containerStyle} onClick={handleContainerClick}>
          <Text type="secondary" style={{ fontStyle: 'italic' }}>
            No data entered - Click to add
          </Text>
        </div>
      )
    }

    if (Array.isArray(parsedData)) {
      return (
        <div style={containerStyle} onClick={handleContainerClick}>
          <Text strong>Array ({parsedData.length} items):</Text>
          <ul style={{ margin: '4px 0', paddingLeft: 16 }}>
            {parsedData.slice(0, 5).map((item, index) => {
              const isString = typeof item === 'string'
              const isEditingThis = inlineEditPath === `[${index}]`

              return (
                <li key={index} style={{ marginBottom: 2 }}>
                  {isEditingThis ? (
                    <Space size="small" onClick={(e) => e.stopPropagation()}>
                      <Input
                        size="small"
                        value={inlineEditValue}
                        onChange={(e) => setInlineEditValue(e.target.value)}
                        onPressEnter={handleInlineStringSave}
                        style={{ width: '90%', minWidth: 200 }}
                        autoFocus
                      />
                      <Button size="small" type="primary" onClick={handleInlineStringSave}>
                        ✓
                      </Button>
                      <Button size="small" onClick={handleInlineStringCancel}>
                        ✕
                      </Button>
                    </Space>
                  ) : (
                    <Text
                      code
                      style={{
                        cursor: isString && allowInlineStringEdit ? 'pointer' : 'default',
                        padding: '2px 4px',
                        borderRadius: '2px',
                        background: isString && allowInlineStringEdit ? '#f0f8ff' : 'transparent',
                        border: isString && allowInlineStringEdit ? '1px solid #d9d9d9' : 'none',
                        display: 'inline-block',
                        transition: 'all 0.2s'
                      }}
                      onClick={(e) => {
                        e.stopPropagation()
                        console.log('Array item clicked:', { isString, allowInlineStringEdit, item, index })
                        if (isString && allowInlineStringEdit) {
                          handleInlineStringEdit(`[${index}]`, item)
                        } else {
                          // For non-string items (objects), still open full editor
                          handleFullEdit()
                        }
                      }}
                      onMouseEnter={(e) => {
                        if (isString && allowInlineStringEdit) {
                          e.currentTarget.style.background = '#e6f7ff'
                          e.currentTarget.style.borderColor = '#1890ff'
                        }
                      }}
                      onMouseLeave={(e) => {
                        if (isString && allowInlineStringEdit) {
                          e.currentTarget.style.background = '#f0f8ff'
                          e.currentTarget.style.borderColor = '#d9d9d9'
                        }
                      }}
                    >
                      {typeof item === 'string' ? item : JSON.stringify(item)}
                      {isString && allowInlineStringEdit && (
                        <span style={{ marginLeft: 4, fontSize: '10px', color: '#1890ff' }}>✏️</span>
                      )}
                    </Text>
                  )}
                </li>
              )
            })}
            {parsedData.length > 5 && (
              <li><Text type="secondary">... and {parsedData.length - 5} more</Text></li>
            )}
          </ul>
          {!allowInlineStringEdit && (
            <div
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                cursor: 'pointer'
              }}
              onClick={() => handleFullEdit()}
            />
          )}
        </div>
      )
    }

    if (typeof parsedData === 'object') {
      const keys = Object.keys(parsedData)
      return (
        <div style={containerStyle} onClick={handleContainerClick}>
          <Text strong>Object ({keys.length} properties):</Text>
          <ul style={{ margin: '4px 0', paddingLeft: 16 }}>
            {keys.slice(0, 8).map(key => {
              const value = parsedData[key]
              const isString = typeof value === 'string'
              const isEditingThis = inlineEditPath === key

              return (
                <li key={key} style={{ marginBottom: 2 }}>
                  <Text code style={{ marginRight: 4 }}>{key}:</Text>
                  {isEditingThis ? (
                    <Space size="small" onClick={(e) => e.stopPropagation()}>
                      <Input
                        size="small"
                        value={inlineEditValue}
                        onChange={(e) => setInlineEditValue(e.target.value)}
                        onPressEnter={handleInlineStringSave}
                        style={{ width: '90%', minWidth: 200 }}
                        autoFocus
                      />
                      <Button size="small" type="primary" onClick={handleInlineStringSave}>
                        ✓
                      </Button>
                      <Button size="small" onClick={handleInlineStringCancel}>
                        ✕
                      </Button>
                    </Space>
                  ) : (
                    <Text
                      code
                      style={{
                        cursor: isString && allowInlineStringEdit ? 'pointer' : 'default',
                        padding: '2px 4px',
                        borderRadius: '2px',
                        background: isString && allowInlineStringEdit ? '#f0f8ff' : 'transparent',
                        border: isString && allowInlineStringEdit ? '1px solid #d9d9d9' : 'none',
                        display: 'inline-block',
                        transition: 'all 0.2s'
                      }}
                      className={isString && allowInlineStringEdit ? 'editable-string' : ''}
                      onClick={(e) => {
                        e.stopPropagation()
                        console.log('Object property clicked:', { key, value, isString, allowInlineStringEdit })
                        if (isString && allowInlineStringEdit) {
                          handleInlineStringEdit(key, value)
                        } else {
                          handleFullEdit()
                        }
                      }}
                      onMouseEnter={(e) => {
                        if (isString && allowInlineStringEdit) {
                          e.currentTarget.style.background = '#e6f7ff'
                          e.currentTarget.style.borderColor = '#1890ff'
                        }
                      }}
                      onMouseLeave={(e) => {
                        if (isString && allowInlineStringEdit) {
                          e.currentTarget.style.background = '#f0f8ff'
                          e.currentTarget.style.borderColor = '#d9d9d9'
                        }
                      }}
                    >
                      {JSON.stringify(value)}
                      {isString && allowInlineStringEdit && (
                        <span style={{ marginLeft: 4, fontSize: '10px', color: '#1890ff' }}>✏️</span>
                      )}
                    </Text>
                  )}
                </li>
              )
            })}
            {keys.length > 8 && (
              <li><Text type="secondary">... and {keys.length - 8} more</Text></li>
            )}
          </ul>
          {!allowInlineStringEdit && (
            <div
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                cursor: 'pointer'
              }}
              onClick={() => handleFullEdit()}
            />
          )}
        </div>
      )
    }

    const isEditingThis = inlineEditPath === 'value'

    return (
      <div style={containerStyle} onClick={handleContainerClick}>
        {isEditingThis ? (
          <Space size="small" onClick={(e) => e.stopPropagation()}>
            <Input
              size="small"
              value={inlineEditValue}
              onChange={(e) => setInlineEditValue(e.target.value)}
              onPressEnter={handleInlineStringSave}
              style={{ width: '90%', minWidth: 200 }}
              autoFocus
            />
            <Button size="small" type="primary" onClick={handleInlineStringSave}>
              ✓
            </Button>
            <Button size="small" onClick={handleInlineStringCancel}>
              ✕
            </Button>
          </Space>
        ) : (
          <Text
            code
            style={{
              cursor: allowInlineStringEdit && typeof parsedData === 'string' ? 'pointer' : 'default',
              padding: '2px 4px',
              borderRadius: '2px',
              background: allowInlineStringEdit && typeof parsedData === 'string' ? '#f0f8ff' : 'transparent',
              border: allowInlineStringEdit && typeof parsedData === 'string' ? '1px solid #d9d9d9' : 'none',
              display: 'inline-block',
              transition: 'all 0.2s'
            }}
            onClick={(e) => {
              e.stopPropagation()
              console.log('Fallback case clicked:', { allowInlineStringEdit, parsedDataType: typeof parsedData, parsedData })
              if (allowInlineStringEdit && typeof parsedData === 'string') {
                handleInlineStringEdit('value', parsedData)
              } else {
                handleFullEdit()
              }
            }}
            onMouseEnter={(e) => {
              if (allowInlineStringEdit && typeof parsedData === 'string') {
                e.currentTarget.style.background = '#e6f7ff'
                e.currentTarget.style.borderColor = '#1890ff'
              }
            }}
            onMouseLeave={(e) => {
              if (allowInlineStringEdit && typeof parsedData === 'string') {
                e.currentTarget.style.background = '#f0f8ff'
                e.currentTarget.style.borderColor = '#d9d9d9'
              }
            }}
          >
            {JSON.stringify(parsedData)}
            {allowInlineStringEdit && typeof parsedData === 'string' && (
              <span style={{ marginLeft: 4, fontSize: '10px', color: '#1890ff' }}>✏️</span>
            )}
          </Text>
        )}
        {!allowInlineStringEdit && (
          <div
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              bottom: 0,
              cursor: 'pointer'
            }}
            onClick={() => handleFullEdit()}
          />
        )}
      </div>
    )
  }



  if (isEditing) {
    return (
      <div>
        <div style={{ marginBottom: 8 }}>
          <Space>
            <Tooltip title="Save changes">
              <Button
                type="primary"
                size="small"
                icon={<CheckOutlined />}
                onClick={handleSave}
              />
            </Tooltip>
            <Tooltip title="Cancel editing">
              <Button
                size="small"
                icon={<CloseOutlined />}
                onClick={handleCancel}
              />
            </Tooltip>
            <Tooltip title="Format JSON">
              <Button
                size="small"
                icon={<FormatPainterOutlined />}
                onClick={handleFormat}
              />
            </Tooltip>
          </Space>
        </div>
        <TextArea
          value={editValue}
          onChange={(e) => setEditValue(e.target.value)}
          placeholder={placeholder}
          style={{
            fontFamily: 'Monaco, Menlo, "Ubuntu Mono", monospace',
            fontSize: '12px',
            height: '200px',
            resize: 'none'
          }}
        />
        {error && (
          <Alert
            message={error}
            type="error"
            size="small"
            style={{ marginTop: 8 }}
          />
        )}
        <div style={{ marginTop: 4, fontSize: '11px', color: '#666' }}>
          <Text type="secondary">
            Tip: Enter valid JSON or comma-separated values for arrays
          </Text>
        </div>
      </div>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 8 }}>
        <Space>
          <Tooltip title="Edit JSON data">
            <Button 
              size="small" 
              icon={<EditOutlined />} 
              onClick={handleEdit}
              disabled={disabled}
            >
              Edit
            </Button>
          </Tooltip>
          {parsedData && (
            <Text type="secondary" style={{ fontSize: '11px' }}>
              {Array.isArray(parsedData) ? `${parsedData.length} items` : 
               typeof parsedData === 'object' ? `${Object.keys(parsedData).length} properties` : 
               'Value'}
            </Text>
          )}
        </Space>
      </div>
      {renderViewer()}
    </div>
  )
}

export default JsonEditor
