import React, { useState, useMemo, useCallback } from 'react'
import { 
  Typography, 
  Tooltip, 
  Tag, 
  Space, 
  Button, 
  Popover,
  Card,
  List,
  Switch,
  Select,
  Badge
} from 'antd'
import {
  TagsOutlined,
  InfoCircleOutlined,
  EyeOutlined,
  EyeInvisibleOutlined,
  HighlightOutlined,
  LinkOutlined
} from '@ant-design/icons'
import { ConfidenceIndicator } from '../common/ConfidenceIndicator'
import type { BusinessEntity, EntityRelationship } from '@shared/types/ai'

const { Text } = Typography

export interface EntityHighlighterProps {
  text: string
  entities: BusinessEntity[]
  interactive?: boolean
  showConfidence?: boolean
  showTooltips?: boolean
  showRelationships?: boolean
  highlightStyle?: 'underline' | 'background' | 'border' | 'badge'
  onEntityClick?: (entity: BusinessEntity, position: { start: number; end: number }) => void
  onEntityHover?: (entity: BusinessEntity | null) => void
  className?: string
  testId?: string
}

interface HighlightedSegment {
  text: string
  entity?: BusinessEntity
  start: number
  end: number
  isHighlighted: boolean
}

/**
 * EntityHighlighter - Interactive entity highlighting in text
 * 
 * Features:
 * - Intelligent entity detection and highlighting
 * - Interactive entity exploration with tooltips
 * - Confidence-based highlighting intensity
 * - Relationship visualization
 * - Multiple highlighting styles
 * - Real-time entity filtering
 */
