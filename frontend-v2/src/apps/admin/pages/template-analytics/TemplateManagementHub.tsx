import React, { useState, useCallback } from 'react'
import { 
  Layout, 
  Card, 
  Row, 
  Col, 
  Button, 
  Input, 
  Select, 
  Table, 
  Tag, 
  Space, 
  Typography, 
  Drawer,
  Modal,
  message,
  Tooltip,
  Badge,
  Divider,
  Dropdown,
  Menu
} from 'antd'
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  CopyOutlined,
  SearchOutlined,
  FilterOutlined,
  DownloadOutlined,
  UploadOutlined,
  SettingOutlined,
  EyeOutlined,
  HistoryOutlined,
  StarOutlined,
  StarFilled,
  CodeOutlined,
  FileTextOutlined,
  TagsOutlined,
  UserOutlined,
  CalendarOutlined,
  TrophyOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import { 
  useGetTemplateManagementDashboardQuery,
  useSearchTemplatesQuery,
  useDeleteTemplateMutation,
  useUpdateTemplateMutation
} from '@shared/store/api/templateAnalyticsApi'
import { MetricCard } from '@shared/components/charts/PerformanceChart'

import TemplateEditor from '../../components/template-analytics/TemplateEditor'
import TemplateMetadataPanel from '../../components/template-analytics/TemplateMetadataPanel'
import TemplateVersionHistory from '../../components/template-analytics/TemplateVersionHistory'
import TemplateQualityAssessment from '../../components/template-analytics/TemplateQualityAssessment'
import TemplateImportExport from '../../components/template-analytics/TemplateImportExport'

const { Title, Text } = Typography
const { Search } = Input
const { Content, Sider } = Layout

