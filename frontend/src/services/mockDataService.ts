import { QueryResponse as FrontendQueryResponse } from '../types/query';

export interface MockDataConfig {
  enabled: boolean;
}

// Mock data for common example queries
const mockDataMap: Record<string, FrontendQueryResponse> = {
  // Top 10 players by deposits
  'top 10 players by deposits': {
    success: true,
    result: {
      data: [
        { player_id: 'P001', player_name: 'John Smith', total_deposits: 15750.00, deposit_count: 23, country: 'United Kingdom', status: 'Active' },
        { player_id: 'P002', player_name: 'Maria Garcia', total_deposits: 14200.50, deposit_count: 18, country: 'Spain', status: 'Active' },
        { player_id: 'P003', player_name: 'Hans Mueller', total_deposits: 13890.25, deposit_count: 31, country: 'Germany', status: 'Active' },
        { player_id: 'P004', player_name: 'Sophie Dubois', total_deposits: 12650.75, deposit_count: 15, country: 'France', status: 'Active' },
        { player_id: 'P005', player_name: 'Alessandro Rossi', total_deposits: 11980.00, deposit_count: 22, country: 'Italy', status: 'Active' },
        { player_id: 'P006', player_name: 'Erik Andersson', total_deposits: 11450.50, deposit_count: 19, country: 'Sweden', status: 'Active' },
        { player_id: 'P007', player_name: 'Katarina Novak', total_deposits: 10890.25, deposit_count: 27, country: 'Czech Republic', status: 'Active' },
        { player_id: 'P008', player_name: 'Dimitri Petrov', total_deposits: 10650.00, deposit_count: 16, country: 'Bulgaria', status: 'Active' },
        { player_id: 'P009', player_name: 'Anna Kowalski', total_deposits: 10200.75, deposit_count: 24, country: 'Poland', status: 'Active' },
        { player_id: 'P010', player_name: 'Carlos Silva', total_deposits: 9850.50, deposit_count: 20, country: 'Portugal', status: 'Active' }
      ],
      metadata: {
        columns: [
          { name: 'player_id', type: 'string', displayName: 'Player ID' },
          { name: 'player_name', type: 'string', displayName: 'Player Name' },
          { name: 'total_deposits', type: 'number', displayName: 'Total Deposits (€)' },
          { name: 'deposit_count', type: 'number', displayName: 'Deposit Count' },
          { name: 'country', type: 'string', displayName: 'Country' },
          { name: 'status', type: 'string', displayName: 'Status' }
        ],
        totalRows: 10,
        executionTimeMs: 245
      }
    },
    sql: `SELECT 
      p.player_id,
      p.player_name,
      SUM(da.deposit_amount) as total_deposits,
      COUNT(da.deposit_amount) as deposit_count,
      c.country_name as country,
      p.status
    FROM tbl_Daily_actions da
    JOIN tbl_Daily_actions_players p ON da.player_id = p.player_id
    JOIN tbl_Countries c ON p.country_id = c.country_id
    WHERE da.deposit_amount > 0
      AND da.action_date >= DATEADD(day, -7, GETDATE())
    GROUP BY p.player_id, p.player_name, c.country_name, p.status
    ORDER BY total_deposits DESC
    OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY`,
    confidence: 0.95,
    suggestions: [
      'Show me top 20 players by deposits',
      'Top players by withdrawals in the last 7 days',
      'Players with highest net gaming revenue this week'
    ],
    cached: false,
    executionTimeMs: 245,
    queryId: 'mock-001',
    timestamp: new Date().toISOString()
  },

  // Total deposits for yesterday
  'total deposits for yesterday': {
    success: true,
    result: {
      data: [
        { 
          date: '2024-01-15',
          total_deposits: 125750.50,
          deposit_count: 1247,
          unique_players: 892,
          average_deposit: 100.84
        }
      ],
      metadata: {
        columns: [
          { name: 'date', type: 'date', displayName: 'Date' },
          { name: 'total_deposits', type: 'number', displayName: 'Total Deposits (€)' },
          { name: 'deposit_count', type: 'number', displayName: 'Number of Deposits' },
          { name: 'unique_players', type: 'number', displayName: 'Unique Players' },
          { name: 'average_deposit', type: 'number', displayName: 'Average Deposit (€)' }
        ],
        totalRows: 1,
        executionTimeMs: 156
      }
    },
    sql: `SELECT 
      CAST(action_date AS DATE) as date,
      SUM(deposit_amount) as total_deposits,
      COUNT(deposit_amount) as deposit_count,
      COUNT(DISTINCT player_id) as unique_players,
      AVG(deposit_amount) as average_deposit
    FROM tbl_Daily_actions
    WHERE CAST(action_date AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)
      AND deposit_amount > 0
    GROUP BY CAST(action_date AS DATE)`,
    confidence: 0.98,
    suggestions: [
      'Total deposits for last week',
      'Compare yesterday deposits with previous day',
      'Hourly deposit breakdown for yesterday'
    ],
    cached: false,
    executionTimeMs: 156,
    queryId: 'mock-002',
    timestamp: new Date().toISOString()
  },

  // Top performing games
  'top performing games': {
    success: true,
    result: {
      data: [
        { game_name: 'Mega Fortune', net_gaming_revenue: 45250.75, sessions: 2847, unique_players: 1205, avg_bet: 12.50 },
        { game_name: 'Starburst', net_gaming_revenue: 38920.50, sessions: 3521, unique_players: 1456, avg_bet: 8.75 },
        { game_name: 'Book of Dead', net_gaming_revenue: 35680.25, sessions: 2934, unique_players: 1189, avg_bet: 15.25 },
        { game_name: 'Gonzo\'s Quest', net_gaming_revenue: 32450.00, sessions: 2156, unique_players: 987, avg_bet: 18.50 },
        { game_name: 'Dead or Alive', net_gaming_revenue: 29870.75, sessions: 1876, unique_players: 823, avg_bet: 22.00 },
        { game_name: 'Reactoonz', net_gaming_revenue: 27650.50, sessions: 2345, unique_players: 1034, avg_bet: 11.25 },
        { game_name: 'Fire Joker', net_gaming_revenue: 25890.25, sessions: 1987, unique_players: 756, avg_bet: 16.75 },
        { game_name: 'Sweet Bonanza', net_gaming_revenue: 24120.00, sessions: 2678, unique_players: 1123, avg_bet: 9.50 },
        { game_name: 'Wolf Gold', net_gaming_revenue: 22750.75, sessions: 1654, unique_players: 689, avg_bet: 19.25 },
        { game_name: 'Big Bass Bonanza', net_gaming_revenue: 21480.50, sessions: 2234, unique_players: 945, avg_bet: 13.75 }
      ],
      metadata: {
        columns: [
          { name: 'game_name', type: 'string', displayName: 'Game Name' },
          { name: 'net_gaming_revenue', type: 'number', displayName: 'Net Gaming Revenue (€)' },
          { name: 'sessions', type: 'number', displayName: 'Game Sessions' },
          { name: 'unique_players', type: 'number', displayName: 'Unique Players' },
          { name: 'avg_bet', type: 'number', displayName: 'Average Bet (€)' }
        ],
        totalRows: 10,
        executionTimeMs: 312
      }
    },
    sql: `SELECT 
      g.game_name,
      SUM(dag.net_gaming_revenue) as net_gaming_revenue,
      COUNT(dag.session_id) as sessions,
      COUNT(DISTINCT dag.player_id) as unique_players,
      AVG(dag.bet_amount) as avg_bet
    FROM tbl_Daily_actions_games dag
    JOIN Games g ON dag.game_id = g.game_id
    WHERE dag.action_date >= DATEADD(month, -1, GETDATE())
    GROUP BY g.game_name
    ORDER BY net_gaming_revenue DESC
    OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY`,
    confidence: 0.92,
    suggestions: [
      'Games with highest player retention this month',
      'Most popular games by session count',
      'Games performance comparison by provider'
    ],
    cached: false,
    executionTimeMs: 312,
    queryId: 'mock-003',
    timestamp: new Date().toISOString()
  },

  // New player registrations
  'new player registrations': {
    success: true,
    result: {
      data: [
        { date: '2024-01-15', new_registrations: 127, country: 'United Kingdom', conversion_rate: 0.23 },
        { date: '2024-01-15', new_registrations: 89, country: 'Germany', conversion_rate: 0.31 },
        { date: '2024-01-15', new_registrations: 76, country: 'Spain', conversion_rate: 0.28 },
        { date: '2024-01-15', new_registrations: 54, country: 'France', conversion_rate: 0.25 },
        { date: '2024-01-15', new_registrations: 43, country: 'Italy', conversion_rate: 0.29 },
        { date: '2024-01-15', new_registrations: 38, country: 'Sweden', conversion_rate: 0.35 },
        { date: '2024-01-15', new_registrations: 32, country: 'Netherlands', conversion_rate: 0.27 },
        { date: '2024-01-15', new_registrations: 28, country: 'Poland', conversion_rate: 0.22 },
        { date: '2024-01-15', new_registrations: 25, country: 'Czech Republic', conversion_rate: 0.26 },
        { date: '2024-01-15', new_registrations: 21, country: 'Portugal', conversion_rate: 0.24 }
      ],
      metadata: {
        columns: [
          { name: 'date', type: 'date', displayName: 'Date' },
          { name: 'new_registrations', type: 'number', displayName: 'New Registrations' },
          { name: 'country', type: 'string', displayName: 'Country' },
          { name: 'conversion_rate', type: 'number', displayName: 'Conversion Rate' }
        ],
        totalRows: 10,
        executionTimeMs: 189
      }
    },
    sql: `SELECT 
      CAST(p.registration_date AS DATE) as date,
      COUNT(*) as new_registrations,
      c.country_name as country,
      CAST(COUNT(CASE WHEN da.deposit_amount > 0 THEN 1 END) AS FLOAT) / COUNT(*) as conversion_rate
    FROM tbl_Daily_actions_players p
    JOIN tbl_Countries c ON p.country_id = c.country_id
    LEFT JOIN tbl_Daily_actions da ON p.player_id = da.player_id 
      AND da.action_date >= p.registration_date 
      AND da.action_date <= DATEADD(day, 7, p.registration_date)
    WHERE CAST(p.registration_date AS DATE) >= DATEADD(week, -1, GETDATE())
    GROUP BY CAST(p.registration_date AS DATE), c.country_name
    ORDER BY new_registrations DESC`,
    confidence: 0.89,
    suggestions: [
      'Registration trends by marketing channel',
      'Player retention rates by registration date',
      'Geographic distribution of new players'
    ],
    cached: false,
    executionTimeMs: 189,
    queryId: 'mock-004',
    timestamp: new Date().toISOString()
  }
};

