import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Progress, 
  Row, 
  Col, 
  Typography, 
  Alert, 
  List, 
  Tag, 
  Button,
  Space,
  Tooltip,
  Divider,
  Statistic,
  Rate
} from 'antd'
import { 
  CheckCircleOutlined, 
  ExclamationCircleOutlined, 
  CloseCircleOutlined,
  InfoCircleOutlined,
  TrophyOutlined,
  BugOutlined,
  RocketOutlined,
  ShieldOutlined,
  CodeOutlined,
  FileTextOutlined
} from '@ant-design/icons'

const { Title, Text } = Typography

interface QualityMetric {
  name: string
  score: number
  maxScore: number
  status: 'excellent' | 'good' | 'fair' | 'poor'
  description: string
  recommendations: string[]
  weight: number
}

interface TemplateQualityAssessmentProps {
  template?: any
  onImprove?: (recommendations: string[]) => void
}

export const TemplateQualityAssessment: React.FC<TemplateQualityAssessmentProps> = ({
  template,
  onImprove
}) => {
  const [qualityMetrics, setQualityMetrics] = useState<QualityMetric[]>([])
  const [overallScore, setOverallScore] = useState(0)

  useEffect(() => {
    if (template) {
      // Calculate quality metrics based on template data
      const metrics = calculateQualityMetrics(template)
      setQualityMetrics(metrics)
      
      // Calculate weighted overall score
      const totalWeight = metrics.reduce((sum, metric) => sum + metric.weight, 0)
      const weightedScore = metrics.reduce((sum, metric) => 
        sum + (metric.score / metric.maxScore) * metric.weight, 0
      )
      setOverallScore((weightedScore / totalWeight) * 100)
    }
  }, [template])

  const calculateQualityMetrics = (template: any): QualityMetric[] => {
    const metrics: QualityMetric[] = []

    // Syntax Quality
    const syntaxScore = analyzeSyntax(template.content)
    metrics.push({
      name: 'Syntax Quality',
      score: syntaxScore,
      maxScore: 100,
      status: getStatus(syntaxScore),
      description: 'Template syntax correctness and structure',
      recommendations: getSyntaxRecommendations(syntaxScore),
      weight: 25
    })

    // Performance Metrics
    const performanceScore = template.performanceMetrics?.successRate * 100 || 0
    metrics.push({
      name: 'Performance',
      score: performanceScore,
      maxScore: 100,
      status: getStatus(performanceScore),
      description: 'Template execution success rate and reliability',
      recommendations: getPerformanceRecommendations(performanceScore),
      weight: 30
    })

    // Documentation Quality
    const docScore = analyzeDocumentation(template)
    metrics.push({
      name: 'Documentation',
      score: docScore,
      maxScore: 100,
      status: getStatus(docScore),
      description: 'Completeness of business metadata and documentation',
      recommendations: getDocumentationRecommendations(docScore),
      weight: 20
    })

    // Security & Compliance
    const securityScore = analyzeSecurityCompliance(template.content)
    metrics.push({
      name: 'Security & Compliance',
      score: securityScore,
      maxScore: 100,
      status: getStatus(securityScore),
      description: 'Security best practices and compliance adherence',
      recommendations: getSecurityRecommendations(securityScore),
      weight: 15
    })

    // Maintainability
    const maintainabilityScore = analyzeMaintainability(template)
    metrics.push({
      name: 'Maintainability',
      score: maintainabilityScore,
      maxScore: 100,
      status: getStatus(maintainabilityScore),
      description: 'Code clarity, modularity, and ease of maintenance',
      recommendations: getMaintainabilityRecommendations(maintainabilityScore),
      weight: 10
    })

    return metrics
  }

  const analyzeSyntax = (content: string): number => {
    let score = 100
    
    // Check for balanced braces
    const openBraces = (content.match(/\{\{/g) || []).length
    const closeBraces = (content.match(/\}\}/g) || []).length
    if (openBraces !== closeBraces) score -= 20

    // Check for common variables
    if (!content.includes('{{user_query}}')) score -= 15
    
    // Check for proper structure
    if (content.length < 50) score -= 25
    if (content.length > 2000) score -= 10

    return Math.max(0, score)
  }

  const analyzeDocumentation = (template: any): number => {
    let score = 0
    const metadata = template.businessMetadata

    if (template.description) score += 20
    if (metadata?.businessPurpose) score += 25
    if (metadata?.businessContext) score += 15
    if (metadata?.useCases?.length > 0) score += 15
    if (metadata?.targetAudience?.length > 0) score += 10
    if (metadata?.businessOwner) score += 15

    return Math.min(100, score)
  }

  const analyzeSecurityCompliance = (content: string): number => {
    let score = 100

    // Check for dangerous SQL patterns
    const dangerousPatterns = [
      /drop\s+table/i,
      /delete\s+from.*without.*where/i,
      /truncate\s+table/i,
      /alter\s+table/i,
      /create\s+table/i
    ]

    dangerousPatterns.forEach(pattern => {
      if (pattern.test(content)) score -= 30
    })

    // Check for SQL injection vulnerabilities
    if (content.includes("' OR '1'='1")) score -= 50
    if (content.includes('; DROP TABLE')) score -= 50

    return Math.max(0, score)
  }

  const analyzeMaintainability = (template: any): number => {
    let score = 50 // Base score

    // Check for version control
    if (template.version) score += 15

    // Check for tags
    if (template.tags?.length > 0) score += 10

    // Check for active status
    if (template.isActive) score += 10

    // Check for recent updates
    const lastModified = new Date(template.lastModified)
    const daysSinceUpdate = (Date.now() - lastModified.getTime()) / (1000 * 60 * 60 * 24)
    if (daysSinceUpdate < 30) score += 15

    return Math.min(100, score)
  }

  const getStatus = (score: number): 'excellent' | 'good' | 'fair' | 'poor' => {
    if (score >= 90) return 'excellent'
    if (score >= 70) return 'good'
    if (score >= 50) return 'fair'
    return 'poor'
  }

  const getSyntaxRecommendations = (score: number): string[] => {
    const recommendations = []
    if (score < 90) {
      recommendations.push('Review template syntax for balanced braces and proper variable usage')
      recommendations.push('Ensure all required variables like {{user_query}} are included')
      recommendations.push('Optimize template length for better readability')
    }
    return recommendations
  }

  const getPerformanceRecommendations = (score: number): string[] => {
    const recommendations = []
    if (score < 80) {
      recommendations.push('Analyze failed executions to identify common issues')
      recommendations.push('Consider A/B testing with improved template variants')
      recommendations.push('Review and optimize prompt engineering techniques')
    }
    return recommendations
  }

  const getDocumentationRecommendations = (score: number): string[] => {
    const recommendations = []
    if (score < 70) {
      recommendations.push('Add comprehensive business purpose description')
      recommendations.push('Define clear use cases and target audience')
      recommendations.push('Assign business and technical owners')
      recommendations.push('Include business context and rules')
    }
    return recommendations
  }

  const getSecurityRecommendations = (score: number): string[] => {
    const recommendations = []
    if (score < 90) {
      recommendations.push('Remove or secure dangerous SQL operations')
      recommendations.push('Implement proper input validation')
      recommendations.push('Review template for SQL injection vulnerabilities')
      recommendations.push('Follow security best practices for data access')
    }
    return recommendations
  }

  const getMaintainabilityRecommendations = (score: number): string[] => {
    const recommendations = []
    if (score < 80) {
      recommendations.push('Add descriptive tags for better categorization')
      recommendations.push('Ensure template is actively maintained')
      recommendations.push('Update version information regularly')
      recommendations.push('Review and update template content periodically')
    }
    return recommendations
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'excellent': return '#52c41a'
      case 'good': return '#1890ff'
      case 'fair': return '#fa8c16'
      case 'poor': return '#f5222d'
      default: return '#d9d9d9'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'excellent': return <CheckCircleOutlined />
      case 'good': return <CheckCircleOutlined />
      case 'fair': return <ExclamationCircleOutlined />
      case 'poor': return <CloseCircleOutlined />
      default: return <InfoCircleOutlined />
    }
  }

  const getAllRecommendations = () => {
    return qualityMetrics
      .filter(metric => metric.status !== 'excellent')
      .flatMap(metric => metric.recommendations)
  }

  return (
    <div>
      {/* Overall Quality Score */}
      <Card style={{ marginBottom: '16px' }}>
        <Row gutter={16} align="middle">
          <Col span={8}>
            <div style={{ textAlign: 'center' }}>
              <Progress
                type="circle"
                percent={Math.round(overallScore)}
                strokeColor={getStatusColor(getStatus(overallScore))}
                size={120}
                format={(percent) => (
                  <div>
                    <div style={{ fontSize: '24px', fontWeight: 'bold' }}>{percent}</div>
                    <div style={{ fontSize: '12px' }}>Quality Score</div>
                  </div>
                )}
              />
            </div>
          </Col>
          <Col span={16}>
            <Title level={4}>Template Quality Assessment</Title>
            <Text type="secondary">
              Comprehensive analysis of template quality across multiple dimensions
            </Text>
            <div style={{ marginTop: '16px' }}>
              <Rate 
                disabled 
                value={overallScore / 20} 
                character={<TrophyOutlined />}
                style={{ color: getStatusColor(getStatus(overallScore)) }}
              />
              <Text style={{ marginLeft: '8px', fontWeight: 'bold' }}>
                {getStatus(overallScore).toUpperCase()}
              </Text>
            </div>
          </Col>
        </Row>
      </Card>

      {/* Quality Metrics Breakdown */}
      <Row gutter={16} style={{ marginBottom: '16px' }}>
        {qualityMetrics.map((metric, index) => (
          <Col span={12} key={index} style={{ marginBottom: '16px' }}>
            <Card size="small">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
                <Text strong>{metric.name}</Text>
                <Space>
                  {getStatusIcon(metric.status)}
                  <Text style={{ color: getStatusColor(metric.status), fontWeight: 'bold' }}>
                    {Math.round((metric.score / metric.maxScore) * 100)}%
                  </Text>
                </Space>
              </div>
              <Progress 
                percent={Math.round((metric.score / metric.maxScore) * 100)}
                strokeColor={getStatusColor(metric.status)}
                size="small"
              />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {metric.description}
              </Text>
            </Card>
          </Col>
        ))}
      </Row>

      {/* Recommendations */}
      {getAllRecommendations().length > 0 && (
        <Card 
          title={
            <Space>
              <RocketOutlined />
              Quality Improvement Recommendations
            </Space>
          }
          extra={
            <Button 
              type="primary" 
              size="small"
              onClick={() => onImprove?.(getAllRecommendations())}
            >
              Apply Improvements
            </Button>
          }
        >
          <List
            size="small"
            dataSource={getAllRecommendations()}
            renderItem={(recommendation, index) => (
              <List.Item>
                <Space>
                  <Tag color="blue">{index + 1}</Tag>
                  <Text>{recommendation}</Text>
                </Space>
              </List.Item>
            )}
          />
        </Card>
      )}

      {/* Quality Trends */}
      <Card title="Quality Trends" style={{ marginTop: '16px' }}>
        <Alert
          message="Quality Monitoring"
          description="Track quality metrics over time to identify improvement trends and potential issues. Regular quality assessments help maintain high template standards."
          type="info"
          showIcon
        />
      </Card>
    </div>
  )
}

export default TemplateQualityAssessment
