import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Input, 
  Select, 
  DatePicker, 
  Slider, 
  Button, 
  Space, 
  Typography, 
  Row, 
  Col,
  Tag,
  Collapse,
  Switch,
  Tooltip,
  Badge
} from 'antd'
import { 
  SearchOutlined, 
  FilterOutlined, 
  ClearOutlined,
  SaveOutlined,
  HistoryOutlined,
  StarOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'

const { Search } = Input
const { RangePicker } = DatePicker
const { Text } = Typography
const { Panel } = Collapse

interface SearchCriteria {
  query: string
  intentType?: string
  isActive?: boolean
  qualityScoreMin?: number
  qualityScoreMax?: number
  lastModifiedAfter?: string
  lastModifiedBefore?: string
  businessOwner?: string
  approvalStatus?: string
  tags?: string[]
  hasBusinessMetadata?: boolean
  performanceThreshold?: number
}

interface TemplateSearchFiltersProps {
  onSearch: (criteria: SearchCriteria) => void
  onClear: () => void
  initialCriteria?: SearchCriteria
}

export const TemplateSearchFilters: React.FC<TemplateSearchFiltersProps> = ({
  onSearch,
  onClear,
  initialCriteria
}) => {
  const [criteria, setCriteria] = useState<SearchCriteria>({
    query: '',
    ...initialCriteria
  })
  const [savedSearches, setSavedSearches] = useState<any[]>([])
  const [isAdvancedMode, setIsAdvancedMode] = useState(false)

  useEffect(() => {
    // Load saved searches from localStorage
    const saved = localStorage.getItem('templateSearches')
    if (saved) {
      setSavedSearches(JSON.parse(saved))
    }
  }, [])

  const handleCriteriaChange = (key: keyof SearchCriteria, value: any) => {
    const newCriteria = { ...criteria, [key]: value }
    setCriteria(newCriteria)
    onSearch(newCriteria)
  }

  const handleClear = () => {
    const clearedCriteria: SearchCriteria = { query: '' }
    setCriteria(clearedCriteria)
    onClear()
  }

  const handleSaveSearch = () => {
    const searchName = prompt('Enter a name for this search:')
    if (searchName && criteria.query) {
      const newSearch = {
        id: Date.now(),
        name: searchName,
        criteria: { ...criteria },
        createdAt: new Date().toISOString()
      }
      const updated = [...savedSearches, newSearch]
      setSavedSearches(updated)
      localStorage.setItem('templateSearches', JSON.stringify(updated))
    }
  }

  const handleLoadSearch = (search: any) => {
    setCriteria(search.criteria)
    onSearch(search.criteria)
  }

  const getActiveFiltersCount = () => {
    let count = 0
    if (criteria.intentType) count++
    if (criteria.isActive !== undefined) count++
    if (criteria.qualityScoreMin !== undefined) count++
    if (criteria.businessOwner) count++
    if (criteria.approvalStatus) count++
    if (criteria.tags && criteria.tags.length > 0) count++
    if (criteria.lastModifiedAfter || criteria.lastModifiedBefore) count++
    return count
  }

  const basicFilters = (
    <Row gutter={16}>
      <Col span={8}>
        <Search
          placeholder="Search templates by name, key, or description..."
          value={criteria.query}
          onChange={(e) => handleCriteriaChange('query', e.target.value)}
          onSearch={() => onSearch(criteria)}
          size="large"
          allowClear
        />
      </Col>
      <Col span={4}>
        <Select
          placeholder="Intent Type"
          value={criteria.intentType}
          onChange={(value) => handleCriteriaChange('intentType', value)}
          style={{ width: '100%' }}
          size="large"
          allowClear
        >
          <Select.Option value="sql_generation">SQL Generation</Select.Option>
          <Select.Option value="insight_generation">Insight Generation</Select.Option>
          <Select.Option value="explanation">Explanation</Select.Option>
          <Select.Option value="data_analysis">Data Analysis</Select.Option>
          <Select.Option value="report_generation">Report Generation</Select.Option>
        </Select>
      </Col>
      <Col span={3}>
        <Select
          placeholder="Status"
          value={criteria.isActive}
          onChange={(value) => handleCriteriaChange('isActive', value)}
          style={{ width: '100%' }}
          size="large"
          allowClear
        >
          <Select.Option value={true}>Active</Select.Option>
          <Select.Option value={false}>Inactive</Select.Option>
        </Select>
      </Col>
      <Col span={4}>
        <Select
          placeholder="Quality Score"
          value={criteria.qualityScoreMin}
          onChange={(value) => handleCriteriaChange('qualityScoreMin', value)}
          style={{ width: '100%' }}
          size="large"
          allowClear
        >
          <Select.Option value={90}>Excellent (90+)</Select.Option>
          <Select.Option value={70}>Good (70+)</Select.Option>
          <Select.Option value={50}>Fair (50+)</Select.Option>
          <Select.Option value={0}>All</Select.Option>
        </Select>
      </Col>
      <Col span={5}>
        <Space>
          <Tooltip title="Advanced Filters">
            <Button
              icon={<FilterOutlined />}
              onClick={() => setIsAdvancedMode(!isAdvancedMode)}
              type={isAdvancedMode ? 'primary' : 'default'}
            >
              {getActiveFiltersCount() > 0 && (
                <Badge count={getActiveFiltersCount()} size="small" />
              )}
              Filters
            </Button>
          </Tooltip>
          <Button
            icon={<ClearOutlined />}
            onClick={handleClear}
          >
            Clear
          </Button>
        </Space>
      </Col>
    </Row>
  )

  const advancedFilters = (
    <Collapse activeKey={isAdvancedMode ? ['filters'] : []}>
      <Panel 
        header={
          <Space>
            <FilterOutlined />
            Advanced Filters
            {getActiveFiltersCount() > 0 && (
              <Badge count={getActiveFiltersCount()} size="small" />
            )}
          </Space>
        } 
        key="filters"
      >
        <Row gutter={16} style={{ marginBottom: '16px' }}>
          <Col span={6}>
            <Text strong>Business Owner:</Text>
            <Input
              placeholder="Enter business owner"
              value={criteria.businessOwner}
              onChange={(e) => handleCriteriaChange('businessOwner', e.target.value)}
              style={{ marginTop: '4px' }}
            />
          </Col>
          <Col span={6}>
            <Text strong>Approval Status:</Text>
            <Select
              placeholder="Select approval status"
              value={criteria.approvalStatus}
              onChange={(value) => handleCriteriaChange('approvalStatus', value)}
              style={{ width: '100%', marginTop: '4px' }}
              allowClear
            >
              <Select.Option value="draft">Draft</Select.Option>
              <Select.Option value="pending_review">Pending Review</Select.Option>
              <Select.Option value="approved">Approved</Select.Option>
              <Select.Option value="deprecated">Deprecated</Select.Option>
            </Select>
          </Col>
          <Col span={6}>
            <Text strong>Tags:</Text>
            <Select
              mode="tags"
              placeholder="Select or enter tags"
              value={criteria.tags}
              onChange={(value) => handleCriteriaChange('tags', value)}
              style={{ width: '100%', marginTop: '4px' }}
            >
              <Select.Option value="sql">SQL</Select.Option>
              <Select.Option value="analytics">Analytics</Select.Option>
              <Select.Option value="reporting">Reporting</Select.Option>
              <Select.Option value="business">Business</Select.Option>
              <Select.Option value="performance">Performance</Select.Option>
            </Select>
          </Col>
          <Col span={6}>
            <Text strong>Has Business Metadata:</Text>
            <div style={{ marginTop: '4px' }}>
              <Switch
                checked={criteria.hasBusinessMetadata}
                onChange={(checked) => handleCriteriaChange('hasBusinessMetadata', checked)}
                checkedChildren="Yes"
                unCheckedChildren="No"
              />
            </div>
          </Col>
        </Row>

        <Row gutter={16} style={{ marginBottom: '16px' }}>
          <Col span={8}>
            <Text strong>Quality Score Range:</Text>
            <Slider
              range
              min={0}
              max={100}
              value={[criteria.qualityScoreMin || 0, criteria.qualityScoreMax || 100]}
              onChange={([min, max]) => {
                handleCriteriaChange('qualityScoreMin', min)
                handleCriteriaChange('qualityScoreMax', max)
              }}
              marks={{
                0: '0',
                50: '50',
                70: '70',
                90: '90',
                100: '100'
              }}
              style={{ marginTop: '8px' }}
            />
          </Col>
          <Col span={8}>
            <Text strong>Performance Threshold (%):</Text>
            <Slider
              min={0}
              max={100}
              value={criteria.performanceThreshold || 0}
              onChange={(value) => handleCriteriaChange('performanceThreshold', value)}
              marks={{
                0: '0%',
                50: '50%',
                80: '80%',
                95: '95%',
                100: '100%'
              }}
              style={{ marginTop: '8px' }}
            />
          </Col>
          <Col span={8}>
            <Text strong>Last Modified:</Text>
            <RangePicker
              value={[
                criteria.lastModifiedAfter ? dayjs(criteria.lastModifiedAfter) : null,
                criteria.lastModifiedBefore ? dayjs(criteria.lastModifiedBefore) : null
              ]}
              onChange={(dates) => {
                handleCriteriaChange('lastModifiedAfter', dates?.[0]?.toISOString())
                handleCriteriaChange('lastModifiedBefore', dates?.[1]?.toISOString())
              }}
              style={{ width: '100%', marginTop: '4px' }}
            />
          </Col>
        </Row>
      </Panel>
    </Collapse>
  )

  const savedSearchesPanel = savedSearches.length > 0 && (
    <Card size="small" style={{ marginTop: '16px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
        <Space>
          <HistoryOutlined />
          <Text strong>Saved Searches</Text>
        </Space>
        <Button
          size="small"
          icon={<SaveOutlined />}
          onClick={handleSaveSearch}
          disabled={!criteria.query}
        >
          Save Current
        </Button>
      </div>
      <Space wrap>
        {savedSearches.map((search) => (
          <Tag
            key={search.id}
            color="blue"
            style={{ cursor: 'pointer', marginBottom: '4px' }}
            onClick={() => handleLoadSearch(search)}
          >
            <StarOutlined /> {search.name}
          </Tag>
        ))}
      </Space>
    </Card>
  )

  return (
    <div>
      <Card>
        {basicFilters}
        {isAdvancedMode && (
          <div style={{ marginTop: '16px' }}>
            {advancedFilters}
          </div>
        )}
      </Card>
      {savedSearchesPanel}
    </div>
  )
}

export default TemplateSearchFilters
