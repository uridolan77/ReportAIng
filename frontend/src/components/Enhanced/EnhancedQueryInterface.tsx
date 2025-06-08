import React, { useState, useEffect, useRef } from 'react';
import { Card, Button, Input, Badge, Spin, Alert, Tabs, Progress, Typography, Space, Divider } from 'antd';
import {
  RocketOutlined,
  ThunderboltOutlined,
  BulbOutlined,
  EyeOutlined,
  HistoryOutlined,
  SettingOutlined
} from '@ant-design/icons';
import { useWebSocket } from '../../services/websocketService';

const { TextArea } = Input;
const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;

interface EnhancedQueryResult {
  sql: string;
  explanation: string;
  confidence: number;
  alternativeQueries: string[];
  semanticEntities: string[];
  classification: string;
  usedSchema: any;
  conversationContext?: any;
  decomposition?: any;
}

interface RealTimeInsight {
  queryId: string;
  executionTime: number;
  success: boolean;
  timestamp: string;
  insights: string[];
}

export const EnhancedQueryInterface: React.FC = () => {
  const [query, setQuery] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [result, setResult] = useState<EnhancedQueryResult | null>(null);
  const [realTimeInsights, setRealTimeInsights] = useState<RealTimeInsight[]>([]);
  const [conversationHistory, setConversationHistory] = useState<string[]>([]);
  const [streamingEnabled, setStreamingEnabled] = useState(false);
  
  // WebSocket connection for real-time features
  const { isConnected, sendMessage } = useWebSocket();
  const wsRef = useRef<WebSocket | null>(null);

  useEffect(() => {
    // Connect to streaming hub for real-time insights
    if (isConnected) {
      connectToStreamingHub();
    }
  }, [isConnected]);

  const connectToStreamingHub = () => {
    try {
      const wsUrl = process.env.REACT_APP_WS_URL || 'ws://localhost:55243';
      wsRef.current = new WebSocket(`${wsUrl}/hubs/streaming`);
      
      wsRef.current.onopen = () => {
        console.log('🔗 Connected to streaming hub');
        setStreamingEnabled(true);
        
        // Join user group for real-time updates
        wsRef.current?.send(JSON.stringify({
          type: 'JoinUserGroup',
          userId: 'current-user' // In real app, get from auth
        }));
      };

      wsRef.current.onmessage = (event) => {
        const data = JSON.parse(event.data);
        handleRealTimeMessage(data);
      };

      wsRef.current.onclose = () => {
        console.log('🔌 Disconnected from streaming hub');
        setStreamingEnabled(false);
      };

      wsRef.current.onerror = (error) => {
        console.error('❌ Streaming hub error:', error);
        setStreamingEnabled(false);
      };
    } catch (error) {
      console.error('❌ Failed to connect to streaming hub:', error);
    }
  };

  const handleRealTimeMessage = (data: any) => {
    switch (data.type) {
      case 'QueryInsights':
        setRealTimeInsights(prev => [data.payload, ...prev.slice(0, 9)]);
        break;
      case 'StreamingSessionStarted':
        console.log('🎬 Streaming session started:', data.payload);
        break;
      case 'RealTimeAlert':
        console.log('🚨 Real-time alert:', data.payload);
        break;
      default:
        console.log('📡 Real-time message:', data);
    }
  };

  const handleEnhancedQuery = async () => {
    if (!query.trim()) return;

    setIsProcessing(true);
    try {
      // Add to conversation history
      setConversationHistory(prev => [...prev, query]);

      // Call enhanced query processing endpoint
      const response = await fetch('/api/query/enhanced', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({
          query: query,
          enableConversationContext: true,
          enableQueryDecomposition: true,
          enableSchemaAware: true,
          conversationHistory: conversationHistory
        })
      });

      if (response.ok) {
        const enhancedResult: EnhancedQueryResult = await response.json();
        setResult(enhancedResult);

        // Send query event for real-time processing
        if (streamingEnabled && wsRef.current) {
          wsRef.current.send(JSON.stringify({
            type: 'ProcessQueryStreamEvent',
            payload: {
              queryId: Date.now().toString(),
              userId: 'current-user',
              query: query,
              generatedSQL: enhancedResult.sql,
              timestamp: new Date().toISOString(),
              executionTimeMs: 0,
              success: true
            }
          }));
        }
      } else {
        throw new Error('Enhanced query processing failed');
      }
    } catch (error) {
      console.error('❌ Enhanced query error:', error);
      // Fallback to regular query processing
      handleRegularQuery();
    } finally {
      setIsProcessing(false);
    }
  };

  const handleRegularQuery = async () => {
    try {
      const response = await fetch('/api/query/execute', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ query })
      });

      if (response.ok) {
        const result = await response.json();
        setResult({
          sql: result.sql || '',
          explanation: result.explanation || 'Query processed successfully',
          confidence: 0.8,
          alternativeQueries: [],
          semanticEntities: [],
          classification: 'standard',
          usedSchema: null
        });
      }
    } catch (error) {
      console.error('❌ Regular query error:', error);
    }
  };

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.8) return '#52c41a';
    if (confidence >= 0.6) return '#faad14';
    return '#ff4d4f';
  };

  const getConfidenceText = (confidence: number) => {
    if (confidence >= 0.8) return 'High';
    if (confidence >= 0.6) return 'Medium';
    return 'Low';
  };

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <Card>
        <div style={{ marginBottom: '24px' }}>
          <Title level={2}>
            <RocketOutlined style={{ color: '#1890ff', marginRight: '8px' }} />
            Enhanced AI Query Interface
          </Title>
          <Paragraph>
            Experience next-generation query processing with conversation context, 
            query decomposition, and real-time streaming analytics.
          </Paragraph>
          
          <Space>
            <Badge 
              status={streamingEnabled ? "processing" : "default"} 
              text={streamingEnabled ? "Real-time Streaming Active" : "Real-time Streaming Inactive"}
            />
            <Badge 
              status="success" 
              text="Enhanced AI Processing Enabled"
            />
          </Space>
        </div>

        <div style={{ marginBottom: '16px' }}>
          <TextArea
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Ask your question in natural language... (e.g., 'Show me the top 10 players by revenue this month')"
            rows={3}
            style={{ fontSize: '16px' }}
          />
        </div>

        <div style={{ marginBottom: '24px' }}>
          <Button
            type="primary"
            size="large"
            icon={<BulbOutlined />}
            onClick={handleEnhancedQuery}
            loading={isProcessing}
            disabled={!query.trim()}
            style={{ marginRight: '8px' }}
          >
            {isProcessing ? 'Processing with AI...' : 'Enhanced Query Processing'}
          </Button>
          
          <Button
            icon={<ThunderboltOutlined />}
            onClick={() => setQuery('Show me the top 10 players by total bets in the last 7 days')}
            style={{ marginRight: '8px' }}
          >
            Sample Query
          </Button>
        </div>

        {result && (
          <Tabs defaultActiveKey="1">
            <TabPane tab={<span><EyeOutlined />Results</span>} key="1">
              <Card size="small" style={{ marginBottom: '16px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Text strong>AI Confidence Score</Text>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <Progress
                      percent={Math.round(result.confidence * 100)}
                      size="small"
                      strokeColor={getConfidenceColor(result.confidence)}
                      style={{ width: '100px' }}
                    />
                    <Badge 
                      color={getConfidenceColor(result.confidence)}
                      text={getConfidenceText(result.confidence)}
                    />
                  </div>
                </div>
              </Card>

              <Card title="Generated SQL" size="small" style={{ marginBottom: '16px' }}>
                <pre style={{ 
                  background: '#f6f8fa', 
                  padding: '12px', 
                  borderRadius: '4px',
                  overflow: 'auto'
                }}>
                  {result.sql}
                </pre>
              </Card>

              <Card title="AI Explanation" size="small" style={{ marginBottom: '16px' }}>
                <Paragraph>{result.explanation}</Paragraph>
              </Card>

              {result.semanticEntities.length > 0 && (
                <Card title="Detected Entities" size="small" style={{ marginBottom: '16px' }}>
                  <Space wrap>
                    {result.semanticEntities.map((entity, index) => (
                      <Badge key={index} count={entity} style={{ backgroundColor: '#108ee9' }} />
                    ))}
                  </Space>
                </Card>
              )}

              {result.alternativeQueries.length > 0 && (
                <Card title="Alternative Queries" size="small">
                  {result.alternativeQueries.map((altQuery, index) => (
                    <div key={index} style={{ marginBottom: '8px' }}>
                      <Button 
                        type="link" 
                        size="small"
                        onClick={() => setQuery(altQuery)}
                      >
                        {altQuery}
                      </Button>
                    </div>
                  ))}
                </Card>
              )}
            </TabPane>

            <TabPane tab={<span><ThunderboltOutlined />Real-time Insights</span>} key="2">
              {realTimeInsights.length > 0 ? (
                <div>
                  {realTimeInsights.map((insight, index) => (
                    <Card key={index} size="small" style={{ marginBottom: '8px' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div>
                          <Text strong>Query {insight.queryId}</Text>
                          <br />
                          <Text type="secondary">
                            Execution: {insight.executionTime}ms | 
                            Status: {insight.success ? '✅ Success' : '❌ Failed'}
                          </Text>
                        </div>
                        <Text type="secondary">{insight.timestamp}</Text>
                      </div>
                      <div style={{ marginTop: '8px' }}>
                        {insight.insights.map((ins, i) => (
                          <Badge key={i} status="processing" text={ins} style={{ display: 'block', marginBottom: '4px' }} />
                        ))}
                      </div>
                    </Card>
                  ))}
                </div>
              ) : (
                <Alert
                  message="No real-time insights yet"
                  description="Execute queries to see real-time performance insights and recommendations."
                  type="info"
                  showIcon
                />
              )}
            </TabPane>

            <TabPane tab={<span><HistoryOutlined />Conversation</span>} key="3">
              {conversationHistory.length > 0 ? (
                <div>
                  {conversationHistory.map((historyQuery, index) => (
                    <Card key={index} size="small" style={{ marginBottom: '8px' }}>
                      <Text>{historyQuery}</Text>
                      <Button 
                        type="link" 
                        size="small" 
                        style={{ float: 'right' }}
                        onClick={() => setQuery(historyQuery)}
                      >
                        Reuse
                      </Button>
                    </Card>
                  ))}
                </div>
              ) : (
                <Alert
                  message="No conversation history"
                  description="Your query conversation will appear here to provide context for future queries."
                  type="info"
                  showIcon
                />
              )}
            </TabPane>
          </Tabs>
        )}
      </Card>
    </div>
  );
};

export default EnhancedQueryInterface;
