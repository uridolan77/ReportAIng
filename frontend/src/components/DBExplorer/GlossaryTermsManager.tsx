import React, { useState, useEffect } from 'react';
import {
  Modal,
  Table,
  Button,
  Space,
  Typography,
  Tag,
  Input,
  Form,
  message,
  Popconfirm,
  Select,
  Card,
  Divider,
  List,
  Tooltip
} from 'antd';
import {
  EditOutlined,
  SaveOutlined,
  DeleteOutlined,
  PlusOutlined,
  SearchOutlined,
  TagsOutlined
} from '@ant-design/icons';
import { BusinessGlossaryTerm } from '../../services/tuningApi';
import { tuningApi } from '../../services/tuningApi';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

interface GlossaryTermsManagerProps {
  visible: boolean;
  onClose: () => void;
  initialTerms?: BusinessGlossaryTerm[];
  onTermsUpdated?: (terms: BusinessGlossaryTerm[]) => void;
}

interface EditableGlossaryTerm extends BusinessGlossaryTerm {
  isEditing?: boolean;
  isNew?: boolean;
}

export const GlossaryTermsManager: React.FC<GlossaryTermsManagerProps> = ({
  visible,
  onClose,
  initialTerms = [],
  onTermsUpdated
}) => {
  const [terms, setTerms] = useState<EditableGlossaryTerm[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [form] = Form.useForm();

  // Load terms when modal opens
  useEffect(() => {
    if (visible) {
      loadGlossaryTerms();
    }
  }, [visible]);

  const loadGlossaryTerms = async () => {
    try {
      setLoading(true);
      const allTerms = await tuningApi.getGlossaryTerms();
      const combinedTerms = [...allTerms, ...initialTerms.filter(
        initial => !allTerms.some(existing => existing.term === initial.term)
      )];
      setTerms(combinedTerms.map(term => ({ ...term, isEditing: false, isNew: false })));
    } catch (error) {
      message.error('Failed to load glossary terms');
      setTerms(initialTerms.map(term => ({ ...term, isEditing: false, isNew: false })));
    } finally {
      setLoading(false);
    }
  };

  // Filter terms based on search and category
  const filteredTerms = terms.filter(term => {
    const matchesSearch = !searchTerm || 
      term.term.toLowerCase().includes(searchTerm.toLowerCase()) ||
      term.definition.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory = !selectedCategory || term.category === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  // Get unique categories
  const categories = [...new Set(terms.map(term => term.category).filter(Boolean))];

  const columns = [
    {
      title: 'Term',
      dataIndex: 'term',
      key: 'term',
      width: 200,
      render: (text: string, record: EditableGlossaryTerm, index: number) => 
        record.isEditing ? (
          <Input
            defaultValue={text}
            onBlur={(e) => handleUpdateTerm(index, 'term', e.target.value)}
            onPressEnter={(e) => handleUpdateTerm(index, 'term', (e.target as HTMLInputElement).value)}
          />
        ) : (
          <Space>
            <Text strong>{text}</Text>
            {record.isNew && <Tag color="blue">New</Tag>}
          </Space>
        )
    },
    {
      title: 'Definition',
      dataIndex: 'definition',
      key: 'definition',
      render: (text: string, record: EditableGlossaryTerm, index: number) => 
        record.isEditing ? (
          <TextArea
            defaultValue={text}
            onBlur={(e) => handleUpdateTerm(index, 'definition', e.target.value)}
            autoSize={{ minRows: 2, maxRows: 4 }}
          />
        ) : (
          <Text ellipsis={{ tooltip: text }}>{text}</Text>
        )
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      width: 120,
      render: (text: string, record: EditableGlossaryTerm, index: number) => 
        record.isEditing ? (
          <Select
            defaultValue={text}
            style={{ width: '100%' }}
            onBlur={(value) => handleUpdateTerm(index, 'category', value)}
            onChange={(value) => handleUpdateTerm(index, 'category', value)}
          >
            {categories.map(cat => (
              <Option key={cat} value={cat}>{cat}</Option>
            ))}
            <Option value="General">General</Option>
            <Option value="Business">Business</Option>
            <Option value="Technical">Technical</Option>
          </Select>
        ) : (
          <Tag color="blue">{text || 'General'}</Tag>
        )
    },
    {
      title: 'Synonyms',
      dataIndex: 'synonyms',
      key: 'synonyms',
      width: 150,
      render: (synonyms: string[]) => (
        <Space wrap>
          {synonyms?.slice(0, 2).map((synonym, idx) => (
            <Tag key={idx} size="small">{synonym}</Tag>
          ))}
          {synonyms?.length > 2 && (
            <Tooltip title={synonyms.slice(2).join(', ')}>
              <Tag size="small">+{synonyms.length - 2} more</Tag>
            </Tooltip>
          )}
        </Space>
      )
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_: any, record: EditableGlossaryTerm, index: number) => (
        <Space>
          <Button
            type="text"
            icon={<EditOutlined />}
            size="small"
            onClick={() => handleEditTerm(index)}
            disabled={record.isEditing}
          />
          <Button
            type="primary"
            icon={<SaveOutlined />}
            size="small"
            onClick={() => handleSaveTerm(record, index)}
            loading={loading}
          />
          <Popconfirm
            title="Are you sure you want to delete this term?"
            onConfirm={() => handleDeleteTerm(index)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="text"
              icon={<DeleteOutlined />}
              size="small"
              danger
            />
          </Popconfirm>
        </Space>
      )
    }
  ];

  const handleEditTerm = (index: number) => {
    const newTerms = [...terms];
    newTerms[index].isEditing = true;
    setTerms(newTerms);
  };

  const handleUpdateTerm = (index: number, field: keyof EditableGlossaryTerm, value: string) => {
    const newTerms = [...terms];
    (newTerms[index] as any)[field] = value;
    setTerms(newTerms);
  };

  const handleSaveTerm = async (term: EditableGlossaryTerm, index: number) => {
    try {
      setLoading(true);
      let savedTerm: BusinessGlossaryTerm;
      
      if (term.isNew || term.id === 0) {
        savedTerm = await tuningApi.createGlossaryTerm(term);
      } else {
        savedTerm = await tuningApi.updateGlossaryTerm(term.id, term);
      }
      
      const newTerms = [...terms];
      newTerms[index] = { ...savedTerm, isEditing: false, isNew: false };
      setTerms(newTerms);
      
      message.success(`Glossary term "${term.term}" saved successfully`);
      onTermsUpdated?.(newTerms);
    } catch (error) {
      message.error(`Failed to save term: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteTerm = async (index: number) => {
    const term = terms[index];
    
    try {
      setLoading(true);
      
      if (!term.isNew && term.id > 0) {
        await tuningApi.deleteGlossaryTerm(term.id);
      }
      
      const newTerms = terms.filter((_, i) => i !== index);
      setTerms(newTerms);
      
      message.success(`Glossary term "${term.term}" deleted successfully`);
      onTermsUpdated?.(newTerms);
    } catch (error) {
      message.error(`Failed to delete term: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setLoading(false);
    }
  };

  const handleAddNewTerm = () => {
    const newTerm: EditableGlossaryTerm = {
      id: 0,
      term: '',
      definition: '',
      businessContext: '',
      synonyms: [],
      relatedTerms: [],
      category: 'General',
      examples: [],
      isActive: true,
      usageCount: 0,
      lastUsedDate: undefined,
      createdDate: new Date().toISOString(),
      updatedDate: undefined,
      createdBy: '',
      updatedBy: undefined,
      isEditing: true,
      isNew: true
    };
    
    setTerms([newTerm, ...terms]);
  };

  const handleSaveAll = async () => {
    try {
      setLoading(true);
      const unsavedTerms = terms.filter(term => term.isNew || term.isEditing);
      
      for (const term of unsavedTerms) {
        const index = terms.findIndex(t => t === term);
        await handleSaveTerm(term, index);
      }
      
      message.success('All terms saved successfully');
    } catch (error) {
      message.error('Failed to save all terms');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={
        <Space>
          <TagsOutlined />
          <span>Glossary Terms Manager</span>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      width={1200}
      footer={[
        <Button key="close" onClick={onClose}>
          Close
        </Button>,
        <Button
          key="save-all"
          type="primary"
          loading={loading}
          onClick={handleSaveAll}
          icon={<SaveOutlined />}
        >
          Save All Changes
        </Button>
      ]}
    >
      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        {/* Search and Filter Controls */}
        <Card size="small">
          <Space wrap>
            <Input
              placeholder="Search terms or definitions..."
              prefix={<SearchOutlined />}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{ width: 300 }}
            />
            <Select
              placeholder="Filter by category"
              value={selectedCategory}
              onChange={setSelectedCategory}
              allowClear
              style={{ width: 200 }}
            >
              {categories.map(cat => (
                <Option key={cat} value={cat}>{cat}</Option>
              ))}
            </Select>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={handleAddNewTerm}
            >
              Add New Term
            </Button>
          </Space>
        </Card>

        {/* Terms Table */}
        <Table
          dataSource={filteredTerms}
          columns={columns}
          rowKey={(record) => `${record.term}-${record.id}`}
          loading={loading}
          size="small"
          pagination={{ 
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} terms`
          }}
          scroll={{ y: 400 }}
        />
      </Space>
    </Modal>
  );
};
