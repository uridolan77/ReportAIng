import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from './index'
import type {
  ChatState,
  ChatMessage,
  Conversation,
  StreamingProgress,
  QuerySuggestion,
  ChatSettings,
  ConversationContext
} from '../types/chat'

const initialSettings: ChatSettings = {
  theme: 'light',
  fontSize: 'medium',
  messageGrouping: true,
  showTimestamps: true,
  showMetadata: false,
  autoExecuteQueries: false,
  showSuggestions: true,
  enableNotifications: true,
  saveHistory: true,
  enableSemanticAnalysis: true,
  showConfidenceScores: false,
  enableRealTimeStreaming: true,
  maxHistoryItems: 100,
}

const initialState: ChatState = {
  currentConversation: null,
  messages: [],
  isTyping: false,
  isLoading: false,
  isConnected: false,
  streamingProgress: null,
  inputValue: '',
  suggestions: [],
  showSuggestions: false,
  conversations: [],
  recentQueries: [],
  favoriteQueries: [],
  settings: initialSettings,
  error: null,
  connectionError: null,
}

export const chatSlice = createSlice({
  name: 'chat',
  initialState,
  reducers: {
    // Conversation management
    setCurrentConversation: (state, action: PayloadAction<Conversation | null>) => {
      state.currentConversation = action.payload
      if (action.payload) {
        // Load messages for this conversation
        state.messages = [] // Will be loaded by RTK Query
      } else {
        state.messages = []
      }
    },

    createNewConversation: (state, action: PayloadAction<{ title?: string; context?: Partial<ConversationContext> }>) => {
      const newConversation: Conversation = {
        id: `conv_${Date.now()}`,
        title: action.payload.title || 'New Conversation',
        createdAt: new Date(),
        updatedAt: new Date(),
        messageCount: 0,
        isActive: true,
        tags: [],
        userId: 'current-user', // Will be set from auth state
        context: {
          selectedTables: [],
          selectedColumns: [],
          queryHistory: [],
          commonPatterns: [],
          ...action.payload.context,
        },
      }
      state.currentConversation = newConversation
      state.conversations.unshift(newConversation)
      state.messages = []
    },

    updateConversation: (state, action: PayloadAction<Partial<Conversation> & { id: string }>) => {
      const { id, ...updates } = action.payload
      
      // Update current conversation if it matches
      if (state.currentConversation?.id === id) {
        state.currentConversation = { ...state.currentConversation, ...updates }
      }
      
      // Update in conversations list
      const index = state.conversations.findIndex(conv => conv.id === id)
      if (index !== -1) {
        state.conversations[index] = { ...state.conversations[index], ...updates }
      }
    },

    // Message management
    addMessage: (state, action: PayloadAction<ChatMessage>) => {
      const message = action.payload
      state.messages.push(message)
      
      // Update conversation message count
      if (state.currentConversation) {
        state.currentConversation.messageCount += 1
        state.currentConversation.updatedAt = new Date()
      }
      
      // Add to recent queries if it's a user message
      if (message.type === 'user' && message.content.trim()) {
        const query = message.content.trim()
        state.recentQueries = [query, ...state.recentQueries.filter(q => q !== query)].slice(0, 20)
      }
    },

    updateMessage: (state, action: PayloadAction<{ id: string; updates: Partial<ChatMessage> }>) => {
      const { id, updates } = action.payload
      const messageIndex = state.messages.findIndex(msg => msg.id === id)
      if (messageIndex !== -1) {
        state.messages[messageIndex] = { ...state.messages[messageIndex], ...updates }
      }
    },

    removeMessage: (state, action: PayloadAction<string>) => {
      const messageId = action.payload
      state.messages = state.messages.filter(msg => msg.id !== messageId)
      
      if (state.currentConversation) {
        state.currentConversation.messageCount = Math.max(0, state.currentConversation.messageCount - 1)
      }
    },

    toggleMessageFavorite: (state, action: PayloadAction<string>) => {
      const messageId = action.payload
      const message = state.messages.find(msg => msg.id === messageId)
      if (message) {
        message.isFavorite = !message.isFavorite
        
        // Update favorite queries list
        if (message.type === 'user' && message.content.trim()) {
          const query = message.content.trim()
          if (message.isFavorite) {
            if (!state.favoriteQueries.includes(query)) {
              state.favoriteQueries.push(query)
            }
          } else {
            state.favoriteQueries = state.favoriteQueries.filter(q => q !== query)
          }
        }
      }
    },

    // Input and suggestions
    setInputValue: (state, action: PayloadAction<string>) => {
      state.inputValue = action.payload
    },

    setSuggestions: (state, action: PayloadAction<QuerySuggestion[]>) => {
      state.suggestions = action.payload
    },

    setShowSuggestions: (state, action: PayloadAction<boolean>) => {
      state.showSuggestions = action.payload
    },

    // Streaming and real-time
    setIsTyping: (state, action: PayloadAction<boolean>) => {
      state.isTyping = action.payload
    },

    setIsLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    },

    setIsConnected: (state, action: PayloadAction<boolean>) => {
      state.isConnected = action.payload
      if (action.payload) {
        state.connectionError = null
      }
    },

    setStreamingProgress: (state, action: PayloadAction<StreamingProgress | null>) => {
      state.streamingProgress = action.payload
    },

    // Settings
    updateSettings: (state, action: PayloadAction<Partial<ChatSettings>>) => {
      state.settings = { ...state.settings, ...action.payload }
    },

    // Error handling
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
    },

    setConnectionError: (state, action: PayloadAction<string | null>) => {
      state.connectionError = action.payload
    },

    // Context management
    updateConversationContext: (state, action: PayloadAction<Partial<ConversationContext>>) => {
      if (state.currentConversation) {
        state.currentConversation.context = {
          ...state.currentConversation.context,
          ...action.payload,
        }
      }
    },

    addToQueryHistory: (state, action: PayloadAction<string>) => {
      const query = action.payload.trim()
      if (query && state.currentConversation) {
        const history = state.currentConversation.context.queryHistory
        state.currentConversation.context.queryHistory = [
          query,
          ...history.filter(q => q !== query)
        ].slice(0, 50) // Keep last 50 queries
      }
    },

    // Bulk operations
    clearMessages: (state) => {
      state.messages = []
      if (state.currentConversation) {
        state.currentConversation.messageCount = 0
      }
    },

    clearConversations: (state) => {
      state.conversations = []
      state.currentConversation = null
      state.messages = []
    },

    // Reset state
    resetChatState: (state) => {
      Object.assign(state, initialState)
    },
  },
})