export class MockDataService {
  private static config: MockDataConfig = {
    enabled: true // Default to enabled for testing
  };

  static setConfig(config: Partial<MockDataConfig>) {
    this.config = { ...this.config, ...config };
    localStorage.setItem('mockDataConfig', JSON.stringify(this.config));
  }

  static getConfig(): MockDataConfig {
    try {
      const stored = localStorage.getItem('mockDataConfig');
      if (stored) {
        this.config = JSON.parse(stored);
      }
    } catch (error) {
      console.warn('Failed to load mock data config from localStorage:', error);
    }
    return this.config;
  }

  static isEnabled(): boolean {
    return this.getConfig().enabled;
  }

  static getMockData(query: string): FrontendQueryResponse | null {
    console.log('🎭 Mock data service: getMockData called with query:', query);
    console.log('🎭 Mock data service: isEnabled:', this.isEnabled());

    if (!this.isEnabled()) {
      console.log('🎭 Mock data service: Mock data is disabled, returning null');
      return null;
    }

    const normalizedQuery = query.toLowerCase().trim();
    console.log('🎭 Mock data service: Normalized query:', normalizedQuery);
    console.log('🎭 Mock data service: Available keys:', Object.keys(mockDataMap));

    // Find matching mock data with flexible matching
    for (const [key, mockData] of Object.entries(mockDataMap)) {
      // Split key into words for better matching
      const keyWords = key.split(' ');
      const queryWords = normalizedQuery.split(' ');

      // Check if query contains key words or vice versa
      const hasKeyMatch = keyWords.some(keyWord => normalizedQuery.includes(keyWord));
      const hasQueryMatch = queryWords.some(queryWord => key.includes(queryWord));

      if (normalizedQuery.includes(key) || key.includes(normalizedQuery) || hasKeyMatch || hasQueryMatch) {
        console.log('🎭 Mock data service: Found match for query:', query, '-> key:', key);
        return {
          ...mockData,
          queryId: `mock-${Date.now()}`,
          timestamp: new Date().toISOString()
        };
      }
    }

    // Return generic mock data for unmatched queries
    console.log('🎭 Mock data service: No specific match found, returning generic data for:', query);

    // For testing purposes, always return mock data when enabled
    return this.getGenericMockData(query);
  }

