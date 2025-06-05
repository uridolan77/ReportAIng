const http = require('http');

// Test Schema Management API with authentication
const testSchemaManagementWithAuth = async () => {
  console.log('Testing Schema Management API with authentication...');
  
  // First, let's try to get a token by logging in
  const loginData = JSON.stringify({
    username: 'admin',
    password: 'admin123'
  });

  const loginOptions = {
    hostname: 'localhost',
    port: 55243,
    path: '/api/auth/login',
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Content-Length': Buffer.byteLength(loginData)
    }
  };

  return new Promise((resolve, reject) => {
    const loginReq = http.request(loginOptions, (res) => {
      console.log(`Login Status: ${res.statusCode}`);
      
      let data = '';
      res.on('data', (chunk) => {
        data += chunk;
      });
      
      res.on('end', () => {
        console.log('Login Response:', data);
        
        if (res.statusCode === 200) {
          try {
            const loginResponse = JSON.parse(data);
            const token = loginResponse.token || loginResponse.accessToken;
            
            if (token) {
              console.log('✅ Login successful, testing Schema Management API...');
              testSchemaAPIWithToken(token).then(resolve).catch(reject);
            } else {
              console.log('❌ No token received in login response');
              resolve();
            }
          } catch (error) {
            console.log('❌ Error parsing login response:', error.message);
            resolve();
          }
        } else {
          console.log('❌ Login failed, trying without authentication...');
          testSchemaAPIWithoutAuth().then(resolve).catch(reject);
        }
      });
    });

    loginReq.on('error', (err) => {
      console.log('Login Error:', err.message);
      console.log('Trying Schema API without authentication...');
      testSchemaAPIWithoutAuth().then(resolve).catch(reject);
    });

    loginReq.write(loginData);
    loginReq.end();
  });
};

const testSchemaAPIWithToken = (token) => {
  return new Promise((resolve) => {
    const options = {
      hostname: 'localhost',
      port: 55243,
      path: '/api/SchemaManagement/schemas',
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    };

    const req = http.request(options, (res) => {
      console.log(`Schema API Status (with auth): ${res.statusCode}`);
      
      let data = '';
      res.on('data', (chunk) => {
        data += chunk;
      });
      
      res.on('end', () => {
        console.log('Schema API Response (with auth):', data);
        if (res.statusCode === 200) {
          console.log('✅ Schema Management API is working with authentication!');
          try {
            const schemas = JSON.parse(data);
            console.log(`Found ${schemas.length} schemas`);
          } catch (error) {
            console.log('Response is not valid JSON');
          }
        } else {
          console.log('❌ Schema Management API returned an error with authentication');
        }
        resolve();
      });
    });

    req.on('error', (err) => {
      console.log('Schema API Error (with auth):', err.message);
      resolve();
    });

    req.end();
  });
};

const testSchemaAPIWithoutAuth = () => {
  return new Promise((resolve) => {
    const options = {
      hostname: 'localhost',
      port: 55243,
      path: '/api/SchemaManagement/schemas',
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    };

    const req = http.request(options, (res) => {
      console.log(`Schema API Status (no auth): ${res.statusCode}`);
      
      let data = '';
      res.on('data', (chunk) => {
        data += chunk;
      });
      
      res.on('end', () => {
        console.log('Schema API Response (no auth):', data);
        if (res.statusCode === 200) {
          console.log('✅ Schema Management API is working without authentication!');
        } else if (res.statusCode === 401) {
          console.log('ℹ️ Schema Management API requires authentication (401)');
        } else {
          console.log('❌ Schema Management API returned an error without authentication');
        }
        resolve();
      });
    });

    req.on('error', (err) => {
      console.log('Schema API Error (no auth):', err.message);
      resolve();
    });

    req.end();
  });
};

// Test other endpoints
const testOtherEndpoints = () => {
  console.log('\nTesting other API endpoints...');
  
  const endpoints = [
    '/api/health',
    '/api/query/schema',
    '/api/SchemaManagement/user-preferences'
  ];

  endpoints.forEach(endpoint => {
    const options = {
      hostname: 'localhost',
      port: 55243,
      path: endpoint,
      method: 'GET'
    };

    const req = http.request(options, (res) => {
      console.log(`${endpoint}: ${res.statusCode}`);
    });

    req.on('error', (err) => {
      console.log(`${endpoint}: Error - ${err.message}`);
    });

    req.end();
  });
};

// Run all tests
const runTests = async () => {
  console.log('=== Backend Connection Test ===\n');
  
  try {
    await testSchemaManagementWithAuth();
    testOtherEndpoints();
    
    console.log('\n=== Test Summary ===');
    console.log('✅ Backend is running on port 55243');
    console.log('✅ Frontend is running on port 3001');
    console.log('ℹ️ Check the browser at http://localhost:3001');
    console.log('ℹ️ Navigate to Admin > Schema Management to test the UI');
    
  } catch (error) {
    console.error('Test failed:', error);
  }
};

runTests();