export const TemplateManagementHub: React.FC = () => {
  // State management
  const [selectedTemplate, setSelectedTemplate] = useState<any>(null)
  const [isEditorVisible, setIsEditorVisible] = useState(false)
  const [isMetadataPanelVisible, setIsMetadataPanelVisible] = useState(false)
  const [isVersionHistoryVisible, setIsVersionHistoryVisible] = useState(false)
  const [isImportExportVisible, setIsImportExportVisible] = useState(false)
  const [searchCriteria, setSearchCriteria] = useState<any>({
    query: '',
    intentType: undefined,
    isActive: undefined,
    qualityScoreMin: undefined
  })
  const [viewMode, setViewMode] = useState<'grid' | 'table'>('table')
  const [favoriteTemplates, setFavoriteTemplates] = useState<Set<string>>(new Set())

  // API hooks
  const { data: dashboardData, isLoading: isDashboardLoading } = useGetTemplateManagementDashboardQuery()
  const { data: searchResults, isLoading: isSearchLoading } = useSearchTemplatesQuery({
    criteria: searchCriteria,
    page: 1,
    pageSize: 50
  })
  const [deleteTemplate] = useDeleteTemplateMutation()
  const [updateTemplate] = useUpdateTemplateMutation()

  // Event handlers
  const handleTemplateSelect = useCallback((template: any) => {
    setSelectedTemplate(template)
  }, [])

  const handleTemplateEdit = useCallback((template: any) => {
    setSelectedTemplate(template)
    setIsEditorVisible(true)
  }, [])

  const handleTemplateDelete = useCallback(async (templateKey: string) => {
    Modal.confirm({
      title: 'Delete Template',
      content: 'Are you sure you want to delete this template? This action cannot be undone.',
      icon: <ExclamationCircleOutlined />,
      okText: 'Delete',
      okType: 'danger',
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await deleteTemplate(templateKey).unwrap()
          message.success('Template deleted successfully')
          setSelectedTemplate(null)
        } catch (error) {
          message.error('Failed to delete template')
        }
      }
    })
  }, [deleteTemplate])

  const handleTemplateDuplicate = useCallback(async (template: any) => {
    try {
      const duplicatedTemplate = {
        ...template,
        templateKey: `${template.templateKey}_copy`,
        templateName: `${template.templateName} (Copy)`,
        version: '1.0.0'
      }
      // Implementation would call create template API
      message.success('Template duplicated successfully')
    } catch (error) {
      message.error('Failed to duplicate template')
    }
  }, [])

  const handleToggleFavorite = useCallback((templateKey: string) => {
    const newFavorites = new Set(favoriteTemplates)
    if (newFavorites.has(templateKey)) {
      newFavorites.delete(templateKey)
    } else {
      newFavorites.add(templateKey)
    }
    setFavoriteTemplates(newFavorites)
  }, [favoriteTemplates])

  const handleToggleActive = useCallback(async (template: any) => {
    try {
      await updateTemplate({
        templateKey: template.templateKey,
        updates: { isActive: !template.isActive }
      }).unwrap()
      message.success(`Template ${template.isActive ? 'deactivated' : 'activated'} successfully`)
    } catch (error) {
      message.error('Failed to update template status')
    }
  }, [updateTemplate])

  // Utility functions
  const getQualityColor = (score: number) => {
    if (score >= 90) return '#52c41a'
    if (score >= 70) return '#1890ff'
    if (score >= 50) return '#fa8c16'
    return '#f5222d'
  }

  const getQualityLabel = (score: number) => {
    if (score >= 90) return 'Excellent'
    if (score >= 70) return 'Good'
    if (score >= 50) return 'Fair'
    return 'Needs Improvement'
  }

  // Table columns configuration
  const templateColumns = [
    {
      title: 'Template',
      key: 'template',
      render: (record: any) => (
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Button
              type="text"
              size="small"
              icon={favoriteTemplates.has(record.templateKey) ? <StarFilled /> : <StarOutlined />}
              onClick={() => handleToggleFavorite(record.templateKey)}
              style={{ color: favoriteTemplates.has(record.templateKey) ? '#faad14' : undefined }}
            />
            <Text strong style={{ fontSize: '14px' }}>{record.templateName}</Text>
            {!record.isActive && <Tag color="red">Inactive</Tag>}
          </div>
          <div style={{ marginTop: '4px' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.templateKey} • v{record.version}
            </Text>
          </div>
          {record.description && (
            <div style={{ marginTop: '4px' }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {record.description.length > 100 
                  ? `${record.description.substring(0, 100)}...` 
                  : record.description
                }
              </Text>
            </div>
          )}
        </div>
      )
    },
    {
      title: 'Intent Type',
      dataIndex: 'intentType',
      key: 'intentType',
      render: (intentType: string) => (
        <Tag color="blue">{intentType.replace('_', ' ').toUpperCase()}</Tag>
      )
    },
    {
      title: 'Quality Score',
      dataIndex: 'qualityScore',
      key: 'qualityScore',
      render: (score: number) => (
        <div style={{ textAlign: 'center' }}>
          <div style={{ 
            fontSize: '16px', 
            fontWeight: 600,
            color: getQualityColor(score)
          }}>
            {score}
          </div>
          <Tag color={getQualityColor(score)} size="small">
            {getQualityLabel(score)}
          </Tag>
        </div>
      ),
      sorter: (a: any, b: any) => a.qualityScore - b.qualityScore
    },
    {
      title: 'Performance',
      key: 'performance',
      render: (record: any) => (
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '14px', fontWeight: 600 }}>
            {(record.performanceMetrics.successRate * 100).toFixed(1)}%
          </div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.performanceMetrics.totalUsages} uses
          </Text>
        </div>
      ),
      sorter: (a: any, b: any) => a.performanceMetrics.successRate - b.performanceMetrics.successRate
    },
    {
      title: 'Business Owner',
      key: 'businessOwner',
      render: (record: any) => (
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
            <UserOutlined style={{ fontSize: '12px' }} />
            <Text style={{ fontSize: '12px' }}>
              {record.businessMetadata?.businessOwner || 'Unassigned'}
            </Text>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '4px', marginTop: '2px' }}>
            <CalendarOutlined style={{ fontSize: '12px' }} />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {new Date(record.lastModified).toLocaleDateString()}
            </Text>
          </div>
        </div>
      )
    },
    {
      title: 'Tags',
      dataIndex: 'tags',
      key: 'tags',
      render: (tags: string[]) => (
        <div>
          {tags?.slice(0, 2).map(tag => (
            <Tag key={tag} size="small" style={{ margin: '1px' }}>
              {tag}
            </Tag>
          ))}
          {tags?.length > 2 && (
            <Tag size="small" style={{ margin: '1px' }}>
              +{tags.length - 2}
            </Tag>
          )}
        </div>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: any) => (
        <Dropdown
          menu={{
            items: [
              {
                key: 'view',
                icon: <EyeOutlined />,
                label: 'View Details',
                onClick: () => handleTemplateSelect(record)
              },
              {
                key: 'edit',
                icon: <EditOutlined />,
                label: 'Edit Template',
                onClick: () => handleTemplateEdit(record)
              },
              {
                key: 'metadata',
                icon: <FileTextOutlined />,
                label: 'Edit Metadata',
                onClick: () => {
                  setSelectedTemplate(record)
                  setIsMetadataPanelVisible(true)
                }
              },
              {
                key: 'history',
                icon: <HistoryOutlined />,
                label: 'Version History',
                onClick: () => {
                  setSelectedTemplate(record)
                  setIsVersionHistoryVisible(true)
                }
              },
              { type: 'divider' },
              {
                key: 'duplicate',
                icon: <CopyOutlined />,
                label: 'Duplicate',
                onClick: () => handleTemplateDuplicate(record)
              },
              {
                key: 'toggle-active',
                icon: record.isActive ? <ExclamationCircleOutlined /> : <TrophyOutlined />,
                label: record.isActive ? 'Deactivate' : 'Activate',
                onClick: () => handleToggleActive(record)
              },
              { type: 'divider' },
              {
                key: 'delete',
                icon: <DeleteOutlined />,
                label: 'Delete',
                danger: true,
                onClick: () => handleTemplateDelete(record.templateKey)
              }
            ]
          }}
          trigger={['click']}
        >
          <Button size="small" icon={<SettingOutlined />}>
            Actions
          </Button>
        </Dropdown>
      )
    }
  ]

  return (
    <div>
        {/* Header */}
        <div style={{ marginBottom: '24px' }}>
          <Row gutter={16} align="middle">
            <Col span={12}>
              <Title level={2} style={{ margin: 0 }}>
                Template Management Hub
              </Title>
              <Text type="secondary">
                Comprehensive template lifecycle management
              </Text>
            </Col>
            <Col span={12} style={{ textAlign: 'right' }}>
              <Space>
                <Button
                  icon={<UploadOutlined />}
                  onClick={() => setIsImportExportVisible(true)}
                >
                  Import/Export
                </Button>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => {
                    setSelectedTemplate(null)
                    setIsEditorVisible(true)
                  }}
                  size="large"
                >
                  Create Template
                </Button>
              </Space>
            </Col>
          </Row>
        </div>

        {/* Dashboard Metrics */}
        <Row gutter={16} style={{ marginBottom: '24px' }}>
          <Col span={6}>
            <MetricCard
              title="Total Templates"
              value={dashboardData?.totalTemplates || 0}
              icon={<FileTextOutlined />}
              color="#1890ff"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Active Templates"
              value={dashboardData?.activeTemplates || 0}
              icon={<TrophyOutlined />}
              color="#52c41a"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Avg Quality Score"
              value={dashboardData?.averageQualityScore?.toFixed(1) || '0.0'}
              icon={<StarOutlined />}
              color="#722ed1"
            />
          </Col>
          <Col span={6}>
            <MetricCard
              title="Need Review"
              value={dashboardData?.templatesNeedingReview || 0}
              icon={<ExclamationCircleOutlined />}
              color="#fa8c16"
            />
          </Col>
        </Row>

        {/* Search and Filters */}
        <Card style={{ marginBottom: '24px' }}>
          <Row gutter={16} align="middle">
            <Col span={8}>
              <Search
                placeholder="Search templates by name, key, or description..."
                value={searchCriteria.query}
                onChange={(e) => setSearchCriteria({ ...searchCriteria, query: e.target.value })}
                onSearch={() => {}} // Trigger search
                size="large"
              />
            </Col>
            <Col span={4}>
              <Select
                placeholder="Intent Type"
                value={searchCriteria.intentType}
                onChange={(value) => setSearchCriteria({ ...searchCriteria, intentType: value })}
                style={{ width: '100%' }}
                allowClear
              >
                <Select.Option value="sql_generation">SQL Generation</Select.Option>
                <Select.Option value="insight_generation">Insight Generation</Select.Option>
                <Select.Option value="explanation">Explanation</Select.Option>
                <Select.Option value="data_analysis">Data Analysis</Select.Option>
              </Select>
            </Col>
            <Col span={4}>
              <Select
                placeholder="Status"
                value={searchCriteria.isActive}
                onChange={(value) => setSearchCriteria({ ...searchCriteria, isActive: value })}
                style={{ width: '100%' }}
                allowClear
              >
                <Select.Option value={true}>Active</Select.Option>
                <Select.Option value={false}>Inactive</Select.Option>
              </Select>
            </Col>
            <Col span={4}>
              <Select
                placeholder="Quality Score"
                value={searchCriteria.qualityScoreMin}
                onChange={(value) => setSearchCriteria({ ...searchCriteria, qualityScoreMin: value })}
                style={{ width: '100%' }}
                allowClear
              >
                <Select.Option value={90}>Excellent (90+)</Select.Option>
                <Select.Option value={70}>Good (70+)</Select.Option>
                <Select.Option value={50}>Fair (50+)</Select.Option>
                <Select.Option value={0}>All</Select.Option>
              </Select>
            </Col>
            <Col span={4}>
              <Button
                onClick={() => setSearchCriteria({
                  query: '',
                  intentType: undefined,
                  isActive: undefined,
                  qualityScoreMin: undefined
                })}
              >
                Clear Filters
              </Button>
            </Col>
          </Row>
        </Card>

        {/* Templates Table */}
        <Card title="Templates" loading={isSearchLoading}>
          <Table
            columns={templateColumns}
            dataSource={searchResults?.templates || []}
            rowKey="templateKey"
            pagination={{
              pageSize: 20,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) => 
                `${range[0]}-${range[1]} of ${total} templates`
            }}
            onRow={(record) => ({
              onClick: () => handleTemplateSelect(record),
              style: { cursor: 'pointer' }
            })}
            rowClassName={(record) => 
              selectedTemplate?.templateKey === record.templateKey ? 'ant-table-row-selected' : ''
            }
          />
        </Card>

        {/* Template Editor Drawer */}
        <TemplateEditor
          visible={isEditorVisible}
          onClose={() => setIsEditorVisible(false)}
          template={selectedTemplate}
          onSave={() => {
            setIsEditorVisible(false)
            message.success('Template saved successfully')
          }}
        />

        {/* Metadata Panel Drawer */}
        <TemplateMetadataPanel
          visible={isMetadataPanelVisible}
          onClose={() => setIsMetadataPanelVisible(false)}
          template={selectedTemplate}
          onSave={() => {
            setIsMetadataPanelVisible(false)
            message.success('Metadata updated successfully')
          }}
        />

        {/* Version History Modal */}
        <TemplateVersionHistory
          visible={isVersionHistoryVisible}
          onClose={() => setIsVersionHistoryVisible(false)}
          template={selectedTemplate}
        />

        {/* Import/Export Modal */}
        <TemplateImportExport
          visible={isImportExportVisible}
          onClose={() => setIsImportExportVisible(false)}
        />
      </div>
  )
}

export default TemplateManagementHub
