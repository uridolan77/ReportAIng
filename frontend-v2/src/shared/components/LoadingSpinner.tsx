import React from 'react'
import { Spin, Typography } from 'antd'
import { LoadingOutlined } from '@ant-design/icons'

const { Text } = Typography

interface LoadingSpinnerProps {
  size?: 'small' | 'default' | 'large'
  message?: string
  fullScreen?: boolean
  className?: string
}

const customIcon = <LoadingOutlined style={{ fontSize: 24 }} spin />

export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 'default',
  message = 'Loading...',
  fullScreen = false,
  className = '',
}) => {
  const content = (
    <div className={`flex flex-col items-center justify-center gap-md ${className}`}>
      <Spin 
        size={size} 
        indicator={customIcon}
      />
      {message && (
        <Text type="secondary" className="text-center">
          {message}
        </Text>
      )}
    </div>
  )

  if (fullScreen) {
    return (
      <div className="fixed inset-0 flex items-center justify-center bg-primary bg-opacity-50 z-50">
        <div className="bg-primary rounded-lg p-xl shadow-lg">
          {content}
        </div>
      </div>
    )
  }

  return content
}

// Specialized loading components
export const PageLoading: React.FC<{ message?: string }> = ({ 
  message = 'Loading page...' 
}) => (
  <div className="flex items-center justify-center min-h-screen">
    <LoadingSpinner size="large" message={message} />
  </div>
)

export const ComponentLoading: React.FC<{ message?: string }> = ({ 
  message = 'Loading...' 
}) => (
  <div className="flex items-center justify-center p-xl">
    <LoadingSpinner message={message} />
  </div>
)

export const InlineLoading: React.FC<{ message?: string }> = ({ 
  message 
}) => (
  <div className="flex items-center gap-sm">
    <Spin size="small" />
    {message && <Text type="secondary">{message}</Text>}
  </div>
)
