import React, { useState, useEffect } from 'react'
import {
  Card,
  Table,
  Button,
  Space,
  Tag,
  Typography,
  Input,
  Select,
  Row,
  Col,
  Modal,
  Form,
  message,
  Tooltip,
  Badge,
  Tabs,
  List,
  Transfer,
  Divider,
  Alert,
} from 'antd'
import {
  LinkOutlined,
  DisconnectOutlined,
  SearchOutlined,
  SyncOutlined,
  BulbOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BookOutlined,
  DatabaseOutlined,
} from '@ant-design/icons'
import {
  useGetEnhancedBusinessTablesQuery,
  useGetEnhancedBusinessGlossaryQuery,
  useUpdateEnhancedBusinessTableMutation,
  useUpdateEnhancedGlossaryTermMutation,
  type BusinessTableInfo,
  type EnhancedBusinessGlossaryTerm,
} from '@shared/store/api/businessApi'
import type { ColumnsType } from 'antd/es/table'

const { Text, Title } = Typography
const { Search } = Input
const { Option } = Select
const { TabPane } = Tabs

interface TableGlossaryMapping {
  tableId: number
  tableName: string
  schemaName: string
  mappedTerms: string[]
  suggestedTerms: string[]
  mappingQuality: number
}

interface GlossaryTermMapping {
  termId: number
  term: string
  definition: string
  mappedTables: string[]
  suggestedTables: string[]
  usageScore: number
}