export const EntityHighlighter: React.FC<EntityHighlighterProps> = ({
  text,
  entities,
  interactive = true,
  showConfidence = true,
  showTooltips = true,
  showRelationships = false,
  highlightStyle = 'background',
  onEntityClick,
  onEntityHover,
  className,
  testId = 'entity-highlighter'
}) => {
  const [hoveredEntity, setHoveredEntity] = useState<BusinessEntity | null>(null)
  const [selectedEntity, setSelectedEntity] = useState<BusinessEntity | null>(null)
  const [enabledTypes, setEnabledTypes] = useState<Set<string>>(new Set())
  const [highlightMode, setHighlightMode] = useState<'all' | 'high_confidence' | 'selected_types'>('all')

  // Initialize enabled types
  React.useEffect(() => {
    const types = new Set(entities.map(e => e.type))
    setEnabledTypes(types)
  }, [entities])

  // Get entity type color
  const getEntityTypeColor = (type: string) => {
    const colors = {
      table: '#1890ff',
      column: '#52c41a',
      metric: '#722ed1',
      dimension: '#fa8c16',
      filter: '#13c2c2',
      value: '#eb2f96',
      time: '#faad14',
      person: '#f759ab',
      organization: '#40a9ff',
      location: '#73d13d'
    }
    return colors[type as keyof typeof colors] || '#d9d9d9'
  }

  // Get highlight style based on confidence and type
  const getHighlightStyle = (entity: BusinessEntity, isHovered: boolean = false) => {
    const baseColor = getEntityTypeColor(entity.type)
    const opacity = Math.max(0.3, entity.confidence)
    
    const styles = {
      underline: {
        borderBottom: `2px solid ${baseColor}`,
        borderBottomStyle: entity.confidence > 0.8 ? 'solid' : 'dashed'
      },
      background: {
        backgroundColor: `${baseColor}${Math.round(opacity * 255).toString(16).padStart(2, '0')}`,
        borderRadius: '3px',
        padding: '1px 2px'
      },
      border: {
        border: `1px solid ${baseColor}`,
        borderRadius: '3px',
        padding: '1px 2px'
      },
      badge: {
        position: 'relative' as const,
        '&::after': {
          content: '""',
          position: 'absolute',
          top: '-2px',
          right: '-2px',
          width: '6px',
          height: '6px',
          backgroundColor: baseColor,
          borderRadius: '50%'
        }
      }
    }

    let style = styles[highlightStyle] || styles.background

    if (isHovered) {
      style = {
        ...style,
        boxShadow: `0 0 4px ${baseColor}`,
        transform: 'scale(1.02)'
      }
    }

    return style
  }

  // Check if entity should be highlighted
  const shouldHighlightEntity = (entity: BusinessEntity) => {
    switch (highlightMode) {
      case 'high_confidence':
        return entity.confidence > 0.8
      case 'selected_types':
        return enabledTypes.has(entity.type)
      default:
        return true
    }
  }

  // Create highlighted segments
  const highlightedSegments = useMemo(() => {
    const segments: HighlightedSegment[] = []
    let currentIndex = 0

    // Sort entities by start position to handle overlaps
    const sortedEntities = [...entities]
      .filter(shouldHighlightEntity)
      .sort((a, b) => a.startPosition - b.startPosition)

    for (const entity of sortedEntities) {
      // Add text before entity
      if (currentIndex < entity.startPosition) {
        segments.push({
          text: text.slice(currentIndex, entity.startPosition),
          start: currentIndex,
          end: entity.startPosition,
          isHighlighted: false
        })
      }

      // Add highlighted entity
      segments.push({
        text: text.slice(entity.startPosition, entity.endPosition),
        entity,
        start: entity.startPosition,
        end: entity.endPosition,
        isHighlighted: true
      })

      currentIndex = Math.max(currentIndex, entity.endPosition)
    }

    // Add remaining text
    if (currentIndex < text.length) {
      segments.push({
        text: text.slice(currentIndex),
        start: currentIndex,
        end: text.length,
        isHighlighted: false
      })
    }

    return segments
  }, [text, entities, highlightMode, enabledTypes])

  // Handle entity interaction
  const handleEntityClick = useCallback((entity: BusinessEntity, segment: HighlightedSegment) => {
    setSelectedEntity(entity)
    onEntityClick?.(entity, { start: segment.start, end: segment.end })
  }, [onEntityClick])

  const handleEntityHover = useCallback((entity: BusinessEntity | null) => {
    setHoveredEntity(entity)
    onEntityHover?.(entity)
  }, [onEntityHover])

  // Render entity tooltip content
  const renderEntityTooltip = (entity: BusinessEntity) => (
    <Card size="small" style={{ maxWidth: 300 }}>
      <Space direction="vertical" style={{ width: '100%' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Tag color={getEntityTypeColor(entity.type)}>{entity.type}</Tag>
          {showConfidence && (
            <ConfidenceIndicator
              confidence={entity.confidence}
              size="small"
              type="badge"
            />
          )}
        </div>
        
        <div>
          <Text strong>{entity.name}</Text>
          {entity.businessMeaning && (
            <div style={{ marginTop: 4 }}>
              <Text type="secondary">{entity.businessMeaning}</Text>
            </div>
          )}
        </div>

        {entity.context && (
          <div>
            <Text style={{ fontSize: '12px' }}>{entity.context}</Text>
          </div>
        )}

        {showRelationships && entity.relationships && entity.relationships.length > 0 && (
          <div>
            <Text strong style={{ fontSize: '12px' }}>Related:</Text>
            <div style={{ marginTop: 2 }}>
              {entity.relationships.slice(0, 3).map((rel, index) => (
                <Tag key={index} size="small" color="blue">
                  {rel.relatedEntity}
                </Tag>
              ))}
              {entity.relationships.length > 3 && (
                <Text type="secondary" style={{ fontSize: '11px' }}>
                  +{entity.relationships.length - 3} more
                </Text>
              )}
            </div>
          </div>
        )}
      </Space>
    </Card>
  )

  // Render controls
  const renderControls = () => (
    <Space wrap style={{ marginBottom: 12 }}>
      <Select
        value={highlightMode}
        onChange={setHighlightMode}
        size="small"
        style={{ width: 150 }}
      >
        <Select.Option value="all">All Entities</Select.Option>
        <Select.Option value="high_confidence">High Confidence</Select.Option>
        <Select.Option value="selected_types">Selected Types</Select.Option>
      </Select>

      <Select
        mode="multiple"
        placeholder="Entity Types"
        value={Array.from(enabledTypes)}
        onChange={(values) => setEnabledTypes(new Set(values))}
        size="small"
        style={{ minWidth: 120 }}
        disabled={highlightMode !== 'selected_types'}
      >
        {Array.from(new Set(entities.map(e => e.type))).map(type => (
          <Select.Option key={type} value={type}>
            <Space>
              <div 
                style={{ 
                  width: 8, 
                  height: 8, 
                  backgroundColor: getEntityTypeColor(type),
                  borderRadius: '50%' 
                }} 
              />
              {type}
            </Space>
          </Select.Option>
        ))}
      </Select>

      <Badge count={entities.filter(shouldHighlightEntity).length} size="small">
        <Button size="small" icon={<TagsOutlined />}>
          Entities
        </Button>
      </Badge>
    </Space>
  )

  // Render highlighted text
  const renderHighlightedText = () => (
    <div style={{ lineHeight: 1.6, fontSize: '14px' }}>
      {highlightedSegments.map((segment, index) => {
        if (!segment.isHighlighted || !segment.entity) {
          return <span key={index}>{segment.text}</span>
        }

        const isHovered = hoveredEntity?.id === segment.entity.id
        const isSelected = selectedEntity?.id === segment.entity.id
        
        const highlightedElement = (
          <span
            key={index}
            style={{
              ...getHighlightStyle(segment.entity, isHovered || isSelected),
              cursor: interactive ? 'pointer' : 'default',
              transition: 'all 0.2s ease',
              fontWeight: isSelected ? 'bold' : 'normal'
            }}
            onClick={() => interactive && handleEntityClick(segment.entity!, segment)}
            onMouseEnter={() => interactive && handleEntityHover(segment.entity!)}
            onMouseLeave={() => interactive && handleEntityHover(null)}
          >
            {segment.text}
          </span>
        )

        if (showTooltips && interactive) {
          return (
            <Tooltip
              key={index}
              title={renderEntityTooltip(segment.entity)}
              placement="top"
              mouseEnterDelay={0.5}
            >
              {highlightedElement}
            </Tooltip>
          )
        }

        return highlightedElement
      })}
    </div>
  )

  // Render entity legend
  const renderEntityLegend = () => {
    const entityTypes = Array.from(new Set(entities.map(e => e.type)))
    
    return (
      <div style={{ marginTop: 12, padding: 8, backgroundColor: '#fafafa', borderRadius: 4 }}>
        <Text strong style={{ fontSize: '12px', marginBottom: 8, display: 'block' }}>
          Entity Types:
        </Text>
        <Space wrap>
          {entityTypes.map(type => {
            const count = entities.filter(e => e.type === type).length
            const enabled = enabledTypes.has(type)
            
            return (
              <Tag
                key={type}
                color={enabled ? getEntityTypeColor(type) : 'default'}
                style={{ 
                  cursor: 'pointer',
                  opacity: enabled ? 1 : 0.5
                }}
                onClick={() => {
                  const newEnabledTypes = new Set(enabledTypes)
                  if (enabled) {
                    newEnabledTypes.delete(type)
                  } else {
                    newEnabledTypes.add(type)
                  }
                  setEnabledTypes(newEnabledTypes)
                  setHighlightMode('selected_types')
                }}
              >
                {type} ({count})
              </Tag>
            )
          })}
        </Space>
      </div>
    )
  }

  return (
    <div className={className} data-testid={testId}>
      {interactive && renderControls()}
      {renderHighlightedText()}
      {interactive && renderEntityLegend()}
    </div>
  )
}

export default EntityHighlighter
