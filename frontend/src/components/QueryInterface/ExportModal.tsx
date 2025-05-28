import React, { useState } from 'react';
import { Modal, Form, Select, Switch, Input, Button, Space, message } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import { QueryResponse } from '../../types/query';

const { Option } = Select;

interface ExportModalProps {
  visible: boolean;
  onClose: () => void;
  result: QueryResponse | null;
  query: string;
}

export const ExportModal: React.FC<ExportModalProps> = ({ 
  visible, 
  onClose, 
  result, 
  query 
}) => {
  const [form] = Form.useForm();
  const [exporting, setExporting] = useState(false);

  const handleExport = async (values: any) => {
    if (!result) return;

    setExporting(true);
    try {
      // Simulate export process
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // In a real implementation, this would call the backend export API
      const exportData = {
        format: values.format,
        filename: values.filename || `query-result-${Date.now()}`,
        includeMetadata: values.includeMetadata,
        data: result.result?.data,
        metadata: result.result?.metadata,
        query: query,
        sql: result.sql,
      };

      // For demo purposes, just download as JSON
      const blob = new Blob([JSON.stringify(exportData, null, 2)], { 
        type: 'application/json' 
      });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${exportData.filename}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);

      message.success('Export completed successfully!');
      onClose();
    } catch (error) {
      message.error('Export failed. Please try again.');
    } finally {
      setExporting(false);
    }
  };

  return (
    <Modal
      title="Export Query Results"
      open={visible}
      onCancel={onClose}
      footer={null}
      width={500}
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={handleExport}
        initialValues={{
          format: 'csv',
          includeMetadata: true,
          filename: `query-result-${new Date().toISOString().split('T')[0]}`,
        }}
      >
        <Form.Item
          name="format"
          label="Export Format"
          rules={[{ required: true, message: 'Please select an export format' }]}
        >
          <Select>
            <Option value="csv">CSV</Option>
            <Option value="excel">Excel (.xlsx)</Option>
            <Option value="pdf">PDF Report</Option>
            <Option value="json">JSON</Option>
          </Select>
        </Form.Item>

        <Form.Item
          name="filename"
          label="Filename"
          rules={[{ required: true, message: 'Please enter a filename' }]}
        >
          <Input placeholder="Enter filename (without extension)" />
        </Form.Item>

        <Form.Item
          name="includeMetadata"
          label="Include Metadata"
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>

        <Form.Item style={{ marginBottom: 0 }}>
          <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
            <Button onClick={onClose}>
              Cancel
            </Button>
            <Button 
              type="primary" 
              htmlType="submit" 
              loading={exporting}
              icon={<DownloadOutlined />}
            >
              Export
            </Button>
          </Space>
        </Form.Item>
      </Form>

      {result && (
        <div style={{ marginTop: '16px', padding: '12px', background: '#f5f5f5', borderRadius: '6px' }}>
          <div style={{ fontSize: '12px', color: '#666' }}>
            <strong>Preview:</strong><br />
            Rows: {result.result?.metadata.rowCount || 0}<br />
            Columns: {result.result?.metadata.columnCount || 0}<br />
            Execution Time: {result.executionTimeMs}ms
          </div>
        </div>
      )}
    </Modal>
  );
};
