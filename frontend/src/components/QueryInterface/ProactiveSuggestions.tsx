import React, { useState, useEffect } from 'react';
import { Button, Typography, Spin, Tag, Card, Space, Tooltip } from 'antd';
import {
  ThunderboltOutlined,
  StarOutlined,
  RocketOutlined,
  ReloadOutlined,
  FireOutlined
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
  const [allSuggestions, setAllSuggestions] = useState<GroupedSuggestions[]>([]);
  const [loading, setLoading] = useState(true);
  const [randomizing, setRandomizing] = useState(false);

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
        // Store all suggestions for randomization
        setAllSuggestions(suggestions);
        // Randomly select 2-3 suggestions from each category
        const randomizedSuggestions = suggestions.map(group => ({
          ...group,
          suggestions: getRandomSuggestions(group.suggestions, Math.min(3, group.suggestions.length))
        }));
        setGroupedSuggestions(randomizedSuggestions);
      } else {
        // Use fallback data if no suggestions returned
        const fallbackData = getFallbackSuggestions();
        setAllSuggestions(fallbackData);
        setGroupedSuggestions(fallbackData);
      }
    } catch (err) {
      console.error('Failed to load suggestions:', err);
      // Use fallback data instead of showing error
      const fallbackData = getFallbackSuggestions();
      setAllSuggestions(fallbackData);
      setGroupedSuggestions(fallbackData);
    } finally {
      setLoading(false);
    }
  };

  // Helper function to get random suggestions
  const getRandomSuggestions = (suggestions: QuerySuggestion[], count: number): QuerySuggestion[] => {
    const shuffled = [...suggestions].sort(() => 0.5 - Math.random());
    return shuffled.slice(0, count);
  };

  // Function to randomize displayed suggestions
  const randomizeSuggestions = async () => {
    setRandomizing(true);

    // Add a small delay for visual feedback
    await new Promise(resolve => setTimeout(resolve, 300));

    const randomizedSuggestions = allSuggestions.map(group => ({
      ...group,
      suggestions: getRandomSuggestions(group.suggestions, Math.min(3, group.suggestions.length))
    }));

    setGroupedSuggestions(randomizedSuggestions);
    setRandomizing(false);
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
      padding: '0 0 24px 0',
      width: '100%'
    }}>
      {/* Header */}
      <div style={{
        textAlign: 'center',
        marginBottom: '32px',
        position: 'relative'
      }}>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          gap: '16px',
          marginBottom: '8px'
        }}>
          <FireOutlined style={{
            fontSize: '24px',
            color: '#f59e0b',
            animation: 'pulse 2s infinite'
          }} />
          <Title level={3} style={{
            margin: 0,
            color: '#1f2937',
            fontWeight: 700,
            background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            backgroundClip: 'text'
          }}>
            Try These Examples
          </Title>
          <FireOutlined style={{
            fontSize: '24px',
            color: '#f59e0b',
            animation: 'pulse 2s infinite'
          }} />
        </div>

        <Text type="secondary" style={{
          fontSize: '14px',
          display: 'block',
          marginBottom: '16px'
        }}>
          Organized by category for easy discovery
        </Text>

        {/* Randomize Button */}
        <Tooltip title="Get new random examples from each category">
          <Button
            type="primary"
            icon={<ReloadOutlined spin={randomizing} />}
            onClick={randomizeSuggestions}
            loading={randomizing}
            style={{
              borderRadius: '20px',
              background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
              border: 'none',
              boxShadow: '0 4px 12px rgba(59, 130, 246, 0.4)',
              fontWeight: 600,
              height: '36px',
              paddingLeft: '16px',
              paddingRight: '16px'
            }}
          >
            {randomizing ? 'Shuffling...' : 'Shuffle Examples'}
          </Button>
        </Tooltip>
      </div>

      {/* Categories Grid */}
      <div
        className="suggestions-grid"
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(3, 1fr)',
          gap: '24px',
          alignItems: 'start',
          width: '100%'
        }}>
        {groupedSuggestions.map((group, index) => (
          <Card
            key={`${group.category.id}-${index}`}
            style={{
              borderRadius: '20px',
              border: '1px solid #e1e5e9',
              boxShadow: '0 8px 32px rgba(0, 0, 0, 0.08)',
              transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
              height: 'fit-content',
              background: 'linear-gradient(135deg, #ffffff 0%, #f8fafc 100%)',
              position: 'relative',
              overflow: 'hidden',
              animation: randomizing ? 'cardShuffle 0.6s ease-in-out' : 'none'
            }}
            styles={{ body: { padding: '28px' } }}
            hoverable
            className="suggestion-card"
          >
            {/* Decorative gradient overlay */}
            <div style={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              height: '4px',
              background: 'linear-gradient(90deg, #3b82f6 0%, #1d4ed8 50%, #1e40af 100%)',
              borderRadius: '20px 20px 0 0'
            }} />

            {/* Category Header */}
            <div style={{
              marginBottom: '20px',
              textAlign: 'center',
              position: 'relative'
            }}>
              <div style={{
                fontSize: '32px',
                marginBottom: '8px',
                filter: 'drop-shadow(0 2px 4px rgba(0,0,0,0.1))'
              }}>
                {group.category.icon}
              </div>
              <Title level={5} style={{
                margin: 0,
                color: '#1f2937',
                fontWeight: 700,
                fontSize: '16px',
                letterSpacing: '0.5px',
                whiteSpace: 'nowrap',
                overflow: 'visible',
                textAlign: 'center'
              }}>
                {group.category.title}
              </Title>
              {group.category.description && (
                <Text type="secondary" style={{
                  fontSize: '13px',
                  display: 'block',
                  marginTop: '6px',
                  lineHeight: '1.4',
                  color: '#6b7280'
                }}>
                  {group.category.description}
                </Text>
              )}
            </div>

            {/* Suggestions */}
            <Space direction="vertical" style={{ width: '100%' }} size={12}>
              {group.suggestions.map((suggestion, suggestionIndex) => (
                <Button
                  key={`${suggestion.id}-${suggestionIndex}`}
                  type="text"
                  onClick={() => handleQuerySelect(suggestion)}
                  style={{
                    width: '100%',
                    height: 'auto',
                    padding: '16px 18px',
                    textAlign: 'left',
                    borderRadius: '14px',
                    border: '1px solid #e2e8f0',
                    backgroundColor: '#ffffff',
                    transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.04)',
                    position: 'relative',
                    overflow: 'hidden'
                  }}
                  className="modern-suggestion-button"
                >
                  {/* Subtle hover gradient */}
                  <div style={{
                    position: 'absolute',
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0,
                    background: 'linear-gradient(135deg, rgba(102, 126, 234, 0.02) 0%, rgba(118, 75, 162, 0.02) 100%)',
                    opacity: 0,
                    transition: 'opacity 0.3s ease',
                    borderRadius: '14px',
                    pointerEvents: 'none'
                  }} className="suggestion-hover-gradient" />

                  <div style={{ width: '100%', position: 'relative', zIndex: 1 }}>
                    <div style={{
                      display: 'flex',
                      alignItems: 'flex-start',
                      justifyContent: 'space-between',
                      marginBottom: '6px'
                    }}>
                      <Text strong style={{
                        fontSize: '14px',
                        color: '#1f2937',
                        flex: 1,
                        lineHeight: '1.4',
                        fontWeight: 600,
                        wordBreak: 'break-word',
                        whiteSpace: 'normal'
                      }}>
                        {suggestion.queryText}
                      </Text>
                      <div style={{
                        marginLeft: '12px',
                        display: 'flex',
                        alignItems: 'center'
                      }}>
                        {getComplexityIcon(suggestion.complexity)}
                      </div>
                    </div>
                    <Text type="secondary" style={{
                      fontSize: '12px',
                      lineHeight: '1.5',
                      color: '#6b7280',
                      display: 'block',
                      marginBottom: '8px'
                    }}>
                      {suggestion.description}
                    </Text>
                    {(suggestion.usageCount > 0 || suggestion.defaultTimeFrame) && (
                      <div style={{
                        display: 'flex',
                        gap: '6px',
                        flexWrap: 'wrap'
                      }}>
                        {suggestion.usageCount > 0 && (
                          <Tag color="orange" style={{
                            fontSize: '10px',
                            margin: 0,
                            borderRadius: '12px',
                            padding: '2px 8px'
                          }}>
                            {suggestion.usageCount} uses
                          </Tag>
                        )}
                        {suggestion.defaultTimeFrame && suggestion.defaultTimeFrame !== 'all_time' && (
                          <Tag color="blue" style={{
                            fontSize: '10px',
                            margin: 0,
                            borderRadius: '12px',
                            padding: '2px 8px'
                          }}>
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

      {/* Add CSS animations and responsive styles */}
      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.7; }
        }

        @keyframes cardShuffle {
          0% { transform: translateY(0) scale(1); opacity: 1; }
          50% { transform: translateY(-10px) scale(0.98); opacity: 0.8; }
          100% { transform: translateY(0) scale(1); opacity: 1; }
        }

        .suggestion-card:hover {
          transform: translateY(-4px) !important;
          box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12) !important;
        }

        .modern-suggestion-button:hover {
          transform: translateY(-2px) !important;
          box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1) !important;
          border-color: #93c5fd !important;
        }

        .modern-suggestion-button:hover .suggestion-hover-gradient {
          opacity: 1 !important;
        }

        .modern-suggestion-button:active {
          transform: translateY(0) !important;
        }

        /* Responsive grid adjustments */
        @media (max-width: 1400px) {
          .suggestions-grid {
            grid-template-columns: repeat(3, 1fr) !important;
          }
        }

        @media (max-width: 1200px) {
          .suggestions-grid {
            grid-template-columns: repeat(2, 1fr) !important;
          }
        }

        @media (max-width: 768px) {
          .suggestions-grid {
            grid-template-columns: 1fr !important;
            gap: 16px !important;
          }
        }
      `}</style>
    </div>
  );
};
