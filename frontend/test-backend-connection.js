const https = require('https');
const http = require('http');

// Test backend connection
const testBackendConnection = () => {
  console.log('Testing backend connection...');
  
  // Try HTTP first (port 55243)
  const httpOptions = {
    hostname: 'localhost',
    port: 55243,
    path: '/api/health',
    method: 'GET'
  };

  const httpReq = http.request(httpOptions, (res) => {
    console.log(`HTTP Status: ${res.statusCode}`);
    console.log(`HTTP Headers:`, res.headers);

    let data = '';
    res.on('data', (chunk) => {
      data += chunk;
    });

    res.on('end', () => {
      console.log('HTTP Response:', data);
      testSchemaManagementAPI();
    });
  });

  httpReq.on('error', (err) => {
    console.log('HTTP Error:', err.message);
    console.log('Backend is not accessible on port 55243');
  });

  httpReq.end();
};

// Test Schema Management API
const testSchemaManagementAPI = () => {
  console.log('\nTesting Schema Management API...');
  
  const options = {
    hostname: 'localhost',
    port: 55243,
    path: '/api/SchemaManagement/schemas',
    method: 'GET'
  };

  const req = http.request(options, (res) => {
    console.log(`Schema API Status: ${res.statusCode}`);
    
    let data = '';
    res.on('data', (chunk) => {
      data += chunk;
    });
    
    res.on('end', () => {
      console.log('Schema API Response:', data);
      if (res.statusCode === 200) {
        console.log('✅ Schema Management API is working!');
      } else {
        console.log('❌ Schema Management API returned an error');
      }
    });
  });

  req.on('error', (err) => {
    console.log('Schema API Error:', err.message);
    console.log('❌ Schema Management API is not accessible');
  });

  req.end();
};

// Run the test
testBackendConnection();
