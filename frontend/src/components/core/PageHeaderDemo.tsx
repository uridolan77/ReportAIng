/**
 * Enhanced PageHeader Component Demo
 * 
 * Demonstrates all the new features and capabilities of the enhanced PageHeader component
 */

import React, { useState } from 'react';
import { Card, Space, Switch, Select, Button, Typography, Row, Col } from 'antd';
import { 
  PageHeader, 
  PageHeaderPresets, 
  createHeaderActions, 
  createEnhancedBreadcrumbs,
  usePageHeaderResponsive,
  usePageHeaderTheme
} from './PageHeader';

const { Title, Text } = Typography;
const { Option } = Select;

export const PageHeaderDemo: React.FC = () => {
  const [currentVariant, setCurrentVariant] = useState<'default' | 'gradient' | 'minimal' | 'elevated' | 'glassmorphism'>('default');
  const [currentSize, setCurrentSize] = useState<'small' | 'medium' | 'large'>('medium');
  const [showDivider, setShowDivider] = useState(false);
  const [animated, setAnimated] = useState(false);
  const [darkMode, setDarkMode] = useState(false);
  const [loading, setLoading] = useState(false);  const { isMobile } = usePageHeaderResponsive();
  const themeProps = usePageHeaderTheme(darkMode);

  // Example actions using the utility functions
  const demoActions = [
    createHeaderActions.refresh(() => {
      setLoading(true);
      setTimeout(() => setLoading(false), 2000);
    }, loading),
    createHeaderActions.export(() => alert('Export clicked!')),
    createHeaderActions.settings(() => alert('Settings clicked!')),
    createHeaderActions.add(() => alert('Add new clicked!'), 'Add Report'),
  ];

  // Example breadcrumbs
  const demoBreadcrumbs = createEnhancedBreadcrumbs.trail(
    createEnhancedBreadcrumbs.dashboard(),
    createEnhancedBreadcrumbs.create('Demo Page', '/demo', '🎨')
  );

  return (
    <div style={{ padding: '24px', backgroundColor: darkMode ? '#1a1a1a' : '#f5f5f5', minHeight: '100vh' }}>
      <Card 
        title="Enhanced PageHeader Demo" 
        style={{ marginBottom: '24px' }}
        extra={
          <Space>
            <Text>Dark Mode:</Text>
            <Switch checked={darkMode} onChange={setDarkMode} />
          </Space>
        }
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} md={8}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <Text strong>Variant:</Text>
                <Select 
                  value={currentVariant} 
                  onChange={setCurrentVariant}
                  style={{ width: '100%', marginTop: 4 }}
                >
                  <Option value="default">Default</Option>
                  <Option value="gradient">Gradient</Option>
                  <Option value="minimal">Minimal</Option>
                  <Option value="elevated">Elevated</Option>
                  <Option value="glassmorphism">Glassmorphism</Option>
                </Select>
              </div>
              
              <div>
                <Text strong>Size:</Text>
                <Select 
                  value={currentSize} 
                  onChange={setCurrentSize}
                  style={{ width: '100%', marginTop: 4 }}
                >
                  <Option value="small">Small</Option>
                  <Option value="medium">Medium</Option>
                  <Option value="large">Large</Option>
                </Select>
              </div>
            </Space>
          </Col>
          
          <Col xs={24} md={8}>
            <Space direction="vertical">
              <div>
                <Switch checked={showDivider} onChange={setShowDivider} />
                <Text style={{ marginLeft: 8 }}>Show Divider</Text>
              </div>
              
              <div>
                <Switch checked={animated} onChange={setAnimated} />
                <Text style={{ marginLeft: 8 }}>Animated</Text>
              </div>
              
              <div>
                <Text>Mobile: {isMobile ? 'Yes' : 'No'}</Text>
              </div>
            </Space>
          </Col>
          
          <Col xs={24} md={8}>
            <Space direction="vertical">
              <Button 
                onClick={() => setCurrentVariant('gradient')}
                type={currentVariant === 'gradient' ? 'primary' : 'default'}
              >
                Dashboard Style
              </Button>
              
              <Button 
                onClick={() => setCurrentVariant('glassmorphism')}
                type={currentVariant === 'glassmorphism' ? 'primary' : 'default'}
              >
                Modern Glass
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>      {/* Demo Page Header */}
      <PageHeader
        title="Enhanced Dashboard"
        subtitle="Experience the new PageHeader component with advanced features and styling options"
        breadcrumbItems={demoBreadcrumbs}
        actions={demoActions}
        variant={currentVariant}
        size={currentSize}
        showDivider={showDivider}
        animated={animated}
        shadow={currentVariant === 'elevated' ? 'large' : currentVariant === 'gradient'}
        backgroundGradient={currentVariant === 'gradient'}
        blur={currentVariant === 'glassmorphism'}        {...(darkMode && themeProps.background && { background: themeProps.background })}
        {...(isMobile && { 
          mobileCollapse: true,
          mobileStackActions: true,
          direction: 'vertical' as const
        })}
      />

      {/* Feature Examples */}
      <Row gutter={[24, 24]} style={{ marginTop: '24px' }}>
        <Col xs={24} lg={8}>
          <Card title="Dashboard Style" size="small">
            <PageHeader
              {...PageHeaderPresets.dashboard}
              title="Analytics Dashboard"
              subtitle="Real-time insights and metrics"
              breadcrumbItems={[
                createEnhancedBreadcrumbs.home(),
                createEnhancedBreadcrumbs.dashboard()
              ]}
              actions={[
                createHeaderActions.refresh(() => {}),
                createHeaderActions.export(() => {})
              ]}
            />
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card title="Admin Style" size="small">
            <PageHeader
              {...PageHeaderPresets.admin}
              title="User Management"
              subtitle="Manage users and permissions"
              breadcrumbItems={createEnhancedBreadcrumbs.adminTrail(
                createEnhancedBreadcrumbs.create('Users', '/admin/users', '👥')
              )}
              actions={[
                createHeaderActions.add(() => {}, 'Add User'),
                createHeaderActions.settings(() => {})
              ]}
            />
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card title="Minimal Style" size="small">
            <PageHeader
              {...PageHeaderPresets.minimal}
              title="Simple Page"
              subtitle="Clean and minimal design"
              breadcrumbItems={[
                createEnhancedBreadcrumbs.home(),
                createEnhancedBreadcrumbs.create('Simple', '/simple')
              ]}
            />
          </Card>
        </Col>
      </Row>

      {/* Glassmorphism Example */}
      <Card title="Glassmorphism Style" style={{ marginTop: '24px' }}>
        <div style={{ 
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          padding: '24px',
          borderRadius: '12px'
        }}>
          <PageHeader
            {...PageHeaderPresets.glassmorphism}
            title="Modern Interface"
            subtitle="Beautiful glassmorphism design with backdrop blur effects"
            breadcrumbItems={[
              createEnhancedBreadcrumbs.home(),
              createEnhancedBreadcrumbs.create('Modern', '/modern', '✨')
            ]}
            actions={[
              createHeaderActions.save(() => {}),
              createHeaderActions.help(() => {})
            ]}
          />
        </div>
      </Card>

      {/* Mobile Example */}
      <Card title="Mobile Responsive" style={{ marginTop: '24px' }}>
        <div style={{ maxWidth: '375px', margin: '0 auto', border: '2px solid #ddd', borderRadius: '12px', padding: '16px' }}>
          <PageHeader
            {...PageHeaderPresets.mobile}
            title="Mobile View"
            subtitle="Optimized for mobile devices"
            breadcrumbItems={[
              createEnhancedBreadcrumbs.home(),
              createEnhancedBreadcrumbs.create('Mobile')
            ]}
            actions={[
              createHeaderActions.refresh(() => {}),
              createHeaderActions.add(() => {})
            ]}
          />
        </div>
      </Card>

      {/* Feature Highlights */}
      <Card title="Key Features" style={{ marginTop: '24px' }}>
        <Row gutter={[16, 16]}>
          <Col xs={24} md={6}>
            <div style={{ textAlign: 'center', padding: '16px' }}>
              <Title level={4}>🎨 Multiple Variants</Title>
              <Text>Default, Gradient, Minimal, Elevated, Glassmorphism</Text>
            </div>
          </Col>
          <Col xs={24} md={6}>
            <div style={{ textAlign: 'center', padding: '16px' }}>
              <Title level={4}>📱 Responsive Design</Title>
              <Text>Mobile-first approach with adaptive layouts</Text>
            </div>
          </Col>
          <Col xs={24} md={6}>
            <div style={{ textAlign: 'center', padding: '16px' }}>
              <Title level={4}>⚡ Enhanced Actions</Title>
              <Text>Pre-built action patterns and tooltips</Text>
            </div>
          </Col>
          <Col xs={24} md={6}>
            <div style={{ textAlign: 'center', padding: '16px' }}>
              <Title level={4}>🧭 Smart Breadcrumbs</Title>
              <Text>Utility functions for common patterns</Text>
            </div>
          </Col>
        </Row>
      </Card>
    </div>
  );
};

export default PageHeaderDemo;
