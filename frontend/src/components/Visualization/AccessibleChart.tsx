import React, { useRef, useState } from 'react';
import { useAccessibility, generateChartDescription, generateDataTableDescription } from '../../hooks/useAccessibility';
import { Card } from 'antd';

interface AccessibleChartProps {
  data: any[];
  config: {
    type: string;
    title: string;
    xAxis?: string;
    yAxis?: string;
  };
  children: React.ReactNode; // The actual chart component
}

export const AccessibleChart: React.FC<AccessibleChartProps> = ({ data, config, children }) => {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const chartRef = useRef<HTMLDivElement>(null);
  const { announce, announcerRef, useKeyboardNavigation } = useAccessibility({
    enableKeyboardNavigation: true,
    enableScreenReaderSupport: true
  });

  useKeyboardNavigation(chartRef, {
    ArrowLeft: () => {
      const newIndex = Math.max(0, selectedIndex - 1);
      setSelectedIndex(newIndex);
      if (data[newIndex]) {
        announce(`Selected data point ${newIndex + 1}: ${data[newIndex].value || data[newIndex].y || 'No value'}`);
      }
    },
    ArrowRight: () => {
      const newIndex = Math.min(data.length - 1, selectedIndex + 1);
      setSelectedIndex(newIndex);
      if (data[newIndex]) {
        announce(`Selected data point ${newIndex + 1}: ${data[newIndex].value || data[newIndex].y || 'No value'}`);
      }
    },
    Enter: () => {
      if (data[selectedIndex]) {
        const point = data[selectedIndex];
        announce(`Data point details: ${JSON.stringify(point)}`, 'assertive');
      }
    },
    Escape: () => {
      announce('Chart navigation exited');
      if (chartRef.current) {
        chartRef.current.blur();
      }
    }
  });

  const chartDescription = generateChartDescription(data, config);
  const tableDescription = generateDataTableDescription(data);

  return (
    <Card title={config.title}>
      <div
        ref={chartRef}
        role="img"
        aria-label={chartDescription}
        aria-describedby="chart-description chart-instructions"
        tabIndex={0}
        style={{
          outline: 'none',
          border: '1px solid transparent',
          borderRadius: '4px'
        }}
        onFocus={() => {
          announce('Chart focused. Use arrow keys to navigate data points, Enter for details, Escape to exit.');
        }}
      >
        {children}
        
        {/* Hidden descriptions for screen readers */}
        <div id="chart-description" className="sr-only">
          {tableDescription}
        </div>
        
        <div id="chart-instructions" className="sr-only">
          Use arrow keys to navigate between data points. Press Enter to hear detailed information about the selected point. Press Escape to exit chart navigation.
        </div>
      </div>

      {/* Data table for accessibility */}
      <details style={{ marginTop: '16px' }}>
        <summary>View data table</summary>
        <table style={{ width: '100%', marginTop: '8px', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              {data.length > 0 && Object.keys(data[0]).map((key) => (
                <th key={key} style={{ border: '1px solid #ddd', padding: '8px', textAlign: 'left' }}>
                  {key}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((row, index) => (
              <tr key={index} style={{ backgroundColor: index === selectedIndex ? '#e6f7ff' : 'transparent' }}>
                {Object.values(row).map((value, cellIndex) => (
                  <td key={cellIndex} style={{ border: '1px solid #ddd', padding: '8px' }}>
                    {String(value)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </details>

      {/* Live region for announcements */}
      <div ref={announcerRef} aria-live="polite" className="sr-only">
        {/* Announcements will be inserted here */}
      </div>
    </Card>
  );
};

// Higher-order component to wrap existing charts with accessibility
export const withAccessibility = <P extends object>(
  WrappedComponent: React.ComponentType<P>
) => {
  return React.forwardRef<any, P & AccessibleChartProps>((props, ref) => {
    const { data, config, ...otherProps } = props;
    
    return (
      <AccessibleChart data={data} config={config}>
        <WrappedComponent {...(otherProps as P)} ref={ref} />
      </AccessibleChart>
    );
  });
};

// Example usage component
export const AccessibleBarChart: React.FC<{
  data: Array<{ name: string; value: number }>;
  title: string;
}> = ({ data, title }) => {
  return (
    <AccessibleChart
      data={data}
      config={{
        type: 'bar',
        title,
        xAxis: 'name',
        yAxis: 'value'
      }}
    >
      {/* Your actual chart implementation would go here */}
      <div style={{ height: '300px', background: '#f5f5f5', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <p>Chart visualization would render here</p>
      </div>
    </AccessibleChart>
  );
};
