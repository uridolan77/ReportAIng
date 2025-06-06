import React, { useState } from 'react';
import { 
  Card, 
  Collapse, 
  Typography, 
  Tag, 
  Space, 
  Button, 
  Tooltip, 
  Divider,
  Badge,
  Row,
  Col,
  Alert
} from 'antd';
import {
  CodeOutlined,
  InfoCircleOutlined,
  CopyOutlined,
  ExpandAltOutlined,
  ClockCircleOutlined,
  TagOutlined,
  FileTextOutlined,
  DatabaseOutlined,
  BulbOutlined,
  QuestionCircleOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { PromptDetails } from '../../types/query';

const { Panel } = Collapse;
const { Title, Text, Paragraph } = Typography;

interface PromptDetailsPanelProps {
  promptDetails: PromptDetails;
  className?: string;
}

export const PromptDetailsPanel: React.FC<PromptDetailsPanelProps> = ({
  promptDetails,
  className
}) => {
  const [copiedSection, setCopiedSection] = useState<string | null>(null);

  const handleCopyToClipboard = async (content: string, sectionName: string) => {
    try {
      await navigator.clipboard.writeText(content);
      setCopiedSection(sectionName);
      setTimeout(() => setCopiedSection(null), 2000);
    } catch (err) {
      console.error('Failed to copy to clipboard:', err);
    }
  };

  const getSectionIcon = (type: string) => {
    switch (type) {
      case 'template':
        return <SettingOutlined />;
      case 'user_input':
        return <QuestionCircleOutlined />;
      case 'schema':
        return <DatabaseOutlined />;
      case 'business_rules':
        return <BulbOutlined />;
      case 'examples':
        return <FileTextOutlined />;
      case 'context':
        return <InfoCircleOutlined />;
      default:
        return <CodeOutlined />;
    }
  };

  const getSectionColor = (type: string) => {
    switch (type) {
      case 'template':
        return 'blue';
      case 'user_input':
        return 'green';
      case 'schema':
        return 'blue';
      case 'business_rules':
        return 'orange';
      case 'examples':
        return 'cyan';
      case 'context':
        return 'geekblue';
      default:
        return 'default';
    }
  };

  const formatContent = (content: string, type: string) => {
    if (type === 'schema' && content.length > 1000) {
      return content.substring(0, 1000) + '... (truncated)';
    }
    return content;
  };

  const sortedSections = [...promptDetails.sections].sort((a, b) => a.order - b.order);

  return (
    <div className={`prompt-details-panel ${className || ''}`}>
      <Card 
        title={
          <Space>
            <CodeOutlined />
            <span>AI Prompt Details</span>
            <Badge count={promptDetails.tokenCount} showZero color="blue" />
          </Space>
        }
        extra={
          <Space>
            <Tooltip title="Copy Full Prompt">
              <Button
                icon={<CopyOutlined />}
                size="small"
                onClick={() => handleCopyToClipboard(promptDetails.fullPrompt, 'full')}
                type={copiedSection === 'full' ? 'primary' : 'default'}
              >
                {copiedSection === 'full' ? 'Copied!' : 'Copy All'}
              </Button>
            </Tooltip>
          </Space>
        }
        size="small"
      >
        {/* Prompt Overview */}
        <Row gutter={[16, 8]} style={{ marginBottom: 16 }}>
          <Col span={8}>
            <Space direction="vertical" size="small">
              <Text type="secondary">Template</Text>
              <Text strong>{promptDetails.templateName}</Text>
            </Space>
          </Col>
          <Col span={8}>
            <Space direction="vertical" size="small">
              <Text type="secondary">Version</Text>
              <Tag color="blue">{promptDetails.templateVersion}</Tag>
            </Space>
          </Col>
          <Col span={8}>
            <Space direction="vertical" size="small">
              <Text type="secondary">Generated</Text>
              <Text>
                <ClockCircleOutlined style={{ marginRight: 4 }} />
                {new Date(promptDetails.generatedAt).toLocaleTimeString()}
              </Text>
            </Space>
          </Col>
        </Row>

        <Divider />

        {/* Variables Section */}
        {Object.keys(promptDetails.variables).length > 0 && (
          <>
            <Title level={5}>
              <TagOutlined /> Template Variables
            </Title>
            <Space wrap style={{ marginBottom: 16 }}>
              {Object.entries(promptDetails.variables).map(([key, value]) => (
                <Tooltip key={key} title={value.substring(0, 200) + (value.length > 200 ? '...' : '')}>
                  <Tag color="geekblue">{key}</Tag>
                </Tooltip>
              ))}
            </Space>
            <Divider />
          </>
        )}

        {/* Prompt Sections */}
        <Title level={5}>
          <FileTextOutlined /> Prompt Sections
        </Title>
        
        <Collapse 
          size="small" 
          ghost
          expandIcon={({ isActive }) => <ExpandAltOutlined rotate={isActive ? 90 : 0} />}
        >
          {sortedSections.map((section) => (
            <Panel
              key={section.name}
              header={
                <Space>
                  {getSectionIcon(section.type)}
                  <span>{section.title}</span>
                  <Tag color={getSectionColor(section.type)} size="small">
                    {section.type.replace('_', ' ')}
                  </Tag>
                  {section.metadata && (
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {section.metadata.tableCount && `${section.metadata.tableCount} tables`}
                      {section.metadata.totalColumns && `, ${section.metadata.totalColumns} columns`}
                    </Text>
                  )}
                </Space>
              }
              extra={
                <Button
                  icon={<CopyOutlined />}
                  size="small"
                  type="text"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleCopyToClipboard(section.content, section.name);
                  }}
                  style={{ 
                    opacity: copiedSection === section.name ? 1 : 0.6,
                    color: copiedSection === section.name ? '#1890ff' : undefined
                  }}
                >
                  {copiedSection === section.name ? 'Copied!' : 'Copy'}
                </Button>
              }
            >
              <Card size="small" style={{ backgroundColor: '#fafafa' }}>
                <Paragraph
                  copyable={{
                    text: section.content,
                    tooltips: ['Copy section content', 'Copied!']
                  }}
                  style={{ 
                    whiteSpace: 'pre-wrap',
                    fontFamily: 'monospace',
                    fontSize: '12px',
                    margin: 0,
                    maxHeight: '300px',
                    overflow: 'auto'
                  }}
                >
                  {formatContent(section.content, section.type)}
                </Paragraph>
                
                {section.metadata && Object.keys(section.metadata).length > 0 && (
                  <>
                    <Divider style={{ margin: '8px 0' }} />
                    <Space wrap>
                      {Object.entries(section.metadata).map(([key, value]) => (
                        <Tag key={key} size="small" color="default">
                          {key}: {String(value)}
                        </Tag>
                      ))}
                    </Space>
                  </>
                )}
              </Card>
            </Panel>
          ))}
        </Collapse>

        {/* Full Prompt Section */}
        <Divider />
        <Collapse size="small" ghost>
          <Panel
            key="full-prompt"
            header={
              <Space>
                <CodeOutlined />
                <span>Complete Generated Prompt</span>
                <Badge count={`${promptDetails.tokenCount} tokens`} color="blue" />
              </Space>
            }
          >
            <Alert
              message="This is the complete prompt sent to the AI model"
              type="info"
              showIcon
              style={{ marginBottom: 12 }}
            />
            <Card size="small" style={{ backgroundColor: '#f6f8fa' }}>
              <Paragraph
                copyable={{
                  text: promptDetails.fullPrompt,
                  tooltips: ['Copy full prompt', 'Copied!']
                }}
                style={{ 
                  whiteSpace: 'pre-wrap',
                  fontFamily: 'monospace',
                  fontSize: '11px',
                  margin: 0,
                  maxHeight: '400px',
                  overflow: 'auto'
                }}
              >
                {promptDetails.fullPrompt}
              </Paragraph>
            </Card>
          </Panel>
        </Collapse>
      </Card>
    </div>
  );
};

export default PromptDetailsPanel;
