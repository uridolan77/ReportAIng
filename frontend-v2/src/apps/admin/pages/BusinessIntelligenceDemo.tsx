import React, { useState } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Button, 
  Select,
  Input,
  Tabs,
  Alert,
  Divider
} from 'antd'
import {
  BulbOutlined,
  DatabaseOutlined,
  SearchOutlined,
  EyeOutlined,
  BookOutlined,
  TagsOutlined
} from '@ant-design/icons'
import { 
  BusinessContextVisualizer,
  EntityHighlighter,
  QueryUnderstandingFlow,
  IntentClassificationPanel,
  ContextualSchemaHelp,
  BusinessTermExplorer
} from '@shared/components/ai/intelligence'
import type { 
  BusinessContextProfile, 
  BusinessEntity, 
  QueryUnderstandingResult,
  SchemaContext,
  BusinessTerm,
  TermRelationship,
  TermCategory
} from '@shared/types/ai'

const { Title, Text } = Typography
const { TextArea } = Input

/**
 * BusinessIntelligenceDemo - Comprehensive demo of AI business intelligence components
 * 
 * Features:
 * - Interactive business context visualization
 * - Entity highlighting in natural language queries
 * - Query understanding flow demonstration
 * - Intent classification with alternatives
 * - Contextual schema help and exploration
 * - Business term glossary and relationships
 */
