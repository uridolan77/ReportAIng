import React, { useState, useEffect, useMemo, useCallback } from 'react';
import {
  Card,
  Row,
  Col,
  Typography,
  Button,
  Space,
  Spin,

  Select,
  DatePicker,
  Tooltip,
  Switch,
  Slider,
  Drawer,
  Badge,
  Divider
} from 'antd';
import {
  FullscreenOutlined,
  DownloadOutlined,
  ReloadOutlined,
  SettingOutlined,
  FilterOutlined,
  ShareAltOutlined,
  EyeOutlined,
  ClockCircleOutlined
} from '@ant-design/icons';
import {
  AdvancedDashboardConfig,

  DashboardGenerationMetrics,
  DataSummary
} from '../../types/visualization';
import AdvancedChart from './AdvancedChart';
import dayjs from 'dayjs';

const { Title, Text } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;

interface AdvancedDashboardProps {
  config: AdvancedDashboardConfig;
  data: any[];
  loading?: boolean;
  onRefresh?: () => void;
  onExport?: (format: string) => void;
  onShare?: () => void;
  generationMetrics?: DashboardGenerationMetrics;
  dataSummary?: DataSummary;
}

const AdvancedDashboard: React.FC<AdvancedDashboardProps> = ({
  config,
  data,
  loading = false,
  onRefresh,
  onExport,
  onShare,
  generationMetrics,
  dataSummary
}) => {
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [globalFilters, setGlobalFilters] = useState<Record<string, any>>({});
  const [lastUpdated, setLastUpdated] = useState(dayjs());
  const [autoRefresh, setAutoRefresh] = useState(config.realTime?.autoRefresh ?? false);
  const [refreshInterval, setRefreshInterval] = useState(config.refreshInterval ?? 30);

  // Auto-refresh functionality
  useEffect(() => {
    if (autoRefresh && config.realTime?.enabled) {
      const interval = setInterval(() => {
        onRefresh?.();
        setLastUpdated(dayjs());
      }, refreshInterval * 1000);

      return () => clearInterval(interval);
    }
  }, [autoRefresh, refreshInterval, onRefresh, config.realTime]);

  // Responsive layout calculation
  const responsiveLayout = useMemo(() => {
    const { layout, responsive } = config;

    if (!responsive?.enabled) {
      return {
        columns: layout.columns,
        chartSizes: layout.chartSizes
      };
    }

    // Simple responsive logic based on window width
    const width = window.innerWidth;

    if (width <= 768) {
      return responsive.breakpoints.mobile || { columns: 1, chartSizes: ['full'] };
    } else if (width <= 1024) {
      return responsive.breakpoints.tablet || { columns: 2, chartSizes: ['half'] };
    } else {
      return responsive.breakpoints.desktop || { columns: layout.columns, chartSizes: layout.chartSizes };
    }
  }, [config]);

  // Filter data based on global filters
  const filteredData = useMemo(() => {
    let filtered = [...data];

    Object.entries(globalFilters).forEach(([filterKey, filterValue]) => {
      if (filterValue && filterValue !== 'all') {
        const filter = config.globalFilters.find(f => f.name === filterKey);
        if (filter) {
          if (filter.type === 'daterange' && Array.isArray(filterValue)) {
            const [start, end] = filterValue;
            filtered = filtered.filter(item => {
              const itemDate = dayjs(item[filter.column]);
              return itemDate.isAfter(start) && itemDate.isBefore(end);
            });
          } else if (filter.type === 'multiselect' && Array.isArray(filterValue)) {
            filtered = filtered.filter(item => filterValue.includes(item[filter.column]));
          } else {
            filtered = filtered.filter(item => item[filter.column] === filterValue);
          }
        }
      }
    });

    return filtered;
  }, [data, globalFilters, config.globalFilters]);

  const handleFilterChange = useCallback((filterName: string, value: any) => {
    setGlobalFilters(prev => ({
      ...prev,
      [filterName]: value
    }));
  }, []);

  const getChartSize = (index: number) => {
    const size = responsiveLayout.chartSizes[index] || 'half';
    switch (size) {
      case 'full': return 24;
      case 'half': return 12;
      case 'quarter': return 6;
      case 'third': return 8;
      case 'small': return 8;
      case 'medium': return 12;
      case 'large': return 16;
      default: return 12;
    }
  };

  const renderGlobalFilters = () => (
    <Card size="small" title="Filters" style={{ marginBottom: 16 }}>
      <Row gutter={16}>
        {config.globalFilters.map((filter, index) => (
          <Col span={8} key={index}>
            <div style={{ marginBottom: 8 }}>
              <Text strong>{filter.name}</Text>
            </div>
            {filter.type === 'daterange' ? (
              <RangePicker
                size="small"
                style={{ width: '100%' }}
                onChange={(dates) => handleFilterChange(filter.name, dates)}
                defaultValue={filter.defaultValue === 'last30days' ? [
                  dayjs().subtract(30, 'days'),
                  dayjs()
                ] : undefined}
              />
            ) : filter.type === 'multiselect' ? (
              <Select
                mode="multiple"
                size="small"
                style={{ width: '100%' }}
                placeholder={`Select ${filter.name}`}
                onChange={(value) => handleFilterChange(filter.name, value)}
                defaultValue={filter.defaultValue === 'all' ? [] : filter.defaultValue}
              >
                {filter.options?.map((option: any) => (
                  <Option key={option.value} value={option.value}>
                    {option.label}
                  </Option>
                ))}
              </Select>
            ) : (
              <Select
                size="small"
                style={{ width: '100%' }}
                placeholder={`Select ${filter.name}`}
                onChange={(value) => handleFilterChange(filter.name, value)}
                defaultValue={filter.defaultValue}
              >
                <Option value="all">All</Option>
                {filter.options?.map((option: any) => (
                  <Option key={option.value} value={option.value}>
                    {option.label}
                  </Option>
                ))}
              </Select>
            )}
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderDashboardHeader = () => (
    <div style={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginBottom: 24,
      padding: '16px 0'
    }}>
      <div>
        <Title level={2} style={{ margin: 0 }}>
          {config.title}
        </Title>
        <Text type="secondary">{config.description}</Text>
      </div>

      <Space>
        {config.realTime?.enabled && (
          <Badge
            status={autoRefresh ? "processing" : "default"}
            text={autoRefresh ? "Live" : "Paused"}
          />
        )}

        <Tooltip title="Filters">
          <Button
            icon={<FilterOutlined />}
            onClick={() => setShowFilters(!showFilters)}
            type={showFilters ? "primary" : "default"}
          />
        </Tooltip>

        <Tooltip title="Settings">
          <Button
            icon={<SettingOutlined />}
            onClick={() => setShowSettings(true)}
          />
        </Tooltip>

        <Tooltip title="Refresh">
          <Button
            icon={<ReloadOutlined />}
            onClick={onRefresh}
            loading={loading}
          />
        </Tooltip>

        {config.collaboration?.allowSharing && (
          <Tooltip title="Share">
            <Button
              icon={<ShareAltOutlined />}
              onClick={onShare}
            />
          </Tooltip>
        )}

        <Tooltip title="Export">
          <Button
            icon={<DownloadOutlined />}
            onClick={() => onExport?.('pdf')}
          />
        </Tooltip>

        <Tooltip title="Fullscreen">
          <Button
            icon={<FullscreenOutlined />}
            onClick={() => setIsFullscreen(!isFullscreen)}
          />
        </Tooltip>
      </Space>
    </div>
  );

  const renderDashboardStats = () => (
    <Row gutter={16} style={{ marginBottom: 16 }}>
      <Col span={6}>
        <Card size="small">
          <div style={{ textAlign: 'center' }}>
            <Title level={4} style={{ margin: 0, color: '#1890ff' }}>
              {config.charts.length}
            </Title>
            <Text type="secondary">Charts</Text>
          </div>
        </Card>
      </Col>
      <Col span={6}>
        <Card size="small">
          <div style={{ textAlign: 'center' }}>
            <Title level={4} style={{ margin: 0, color: '#52c41a' }}>
              {filteredData.length}
            </Title>
            <Text type="secondary">Data Points</Text>
          </div>
        </Card>
      </Col>
      <Col span={6}>
        <Card size="small">
          <div style={{ textAlign: 'center' }}>
            <Title level={4} style={{ margin: 0, color: '#faad14' }}>
              {generationMetrics?.complexityScore.toFixed(1) || 'N/A'}
            </Title>
            <Text type="secondary">Complexity</Text>
          </div>
        </Card>
      </Col>
      <Col span={6}>
        <Card size="small">
          <div style={{ textAlign: 'center' }}>
            <Title level={4} style={{ margin: 0, color: '#722ed1' }}>
              {lastUpdated.format('HH:mm')}
            </Title>
            <Text type="secondary">Last Updated</Text>
          </div>
        </Card>
      </Col>
    </Row>
  );

  const renderSettingsDrawer = () => (
    <Drawer
      title="Dashboard Settings"
      placement="right"
      onClose={() => setShowSettings(false)}
      open={showSettings}
      width={400}
    >
      <Space direction="vertical" style={{ width: '100%' }}>
        <div>
          <Text strong>Real-time Updates</Text>
          <div style={{ marginTop: 8 }}>
            <Switch
              checked={autoRefresh}
              onChange={setAutoRefresh}
              disabled={!config.realTime?.enabled}
            />
            <Text style={{ marginLeft: 8 }}>
              {autoRefresh ? 'Enabled' : 'Disabled'}
            </Text>
          </div>
        </div>

        {config.realTime?.enabled && (
          <div>
            <Text strong>Refresh Interval (seconds)</Text>
            <Slider
              min={10}
              max={300}
              value={refreshInterval}
              onChange={setRefreshInterval}
              marks={{
                10: '10s',
                30: '30s',
                60: '1m',
                300: '5m'
              }}
            />
          </div>
        )}

        <Divider />

        <div>
          <Text strong>Theme</Text>
          <Select
            style={{ width: '100%', marginTop: 8 }}
            defaultValue={config.theme?.name || 'default'}
            disabled
          >
            <Option value="default">Default</Option>
            <Option value="dark">Dark</Option>
            <Option value="light">Light</Option>
          </Select>
        </div>

        <div>
          <Text strong>Layout</Text>
          <Select
            style={{ width: '100%', marginTop: 8 }}
            defaultValue={config.layout.type}
            disabled
          >
            <Option value="grid">Grid</Option>
            <Option value="masonry">Masonry</Option>
            <Option value="responsive">Responsive</Option>
          </Select>
        </div>

        {generationMetrics && (
          <>
            <Divider />
            <div>
              <Text strong>Generation Metrics</Text>
              <div style={{ marginTop: 8 }}>
                <Text type="secondary">
                  Generation Time: {generationMetrics.generationTime}
                </Text>
                <br />
                <Text type="secondary">
                  Confidence: {(generationMetrics.recommendationConfidence * 100).toFixed(1)}%
                </Text>
              </div>
            </div>
          </>
        )}
      </Space>
    </Drawer>
  );

  return (
    <div className={`advanced-dashboard ${isFullscreen ? 'fullscreen' : ''}`}>
      <Spin spinning={loading}>
        {renderDashboardHeader()}

        {renderDashboardStats()}

        {showFilters && config.globalFilters.length > 0 && renderGlobalFilters()}

        <Row gutter={[16, 16]}>
          {config.charts.map((chart, index) => (
            <Col
              key={index}
              span={getChartSize(index)}
              style={{ minHeight: 400 }}
            >
              <AdvancedChart
                config={chart}
                data={filteredData}
                loading={loading}
                onExport={(format) => onExport?.(format)}
              />
            </Col>
          ))}
        </Row>

        {/* Dashboard Footer */}
        <Card size="small" style={{ marginTop: 16 }}>
          <Row justify="space-between" align="middle">
            <Col>
              <Space>
                <EyeOutlined />
                <Text type="secondary">
                  Layout: {config.layout.type} |
                  Charts: {config.charts.length} |
                  Data: {filteredData.length} rows
                  {Object.keys(globalFilters).length > 0 && ' (filtered)'}
                </Text>
              </Space>
            </Col>
            <Col>
              <Space>
                <ClockCircleOutlined />
                <Text type="secondary">
                  Last updated: {lastUpdated.format('YYYY-MM-DD HH:mm:ss')}
                </Text>
              </Space>
            </Col>
          </Row>
        </Card>

        {renderSettingsDrawer()}
      </Spin>
    </div>
  );
};

export default AdvancedDashboard;