  private static getGenericMockData(query: string): FrontendQueryResponse {
    return {
      success: true,
      result: {
        data: [
          { id: 1, name: 'Sample Data 1', value: 100, category: 'A' },
          { id: 2, name: 'Sample Data 2', value: 200, category: 'B' },
          { id: 3, name: 'Sample Data 3', value: 150, category: 'A' },
          { id: 4, name: 'Sample Data 4', value: 300, category: 'C' },
          { id: 5, name: 'Sample Data 5', value: 250, category: 'B' }
        ],
        metadata: {
          columns: [
            { name: 'id', type: 'number', displayName: 'ID' },
            { name: 'name', type: 'string', displayName: 'Name' },
            { name: 'value', type: 'number', displayName: 'Value' },
            { name: 'category', type: 'string', displayName: 'Category' }
          ],
          totalRows: 5,
          executionTimeMs: 50
        }
      },
      sql: `-- Mock SQL for: ${query}\nSELECT * FROM mock_table WHERE condition = 'sample'`,
      confidence: 0.75,
      suggestions: [
        'Try a more specific query',
        'Check the available tables',
        'Use example queries for better results'
      ],
      cached: false,
      executionTimeMs: 50,
      queryId: `mock-generic-${Date.now()}`,
      timestamp: new Date().toISOString()
    };
  }

  static getAvailableQueries(): string[] {
    return Object.keys(mockDataMap);
  }
}
