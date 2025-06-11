import React, { useState } from 'react';
import { Layout, Card, Typography, Space, Button, Switch, Divider, Row, Col } from 'antd';
import { 
  MoonOutlined, 
  SunOutlined, 
  StarOutlined,
  RocketOutlined,
  ThunderboltOutlined,
  HeartOutlined
} from '@ant-design/icons';
import { useTheme } from '../contexts/ThemeContext';
import { QueryInterface } from '../components/QueryInterface/QueryInterface';
import { ModernSidebar } from '../components/Layout';

const { Content } = Layout;
const { Title, Text, Paragraph } = Typography;

export const EnhancedUIDemo: React.FC = () => {
  const { isDarkMode, toggleTheme } = useTheme();
  const [loading, setLoading] = useState(false);

  const handleQuerySubmit = (query: string) => {
    console.log('Query submitted:', query);
    setLoading(true);
    // Simulate processing
    setTimeout(() => {
      setLoading(false);
    }, 3000);
  };

  const demoCards = [
    {
      title: "Enhanced Query Input",
      description: "Modern input with passepartout effect and sophisticated styling",
      icon: <StarOutlined />,
      color: "#3b82f6"
    },
    {
      title: "Refined Navigation",
      description: "Sidebar with smooth animations and better spacing",
      icon: <RocketOutlined />,
      color: "#10b981"
    },
    {
      title: "Dark Mode Support",
      description: "Sophisticated dark theme with proper contrast ratios",
      icon: <MoonOutlined />,
      color: "#8b5cf6"
    },
    {
      title: "Micro-interactions",
      description: "Subtle animations and hover effects for better UX",
      icon: <ThunderboltOutlined />,
      color: "#f59e0b"
    }
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <ModernSidebar />
      
      <Content style={{ 
        background: isDarkMode 
          ? 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)'
          : 'linear-gradient(135deg, #f7fafc 0%, #edf2f7 100%)',
        minHeight: '100vh'
      }}>
        {/* Header Section */}
        <div style={{ 
          padding: '40px 40px 20px',
          textAlign: 'center',
          background: isDarkMode ? 'rgba(30, 41, 59, 0.5)' : 'rgba(255, 255, 255, 0.5)',
          backdropFilter: 'blur(10px)',
          borderBottom: `1px solid ${isDarkMode ? '#334155' : '#e2e8f0'}`
        }}>
          <Space direction="vertical" size="large" style={{ width: '100%' }}>
            <div>
              <Title 
                level={1} 
                style={{ 
                  margin: 0,
                  background: 'linear-gradient(135deg, #3b82f6 0%, #8b5cf6 100%)',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent',
                  backgroundClip: 'text',
                  fontSize: '3rem',
                  fontWeight: 800
                }}
              >
                Enhanced UI/UX Demo
              </Title>
              <Paragraph style={{ 
                fontSize: '1.25rem',
                color: isDarkMode ? '#94a3b8' : '#4a5568',
                margin: '16px 0 0 0'
              }}>
                Experience the modern, high-end interface design
              </Paragraph>
            </div>
            
            <Space>
              <Text style={{ color: isDarkMode ? '#e2e8f0' : '#1a202c' }}>
                {isDarkMode ? 'Dark Mode' : 'Light Mode'}
              </Text>
              <Switch
                checked={isDarkMode}
                onChange={toggleTheme}
                checkedChildren={<MoonOutlined />}
                unCheckedChildren={<SunOutlined />}
                style={{ 
                  background: isDarkMode ? '#3b82f6' : '#e2e8f0'
                }}
              />
            </Space>
          </Space>
        </div>

        {/* Demo Features Grid */}
        <div style={{ padding: '40px' }}>
          <Row gutter={[24, 24]} style={{ marginBottom: '48px' }}>
            {demoCards.map((card, index) => (
              <Col xs={24} sm={12} lg={6} key={index}>
                <Card
                  className="enhanced-card-modern fade-in-up"
                  style={{ 
                    height: '100%',
                    animationDelay: `${index * 0.1}s`,
                    border: `1px solid ${isDarkMode ? '#334155' : '#e2e8f0'}`
                  }}
                  bodyStyle={{ 
                    padding: '24px',
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column'
                  }}
                >
                  <Space direction="vertical" size="middle" style={{ width: '100%', flex: 1 }}>
                    <div style={{
                      width: '48px',
                      height: '48px',
                      borderRadius: '12px',
                      background: `linear-gradient(135deg, ${card.color}, ${card.color}dd)`,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      color: 'white',
                      fontSize: '20px'
                    }}>
                      {card.icon}
                    </div>
                    <div>
                      <Title level={4} style={{ 
                        margin: '0 0 8px 0',
                        color: isDarkMode ? '#e2e8f0' : '#1a202c'
                      }}>
                        {card.title}
                      </Title>
                      <Text style={{ 
                        color: isDarkMode ? '#94a3b8' : '#4a5568',
                        lineHeight: '1.5'
                      }}>
                        {card.description}
                      </Text>
                    </div>
                  </Space>
                </Card>
              </Col>
            ))}
          </Row>

          <Divider style={{ 
            borderColor: isDarkMode ? '#334155' : '#e2e8f0',
            margin: '48px 0'
          }} />

          {/* Enhanced Query Interface Demo */}
          <div style={{ marginBottom: '48px' }}>
            <Title level={2} style={{ 
              textAlign: 'center',
              marginBottom: '32px',
              color: isDarkMode ? '#e2e8f0' : '#1a202c'
            }}>
              Enhanced Query Interface
            </Title>
            
            <div style={{
              background: isDarkMode ? 'rgba(30, 41, 59, 0.5)' : 'rgba(255, 255, 255, 0.8)',
              padding: '24px',
              borderRadius: '16px',
              border: `1px solid ${isDarkMode ? '#334155' : '#e2e8f0'}`
            }}>
              <QueryInterface />
            </div>
          </div>

          {/* Design System Showcase */}
          <Card 
            className="enhanced-card-modern"
            title={
              <Title level={3} style={{ 
                margin: 0,
                color: isDarkMode ? '#e2e8f0' : '#1a202c'
              }}>
                Design System Elements
              </Title>
            }
            style={{ 
              border: `1px solid ${isDarkMode ? '#334155' : '#e2e8f0'}`
            }}
          >
            <Row gutter={[24, 24]}>
              <Col xs={24} md={12}>
                <Space direction="vertical" size="large" style={{ width: '100%' }}>
                  <div>
                    <Text strong style={{ 
                      color: isDarkMode ? '#e2e8f0' : '#1a202c',
                      display: 'block',
                      marginBottom: '12px'
                    }}>
                      Enhanced Buttons
                    </Text>
                    <Space wrap>
                      <Button type="primary" className="enhanced-submit-button">
                        Primary Action
                      </Button>
                      <Button className="enhanced-card-modern" style={{ 
                        borderRadius: '12px',
                        height: '40px',
                        fontWeight: 500
                      }}>
                        Secondary
                      </Button>
                    </Space>
                  </div>
                  
                  <div>
                    <Text strong style={{ 
                      color: isDarkMode ? '#e2e8f0' : '#1a202c',
                      display: 'block',
                      marginBottom: '12px'
                    }}>
                      Loading States
                    </Text>
                    <div className="skeleton-loader skeleton-text" style={{ width: '100%' }} />
                    <div className="skeleton-loader skeleton-text" style={{ width: '80%' }} />
                    <div className="skeleton-loader skeleton-text" style={{ width: '60%' }} />
                  </div>
                </Space>
              </Col>
              
              <Col xs={24} md={12}>
                <Space direction="vertical" size="large" style={{ width: '100%' }}>
                  <div>
                    <Text strong style={{ 
                      color: isDarkMode ? '#e2e8f0' : '#1a202c',
                      display: 'block',
                      marginBottom: '12px'
                    }}>
                      Typography Scale
                    </Text>
                    <div>
                      <Title level={1} style={{ 
                        margin: '8px 0',
                        fontSize: '2.5rem',
                        color: isDarkMode ? '#e2e8f0' : '#1a202c'
                      }}>
                        Heading 1
                      </Title>
                      <Title level={2} style={{ 
                        margin: '8px 0',
                        color: isDarkMode ? '#e2e8f0' : '#1a202c'
                      }}>
                        Heading 2
                      </Title>
                      <Text style={{ 
                        fontSize: '16px',
                        color: isDarkMode ? '#94a3b8' : '#4a5568'
                      }}>
                        Body text with proper contrast and readability
                      </Text>
                    </div>
                  </div>
                </Space>
              </Col>
            </Row>
          </Card>

          {/* Footer */}
          <div style={{ 
            textAlign: 'center',
            marginTop: '48px',
            padding: '24px',
            borderTop: `1px solid ${isDarkMode ? '#334155' : '#e2e8f0'}`
          }}>
            <Space>
              <HeartOutlined style={{ color: '#ef4444' }} />
              <Text style={{ color: isDarkMode ? '#94a3b8' : '#4a5568' }}>
                Crafted with attention to detail for the best user experience
              </Text>
            </Space>
          </div>
        </div>
      </Content>
    </Layout>
  );
};

export default EnhancedUIDemo;
