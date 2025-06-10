/**
 * Filter Step
 */

import React from 'react';
import { Button, Space, Empty } from 'antd';
import { WizardStepProps } from './types';

const FilterStep: React.FC<WizardStepProps> = ({
  onNext,
  onPrevious,
  data,
  onChange
}) => {
  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <h3 style={{ marginBottom: '8px' }}>Add Filters</h3>
        <p style={{ color: '#8c8c8c', marginBottom: '16px' }}>
          Add conditions to filter your data
        </p>
      </div>

      <Empty
        description="Filter configuration coming soon"
        style={{ margin: '40px 0' }}
      />

      <div style={{ 
        marginTop: '24px', 
        paddingTop: '24px', 
        borderTop: '1px solid #f0f0f0',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <div />
        <Space>
          <Button onClick={onPrevious}>
            Previous
          </Button>
          <Button
            type="primary"
            onClick={onNext}
            style={{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none'
            }}
          >
            Next: Grouping
          </Button>
        </Space>
      </div>
    </div>
  );
};

export default FilterStep;
