import React, { useState, useEffect } from 'react';
import {
  Card,
  Typography,
  Tag,
  List,
  Row,
  Col,
  Collapse,
  Spin,
  Alert,
  Avatar,
  Badge,
  Space,
  Flex,
} from 'antd';
import {
  UserOutlined,
  TableOutlined,
  FilterOutlined,
  AppstoreOutlined,
  RiseOutlined,
  ClockCircleOutlined,
  GlobalOutlined,
  BulbOutlined,
} from '@ant-design/icons';
import { ApiService, UserContextResponse } from '../../services/api';

const UserContextPanel: React.FC = () => {
  const [userContext, setUserContext] = useState<UserContextResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadUserContext();
  }, []);

  const loadUserContext = async () => {
    try {
      setLoading(true);
      const context = await ApiService.getUserContext();
      setUserContext(context);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load user context');
    } finally {
      setLoading(false);
    }
  };

  const getDomainColor = (domain: string) => {
    switch (domain.toLowerCase()) {
      case 'sales': return 'success';
      case 'marketing': return 'blue';
      case 'finance': return 'orange';
      case 'hr': return 'cyan';
      case 'operations': return 'purple';
      default: return 'default';
    }
  };

  const getIntentIcon = (intent: string) => {
    switch (intent.toLowerCase()) {
      case 'aggregation': return <RiseOutlined />;
      case 'trend': return <RiseOutlined />;
      case 'comparison': return <BulbOutlined />;
      case 'filtering': return <FilterOutlined />;
      default: return <AppstoreOutlined />;
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 200 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message={error}
        type="error"
        style={{ marginBottom: 16 }}
        showIcon
      />
    );
  }

  if (!userContext) {
    return (
      <Alert
        message="No user context available. Start asking questions to build your AI profile!"
        type="info"
        style={{ marginBottom: 16 }}
        showIcon
      />
    );
  }

  return (
    <div>
      <Typography.Title level={3}>
        <UserOutlined style={{ marginRight: 8 }} />
        Your AI Profile
      </Typography.Title>
      <Typography.Text type="secondary">
        AI learns from your query patterns to provide better suggestions and insights
      </Typography.Text>

      {/* Domain and Overview */}
      <Card style={{ marginBottom: 24 }}>
        <Row gutter={24}>
          <Col xs={24} md={12}>
            <Flex align="center" style={{ marginBottom: 16 }}>
              <Avatar style={{ backgroundColor: '#1890ff', marginRight: 16 }}>
                <GlobalOutlined />
              </Avatar>
              <div>
                <Typography.Title level={5} style={{ margin: 0 }}>Domain Focus</Typography.Title>
                <Tag
                  color={getDomainColor(userContext.domain)}
                >
                  {userContext.domain || 'General'}
                </Tag>
              </div>
            </Flex>
          </Col>
          <Col xs={24} md={12}>
            <Flex align="center" style={{ marginBottom: 16 }}>
              <Avatar style={{ backgroundColor: '#722ed1', marginRight: 16 }}>
                <ClockCircleOutlined />
              </Avatar>
              <div>
                <Typography.Title level={5} style={{ margin: 0 }}>Last Updated</Typography.Title>
                <Typography.Text type="secondary">
                  {formatDate(userContext.lastUpdated)}
                </Typography.Text>
              </div>
            </Flex>
          </Col>
        </Row>
      </Card>

      {/* Preferred Tables */}
      {userContext.preferredTables.length > 0 && (
        <Collapse style={{ marginBottom: 16 }}>
          <Collapse.Panel
            header={
              <Flex align="center">
                <TableOutlined style={{ marginRight: 8 }} />
                <Typography.Title level={5} style={{ margin: 0 }}>
                  Preferred Tables
                </Typography.Title>
                <Badge
                  count={userContext.preferredTables.length}
                  style={{ marginLeft: 16 }}
                />
              </Flex>
            }
            key="1"
          >
            <Space wrap>
              {userContext.preferredTables.map((table, index) => (
                <Tag
                  key={index}
                  icon={<TableOutlined />}
                  color="blue"
                >
                  {table}
                </Tag>
              ))}
            </Space>
          </Collapse.Panel>
        </Collapse>
      )}

      {/* Common Filters */}
      {userContext.commonFilters.length > 0 && (
        <Collapse style={{ marginBottom: 16 }}>
          <Collapse.Panel
            header={
              <Flex align="center">
                <FilterOutlined style={{ marginRight: 8 }} />
                <Typography.Title level={5} style={{ margin: 0 }}>
                  Common Filters
                </Typography.Title>
                <Badge
                  count={userContext.commonFilters.length}
                  style={{ marginLeft: 16 }}
                />
              </Flex>
            }
            key="1"
          >
            <Space wrap>
              {userContext.commonFilters.map((filter, index) => (
                <Tag
                  key={index}
                  icon={<FilterOutlined />}
                  color="purple"
                >
                  {filter}
                </Tag>
              ))}
            </Space>
          </Collapse.Panel>
        </Collapse>
      )}

      {/* Query Patterns */}
      {userContext.recentPatterns.length > 0 && (
        <Collapse>
          <Collapse.Panel
            header={
              <Flex align="center">
                <AppstoreOutlined style={{ marginRight: 8 }} />
                <Typography.Title level={5} style={{ margin: 0 }}>
                  Query Patterns
                </Typography.Title>
                <Badge
                  count={userContext.recentPatterns.length}
                  style={{ marginLeft: 16 }}
                />
              </Flex>
            }
            key="1"
          >
            <List
              dataSource={userContext.recentPatterns}
              renderItem={(pattern) => (
                <List.Item>
                  <List.Item.Meta
                    avatar={getIntentIcon(pattern.intent)}
                    title={
                      <Flex justify="space-between" align="center">
                        <Typography.Text style={{ flex: 1 }}>
                          {pattern.pattern}
                        </Typography.Text>
                        <Space>
                          <Tag color="blue">
                            {pattern.frequency}x
                          </Tag>
                          <Tag>
                            {pattern.intent}
                          </Tag>
                        </Space>
                      </Flex>
                    }
                    description={
                      <div>
                        <Typography.Text type="secondary" style={{ fontSize: '12px' }}>
                          Last used: {formatDate(pattern.lastUsed)}
                        </Typography.Text>
                        {pattern.associatedTables.length > 0 && (
                          <div style={{ marginTop: 8 }}>
                            <Space wrap>
                              {pattern.associatedTables.map((table, tableIndex) => (
                                <Tag
                                  key={tableIndex}
                                  style={{ fontSize: '11px' }}
                                >
                                  {table}
                                </Tag>
                              ))}
                            </Space>
                          </div>
                        )}
                      </div>
                    }
                  />
                </List.Item>
              )}
            />
          </Collapse.Panel>
        </Collapse>
      )}

      {/* Empty State */}
      {userContext.preferredTables.length === 0 &&
       userContext.commonFilters.length === 0 &&
       userContext.recentPatterns.length === 0 && (
        <Card>
          <div style={{ textAlign: 'center', padding: '32px 0' }}>
            <BulbOutlined style={{ fontSize: 64, color: '#bfbfbf', marginBottom: 16 }} />
            <Typography.Title level={4}>
              Start Building Your AI Profile
            </Typography.Title>
            <Typography.Text type="secondary">
              Ask questions and execute queries to help AI learn your preferences and provide better suggestions.
            </Typography.Text>
          </div>
        </Card>
      )}
    </div>
  );
};

export default UserContextPanel;
