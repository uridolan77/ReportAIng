import React, { useState, useEffect, useCallback } from 'react'
import { debounce } from 'lodash'
import {
  Alert,
  Space,
  Typography,
  Tag,
  Tooltip,
  Button,
  Popover,
  List,
  Badge,
} from 'antd'
import {
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  LoadingOutlined,
  BulbOutlined,
  WarningOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons'
import {
  useValidateEnhancedBusinessTableMutation,
  type BusinessTableValidationRequest,
  type ValidationIssue,
  type ValidationWarning,
  type ValidationSuggestion,
} from '@shared/store/api/businessApi'

const { Text } = Typography

interface RealTimeValidationProps {
  tableData: any
  fieldName?: string
  validationRules?: string[]
  onValidationChange?: (isValid: boolean, issues: ValidationIssue[], warnings: ValidationWarning[]) => void
  debounceMs?: number
  showInline?: boolean
  showSuggestions?: boolean
}

export const RealTimeValidation: React.FC<RealTimeValidationProps> = ({
  tableData,
  fieldName,
  validationRules = [],
  onValidationChange,
  debounceMs = 1000,
  showInline = true,
  showSuggestions = true,
}) => {
  const [validationState, setValidationState] = useState<{
    isValidating: boolean
    isValid: boolean
    issues: ValidationIssue[]
    warnings: ValidationWarning[]
    suggestions: ValidationSuggestion[]
  }>({
    isValidating: false,
    isValid: true,
    issues: [],
    warnings: [],
    suggestions: [],
  })

  const [validateTable] = useValidateEnhancedBusinessTableMutation()

  // Debounced validation function
  const debouncedValidate = useCallback(
    debounce(async (data: any) => {
      setValidationState(prev => ({ ...prev, isValidating: true }))

      try {
        const request: BusinessTableValidationRequest = {
          tableId: data.id,
          validateBusinessRules: true,
          validateDataQuality: true,
          validateRelationships: false, // Skip for real-time to improve performance
        }

        const result = await validateTable(request).unwrap()
        const { isValid, issues, warnings, suggestions } = result.data

        // Filter issues and warnings for specific field if provided
        const filteredIssues = fieldName 
          ? issues.filter((issue: ValidationIssue) => issue.field === fieldName)
          : issues

        const filteredWarnings = fieldName
          ? warnings.filter((warning: ValidationWarning) => warning.field === fieldName)
          : warnings

        const newState = {
          isValidating: false,
          isValid: filteredIssues.length === 0,
          issues: filteredIssues,
          warnings: filteredWarnings,
          suggestions: suggestions || [],
        }

        setValidationState(newState)
        onValidationChange?.(newState.isValid, filteredIssues, filteredWarnings)
      } catch (error) {
        console.error('Real-time validation failed:', error)
        setValidationState(prev => ({ ...prev, isValidating: false }))
      }
    }, debounceMs),
    [validateTable, fieldName, onValidationChange, debounceMs]
  )

  // Trigger validation when table data changes
  useEffect(() => {
    if (tableData && (tableData.id || (tableData.schemaName && tableData.tableName))) {
      debouncedValidate(tableData)
    }

    // Cleanup debounced function on unmount
    return () => {
      debouncedValidate.cancel()
    }
  }, [tableData, debouncedValidate])

  // Client-side validation for immediate feedback
  const performClientSideValidation = useCallback((data: any, field?: string) => {
    const clientIssues: ValidationIssue[] = []
    const clientWarnings: ValidationWarning[] = []

    if (field) {
      const value = data[field]
      
      // Required field validation
      if (!value || (typeof value === 'string' && value.trim() === '')) {
        if (['businessPurpose', 'businessContext', 'primaryUseCase'].includes(field)) {
          clientIssues.push({
            type: 'RequiredField',
            message: `${field} is required`,
            severity: 'Error',
            field,
          })
        }
      }

      // Length validation
      if (typeof value === 'string') {
        if (value.length > 1000) {
          clientWarnings.push({
            type: 'LengthWarning',
            message: `${field} is very long (${value.length} characters)`,
            field,
          })
        }
        if (value.length < 10 && ['businessPurpose', 'businessContext'].includes(field)) {
          clientWarnings.push({
            type: 'LengthWarning',
            message: `${field} seems too short for meaningful description`,
            field,
          })
        }
      }

      // Array validation
      if (Array.isArray(value)) {
        if (value.length === 0 && ['naturalLanguageAliases', 'businessProcesses'].includes(field)) {
          clientWarnings.push({
            type: 'EmptyArray',
            message: `Consider adding ${field} for better searchability`,
            field,
          })
        }
      }
    }

    return { issues: clientIssues, warnings: clientWarnings }
  }, [])

  // Immediate client-side validation
  useEffect(() => {
    if (fieldName && tableData) {
      const { issues, warnings } = performClientSideValidation(tableData, fieldName)
      if (issues.length > 0 || warnings.length > 0) {
        setValidationState(prev => ({
          ...prev,
          issues: [...prev.issues, ...issues],
          warnings: [...prev.warnings, ...warnings],
          isValid: prev.isValid && issues.length === 0,
        }))
      }
    }
  }, [tableData, fieldName, performClientSideValidation])

  const getValidationIcon = () => {
    if (validationState.isValidating) {
      return <LoadingOutlined style={{ color: '#1890ff' }} />
    }
    if (validationState.issues.length > 0) {
      return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
    }
    if (validationState.warnings.length > 0) {
      return <ExclamationCircleOutlined style={{ color: '#faad14' }} />
    }
    return <CheckCircleOutlined style={{ color: '#52c41a' }} />
  }

  const getValidationMessage = () => {
    if (validationState.isValidating) {
      return 'Validating...'
    }
    if (validationState.issues.length > 0) {
      return `${validationState.issues.length} issue(s) found`
    }
    if (validationState.warnings.length > 0) {
      return `${validationState.warnings.length} warning(s)`
    }
    return 'Valid'
  }

  const renderValidationDetails = () => (
    <div style={{ maxWidth: 300 }}>
      {validationState.issues.length > 0 && (
        <div style={{ marginBottom: 8 }}>
          <Text strong style={{ color: '#ff4d4f' }}>Issues:</Text>
          <List
            size="small"
            dataSource={validationState.issues}
            renderItem={(issue) => (
              <List.Item style={{ padding: '4px 0' }}>
                <Space>
                  <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
                  <Text>{issue.message}</Text>
                </Space>
              </List.Item>
            )}
          />
        </div>
      )}

      {validationState.warnings.length > 0 && (
        <div style={{ marginBottom: 8 }}>
          <Text strong style={{ color: '#faad14' }}>Warnings:</Text>
          <List
            size="small"
            dataSource={validationState.warnings}
            renderItem={(warning) => (
              <List.Item style={{ padding: '4px 0' }}>
                <Space>
                  <WarningOutlined style={{ color: '#faad14' }} />
                  <Text>{warning.message}</Text>
                </Space>
              </List.Item>
            )}
          />
        </div>
      )}

      {showSuggestions && validationState.suggestions.length > 0 && (
        <div>
          <Text strong style={{ color: '#1890ff' }}>Suggestions:</Text>
          <List
            size="small"
            dataSource={validationState.suggestions}
            renderItem={(suggestion) => (
              <List.Item style={{ padding: '4px 0' }}>
                <Space>
                  <BulbOutlined style={{ color: '#1890ff' }} />
                  <Text>{suggestion.message}</Text>
                </Space>
              </List.Item>
            )}
          />
        </div>
      )}
    </div>
  )

  if (!showInline) {
    return null
  }

  const hasIssuesOrWarnings = validationState.issues.length > 0 || validationState.warnings.length > 0

  return (
    <Space>
      {hasIssuesOrWarnings || validationState.isValidating ? (
        <Popover
          content={renderValidationDetails()}
          title="Validation Details"
          trigger="hover"
          placement="topLeft"
        >
          <Badge
            count={validationState.issues.length + validationState.warnings.length}
            size="small"
            offset={[5, -5]}
          >
            <Button
              type="text"
              size="small"
              icon={getValidationIcon()}
              style={{
                color: validationState.issues.length > 0 ? '#ff4d4f' : 
                       validationState.warnings.length > 0 ? '#faad14' : '#52c41a'
              }}
            >
              {getValidationMessage()}
            </Button>
          </Badge>
        </Popover>
      ) : (
        <Tooltip title="Validation passed">
          <Tag color="success" icon={<CheckCircleOutlined />}>
            Valid
          </Tag>
        </Tooltip>
      )}
    </Space>
  )
}

// Validation status indicator for form fields
export const ValidationStatus: React.FC<{
  isValid: boolean
  isValidating: boolean
  issues: ValidationIssue[]
  warnings: ValidationWarning[]
}> = ({ isValid, isValidating, issues, warnings }) => {
  if (isValidating) {
    return (
      <Space>
        <LoadingOutlined style={{ color: '#1890ff' }} />
        <Text type="secondary">Validating...</Text>
      </Space>
    )
  }

  if (!isValid && issues.length > 0) {
    return (
      <Space>
        <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
        <Text type="danger">{issues.length} error(s)</Text>
      </Space>
    )
  }

  if (warnings.length > 0) {
    return (
      <Space>
        <ExclamationCircleOutlined style={{ color: '#faad14' }} />
        <Text style={{ color: '#faad14' }}>{warnings.length} warning(s)</Text>
      </Space>
    )
  }

  return (
    <Space>
      <CheckCircleOutlined style={{ color: '#52c41a' }} />
      <Text type="success">Valid</Text>
    </Space>
  )
}

export default RealTimeValidation
