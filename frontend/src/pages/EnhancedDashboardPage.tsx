/**
 * Enhanced Dashboard Page Example
 * 
 * Demonstrates the new PageHeader component with enhanced features
 * in a real-world dashboard context.
 */

import React, { useState } from 'react';
import { Card, Row, Col, Statistic, Button, Space, Typography, Avatar, Progress } from 'antd';
import { 
  UserOutlined, 
  TrophyOutlined, 
  RiseOutlined, 
  EyeOutlined,
  DownloadOutlined,
  SettingOutlined,
  PlusOutlined
} from '@ant-design/icons';
import { 
  PageHeader, 
  PageHeaderPresets, 
  createHeaderActions, 
  createEnhancedBreadcrumbs 
} from '../components/core/PageHeader';

const { Text } = Typography;

export const EnhancedDashboardPage: React.FC = () => {
  const [refreshing, setRefreshing] = useState(false);

  const handleRefresh = () => {
    setRefreshing(true);
    setTimeout(() => setRefreshing(false), 2000);
  };

  const handleExport = () => {
    console.log('Exporting dashboard data...');
  };

  const handleSettings = () => {
    console.log('Opening dashboard settings...');
  };
  const handleAddWidget = () => {
    console.log('Adding new widget...');
  };

  // Enhanced breadcrumbs
  const breadcrumbs = createEnhancedBreadcrumbs.trail(
    createEnhancedBreadcrumbs.dashboard()
  );

  // Enhanced actions with utilities
  const headerActions = [
    createHeaderActions.refresh(handleRefresh, refreshing),
    createHeaderActions.export(handleExport),
    createHeaderActions.settings(handleSettings),
    createHeaderActions.add(handleAddWidget, 'Add Widget'),
  ];

  return (
    <div style={{ backgroundColor: '#f5f7fa', minHeight: '100vh' }}>
      {/* Enhanced PageHeader with gradient style */}
      <PageHeader
        {...PageHeaderPresets.dashboard}
        title="Analytics Dashboard"
        subtitle="Real-time insights and performance metrics for your business"
        breadcrumbItems={breadcrumbs}
        actions={headerActions}
        animated={true}
        animationDelay={200}
      />

      <div style={{ padding: '0 24px 24px' }}>
        {/* Stats Overview */}
        <Row gutter={[24, 24]} style={{ marginBottom: '24px' }}>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Total Users"
                value={11028}
                prefix={<UserOutlined />}
                valueStyle={{ color: '#3f8600' }}
                suffix={<Text type="secondary">+12%</Text>}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Revenue"
                value={9280}
                precision={2}
                prefix="$"
                valueStyle={{ color: '#cf1322' }}
                suffix={<Text type="secondary">+8%</Text>}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Conversion Rate"
                value={23.5}
                suffix="%"
                prefix={<TrophyOutlined />}
                valueStyle={{ color: '#1890ff' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="Page Views"
                value={58420}
                prefix={<EyeOutlined />}
                valueStyle={{ color: '#722ed1' }}
                suffix={<Text type="secondary">+24%</Text>}
              />
            </Card>
          </Col>
        </Row>

        {/* Main Content with various PageHeader styles */}
        <Row gutter={[24, 24]}>
          {/* Reports Section - Elevated Style */}
          <Col xs={24} lg={12}>
            <Card style={{ height: '100%' }}>
              <PageHeader
                variant="elevated"
                size="medium"
                title="Reports"
                subtitle="Generate and manage your reports"
                showDivider={true}
                actions={[
                  {
                    key: 'new-report',
                    label: 'New Report',
                    icon: <PlusOutlined />,
                    onClick: () => console.log('New report'),
                    type: 'primary',
                    tooltip: 'Create a new report'
                  },
                  {
                    key: 'view-all',
                    label: 'View All',
                    icon: <EyeOutlined />,
                    onClick: () => console.log('View all'),
                    tooltip: 'View all reports'
                  }
                ]}
              />
              <div style={{ padding: '16px 0' }}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Monthly Sales Report</Text>
                    <Progress percent={85} size="small" style={{ width: '100px' }} />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>User Analytics</Text>
                    <Progress percent={92} size="small" style={{ width: '100px' }} />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Text>Financial Summary</Text>
                    <Progress percent={67} size="small" style={{ width: '100px' }} />
                  </div>
                </Space>
              </div>
            </Card>
          </Col>

          {/* Team Section - Glassmorphism Style */}
          <Col xs={24} lg={12}>
            <div style={{ 
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              borderRadius: '12px',
              padding: '1px'
            }}>
              <Card style={{ height: '100%', background: 'transparent', border: 'none' }}>
                <PageHeader
                  variant="glassmorphism"
                  size="medium"
                  title="Team Performance"
                  subtitle="Track your team's achievements"
                  blur={true}
                  actions={[
                    {
                      key: 'team-settings',
                      label: 'Settings',
                      icon: <SettingOutlined />,
                      onClick: () => console.log('Team settings'),
                      ghost: true
                    }
                  ]}
                />
                <div style={{ padding: '16px 0' }}>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                      <Avatar icon={<UserOutlined />} />
                      <div>
                        <Text strong style={{ color: 'white' }}>John Doe</Text>
                        <br />
                        <Text style={{ color: 'rgba(255,255,255,0.8)' }}>Lead Developer</Text>
                      </div>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                      <Avatar icon={<UserOutlined />} />
                      <div>
                        <Text strong style={{ color: 'white' }}>Jane Smith</Text>
                        <br />
                        <Text style={{ color: 'rgba(255,255,255,0.8)' }}>UI Designer</Text>
                      </div>
                    </div>
                  </Space>
                </div>
              </Card>
            </div>
          </Col>
        </Row>

        {/* Quick Actions Section - Minimal Style */}
        <Row style={{ marginTop: '24px' }}>
          <Col span={24}>
            <Card>
              <PageHeader
                variant="minimal"
                title="Quick Actions"
                subtitle="Frequently used tools and shortcuts"
                showDivider={true}
                direction="horizontal"
                actions={
                  <Space wrap>
                    <Button icon={<DownloadOutlined />}>Export Data</Button>
                    <Button icon={<SettingOutlined />}>Configure</Button>
                    <Button icon={<RiseOutlined />} type="primary">Generate Report</Button>
                  </Space>
                }
              />
              <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
                <Col xs={24} sm={8}>
                  <Card size="small" hoverable>
                    <Statistic title="Quick Export" value="CSV" prefix="📊" />
                  </Card>
                </Col>
                <Col xs={24} sm={8}>
                  <Card size="small" hoverable>
                    <Statistic title="Backup" value="Auto" prefix="💾" />
                  </Card>
                </Col>
                <Col xs={24} sm={8}>
                  <Card size="small" hoverable>
                    <Statistic title="Sync Status" value="Live" prefix="🔄" />
                  </Card>
                </Col>
              </Row>
            </Card>
          </Col>
        </Row>
      </div>
    </div>
  );
};

export default EnhancedDashboardPage;
