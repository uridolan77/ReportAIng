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

      // Handle different path formats
      if (inlineEditPath.includes('[') && inlineEditPath.includes(']')) {
        // Handle object property arrays like "commonJoins[0]", "typicalFilters[1]", etc.
        const match = inlineEditPath.match(/^([^[]+)\[(\d+)\]$/)
        if (match) {
          const propertyName = match[1]
          const index = parseInt(match[2])
          if (newData[propertyName] && Array.isArray(newData[propertyName]) &&
              index >= 0 && index < newData[propertyName].length) {
            newData[propertyName][index] = inlineEditValue
          }
        }
      } else if (inlineEditPath.startsWith('[') && inlineEditPath.includes('].')) {
        // Array object property like [0].key, [1].property, etc.
        const match = inlineEditPath.match(/\[(\d+)\]\.(.+)/)
        if (match) {
          const index = parseInt(match[1])
          const property = match[2]
          if (Array.isArray(newData) && index >= 0 && index < newData.length) {
            if (newData[index] && typeof newData[index] === 'object') {
              newData[index][property] = inlineEditValue
            }
          }
        }
      } else if (inlineEditPath.startsWith('[') && inlineEditPath.endsWith(']')) {
        // Simple array index like [0], [1], etc.
        const index = parseInt(inlineEditPath.slice(1, -1))
        if (Array.isArray(newData) && index >= 0 && index < newData.length) {
          newData[index] = inlineEditValue
        }
      } else if (!inlineEditPath.includes('.')) {
        // Simple object property
        newData[inlineEditPath] = inlineEditValue
      } else {
        // Handle complex nested paths
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
              const isObject = typeof item === 'object' && item !== null && !Array.isArray(item)
              const isEditingThis = inlineEditPath === `[${index}]`

              return (
                <li key={index} style={{ marginBottom: 4 }}>
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
                  ) : isObject ? (
                    // Render object properties inline
                    <div style={{ marginLeft: 8 }}>
                      <Text strong style={{ fontSize: '14px', color: '#666' }}>Object {index + 1}:</Text>
                      <ul style={{ margin: '4px 0', paddingLeft: 16 }}>
                        {Object.keys(item).slice(0, 5).map(key => {
                          const value = item[key]
                          const isStringValue = typeof value === 'string'
                          const isEditingThisProperty = inlineEditPath === `[${index}].${key}`

                          return (
                            <li key={key} style={{ marginBottom: 3 }}>
                              <Text code style={{ marginRight: 6, fontSize: '14px' }}>{key}:</Text>
                              {isEditingThisProperty ? (
                                <Space size="small" onClick={(e) => e.stopPropagation()}>
                                  <Input
                                    size="small"
                                    value={inlineEditValue}
                                    onChange={(e) => setInlineEditValue(e.target.value)}
                                    onPressEnter={handleInlineStringSave}
                                    style={{ width: '90%', minWidth: 150 }}
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
                                    cursor: isStringValue && allowInlineStringEdit ? 'pointer' : 'default',
                                    padding: '2px 4px',
                                    borderRadius: '2px',
                                    background: isStringValue && allowInlineStringEdit ? '#f0f8ff' : 'transparent',
                                    border: isStringValue && allowInlineStringEdit ? '1px solid #d9d9d9' : 'none',
                                    display: 'inline-block',
                                    transition: 'all 0.2s',
                                    fontSize: '14px'
                                  }}
                                  className={isStringValue && allowInlineStringEdit ? 'editable-string' : ''}
                                  onClick={(e) => {
                                    e.stopPropagation()
                                    if (isStringValue && allowInlineStringEdit) {
                                      handleInlineStringEdit(`[${index}].${key}`, value)
                                    } else {
                                      handleFullEdit()
                                    }
                                  }}
                                  onMouseEnter={(e) => {
                                    if (isStringValue && allowInlineStringEdit) {
                                      e.currentTarget.style.background = '#e6f7ff'
                                      e.currentTarget.style.borderColor = '#1890ff'
                                    }
                                  }}
                                  onMouseLeave={(e) => {
                                    if (isStringValue && allowInlineStringEdit) {
                                      e.currentTarget.style.background = '#f0f8ff'
                                      e.currentTarget.style.borderColor = '#d9d9d9'
                                    }
                                  }}
                                >
                                  {JSON.stringify(value)}
                                  {isStringValue && allowInlineStringEdit && (
                                    <span style={{ marginLeft: 4, fontSize: '10px', color: '#1890ff' }}>✏️</span>
                                  )}
                                </Text>
                              )}
                            </li>
                          )
                        })}
                        {Object.keys(item).length > 5 && (
                          <li><Text type="secondary" style={{ fontSize: '12px' }}>... and {Object.keys(item).length - 5} more</Text></li>
                        )}
                      </ul>
                    </div>
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
          <Text strong style={{ fontSize: '14px' }}>Object ({keys.length} properties):</Text>
          <ul style={{ margin: '6px 0', paddingLeft: 16 }}>
            {keys.slice(0, 8).map(key => {
              const value = parsedData[key]
              const isString = typeof value === 'string'
              const isNumber = typeof value === 'number'
              const isArray = Array.isArray(value)
              const isEditable = (isString || isNumber) && allowInlineStringEdit
              const isEditingThis = inlineEditPath === key

              return (
                <li key={key} style={{ marginBottom: 4 }}>
                  <Text code style={{ marginRight: 6, fontSize: '14px' }}>{key}:</Text>
                  {isEditingThis ? (
                    <Space size="small" onClick={(e) => e.stopPropagation()}>
                      <Input
                        size="small"
                        value={inlineEditValue}
                        onChange={(e) => setInlineEditValue(e.target.value)}
                        onPressEnter={handleInlineStringSave}
                        style={{ width: '300px', minWidth: 250 }}
                        autoFocus
                      />
                      <Button size="small" type="primary" onClick={handleInlineStringSave}>
                        ✓
                      </Button>
                      <Button size="small" onClick={handleInlineStringCancel}>
                        ✕
                      </Button>
                    </Space>
                  ) : isArray ? (
                    // Render array items inline with edit capability
                    <div style={{ display: 'inline-block' }}>
                      <Text code style={{ fontSize: '14px', marginRight: 4 }}>[</Text>
                      {value.slice(0, 3).map((item, index) => {
                        const isStringItem = typeof item === 'string'
                        const isNumberItem = typeof item === 'number'
                        const isEditableItem = (isStringItem || isNumberItem) && allowInlineStringEdit
                        const isEditingThisItem = inlineEditPath === `${key}[${index}]`

                        return (
                          <span key={index}>
                            {isEditingThisItem ? (
                              <Space size="small" onClick={(e) => e.stopPropagation()}>
                                <Input
                                  size="small"
                                  value={inlineEditValue}
                                  onChange={(e) => setInlineEditValue(e.target.value)}
                                  onPressEnter={handleInlineStringSave}
                                  style={{ width: '200px' }}
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
                                  cursor: isEditableItem ? 'pointer' : 'default',
                                  padding: '1px 3px',
                                  borderRadius: '2px',
                                  background: isEditableItem ? '#f0f8ff' : 'transparent',
                                  border: isEditableItem ? '1px solid #d9d9d9' : 'none',
                                  display: 'inline-block',
                                  transition: 'all 0.2s',
                                  fontSize: '11px',
                                  marginRight: index < value.length - 1 ? 4 : 0
                                }}
                                className={isEditableItem ? 'editable-string' : ''}
                                onClick={(e) => {
                                  e.stopPropagation()
                                  if (isEditableItem) {
                                    handleInlineStringEdit(`${key}[${index}]`, item)
                                  } else {
                                    handleFullEdit()
                                  }
                                }}
                                onMouseEnter={(e) => {
                                  if (isEditableItem) {
                                    e.currentTarget.style.background = '#e6f7ff'
                                    e.currentTarget.style.borderColor = '#1890ff'
                                  }
                                }}
                                onMouseLeave={(e) => {
                                  if (isEditableItem) {
                                    e.currentTarget.style.background = '#f0f8ff'
                                    e.currentTarget.style.borderColor = '#d9d9d9'
                                  }
                                }}
                              >
                                {isStringItem ? `"${item}"` : item}
                                {isEditableItem && (
                                  <span style={{ marginLeft: 2, fontSize: '9px', color: '#1890ff' }}>✏️</span>
                                )}
                              </Text>
                            )}
                            {index < Math.min(value.length, 3) - 1 && <Text code style={{ fontSize: '12px', margin: '0 2px' }}>,</Text>}
                          </span>
                        )
                      })}
                      {value.length > 3 && <Text type="secondary" style={{ fontSize: '11px', marginLeft: 4 }}>...+{value.length - 3}</Text>}
                      <Text code style={{ fontSize: '12px', marginLeft: 4 }}>]</Text>
                    </div>

                  ) : (
                    <Text
                      code
                      style={{
                        cursor: isEditable ? 'pointer' : 'default',
                        padding: '2px 4px',
                        borderRadius: '2px',
                        background: isEditable ? '#f0f8ff' : 'transparent',
                        border: isEditable ? '1px solid #d9d9d9' : 'none',
                        display: 'inline-block',
                        transition: 'all 0.2s',
                        fontSize: '14px'
                      }}
                      className={isEditable ? 'editable-string' : ''}
                      onClick={(e) => {
                        e.stopPropagation()
                        if (isEditable) {
                          handleInlineStringEdit(key, value)
                        } else {
                          handleFullEdit()
                        }
                      }}
                      onMouseEnter={(e) => {
                        if (isEditable) {
                          e.currentTarget.style.background = '#e6f7ff'
                          e.currentTarget.style.borderColor = '#1890ff'
                        }
                      }}
                      onMouseLeave={(e) => {
                        if (isEditable) {
                          e.currentTarget.style.background = '#f0f8ff'
                          e.currentTarget.style.borderColor = '#d9d9d9'
                        }
                      }}
                    >
                      {JSON.stringify(value)}
                      {isEditable && (
                        <span style={{ marginLeft: 4, fontSize: '10px', color: '#1890ff' }}>✏️</span>
                      )}
                    </Text>
                  )}
                </li>
              )
            })}
            {keys.length > 8 && (
              <li><Text type="secondary" style={{ fontSize: '12px' }}>... and {keys.length - 8} more</Text></li>
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
              style={{ width: '300px', minWidth: 250 }}
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
              cursor: allowInlineStringEdit && (typeof parsedData === 'string' || typeof parsedData === 'number') ? 'pointer' : 'default',
              padding: '2px 4px',
              borderRadius: '2px',
              background: allowInlineStringEdit && (typeof parsedData === 'string' || typeof parsedData === 'number') ? '#f0f8ff' : 'transparent',
              border: allowInlineStringEdit && (typeof parsedData === 'string' || typeof parsedData === 'number') ? '1px solid #d9d9d9' : 'none',
              display: 'inline-block',
              transition: 'all 0.2s'
            }}
            onClick={(e) => {
              e.stopPropagation()
              if (allowInlineStringEdit && (typeof parsedData === 'string' || typeof parsedData === 'number')) {
                handleInlineStringEdit('value', parsedData)
              } else {
                handleFullEdit()
              }
            }}
            onMouseEnter={(e) => {
              if (allowInlineStringEdit && (typeof parsedData === 'string' || typeof parsedData === 'number')) {
                e.currentTarget.style.background = '#e6f7ff'
                e.currentTarget.style.borderColor = '#1890ff'
              }
            }}
            onMouseLeave={(e) => {
              if (allowInlineStringEdit && (typeof parsedData === 'string' || typeof parsedData === 'number')) {
                e.currentTarget.style.background = '#f0f8ff'
                e.currentTarget.style.borderColor = '#d9d9d9'
              }
            }}
          >
            {JSON.stringify(parsedData)}
            {allowInlineStringEdit && (typeof parsedData === 'string' || typeof parsedData === 'number') && (
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
