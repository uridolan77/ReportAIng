import React, { useState, useEffect } from 'react';
import { Button, Typography, Spin, Tag, Card, Space } from 'antd';
import {
  ThunderboltOutlined,
  StarOutlined,
  RocketOutlined
} from '@ant-design/icons';
import { querySuggestionService, GroupedSuggestions, QuerySuggestion } from '../../services/querySuggestionService';

const { Text, Title } = Typography;

interface ProactiveSuggestionsProps {
  onQuerySelect: (query: string) => void;
  onStartWizard?: () => void;
  userRole?: string;
  recentQueries?: string[];
}

export const ProactiveSuggestions: React.FC<ProactiveSuggestionsProps> = ({
  onQuerySelect
}) => {
  const [groupedSuggestions, setGroupedSuggestions] = useState<GroupedSuggestions[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadSuggestions();
  }, []);

  // Fallback suggestions when API fails
  const getFallbackSuggestions = (): GroupedSuggestions[] => [
    {
      category: {
        id: 1,
        categoryKey: 'financial',
        title: '💰 Financial & Revenue',
        icon: '💰',
        description: 'Revenue, deposits, withdrawals, and financial performance metrics',
        sortOrder: 1,
        isActive: true,
        suggestionCount: 5
      },
      suggestions: [
        {
          id: 1,
          categoryId: 1,
          categoryKey: 'financial',
          categoryTitle: '💰 Financial & Revenue',
          queryText: 'Show me total deposits for yesterday',
          description: 'Daily deposit performance tracking',
          defaultTimeFrame: 'yesterday',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions'],
          complexity: 1,
          requiredPermissions: [],
          tags: ['deposits', 'daily', 'financial'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 2,
          categoryId: 1,
          categoryKey: 'financial',
          categoryTitle: '💰 Financial & Revenue',
          queryText: 'Revenue breakdown by country for last week',
          description: 'Geographic revenue analysis and performance',
          defaultTimeFrame: 'last_7_days',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players', 'tbl_Countries'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['revenue', 'geography', 'countries'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 3,
          categoryId: 1,
          categoryKey: 'financial',
          categoryTitle: '💰 Financial & Revenue',
          queryText: 'Net gaming revenue trends last 30 days',
          description: 'Revenue trend analysis and performance tracking',
          defaultTimeFrame: 'last_30_days',
          sortOrder: 3,
          isActive: true,
          targetTables: ['tbl_Daily_actions'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['ngr', 'trends', 'performance'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 2,
        categoryKey: 'players',
        title: '👥 Player Analytics',
        icon: '👥',
        description: 'Player behavior, demographics, and lifecycle analysis',
        sortOrder: 2,
        isActive: true,
        suggestionCount: 5
      },
      suggestions: [
        {
          id: 4,
          categoryId: 2,
          categoryKey: 'players',
          categoryTitle: '👥 Player Analytics',
          queryText: 'Top 10 players by deposits in the last 7 days',
          description: 'High-value player identification and analysis',
          defaultTimeFrame: 'last_7_days',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['high_value', 'players', 'deposits', 'top_performers'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 5,
          categoryId: 2,
          categoryKey: 'players',
          categoryTitle: '👥 Player Analytics',
          queryText: 'New player registrations by country this week',
          description: 'Player acquisition analysis by geography',
          defaultTimeFrame: 'this_week',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players', 'tbl_Countries'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['registrations', 'acquisition', 'geography'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 6,
          categoryId: 2,
          categoryKey: 'players',
          categoryTitle: '👥 Player Analytics',
          queryText: 'Active players by white label brand yesterday',
          description: 'Brand performance comparison and analysis',
          defaultTimeFrame: 'yesterday',
          sortOrder: 3,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_White_labels'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['active_players', 'brands', 'performance'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 3,
        categoryKey: 'gaming',
        title: '🎮 Gaming & Products',
        icon: '🎮',
        description: 'Game performance, provider analysis, and product metrics',
        sortOrder: 3,
        isActive: true,
        suggestionCount: 5
      },
      suggestions: [
        {
          id: 7,
          categoryId: 3,
          categoryKey: 'gaming',
          categoryTitle: '🎮 Gaming & Products',
          queryText: 'Top performing games by net gaming revenue this month',
          description: 'Game performance ranking and analysis',
          defaultTimeFrame: 'this_month',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions_games', 'Games'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['games', 'performance', 'ngr'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 8,
          categoryId: 3,
          categoryKey: 'gaming',
          categoryTitle: '🎮 Gaming & Products',
          queryText: 'Slot vs table games revenue comparison this week',
          description: 'Game type performance analysis',
          defaultTimeFrame: 'this_week',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions_games', 'Games'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['game_types', 'slots', 'tables', 'comparison'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 9,
          categoryId: 3,
          categoryKey: 'gaming',
          categoryTitle: '🎮 Gaming & Products',
          queryText: 'Casino vs sports betting revenue split this month',
          description: 'Product vertical comparison and analysis',
          defaultTimeFrame: 'this_month',
          sortOrder: 3,
          isActive: true,
          targetTables: ['tbl_Daily_actions'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['casino', 'sports', 'verticals', 'comparison'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 4,
        categoryKey: 'transactions',
        title: '💳 Transactions & Payments',
        icon: '💳',
        description: 'Payment methods, transaction analysis, and processing metrics',
        sortOrder: 4,
        isActive: true,
        suggestionCount: 3
      },
      suggestions: [
        {
          id: 10,
          categoryId: 4,
          categoryKey: 'transactions',
          categoryTitle: '💳 Transactions & Payments',
          queryText: 'Transaction volumes by payment method today',
          description: 'Payment method usage analysis',
          defaultTimeFrame: 'today',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions_transactions'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['transactions', 'payment_methods', 'volumes'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 11,
          categoryId: 4,
          categoryKey: 'transactions',
          categoryTitle: '💳 Transactions & Payments',
          queryText: 'Failed vs successful transactions by payment type this week',
          description: 'Payment success rate analysis',
          defaultTimeFrame: 'this_week',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions_transactions'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['transaction_status', 'success_rates', 'payment_types'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 12,
          categoryId: 4,
          categoryKey: 'transactions',
          categoryTitle: '💳 Transactions & Payments',
          queryText: 'Average transaction amount by payment method this month',
          description: 'Payment method value insights',
          defaultTimeFrame: 'this_month',
          sortOrder: 3,
          isActive: true,
          targetTables: ['tbl_Daily_actions_transactions'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['transaction_amounts', 'payment_methods', 'averages'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    }
  ];

  const loadSuggestions = async () => {
    try {
      setLoading(true);
      const suggestions = await querySuggestionService.getGroupedSuggestions();

      if (suggestions && suggestions.length > 0) {
        // Randomly select 2-3 suggestions from each category
        const randomizedSuggestions = suggestions.map(group => ({
          ...group,
          suggestions: getRandomSuggestions(group.suggestions, Math.min(3, group.suggestions.length))
        }));
        setGroupedSuggestions(randomizedSuggestions);
      } else {
        // Use fallback data if no suggestions returned
        setGroupedSuggestions(getFallbackSuggestions());
      }
    } catch (err) {
      console.error('Failed to load suggestions:', err);
      // Use fallback data instead of showing error
      setGroupedSuggestions(getFallbackSuggestions());
    } finally {
      setLoading(false);
    }
  };

  // Helper function to get random suggestions
  const getRandomSuggestions = (suggestions: QuerySuggestion[], count: number): QuerySuggestion[] => {
    const shuffled = [...suggestions].sort(() => 0.5 - Math.random());
    return shuffled.slice(0, count);
  };

  const handleQuerySelect = async (suggestion: QuerySuggestion) => {
    try {
      // Record usage analytics
      await querySuggestionService.recordUsage({
        suggestionId: suggestion.id,
        sessionId: sessionStorage.getItem('sessionId') || undefined,
        timeFrameUsed: suggestion.defaultTimeFrame,
        wasSuccessful: true
      });
      
      // Apply time frame to query if specified
      let queryText = suggestion.queryText;
      if (suggestion.defaultTimeFrame && suggestion.defaultTimeFrame !== 'all_time') {
        const timeFrameDisplay = querySuggestionService.formatTimeFrame(suggestion.defaultTimeFrame);
        if (!queryText.toLowerCase().includes(timeFrameDisplay.toLowerCase())) {
          queryText = `${queryText} (${timeFrameDisplay})`;
        }
      }
      
      onQuerySelect(queryText);
    } catch (err) {
      console.error('Failed to record usage:', err);
      // Still execute the query even if analytics fail
      onQuerySelect(suggestion.queryText);
    }
  };

  const getComplexityIcon = (complexity: number) => {
    switch (complexity) {
      case 1: return <StarOutlined style={{ color: '#52c41a' }} />;
      case 2: return <ThunderboltOutlined style={{ color: '#faad14' }} />;
      case 3: return <RocketOutlined style={{ color: '#f5222d' }} />;
      default: return <StarOutlined style={{ color: '#d9d9d9' }} />;
    }
  };



  if (loading) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        padding: '40px 0' 
      }}>
        <Spin size="large" />
        <Text style={{ marginLeft: '12px' }}>Loading suggestions...</Text>
      </div>
    );
  }



  if (groupedSuggestions.length === 0) {
    return (
      <div style={{ 
        textAlign: 'center', 
        padding: '40px 0' 
      }}>
        <Text type="secondary">No suggestions available</Text>
      </div>
    );
  }

  return (
    <div style={{
      padding: '0 24px 24px 24px',
      maxWidth: '1200px',
      margin: '0 auto'
    }}>
      {/* Header */}
      <div style={{
        textAlign: 'center',
        marginBottom: '32px'
      }}>
        <Title level={3} style={{
          margin: 0,
          color: '#1f2937',
          fontWeight: 600
        }}>
          ⚡ Try These Examples
        </Title>
        <Text type="secondary" style={{ fontSize: '14px' }}>
          Organized by category for easy discovery
        </Text>
      </div>

      {/* Categories Grid */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
        gap: '20px',
        alignItems: 'start',
        justifyContent: 'center'
      }}>
        {groupedSuggestions.map((group) => (
          <Card
            key={group.category.id}
            style={{
              borderRadius: '16px',
              border: '1px solid #e5e7eb',
              boxShadow: '0 2px 8px rgba(0, 0, 0, 0.06)',
              transition: 'all 0.3s ease',
              height: 'fit-content',
              background: 'linear-gradient(135deg, #ffffff 0%, #fafbfc 100%)'
            }}
            styles={{ body: { padding: '24px' } }}
            hoverable
          >
            {/* Category Header */}
            <div style={{
              marginBottom: '16px',
              textAlign: 'center'
            }}>
              <div style={{
                fontSize: '24px',
                marginBottom: '4px'
              }}>
                {group.category.icon}
              </div>
              <Title level={5} style={{
                margin: 0,
                color: '#374151',
                fontWeight: 600
              }}>
                {group.category.title}
              </Title>
              {group.category.description && (
                <Text type="secondary" style={{
                  fontSize: '12px',
                  display: 'block',
                  marginTop: '4px'
                }}>
                  {group.category.description}
                </Text>
              )}
            </div>

            {/* Suggestions */}
            <Space direction="vertical" style={{ width: '100%' }} size="small">
              {group.suggestions.map((suggestion) => (
                <Button
                  key={suggestion.id}
                  type="text"
                  onClick={() => handleQuerySelect(suggestion)}
                  style={{
                    width: '100%',
                    height: 'auto',
                    padding: '14px 16px',
                    textAlign: 'left',
                    borderRadius: '10px',
                    border: '1px solid #e5e7eb',
                    backgroundColor: '#ffffff',
                    transition: 'all 0.3s ease',
                    boxShadow: '0 1px 3px rgba(0, 0, 0, 0.05)'
                  }}
                  className="modern-suggestion-button"
                >
                  <div style={{ width: '100%' }}>
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'space-between',
                      marginBottom: '4px'
                    }}>
                      <Text strong style={{
                        fontSize: '13px',
                        color: '#374151',
                        flex: 1
                      }}>
                        {suggestion.queryText}
                      </Text>
                      <div style={{ marginLeft: '8px' }}>
                        {getComplexityIcon(suggestion.complexity)}
                      </div>
                    </div>
                    <Text type="secondary" style={{
                      fontSize: '11px',
                      lineHeight: '1.4'
                    }}>
                      {suggestion.description}
                    </Text>
                    {(suggestion.usageCount > 0 || suggestion.defaultTimeFrame) && (
                      <div style={{
                        marginTop: '6px',
                        display: 'flex',
                        gap: '4px'
                      }}>
                        {suggestion.usageCount > 0 && (
                          <Tag color="orange" style={{ fontSize: '10px', margin: 0 }}>
                            {suggestion.usageCount} uses
                          </Tag>
                        )}
                        {suggestion.defaultTimeFrame && suggestion.defaultTimeFrame !== 'all_time' && (
                          <Tag color="blue" style={{ fontSize: '10px', margin: 0 }}>
                            {querySuggestionService.formatTimeFrame(suggestion.defaultTimeFrame)}
                          </Tag>
                        )}
                      </div>
                    )}
                  </div>
                </Button>
              ))}
            </Space>
          </Card>
        ))}
      </div>
    </div>
  );
};
