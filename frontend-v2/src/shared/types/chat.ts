/**
 * Comprehensive Chat Interface Types
 * Production-ready type definitions for the BI Reporting Copilot chat system
 */

export interface ChatMessage {
  id: string
  conversationId: string
  type: 'user' | 'assistant' | 'system' | 'error'
  content: string
  timestamp: Date
  status: 'sending' | 'sent' | 'delivered' | 'error'
  
  // Enhanced metadata
  metadata?: {
    queryId?: string
    sessionId?: string
    executionTime?: number
    tokenCount?: number
    model?: string
    confidence?: number
  }
  
  // SQL and results
  sql?: string
  sqlFormatted?: string
  results?: QueryResult[]
  resultMetadata?: QueryMetadata
  
  // Semantic analysis
  semanticAnalysis?: SemanticAnalysis
  
  // User interaction
  isFavorite?: boolean
  isEdited?: boolean
  editHistory?: string[]
  reactions?: MessageReaction[]
  
  // Threading
  parentMessageId?: string
  threadMessages?: ChatMessage[]
  
  // Error handling
  error?: {
    code: string
    message: string
    details?: any
    retryable?: boolean
  }
}

export interface QueryResult {
  [key: string]: any
}

export interface QueryMetadata {
  executionTime: number
  rowCount: number
  columnCount: number
  queryComplexity: 'Simple' | 'Medium' | 'Complex'
  tablesUsed: string[]
  estimatedCost: number
  cacheHit?: boolean
  dataSource?: string
}

export interface SemanticAnalysis {
  intent: 'Aggregation' | 'Filter' | 'Join' | 'Comparison' | 'Trend' | 'Exploration'
  confidence: number
  entities: Array<{
    name: string
    type: 'table' | 'column' | 'metric' | 'dimension' | 'filter'
    confidence: number
    businessMeaning?: string
  }>
  businessTerms: Array<{
    term: string
    definition: string
    confidence: number
  }>
  ambiguities: Array<{
    term: string
    possibleMeanings: string[]
    suggestedClarification: string
  }>
  suggestedQueries: string[]
  relevantTables: Array<{
    tableName: string
    schemaName: string
    relevanceScore: number
    businessPurpose: string
  }>
}

export interface MessageReaction {
  type: 'like' | 'dislike' | 'helpful' | 'unclear'
  userId: string
  timestamp: Date
}

export interface Conversation {
  id: string
  title: string
  createdAt: Date
  updatedAt: Date
  messageCount: number
  isActive: boolean
  tags: string[]
  summary?: string
  userId: string
  
  // Context management
  context: ConversationContext
  
  // Sharing and collaboration
  isShared?: boolean
  sharedWith?: string[]
  permissions?: ConversationPermissions
}

export interface ConversationContext {
  // Business context
  selectedTables?: string[]
  selectedColumns?: string[]
  businessDomain?: string
  
  // Query context
  lastSuccessfulQuery?: string
  queryHistory: string[]
  commonPatterns: string[]
  
  // User preferences
  preferredChartTypes?: string[]
  defaultRowLimit?: number
  outputFormat?: 'table' | 'chart' | 'both'
  
  // Session state
  currentSchema?: string
  activeFilters?: Record<string, any>
  temporaryTables?: string[]
}

export interface ConversationPermissions {
  canEdit: boolean
  canShare: boolean
  canDelete: boolean
  canExport: boolean
}

export interface StreamingProgress {
  sessionId: string
  phase: 'parsing' | 'analyzing' | 'executing' | 'formatting' | 'complete' | 'error'
  progress: number
  message: string
  timestamp: Date
  
  // Phase-specific data
  phaseData?: {
    parsing?: {
      tokensProcessed: number
      entitiesFound: string[]
    }
    analyzing?: {
      tablesIdentified: string[]
      confidence: number
    }
    executing?: {
      queryPlan?: string
      estimatedTime?: number
    }
    formatting?: {
      rowsProcessed: number
      totalRows: number
    }
  }
}

export interface QuerySuggestion {
  id: string
  text: string
  description: string
  category: 'recent' | 'popular' | 'recommended' | 'template'
  confidence: number
  metadata?: {
    tablesUsed: string[]
    complexity: string
    estimatedTime: number
  }
}

export interface ChatSettings {
  // Display preferences
  theme: 'light' | 'dark' | 'auto'
  fontSize: 'small' | 'medium' | 'large'
  messageGrouping: boolean
  showTimestamps: boolean
  showMetadata: boolean
  
  // Behavior preferences
  autoExecuteQueries: boolean
  showSuggestions: boolean
  enableNotifications: boolean
  saveHistory: boolean
  
  // Advanced features
  enableSemanticAnalysis: boolean
  showConfidenceScores: boolean
  enableRealTimeStreaming: boolean
  maxHistoryItems: number
}

export interface ExportOptions {
  format: 'csv' | 'excel' | 'pdf' | 'json'
  includeMetadata: boolean
  includeCharts: boolean
  dateRange?: {
    start: Date
    end: Date
  }
  filters?: Record<string, any>
}

// Redux state interfaces
export interface ChatState {
  // Current conversation
  currentConversation: Conversation | null
  messages: ChatMessage[]
  
  // UI state
  isTyping: boolean
  isLoading: boolean
  isConnected: boolean
  streamingProgress: StreamingProgress | null
  
  // Input state
  inputValue: string
  suggestions: QuerySuggestion[]
  showSuggestions: boolean
  
  // History and context
  conversations: Conversation[]
  recentQueries: string[]
  favoriteQueries: string[]
  
  // Settings
  settings: ChatSettings
  
  // Error handling
  error: string | null
  connectionError: string | null
}

// API request/response types
export interface SendMessageRequest {
  conversationId?: string
  message: string
  context?: Partial<ConversationContext>
  options?: {
    includeSemanticAnalysis?: boolean
    enableStreaming?: boolean
    maxResults?: number
  }
}

export interface SendMessageResponse {
  message: ChatMessage
  conversation: Conversation
  suggestions?: QuerySuggestion[]
}

export interface GetConversationsRequest {
  page?: number
  limit?: number
  search?: string
  tags?: string[]
  dateRange?: {
    start: Date
    end: Date
  }
}

export interface GetConversationsResponse {
  conversations: Conversation[]
  total: number
  page: number
  hasMore: boolean
}