export const BusinessIntelligenceDemo: React.FC = () => {
  const [selectedQuery, setSelectedQuery] = useState('Show me quarterly sales by region for the last year')
  const [activeTab, setActiveTab] = useState('context')

  // Mock data for demonstration
  const mockBusinessContext: BusinessContextProfile = {
    id: 'ctx-001',
    confidence: 0.89,
    intent: {
      type: 'aggregation',
      confidence: 0.92,
      complexity: 'moderate',
      description: 'User wants to aggregate sales data by region over a quarterly time period',
      businessGoal: 'Analyze regional sales performance to identify growth opportunities',
      subIntents: ['time_analysis', 'geographic_comparison'],
      reasoning: [
        'Query contains aggregation keywords: "quarterly", "by region"',
        'Time dimension identified: "last year"',
        'Geographic dimension identified: "region"'
      ]
    },
    entities: [
      {
        id: 'ent-001',
        name: 'sales',
        type: 'metric',
        startPosition: 17,
        endPosition: 22,
        confidence: 0.95,
        businessMeaning: 'Revenue generated from product sales',
        context: 'Primary business metric for performance analysis',
        relationships: [
          { relatedEntity: 'revenue', relationshipType: 'synonym', strength: 0.9 }
        ]
      },
      {
        id: 'ent-002',
        name: 'region',
        type: 'dimension',
        startPosition: 26,
        endPosition: 32,
        confidence: 0.88,
        businessMeaning: 'Geographic sales territory',
        context: 'Used for geographic analysis and territory management',
        relationships: [
          { relatedEntity: 'territory', relationshipType: 'synonym', strength: 0.85 }
        ]
      },
      {
        id: 'ent-003',
        name: 'quarterly',
        type: 'time',
        startPosition: 0,
        endPosition: 9,
        confidence: 0.92,
        businessMeaning: 'Three-month time period',
        context: 'Standard business reporting period'
      }
    ],
    domain: {
      name: 'Sales Analytics',
      description: 'Analysis of sales performance, trends, and geographic distribution',
      relevance: 0.94,
      concepts: ['Revenue', 'Territory Management', 'Performance Analysis', 'Time Series'],
      relationships: ['Sales connects to Customer', 'Region connects to Territory', 'Quarter connects to Fiscal Year']
    },
    businessTerms: ['Sales', 'Revenue', 'Region', 'Territory', 'Quarter', 'Performance'],
    timeContext: {
      period: 'last year',
      granularity: 'quarterly',
      relativeTo: 'current date'
    },
    metadata: {}
  }

  const mockQueryUnderstanding: QueryUnderstandingResult = {
    understandingId: 'qu-001',
    originalQuery: selectedQuery,
    intent: mockBusinessContext.intent,
    entities: mockBusinessContext.entities,
    complexity: {
      level: 'moderate',
      factors: ['Multiple dimensions', 'Time aggregation', 'Geographic grouping'],
      score: 0.65
    },
    confidence: 0.89,
    processingTime: 1250,
    processingSteps: [
      {
        id: 'step-001',
        name: 'Query Parsing',
        description: 'Parse natural language query into structured components',
        status: 'completed',
        confidence: 0.95,
        startTime: '2024-01-15T10:30:00Z',
        endTime: '2024-01-15T10:30:00.200Z',
        duration: 200,
        reasoning: 'Successfully identified key terms and structure'
      },
      {
        id: 'step-002',
        name: 'Entity Recognition',
        description: 'Identify business entities and their relationships',
        status: 'completed',
        confidence: 0.88,
        startTime: '2024-01-15T10:30:00.200Z',
        endTime: '2024-01-15T10:30:00.650Z',
        duration: 450,
        reasoning: 'Found 3 entities with high confidence scores'
      },
      {
        id: 'step-003',
        name: 'Intent Classification',
        description: 'Determine the primary intent and business goal',
        status: 'completed',
        confidence: 0.92,
        startTime: '2024-01-15T10:30:00.650Z',
        endTime: '2024-01-15T10:30:01.000Z',
        duration: 350,
        reasoning: 'Clear aggregation pattern with geographic and temporal dimensions'
      },
      {
        id: 'step-004',
        name: 'Context Integration',
        description: 'Integrate business context and domain knowledge',
        status: 'completed',
        confidence: 0.85,
        startTime: '2024-01-15T10:30:01.000Z',
        endTime: '2024-01-15T10:30:01.250Z',
        duration: 250,
        reasoning: 'Successfully mapped to Sales Analytics domain'
      }
    ],
    suggestions: [
      {
        type: 'optimization',
        title: 'Add comparison baseline',
        description: 'Consider comparing with previous year for better insights',
        suggestedQuery: 'Show me quarterly sales by region for the last year compared to the previous year',
        confidence: 0.82,
        impact: 'medium'
      },
      {
        type: 'enhancement',
        title: 'Include growth metrics',
        description: 'Add growth rate calculations for trend analysis',
        confidence: 0.75,
        impact: 'high'
      }
    ],
    metadata: {}
  }

  const mockSchemaContext: SchemaContext = {
    confidence: 0.87,
    tables: [
      {
        name: 'sales_fact',
        description: 'Main sales transaction table with detailed sales records',
        businessPurpose: 'Core sales data for analytics and reporting',
        relevanceScore: 0.95,
        columns: [
          { name: 'sale_id', type: 'string', description: 'Unique sale identifier', businessMeaning: 'Primary key for sales transactions', nullable: false, isPrimaryKey: true, isForeignKey: false },
          { name: 'amount', type: 'number', description: 'Sale amount in USD', businessMeaning: 'Revenue value of the transaction', nullable: false, isPrimaryKey: false, isForeignKey: false },
          { name: 'region_id', type: 'string', description: 'Geographic region identifier', businessMeaning: 'Links to regional territory data', nullable: false, isPrimaryKey: false, isForeignKey: true },
          { name: 'sale_date', type: 'date', description: 'Date of the sale', businessMeaning: 'Transaction timestamp for temporal analysis', nullable: false, isPrimaryKey: false, isForeignKey: false }
        ],
        keyColumns: ['sale_id'],
        relationships: [
          { relatedTable: 'region_dim', relationshipType: 'many-to-one', strength: 0.9, description: 'Sales belong to regions' }
        ],
        estimatedRowCount: 2500000,
        lastUpdated: '2024-01-15T08:00:00Z'
      },
      {
        name: 'region_dim',
        description: 'Geographic region dimension table',
        businessPurpose: 'Regional territory definitions and hierarchies',
        relevanceScore: 0.88,
        columns: [
          { name: 'region_id', type: 'string', description: 'Region identifier', businessMeaning: 'Primary key for regions', nullable: false, isPrimaryKey: true, isForeignKey: false },
          { name: 'region_name', type: 'string', description: 'Human-readable region name', businessMeaning: 'Display name for regional analysis', nullable: false, isPrimaryKey: false, isForeignKey: false },
          { name: 'country', type: 'string', description: 'Country code', businessMeaning: 'Country-level grouping', nullable: false, isPrimaryKey: false, isForeignKey: false }
        ],
        keyColumns: ['region_id'],
        relationships: [],
        estimatedRowCount: 50
      }
    ],
    businessTerms: [
      { term: 'Sales', definition: 'Revenue generated from product or service transactions', category: 'Financial', synonyms: ['Revenue', 'Income'], relatedTerms: ['Profit', 'Margin'] },
      { term: 'Region', definition: 'Geographic territory for sales and marketing operations', category: 'Geographic', synonyms: ['Territory', 'Area'], relatedTerms: ['Country', 'Market'] },
      { term: 'Quarter', definition: 'Three-month business reporting period', category: 'Temporal', synonyms: ['Q1', 'Q2', 'Q3', 'Q4'], relatedTerms: ['Month', 'Year'] }
    ],
    suggestions: [
      'Consider joining with customer dimension for deeper insights',
      'Add product category for cross-category analysis',
      'Include time dimension table for better temporal queries'
    ]
  }

  const mockBusinessTerms: BusinessTerm[] = [
    { term: 'Sales', definition: 'Revenue generated from product or service transactions', category: 'Financial', synonyms: ['Revenue', 'Income'], relatedTerms: ['Profit', 'Margin', 'Commission'] },
    { term: 'Region', definition: 'Geographic territory for sales and marketing operations', category: 'Geographic', synonyms: ['Territory', 'Area'], relatedTerms: ['Country', 'Market', 'Zone'] },
    { term: 'Quarter', definition: 'Three-month business reporting period', category: 'Temporal', synonyms: ['Q1', 'Q2', 'Q3', 'Q4'], relatedTerms: ['Month', 'Year', 'Fiscal Period'] },
    { term: 'Customer', definition: 'Individual or organization that purchases products or services', category: 'Business', synonyms: ['Client', 'Account'], relatedTerms: ['Prospect', 'Lead', 'Contact'] },
    { term: 'Product', definition: 'Goods or services offered for sale', category: 'Business', synonyms: ['Item', 'SKU'], relatedTerms: ['Category', 'Brand', 'Inventory'] }
  ]

  const mockTermRelationships: TermRelationship[] = [
    { fromTerm: 'Sales', toTerm: 'Revenue', relationshipType: 'synonym', strength: 0.95 },
    { fromTerm: 'Region', toTerm: 'Territory', relationshipType: 'synonym', strength: 0.88 },
    { fromTerm: 'Sales', toTerm: 'Customer', relationshipType: 'related', strength: 0.85 },
    { fromTerm: 'Sales', toTerm: 'Product', relationshipType: 'related', strength: 0.82 }
  ]

  const mockTermCategories: TermCategory[] = [
    { name: 'Financial', description: 'Financial and accounting terms', termCount: 15 },
    { name: 'Geographic', description: 'Location and territory terms', termCount: 8 },
    { name: 'Temporal', description: 'Time and date-related terms', termCount: 12 },
    { name: 'Business', description: 'General business concepts', termCount: 25 }
  ]

  const sampleQueries = [
    'Show me quarterly sales by region for the last year',
    'What are the top 10 customers by revenue this month?',
    'Compare product performance across different regions',
    'Analyze sales trends over the past 5 years',
    'Find customers with declining purchase patterns'
  ]

  const tabItems = [
    {
      key: 'context',
      label: (
        <Space>
          <BulbOutlined />
          <span>Business Context</span>
        </Space>
      ),
      children: (
        <BusinessContextVisualizer
          context={mockBusinessContext}
          interactive={true}
          showDomainDetails={true}
          showEntityRelationships={true}
        />
      )
    },
    {
      key: 'entities',
      label: (
        <Space>
          <TagsOutlined />
          <span>Entity Highlighting</span>
        </Space>
      ),
      children: (
        <EntityHighlighter
          text={selectedQuery}
          entities={mockBusinessContext.entities}
          interactive={true}
          showConfidence={true}
          showTooltips={true}
          showRelationships={true}
        />
      )
    },
    {
      key: 'understanding',
      label: (
        <Space>
          <SearchOutlined />
          <span>Query Understanding</span>
        </Space>
      ),
      children: (
        <QueryUnderstandingFlow
          understanding={mockQueryUnderstanding}
          showStepDetails={true}
          showEntityHighlighting={true}
          interactive={true}
        />
      )
    },
    {
      key: 'intent',
      label: (
        <Space>
          <EyeOutlined />
          <span>Intent Classification</span>
        </Space>
      ),
      children: (
        <IntentClassificationPanel
          intent={mockBusinessContext.intent}
          alternatives={[
            { id: 'alt-001', type: 'trend_analysis', confidence: 0.75, description: 'Analyze sales trends over time', reasoning: 'Time dimension suggests trend analysis' },
            { id: 'alt-002', type: 'comparison', confidence: 0.68, description: 'Compare regional performance', reasoning: 'Geographic dimension suggests comparison' }
          ]}
          showAlternatives={true}
          showConfidenceBreakdown={true}
          interactive={true}
        />
      )
    },
    {
      key: 'schema',
      label: (
        <Space>
          <DatabaseOutlined />
          <span>Schema Help</span>
        </Space>
      ),
      children: (
        <ContextualSchemaHelp
          schemaContext={mockSchemaContext}
          currentQuery={selectedQuery}
          showBusinessTerms={true}
          showRelationships={true}
          interactive={true}
        />
      )
    },
    {
      key: 'terms',
      label: (
        <Space>
          <BookOutlined />
          <span>Business Terms</span>
        </Space>
      ),
      children: (
        <BusinessTermExplorer
          terms={mockBusinessTerms}
          relationships={mockTermRelationships}
          categories={mockTermCategories}
          showRelationshipGraph={true}
          showCategoryTree={true}
          interactive={true}
        />
      )
    }
  ]

  return (
    <div style={{ padding: 24 }}>
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: 24 
      }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>
            <Space>
              <BulbOutlined />
              Business Intelligence Demo
            </Space>
          </Title>
          <Text type="secondary">
            Interactive demonstration of AI-powered business context and query understanding
          </Text>
        </div>
      </div>

      {/* Query Input */}
      <Card style={{ marginBottom: 24 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text strong>Sample Query:</Text>
            <Select
              value={selectedQuery}
              onChange={setSelectedQuery}
              style={{ width: 400 }}
              placeholder="Select a sample query"
            >
              {sampleQueries.map((query, index) => (
                <Select.Option key={index} value={query}>
                  {query}
                </Select.Option>
              ))}
            </Select>
          </div>
          <TextArea
            value={selectedQuery}
            onChange={(e) => setSelectedQuery(e.target.value)}
            placeholder="Enter your natural language query here..."
            rows={3}
            style={{ fontSize: '16px' }}
          />
          <Alert
            message="Interactive Demo"
            description="This demo shows how AI analyzes and understands business queries. Try different queries to see how the analysis changes."
            type="info"
            showIcon
          />
        </Space>
      </Card>

      {/* Main Content */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        size="large"
      />
    </div>
  )
}

export default BusinessIntelligenceDemo
