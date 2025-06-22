import React, { useState, useEffect } from 'react'
import { 
  Card, 
  Space, 
  Typography, 
  Button, 
  Rate,
  Input,
  Alert,
  Tag,
  List,
  Avatar,
  Tooltip,
  Divider,
  notification
} from 'antd'
import {
  LikeOutlined,
  DislikeOutlined,
  MessageOutlined,
  SendOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  BulbOutlined,
  UserOutlined,
  ClockCircleOutlined
} from '@ant-design/icons'

const { Title, Text, Paragraph } = Typography
const { TextArea } = Input

export interface InstantFeedbackPanelProps {
  queryId?: string
  traceId?: string
  showRating?: boolean
  showComments?: boolean
  showSuggestions?: boolean
  onFeedbackSubmit?: (feedback: FeedbackData) => void
  onSuggestionSubmit?: (suggestion: string) => void
  className?: string
  testId?: string
}

export interface FeedbackData {
  queryId: string
  traceId?: string
  rating: number
  sentiment: 'positive' | 'negative' | 'neutral'
  comment?: string
  categories: string[]
  timestamp: string
  userId: string
}

export interface FeedbackItem {
  id: string
  type: 'rating' | 'comment' | 'suggestion'
  content: string
  rating?: number
  timestamp: string
  status: 'pending' | 'acknowledged' | 'implemented'
}

/**
 * InstantFeedbackPanel - Collects and displays real-time user feedback
 * 
 * Features:
 * - Quick rating system
 * - Comment collection
 * - Suggestion submission
 * - Feedback history
 * - Real-time acknowledgment
 * - Categorized feedback
 */
