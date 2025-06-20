import React, { useEffect, useState } from 'react'
import { useAppSelector, useAppDispatch } from '@shared/hooks'
import { useResponsive } from '@shared/hooks/useResponsive'
import {
  selectMessages,
  selectCurrentConversation,
  selectIsLoading,
  selectStreamingProgress,
  chatActions
} from '@shared/store/chat'
import { useSendMessageMutation, useCreateConversationMutation } from '@shared/store/api/chatApi'
import { socketService } from '@shared/services/socketService'

// Components
import { CreativeChatInterface } from '../components/CreativeChatInterface'
import { MobileChatInterface } from '../components/MobileChatInterface'
import { ChatInterface } from '../components/ChatInterface'
import { MessageItem } from '../components/MessageItem'
import { QueryResultsViewer } from '../components/QueryResultsViewer'
import { SemanticInsightsPanel } from '../components/SemanticInsightsPanel'

export const ChatPage: React.FC = () => {
  const dispatch = useAppDispatch()
  const responsive = useResponsive()
  
  const messages = useAppSelector(selectMessages)
  const currentConversation = useAppSelector(selectCurrentConversation)
  const isLoading = useAppSelector(selectIsLoading)
  const streamingProgress = useAppSelector(selectStreamingProgress)
  
  const [sendMessage] = useSendMessageMutation()
  const [createConversation] = useCreateConversationMutation()
  
  const [interfaceMode, setInterfaceMode] = useState<'creative' | 'standard' | 'mobile'>('creative')

  // Initialize conversation if none exists
  useEffect(() => {
    if (!currentConversation) {
      const initializeConversation = async () => {
        try {
          const conversation = await createConversation({
            title: 'New Chat Session'
          }).unwrap()
          
          dispatch(chatActions.setCurrentConversation(conversation))
        } catch (error) {
          console.error('Failed to create conversation:', error)
        }
      }
      
      initializeConversation()
    }
  }, [currentConversation, createConversation, dispatch])

  // Auto-detect interface mode based on device
  useEffect(() => {
    if (responsive.isMobile) {
      setInterfaceMode('mobile')
    } else {
      // Default to creative for desktop/tablet
      setInterfaceMode('creative')
    }
  }, [responsive.isMobile])

  // Initialize socket connection
  useEffect(() => {
    const initializeSocket = async () => {
      try {
        if (!socketService.isConnected()) {
          await socketService.connect()
        }
        
        if (currentConversation?.id) {
          socketService.joinConversation(currentConversation.id)
        }
      } catch (error) {
        console.error('Failed to initialize socket:', error)
      }
    }

    initializeSocket()

    return () => {
      if (currentConversation?.id) {
        socketService.leaveConversation(currentConversation.id)
      }
    }
  }, [currentConversation?.id])

  const handleSendMessage = async (content: string) => {
    if (!content.trim()) return

    try {
      // Create optimistic message
      const tempMessage = {
        id: `temp-${Date.now()}`,
        content,
        type: 'user' as const,
        timestamp: new Date().toISOString(),
        status: 'sending' as const,
        conversationId: currentConversation?.id
      }

      dispatch(chatActions.addMessage(tempMessage))
      dispatch(chatActions.setIsLoading(true))

      // Send message via API
      const result = await sendMessage({
        content,
        conversationId: currentConversation?.id,
        type: 'user'
      }).unwrap()

      // Update with real message
      dispatch(chatActions.updateMessage({
        id: tempMessage.id,
        updates: {
          id: result.message.id,
          status: 'delivered'
        }
      }))

      // Start streaming if enabled and session ID provided
      if (result.sessionId) {
        socketService.startStreamingQuery({
          query: content,
          messageId: result.message.id,
          conversationId: currentConversation?.id
        })
      } else {
        // If no streaming, add assistant response immediately
        if (result.response) {
          dispatch(chatActions.addMessage({
            id: `response-${Date.now()}`,
            content: result.response.content,
            type: 'assistant',
            timestamp: new Date().toISOString(),
            status: 'delivered',
            conversationId: currentConversation?.id,
            sql: result.response.sql,
            results: result.response.results,
            resultMetadata: result.response.metadata,
            semanticAnalysis: result.response.semanticAnalysis
          }))
        }
        
        dispatch(chatActions.setIsLoading(false))
      }

    } catch (error: any) {
      console.error('Failed to send message:', error)
      
      // Update message with error status
      dispatch(chatActions.updateMessage({
        id: `temp-${Date.now()}`,
        updates: {
          status: 'error',
          error: {
            code: 'SEND_FAILED',
            message: error.message || 'Failed to send message',
            retryable: true
          }
        }
      }))
      
      dispatch(chatActions.setIsLoading(false))
    }
  }

  const handleRerunQuery = (query: string) => {
    handleSendMessage(query)
  }

  const handleExportResults = (format: string, data: any[]) => {
    // Implementation for exporting results
    console.log('Exporting results:', { format, data })
  }

  const handleShareMessage = (messageId: string) => {
    // Implementation for sharing messages
    console.log('Sharing message:', messageId)
  }

  // Render appropriate interface based on mode and device
  const renderInterface = () => {
    switch (interfaceMode) {
      case 'mobile':
        return (
          <MobileChatInterface
            messages={messages}
            onSendMessage={handleSendMessage}
            isLoading={isLoading}
            streamingProgress={streamingProgress}
          />
        )
      
      case 'creative':
        return messages.length === 0 ? (
          <CreativeChatInterface
            onSendMessage={handleSendMessage}
            isLoading={isLoading}
          />
        ) : (
          <ChatInterface
            conversationId={currentConversation?.id}
            showHeader={true}
            showExamples={false}
          />
        )
      
      case 'standard':
      default:
        return (
          <ChatInterface
            conversationId={currentConversation?.id}
            showHeader={true}
            showExamples={messages.length === 0}
          />
        )
    }
  }

  // For mobile, use the mobile interface directly
  if (responsive.isMobile) {
    return (
      <MobileChatInterface
        messages={messages}
        onSendMessage={handleSendMessage}
        isLoading={isLoading}
        streamingProgress={streamingProgress}
      />
    )
  }

  // For desktop/tablet, show creative interface when no messages, standard when chatting
  if (messages.length === 0 && !isLoading) {
    return (
      <CreativeChatInterface
        onSendMessage={handleSendMessage}
        isLoading={isLoading}
      />
    )
  }

  // Standard chat interface with messages
  return (
    <div style={{
      height: '100vh',
      display: 'flex',
      flexDirection: 'column',
      background: '#f5f5f5'
    }}>
      {/* Messages Area */}
      <div style={{
        flex: 1,
        overflow: 'auto',
        padding: '20px',
        display: 'flex',
        flexDirection: 'column',
        gap: '16px'
      }}>
        {messages.map((message) => (
          <div key={message.id}>
            <MessageItem
              message={message}
              showMetadata={true}
              onRerun={handleRerunQuery}
              onEdit={(messageId) => console.log('Edit message:', messageId)}
            />
            
            {/* Show results viewer for assistant messages with results */}
            {message.type === 'assistant' && message.results && (
              <QueryResultsViewer
                message={message}
                onExport={handleExportResults}
                onShare={() => handleShareMessage(message.id)}
              />
            )}
            
            {/* Show semantic analysis for assistant messages */}
            {message.type === 'assistant' && message.semanticAnalysis && (
              <SemanticInsightsPanel
                analysis={message.semanticAnalysis}
                onEntityClick={(entity) => console.log('Entity clicked:', entity)}
                onSuggestionClick={handleSendMessage}
                showAdvanced={false}
              />
            )}
          </div>
        ))}
      </div>

      {/* Chat Input */}
      <div style={{
        padding: '20px',
        borderTop: '1px solid #e5e5e5',
        background: 'white'
      }}>
        <ChatInterface
          conversationId={currentConversation?.id}
          showHeader={false}
          showExamples={false}
        />
      </div>
    </div>
  )
}
