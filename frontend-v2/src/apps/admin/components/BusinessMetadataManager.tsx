import React, { useState } from 'react'
import {
  Card,
  Row,
  Col,
  Button,
  Progress,
  Typography,
  Table,
  Space,
  Tag,
  Modal,
  Form,
  Input,
  Switch,
  Alert,
  Tooltip,
  Statistic,
  Divider
} from 'antd'
import {
  DatabaseOutlined,
  PlayCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  ReloadOutlined,
  SettingOutlined,
  BulbOutlined,
  EditOutlined
} from '@ant-design/icons'
import {
  useGetBusinessMetadataStatusQuery,
  usePopulateAllBusinessMetadataMutation,
  usePopulateTableBusinessMetadataMutation,
  useValidateBusinessMetadataQuery,
  useGetAllSchemaTablesQuery,
  useRefreshSchemaMutation
} from '@shared/store/api/businessApi'
import { useResponsive } from '@shared/hooks/useResponsive'

const { Title, Text } = Typography
const { TextArea } = Input

export const BusinessMetadataManager: React.FC = () => {
  const responsive = useResponsive()
  const [selectedTables, setSelectedTables] = useState<string[]>([])
  const [showPopulateModal, setShowPopulateModal] = useState(false)
  const [populateSettings, setPopulateSettings] = useState({
    useAI: true,
    overwriteExisting: false,
    batchSize: 10
  })

  const {
    data: metadataStatus,
    isLoading: statusLoading,
    refetch: refetchStatus
  } = useGetBusinessMetadataStatusQuery()

  const {
    data: validationResult,
    isLoading: validationLoading,
    refetch: refetchValidation
  } = useValidateBusinessMetadataQuery()

  const {
    data: allTables,
    isLoading: tablesLoading
  } = useGetAllSchemaTablesQuery()

  const [populateAll, { isLoading: populatingAll }] = usePopulateAllBusinessMetadataMutation()
  const [populateTable, { isLoading: populatingTable }] = usePopulateTableBusinessMetadataMutation()
  const [refreshSchema] = useRefreshSchemaMutation()

  const handlePopulateAll = async () => {
    try {
      const result = await populateAll({
        useAI: populateSettings.useAI,
        overwriteExisting: populateSettings.overwriteExisting
      }).unwrap()

      Modal.success({
        title: 'Population Complete',
        content: (
          <div>
            <p>Successfully processed {result.tablesProcessed} tables</p>
            <p>Updated {result.tablesUpdated} tables</p>
            {result.errors.length > 0 && (
              <div>
                <p>Errors encountered:</p>
                <ul>
                  {result.errors.slice(0, 5).map((error, index) => (
                    <li key={index}>{error}</li>
                  ))}
                  {result.errors.length > 5 && <li>...and {result.errors.length - 5} more</li>}
                </ul>
              </div>
            )}
            <p>Duration: {(result.duration / 1000).toFixed(1)} seconds</p>
          </div>
        )
      })

      refetchStatus()
      refetchValidation()
      setShowPopulateModal(false)
    } catch (error: any) {
      Modal.error({
        title: 'Population Failed',
        content: error.message || 'An error occurred while populating metadata'
      })
    }
  }

  const handlePopulateSelected = async () => {
    if (selectedTables.length === 0) return

    try {
      const promises = selectedTables.map(tableKey => {
        const [schemaName, tableName] = tableKey.split('.')
        return populateTable({
          schemaName,
          tableName,
          useAI: populateSettings.useAI,
          overwrite: populateSettings.overwriteExisting
        }).unwrap()
      })

      await Promise.all(promises)

      Modal.success({
        title: 'Selected Tables Updated',
        content: `Successfully updated ${selectedTables.length} tables`
      })

      refetchStatus()
      refetchValidation()
      setSelectedTables([])
    } catch (error: any) {
      Modal.error({
        title: 'Update Failed',
        content: error.message || 'An error occurred while updating selected tables'
      })
    }
  }

  const handleRefreshSchema = async () => {
    try {
      const result = await refreshSchema().unwrap()
      Modal.success({
        title: 'Schema Refreshed',
        content: `${result.message}. Discovered ${result.tablesDiscovered} tables.`
      })
      refetchStatus()
    } catch (error: any) {
      Modal.error({
        title: 'Refresh Failed',
        content: error.message || 'Failed to refresh schema'
      })
    }
  }

  const tableColumns = [
    {
      title: 'Schema',
      dataIndex: 'schemaName',
      key: 'schemaName',
      width: 120,
    },
    {
      title: 'Table Name',
      dataIndex: 'tableName',
      key: 'tableName',
      width: 200,
    },
    {
      title: 'Business Purpose',
      dataIndex: 'businessPurpose',
      key: 'businessPurpose',
      render: (purpose: string) => purpose ? (
        <Text>{purpose}</Text>
      ) : (
        <Tag color="orange">Missing</Tag>
      ),
    },
    {
      title: 'Domain',
      dataIndex: 'domainClassification',
      key: 'domainClassification',
      width: 120,
      render: (domain: string) => domain ? (
        <Tag color="blue">{domain}</Tag>
      ) : (
        <Tag color="orange">Missing</Tag>
      ),
    },
    {
      title: 'Row Count',
      dataIndex: 'estimatedRowCount',
      key: 'estimatedRowCount',
      width: 100,
      render: (count: number) => count ? count.toLocaleString() : 'Unknown',
    },
    {
      title: 'Last Updated',
      dataIndex: 'lastUpdated',
      key: 'lastUpdated',
      width: 120,
      render: (date: string) => date ? new Date(date).toLocaleDateString() : 'Never',
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_, record: any) => (
        <Space>
          <Tooltip title="Edit Metadata">
            <Button
              type="text"
              size="small"
              icon={<EditOutlined />}
              onClick={() => {
                // Handle edit metadata
                console.log('Edit metadata for', record)
              }}
            />
          </Tooltip>
          <Tooltip title="Populate with AI">
            <Button
              type="text"
              size="small"
              icon={<BulbOutlined />}
              onClick={() => {
                populateTable({
                  schemaName: record.schemaName,
                  tableName: record.tableName,
                  useAI: true,
                  overwrite: true
                })
              }}
              loading={populatingTable}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  const coveragePercentage = metadataStatus 
    ? Math.round((metadataStatus.populatedTables / metadataStatus.totalTables) * 100)
    : 0

  return (
    <div style={{ padding: responsive.isMobile ? '16px' : '24px' }}>
      {/* Header */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '24px'
      }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>
            Business Metadata Manager
          </Title>
          <Text type="secondary">
            Manage and populate business metadata for database tables
          </Text>
        </div>
        
        <Space>
          <Button
            icon={<ReloadOutlined />}
            onClick={handleRefreshSchema}
          >
            Refresh Schema
          </Button>
          <Button
            type="primary"
            icon={<SettingOutlined />}
            onClick={() => setShowPopulateModal(true)}
          >
            Populate Metadata
          </Button>
        </Space>
      </div>

      {/* Status Overview */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Total Tables"
              value={metadataStatus?.totalTables || 0}
              prefix={<DatabaseOutlined />}
            />
          </Card>
        </Col>
        
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Populated Tables"
              value={metadataStatus?.populatedTables || 0}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        
        <Col xs={24} sm={8}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Progress
                type="circle"
                percent={coveragePercentage}
                strokeColor={{
                  '0%': '#ff4d4f',
                  '50%': '#faad14',
                  '100%': '#52c41a',
                }}
              />
              <div style={{ marginTop: '8px' }}>
                <Text strong>Coverage</Text>
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Validation Results */}
      {validationResult && (
        <Alert
          type={validationResult.coverage > 80 ? 'success' : validationResult.coverage > 50 ? 'warning' : 'error'}
          message={`Metadata Coverage: ${validationResult.coverage.toFixed(1)}%`}
          description={
            <div>
              {validationResult.missingTables.length > 0 && (
                <div>
                  <Text strong>Missing metadata for {validationResult.missingTables.length} tables</Text>
                </div>
              )}
              {validationResult.issues.length > 0 && (
                <div>
                  <Text strong>Issues found:</Text>
                  <ul>
                    {validationResult.issues.slice(0, 3).map((issue, index) => (
                      <li key={index}>{issue}</li>
                    ))}
                    {validationResult.issues.length > 3 && (
                      <li>...and {validationResult.issues.length - 3} more issues</li>
                    )}
                  </ul>
                </div>
              )}
            </div>
          }
          style={{ marginBottom: '24px' }}
          showIcon
        />
      )}

      {/* Tables List */}
      <Card
        title={
          <Space>
            <DatabaseOutlined />
            <span>Database Tables</span>
            {selectedTables.length > 0 && (
              <Tag color="blue">{selectedTables.length} selected</Tag>
            )}
          </Space>
        }
        extra={
          selectedTables.length > 0 && (
            <Button
              type="primary"
              size="small"
              onClick={handlePopulateSelected}
              loading={populatingTable}
            >
              Populate Selected
            </Button>
          )
        }
      >
        <Table
          columns={tableColumns}
          dataSource={allTables}
          loading={tablesLoading}
          rowKey={(record) => `${record.schemaName}.${record.tableName}`}
          rowSelection={{
            selectedRowKeys: selectedTables,
            onChange: setSelectedTables,
            getCheckboxProps: (record) => ({
              disabled: false,
            }),
          }}
          pagination={{
            pageSize: 20,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => 
              `${range[0]}-${range[1]} of ${total} tables`,
          }}
          scroll={{ x: 'max-content' }}
          size={responsive.isMobile ? 'small' : 'middle'}
        />
      </Card>

      {/* Populate Modal */}
      <Modal
        title="Populate Business Metadata"
        open={showPopulateModal}
        onCancel={() => setShowPopulateModal(false)}
        footer={[
          <Button key="cancel" onClick={() => setShowPopulateModal(false)}>
            Cancel
          </Button>,
          <Button
            key="populate"
            type="primary"
            onClick={handlePopulateAll}
            loading={populatingAll}
            icon={<PlayCircleOutlined />}
          >
            Start Population
          </Button>,
        ]}
      >
        <Form layout="vertical">
          <Form.Item label="Use AI for Metadata Generation">
            <Switch
              checked={populateSettings.useAI}
              onChange={(checked) => 
                setPopulateSettings(prev => ({ ...prev, useAI: checked }))
              }
            />
            <div style={{ marginTop: '4px' }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Use AI to generate business-friendly descriptions and classifications
              </Text>
            </div>
          </Form.Item>

          <Form.Item label="Overwrite Existing Metadata">
            <Switch
              checked={populateSettings.overwriteExisting}
              onChange={(checked) => 
                setPopulateSettings(prev => ({ ...prev, overwriteExisting: checked }))
              }
            />
            <div style={{ marginTop: '4px' }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Replace existing metadata with newly generated content
              </Text>
            </div>
          </Form.Item>

          <Alert
            type="info"
            message="Population Process"
            description="This process will analyze your database schema and generate business-friendly metadata for all tables and columns. Depending on the size of your database, this may take several minutes."
            style={{ marginTop: '16px' }}
            showIcon
          />
        </Form>
      </Modal>
    </div>
  )
}
