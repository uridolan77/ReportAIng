import React from 'react';
import { useToken } from 'antd/es/theme/internal';
import _ from 'lodash';
import { DataTableColumn } from '../types';

interface AggregationRowProps {
  columns: DataTableColumn[];
  data: any[];
  enableSelection: boolean;
  position?: 'top' | 'bottom';
}

export const AggregationRow: React.FC<AggregationRowProps> = ({
  columns,
  data,
  enableSelection,
  position = 'bottom'
}) => {
  const [, token] = useToken();

  const calculateAggregation = (column: DataTableColumn) => {
    if (!column.aggregation) return null;
    
    const values = data.map(row => _.get(row, column.dataIndex));
    let aggregatedValue;
    
    switch (column.aggregation) {
      case 'sum':
        aggregatedValue = _.sum(values.filter(v => typeof v === 'number'));
        break;
      case 'avg':
        aggregatedValue = _.mean(values.filter(v => typeof v === 'number'));
        break;
      case 'min':
        aggregatedValue = _.min(values);
        break;
      case 'max':
        aggregatedValue = _.max(values);
        break;
      case 'count':
        aggregatedValue = values.filter(v => v != null).length;
        break;
      case 'custom':
        aggregatedValue = column.customAggregation?.(values);
        break;
      default:
        return null;
    }
    
    return column.aggregationFormatter
      ? column.aggregationFormatter(aggregatedValue)
      : aggregatedValue;
  };

  return (
    <tr style={{ 
      background: token.colorBgTextHover, 
      fontWeight: 'bold',
      borderTop: position === 'bottom' ? `2px solid ${token.colorBorder}` : undefined,
      borderBottom: position === 'top' ? `2px solid ${token.colorBorder}` : undefined
    }}>
      {enableSelection && <td style={{ padding: '8px 16px' }} />}
      {columns.map(column => (
        <td key={column.key} style={{ padding: '8px 16px' }}>
          {calculateAggregation(column)}
        </td>
      ))}
    </tr>
  );
};
