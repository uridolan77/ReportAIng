import React, { useState } from 'react';
import {
  Card,
  Button,
  Badge,
  Dropdown,
  Modal,
  Space,
  Typography,
  Empty,
  Menu
} from 'antd';
import {
  MoreOutlined,
  StarOutlined,
  StarFilled,
  EditOutlined,
  DeleteOutlined,
  CopyOutlined,
  DownloadOutlined,
  UserOutlined,
  CalendarOutlined,
  SettingOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import { BusinessSchemaDto } from '../../types/schemaManagement';
import { schemaManagementApi } from '../../services/schemaManagementApi';

const { Text } = Typography;
const { confirm } = Modal;

interface SchemaListProps {
  schemas: BusinessSchemaDto[];
  selectedSchema: BusinessSchemaDto | null;
  onSchemaSelect: (schema: BusinessSchemaDto) => void;
  onSchemaUpdated: (schema: BusinessSchemaDto) => void;
  onSchemaDeleted: (schemaId: string) => void;
  onSetDefault: (schemaId: string) => void;
}

export const SchemaList: React.FC<SchemaListProps> = ({
  schemas,
  selectedSchema,
  onSchemaSelect,
  onSchemaUpdated,
  onSchemaDeleted,
  onSetDefault
}) => {
  const [loading, setLoading] = useState<string | null>(null);

  const handleDeleteClick = (schema: BusinessSchemaDto) => {
    confirm({
      title: 'Delete Schema',
      icon: <ExclamationCircleOutlined />,
      content: `Are you sure you want to delete "${schema.name}"? This action cannot be undone. All versions and associated data will be permanently removed.`,
      okText: 'Delete',
      okType: 'danger',
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          setLoading(schema.id);
          await schemaManagementApi.deleteSchema(schema.id);
          onSchemaDeleted(schema.id);
        } catch (error) {
          console.error('Error deleting schema:', error);
        } finally {
          setLoading(null);
        }
      },
    });
  };

  const handleSetDefault = async (schema: BusinessSchemaDto) => {
    try {
      setLoading(schema.id);
      await onSetDefault(schema.id);
    } catch (error) {
      console.error('Error setting default schema:', error);
    } finally {
      setLoading(null);
    }
  };

  const handleDuplicateSchema = async (schema: BusinessSchemaDto) => {
    try {
      setLoading(schema.id);
      const newSchema = await schemaManagementApi.createSchema({
        name: `${schema.name} (Copy)`,
        description: `Copy of ${schema.description || schema.name}`,
        tags: [...schema.tags, 'Copy']
      });
      onSchemaUpdated(newSchema);
    } catch (error) {
      console.error('Error duplicating schema:', error);
    } finally {
      setLoading(null);
    }
  };

  const handleExportSchema = async (schema: BusinessSchemaDto) => {
    if (!schema.currentVersion) return;

    try {
      setLoading(schema.id);
      const schemaData = await schemaManagementApi.exportSchemaVersion(schema.currentVersion.id);
      const blob = new Blob([JSON.stringify(schemaData, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `schema-${schema.name.replace(/\s+/g, '-').toLowerCase()}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error exporting schema:', error);
    } finally {
      setLoading(null);
    }
  };

  if (schemas.length === 0) {
    return (
      <Empty
        image={<SettingOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />}
        description={
          <div>
            <Text strong>No Schemas</Text>
            <br />
            <Text type="secondary">Create your first business schema to get started.</Text>
          </div>
        }
      />
    );
  }

  const getDropdownMenu = (schema: BusinessSchemaDto) => (
    <Menu>
      <Menu.Item
        key="edit"
        icon={<EditOutlined />}
        onClick={() => onSchemaSelect(schema)}
      >
        Edit
      </Menu.Item>
      <Menu.Item
        key="duplicate"
        icon={<CopyOutlined />}
        onClick={() => handleDuplicateSchema(schema)}
        disabled={loading === schema.id}
      >
        Duplicate
      </Menu.Item>
      <Menu.Item
        key="export"
        icon={<DownloadOutlined />}
        onClick={() => handleExportSchema(schema)}
        disabled={loading === schema.id || !schema.currentVersion}
      >
        Export
      </Menu.Item>
      <Menu.Divider />
      {!schema.isDefault ? (
        <Menu.Item
          key="setDefault"
          icon={<StarOutlined />}
          onClick={() => handleSetDefault(schema)}
          disabled={loading === schema.id}
        >
          Set as Default
        </Menu.Item>
      ) : (
        <Menu.Item key="alreadyDefault" icon={<StarFilled />} disabled>
          Already Default
        </Menu.Item>
      )}
      <Menu.Divider />
      <Menu.Item
        key="delete"
        icon={<DeleteOutlined />}
        onClick={() => handleDeleteClick(schema)}
        disabled={loading === schema.id || schema.isDefault}
        danger
      >
        Delete
      </Menu.Item>
    </Menu>
  );

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
      {schemas.map((schema) => (
        <Card
          key={schema.id}
          hoverable
          style={{
            cursor: 'pointer',
            border: selectedSchema?.id === schema.id ? '2px solid #1890ff' : '1px solid #d9d9d9',
            backgroundColor: selectedSchema?.id === schema.id ? '#f0f5ff' : 'white'
          }}
          onClick={() => onSchemaSelect(schema)}
          bodyStyle={{ padding: '16px' }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
                <Text strong style={{ fontSize: '16px' }}>{schema.name}</Text>
                <Space>
                  {schema.isDefault && (
                    <Badge status="success" text="Default" />
                  )}
                  <Badge
                    status={schema.isActive ? "processing" : "default"}
                    text={schema.isActive ? "Active" : "Inactive"}
                  />
                </Space>
              </div>

              <Text type="secondary" style={{ display: 'block', marginBottom: '12px' }}>
                {schema.description || 'No description provided'}
              </Text>

              <Space size="large" style={{ fontSize: '12px', color: '#8c8c8c' }}>
                <Space size="small">
                  <UserOutlined />
                  <span>{schema.createdBy}</span>
                </Space>
                <Space size="small">
                  <CalendarOutlined />
                  <span>{new Date(schema.createdAt).toLocaleDateString()}</span>
                </Space>
                <Space size="small">
                  <SettingOutlined />
                  <span>{schema.totalVersions} version{schema.totalVersions !== 1 ? 's' : ''}</span>
                </Space>
              </Space>

              {schema.tags.length > 0 && (
                <div style={{ marginTop: '8px' }}>
                  <Space wrap>
                    {schema.tags.slice(0, 3).map((tag, index) => (
                      <Badge key={index} count={tag} style={{ backgroundColor: '#f0f0f0', color: '#666', fontSize: '10px' }} />
                    ))}
                    {schema.tags.length > 3 && (
                      <Badge count={`+${schema.tags.length - 3} more`} style={{ backgroundColor: '#f0f0f0', color: '#666', fontSize: '10px' }} />
                    )}
                  </Space>
                </div>
              )}
            </div>

            <Dropdown
              overlay={getDropdownMenu(schema)}
              trigger={['click']}
              placement="bottomRight"
            >
              <Button
                type="text"
                icon={<MoreOutlined />}
                onClick={(e) => e.stopPropagation()}
                loading={loading === schema.id}
                style={{ border: 'none', boxShadow: 'none' }}
              />
            </Dropdown>
          </div>
        </Card>
      ))}
    </div>
  );
};
