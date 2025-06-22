import React, { useState, useMemo } from 'react'
import { 
  Card, 
  Tree, 
  Space, 
  Typography, 
  Tag, 
  Button, 
  Tooltip, 
  Modal,
  Descriptions,
  Progress,
  Alert,
  Row,
  Col,
  Statistic
} from 'antd'
import {
  BranchesOutlined,
  NodeIndexOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined,
  EyeOutlined,
  BarChartOutlined,
  ClockCircleOutlined,
  BulbOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { PromptConstructionStep, AlternativeOption } from '@shared/types/transparency'
import type { DataNode } from 'antd/es/tree'

const { Title, Text, Paragraph } = Typography

export interface DecisionTreeViewerProps {
  steps: PromptConstructionStep[]
  alternatives?: AlternativeOption[]
  showAlternatives?: boolean
  showMetrics?: boolean
  expandAll?: boolean
  onNodeSelect?: (node: DecisionNode) => void
  className?: string
  testId?: string
}

interface DecisionNode {
  id: string
  title: string
  type: 'decision' | 'action' | 'result' | 'alternative'
  confidence: number
  impact: number
  reasoning: string
  metrics: {
    processingTime: number
    tokens: number
    success: boolean
  }
  children?: DecisionNode[]
  alternatives?: DecisionNode[]
}

/**
 * DecisionTreeViewer - Visualizes AI decision-making process as a hierarchical tree
 * 
 * Features:
 * - Interactive decision tree visualization
 * - Alternative path exploration
 * - Confidence-based node styling
 * - Detailed node inspection
 * - Decision reasoning display
 * - Performance impact analysis
 */
export const DecisionTreeViewer: React.FC<DecisionTreeViewerProps> = ({
  steps,
  alternatives = [],
  showAlternatives = true,
  showMetrics = true,
  expandAll = false,
  onNodeSelect,
  className,
  testId = 'decision-tree-viewer'
}) => {
  const [selectedNode, setSelectedNode] = useState<DecisionNode | null>(null)
  const [modalVisible, setModalVisible] = useState(false)
  const [expandedKeys, setExpandedKeys] = useState<React.Key[]>([])

  // Transform steps into decision tree structure
  const decisionTree = useMemo((): DecisionNode[] => {
    if (!steps || steps.length === 0) return []

    const nodes: DecisionNode[] = []
    let currentParent: DecisionNode | null = null

    steps.forEach((step, index) => {
      const node: DecisionNode = {
        id: step.id,
        title: step.stepName,
        type: getNodeType(step.stepName),
        confidence: step.confidence,
        impact: calculateImpact(step, index, steps.length),
        reasoning: step.content.substring(0, 200) + (step.content.length > 200 ? '...' : ''),
        metrics: {
          processingTime: step.processingTimeMs,
          tokens: step.tokensAdded,
          success: step.success
        },
        children: [],
        alternatives: []
      }

      // Add alternatives if available
      if (showAlternatives && alternatives.length > 0) {
        const stepAlternatives = alternatives.filter(alt => 
          alt.description.toLowerCase().includes(step.stepName.toLowerCase())
        )
        
        node.alternatives = stepAlternatives.map(alt => ({
          id: `alt-${alt.id}`,
          title: alt.title,
          type: 'alternative' as const,
          confidence: alt.confidence,
          impact: alt.impact,
          reasoning: alt.description,
          metrics: {
            processingTime: alt.estimatedTime || 0,
            tokens: alt.estimatedTokens || 0,
            success: true
          }
        }))
      }

      // Build hierarchical structure
      if (index === 0 || !isChildStep(step, steps[index - 1])) {
        nodes.push(node)
        currentParent = node
      } else if (currentParent) {
        currentParent.children = currentParent.children || []
        currentParent.children.push(node)
      }
    })

    return nodes
  }, [steps, alternatives, showAlternatives])

  // Convert to Ant Design Tree format
  const treeData = useMemo((): DataNode[] => {
    const convertToTreeData = (nodes: DecisionNode[]): DataNode[] => {
      return nodes.map(node => ({
        key: node.id,
        title: renderNodeTitle(node),
        children: [
          ...(node.children ? convertToTreeData(node.children) : []),
          ...(node.alternatives ? node.alternatives.map(alt => ({
            key: alt.id,
            title: renderNodeTitle(alt),
            isLeaf: true,
            className: 'alternative-node'
          })) : [])
        ].filter(Boolean)
      }))
    }

    return convertToTreeData(decisionTree)
  }, [decisionTree])

  function getNodeType(stepName: string): DecisionNode['type'] {
    const name = stepName.toLowerCase()
    if (name.includes('decision') || name.includes('choose') || name.includes('select')) return 'decision'
    if (name.includes('action') || name.includes('execute') || name.includes('process')) return 'action'
    return 'result'
  }

  function calculateImpact(step: PromptConstructionStep, index: number, total: number): number {
    // Impact based on position in flow, confidence, and token usage
    const positionWeight = (total - index) / total // Later steps have higher impact
    const confidenceWeight = step.confidence
    const tokenWeight = Math.min(1, step.tokensAdded / 1000) // Normalize token impact
    
    return (positionWeight * 0.4 + confidenceWeight * 0.4 + tokenWeight * 0.2) * 100
  }

  function isChildStep(current: PromptConstructionStep, previous: PromptConstructionStep): boolean {
    // Simple heuristic: if current step order is sequential and processing time is lower
    return current.stepOrder === previous.stepOrder + 1 && 
           current.processingTimeMs < previous.processingTimeMs * 1.5
  }

  function renderNodeTitle(node: DecisionNode) {
    return (
      <Space>
        <Text strong={node.type === 'decision'}>{node.title}</Text>
        <ConfidenceIndicator
          confidence={node.confidence}
          size="small"
          type="badge"
        />
        <Tag 
          color={
            node.type === 'decision' ? 'purple' :
            node.type === 'alternative' ? 'orange' :
            node.type === 'action' ? 'blue' : 'green'
          }
          size="small"
        >
          {node.type}
        </Tag>
        {showMetrics && (
          <Space size="small">
            <Tag size="small">{node.metrics.processingTime}ms</Tag>
            <Tag size="small">{node.metrics.tokens}t</Tag>
          </Space>
        )}
        {!node.metrics.success && (
          <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />
        )}
      </Space>
    )
  }

  const handleNodeSelect = (selectedKeys: React.Key[], info: any) => {
    if (selectedKeys.length > 0) {
      const nodeId = selectedKeys[0] as string
      const findNode = (nodes: DecisionNode[]): DecisionNode | null => {
        for (const node of nodes) {
          if (node.id === nodeId) return node
          if (node.children) {
            const found = findNode(node.children)
            if (found) return found
          }
          if (node.alternatives) {
            const found = node.alternatives.find(alt => alt.id === nodeId)
            if (found) return found
          }
        }
        return null
      }

      const node = findNode(decisionTree)
      if (node) {
        setSelectedNode(node)
        onNodeSelect?.(node)
      }
    }
  }

  const handleNodeDoubleClick = () => {
    if (selectedNode) {
      setModalVisible(true)
    }
  }

  const renderNodeDetails = (node: DecisionNode) => (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Card title="Node Information" size="small">
        <Descriptions column={2} size="small">
          <Descriptions.Item label="Type">{node.type}</Descriptions.Item>
          <Descriptions.Item label="Confidence">
            <ConfidenceIndicator
              confidence={node.confidence}
              size="small"
              type="badge"
              showPercentage
            />
          </Descriptions.Item>
          <Descriptions.Item label="Impact Score">
            {node.impact.toFixed(1)}%
          </Descriptions.Item>
          <Descriptions.Item label="Success">
            {node.metrics.success ? 'Yes' : 'No'}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Card title="Performance Metrics" size="small">
        <Row gutter={[16, 16]}>
          <Col span={8}>
            <Statistic
              title="Processing Time"
              value={node.metrics.processingTime}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
            />
          </Col>
          <Col span={8}>
            <Statistic
              title="Tokens Used"
              value={node.metrics.tokens}
              prefix={<BarChartOutlined />}
            />
          </Col>
          <Col span={8}>
            <Statistic
              title="Impact Score"
              value={node.impact.toFixed(1)}
              suffix="%"
              prefix={<BulbOutlined />}
              valueStyle={{ 
                color: node.impact > 70 ? '#3f8600' : node.impact > 40 ? '#faad14' : '#cf1322' 
              }}
            />
          </Col>
        </Row>
      </Card>

      <Card title="Reasoning" size="small">
        <Paragraph>
          {node.reasoning}
        </Paragraph>
      </Card>

      {node.alternatives && node.alternatives.length > 0 && (
        <Card title="Alternative Options" size="small">
          <Space direction="vertical" style={{ width: '100%' }}>
            {node.alternatives.map(alt => (
              <Card key={alt.id} size="small" style={{ background: '#fafafa' }}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Space>
                    <Text strong>{alt.title}</Text>
                    <ConfidenceIndicator
                      confidence={alt.confidence}
                      size="small"
                      type="badge"
                    />
                  </Space>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    {alt.reasoning}
                  </Text>
                </Space>
              </Card>
            ))}
          </Space>
        </Card>
      )}
    </Space>
  )

  if (!decisionTree || decisionTree.length === 0) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <BranchesOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No decision tree data available</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <div className={className} data-testid={testId}>
      <Card 
        title={
          <Space>
            <BranchesOutlined />
            <span>Decision Tree</span>
            <Tag color="blue">{decisionTree.length} root decisions</Tag>
          </Space>
        }
        extra={
          <Space>
            <Button 
              size="small" 
              onClick={() => setExpandedKeys(expandAll ? [] : treeData.map(node => node.key))}
            >
              {expandAll ? 'Collapse All' : 'Expand All'}
            </Button>
            {selectedNode && (
              <Button 
                size="small" 
                icon={<EyeOutlined />}
                onClick={() => setModalVisible(true)}
              >
                Details
              </Button>
            )}
          </Space>
        }
      >
        <Tree
          treeData={treeData}
          onSelect={handleNodeSelect}
          onDoubleClick={handleNodeDoubleClick}
          expandedKeys={expandedKeys}
          onExpand={setExpandedKeys}
          showLine={{ showLeafIcon: false }}
          defaultExpandAll={expandAll}
        />
      </Card>

      <Modal
        title={selectedNode ? `${selectedNode.title} - Details` : 'Node Details'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        footer={null}
        width={800}
      >
        {selectedNode && renderNodeDetails(selectedNode)}
      </Modal>
    </div>
  )
}

export default DecisionTreeViewer
