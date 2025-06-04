import React from 'react';
import { Input, InputNumber, DatePicker, Select, Switch } from 'antd';
import moment from 'moment';

interface DataTableColumn {
  editor?: 'text' | 'number' | 'date' | 'select' | 'multiselect' | 'boolean' | 'custom';
  editorOptions?: any;
}

interface CellEditorProps {
  value: any;
  column: DataTableColumn;
  onSave: (value: any) => void;
  autoFocus?: boolean;
}

export const CellEditor: React.FC<CellEditorProps> = ({
  value,
  column,
  onSave,
  autoFocus = true
}) => {
  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
    }
  };

  switch (column.editor) {
    case 'text':
      return (
        <Input
          defaultValue={value}
          autoFocus={autoFocus}
          onPressEnter={e => onSave((e.target as any).value)}
          onBlur={e => onSave(e.target.value)}
          onKeyPress={handleKeyPress}
        />
      );
    
    case 'number':
      return (
        <InputNumber
          defaultValue={value}
          autoFocus={autoFocus}
          onPressEnter={onSave}
          onBlur={onSave}
          onKeyPress={handleKeyPress}
          style={{ width: '100%' }}
        />
      );
    
    case 'date':
      return (
        <DatePicker
          defaultValue={value ? moment(value) : undefined}
          autoFocus={autoFocus}
          onChange={date => onSave(date?.toISOString())}
          style={{ width: '100%' }}
        />
      );
    
    case 'select':
      return (
        <Select
          defaultValue={value}
          autoFocus={autoFocus}
          onChange={onSave}
          options={column.editorOptions}
          style={{ width: '100%' }}
        />
      );

    case 'multiselect':
      return (
        <Select
          mode="multiple"
          defaultValue={value}
          autoFocus={autoFocus}
          onChange={onSave}
          options={column.editorOptions}
          style={{ width: '100%' }}
        />
      );
    
    case 'boolean':
      return (
        <Switch
          defaultChecked={value}
          onChange={onSave}
          autoFocus={autoFocus}
        />
      );
    
    default:
      return (
        <Input
          defaultValue={value}
          autoFocus={autoFocus}
          onPressEnter={e => onSave((e.target as any).value)}
          onBlur={e => onSave(e.target.value)}
        />
      );
  }
};
