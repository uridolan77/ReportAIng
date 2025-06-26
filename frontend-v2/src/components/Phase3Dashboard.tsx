/**
 * Phase 3 Optimization Dashboard
 * Demonstrates integration of multi-cluster federation, AI-driven anomaly detection,
 * cost optimization, and observability-as-code features
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Progress,
  Alert,
  Table,
  Tag,
  Tabs,
  Button,
  Space,
  Typography,
  Collapse,
  Badge,
  Tooltip
} from 'antd';
import {
  CloudServerOutlined,
  DollarOutlined,
  ExclamationTriangleOutlined,
  MonitorOutlined,
  ThunderboltOutlined,
  BarChartOutlined
} from '@ant-design/icons';

import ClusterManager, { ClusterMetrics, QueryRoutingDecision } from '../shared/services/federation/ClusterManager';
import AnomalyDetector, { Anomaly, PerformanceMetrics } from '../shared/services/intelligence/AnomalyDetector';
import CostOptimizer, { CostPrediction, Budget } from '../shared/services/optimization/CostOptimizer';
import ObservabilityManager, { MonitoringData } from '../shared/services/observability/ObservabilityManager';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

interface Phase3DashboardProps {
  className?: string;
}

const Phase3Dashboard: React.FC<Phase3DashboardProps> = ({ className }) => {
  // State management
  const [clusters, setClusters] = useState<ClusterMetrics[]>([]);
  const [anomalies, setAnomalies] = useState<Anomaly[]>([]);
  const [costAnalytics, setCostAnalytics] = useState<any>(null);
  const [monitoringData, setMonitoringData] = useState<MonitoringData | null>(null);
  const [routingDecision, setRoutingDecision] = useState<QueryRoutingDecision | null>(null);
  const [budget, setBudget] = useState<Budget>({
    dailyLimit: 1000,
    monthlyLimit: 30000,
    currentDailySpend: 450,
    currentMonthlySpend: 12500,
    alertThresholds: { warning: 80, critical: 95 }
  });

  // Service instances
  const [clusterManager] = useState(() => new ClusterManager({
    healthCheckIntervalMs: 30000,
    maxRetries: 3,
    timeoutMs: 5000
  }));

  const [anomalyDetector] = useState(() => new AnomalyDetector({
    performanceThresholds: {
      responseTimeMultiplier: 2.5,
      errorRateThreshold: 0.05,
      resourceUsageThreshold: 80
    },
    behaviorThresholds: {
      sessionDurationMultiplier: 3,
      actionFrequencyMultiplier: 2,
      errorRateThreshold: 0.1
    },
    costThresholds: {
      costIncreaseThreshold: 0.2,
      efficiencyDecreaseThreshold: 0.15
    },
    baselineWindowHours: 24
  }));

  const [costOptimizer] = useState(() => new CostOptimizer({
    costPredictionWindow: 24,
    optimizationThreshold: 10,
    maxRiskLevel: 'medium',
    enablePredictiveOptimization: true
  }));

  const [observabilityManager] = useState(() => new ObservabilityManager({
    metrics: [
      {
        name: 'query_response_time',
        type: 'histogram',
        description: 'Query response time distribution',
        labels: ['cluster', 'complexity']
      }
    ],
    traces: [],
    logs: [],
    alerts: [],
    dashboards: []
  }));

  // Initialize data
  useEffect(() => {
    initializePhase3Services();
    const interval = setInterval(refreshData, 30000); // Refresh every 30 seconds
    return () => clearInterval(interval);
  }, []);

  const initializePhase3Services = async () => {
    try {
      // Register sample clusters
      await clusterManager.registerCluster({
        clusterId: 'us-east-1-prod',
        region: 'us-east-1',
        endpoint: 'https://api-us-east-1.example.com',
        capacity: {
          maxConcurrentQueries: 100,
          currentLoad: 45,
          availableResources: 55
        },
        cost: {
          hourlyRate: 50,
          currentCost: 25,
          projectedDailyCost: 600
        }
      });

      await clusterManager.registerCluster({
        clusterId: 'eu-west-1-prod',
        region: 'eu-west-1',
        endpoint: 'https://api-eu-west-1.example.com',
        capacity: {
          maxConcurrentQueries: 80,
          currentLoad: 20,
          availableResources: 60
        },
        cost: {
          hourlyRate: 45,
          currentCost: 15,
          projectedDailyCost: 540
        }
      });

      // Generate sample data
      await refreshData();
    } catch (error) {
      console.error('Failed to initialize Phase 3 services:', error);
    }
  };

  const refreshData = async () => {
    try {
      // Update clusters
      setClusters(clusterManager.getAvailableClusters());

      // Generate sample performance metrics for anomaly detection
      const sampleMetrics: PerformanceMetrics[] = Array.from({ length: 20 }, (_, i) => ({
        timestamp: new Date(Date.now() - i * 60000),
        responseTime: 200 + Math.random() * 300 + (i > 15 ? 500 : 0), // Spike in recent data
        throughput: 100 + Math.random() * 50,
        errorRate: Math.random() * 0.02 + (i > 17 ? 0.05 : 0), // Error spike
        cpuUsage: 40 + Math.random() * 30,
        memoryUsage: 50 + Math.random() * 25,
        networkLatency: 10 + Math.random() * 20,
        queryComplexity: Math.random() * 10
      }));

      // Detect anomalies
      const detectedAnomalies = anomalyDetector.detectPerformanceAnomalies(sampleMetrics);
      setAnomalies(detectedAnomalies);

      // Get cost analytics
      const analytics = costOptimizer.getCostAnalytics();
      setCostAnalytics(analytics);

      // Get monitoring data
      const monitoring = observabilityManager.getMonitoringData();
      setMonitoringData(monitoring);

      // Simulate query routing
      const routing = await clusterManager.routeQuery({
        complexity: 'medium',
        priority: 'normal',
        estimatedDuration: 300,
        maxCost: 100
      });
      setRoutingDecision(routing);

    } catch (error) {
      console.error('Failed to refresh data:', error);
    }
  };

  const handleOptimizeQuery = async () => {
    try {
      const prediction = await costOptimizer.predictQueryCost({
        queryId: 'sample-query',
        complexity: 'medium',
        estimatedTokens: 2000,
        estimatedDuration: 300,
        priority: 'normal'
      });

      console.log('Cost prediction:', prediction);
    } catch (error) {
      console.error('Failed to optimize query:', error);
    }
  };

  const renderClusterOverview = () => (
    <Row gutter={[16, 16]}>
      {clusters.map(cluster => (
        <Col xs={24} sm={12} lg={8} key={cluster.clusterId}>
          <Card
            title={
              <Space>
                <CloudServerOutlined />
                {cluster.clusterId}
                <Tag color={cluster.health.status === 'healthy' ? 'green' : 'orange'}>
                  {cluster.health.status}
                </Tag>
              </Space>
            }
            size="small"
          >
            <Row gutter={16}>
              <Col span={12}>
                <Statistic
                  title="Response Time"
                  value={cluster.health.responseTime}
                  suffix="ms"
                  valueStyle={{ fontSize: '14px' }}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="Load"
                  value={Math.round((cluster.capacity.currentLoad / cluster.capacity.maxConcurrentQueries) * 100)}
                  suffix="%"
                  valueStyle={{ fontSize: '14px' }}
                />
              </Col>
            </Row>
            <div style={{ marginTop: 8 }}>
              <Text type="secondary">Cost: ${cluster.cost.hourlyRate}/hr</Text>
            </div>
          </Card>
        </Col>
      ))}
    </Row>
  );

  const renderAnomalies = () => (
    <div>
      {anomalies.length === 0 ? (
        <Alert message="No anomalies detected" type="success" showIcon />
      ) : (
        <Space direction="vertical" style={{ width: '100%' }}>
          {anomalies.map(anomaly => (
            <Alert
              key={anomaly.id}
              message={anomaly.description}
              description={
                <div>
                  <Text>Confidence: {(anomaly.confidence * 100).toFixed(1)}%</Text>
                  <br />
                  <Text>Suggested Actions: {anomaly.suggestedActions.join(', ')}</Text>
                </div>
              }
              type={anomaly.severity === 'critical' ? 'error' : 'warning'}
              showIcon
            />
          ))}
        </Space>
      )}
    </div>
  );

  const renderCostOptimization = () => {
    const budgetStatus = costOptimizer.monitorBudget(budget);
    
    return (
      <Space direction="vertical" style={{ width: '100%' }}>
        <Row gutter={16}>
          <Col span={8}>
            <Card>
              <Statistic
                title="Daily Spend"
                value={budget.currentDailySpend}
                prefix="$"
                suffix={`/ $${budget.dailyLimit}`}
              />
              <Progress
                percent={budgetStatus.dailyUsagePercentage}
                status={budgetStatus.status === 'critical' ? 'exception' : 'normal'}
                size="small"
              />
            </Card>
          </Col>
          <Col span={8}>
            <Card>
              <Statistic
                title="Monthly Spend"
                value={budget.currentMonthlySpend}
                prefix="$"
                suffix={`/ $${budget.monthlyLimit}`}
              />
              <Progress
                percent={budgetStatus.monthlyUsagePercentage}
                status={budgetStatus.status === 'critical' ? 'exception' : 'normal'}
                size="small"
              />
            </Card>
          </Col>
          <Col span={8}>
            <Card>
              <Statistic
                title="Projected Monthly"
                value={budgetStatus.projectedMonthlySpend}
                prefix="$"
                valueStyle={{
                  color: budgetStatus.projectedMonthlySpend > budget.monthlyLimit ? '#cf1322' : '#3f8600'
                }}
              />
            </Card>
          </Col>
        </Row>

        {costAnalytics && (
          <Card title="Cost Analytics">
            <Row gutter={16}>
              <Col span={6}>
                <Statistic
                  title="Total Cost"
                  value={costAnalytics.totalCost}
                  prefix="$"
                  precision={2}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Avg Cost/Query"
                  value={costAnalytics.averageCostPerQuery}
                  prefix="$"
                  precision={4}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Trend"
                  value={costAnalytics.costTrend}
                  valueStyle={{
                    color: costAnalytics.costTrend === 'increasing' ? '#cf1322' : '#3f8600'
                  }}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Savings"
                  value={costAnalytics.savingsFromOptimizations}
                  prefix="$"
                  precision={2}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Col>
            </Row>
          </Card>
        )}

        <Button type="primary" icon={<ThunderboltOutlined />} onClick={handleOptimizeQuery}>
          Optimize Next Query
        </Button>
      </Space>
    );
  };

  const renderObservability = () => {
    const healthStatus = observabilityManager.getHealthStatus();
    
    return (
      <Space direction="vertical" style={{ width: '100%' }}>
        <Card title="System Health">
          <Row gutter={16}>
            <Col span={6}>
              <Badge
                status={healthStatus.status === 'healthy' ? 'success' : 'error'}
                text={`Overall: ${healthStatus.status}`}
              />
            </Col>
            <Col span={6}>
              <Badge
                status={healthStatus.components.metrics === 'up' ? 'success' : 'error'}
                text="Metrics"
              />
            </Col>
            <Col span={6}>
              <Badge
                status={healthStatus.components.tracing === 'up' ? 'success' : 'error'}
                text="Tracing"
              />
            </Col>
            <Col span={6}>
              <Badge
                status={healthStatus.components.logging === 'up' ? 'success' : 'error'}
                text="Logging"
              />
            </Col>
          </Row>
        </Card>

        <Row gutter={16}>
          <Col span={6}>
            <Card>
              <Statistic
                title="Metrics Collected"
                value={healthStatus.metrics.metricsCollected}
                prefix={<BarChartOutlined />}
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <Statistic
                title="Active Traces"
                value={healthStatus.metrics.activeTraces}
                prefix={<MonitorOutlined />}
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <Statistic
                title="Log Rate"
                value={healthStatus.metrics.logRate}
                suffix="/min"
              />
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <Statistic
                title="Active Alerts"
                value={healthStatus.metrics.activeAlerts}
                prefix={<ExclamationTriangleOutlined />}
                valueStyle={{
                  color: healthStatus.metrics.activeAlerts > 0 ? '#cf1322' : '#3f8600'
                }}
              />
            </Card>
          </Col>
        </Row>

        {routingDecision && (
          <Card title="Latest Query Routing">
            <Row gutter={16}>
              <Col span={8}>
                <Text strong>Selected Cluster:</Text>
                <br />
                <Tag color="blue">{routingDecision.selectedCluster}</Tag>
              </Col>
              <Col span={8}>
                <Text strong>Confidence:</Text>
                <br />
                <Progress
                  percent={Math.round(routingDecision.confidence * 100)}
                  size="small"
                  status="active"
                />
              </Col>
              <Col span={8}>
                <Text strong>Estimated Cost:</Text>
                <br />
                <Text>${routingDecision.estimatedCost.toFixed(4)}</Text>
              </Col>
            </Row>
            <div style={{ marginTop: 8 }}>
              <Text type="secondary">{routingDecision.reason}</Text>
            </div>
          </Card>
        )}
      </Space>
    );
  };

  return (
    <div className={className}>
      <Title level={2}>
        <ThunderboltOutlined /> Phase 3 Optimization Dashboard
      </Title>
      
      <Tabs defaultActiveKey="clusters" size="large">
        <TabPane
          tab={
            <span>
              <CloudServerOutlined />
              Multi-Cluster Federation
            </span>
          }
          key="clusters"
        >
          {renderClusterOverview()}
        </TabPane>

        <TabPane
          tab={
            <span>
              <ExclamationTriangleOutlined />
              AI Anomaly Detection
              {anomalies.length > 0 && <Badge count={anomalies.length} style={{ marginLeft: 8 }} />}
            </span>
          }
          key="anomalies"
        >
          {renderAnomalies()}
        </TabPane>

        <TabPane
          tab={
            <span>
              <DollarOutlined />
              Cost Optimization
            </span>
          }
          key="cost"
        >
          {renderCostOptimization()}
        </TabPane>

        <TabPane
          tab={
            <span>
              <MonitorOutlined />
              Observability
            </span>
          }
          key="observability"
        >
          {renderObservability()}
        </TabPane>
      </Tabs>
    </div>
  );
};

export default Phase3Dashboard;
