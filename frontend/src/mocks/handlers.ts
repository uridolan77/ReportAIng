import { rest } from 'msw';

// Mock data
const mockUser = {
  id: '1',
  username: 'testuser',
  displayName: 'Test User',
  email: 'test@example.com',
  roles: ['user']
};

const mockQueryResults = [
  { id: 1, name: 'Product A', sales: 1000, region: 'North' },
  { id: 2, name: 'Product B', sales: 1500, region: 'South' },
  { id: 3, name: 'Product C', sales: 800, region: 'East' },
  { id: 4, name: 'Product D', sales: 1200, region: 'West' }
];

const mockDashboards = [
  {
    id: '1',
    name: 'Sales Dashboard',
    description: 'Monthly sales overview',
    visualizations: [],
    layout: {},
    createdAt: Date.now() - 86400000,
    updatedAt: Date.now()
  }
];

export const handlers = [
  // Authentication endpoints
  rest.post('/api/auth/login', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        accessToken: 'mock-access-token',
        refreshToken: 'mock-refresh-token',
        user: mockUser
      })
    );
  }),

  rest.post('/api/auth/refresh', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        accessToken: 'new-mock-access-token',
        refreshToken: 'new-mock-refresh-token'
      })
    );
  }),

  rest.post('/api/auth/logout', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({ success: true })
    );
  }),

  // Query endpoints
  rest.post('/api/query/natural-language', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        sql: 'SELECT * FROM products ORDER BY sales DESC',
        results: mockQueryResults,
        executionTime: 150,
        rowCount: mockQueryResults.length
      })
    );
  }),

  rest.post('/api/query/execute-sql', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        results: mockQueryResults,
        executionTime: 89,
        rowCount: mockQueryResults.length
      })
    );
  }),

  rest.get('/api/query/history', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        queries: [
          {
            id: '1',
            question: 'Show me total sales',
            sql: 'SELECT SUM(sales) FROM products',
            successful: true,
            executionTimeMs: 150,
            timestamp: Date.now() - 3600000
          },
          {
            id: '2',
            question: 'Top 10 products',
            sql: 'SELECT * FROM products ORDER BY sales DESC LIMIT 10',
            successful: true,
            executionTimeMs: 89,
            timestamp: Date.now() - 1800000
          }
        ]
      })
    );
  }),

  rest.get('/api/query/suggestions', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        suggestions: [
          'Show me total sales by region',
          'What are the top 5 products?',
          'Compare this month vs last month',
          'Show sales trends over time'
        ]
      })
    );
  }),

  // Dashboard endpoints
  rest.get('/api/dashboard', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        dashboards: mockDashboards
      })
    );
  }),

  rest.post('/api/dashboard', (req, res, ctx) => {
    return res(
      ctx.status(201),
      ctx.json({
        success: true,
        dashboard: {
          id: '2',
          name: 'New Dashboard',
          description: 'Test dashboard',
          visualizations: [],
          layout: {},
          createdAt: Date.now(),
          updatedAt: Date.now()
        }
      })
    );
  }),

  // Visualization endpoints
  rest.post('/api/advanced-visualization/generate', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        visualization: {
          id: 'viz-1',
          type: 'bar',
          title: 'Sales by Product',
          data: mockQueryResults,
          config: {
            xAxis: 'name',
            yAxis: 'sales',
            colorScheme: 'blue'
          }
        }
      })
    );
  }),

  // Health check
  rest.get('/health', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        status: 'healthy',
        timestamp: new Date().toISOString(),
        services: {
          database: 'connected',
          ai: 'available',
          cache: 'active'
        }
      })
    );
  }),

  // Tuning endpoints
  rest.get('/api/tuning/business-tables', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        success: true,
        tables: [
          {
            id: '1',
            tableName: 'products',
            businessName: 'Product Catalog',
            description: 'All product information and sales data',
            columns: [
              { name: 'id', businessName: 'Product ID', description: 'Unique identifier' },
              { name: 'name', businessName: 'Product Name', description: 'Display name' },
              { name: 'sales', businessName: 'Sales Amount', description: 'Total sales in USD' }
            ]
          }
        ]
      })
    );
  }),

  // Error scenarios for testing
  rest.post('/api/query/error-test', (req, res, ctx) => {
    return res(
      ctx.status(500),
      ctx.json({
        success: false,
        error: 'Database connection failed',
        code: 'DB_ERROR'
      })
    );
  }),

  rest.post('/api/auth/invalid-login', (req, res, ctx) => {
    return res(
      ctx.status(401),
      ctx.json({
        success: false,
        error: 'Invalid credentials'
      })
    );
  })
];
