import React, { useState, useEffect, useMemo } from 'react'
import { Card, Tabs, Spin, Alert, Button, Space, Typography, Tooltip } from 'antd'
import {
  EyeOutlined,
  SettingOutlined,
  BarChartOutlined,
  BulbOutlined,
  ReloadOutlined,
  ExportOutlined,
  InfoCircleOutlined
} from '@ant-design/icons'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import {
  selectActiveTrace,
  selectTransparencySettings,
  selectTransparencyUI,
  setActiveTab,
  setShowDetailedMetrics
} from '@shared/store/aiTransparencySlice'
import { AIFeatureWrapper } from '../common/AIFeatureWrapper'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import { PromptConstructionViewer } from './PromptConstructionViewer'
import { ConfidenceBreakdownChart } from './ConfidenceBreakdownChart'
import { AIDecisionExplainer } from './AIDecisionExplainer'
import type { AITransparencyPanelProps } from './types'

const { Title, Text } = Typography

/**
 * AITransparencyPanel - Main component for AI decision transparency
 * 
 * Features:
 * - Prompt construction visualization
 * - Confidence analysis and breakdown
 * - AI decision explanations
 * - Alternative suggestions
 * - Performance metrics
 * - Export capabilities
 */
export const AITransparencyPanel: React.FC<AITransparencyPanelProps> = ({
  traceId,
  showDetailedMetrics: propShowDetailedMetrics,
  compact = false,
  onOptimizationSuggestion,
  onAlternativeSelect,
  className,
  testId = 'ai-transparency-panel'
}) => {
  const dispatch = useAppDispatch()
  const activeTrace = useAppSelector(selectActiveTrace)
  const { showDetailedMetrics, transparencyLevel, confidenceThreshold } = useAppSelector(selectTransparencySettings)
  const { activeTab, loading, error } = useAppSelector(selectTransparencyUI)
  
  const [localLoading, setLocalLoading] = useState(false)
  const [refreshing, setRefreshing] = useState(false)

  // Use prop value if provided, otherwise use store value
  const effectiveShowDetailedMetrics = propShowDetailedMetrics ?? showDetailedMetrics

  // Mock data for development - replace with actual API calls
  const mockTransparencyData = useMemo(() => {
    if (!traceId) return null
    
    return {
      trace: {
        traceId,
        steps: [
          {
            stepName: 'Context Analysis',
            description: 'Analyzing user query and business context',
            confidence: 0.92,
            context: ['user_query', 'business_metadata', 'schema_info'],
            alternatives: ['Simple keyword matching', 'Semantic search only'],
            reasoning: 'High confidence due to clear business intent and available metadata',
            timestamp: new Date().toISOString()
          },
          {
            stepName: 'Intent Classification',
            description: 'Determining query intent and complexity',
            confidence: 0.87,
            context: ['query_patterns', 'user_history', 'domain_knowledge'],
            alternatives: ['Basic classification', 'Rule-based approach'],
            reasoning: 'Strong pattern match with historical successful queries',
            timestamp: new Date().toISOString()
          },
          {
            stepName: 'SQL Generation',
            description: 'Generating optimized SQL query',
            confidence: 0.94,
            context: ['schema_relationships', 'performance_hints', 'business_rules'],
            alternatives: ['Basic SQL template', 'Manual query building'],
            reasoning: 'Excellent schema understanding and optimization opportunities identified',
            timestamp: new Date().toISOString()
          }
        ],
        finalPrompt: 'Generate SQL query for sales analysis with proper joins and filters...',
        totalConfidence: 0.91,
        optimizationSuggestions: [
          {
            id: 'opt-1',
            type: 'performance',
            title: 'Add Index Hint',
            description: 'Consider adding index hints for better performance',
            impact: 'medium',
            effort: 'low',
            expectedImprovement: 0.15,
            implementation: 'Add /*+ INDEX(sales_table, idx_date) */ to query'
          }
        ],
        metadata: {
          modelUsed: 'gpt-4',
          provider: 'openai',
          tokensUsed: 1250,
          processingTime: 850
        }
      },
      analysis: {
        overallConfidence: 0.91,
        factors: [
          {
            name: 'Context Relevance',
            score: 0.95,
            explanation: 'Query context strongly matches available business metadata',
            impact: 'high',
            category: 'context'
          },
          {
            name: 'Schema Understanding',
            score: 0.89,
            explanation: 'Good understanding of table relationships and constraints',
            impact: 'high',
            category: 'business'
          },
          {
            name: 'Query Complexity',
            score: 0.88,
            explanation: 'Moderate complexity query with clear optimization paths',
            impact: 'medium',
            category: 'syntax'
          }
        ],
        breakdown: {
          contextualRelevance: 0.95,
          syntacticCorrectness: 0.88,
          semanticClarity: 0.92,
          businessAlignment: 0.89
        },
        recommendations: [
          'Consider adding more specific date filters for better performance',
          'Review business rules for additional validation opportunities'
        ]
      }
    }
  }, [traceId])

  // Handle tab change
  const handleTabChange = (key: string) => {
    dispatch(setActiveTab(key))
  }

  // Handle refresh
  const handleRefresh = async () => {
    setRefreshing(true)
    try {
      // TODO: Implement actual refresh logic
      await new Promise(resolve => setTimeout(resolve, 1000))
    } finally {
      setRefreshing(false)
    }
  }

  // Handle export
  const handleExport = () => {
    if (mockTransparencyData) {
      const dataStr = JSON.stringify(mockTransparencyData, null, 2)
      const dataBlob = new Blob([dataStr], { type: 'application/json' })
      const url = URL.createObjectURL(dataBlob)
      const link = document.createElement('a')
      link.href = url
      link.download = `transparency-trace-${traceId}.json`
      link.click()
      URL.revokeObjectURL(url)
    }
  }

  // Handle settings toggle
  const handleToggleDetailedMetrics = () => {
    dispatch(setShowDetailedMetrics(!effectiveShowDetailedMetrics))
  }

  if (loading || localLoading) {
    return (
      <Card className={className} data-testid={`${testId}-loading`}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text>Loading AI transparency data...</Text>
          </div>
        </div>
      </Card>
    )
  }

  if (error) {
    return (
      <Card className={className} data-testid={`${testId}-error`}>
        <Alert
          message="Transparency Data Unavailable"
          description={error}
          type="error"
          showIcon
          action={
            <Button size="small" onClick={handleRefresh}>
              Retry
            </Button>
          }
        />
      </Card>
    )
  }

  if (!mockTransparencyData) {
    return (
      <Card className={className} data-testid={`${testId}-no-data`}>
        <Alert
          message="No Transparency Data"
          description="No transparency trace found for the specified ID."
          type="info"
          showIcon
        />
      </Card>
    )
  }

  const { trace, analysis } = mockTransparencyData

  // Tab items configuration
  const tabItems = [
    {
      key: 'construction',
      label: (
        <Space>
          <EyeOutlined />
          <span>Prompt Construction</span>
        </Space>
      ),
      children: (
        <PromptConstructionViewer
          trace={trace}
          interactive={!compact}
          showTimeline={effectiveShowDetailedMetrics}
        />
      )
    },
    {
      key: 'confidence',
      label: (
        <Space>
          <BarChartOutlined />
          <span>Confidence Analysis</span>
        </Space>
      ),
      children: (
        <ConfidenceBreakdownChart
          analysis={analysis}
          showFactors={effectiveShowDetailedMetrics}
          interactive={!compact}
        />
      )
    },
    {
      key: 'explanation',
      label: (
        <Space>
          <BulbOutlined />
          <span>Decision Explanation</span>
        </Space>
      ),
      children: (
        <AIDecisionExplainer
          explanation={{
            decision: 'Generated optimized SQL query with proper joins',
            reasoning: [
              'Identified clear business intent from user query',
              'Found optimal table relationships in schema',
              'Applied performance optimization rules',
              'Validated against business constraints'
            ],
            confidence: trace.totalConfidence,
            alternatives: [],
            factors: analysis.factors,
            recommendations: trace.optimizationSuggestions
          }}
          showAlternatives={effectiveShowDetailedMetrics}
          showRecommendations={true}
          onAlternativeSelect={onAlternativeSelect}
          onRecommendationApply={onOptimizationSuggestion}
        />
      )
    }
  ]

  // Add performance tab if detailed metrics enabled
  if (effectiveShowDetailedMetrics) {
    tabItems.push({
      key: 'performance',
      label: (
        <Space>
          <SettingOutlined />
          <span>Performance</span>
        </Space>
      ),
      children: (
        <div style={{ padding: 16 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <div>
              <Text strong>Processing Time:</Text>
              <Text style={{ marginLeft: 8 }}>{trace.metadata.processingTime}ms</Text>
            </div>
            <div>
              <Text strong>Tokens Used:</Text>
              <Text style={{ marginLeft: 8 }}>{trace.metadata.tokensUsed}</Text>
            </div>
            <div>
              <Text strong>Model:</Text>
              <Text style={{ marginLeft: 8 }}>{trace.metadata.modelUsed}</Text>
            </div>
            <div>
              <Text strong>Provider:</Text>
              <Text style={{ marginLeft: 8 }}>{trace.metadata.provider}</Text>
            </div>
          </Space>
        </div>
      )
    })
  }

  return (
    <AIFeatureWrapper feature="transparencyPanelEnabled" className={className}>
      <Card
        title={
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Space>
              <Title level={4} style={{ margin: 0 }}>
                AI Decision Transparency
              </Title>
              <ConfidenceIndicator
                confidence={trace.totalConfidence}
                size="small"
                type="badge"
              />
            </Space>
            <Space>
              <Tooltip title="Toggle detailed metrics">
                <Button
                  icon={<SettingOutlined />}
                  size="small"
                  type={effectiveShowDetailedMetrics ? 'primary' : 'default'}
                  onClick={handleToggleDetailedMetrics}
                />
              </Tooltip>
              <Tooltip title="Refresh data">
                <Button
                  icon={<ReloadOutlined />}
                  size="small"
                  loading={refreshing}
                  onClick={handleRefresh}
                />
              </Tooltip>
              <Tooltip title="Export transparency data">
                <Button
                  icon={<ExportOutlined />}
                  size="small"
                  onClick={handleExport}
                />
              </Tooltip>
            </Space>
          </div>
        }
        size={compact ? 'small' : 'default'}
        data-testid={testId}
      >
        <Tabs
          activeKey={activeTab}
          onChange={handleTabChange}
          items={tabItems}
          size={compact ? 'small' : 'default'}
        />
      </Card>
    </AIFeatureWrapper>
  )
}

export default AITransparencyPanel
