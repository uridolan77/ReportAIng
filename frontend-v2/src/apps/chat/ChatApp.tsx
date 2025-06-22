import React from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { AppLayout } from '@shared/components/core/Layout'
import ChatInterface from './pages/ChatInterface'
import EnhancedChatInterface from './components/EnhancedChatInterface'
import QueryHistory from './pages/QueryHistory'
import QueryResults from './pages/QueryResults'

export default function ChatApp() {
  return (
    <AppLayout>
      <Routes>
        {/* Main chat interface */}
        <Route path="/" element={<ChatInterface />} />

        {/* Enhanced chat interface with transparency */}
        <Route path="/enhanced" element={<EnhancedChatInterface />} />
        <Route path="/enhanced/:conversationId" element={<EnhancedChatInterface />} />

        {/* Query history */}
        <Route path="/history" element={<QueryHistory />} />

        {/* Query results */}
        <Route path="/results/:queryId?" element={<QueryResults />} />

        {/* Default redirect */}
        <Route path="*" element={<Navigate to="/chat" replace />} />
      </Routes>
    </AppLayout>
  )
}
