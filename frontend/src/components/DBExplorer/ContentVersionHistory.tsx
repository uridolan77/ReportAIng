import React, { useState } from 'react';
import {
  Modal,
  Timeline,
  Typography,
  Space,
  Tag,
  Card,
  Descriptions,
  Button,
  Tooltip,
  Divider,
  List,
  Alert
} from 'antd';
import {
  HistoryOutlined,
  UserOutlined,
  EditOutlined,
  PlusOutlined,
  DeleteOutlined,
  RollbackOutlined
} from '@ant-design/icons';
import { ContentVersion, ContentChange } from '../../types/dbExplorer';

const { Title, Text } = Typography;

interface ContentVersionHistoryProps {
  visible: boolean;
  onClose: () => void;
  contentId: string;
  contentName: string;
  contentType: 'table_context' | 'glossary_term';
  versions: ContentVersion[];
  onRevertToVersion?: (versionId: string) => void;
}

export const ContentVersionHistory: React.FC<ContentVersionHistoryProps> = ({
  visible,
  onClose,
  contentId,
  contentName,
  contentType,
  versions,
  onRevertToVersion
}) => {
  const [selectedVersion, setSelectedVersion] = useState<ContentVersion | null>(null);

  const getActionIcon = (action: string) => {
    switch (action) {
      case 'created':
        return <PlusOutlined style={{ color: '#52c41a' }} />;
      case 'updated':
        return <EditOutlined style={{ color: '#1890ff' }} />;
      case 'deleted':
        return <DeleteOutlined style={{ color: '#ff4d4f' }} />;
      default:
        return <EditOutlined />;
    }
  };

  const getActionColor = (action: string) => {
    switch (action) {
      case 'created':
        return 'green';
      case 'updated':
        return 'blue';
      case 'deleted':
        return 'red';
      default:
        return 'default';
    }
  };

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  const renderChangeDetails = (change: ContentChange) => (
    <Card size="small" style={{ marginBottom: 8 }}>
      <Space direction="vertical" style={{ width: '100%' }}>
        <Space>
          {getActionIcon(change.action)}
          <Text strong>{change.itemName}</Text>
          <Tag color={getActionColor(change.action)}>{change.action}</Tag>
        </Space>
        
        {change.fieldChanges && change.fieldChanges.length > 0 && (
          <div>
            <Text type="secondary">Field Changes:</Text>
            <List
              size="small"
              dataSource={change.fieldChanges}
              renderItem={(fieldChange) => (
                <List.Item>
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Text strong>{fieldChange.field}:</Text>
                    <div style={{ paddingLeft: 16 }}>
                      <Text delete type="secondary">{fieldChange.oldValue}</Text>
                      <br />
                      <Text type="success">{fieldChange.newValue}</Text>
                    </div>
                  </Space>
                </List.Item>
              )}
            />
          </div>
        )}
      </Space>
    </Card>
  );

  const renderVersionDetails = (version: ContentVersion) => (
    <Modal
      title={`Version Details - ${formatTimestamp(version.timestamp)}`}
      open={!!selectedVersion}
      onCancel={() => setSelectedVersion(null)}
      width={800}
      footer={[
        <Button key="close" onClick={() => setSelectedVersion(null)}>
          Close
        </Button>,
        ...(onRevertToVersion ? [
          <Button
            key="revert"
            type="primary"
            danger
            icon={<RollbackOutlined />}
            onClick={() => {
              onRevertToVersion(version.id);
              setSelectedVersion(null);
            }}
          >
            Revert to This Version
          </Button>
        ] : [])
      ]}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        <Descriptions bordered size="small">
          <Descriptions.Item label="Version ID">{version.id}</Descriptions.Item>
          <Descriptions.Item label="Author">{version.author}</Descriptions.Item>
          <Descriptions.Item label="Timestamp">{formatTimestamp(version.timestamp)}</Descriptions.Item>
          <Descriptions.Item label="Changes Count">{version.changes.length}</Descriptions.Item>
        </Descriptions>
        
        {version.comment && (
          <Alert
            message="Version Comment"
            description={version.comment}
            type="info"
            showIcon
          />
        )}
        
        <Divider>Changes in This Version</Divider>
        
        <Space direction="vertical" style={{ width: '100%' }}>
          {version.changes.map((change, index) => (
            <div key={index}>
              {renderChangeDetails(change)}
            </div>
          ))}
        </Space>
      </Space>
    </Modal>
  );

  return (
    <>
      <Modal
        title={
          <Space>
            <HistoryOutlined />
            <span>Version History: {contentName}</span>
          </Space>
        }
        open={visible}
        onCancel={onClose}
        width={900}
        footer={[
          <Button key="close" onClick={onClose}>
            Close
          </Button>
        ]}
      >
        <Space direction="vertical" style={{ width: '100%' }} size="middle">
          <Alert
            message="Content Version History"
            description={`Track all changes made to this ${contentType.replace('_', ' ')} over time. Click on any version to see detailed changes.`}
            type="info"
            showIcon
          />
          
          {versions.length === 0 ? (
            <Card>
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <HistoryOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
                <div style={{ marginTop: 16 }}>
                  <Text type="secondary">No version history available</Text>
                </div>
              </div>
            </Card>
          ) : (
            <Timeline mode="left">
              {versions.map((version, index) => (
                <Timeline.Item
                  key={version.id}
                  color={index === 0 ? 'green' : 'blue'}
                  dot={index === 0 ? <EditOutlined style={{ fontSize: '16px' }} /> : undefined}
                >
                  <Card
                    size="small"
                    hoverable
                    onClick={() => setSelectedVersion(version)}
                    style={{ cursor: 'pointer' }}
                    extra={
                      index === 0 && (
                        <Tag color="green">Current</Tag>
                      )
                    }
                  >
                    <Space direction="vertical" style={{ width: '100%' }}>
                      <Space>
                        <UserOutlined />
                        <Text strong>{version.author}</Text>
                        <Text type="secondary">{formatTimestamp(version.timestamp)}</Text>
                      </Space>
                      
                      <Space wrap>
                        {version.changes.map((change, changeIndex) => (
                          <Tag
                            key={changeIndex}
                            color={getActionColor(change.action)}
                            icon={getActionIcon(change.action)}
                          >
                            {change.action} {change.itemName}
                          </Tag>
                        ))}
                      </Space>
                      
                      {version.comment && (
                        <Text type="secondary" italic>
                          "{version.comment}"
                        </Text>
                      )}
                      
                      <div>
                        <Button
                          type="link"
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation();
                            setSelectedVersion(version);
                          }}
                        >
                          View Details
                        </Button>
                        {onRevertToVersion && index > 0 && (
                          <Button
                            type="link"
                            size="small"
                            danger
                            icon={<RollbackOutlined />}
                            onClick={(e) => {
                              e.stopPropagation();
                              onRevertToVersion(version.id);
                            }}
                          >
                            Revert
                          </Button>
                        )}
                      </div>
                    </Space>
                  </Card>
                </Timeline.Item>
              ))}
            </Timeline>
          )}
        </Space>
      </Modal>
      
      {selectedVersion && renderVersionDetails(selectedVersion)}
    </>
  );
};
