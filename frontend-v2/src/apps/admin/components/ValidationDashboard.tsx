import React, { useState, useEffect } from 'react'
import {
  Card,
  Row,
  Col,
  Progress,
  Tag,
  Button,
  Space,
  Typography,
  Alert,
  List,
  Tooltip,
  Badge,
  Statistic,
  Modal,
  Form,
  Select,
  Switch,
  Divider,
} from 'antd'
import {
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  BulbOutlined,
  ReloadOutlined,
  SettingOutlined,
  PlayCircleOutlined,
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

const { Title, Text } = Typography

interface ValidationDashboardProps {
  tableId?: number
  schemaName?: string
  tableName?: string
  onValidationComplete?: (result: any) => void
}

export const ValidationDashboard: React.FC<ValidationDashboardProps> = ({
  tableId,
  schemaName,
  tableName,
  onValidationComplete,
}) => {
  const [validationResult, setValidationResult] = useState<any>(null)
  const [settingsVisible, setSettingsVisible] = useState(false)
  const [validationSettings, setValidationSettings] = useState({
    validateBusinessRules: true,
    validateDataQuality: true,
    validateRelationships: true,
  })

  const [validateTable, { isLoading: validating }] = useValidateEnhancedBusinessTableMutation()

  const handleValidation = async () => {
    try {
      const request: BusinessTableValidationRequest = {
        ...(tableId && { tableId }),
        ...(schemaName && { schemaName }),
        ...(tableName && { tableName }),
        ...validationSettings,
      }

      const result = await validateTable(request).unwrap()
      setValidationResult(result.data)
      onValidationComplete?.(result)
    } catch (error) {
      console.error('Validation failed:', error)
    }
  }

  useEffect(() => {
    if (tableId || (schemaName && tableName)) {
      handleValidation()
    }
  }, [tableId, schemaName, tableName])

  const getValidationStatus = () => {
    if (!validationResult) return { status: 'default', text: 'Not Validated', color: '#d9d9d9' }
    
    const { isValid, issues, warnings } = validationResult
    
    if (isValid && warnings.length === 0) {
      return { status: 'success', text: 'Excellent', color: '#52c41a' }
    } else if (isValid && warnings.length > 0) {
      return { status: 'warning', text: 'Good', color: '#faad14' }
    } else if (issues.filter((i: ValidationIssue) => i.severity === 'Error').length > 0) {
      return { status: 'error', text: 'Needs Attention', color: '#ff4d4f' }
    } else {
      return { status: 'warning', text: 'Fair', color: '#faad14' }
    }
  }

  const getQualityScore = () => {
    if (!validationResult) return 0
    
    const { issues, warnings } = validationResult
    const errorCount = issues.filter((i: ValidationIssue) => i.severity === 'Error').length
    const warningCount = warnings.length
    
    // Calculate score based on issues (100 - penalties)
    const errorPenalty = errorCount * 20
    const warningPenalty = warningCount * 5
    
    return Math.max(0, 100 - errorPenalty - warningPenalty)
  }

  const validationStatus = getValidationStatus()
  const qualityScore = getQualityScore()

  const renderIssueIcon = (severity: string) => {
    switch (severity) {
      case 'Error':
        return <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
      case 'Warning':
        return <ExclamationCircleOutlined style={{ color: '#faad14' }} />
      default:
        return <InfoCircleOutlined style={{ color: '#1890ff' }} />
    }
  }

  const renderValidationSettings = () => (
    <Modal
      title="Validation Settings"
      open={settingsVisible}
      onOk={() => {
        setSettingsVisible(false)
        handleValidation()
      }}
      onCancel={() => setSettingsVisible(false)}
      okText="Apply & Validate"
    >
      <Form layout="vertical">
        <Form.Item label="Validation Options">
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Switch
                checked={validationSettings.validateBusinessRules}
                onChange={(checked) =>
                  setValidationSettings(prev => ({ ...prev, validateBusinessRules: checked }))
                }
              />
              <span style={{ marginLeft: 8 }}>Validate Business Rules</span>
            </div>
            <div>
              <Switch
                checked={validationSettings.validateDataQuality}
                onChange={(checked) =>
                  setValidationSettings(prev => ({ ...prev, validateDataQuality: checked }))
                }
              />
              <span style={{ marginLeft: 8 }}>Validate Data Quality</span>
            </div>
            <div>
              <Switch
                checked={validationSettings.validateRelationships}
                onChange={(checked) =>
                  setValidationSettings(prev => ({ ...prev, validateRelationships: checked }))
                }
              />
              <span style={{ marginLeft: 8 }}>Validate Relationships</span>
            </div>
          </Space>
        </Form.Item>
      </Form>
    </Modal>
  )

  return (
    <div>
      {/* Validation Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Title level={4} style={{ margin: 0 }}>
                Validation Status
              </Title>
              <Tag color={validationStatus.color} icon={
                validationStatus.status === 'success' ? <CheckCircleOutlined /> :
                validationStatus.status === 'error' ? <CloseCircleOutlined /> :
                <ExclamationCircleOutlined />
              }>
                {validationStatus.text}
              </Tag>
            </Space>
          </Col>
          <Col>
            <Space>
              <Button
                icon={<SettingOutlined />}
                onClick={() => setSettingsVisible(true)}
              >
                Settings
              </Button>
              <Button
                type="primary"
                icon={<ReloadOutlined />}
                loading={validating}
                onClick={handleValidation}
              >
                Re-validate
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Quality Metrics */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={8}>
          <Card>
            <Statistic
              title="Quality Score"
              value={qualityScore}
              suffix="/ 100"
              valueStyle={{ color: qualityScore >= 80 ? '#3f8600' : qualityScore >= 60 ? '#faad14' : '#cf1322' }}
            />
            <Progress
              percent={qualityScore}
              strokeColor={qualityScore >= 80 ? '#52c41a' : qualityScore >= 60 ? '#faad14' : '#ff4d4f'}
              showInfo={false}
              style={{ marginTop: 8 }}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Issues Found"
              value={validationResult?.issues?.length || 0}
              prefix={<CloseCircleOutlined />}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Warnings"
              value={validationResult?.warnings?.length || 0}
              prefix={<WarningOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Validation Results */}
      {validationResult && (
        <Row gutter={16}>
          {/* Issues */}
          {validationResult.issues && validationResult.issues.length > 0 && (
            <Col span={8}>
              <Card
                title={
                  <Space>
                    <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
                    <span>Issues ({validationResult.issues.length})</span>
                  </Space>
                }
                size="small"
              >
                <List
                  size="small"
                  dataSource={validationResult.issues}
                  renderItem={(issue: ValidationIssue) => (
                    <List.Item>
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Space>
                          {renderIssueIcon(issue.severity)}
                          <Text strong>{issue.type}</Text>
                        </Space>
                        <Text>{issue.message}</Text>
                        {issue.field && (
                          <Tag size="small" color="red">
                            Field: {issue.field}
                          </Tag>
                        )}
                      </Space>
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          )}

          {/* Warnings */}
          {validationResult.warnings && validationResult.warnings.length > 0 && (
            <Col span={8}>
              <Card
                title={
                  <Space>
                    <WarningOutlined style={{ color: '#faad14' }} />
                    <span>Warnings ({validationResult.warnings.length})</span>
                  </Space>
                }
                size="small"
              >
                <List
                  size="small"
                  dataSource={validationResult.warnings}
                  renderItem={(warning: ValidationWarning) => (
                    <List.Item>
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Space>
                          <ExclamationCircleOutlined style={{ color: '#faad14' }} />
                          <Text strong>{warning.type}</Text>
                        </Space>
                        <Text>{warning.message}</Text>
                        {warning.field && (
                          <Tag size="small" color="orange">
                            Field: {warning.field}
                          </Tag>
                        )}
                      </Space>
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          )}

          {/* Suggestions */}
          {validationResult.suggestions && validationResult.suggestions.length > 0 && (
            <Col span={8}>
              <Card
                title={
                  <Space>
                    <BulbOutlined style={{ color: '#1890ff' }} />
                    <span>Suggestions ({validationResult.suggestions.length})</span>
                  </Space>
                }
                size="small"
              >
                <List
                  size="small"
                  dataSource={validationResult.suggestions}
                  renderItem={(suggestion: ValidationSuggestion) => (
                    <List.Item>
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Space>
                          <BulbOutlined style={{ color: '#1890ff' }} />
                          <Text strong>{suggestion.type}</Text>
                        </Space>
                        <Text>{suggestion.message}</Text>
                        <Button
                          size="small"
                          type="link"
                          icon={<PlayCircleOutlined />}
                        >
                          {suggestion.recommendedAction}
                        </Button>
                      </Space>
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          )}
        </Row>
      )}

      {/* No Issues State */}
      {validationResult && validationResult.isValid && 
       (!validationResult.warnings || validationResult.warnings.length === 0) && (
        <Alert
          message="Validation Passed"
          description="This table metadata meets all validation criteria. No issues or warnings found."
          type="success"
          icon={<CheckCircleOutlined />}
          showIcon
        />
      )}

      {/* Validation Settings Modal */}
      {renderValidationSettings()}
    </div>
  )
}

export default ValidationDashboard
