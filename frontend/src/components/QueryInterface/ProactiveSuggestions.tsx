import React, { useState, useEffect } from 'react';
import { Button, Typography, Spin, Tag, Card, Space, Tooltip, Collapse } from 'antd';
import {
  ThunderboltOutlined,
  StarOutlined,
  RocketOutlined,
  ReloadOutlined,
  FireOutlined,
  SendOutlined,
  DownOutlined,
  UpOutlined
} from '@ant-design/icons';
import { querySuggestionService, GroupedSuggestions, QuerySuggestion } from '../../services/querySuggestionService';

const { Text, Title } = Typography;

interface ProactiveSuggestionsProps {
  onQuerySelect: (query: string) => void;
  onSubmitQuery?: (query: string) => void;
  onStartWizard?: () => void;
  userRole?: string;
  recentQueries?: string[];
}

export const ProactiveSuggestions: React.FC<ProactiveSuggestionsProps> = ({
  onQuerySelect,
  onSubmitQuery
}) => {
  const [groupedSuggestions, setGroupedSuggestions] = useState<GroupedSuggestions[]>([]);
  const [allSuggestions, setAllSuggestions] = useState<GroupedSuggestions[]>([]);
  const [loading, setLoading] = useState(true);
  const [randomizing, setRandomizing] = useState(false);
  const [hoveredCategory, setHoveredCategory] = useState<string | null>(null);
  const [shufflingCategory, setShufflingCategory] = useState<string | null>(null);
  const [isCollapsed, setIsCollapsed] = useState(true); // Collapsed by default

  useEffect(() => {
    loadSuggestions();
  }, []);

  // Fallback suggestions when API fails
  const getFallbackSuggestions = (): GroupedSuggestions[] => [
    {
      category: {
        id: 1,
        categoryKey: 'financial',
        title: 'Financial & Revenue',
        icon: '💎',
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
          categoryTitle: 'Financial & Revenue',
          queryText: 'total deposits for yesterday',
          description: 'Daily deposit performance tracking with mock data available',
          defaultTimeFrame: 'yesterday',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions'],
          complexity: 1,
          requiredPermissions: [],
          tags: ['deposits', 'daily', 'financial', 'mock-data'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 2,
          categoryId: 1,
          categoryKey: 'financial',
          categoryTitle: 'Financial & Revenue',
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
          categoryTitle: 'Financial & Revenue',
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
        title: 'Player Analytics',
        icon: '🎯',
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
          categoryTitle: 'Player Analytics',
          queryText: 'top 10 players by deposits',
          description: 'High-value player identification with mock data available',
          defaultTimeFrame: 'last_7_days',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['high_value', 'players', 'deposits', 'top_performers', 'mock-data'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 5,
          categoryId: 2,
          categoryKey: 'players',
          categoryTitle: 'Player Analytics',
          queryText: 'new player registrations',
          description: 'Player acquisition analysis with mock data available',
          defaultTimeFrame: 'this_week',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players', 'tbl_Countries'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['registrations', 'acquisition', 'geography', 'mock-data'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 6,
          categoryId: 2,
          categoryKey: 'players',
          categoryTitle: 'Player Analytics',
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
        title: 'Gaming & Products',
        icon: '🎲',
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
          categoryTitle: 'Gaming & Products',
          queryText: 'top performing games',
          description: 'Game performance ranking with mock data available',
          defaultTimeFrame: 'this_month',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions_games', 'Games'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['games', 'performance', 'ngr', 'mock-data'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 8,
          categoryId: 3,
          categoryKey: 'gaming',
          categoryTitle: 'Gaming & Products',
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
          categoryTitle: 'Gaming & Products',
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
        title: 'Transactions & Payments',
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
          categoryTitle: 'Transactions & Payments',
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
          categoryTitle: 'Transactions & Payments',
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
          categoryTitle: 'Transactions & Payments',
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
    },
    {
      category: {
        id: 5,
        categoryKey: 'demographics',
        title: 'Demographics & Behavior',
        icon: '🌍',
        description: 'Player demographics, behavior patterns, and segmentation analysis',
        sortOrder: 5,
        isActive: true,
        suggestionCount: 3
      },
      suggestions: [
        {
          id: 13,
          categoryId: 5,
          categoryKey: 'demographics',
          categoryTitle: 'Demographics & Behavior',
          queryText: 'Player age distribution by country this month',
          description: 'Demographic analysis by geography',
          defaultTimeFrame: 'this_month',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions_players', 'tbl_Countries'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['demographics', 'age', 'geography'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 14,
          categoryId: 5,
          categoryKey: 'demographics',
          categoryTitle: 'Demographics & Behavior',
          queryText: 'Player session duration patterns this week',
          description: 'Behavioral pattern analysis',
          defaultTimeFrame: 'this_week',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions_players'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['behavior', 'sessions', 'patterns'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 6,
        categoryKey: 'account',
        title: 'Account & Status',
        icon: '🔐',
        description: 'Account management, player status, and lifecycle tracking',
        sortOrder: 6,
        isActive: true,
        suggestionCount: 3
      },
      suggestions: [
        {
          id: 15,
          categoryId: 6,
          categoryKey: 'account',
          categoryTitle: 'Account & Status',
          queryText: 'New player conversion rates by registration channel',
          description: 'Account lifecycle and conversion analysis',
          defaultTimeFrame: 'this_month',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions_players'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['conversion', 'registration', 'lifecycle'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 16,
          categoryId: 6,
          categoryKey: 'account',
          categoryTitle: 'Account & Status',
          queryText: 'Player verification status breakdown today',
          description: 'Account verification and compliance tracking',
          defaultTimeFrame: 'today',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions_players'],
          complexity: 1,
          requiredPermissions: [],
          tags: ['verification', 'compliance', 'status'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 7,
        categoryKey: 'bonus',
        title: 'Bonus & Promotions',
        icon: '🎁',
        description: 'Bonus campaigns, promotional effectiveness, and reward analysis',
        sortOrder: 7,
        isActive: true,
        suggestionCount: 3
      },
      suggestions: [
        {
          id: 17,
          categoryId: 7,
          categoryKey: 'bonus',
          categoryTitle: 'Bonus & Promotions',
          queryText: 'Bonus utilization rates by promotion type this week',
          description: 'Promotional campaign effectiveness analysis',
          defaultTimeFrame: 'this_week',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions_bonuses'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['bonuses', 'promotions', 'utilization'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 18,
          categoryId: 7,
          categoryKey: 'bonus',
          categoryTitle: 'Bonus & Promotions',
          queryText: 'Top performing bonus campaigns by revenue impact',
          description: 'Bonus campaign ROI and performance tracking',
          defaultTimeFrame: 'this_month',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions_bonuses', 'tbl_Daily_actions'],
          complexity: 3,
          requiredPermissions: [],
          tags: ['campaigns', 'roi', 'performance'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 8,
        categoryKey: 'compliance',
        title: 'Compliance & Risk',
        icon: '⚖️',
        description: 'Risk management, compliance monitoring, and regulatory reporting',
        sortOrder: 8,
        isActive: true,
        suggestionCount: 3
      },
      suggestions: [
        {
          id: 19,
          categoryId: 8,
          categoryKey: 'compliance',
          categoryTitle: 'Compliance & Risk',
          queryText: 'High-risk transaction patterns this week',
          description: 'Risk assessment and fraud detection analysis',
          defaultTimeFrame: 'this_week',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions_transactions'],
          complexity: 3,
          requiredPermissions: ['risk_analysis'],
          tags: ['risk', 'fraud', 'compliance'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 20,
          categoryId: 8,
          categoryKey: 'compliance',
          categoryTitle: 'Compliance & Risk',
          queryText: 'Regulatory reporting metrics for last month',
          description: 'Compliance reporting and regulatory metrics',
          defaultTimeFrame: 'last_month',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players'],
          complexity: 2,
          requiredPermissions: ['compliance_reports'],
          tags: ['regulatory', 'reporting', 'compliance'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 9,
        categoryKey: 'business',
        title: 'Business Intelligence',
        icon: '📈',
        description: 'Strategic insights, KPI tracking, and business performance analysis',
        sortOrder: 9,
        isActive: true,
        suggestionCount: 3
      },
      suggestions: [
        {
          id: 21,
          categoryId: 9,
          categoryKey: 'business',
          categoryTitle: 'Business Intelligence',
          queryText: 'Key performance indicators dashboard for this quarter',
          description: 'Strategic KPI tracking and business metrics',
          defaultTimeFrame: 'this_quarter',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions', 'tbl_Daily_actions_players'],
          complexity: 3,
          requiredPermissions: [],
          tags: ['kpi', 'dashboard', 'strategic'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 22,
          categoryId: 9,
          categoryKey: 'business',
          categoryTitle: 'Business Intelligence',
          queryText: 'Market share analysis by product vertical',
          description: 'Competitive analysis and market positioning',
          defaultTimeFrame: 'this_month',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions'],
          complexity: 3,
          requiredPermissions: [],
          tags: ['market_share', 'competitive', 'analysis'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        }
      ]
    },
    {
      category: {
        id: 10,
        categoryKey: 'operations',
        title: 'Operations & Trends',
        icon: '⚙️',
        description: 'Operational metrics, trends, and performance monitoring',
        sortOrder: 10,
        isActive: true,
        suggestionCount: 3
      },
      suggestions: [
        {
          id: 23,
          categoryId: 10,
          categoryKey: 'operations',
          categoryTitle: 'Operations & Trends',
          queryText: 'Hourly revenue patterns for weekends this month',
          description: 'Peak activity analysis and operational insights',
          defaultTimeFrame: 'this_month',
          sortOrder: 1,
          isActive: true,
          targetTables: ['tbl_Daily_actions'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['hourly', 'patterns', 'operations'],
          usageCount: 0,
          createdDate: new Date().toISOString(),
          createdBy: 'System'
        },
        {
          id: 24,
          categoryId: 10,
          categoryKey: 'operations',
          categoryTitle: 'Operations & Trends',
          queryText: 'System performance metrics and uptime this week',
          description: 'Operational health and system monitoring',
          defaultTimeFrame: 'this_week',
          sortOrder: 2,
          isActive: true,
          targetTables: ['tbl_Daily_actions'],
          complexity: 2,
          requiredPermissions: [],
          tags: ['performance', 'uptime', 'monitoring'],
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
        // Fix any missing or invalid icons with fallback icons
        const fixedSuggestions = suggestions.map(group => {
          let icon = group.category.icon;

          // Map text-based icon names to actual emojis (based on database values)
          const textToEmojiMap: Record<string, string> = {
            'Dollar': '💎',      // Financial & Revenue
            'Target': '🎯',      // Player Analytics
            'Game': '🎲',        // Gaming & Products
            'Credit': '💳',      // Transactions & Payments
            'Global': '🌍',      // Demographics & Behavior
            'Lock': '🔐',        // Account & Status
            'Gift': '🎁',        // Bonus & Promotions
            'Shield': '🛡️',      // Compliance & Risk
            'Chart': '📊',       // Business Intelligence
            'Setting': '⚙️',     // Operations & Trends
            // Additional common variations
            'Gamepad2': '🎲',
            'CreditCard': '💳',
            'Globe': '🌍',
            'Scale': '⚖️',
            'BarChart': '📊',
            'Settings': '⚙️',
            'TrendingUp': '📈',
            'Users': '👥',
            'Activity': '📊',
            'Calendar': '📅',
            'Clock': '⏰',
            'Database': '🗄️',
            'FileText': '📄',
            'PieChart': '📊',
            'LineChart': '📈'
          };

          // First, try to map text-based icon names to emojis
          if (icon && textToEmojiMap[icon]) {
            icon = textToEmojiMap[icon];
          }
          // If still no valid emoji icon, map by category key
          else if (!icon || icon === '??' || icon === '?' || icon.trim() === '' || !icon.match(/[\u{1F600}-\u{1F64F}]|[\u{1F300}-\u{1F5FF}]|[\u{1F680}-\u{1F6FF}]|[\u{1F1E0}-\u{1F1FF}]|[\u{2600}-\u{26FF}]|[\u{2700}-\u{27BF}]/u)) {
            // Map category keys to appropriate icons
            const categoryIconMap: Record<string, string> = {
              'financial': '💎',        // Financial & Revenue - Diamond for premium/value
              'players': '🎯',         // Player Analytics - Target for precision analytics
              'gaming': '🎲',          // Gaming & Products - Dice for gaming
              'transactions': '💳',     // Transactions & Payments - Credit card
              'demographics': '🌍',     // Demographics & Behavior - Globe for global demographics
              'account': '🔐',         // Account & Status - Lock for security/accounts
              'bonus': '🎁',          // Bonus & Promotions - Gift box
              'compliance': '⚖️',      // Compliance & Risk - Scales of justice
              'business': '📊',        // Business Intelligence - Chart for BI
              'operations': '⚙️',      // Operations & Trends - Gear for operations
              'revenue': '💎',
              'analytics': '🎯',
              'payments': '💳',
              'behavior': '🌍',
              'status': '🔐',
              'promotions': '🎁',
              'risk': '⚖️',
              'intelligence': '📊',
              'trends': '⚙️'
            };

            icon = categoryIconMap[group.category.categoryKey] || categoryIconMap[group.category.categoryKey.toLowerCase()] || '📊';
          }

          return {
            ...group,
            category: {
              ...group.category,
              icon
            }
          };
        });

        // Store all suggestions for randomization
        setAllSuggestions(fixedSuggestions);
        // Randomly select 2 suggestions from each category
        const randomizedSuggestions = fixedSuggestions.map(group => ({
          ...group,
          suggestions: getRandomSuggestions(group.suggestions, Math.min(2, group.suggestions.length))
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
      suggestions: getRandomSuggestions(group.suggestions, Math.min(2, group.suggestions.length))
    }));

    setGroupedSuggestions(randomizedSuggestions);
    setRandomizing(false);
  };

  const shuffleCategory = async (categoryKey: string) => {
    if (shufflingCategory === categoryKey) return;

    setShufflingCategory(categoryKey);

    // Add a small delay for visual feedback
    await new Promise(resolve => setTimeout(resolve, 200));

    try {
      const categoryData = allSuggestions.find(cat => cat.category.categoryKey === categoryKey);
      if (categoryData) {
        const newSuggestions = getRandomSuggestions(categoryData.suggestions, Math.min(2, categoryData.suggestions.length));
        setGroupedSuggestions(prev =>
          prev.map(cat =>
            cat.category.categoryKey === categoryKey
              ? { ...cat, suggestions: newSuggestions }
              : cat
          )
        );
      }
    } catch (error) {
      console.error('Error shuffling category:', error);
    } finally {
      setShufflingCategory(null);
    }
  };

  const handleQuerySelect = (suggestion: QuerySuggestion) => {
    // Apply time frame to query if specified
    let queryText = suggestion.queryText;
    if (suggestion.defaultTimeFrame && suggestion.defaultTimeFrame !== 'all_time') {
      const timeFrameDisplay = querySuggestionService.formatTimeFrame(suggestion.defaultTimeFrame);
      if (!queryText.toLowerCase().includes(timeFrameDisplay.toLowerCase())) {
        queryText = `${queryText} (${timeFrameDisplay})`;
      }
    }

    // Update UI immediately for better UX
    onQuerySelect(queryText);

    // Record usage analytics in background (don't await)
    querySuggestionService.recordUsage({
      suggestionId: suggestion.id,
      sessionId: sessionStorage.getItem('sessionId') || undefined,
      timeFrameUsed: suggestion.defaultTimeFrame,
      wasSuccessful: true
    }).catch(err => {
      console.error('Failed to record usage analytics:', err);
      // Analytics failure shouldn't affect user experience
    });
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
      {/* Header with Toggle - Enhanced Design */}
      <div style={{
        textAlign: 'center',
        marginBottom: isCollapsed ? '24px' : '40px',
        position: 'relative'
      }}>
        {/* Main Toggle Button */}
        <div
          onClick={() => setIsCollapsed(!isCollapsed)}
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '12px',
            padding: '16px 32px',
            background: 'linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%)',
            borderRadius: '16px',
            border: '1px solid rgba(255, 255, 255, 0.8)',
            boxShadow: '0 8px 24px rgba(102, 126, 234, 0.12)',
            cursor: 'pointer',
            transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
            position: 'relative',
            overflow: 'hidden'
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-2px)';
            e.currentTarget.style.boxShadow = '0 12px 32px rgba(102, 126, 234, 0.18)';
            e.currentTarget.style.background = 'linear-gradient(135deg, rgba(102, 126, 234, 0.12) 0%, rgba(118, 75, 162, 0.12) 100%)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 8px 24px rgba(102, 126, 234, 0.12)';
            e.currentTarget.style.background = 'linear-gradient(135deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%)';
          }}
        >
          <FireOutlined style={{
            fontSize: '20px',
            color: '#f59e0b',
            animation: 'pulse 2s infinite'
          }} />
          <Title level={3} style={{
            margin: 0,
            color: '#1f2937',
            fontWeight: 700,
            fontSize: '20px',
            background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            backgroundClip: 'text'
          }}>
            Try These Examples
          </Title>

          {/* Dropdown Arrow */}
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            marginLeft: '8px'
          }}>
            {isCollapsed ? <DownOutlined style={{ fontSize: '16px', color: '#6b7280' }} /> : <UpOutlined style={{ fontSize: '16px', color: '#6b7280' }} />}
          </div>
        </div>

        {/* Secondary Controls - Only show when expanded */}
        {!isCollapsed && (
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            gap: '16px',
            marginTop: '16px'
          }}>
            <Tooltip title="Shuffle all examples">
              <Button
                type="text"
                size="small"
                icon={<ReloadOutlined spin={randomizing} />}
                onClick={randomizeSuggestions}
                loading={randomizing}
                style={{
                  borderRadius: '50%',
                  width: '32px',
                  height: '32px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  border: '1px solid #e2e8f0',
                  backgroundColor: '#ffffff',
                  boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
                  transition: 'all 0.3s ease'
                }}
              />
            </Tooltip>
          </div>
        )}

        {!isCollapsed && (
          <Text type="secondary" style={{
            fontSize: '14px',
            display: 'block',
            marginTop: '16px',
            marginBottom: '16px',
            textAlign: 'center'
          }}>
            Organized by category for easy discovery
          </Text>
        )}
      </div>

      {/* Categories Grid - Only show when not collapsed */}
      {!isCollapsed && (
        <div
          className="suggestions-grid"
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(3, 1fr)',
            gap: '24px',
            alignItems: 'start',
            width: '100%'
          }}>
        {groupedSuggestions.map((group, index) => {
          const isHovered = hoveredCategory === group.category.categoryKey;
          const displaySuggestions = isHovered
            ? allSuggestions.find(cat => cat.category.categoryKey === group.category.categoryKey)?.suggestions || group.suggestions
            : group.suggestions;

          return (
            <Card
              key={`${group.category.id}-${index}`}
              onMouseEnter={() => setHoveredCategory(group.category.categoryKey)}
              onMouseLeave={() => setHoveredCategory(null)}
              style={{
                borderRadius: '20px',
                border: 'none',
                boxShadow: isHovered
                  ? '0 16px 48px rgba(0, 0, 0, 0.15)'
                  : '0 8px 32px rgba(0, 0, 0, 0.08)',
                transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
                height: 'fit-content',
                background: 'linear-gradient(135deg, #ffffff 0%, #f8fafc 100%)',
                position: 'relative',
                overflow: 'hidden',
                animation: randomizing ? 'cardShuffle 0.6s ease-in-out' : 'none',
                transform: isHovered ? 'scale(1.02) translateY(-8px)' : 'scale(1) translateY(0)',
                zIndex: isHovered ? 10 : 1
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
              {/* Individual shuffle button */}
              <Tooltip title={`Shuffle ${group.category.title} examples`}>
                <Button
                  type="text"
                  size="small"
                  icon={<ReloadOutlined spin={shufflingCategory === group.category.categoryKey} />}
                  onClick={(e) => {
                    e.stopPropagation();
                    shuffleCategory(group.category.categoryKey);
                  }}
                  loading={shufflingCategory === group.category.categoryKey}
                  style={{
                    position: 'absolute',
                    top: '0',
                    right: '0',
                    borderRadius: '50%',
                    width: '28px',
                    height: '28px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    border: '1px solid #e2e8f0',
                    backgroundColor: '#ffffff',
                    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
                    transition: 'all 0.3s ease',
                    zIndex: 10
                  }}
                />
              </Tooltip>

              {group.category.icon && (
                <div style={{
                  fontSize: '32px',
                  marginBottom: '8px',
                  filter: 'drop-shadow(0 2px 4px rgba(0,0,0,0.1))'
                }}>
                  {group.category.icon}
                </div>
              )}
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
              {displaySuggestions.map((suggestion, suggestionIndex) => (
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
                    border: 'none',
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
                        alignItems: 'center',
                        gap: '8px'
                      }}>
                        {onSubmitQuery && (
                          <Tooltip title="Submit this query">
                            <Button
                              type="primary"
                              size="small"
                              icon={<SendOutlined />}
                              onClick={(e) => {
                                e.stopPropagation();
                                onSubmitQuery(suggestion.queryText);
                              }}
                              style={{
                                borderRadius: '8px',
                                height: '24px',
                                width: '24px',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                fontSize: '10px',
                                background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
                                border: 'none',
                                boxShadow: '0 2px 8px rgba(59, 130, 246, 0.3)'
                              }}
                            />
                          </Tooltip>
                        )}
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
          );
        })}
        </div>
      )}

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
          transform: scale(1.02) translateY(-8px) !important;
          box-shadow: 0 16px 48px rgba(0, 0, 0, 0.15) !important;
          z-index: 10 !important;
        }

        .suggestion-card {
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
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
