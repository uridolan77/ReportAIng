import React, { useState, useCallback } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Typography, 
  Space, 
  Button, 
  Input,
  Tabs,
  Alert,
  Spin,
  message
} from 'antd'
import {
  BulbOutlined,
  DatabaseOutlined,
  SearchOutlined,
  EyeOutlined,
  BookOutlined,
  TagsOutlined,
  HistoryOutlined,
  ThunderboltOutlined
} from '@ant-design/icons'
import { useQueryAnalysis, useBusinessContext, useQuerySuggestions } from '@shared/hooks/business-intelligence'
import { QueryAnalysisInput } from '@shared/components/business-intelligence/QueryAnalysisInput'
import { BusinessContextPanel } from '@shared/components/business-intelligence/BusinessContextPanel'
import { EntityHighlightPanel } from '@shared/components/business-intelligence/EntityHighlightPanel'
import { QueryUnderstandingPanel } from '@shared/components/business-intelligence/QueryUnderstandingPanel'
import { IntentClassificationPanel } from '@shared/components/business-intelligence/IntentClassificationPanel'
import { SchemaIntelligencePanel } from '@shared/components/business-intelligence/SchemaIntelligencePanel'
import { BusinessTermsPanel } from '@shared/components/business-intelligence/BusinessTermsPanel'
import { QueryHistoryPanel } from '@shared/components/business-intelligence/QueryHistoryPanel'
import { PerformanceMetricsPanel } from '@shared/components/business-intelligence/PerformanceMetricsPanel'
import type { 
  QueryAnalysisRequest,
  BusinessContextProfile,
  QueryUnderstandingResult
} from '@shared/types/business-intelligence'

const { Title, Text } = Typography
const { TextArea } = Input

/**
 * BusinessIntelligencePage - Production Business Intelligence System
 * 
 * Features:
 * - Real-time natural language query analysis
 * - AI-powered business context understanding
 * - Interactive entity detection and highlighting
 * - Query understanding flow with transparency
 * - Intent classification with confidence scoring
 * - Contextual schema assistance
 * - Business terms integration
 * - Query history and performance analytics
 */
