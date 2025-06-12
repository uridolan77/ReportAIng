import React, { useState } from 'react';
import {
  Card,
  Tabs,
  Alert,
  Typography,
  Row,
  Col,
  Statistic,
  Space
} from 'antd';
import {
  BookOutlined,
  TableOutlined,
  SearchOutlined,
  DatabaseOutlined,
  BranchesOutlined,
  FileTextOutlined
} from '@ant-design/icons';
import { BusinessGlossaryManager } from './BusinessGlossaryManager';
import { BusinessTableManager } from './BusinessTableManager';
import { QueryPatternManager } from './QueryPatternManager';

const { Title } = Typography;
const { TabPane } = Tabs;

interface KnowledgeBaseManagerProps {
  onDataChange?: () => void;
}

export const KnowledgeBaseManager: React.FC<KnowledgeBaseManagerProps> = ({ onDataChange }) => {
  const [activeTab, setActiveTab] = useState('glossary');

  const handleDataChange = () => {
    onDataChange?.();
  };

  return (
    <div>
      <Alert
        message="Business Knowledge & Data Management"
        description="Centralized management of business terminology, database schema information, and query patterns to enhance AI understanding."
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      {/* Overview Statistics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Business Terms"
              value={0}
              prefix={<BookOutlined />}
              suffix="defined"
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Database Tables"
              value={0}
              prefix={<TableOutlined />}
              suffix="configured"
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Query Patterns"
              value={0}
              prefix={<SearchOutlined />}
              suffix="learned"
            />
          </Card>
        </Col>
      </Row>

      <Tabs activeKey={activeTab} onChange={setActiveTab} size="large">
        <TabPane
          tab={
            <span>
              <BookOutlined />
              Business Glossary
            </span>
          }
          key="glossary"
        >
          <Card
            title={
              <Space>
                <BookOutlined />
                <span>Business Terms & Definitions</span>
              </Space>
            }
          >
            <Alert
              message="Business Glossary"
              description="Define business terms, concepts, and their relationships to help AI understand domain-specific language and context."
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <BusinessGlossaryManager onDataChange={handleDataChange} />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <TableOutlined />
              Database Tables
            </span>
          }
          key="tables"
        >
          <Card
            title={
              <Space>
                <DatabaseOutlined />
                <span>Database Schema Management</span>
              </Space>
            }
          >
            <Alert
              message="Database Table Configuration"
              description="Configure database tables, columns, and relationships to provide AI with comprehensive schema understanding."
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <BusinessTableManager onDataChange={handleDataChange} />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <SearchOutlined />
              Query Patterns
            </span>
          }
          key="patterns"
        >
          <Card
            title={
              <Space>
                <BranchesOutlined />
                <span>Query Pattern Analysis</span>
              </Space>
            }
          >
            <Alert
              message="Query Pattern Learning"
              description="Analyze and manage common query patterns to improve AI's ability to generate accurate and efficient SQL queries."
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            <QueryPatternManager onDataChange={handleDataChange} />
          </Card>
        </TabPane>

        <TabPane
          tab={
            <span>
              <FileTextOutlined />
              Documentation
            </span>
          }
          key="documentation"
        >
          <Card
            title={
              <Space>
                <FileTextOutlined />
                <span>Knowledge Base Documentation</span>
              </Space>
            }
          >
            <Alert
              message="Coming Soon"
              description="Comprehensive documentation and knowledge base articles will be available here to help users understand business processes and data relationships."
              type="info"
              showIcon
            />
            
            <div style={{ padding: '40px', textAlign: 'center' }}>
              <Title level={4}>Documentation Hub</Title>
              <p>This section will include:</p>
              <ul style={{ textAlign: 'left', display: 'inline-block' }}>
                <li>Business process documentation</li>
                <li>Data dictionary and field definitions</li>
                <li>Common query examples and use cases</li>
                <li>Best practices for data analysis</li>
                <li>Troubleshooting guides</li>
              </ul>
            </div>
          </Card>
        </TabPane>
      </Tabs>
    </div>
  );
};
