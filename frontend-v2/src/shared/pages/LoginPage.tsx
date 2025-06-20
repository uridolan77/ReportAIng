import React, { useState, useEffect } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { Form, Input, Button, Card, Typography, Alert, Space, Divider } from 'antd'
import { UserOutlined, LockOutlined, SafetyOutlined } from '@ant-design/icons'
import { useAppSelector, useAppDispatch } from '../hooks'
import { selectIsAuthenticated, selectRequiresMfa, selectMfaChallenge, authActions } from '../store/auth'
import { useLoginMutation, useLoginWithMfaMutation } from '../store/api/authApi'

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
        // Regular login
        setCredentials({ username: values.username, password: values.password })
        
        const result = await login({
          username: values.username,
          password: values.password,
        }).unwrap()
        
        if (result.success && result.user) {
          dispatch(authActions.loginSuccess({
            user: result.user,
            accessToken: result.accessToken,
            refreshToken: result.refreshToken,
          }))
        } else if (result.requiresMfa && result.mfaChallenge) {
          dispatch(authActions.loginMfaRequired({
            challengeId: result.mfaChallenge.challengeId,
          }))
        }
      }
    } catch (error: any) {
      dispatch(authActions.setError(error.data?.message || 'Login failed'))
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
                  placeholder="Enter your username"
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
                  placeholder="Enter your password"
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