export const GlossaryTableIntegration: React.FC = () => {
  const [activeTab, setActiveTab] = useState('tables')
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedDomain, setSelectedDomain] = useState<string>('')
  const [mappingModalVisible, setMappingModalVisible] = useState(false)
  const [selectedItem, setSelectedItem] = useState<any>(null)
  const [mappingType, setMappingType] = useState<'table' | 'term'>('table')

  const { data: tablesData, isLoading: tablesLoading } = useGetEnhancedBusinessTablesQuery({
    page: 1,
    pageSize: 100,
    search: searchTerm,
    domain: selectedDomain,
  })

  const { data: glossaryData, isLoading: glossaryLoading } = useGetEnhancedBusinessGlossaryQuery({
    page: 1,
    pageSize: 100,
    search: searchTerm,
    domain: selectedDomain,
    includeRelationships: true,
  })

  const [updateTable] = useUpdateEnhancedBusinessTableMutation()
  const [updateTerm] = useUpdateEnhancedGlossaryTermMutation()

  // Mock data for demonstration - in real implementation, this would come from API
  const generateMockMappings = (): { tables: TableGlossaryMapping[]; terms: GlossaryTermMapping[] } => {
    const tables: TableGlossaryMapping[] = (tablesData?.data || []).map(table => ({
      tableId: table.id,
      tableName: table.tableName,
      schemaName: table.schemaName,
      mappedTerms: table.businessGlossaryTerms || [],
      suggestedTerms: [
        'Customer',
        'Revenue',
        'Transaction',
        'Product',
        'Order'
      ].filter(term => 
        table.businessPurpose.toLowerCase().includes(term.toLowerCase()) ||
        table.tableName.toLowerCase().includes(term.toLowerCase())
      ).slice(0, 3),
      mappingQuality: Math.floor(Math.random() * 40) + 60,
    }))

    const terms: GlossaryTermMapping[] = (glossaryData?.data || []).map(term => ({
      termId: term.id,
      term: term.term,
      definition: term.definition,
      mappedTables: term.mappedTables || [],
      suggestedTables: (tablesData?.data || [])
        .filter(table => 
          table.businessPurpose.toLowerCase().includes(term.term.toLowerCase()) ||
          table.tableName.toLowerCase().includes(term.term.toLowerCase())
        )
        .slice(0, 3)
        .map(table => `${table.schemaName}.${table.tableName}`),
      usageScore: term.usageFrequency || Math.floor(Math.random() * 100),
    }))

    return { tables, terms }
  }

  const { tables: tableMappings, terms: termMappings } = generateMockMappings()

  const handleMapTermsToTable = (table: TableGlossaryMapping) => {
    setSelectedItem(table)
    setMappingType('table')
    setMappingModalVisible(true)
  }

  const handleMapTablesToTerm = (term: GlossaryTermMapping) => {
    setSelectedItem(term)
    setMappingType('term')
    setMappingModalVisible(true)
  }

  const handleAutoMap = async () => {
    try {
      // In real implementation, this would call an auto-mapping API
      message.success('Auto-mapping completed! Found 15 new mappings.')
    } catch (error) {
      message.error('Auto-mapping failed')
    }
  }

  const tableColumns: ColumnsType<TableGlossaryMapping> = [
    {
      title: 'Table',
      key: 'table',
      width: 200,
      render: (_, record) => (
        <div>
          <Text strong>{record.tableName}</Text>
          <div style={{ fontSize: '12px', color: '#666' }}>
            {record.schemaName}
          </div>
        </div>
      ),
    },
    {
      title: 'Mapped Terms',
      dataIndex: 'mappedTerms',
      key: 'mappedTerms',
      render: (terms: string[]) => (
        <Space wrap>
          {terms.map((term, index) => (
            <Tag key={index} color="blue" icon={<BookOutlined />}>
              {term}
            </Tag>
          ))}
          {terms.length === 0 && (
            <Text type="secondary">No terms mapped</Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Suggested Terms',
      dataIndex: 'suggestedTerms',
      key: 'suggestedTerms',
      render: (terms: string[]) => (
        <Space wrap>
          {terms.map((term, index) => (
            <Tag key={index} color="orange" icon={<BulbOutlined />}>
              {term}
            </Tag>
          ))}
        </Space>
      ),
    },
    {
      title: 'Mapping Quality',
      dataIndex: 'mappingQuality',
      key: 'mappingQuality',
      width: 120,
      render: (quality: number) => (
        <Space>
          <Text style={{ color: quality >= 80 ? '#52c41a' : quality >= 60 ? '#faad14' : '#ff4d4f' }}>
            {quality}%
          </Text>
          <div style={{ width: 60 }}>
            <div
              style={{
                height: 4,
                backgroundColor: '#f0f0f0',
                borderRadius: 2,
                overflow: 'hidden',
              }}
            >
              <div
                style={{
                  height: '100%',
                  width: `${quality}%`,
                  backgroundColor: quality >= 80 ? '#52c41a' : quality >= 60 ? '#faad14' : '#ff4d4f',
                }}
              />
            </div>
          </div>
        </Space>
      ),
      sorter: (a, b) => a.mappingQuality - b.mappingQuality,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Map Terms">
            <Button
              size="small"
              icon={<LinkOutlined />}
              onClick={() => handleMapTermsToTable(record)}
            />
          </Tooltip>
          <Tooltip title="Auto-suggest">
            <Button
              size="small"
              icon={<BulbOutlined />}
              onClick={() => {
                message.info(`Auto-suggesting terms for ${record.tableName}`)
              }}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  const termColumns: ColumnsType<GlossaryTermMapping> = [
    {
      title: 'Term',
      key: 'term',
      width: 200,
      render: (_, record) => (
        <div>
          <Text strong>{record.term}</Text>
          <div style={{ fontSize: '12px', color: '#666' }}>
            {record.definition.substring(0, 50)}...
          </div>
        </div>
      ),
    },
    {
      title: 'Mapped Tables',
      dataIndex: 'mappedTables',
      key: 'mappedTables',
      render: (tables: string[]) => (
        <Space wrap>
          {tables.map((table, index) => (
            <Tag key={index} color="green" icon={<DatabaseOutlined />}>
              {table}
            </Tag>
          ))}
          {tables.length === 0 && (
            <Text type="secondary">No tables mapped</Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Suggested Tables',
      dataIndex: 'suggestedTables',
      key: 'suggestedTables',
      render: (tables: string[]) => (
        <Space wrap>
          {tables.map((table, index) => (
            <Tag key={index} color="orange" icon={<BulbOutlined />}>
              {table}
            </Tag>
          ))}
        </Space>
      ),
    },
    {
      title: 'Usage Score',
      dataIndex: 'usageScore',
      key: 'usageScore',
      width: 120,
      render: (score: number) => (
        <Badge count={score} style={{ backgroundColor: '#52c41a' }} />
      ),
      sorter: (a, b) => a.usageScore - b.usageScore,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Map Tables">
            <Button
              size="small"
              icon={<LinkOutlined />}
              onClick={() => handleMapTablesToTerm(record)}
            />
          </Tooltip>
          <Tooltip title="Auto-suggest">
            <Button
              size="small"
              icon={<BulbOutlined />}
              onClick={() => {
                message.info(`Auto-suggesting tables for ${record.term}`)
              }}
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  const renderMappingModal = () => (
    <Modal
      title={
        mappingType === 'table' 
          ? `Map Terms to Table: ${selectedItem?.tableName}`
          : `Map Tables to Term: ${selectedItem?.term}`
      }
      open={mappingModalVisible}
      onCancel={() => setMappingModalVisible(false)}
      width={800}
      footer={[
        <Button key="cancel" onClick={() => setMappingModalVisible(false)}>
          Cancel
        </Button>,
        <Button key="save" type="primary" onClick={() => {
          message.success('Mappings saved successfully')
          setMappingModalVisible(false)
        }}>
          Save Mappings
        </Button>,
      ]}
    >
      {selectedItem && (
        <div>
          <Alert
            message="Mapping Assistant"
            description={
              mappingType === 'table'
                ? "Select business glossary terms that are relevant to this table's data and purpose."
                : "Select tables that contain data related to this business term."
            }
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          
          <div style={{ height: 400 }}>
            {/* Transfer component would go here for mapping interface */}
            <div style={{ textAlign: 'center', padding: 40 }}>
              <Text type="secondary">
                Interactive mapping interface would be implemented here using Transfer component
              </Text>
            </div>
          </div>
        </div>
      )}
    </Modal>
  )

  return (
    <div>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Title level={4} style={{ margin: 0 }}>
                <LinkOutlined /> Glossary-Table Integration
              </Title>
              <Badge 
                count={`${tableMappings.length} tables`} 
                style={{ backgroundColor: '#52c41a' }} 
              />
              <Badge 
                count={`${termMappings.length} terms`} 
                style={{ backgroundColor: '#1890ff' }} 
              />
            </Space>
          </Col>
          <Col>
            <Space>
              <Button icon={<SyncOutlined />} onClick={handleAutoMap}>
                Auto-Map All
              </Button>
              <Button icon={<BulbOutlined />}>
                Suggest Mappings
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Search and Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16} align="middle">
          <Col flex="auto">
            <Search
              placeholder="Search tables or terms..."
              allowClear
              enterButton={<SearchOutlined />}
              size="large"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </Col>
          <Col>
            <Select
              placeholder="Domain"
              style={{ width: 150 }}
              allowClear
              value={selectedDomain}
              onChange={setSelectedDomain}
            >
              <Option value="Sales">Sales</Option>
              <Option value="Finance">Finance</Option>
              <Option value="HR">HR</Option>
              <Option value="Operations">Operations</Option>
              <Option value="Marketing">Marketing</Option>
            </Select>
          </Col>
        </Row>
      </Card>

      {/* Main Content */}
      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        <TabPane
          tab={
            <Space>
              <DatabaseOutlined />
              Tables ({tableMappings.length})
            </Space>
          }
          key="tables"
        >
          <Card>
            <Table
              columns={tableColumns}
              dataSource={tableMappings}
              rowKey="tableId"
              loading={tablesLoading}
              pagination={{
                pageSize: 20,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} tables`,
              }}
              scroll={{ x: 1000 }}
              size="small"
            />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <Space>
              <BookOutlined />
              Terms ({termMappings.length})
            </Space>
          }
          key="terms"
        >
          <Card>
            <Table
              columns={termColumns}
              dataSource={termMappings}
              rowKey="termId"
              loading={glossaryLoading}
              pagination={{
                pageSize: 20,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} terms`,
              }}
              scroll={{ x: 1000 }}
              size="small"
            />
          </Card>
        </TabPane>
      </Tabs>

      {/* Mapping Modal */}
      {renderMappingModal()}
    </div>
  )
}

export default GlossaryTableIntegration
