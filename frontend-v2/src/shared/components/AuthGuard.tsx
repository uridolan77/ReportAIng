import React from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { Result, Button } from 'antd'
import { LockOutlined, UserOutlined } from '@ant-design/icons'
import { useAppSelector } from '../hooks'
import { selectIsAuthenticated, selectIsAdmin } from '../store/auth'
import { LoadingSpinner } from './LoadingSpinner'

interface AuthGuardProps {
  children: React.ReactNode
  requireAdmin?: boolean
  fallbackPath?: string
}

export const AuthGuard: React.FC<AuthGuardProps> = ({
  children,
  requireAdmin = false,
  fallbackPath = '/login',
}) => {
  const location = useLocation()
  const isAuthenticated = useAppSelector(selectIsAuthenticated)
  const isAdmin = useAppSelector(selectIsAdmin)

  // Show loading while authentication state is being determined
  if (isAuthenticated === undefined) {
    return <LoadingSpinner fullScreen message="Checking authentication..." />
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return (
      <Navigate
        to={fallbackPath}
        state={{ from: location }}
        replace
      />
    )
  }

  // Show access denied if admin access is required but user is not admin
  if (requireAdmin && !isAdmin) {
    return (
      <div className="flex items-center justify-center min-h-screen p-lg">
        <Result
          status="403"
          title="Access Denied"
          subTitle="You don't have permission to access this page. Administrator privileges are required."
          icon={<LockOutlined />}
          extra={
            <Button
              type="primary"
              icon={<UserOutlined />}
              onClick={() => window.history.back()}
            >
              Go Back
            </Button>
          }
        />
      </div>
    )
  }

  return <>{children}</>
}

// Higher-order component version
export const withAuthGuard = <P extends object>(
  Component: React.ComponentType<P>,
  options?: { requireAdmin?: boolean; fallbackPath?: string }
) => {
  const AuthGuardedComponent = (props: P) => (
    <AuthGuard {...options}>
      <Component {...props} />
    </AuthGuard>
  )

  AuthGuardedComponent.displayName = `withAuthGuard(${Component.displayName || Component.name})`
  
  return AuthGuardedComponent
}
