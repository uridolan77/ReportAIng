import React, { useState, useEffect } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { Form, Input, Button, Card, Typography, Alert, Space, Divider, Tag } from 'antd'
import { UserOutlined, LockOutlined, SafetyOutlined, ThunderboltOutlined } from '@ant-design/icons'
import { useAppSelector, useAppDispatch } from '../hooks'
import { selectIsAuthenticated, selectRequiresMfa, selectMfaChallenge, authActions } from '../store/auth'
import { useLoginMutation, useLoginWithMfaMutation } from '../store/api/authApi'
// Enhanced API hooks removed - using standard RTK Query hooks
import { useApiMode } from '../components/core/ApiModeToggle'

const { Title, Text } = Typography

interface LoginFormData {
  username: string
  password: string
  mfaCode?: string
}

export default function LoginPage() {
  const location = useLocation()
  const dispatch = useAppDispatch()
  const isAuthenticated = useAppSelector(selectIsAuthenticated)
  const requiresMfa = useAppSelector(selectRequiresMfa)
  const mfaChallenge = useAppSelector(selectMfaChallenge)
  
  const [form] = Form.useForm()
  const [showMfa, setShowMfa] = useState(false)
  const [credentials, setCredentials] = useState<{ username: string; password: string } | null>(null)
  
  const [login, { isLoading: isLoginLoading, error: loginError }] = useLoginMutation()
  const [loginWithMfa, { isLoading: isMfaLoading, error: mfaError }] = useLoginWithMfaMutation()

  const { useMockData } = useApiMode()

  const from = (location.state as any)?.from?.pathname || '/chat'

  useEffect(() => {
    if (requiresMfa && mfaChallenge) {
      setShowMfa(true)
    }
  }, [requiresMfa, mfaChallenge])

  // Redirect if already authenticated
  if (isAuthenticated) {
    return <Navigate to={from} replace />
  }

  const handleLogin = async (values: LoginFormData) => {
    try {
      dispatch(authActions.setError(null))

      if (showMfa && credentials) {
        // MFA login
        const result = await loginWithMfa({
          ...credentials,
          mfaCode: values.mfaCode!,
          challengeId: mfaChallenge || undefined,
        }).unwrap()

        if (result.success && result.user) {
          dispatch(authActions.loginSuccess({
            user: result.user,
            accessToken: result.accessToken,
            refreshToken: result.refreshToken,
          }))
        }
      } else {
        // Standard login
        const result = await login({
          username: values.username,
          password: values.password
        }).unwrap()

        if (result.success && result.user) {
          dispatch(authActions.loginSuccess({
            user: result.user,
            accessToken: result.accessToken,
            refreshToken: result.refreshToken,
          }))
        } else if (result.requiresMfa) {
          // Handle MFA requirement
          setShowMfa(true)
          setCredentials({ username: values.username, password: values.password })
        }
      }
    } catch (error: any) {
      dispatch(authActions.setError(error.data?.message || 'Login failed'))
    }
  }

  const handleQuickLogin = async () => {
    try {
      dispatch(authActions.setError(null))

      // Quick login with admin credentials for development
      const result = await login({
        username: 'admin',
        password: 'admin'
      }).unwrap()

      if (result.success && result.user) {
        dispatch(authActions.loginSuccess({
          user: result.user,
          accessToken: result.accessToken,
          refreshToken: result.refreshToken,
        }))
      }
    } catch (error: any) {
      dispatch(authActions.setError(error.data?.message || 'Quick login failed'))
    }
  }

  const handleBackToLogin = () => {
    setShowMfa(false)
    setCredentials(null)
    dispatch(authActions.setError(null))
    form.resetFields(['mfaCode'])
  }

  const currentError = loginError || mfaError
  const isLoading = isLoginLoading || isMfaLoading

  return (
    <div className="min-h-screen flex items-center justify-center bg-secondary p-lg">
      <Card className="w-full max-w-md shadow-lg">
        <div className="text-center mb-lg">
          <Title level={2} className="mb-sm">
            BI Reporting Copilot
          </Title>
          <Text type="secondary">
            {showMfa ? 'Enter your authentication code' : 'Sign in to your account'}
          </Text>


        </div>

        {currentError && (
          <Alert
            message="Authentication Error"
            description={
              typeof currentError === 'object' && 'data' in currentError
                ? (currentError.data as any)?.message || 'An error occurred'
                : 'An error occurred'
            }
            type="error"
            showIcon
            className="mb-lg"
          />
        )}

        {/* Mock Mode Information */}
        {useMockData && !showMfa && (
          <Alert
            message="Development Mode Active"
            description={
              <div>
                <p>You're in mock mode. You can:</p>
                <ul style={{ marginBottom: 8, paddingLeft: 20 }}>
                  <li>Use any username (try: admin, analyst, user)</li>
                  <li>Use any password (try: password, admin, demo)</li>
                  <li>Or click Quick Login below</li>
                </ul>
                <Button
                  size="small"
                  type="primary"
                  icon={<ThunderboltOutlined />}
                  onClick={handleQuickLogin}
                  loading={isLoading}
                  style={{ marginTop: 8 }}
                >
                  Quick Login (admin)
                </Button>
              </div>
            }
            type="info"
            showIcon
            className="mb-lg"
          />
        )}

        <Form
          form={form}
          name="login"
          onFinish={handleLogin}
          layout="vertical"
          size="large"
        >
          {!showMfa ? (
            <>
              <Form.Item
                name="username"
                label="Username"
                rules={[
                  { required: true, message: 'Please enter your username' },
                ]}
              >
                <Input
                  prefix={<UserOutlined />}
                  placeholder={useMockData ? "Try: admin, analyst, or any username" : "Enter your username"}
                  autoComplete="username"
                />
              </Form.Item>

              <Form.Item
                name="password"
                label="Password"
                rules={[
                  { required: true, message: 'Please enter your password' },
                ]}
              >
                <Input.Password
                  prefix={<LockOutlined />}
                  placeholder={useMockData ? "Try: password, admin, demo, or any 3+ chars" : "Enter your password"}
                  autoComplete="current-password"
                />
              </Form.Item>
            </>
          ) : (
            <Form.Item
              name="mfaCode"
              label="Authentication Code"
              rules={[
                { required: true, message: 'Please enter your authentication code' },
                { len: 6, message: 'Authentication code must be 6 digits' },
              ]}
            >
              <Input
                prefix={<SafetyOutlined />}
                placeholder="Enter 6-digit code"
                maxLength={6}
                autoComplete="one-time-code"
              />
            </Form.Item>
          )}

          <Form.Item className="mb-sm">
            <Button
              type="primary"
              htmlType="submit"
              loading={isLoading}
              block
            >
              {showMfa ? 'Verify Code' : 'Sign In'}
            </Button>
          </Form.Item>

          {showMfa && (
            <>
              <Divider />
              <Space direction="vertical" className="w-full">
                <Text type="secondary" className="text-center block">
                  Enter the 6-digit code from your authenticator app
                </Text>
                <Button
                  type="link"
                  onClick={handleBackToLogin}
                  block
                >
                  Back to Login
                </Button>
              </Space>
            </>
          )}
        </Form>
      </Card>
    </div>
  )
}
