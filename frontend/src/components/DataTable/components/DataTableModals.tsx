import React from 'react';
import { Modal, Space, Button, Typography, Checkbox } from 'antd';
import { 
  FileExcelOutlined, 
  FilePdfOutlined, 
  FileTextOutlined, 
  DownloadOutlined,
  LockOutlined 
} from '@ant-design/icons';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  horizontalListSortingStrategy,
} from '@dnd-kit/sortable';

const { Text } = Typography;

interface DataTableColumn {
  key: string;
  title: string;
  dataIndex: string;
  hidden?: boolean;
  locked?: boolean;
}

interface ColumnChooserModalProps {
  visible: boolean;
  columns: DataTableColumn[];
  onClose: () => void;
  onColumnToggle: (columnKey: string, visible: boolean) => void;
  onColumnReorder: (columns: DataTableColumn[]) => void;
}

interface ExportModalProps {
  visible: boolean;
  exportFormats: string[];
  onClose: () => void;
  onExport: (format: string) => void;
}

export const ColumnChooserModal: React.FC<ColumnChooserModalProps> = ({
  visible,
  columns,
  onClose,
  onColumnToggle,
  onColumnReorder
}) => {
  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  return (
    <Modal
      title="Column Chooser"
      open={visible}
      onCancel={onClose}
      onOk={onClose}
      width={600}
    >
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragEnd={(event) => {
          const { active, over } = event;
          if (active.id !== over?.id) {
            const oldIndex = columns.findIndex(col => col.key === active.id);
            const newIndex = columns.findIndex(col => col.key === over?.id);
            const reordered = arrayMove(columns, oldIndex, newIndex);
            onColumnReorder(reordered);
          }
        }}
      >
        <SortableContext
          items={columns.map(col => col.key)}
          strategy={horizontalListSortingStrategy}
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            {columns.map(column => (
              <div key={column.key} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <Checkbox
                  checked={!column.hidden}
                  onChange={e => onColumnToggle(column.key, e.target.checked)}
                  disabled={column.locked}
                />
                <span>{column.title}</span>
                {column.locked && <LockOutlined />}
              </div>
            ))}
          </Space>
        </SortableContext>
      </DndContext>
    </Modal>
  );
};

export const ExportModal: React.FC<ExportModalProps> = ({
  visible,
  exportFormats,
  onClose,
  onExport
}) => {
  return (
    <Modal
      title="Export Data"
      open={visible}
      onCancel={onClose}
      footer={null}
      width={400}
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        <Text>Choose export format:</Text>
        {exportFormats.map(format => (
          <Button
            key={format}
            block
            icon={
              format === 'excel' ? <FileExcelOutlined /> :
              format === 'pdf' ? <FilePdfOutlined /> :
              format === 'csv' ? <FileTextOutlined /> :
              <DownloadOutlined />
            }
            onClick={() => onExport(format)}
          >
            Export as {format.toUpperCase()}
          </Button>
        ))}
      </Space>
    </Modal>
  );
};
