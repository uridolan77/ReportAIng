import React, { useEffect, useState } from 'react'
import {
  Card,
  Row,
  Col,
  Statistic,
  Typography,
  List,
  Tag,
  Progress,
  Alert,
  Button,
  Space,
  Tooltip,
  Badge
} from 'antd'
import {
  UserOutlined,
  QueryDatabaseOutlined,
  ClockCircleOutlined,
  ServerOutlined,
  ReloadOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  StopOutlined
} from '@ant-design/icons'
import {
  useGetRealTimeDashboardQuery,
  useGetActiveQueriesQuery,
  useGetSystemHealthQuery,
  useStartStreamingSessionMutation,
  useStopStreamingSessionMutation
} from '@shared/store/api/featuresApi'
import { socketService } from '@shared/services/socketService'
import { useResponsive } from '@shared/hooks/useResponsive'

const { Title, Text } = Typography

export const RealTimeDashboard: React.FC = () => {
  const responsive = useResponsive()
  const [isSubscribed, setIsSubscribed] = useState(false)
  
  const {
    data: dashboardData,
    isLoading: dashboardLoading,
    refetch: refetchDashboard
  } = useGetRealTimeDashboardQuery()
  
  const {
    data: activeQueries,
    isLoading: queriesLoading
  } = useGetActiveQueriesQuery()
  
  const {
    data: systemHealth,
    isLoading: healthLoading
  } = useGetSystemHealthQuery()
  
  const [startSession] = useStartStreamingSessionMutation()
  const [stopSession] = useStopStreamingSessionMutation()

  // Subscribe to real-time updates
  useEffect(() => {
    if (socketService.isConnected() && !isSubscribed) {
      socketService.subscribeToDashboard()
      socketService.subscribeToSystemHealth()
      setIsSubscribed(true)
    }

    return () => {
      if (isSubscribed) {
        socketService.unsubscribeFromDashboard()
        socketService.unsubscribeFromSystemHealth()
        setIsSubscribed(false)
      }
    }
  }, [isSubscribed])

  const getHealthStatusColor = (status: string) => {
    switch (status) {
      case 'healthy': return '#52c41a'
      case 'warning': return '#faad14'
      case 'critical': return '#ff4d4f'
      default: return '#d9d9d9'
    }
  }

  const getServiceStatusIcon = (status: string) => {
    switch (status) {
      case 'up': return '🟢'
      case 'down': return '🔴'
      case 'degraded': return '🟡'
      default: return '⚪'
    }
  }

  const handleRefreshAll = () => {
    refetchDashboard()
  }

  if (dashboardLoading || queriesLoading || healthLoading) {
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
            Real-time Dashboard
          </Title>
          <Text type="secondary">
            Live system monitoring and query tracking
          </Text>
        </div>
        
        <Space>
          <Badge
            status={isSubscribed ? 'processing' : 'default'}
            text={isSubscribed ? 'Live' : 'Offline'}
          />
          <Button
            icon={<ReloadOutlined />}
            onClick={handleRefreshAll}
          >
            Refresh
          </Button>
        </Space>
      </div>

      {/* System Health Alert */}
      {systemHealth && systemHealth.status !== 'healthy' && (
        <Alert
          type={systemHealth.status === 'warning' ? 'warning' : 'error'}
          message={`System Status: ${systemHealth.status.toUpperCase()}`}
          description={
            systemHealth.alerts.length > 0 
              ? systemHealth.alerts[0].message 
              : 'System is experiencing issues'
          }
          showIcon
          style={{ marginBottom: '24px' }}
        />
      )}

      {/* Key Metrics */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Active Queries"
              value={dashboardData?.activeQueries || 0}
              prefix={<QueryDatabaseOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Connected Users"
              value={dashboardData?.connectedUsers || 0}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Avg Response Time"
              value={dashboardData?.averageResponseTime || 0}
              suffix="ms"
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="System Load"
              value={dashboardData?.systemLoad || 0}
              suffix="%"
              prefix={<ServerOutlined />}
              valueStyle={{ 
                color: (dashboardData?.systemLoad || 0) > 80 ? '#ff4d4f' : '#52c41a' 
              }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        {/* Active Queries */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <PlayCircleOutlined />
                <span>Active Queries</span>
                <Badge count={activeQueries?.length || 0} />
              </Space>
            }
            extra={
              <Button size="small" onClick={handleRefreshAll}>
                Refresh
              </Button>
            }
          >
            {activeQueries && activeQueries.length > 0 ? (
              <List
                size="small"
                dataSource={activeQueries}
                renderItem={(query) => (
                  <List.Item
                    actions={[
                      <Tooltip title="Stop Query">
                        <Button
                          type="text"
                          size="small"
                          icon={<StopOutlined />}
                          onClick={() => stopSession(query.sessionId)}
                          danger
                        />
                      </Tooltip>
                    ]}
                  >
                    <List.Item.Meta
                      title={
                        <Space>
                          <Text strong>{query.user}</Text>
                          <Tag color="blue">
                            {query.progress}% complete
                          </Tag>
                        </Space>
                      }
                      description={
                        <div>
                          <Text ellipsis style={{ maxWidth: '300px' }}>
                            {query.query}
                          </Text>
                          <br />
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            Started: {new Date(query.startTime).toLocaleTimeString()}
                            {query.estimatedCompletion && (
                              <> • ETA: {new Date(query.estimatedCompletion).toLocaleTimeString()}</>
                            )}
                          </Text>
                        </div>
                      }
                    />
                    <Progress
                      percent={query.progress}
                      size="small"
                      style={{ width: '100px' }}
                    />
                  </List.Item>
                )}
              />
            ) : (
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <Text type="secondary">No active queries</Text>
              </div>
            )}
          </Card>
        </Col>

        {/* System Services */}
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <ServerOutlined />
                <span>System Services</span>
                <Tag color={getHealthStatusColor(systemHealth?.status || 'unknown')}>
                  {systemHealth?.status || 'Unknown'}
                </Tag>
              </Space>
            }
          >
            {systemHealth?.services ? (
              <List
                size="small"
                dataSource={systemHealth.services}
                renderItem={(service) => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={getServiceStatusIcon(service.status)}
                      title={
                        <Space>
                          <Text strong>{service.name}</Text>
                          <Tag
                            color={
                              service.status === 'up' ? 'green' :
                              service.status === 'degraded' ? 'orange' : 'red'
                            }
                          >
                            {service.status}
                          </Tag>
                        </Space>
                      }
                      description={
                        <Space>
                          <Text type="secondary">
                            Response: {service.responseTime}ms
                          </Text>
                          <Text type="secondary">
                            Last check: {new Date(service.lastCheck).toLocaleTimeString()}
                          </Text>
                        </Space>
                      }
                    />
                  </List.Item>
                )}
              />
            ) : (
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <Text type="secondary">Service status unavailable</Text>
              </div>
            )}
          </Card>
        </Col>

        {/* Recent Activity */}
        <Col xs={24}>
          <Card
            title={
              <Space>
                <ClockCircleOutlined />
                <span>Recent Activity</span>
              </Space>
            }
          >
            {dashboardData?.recentActivity ? (
              <List
                size="small"
                dataSource={dashboardData.recentActivity.slice(0, 10)}
                renderItem={(activity) => (
                  <List.Item>
                    <List.Item.Meta
                      title={
                        <Space>
                          <Text strong>{activity.user}</Text>
                          <Text>{activity.action}</Text>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {new Date(activity.timestamp).toLocaleString()}
                          </Text>
                        </Space>
                      }
                      description={
                        activity.query && (
                          <Text ellipsis style={{ maxWidth: '500px' }}>
                            {activity.query}
                          </Text>
                        )
                      }
                    />
                  </List.Item>
                )}
              />
            ) : (
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <Text type="secondary">No recent activity</Text>
              </div>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  )
}
