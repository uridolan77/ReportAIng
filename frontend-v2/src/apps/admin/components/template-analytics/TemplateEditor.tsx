import React, { useState, useEffect, useRef } from 'react'
import { 
  Drawer, 
  Form, 
  Input, 
  Select, 
  Button, 
  Space, 
  Typography, 
  Card,
  Row,
  Col,
  Tag,
  Alert,
  Tabs,
  Divider,
  Switch,
  message,
  Tooltip
} from 'antd'
import { 
  SaveOutlined, 
  PlayCircleOutlined, 
  EyeOutlined,
  CodeOutlined,
  FileTextOutlined,
  BugOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import Editor from '@monaco-editor/react'
import { 
  useCreateTemplateMutation,
  useUpdateTemplateMutation
} from '@shared/store/api/templateAnalyticsApi'

const { Title, Text, Paragraph } = Typography
const { TextArea } = Input

interface TemplateEditorProps {
  visible: boolean
  onClose: () => void
  template?: any
  onSave: () => void
}

export const TemplateEditor: React.FC<TemplateEditorProps> = ({
  visible,
  onClose,
  template,
  onSave
}) => {
  const [form] = Form.useForm()
  const [templateContent, setTemplateContent] = useState('')
  const [isPreviewMode, setIsPreviewMode] = useState(false)
  const [validationErrors, setValidationErrors] = useState<string[]>([])
  const [isValidating, setIsValidating] = useState(false)
  const [activeTab, setActiveTab] = useState('editor')
  const editorRef = useRef<any>(null)

  const [createTemplate] = useCreateTemplateMutation()
  const [updateTemplate] = useUpdateTemplateMutation()

  const isEditMode = !!template

  useEffect(() => {
    if (template) {
      form.setFieldsValue({
        templateName: template.templateName,
        templateKey: template.templateKey,
        intentType: template.intentType,
        description: template.description,
        tags: template.tags,
        isActive: template.isActive
      })
      setTemplateContent(template.content)
    } else {
      form.resetFields()
      setTemplateContent('')
    }
    setValidationErrors([])
    setActiveTab('editor')
  }, [template, form])

  const handleEditorDidMount = (editor: any, monaco: any) => {
    editorRef.current = editor

    // Configure Monaco for template editing
    monaco.languages.register({ id: 'template' })
    monaco.languages.setMonarchTokensProvider('template', {
      tokenizer: {
        root: [
          [/\{\{[^}]*\}\}/, 'variable'],
          [/\{%[^%]*%\}/, 'keyword'],
          [/\{#[^#]*#\}/, 'comment'],
          [/\$\{[^}]*\}/, 'variable'],
          [/@[a-zA-Z_][a-zA-Z0-9_]*/, 'type'],
        ]
      }
    })

    monaco.editor.defineTheme('templateTheme', {
      base: 'vs',
      inherit: true,
      rules: [
        { token: 'variable', foreground: '0066cc', fontStyle: 'bold' },
        { token: 'keyword', foreground: 'cc6600', fontStyle: 'italic' },
        { token: 'comment', foreground: '008000', fontStyle: 'italic' },
        { token: 'type', foreground: '800080', fontStyle: 'bold' }
      ],
      colors: {}
    })

    monaco.editor.setTheme('templateTheme')

    // Add template-specific snippets
    monaco.languages.registerCompletionItemProvider('template', {
      provideCompletionItems: () => ({
        suggestions: [
          {
            label: 'user_query',
            kind: monaco.languages.CompletionItemKind.Variable,
            insertText: '{{user_query}}',
            documentation: 'The user\'s input query'
          },
          {
            label: 'context',
            kind: monaco.languages.CompletionItemKind.Variable,
            insertText: '{{context}}',
            documentation: 'Additional context information'
          },
          {
            label: 'schema_info',
            kind: monaco.languages.CompletionItemKind.Variable,
            insertText: '{{schema_info}}',
            documentation: 'Database schema information'
          },
          {
            label: 'examples',
            kind: monaco.languages.CompletionItemKind.Variable,
            insertText: '{{examples}}',
            documentation: 'Example queries or outputs'
          }
        ]
      })
    })
  }

  const validateTemplate = async () => {
    setIsValidating(true)
    const errors: string[] = []

    try {
      // Basic syntax validation
      if (!templateContent.trim()) {
        errors.push('Template content cannot be empty')
      }

      // Check for balanced braces
      const openBraces = (templateContent.match(/\{\{/g) || []).length
      const closeBraces = (templateContent.match(/\}\}/g) || []).length
      if (openBraces !== closeBraces) {
        errors.push('Unbalanced template braces {{ }}')
      }

      // Check for required variables based on intent type
      const intentType = form.getFieldValue('intentType')
      if (intentType === 'sql_generation' && !templateContent.includes('{{user_query}}')) {
        errors.push('SQL generation templates should include {{user_query}} variable')
      }

      // Check for potentially dangerous patterns
      if (templateContent.toLowerCase().includes('drop table')) {
        errors.push('Template contains potentially dangerous SQL operations')
      }

      setValidationErrors(errors)
    } catch (error) {
      errors.push('Template validation failed')
      setValidationErrors(errors)
    } finally {
      setIsValidating(false)
    }

    return errors.length === 0
  }

  const handleSave = async () => {
    try {
      const values = await form.validateFields()
      const isValid = await validateTemplate()

      if (!isValid) {
        message.error('Please fix validation errors before saving')
        return
      }

      const templateData = {
        ...values,
        content: templateContent,
        tags: values.tags || []
      }

      if (isEditMode) {
        await updateTemplate({
          templateKey: template.templateKey,
          updates: templateData
        }).unwrap()
      } else {
        await createTemplate(templateData).unwrap()
      }

      onSave()
    } catch (error) {
      message.error('Failed to save template')
    }
  }

  const handleTest = () => {
    // Implementation for testing template with sample data
    message.info('Template testing functionality coming soon')
  }

  const getValidationIcon = () => {
    if (isValidating) return <InfoCircleOutlined spin />
    if (validationErrors.length === 0) return <CheckCircleOutlined style={{ color: '#52c41a' }} />
    return <ExclamationCircleOutlined style={{ color: '#f5222d' }} />
  }

  const templatePreview = () => {
    // Simple preview - replace variables with sample data
    let preview = templateContent
    preview = preview.replace(/\{\{user_query\}\}/g, '[User Query Here]')
    preview = preview.replace(/\{\{context\}\}/g, '[Context Information]')
    preview = preview.replace(/\{\{schema_info\}\}/g, '[Database Schema]')
    preview = preview.replace(/\{\{examples\}\}/g, '[Example Queries]')
    return preview
  }

  const editorTabs = [
    {
      key: 'editor',
      label: (
        <Space>
          <CodeOutlined />
          Template Editor
        </Space>
      ),
      children: (
        <div style={{ height: '500px' }}>
          <Editor
            height="100%"
            language="template"
            value={templateContent}
            onChange={(value) => setTemplateContent(value || '')}
            onMount={handleEditorDidMount}
            options={{
              minimap: { enabled: false },
              fontSize: 14,
              lineNumbers: 'on',
              wordWrap: 'on',
              automaticLayout: true,
              scrollBeyondLastLine: false,
              folding: true,
              renderLineHighlight: 'all',
              selectOnLineNumbers: true,
              bracketMatching: 'always',
              autoIndent: 'full',
              formatOnPaste: true,
              formatOnType: true
            }}
          />
        </div>
      )
    },
    {
      key: 'preview',
      label: (
        <Space>
          <EyeOutlined />
          Preview
        </Space>
      ),
      children: (
        <div style={{ height: '500px', overflow: 'auto' }}>
          <Card>
            <Title level={4}>Template Preview</Title>
            <pre style={{ 
              whiteSpace: 'pre-wrap', 
              fontFamily: 'monospace',
              backgroundColor: '#f5f5f5',
              padding: '16px',
              borderRadius: '4px',
              fontSize: '13px',
              lineHeight: '1.5'
            }}>
              {templatePreview()}
            </pre>
          </Card>
        </div>
      )
    },
    {
      key: 'validation',
      label: (
        <Space>
          <BugOutlined />
          Validation
          {getValidationIcon()}
        </Space>
      ),
      children: (
        <div style={{ height: '500px', overflow: 'auto' }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Button 
              type="primary" 
              onClick={validateTemplate}
              loading={isValidating}
              icon={<BugOutlined />}
            >
              Validate Template
            </Button>
            
            {validationErrors.length === 0 && !isValidating ? (
              <Alert
                message="Template Validation Passed"
                description="Your template syntax is valid and ready to use."
                type="success"
                showIcon
              />
            ) : validationErrors.length > 0 ? (
              <Alert
                message="Validation Errors Found"
                description={
                  <ul style={{ margin: 0, paddingLeft: '20px' }}>
                    {validationErrors.map((error, index) => (
                      <li key={index}>{error}</li>
                    ))}
                  </ul>
                }
                type="error"
                showIcon
              />
            ) : null}

            <Card title="Template Guidelines" size="small">
              <ul style={{ margin: 0, paddingLeft: '20px' }}>
                <li>Use <code>{`{{variable_name}}`}</code> for dynamic content</li>
                <li>Include <code>{`{{user_query}}`}</code> for user input</li>
                <li>Add <code>{`{{context}}`}</code> for additional context</li>
                <li>Use <code>{`{{schema_info}}`}</code> for database schema</li>
                <li>Avoid dangerous SQL operations like DROP, DELETE without conditions</li>
                <li>Test your template with various inputs</li>
              </ul>
            </Card>
          </Space>
        </div>
      )
    }
  ]

  return (
    <Drawer
      title={
        <Space>
          <FileTextOutlined />
          {isEditMode ? `Edit Template: ${template?.templateName}` : 'Create New Template'}
        </Space>
      }
      width="80%"
      open={visible}
      onClose={onClose}
      extra={
        <Space>
          <Button onClick={handleTest} icon={<PlayCircleOutlined />}>
            Test
          </Button>
          <Button type="primary" onClick={handleSave} icon={<SaveOutlined />}>
            {isEditMode ? 'Update' : 'Create'}
          </Button>
        </Space>
      }
    >
      <Row gutter={16}>
        {/* Left Panel - Form */}
        <Col span={8}>
          <Card title="Template Information" size="small">
            <Form form={form} layout="vertical">
              <Form.Item
                name="templateName"
                label="Template Name"
                rules={[{ required: true, message: 'Please enter template name' }]}
              >
                <Input placeholder="Enter descriptive template name" />
              </Form.Item>

              <Form.Item
                name="templateKey"
                label="Template Key"
                rules={[
                  { required: true, message: 'Please enter template key' },
                  { pattern: /^[a-z0-9_]+$/, message: 'Only lowercase letters, numbers, and underscores allowed' }
                ]}
              >
                <Input 
                  placeholder="template_key" 
                  disabled={isEditMode}
                />
              </Form.Item>

              <Form.Item
                name="intentType"
                label="Intent Type"
                rules={[{ required: true, message: 'Please select intent type' }]}
              >
                <Select placeholder="Select intent type">
                  <Select.Option value="sql_generation">SQL Generation</Select.Option>
                  <Select.Option value="insight_generation">Insight Generation</Select.Option>
                  <Select.Option value="explanation">Explanation</Select.Option>
                  <Select.Option value="data_analysis">Data Analysis</Select.Option>
                  <Select.Option value="report_generation">Report Generation</Select.Option>
                </Select>
              </Form.Item>

              <Form.Item
                name="description"
                label="Description"
              >
                <TextArea 
                  rows={3} 
                  placeholder="Describe what this template does..."
                />
              </Form.Item>

              <Form.Item
                name="tags"
                label="Tags"
              >
                <Select
                  mode="tags"
                  placeholder="Add tags for categorization"
                  style={{ width: '100%' }}
                >
                  <Select.Option value="sql">SQL</Select.Option>
                  <Select.Option value="analytics">Analytics</Select.Option>
                  <Select.Option value="reporting">Reporting</Select.Option>
                  <Select.Option value="business">Business</Select.Option>
                </Select>
              </Form.Item>

              <Form.Item
                name="isActive"
                label="Status"
                valuePropName="checked"
                initialValue={true}
              >
                <Switch 
                  checkedChildren="Active" 
                  unCheckedChildren="Inactive"
                />
              </Form.Item>
            </Form>
          </Card>
        </Col>

        {/* Right Panel - Editor */}
        <Col span={16}>
          <Card title="Template Content" size="small">
            <Tabs
              activeKey={activeTab}
              onChange={setActiveTab}
              items={editorTabs}
            />
          </Card>
        </Col>
      </Row>
    </Drawer>
  )
}

export default TemplateEditor