export const InstantFeedbackPanel: React.FC<InstantFeedbackPanelProps> = ({
  queryId,
  traceId,
  showRating = true,
  showComments = true,
  showSuggestions = true,
  onFeedbackSubmit,
  onSuggestionSubmit,
  className,
  testId = 'instant-feedback-panel'
}) => {
  const [rating, setRating] = useState<number>(0)
  const [comment, setComment] = useState<string>('')
  const [suggestion, setSuggestion] = useState<string>('')
  const [feedbackHistory, setFeedbackHistory] = useState<FeedbackItem[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [selectedCategories, setSelectedCategories] = useState<string[]>([])

  const feedbackCategories = [
    'Accuracy',
    'Speed',
    'Clarity',
    'Completeness',
    'Relevance',
    'User Experience'
  ]

  // Load existing feedback for this query
  useEffect(() => {
    if (queryId) {
      loadFeedbackHistory(queryId)
    }
  }, [queryId])

  const loadFeedbackHistory = (id: string) => {
    // Simulate loading feedback history
    const mockHistory: FeedbackItem[] = [
      {
        id: 'feedback-1',
        type: 'rating',
        content: 'Rated 4 stars',
        rating: 4,
        timestamp: new Date(Date.now() - 300000).toISOString(), // 5 minutes ago
        status: 'acknowledged'
      },
      {
        id: 'feedback-2',
        type: 'comment',
        content: 'The response was accurate but could be faster',
        timestamp: new Date(Date.now() - 180000).toISOString(), // 3 minutes ago
        status: 'acknowledged'
      }
    ]
    
    setFeedbackHistory(mockHistory)
  }

  const handleRatingChange = (value: number) => {
    setRating(value)
  }

  const handleCategoryToggle = (category: string) => {
    setSelectedCategories(prev => 
      prev.includes(category) 
        ? prev.filter(c => c !== category)
        : [...prev, category]
    )
  }

  const handleSubmitFeedback = async () => {
    if (!queryId || (!rating && !comment.trim())) {
      notification.warning({
        message: 'Incomplete Feedback',
        description: 'Please provide a rating or comment before submitting.'
      })
      return
    }

    setIsSubmitting(true)

    try {
      const feedbackData: FeedbackData = {
        queryId,
        traceId,
        rating,
        sentiment: rating >= 4 ? 'positive' : rating >= 3 ? 'neutral' : 'negative',
        comment: comment.trim() || undefined,
        categories: selectedCategories,
        timestamp: new Date().toISOString(),
        userId: 'current-user' // In real app, get from auth context
      }

      // Add to history immediately for instant feedback
      const newFeedbackItem: FeedbackItem = {
        id: `feedback-${Date.now()}`,
        type: rating > 0 && comment ? 'comment' : rating > 0 ? 'rating' : 'comment',
        content: comment || `Rated ${rating} stars`,
        rating: rating > 0 ? rating : undefined,
        timestamp: new Date().toISOString(),
        status: 'pending'
      }

      setFeedbackHistory(prev => [newFeedbackItem, ...prev])

      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 1000))

      // Update status to acknowledged
      setFeedbackHistory(prev => 
        prev.map(item => 
          item.id === newFeedbackItem.id 
            ? { ...item, status: 'acknowledged' }
            : item
        )
      )

      onFeedbackSubmit?.(feedbackData)

      // Reset form
      setRating(0)
      setComment('')
      setSelectedCategories([])

      notification.success({
        message: 'Feedback Submitted',
        description: 'Thank you for your feedback! It helps us improve the system.'
      })

    } catch (error) {
      notification.error({
        message: 'Submission Failed',
        description: 'Failed to submit feedback. Please try again.'
      })
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleSubmitSuggestion = async () => {
    if (!suggestion.trim()) {
      notification.warning({
        message: 'Empty Suggestion',
        description: 'Please enter a suggestion before submitting.'
      })
      return
    }

    setIsSubmitting(true)

    try {
      // Add to history immediately
      const newSuggestion: FeedbackItem = {
        id: `suggestion-${Date.now()}`,
        type: 'suggestion',
        content: suggestion.trim(),
        timestamp: new Date().toISOString(),
        status: 'pending'
      }

      setFeedbackHistory(prev => [newSuggestion, ...prev])

      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 1000))

      // Update status
      setFeedbackHistory(prev => 
        prev.map(item => 
          item.id === newSuggestion.id 
            ? { ...item, status: 'acknowledged' }
            : item
        )
      )

      onSuggestionSubmit?.(suggestion.trim())

      setSuggestion('')

      notification.success({
        message: 'Suggestion Submitted',
        description: 'Your suggestion has been recorded and will be reviewed.'
      })

    } catch (error) {
      notification.error({
        message: 'Submission Failed',
        description: 'Failed to submit suggestion. Please try again.'
      })
    } finally {
      setIsSubmitting(false)
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'pending': return <ClockCircleOutlined style={{ color: '#faad14' }} />
      case 'acknowledged': return <CheckCircleOutlined style={{ color: '#52c41a' }} />
      case 'implemented': return <CheckCircleOutlined style={{ color: '#1890ff' }} />
      default: return <ClockCircleOutlined />
    }
  }

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'rating': return <LikeOutlined />
      case 'comment': return <MessageOutlined />
      case 'suggestion': return <BulbOutlined />
      default: return <MessageOutlined />
    }
  }

  if (!queryId) {
    return (
      <Card className={className} data-testid={testId}>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Space direction="vertical">
            <MessageOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
            <Text type="secondary">No query selected for feedback</Text>
          </Space>
        </div>
      </Card>
    )
  }

  return (
    <Card 
      title={
        <Space>
          <MessageOutlined />
          <span>Instant Feedback</span>
          <Tag color="blue">{queryId.substring(0, 8)}...</Tag>
        </Space>
      }
      className={className}
      data-testid={testId}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        {/* Quick Rating */}
        {showRating && (
          <Card size="small" title="Rate this Response">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Rate 
                value={rating} 
                onChange={handleRatingChange}
                style={{ fontSize: '24px' }}
              />
              <Text type="secondary">
                {rating === 0 ? 'Click to rate' :
                 rating === 1 ? 'Poor' :
                 rating === 2 ? 'Fair' :
                 rating === 3 ? 'Good' :
                 rating === 4 ? 'Very Good' : 'Excellent'}
              </Text>
            </Space>
          </Card>
        )}

        {/* Categories */}
        <Card size="small" title="Feedback Categories">
          <Space wrap>
            {feedbackCategories.map(category => (
              <Tag
                key={category}
                color={selectedCategories.includes(category) ? 'blue' : 'default'}
                style={{ cursor: 'pointer' }}
                onClick={() => handleCategoryToggle(category)}
              >
                {category}
              </Tag>
            ))}
          </Space>
        </Card>

        {/* Comment Section */}
        {showComments && (
          <Card size="small" title="Additional Comments">
            <Space direction="vertical" style={{ width: '100%' }}>
              <TextArea
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                placeholder="Share your thoughts about this response..."
                rows={3}
                maxLength={500}
                showCount
              />
              <Button 
                type="primary" 
                icon={<SendOutlined />}
                onClick={handleSubmitFeedback}
                loading={isSubmitting}
                disabled={!rating && !comment.trim()}
              >
                Submit Feedback
              </Button>
            </Space>
          </Card>
        )}

        {/* Suggestions */}
        {showSuggestions && (
          <Card size="small" title="Suggestions for Improvement">
            <Space direction="vertical" style={{ width: '100%' }}>
              <TextArea
                value={suggestion}
                onChange={(e) => setSuggestion(e.target.value)}
                placeholder="How can we improve this response or the system?"
                rows={2}
                maxLength={300}
                showCount
              />
              <Button 
                icon={<BulbOutlined />}
                onClick={handleSubmitSuggestion}
                loading={isSubmitting}
                disabled={!suggestion.trim()}
              >
                Submit Suggestion
              </Button>
            </Space>
          </Card>
        )}

        <Divider />

        {/* Feedback History */}
        <Card size="small" title={`Feedback History (${feedbackHistory.length})`}>
          {feedbackHistory.length > 0 ? (
            <List
              dataSource={feedbackHistory.slice(0, 5)} // Show last 5 items
              renderItem={(item) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={
                      <Avatar 
                        icon={getTypeIcon(item.type)} 
                        style={{ backgroundColor: '#1890ff' }}
                      />
                    }
                    title={
                      <Space>
                        <Text strong>{item.type.charAt(0).toUpperCase() + item.type.slice(1)}</Text>
                        {item.rating && (
                          <Rate disabled value={item.rating} style={{ fontSize: '14px' }} />
                        )}
                        <Tooltip title={`Status: ${item.status}`}>
                          {getStatusIcon(item.status)}
                        </Tooltip>
                      </Space>
                    }
                    description={
                      <Space direction="vertical" size="small">
                        <Text>{item.content}</Text>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {new Date(item.timestamp).toLocaleString()}
                        </Text>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          ) : (
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <Space direction="vertical">
                <MessageOutlined style={{ fontSize: '24px', color: '#d9d9d9' }} />
                <Text type="secondary">No feedback history</Text>
              </Space>
            </div>
          )}
        </Card>

        {/* Quick Actions */}
        <Space>
          <Button 
            icon={<LikeOutlined />} 
            onClick={() => {
              setRating(5)
              handleSubmitFeedback()
            }}
            disabled={isSubmitting}
          >
            Quick Like
          </Button>
          <Button 
            icon={<DislikeOutlined />} 
            onClick={() => {
              setRating(2)
              handleSubmitFeedback()
            }}
            disabled={isSubmitting}
          >
            Quick Dislike
          </Button>
        </Space>
      </Space>
    </Card>
  )
}

export default InstantFeedbackPanel