export const BusinessIntelligencePage: React.FC = () => {
  const [currentQuery, setCurrentQuery] = useState('')
  const [activeTab, setActiveTab] = useState('analysis')
  const [analysisResults, setAnalysisResults] = useState<any>(null)

  // API Integration Hooks
  const { 
    analyzeQuery, 
    isAnalyzing, 
    error: analysisError 
  } = useQueryAnalysis()
  
  const { 
    businessContext, 
    isLoading: contextLoading, 
    refetch: refetchContext 
  } = useBusinessContext(currentQuery)
  
  const { 
    suggestions, 
    isLoading: suggestionsLoading 
  } = useQuerySuggestions()

  // Handle query analysis
  const handleQueryAnalysis = useCallback(async (query: string) => {
    if (!query.trim()) {
      message.warning('Please enter a query to analyze')
      return
    }

    try {
      const analysisRequest: QueryAnalysisRequest = {
        query: query.trim(),
        userId: 'current-user', // TODO: Get from auth context
        context: {
          userRole: 'analyst', // TODO: Get from user context
          department: 'sales', // TODO: Get from user context
          accessLevel: 'standard',
          timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
        },
        options: {
          includeEntityDetails: true,
          includeAlternatives: true,
          includeSuggestions: true
        }
      }

      const result = await analyzeQuery(analysisRequest)
      setAnalysisResults(result)
      setCurrentQuery(query)
      
      // Refetch related data
      refetchContext()
      
      message.success('Query analyzed successfully')
    } catch (error) {
      console.error('Query analysis failed:', error)
      message.error('Failed to analyze query. Please try again.')
    }
  }, [analyzeQuery, refetchContext])

  // Handle query suggestion selection
  const handleSuggestionSelect = useCallback((suggestion: string) => {
    setCurrentQuery(suggestion)
    handleQueryAnalysis(suggestion)
  }, [handleQueryAnalysis])

  // Tab items configuration
  const tabItems = [
    {
      key: 'analysis',
      label: (
        <Space>
          <BulbOutlined />
          <span>Query Analysis</span>
        </Space>
      ),
      children: (
        <BusinessContextPanel
          context={analysisResults?.businessContext}
          loading={isAnalyzing || contextLoading}
          interactive={true}
          showDomainDetails={true}
          showEntityRelationships={true}
          showAdvancedMetrics={true}
          showUserContext={true}
          onEntityClick={(entityId) => console.log('Entity clicked:', entityId)}
          onDomainExplore={(domain) => console.log('Domain explore:', domain)}
        />
      )
    },
    {
      key: 'entities',
      label: (
        <Space>
          <TagsOutlined />
          <span>Entity Detection</span>
        </Space>
      ),
      children: (
        <EntityHighlightPanel
          query={currentQuery}
          entities={analysisResults?.businessContext?.entities || []}
          loading={isAnalyzing}
          interactive={true}
          showConfidence={true}
          showTooltips={true}
          showRelationships={true}
          showMappings={true}
          showAdvancedAnalysis={true}
          onEntityClick={(entityId) => console.log('Entity clicked:', entityId)}
          onEntityEdit={(entityId) => console.log('Entity edit:', entityId)}
          onRelationshipExplore={(relationship) => console.log('Relationship explore:', relationship)}
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
        <QueryUnderstandingPanel
          query={currentQuery}
          loading={isAnalyzing}
          showSteps={true}
          showAlternatives={true}
          showConfidenceBreakdown={true}
          interactive={true}
          onAlternativeSelect={(alternative) => console.log('Alternative selected:', alternative)}
          onStepRerun={(stepId) => console.log('Step rerun:', stepId)}
          onQueryRefine={(refinedQuery) => console.log('Query refined:', refinedQuery)}
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
          intent={analysisResults?.businessContext?.intent}
          loading={isAnalyzing}
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
          <span>Schema Intelligence</span>
        </Space>
      ),
      children: (
        <SchemaIntelligencePanel
          query={currentQuery}
          loading={isAnalyzing}
          showBusinessTerms={true}
          showRelationships={true}
          showOptimizations={true}
          interactive={true}
          onTableSelect={(tableName) => console.log('Table selected:', tableName)}
          onJoinSuggestionApply={(suggestion) => console.log('Join applied:', suggestion)}
          onOptimizationApply={(tip) => console.log('Optimization applied:', tip)}
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
        <BusinessTermsPanel
          query={currentQuery}
          loading={isAnalyzing}
          showRelationshipGraph={true}
          showCategoryTree={true}
          interactive={true}
          onTermSelect={(termId) => console.log('Term selected:', termId)}
          onTermEdit={(termId) => console.log('Term edit:', termId)}
          onCategoryFilter={(categoryId) => console.log('Category filter:', categoryId)}
        />
      )
    },
    {
      key: 'history',
      label: (
        <Space>
          <HistoryOutlined />
          <span>Query History</span>
        </Space>
      ),
      children: (
        <QueryHistoryPanel
          onQuerySelect={handleSuggestionSelect}
          interactive={true}
        />
      )
    },
    {
      key: 'performance',
      label: (
        <Space>
          <ThunderboltOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: (
        <PerformanceMetricsPanel
          query={currentQuery}
          analysisResults={analysisResults}
          showOptimizations={true}
          showUsageAnalytics={true}
          showRealTimeMetrics={true}
          interactive={true}
          onOptimizationApply={(suggestionId) => console.log('Optimization applied:', suggestionId)}
          onMetricRefresh={() => console.log('Metrics refreshed')}
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
              Business Intelligence
            </Space>
          </Title>
          <Text type="secondary">
            AI-powered natural language query analysis and business context understanding
          </Text>
        </div>
        <Space>
          <Button 
            type="primary" 
            icon={<SearchOutlined />}
            onClick={() => handleQueryAnalysis(currentQuery)}
            loading={isAnalyzing}
            disabled={!currentQuery.trim()}
          >
            Analyze Query
          </Button>
        </Space>
      </div>

      {/* Error Display */}
      {analysisError && (
        <Alert
          message="Analysis Error"
          description={analysisError.message || 'Failed to analyze query'}
          type="error"
          showIcon
          closable
          style={{ marginBottom: 24 }}
        />
      )}

      {/* Query Input Section */}
      <Card style={{ marginBottom: 24 }}>
        <QueryAnalysisInput
          value={currentQuery}
          onChange={setCurrentQuery}
          onAnalyze={handleQueryAnalysis}
          suggestions={suggestions}
          onSuggestionSelect={handleSuggestionSelect}
          loading={isAnalyzing}
          suggestionsLoading={suggestionsLoading}
          placeholder="Enter your natural language query here... (e.g., 'Show me quarterly sales by region for the last year')"
        />
      </Card>

      {/* Main Content Tabs */}
      <Spin spinning={isAnalyzing && !analysisResults}>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
          size="large"
          type="card"
        />
      </Spin>
    </div>
  )
}

export default BusinessIntelligencePage
