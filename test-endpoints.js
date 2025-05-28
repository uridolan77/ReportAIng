// Test script for new streaming and visualization endpoints
const API_BASE = 'https://localhost:7001/api';

// Mock data for testing
const mockColumns = [
  { name: 'ProductName', dataType: 'string' },
  { name: 'Sales', dataType: 'decimal' },
  { name: 'Region', dataType: 'string' },
  { name: 'Date', dataType: 'datetime' }
];

const mockData = [
  { ProductName: 'Product A', Sales: 1000, Region: 'North', Date: '2024-01-01' },
  { ProductName: 'Product B', Sales: 1500, Region: 'South', Date: '2024-01-02' },
  { ProductName: 'Product C', Sales: 800, Region: 'East', Date: '2024-01-03' },
  { ProductName: 'Product D', Sales: 2000, Region: 'West', Date: '2024-01-04' },
  { ProductName: 'Product E', Sales: 1200, Region: 'North', Date: '2024-01-05' }
];

// Test functions
async function testStreamingBackpressure() {
  console.log('\n🚀 Testing Streaming with Backpressure...');
  
  try {
    const response = await fetch(`${API_BASE}/streaming-query/stream-backpressure`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify({
        question: 'Show me all sales data',
        maxRows: 5000,
        timeoutSeconds: 120,
        chunkSize: 100,
        enableProgressReporting: true
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    console.log('✅ Backpressure streaming endpoint is accessible');
    
    // Test streaming response
    const reader = response.body?.getReader();
    if (reader) {
      let chunkCount = 0;
      const decoder = new TextDecoder();
      
      while (chunkCount < 3) { // Read first 3 chunks
        const { done, value } = await reader.read();
        if (done) break;
        
        const chunk = decoder.decode(value);
        console.log(`📦 Received chunk ${++chunkCount}:`, chunk.substring(0, 100) + '...');
      }
      
      reader.cancel();
    }
    
  } catch (error) {
    console.error('❌ Backpressure streaming test failed:', error.message);
  }
}

async function testStreamingProgress() {
  console.log('\n📊 Testing Streaming with Progress...');
  
  try {
    const response = await fetch(`${API_BASE}/streaming-query/stream-progress`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify({
        question: 'Show me customer data with progress tracking',
        maxRows: 1000,
        timeoutSeconds: 60,
        chunkSize: 50,
        enableProgressReporting: true
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    console.log('✅ Progress streaming endpoint is accessible');
    
    // Test streaming response
    const reader = response.body?.getReader();
    if (reader) {
      let progressCount = 0;
      const decoder = new TextDecoder();
      
      while (progressCount < 3) { // Read first 3 progress updates
        const { done, value } = await reader.read();
        if (done) break;
        
        const progressData = decoder.decode(value);
        console.log(`📈 Progress update ${++progressCount}:`, progressData.substring(0, 150) + '...');
      }
      
      reader.cancel();
    }
    
  } catch (error) {
    console.error('❌ Progress streaming test failed:', error.message);
  }
}

async function testInteractiveVisualization() {
  console.log('\n🎨 Testing Interactive Visualization...');
  
  try {
    const response = await fetch(`${API_BASE}/visualization/interactive`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify({
        query: 'Sales by region analysis',
        columns: mockColumns,
        data: mockData
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const result = await response.json();
    console.log('✅ Interactive visualization endpoint working');
    console.log('📊 Generated config:', {
      chartType: result.baseVisualization?.type,
      filtersCount: result.filters?.length || 0,
      interactiveFeatures: Object.keys(result.interactiveFeatures || {}).length,
      drillDownOptions: result.drillDownOptions?.length || 0
    });
    
  } catch (error) {
    console.error('❌ Interactive visualization test failed:', error.message);
  }
}

async function testDashboard() {
  console.log('\n📋 Testing Dashboard Generation...');
  
  try {
    const response = await fetch(`${API_BASE}/visualization/dashboard`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify({
        query: 'Comprehensive sales dashboard',
        columns: mockColumns,
        data: mockData
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const result = await response.json();
    console.log('✅ Dashboard endpoint working');
    console.log('📊 Generated dashboard:', {
      title: result.title,
      chartsCount: result.charts?.length || 0,
      layout: `${result.layout?.rows}x${result.layout?.columns}`,
      globalFilters: result.globalFilters?.length || 0,
      refreshInterval: result.refreshInterval || 'none'
    });
    
  } catch (error) {
    console.error('❌ Dashboard test failed:', error.message);
  }
}

async function testEnhancedChartTypes() {
  console.log('\n📈 Testing Enhanced Chart Types...');
  
  try {
    const response = await fetch(`${API_BASE}/visualization/chart-types/enhanced`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${getToken()}`
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const chartTypes = await response.json();
    console.log('✅ Enhanced chart types endpoint working');
    console.log(`📊 Available chart types: ${chartTypes.length}`);
    
    chartTypes.slice(0, 3).forEach(chart => {
      console.log(`  - ${chart.name}: ${chart.interactiveFeatures?.join(', ') || 'none'}`);
    });
    
  } catch (error) {
    console.error('❌ Enhanced chart types test failed:', error.message);
  }
}

async function testBasicQuery() {
  console.log('\n🔍 Testing Basic Query (for comparison)...');
  
  try {
    const response = await fetch(`${API_BASE}/query/natural-language`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify({
        question: 'Show me total sales by region',
        sessionId: 'test-session',
        options: {
          includeVisualization: true,
          maxRows: 100,
          enableCache: false
        }
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const result = await response.json();
    console.log('✅ Basic query endpoint working');
    console.log('📊 Query result:', {
      success: result.success,
      rowCount: result.data?.length || 0,
      hasVisualization: !!result.visualization,
      executionTime: result.executionTimeMs || 'unknown'
    });
    
  } catch (error) {
    console.error('❌ Basic query test failed:', error.message);
  }
}

// Helper function to get auth token
function getToken() {
  // In a real scenario, you'd get this from localStorage or a secure store
  // For testing, you might need to implement authentication first
  return 'test-token-replace-with-real-token';
}

// Main test runner
async function runAllTests() {
  console.log('🧪 Starting API Endpoint Tests...');
  console.log('=' .repeat(50));
  
  // Test basic functionality first
  await testBasicQuery();
  
  // Test new streaming endpoints
  await testStreamingBackpressure();
  await testStreamingProgress();
  
  // Test new visualization endpoints
  await testInteractiveVisualization();
  await testDashboard();
  await testEnhancedChartTypes();
  
  console.log('\n' + '=' .repeat(50));
  console.log('🏁 All tests completed!');
  console.log('\n💡 Next steps:');
  console.log('1. Configure your OpenAI API keys in appsettings.json');
  console.log('2. Start the backend server');
  console.log('3. Run these tests with a valid auth token');
  console.log('4. Test the frontend components in the browser');
}

// Export for use in browser or Node.js
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { runAllTests, testStreamingBackpressure, testInteractiveVisualization };
} else {
  // Browser environment - attach to window
  window.apiTests = { runAllTests, testStreamingBackpressure, testInteractiveVisualization };
}

// Auto-run if this is the main script
if (typeof require !== 'undefined' && require.main === module) {
  runAllTests().catch(console.error);
}
