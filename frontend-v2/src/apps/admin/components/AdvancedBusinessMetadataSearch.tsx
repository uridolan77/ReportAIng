import React, { useState } from 'react'
import {
  Modal,
  Form,
  Input,
  Select,
  Switch,
  Slider,
  Button,
  Space,
  Card,
  Row,
  Col,
  Tag,
  Typography,
  Divider,
} from 'antd'
import { SearchOutlined, ClearOutlined, SaveOutlined } from '@ant-design/icons'
import type { BusinessTableSearchRequest } from '@shared/store/api/businessApi'

const { TextArea } = Input
const { Option } = Select
const { Title, Text } = Typography

interface AdvancedSearchProps {
  visible: boolean
  onClose: () => void
  onSearch: (searchRequest: BusinessTableSearchRequest) => Promise<void>
  loading?: boolean
}

export const AdvancedBusinessMetadataSearch: React.FC<AdvancedSearchProps> = ({
  visible,
  onClose,
  onSearch,
  loading = false,
}) => {
  const [form] = Form.useForm()
  const [selectedSchemas, setSelectedSchemas] = useState<string[]>([])
  const [selectedDomains, setSelectedDomains] = useState<string[]>([])
  const [selectedTags, setSelectedTags] = useState<string[]>([])

  const handleSearch = async () => {
    try {
      const values = await form.validateFields()
      
      const searchRequest: BusinessTableSearchRequest = {
        searchQuery: values.searchQuery || '',
        schemas: selectedSchemas,
        domains: selectedDomains,
        tags: selectedTags,
        includeColumns: values.includeColumns || false,
        includeGlossaryTerms: values.includeGlossaryTerms || false,
        maxResults: values.maxResults || 50,
        minRelevanceScore: values.minRelevanceScore || 0.1,
      }

      await onSearch(searchRequest)
      onClose()
    } catch (error) {
      console.error('Search validation failed:', error)
    }
  }

  const handleReset = () => {
    form.resetFields()
    setSelectedSchemas([])
    setSelectedDomains([])
    setSelectedTags([])
  }

  const availableSchemas = ['dbo', 'sales', 'finance', 'hr', 'operations', 'marketing']
  const availableDomains = ['Sales', 'Finance', 'HR', 'Operations', 'Marketing', 'Reference', 'Analytics']
  const availableTags = ['analytics', 'reporting', 'transactional', 'master-data', 'reference', 'operational']

  return (
    <Modal
      title={
        <Space>
          <SearchOutlined />
          <span>Advanced Business Metadata Search</span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width={800}
      footer={[
        <Button key="reset" icon={<ClearOutlined />} onClick={handleReset}>
          Reset
        </Button>,
        <Button key="cancel" onClick={onClose}>
          Cancel
        </Button>,
        <Button key="search" type="primary" icon={<SearchOutlined />} onClick={handleSearch} loading={loading}>
          Search
        </Button>,
      ]}
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{
          includeColumns: true,
          includeGlossaryTerms: true,
          maxResults: 50,
          minRelevanceScore: 0.3,
        }}
      >
        {/* Search Query */}
        <Card size="small" title="Search Query" style={{ marginBottom: 16 }}>
          <Form.Item
            name="searchQuery"
            label="Natural Language Query"
            help="Enter your search query in natural language (e.g., 'customer sales analytics tables')"
          >
            <TextArea
              rows={3}
              placeholder="Describe what you're looking for..."
              showCount
              maxLength={500}
            />
          </Form.Item>
        </Card>

        {/* Filters */}
        <Card size="small" title="Filters" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item label="Schemas">
                <Select
                  mode="multiple"
                  placeholder="Select schemas"
                  value={selectedSchemas}
                  onChange={setSelectedSchemas}
                  style={{ width: '100%' }}
                >
                  {availableSchemas.map(schema => (
                    <Option key={schema} value={schema}>
                      {schema}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="Domains">
                <Select
                  mode="multiple"
                  placeholder="Select domains"
                  value={selectedDomains}
                  onChange={setSelectedDomains}
                  style={{ width: '100%' }}
                >
                  {availableDomains.map(domain => (
                    <Option key={domain} value={domain}>
                      {domain}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="Tags">
                <Select
                  mode="multiple"
                  placeholder="Select tags"
                  value={selectedTags}
                  onChange={setSelectedTags}
                  style={{ width: '100%' }}
                >
                  {availableTags.map(tag => (
                    <Option key={tag} value={tag}>
                      {tag}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
          </Row>

          {/* Selected Filters Display */}
          {(selectedSchemas.length > 0 || selectedDomains.length > 0 || selectedTags.length > 0) && (
            <div style={{ marginTop: 16 }}>
              <Text strong>Selected Filters:</Text>
              <div style={{ marginTop: 8 }}>
                {selectedSchemas.map(schema => (
                  <Tag
                    key={`schema-${schema}`}
                    color="blue"
                    closable
                    onClose={() => setSelectedSchemas(prev => prev.filter(s => s !== schema))}
                  >
                    Schema: {schema}
                  </Tag>
                ))}
                {selectedDomains.map(domain => (
                  <Tag
                    key={`domain-${domain}`}
                    color="green"
                    closable
                    onClose={() => setSelectedDomains(prev => prev.filter(d => d !== domain))}
                  >
                    Domain: {domain}
                  </Tag>
                ))}
                {selectedTags.map(tag => (
                  <Tag
                    key={`tag-${tag}`}
                    color="orange"
                    closable
                    onClose={() => setSelectedTags(prev => prev.filter(t => t !== tag))}
                  >
                    Tag: {tag}
                  </Tag>
                ))}
              </div>
            </div>
          )}
        </Card>

        {/* Search Options */}
        <Card size="small" title="Search Options" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="includeColumns" valuePropName="checked">
                <Switch />
                <span style={{ marginLeft: 8 }}>Include column information in search</span>
              </Form.Item>
              <Form.Item name="includeGlossaryTerms" valuePropName="checked">
                <Switch />
                <span style={{ marginLeft: 8 }}>Include business glossary terms</span>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="maxResults"
                label="Maximum Results"
                help="Limit the number of results returned"
              >
                <Slider
                  min={10}
                  max={200}
                  step={10}
                  marks={{
                    10: '10',
                    50: '50',
                    100: '100',
                    200: '200',
                  }}
                />
              </Form.Item>
              <Form.Item
                name="minRelevanceScore"
                label="Minimum Relevance Score"
                help="Filter results by relevance threshold"
              >
                <Slider
                  min={0.1}
                  max={1.0}
                  step={0.1}
                  marks={{
                    0.1: '0.1',
                    0.3: '0.3',
                    0.5: '0.5',
                    0.7: '0.7',
                    1.0: '1.0',
                  }}
                />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Search Tips */}
        <Card size="small" title="Search Tips">
          <ul style={{ margin: 0, paddingLeft: 20 }}>
            <li>Use natural language to describe what you're looking for</li>
            <li>Include business terms, use cases, or domain-specific language</li>
            <li>Combine filters to narrow down results</li>
            <li>Lower relevance scores will return more results</li>
            <li>Include columns and glossary terms for more comprehensive search</li>
          </ul>
        </Card>
      </Form>
    </Modal>
  )
}

export default AdvancedBusinessMetadataSearch
