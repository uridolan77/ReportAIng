import React, { useState } from 'react'
import {
  Card,
  Form,
  Input,
  Button,
  Space,
  Typography,
  Avatar,
  Upload,
  Switch,
  Select,
  Divider,
  Row,
  Col,
  Modal,
  Alert,
  Tabs,
  List,
  Tag,
  Tooltip,
  Progress,
  Statistic
} from 'antd'
import {
  UserOutlined,
  EditOutlined,
  CameraOutlined,
  LockOutlined,
  SafetyOutlined,
  SettingOutlined,
  DeleteOutlined,
  DownloadOutlined,
  MailOutlined,
  MobileOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons'
import {
  useGetCurrentUserQuery,
  useUpdateUserProfileMutation,
  useGetUserPreferencesQuery,
  useUpdateUserPreferencesMutation,
  useGetUserActivityQuery,
  useUploadAvatarMutation,
  useDeleteAvatarMutation,
  useChangePasswordMutation,
  useGetActiveSessionsQuery,
  useRevokeSessionMutation,
  useSetupMfaMutation,
  useDisableMfaMutation,
  useDeleteAccountMutation,
  useExportUserDataMutation,
  useSendVerificationEmailMutation
} from '@shared/store/api/authApi'
import { useResponsive } from '@shared/hooks/useResponsive'

const { Title, Text } = Typography
const { TabPane } = Tabs
const { Option } = Select
const { confirm } = Modal

export const UserProfile: React.FC = () => {
  const responsive = useResponsive()
  const [activeTab, setActiveTab] = useState('profile')
  const [showPasswordModal, setShowPasswordModal] = useState(false)
  const [showMfaModal, setShowMfaModal] = useState(false)
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  
  const [profileForm] = Form.useForm()
  const [preferencesForm] = Form.useForm()
  const [passwordForm] = Form.useForm()

  const {
    data: user,
    isLoading: userLoading,
    refetch: refetchUser
  } = useGetCurrentUserQuery()

  const {
    data: preferences,
    isLoading: preferencesLoading
  } = useGetUserPreferencesQuery()

  const {
    data: activity,
    isLoading: activityLoading
  } = useGetUserActivityQuery({ days: 30 })

  const {
    data: sessions,
    isLoading: sessionsLoading
  } = useGetActiveSessionsQuery()

  const [updateProfile, { isLoading: updatingProfile }] = useUpdateUserProfileMutation()
  const [updatePreferences, { isLoading: updatingPreferences }] = useUpdateUserPreferencesMutation()
  const [uploadAvatar, { isLoading: uploadingAvatar }] = useUploadAvatarMutation()
  const [deleteAvatar] = useDeleteAvatarMutation()
  const [changePassword] = useChangePasswordMutation()
  const [revokeSession] = useRevokeSessionMutation()
  const [setupMfa] = useSetupMfaMutation()
  const [disableMfa] = useDisableMfaMutation()
  const [deleteAccount] = useDeleteAccountMutation()
  const [exportData] = useExportUserDataMutation()
  const [sendVerificationEmail] = useSendVerificationEmailMutation()

  const handleProfileSave = async (values: any) => {
    try {
      await updateProfile(values).unwrap()
      Modal.success({
        title: 'Profile Updated',
        content: 'Your profile has been updated successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Update Failed',
        content: error.message || 'Failed to update profile'
      })
    }
  }

  const handlePreferencesSave = async (values: any) => {
    try {
      await updatePreferences(values).unwrap()
      Modal.success({
        title: 'Preferences Updated',
        content: 'Your preferences have been saved successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Update Failed',
        content: error.message || 'Failed to update preferences'
      })
    }
  }

  const handleAvatarUpload = async (file: File) => {
    const formData = new FormData()
    formData.append('avatar', file)
    
    try {
      await uploadAvatar(formData).unwrap()
      refetchUser()
      Modal.success({
        title: 'Avatar Updated',
        content: 'Your profile picture has been updated successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Upload Failed',
        content: error.message || 'Failed to upload avatar'
      })
    }
  }

  const handlePasswordChange = async (values: any) => {
    try {
      await changePassword(values).unwrap()
      setShowPasswordModal(false)
      passwordForm.resetFields()
      Modal.success({
        title: 'Password Changed',
        content: 'Your password has been changed successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Change Failed',
        content: error.message || 'Failed to change password'
      })
    }
  }

  const handleRevokeSession = async (sessionId: string) => {
    try {
      await revokeSession(sessionId).unwrap()
      Modal.success({
        title: 'Session Revoked',
        content: 'The session has been revoked successfully.'
      })
    } catch (error: any) {
      Modal.error({
        title: 'Revoke Failed',
        content: error.message || 'Failed to revoke session'
      })
    }
  }

  const handleMfaSetup = async () => {
    try {
      const result = await setupMfa().unwrap()
      Modal.info({
        title: 'MFA Setup',
        content: (
          <div>
            <p>Scan this QR code with your authenticator app:</p>
            <img src={result.qrCode} alt="MFA QR Code" style={{ maxWidth: '200px' }} />
            <p>Backup codes:</p>
            <ul>
              {result.backupCodes.map((code, index) => (
                <li key={index}>{code}</li>
              ))}
            </ul>
          </div>
        ),
        width: 400
      })
    } catch (error: any) {
      Modal.error({
        title: 'Setup Failed',
        content: error.message || 'Failed to setup MFA'
      })
    }
  }

  const handleExportData = async () => {
    try {
      const blob = await exportData().unwrap()
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = 'user-data-export.json'
      link.click()
      URL.revokeObjectURL(url)
    } catch (error: any) {
      Modal.error({
        title: 'Export Failed',
        content: error.message || 'Failed to export user data'
      })
    }
  }

  const handleDeleteAccount = () => {
    confirm({
      title: 'Delete Account',
      icon: <ExclamationCircleOutlined />,
      content: 'Are you sure you want to delete your account? This action cannot be undone.',
      okText: 'Delete Account',
      okType: 'danger',
      onOk() {
        setShowDeleteModal(true)
      },
    })
  }

  if (userLoading || preferencesLoading) {
    return (
      <div style={{ padding: '24px' }}>
        <Card loading style={{ minHeight: '400px' }} />
      </div>
    )
  }

  return (
    <div style={{ padding: responsive.isMobile ? '16px' : '24px' }}>
      {/* Header */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '24px'
      }}>
        <div>
          <Title level={2} style={{ margin: 0 }}>
            User Profile
          </Title>
          <Text type="secondary">
            Manage your account settings and preferences
          </Text>
        </div>
        
        <Space>
          <Button
            icon={<DownloadOutlined />}
            onClick={handleExportData}
          >
            Export Data
          </Button>
        </Space>
      </div>

      <Tabs activeKey={activeTab} onChange={setActiveTab}>
        {/* Profile Tab */}
        <TabPane
          tab={
            <Space>
              <UserOutlined />
              <span>Profile</span>
            </Space>
          }
          key="profile"
        >
          <Row gutter={[24, 24]}>
            <Col xs={24} md={8}>
              <Card title="Profile Picture">
                <div style={{ textAlign: 'center' }}>
                  <Avatar
                    size={120}
                    src={user?.avatar}
                    icon={<UserOutlined />}
                    style={{ marginBottom: '16px' }}
                  />
                  <div>
                    <Upload
                      accept="image/*"
                      showUploadList={false}
                      beforeUpload={(file) => {
                        handleAvatarUpload(file)
                        return false
                      }}
                    >
                      <Button
                        icon={<CameraOutlined />}
                        loading={uploadingAvatar}
                        style={{ marginRight: '8px' }}
                      >
                        Upload
                      </Button>
                    </Upload>
                    {user?.avatar && (
                      <Button
                        icon={<DeleteOutlined />}
                        onClick={() => deleteAvatar()}
                      >
                        Remove
                      </Button>
                    )}
                  </div>
                </div>
              </Card>
            </Col>
            
            <Col xs={24} md={16}>
              <Card title="Personal Information">
                <Form
                  form={profileForm}
                  layout="vertical"
                  initialValues={user}
                  onFinish={handleProfileSave}
                >
                  <Row gutter={16}>
                    <Col span={12}>
                      <Form.Item name="firstName" label="First Name">
                        <Input />
                      </Form.Item>
                    </Col>
                    <Col span={12}>
                      <Form.Item name="lastName" label="Last Name">
                        <Input />
                      </Form.Item>
                    </Col>
                  </Row>
                  
                  <Form.Item name="displayName" label="Display Name">
                    <Input />
                  </Form.Item>
                  
                  <Form.Item name="email" label="Email">
                    <Input
                      suffix={
                        user?.emailVerified ? (
                          <Tag color="green">Verified</Tag>
                        ) : (
                          <Button
                            type="link"
                            size="small"
                            onClick={() => sendVerificationEmail()}
                          >
                            Verify
                          </Button>
                        )
                      }
                    />
                  </Form.Item>
                  
                  <Form.Item>
                    <Space>
                      <Button
                        type="primary"
                        htmlType="submit"
                        loading={updatingProfile}
                      >
                        Save Changes
                      </Button>
                      <Button
                        icon={<LockOutlined />}
                        onClick={() => setShowPasswordModal(true)}
                      >
                        Change Password
                      </Button>
                    </Space>
                  </Form.Item>
                </Form>
              </Card>
            </Col>
          </Row>
        </TabPane>

        {/* Preferences Tab */}
        <TabPane
          tab={
            <Space>
              <SettingOutlined />
              <span>Preferences</span>
            </Space>
          }
          key="preferences"
        >
          <Card>
            <Form
              form={preferencesForm}
              layout="vertical"
              initialValues={preferences}
              onFinish={handlePreferencesSave}
            >
              <Row gutter={[24, 0]}>
                <Col xs={24} md={12}>
                  <Title level={4}>Appearance</Title>
                  
                  <Form.Item name={['theme']} label="Theme">
                    <Select>
                      <Option value="light">Light</Option>
                      <Option value="dark">Dark</Option>
                      <Option value="auto">Auto</Option>
                    </Select>
                  </Form.Item>
                  
                  <Form.Item name={['language']} label="Language">
                    <Select>
                      <Option value="en">English</Option>
                      <Option value="es">Spanish</Option>
                      <Option value="fr">French</Option>
                    </Select>
                  </Form.Item>
                </Col>
                
                <Col xs={24} md={12}>
                  <Title level={4}>Notifications</Title>
                  
                  <Form.Item name={['notifications', 'email']} label="Email Notifications" valuePropName="checked">
                    <Switch />
                  </Form.Item>
                  
                  <Form.Item name={['notifications', 'queryComplete']} label="Query Complete" valuePropName="checked">
                    <Switch />
                  </Form.Item>
                  
                  <Form.Item name={['notifications', 'systemAlerts']} label="System Alerts" valuePropName="checked">
                    <Switch />
                  </Form.Item>
                </Col>
              </Row>
              
              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  loading={updatingPreferences}
                >
                  Save Preferences
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </TabPane>

        {/* Security Tab */}
        <TabPane
          tab={
            <Space>
              <SafetyOutlined />
              <span>Security</span>
            </Space>
          }
          key="security"
        >
          <Row gutter={[24, 24]}>
            <Col xs={24} md={12}>
              <Card title="Multi-Factor Authentication">
                <div style={{ marginBottom: '16px' }}>
                  <Text>
                    {user?.mfaEnabled ? 'MFA is enabled' : 'MFA is disabled'}
                  </Text>
                  <Tag color={user?.mfaEnabled ? 'green' : 'orange'} style={{ marginLeft: '8px' }}>
                    {user?.mfaEnabled ? 'Enabled' : 'Disabled'}
                  </Tag>
                </div>
                
                {user?.mfaEnabled ? (
                  <Button
                    danger
                    onClick={() => setShowMfaModal(true)}
                  >
                    Disable MFA
                  </Button>
                ) : (
                  <Button
                    type="primary"
                    onClick={handleMfaSetup}
                  >
                    Enable MFA
                  </Button>
                )}
              </Card>
            </Col>
            
            <Col xs={24} md={12}>
              <Card title="Active Sessions">
                <List
                  size="small"
                  dataSource={sessions}
                  loading={sessionsLoading}
                  renderItem={(session) => (
                    <List.Item
                      actions={[
                        !session.isCurrent && (
                          <Button
                            type="text"
                            size="small"
                            danger
                            onClick={() => handleRevokeSession(session.id)}
                          >
                            Revoke
                          </Button>
                        )
                      ]}
                    >
                      <List.Item.Meta
                        title={
                          <Space>
                            <Text>{session.deviceInfo}</Text>
                            {session.isCurrent && <Tag color="blue">Current</Tag>}
                          </Space>
                        }
                        description={
                          <div>
                            <Text type="secondary">{session.location}</Text>
                            <br />
                            <Text type="secondary" style={{ fontSize: '12px' }}>
                              Last active: {new Date(session.lastActivity).toLocaleString()}
                            </Text>
                          </div>
                        }
                      />
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          </Row>
        </TabPane>

        {/* Activity Tab */}
        <TabPane
          tab={
            <Space>
              <EditOutlined />
              <span>Activity</span>
            </Space>
          }
          key="activity"
        >
          {activity && (
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={8}>
                <Card>
                  <Statistic
                    title="Total Queries"
                    value={activity.totalQueries}
                    valueStyle={{ color: '#3f8600' }}
                  />
                </Card>
              </Col>
              
              <Col xs={24} sm={8}>
                <Card>
                  <Statistic
                    title="Success Rate"
                    value={((activity.successfulQueries / activity.totalQueries) * 100).toFixed(1)}
                    suffix="%"
                    valueStyle={{ color: '#cf1322' }}
                  />
                </Card>
              </Col>
              
              <Col xs={24} sm={8}>
                <Card>
                  <Statistic
                    title="Avg Execution Time"
                    value={activity.avgExecutionTime}
                    suffix="ms"
                    valueStyle={{ color: '#1890ff' }}
                  />
                </Card>
              </Col>
            </Row>
          )}
        </TabPane>

        {/* Danger Zone Tab */}
        <TabPane
          tab={
            <Space>
              <DeleteOutlined />
              <span>Danger Zone</span>
            </Space>
          }
          key="danger"
        >
          <Card title="Danger Zone">
            <Alert
              type="warning"
              message="Irreversible Actions"
              description="The actions in this section cannot be undone. Please proceed with caution."
              style={{ marginBottom: '24px' }}
              showIcon
            />
            
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Title level={5}>Delete Account</Title>
                <Text type="secondary">
                  Permanently delete your account and all associated data.
                </Text>
                <div style={{ marginTop: '8px' }}>
                  <Button
                    danger
                    onClick={handleDeleteAccount}
                  >
                    Delete Account
                  </Button>
                </div>
              </div>
            </Space>
          </Card>
        </TabPane>
      </Tabs>

      {/* Password Change Modal */}
      <Modal
        title="Change Password"
        open={showPasswordModal}
        onCancel={() => setShowPasswordModal(false)}
        footer={null}
      >
        <Form
          form={passwordForm}
          layout="vertical"
          onFinish={handlePasswordChange}
        >
          <Form.Item
            name="currentPassword"
            label="Current Password"
            rules={[{ required: true, message: 'Please enter your current password' }]}
          >
            <Input.Password />
          </Form.Item>
          
          <Form.Item
            name="newPassword"
            label="New Password"
            rules={[
              { required: true, message: 'Please enter a new password' },
              { min: 8, message: 'Password must be at least 8 characters' }
            ]}
          >
            <Input.Password />
          </Form.Item>
          
          <Form.Item
            name="confirmPassword"
            label="Confirm New Password"
            dependencies={['newPassword']}
            rules={[
              { required: true, message: 'Please confirm your new password' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('newPassword') === value) {
                    return Promise.resolve()
                  }
                  return Promise.reject(new Error('Passwords do not match'))
                },
              }),
            ]}
          >
            <Input.Password />
          </Form.Item>
          
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit">
                Change Password
              </Button>
              <Button onClick={() => setShowPasswordModal(false)}>
                Cancel
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