export const chatActions = chatSlice.actions

// Selectors
export const selectChat = (state: RootState) => state.chat
export const selectCurrentConversation = (state: RootState) => state.chat.currentConversation
export const selectMessages = (state: RootState) => state.chat.messages
export const selectIsTyping = (state: RootState) => state.chat.isTyping
export const selectIsLoading = (state: RootState) => state.chat.isLoading
export const selectIsConnected = (state: RootState) => state.chat.isConnected
export const selectStreamingProgress = (state: RootState) => state.chat.streamingProgress
export const selectInputValue = (state: RootState) => state.chat.inputValue
export const selectSuggestions = (state: RootState) => state.chat.suggestions
export const selectShowSuggestions = (state: RootState) => state.chat.showSuggestions
export const selectConversations = (state: RootState) => state.chat.conversations
export const selectRecentQueries = (state: RootState) => state.chat.recentQueries
export const selectFavoriteQueries = (state: RootState) => state.chat.favoriteQueries
export const selectChatSettings = (state: RootState) => state.chat.settings
export const selectChatError = (state: RootState) => state.chat.error
export const selectConnectionError = (state: RootState) => state.chat.connectionError

// Computed selectors
export const selectLastMessage = (state: RootState) => {
  const messages = state.chat.messages
  return messages.length > 0 ? messages[messages.length - 1] : null
}

export const selectMessagesByType = (state: RootState, type: ChatMessage['type']) => {
  return state.chat.messages.filter(msg => msg.type === type)
}

export const selectFavoriteMessages = (state: RootState) => {
  return state.chat.messages.filter(msg => msg.isFavorite)
}

export const selectConversationContext = (state: RootState) => {
  return state.chat.currentConversation?.context || null
}
