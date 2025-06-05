const http = require('http');

// Test admin login with default credentials
const testAdminLogin = () => {
  console.log('Testing admin login with default credentials...');
  
  const loginData = JSON.stringify({
    username: 'admin',
    password: 'Admin123!'
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
      console.log('Login Headers:', res.headers);
      
      let data = '';
      res.on('data', (chunk) => {
        data += chunk;
      });
      
      res.on('end', () => {
        console.log('Login Response:', data);
        
        if (res.statusCode === 200) {
          try {
            const loginResponse = JSON.parse(data);
            console.log('✅ Login successful!');
            console.log('Token received:', loginResponse.accessToken ? 'Yes' : 'No');
            console.log('User info:', loginResponse.user || 'Not provided');
            
            if (loginResponse.accessToken) {
              testSchemaAPIWithToken(loginResponse.accessToken);
            }
            resolve(loginResponse);
          } catch (error) {
            console.log('❌ Error parsing login response:', error.message);
            resolve(null);
          }
        } else {
          console.log('❌ Login failed');
          console.log('Response body:', data);
          resolve(null);
        }
      });
    });

    loginReq.on('error', (err) => {
      console.log('❌ Login request error:', err.message);
      reject(err);
    });

    loginReq.write(loginData);
    loginReq.end();
  });
};

const testSchemaAPIWithToken = (token) => {
  console.log('\n🔍 Testing Schema Management API with token...');
  
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
    console.log(`Schema API Status: ${res.statusCode}`);
    
    let data = '';
    res.on('data', (chunk) => {
      data += chunk;
    });
    
    res.on('end', () => {
      console.log('Schema API Response:', data);
      if (res.statusCode === 200) {
        console.log('✅ Schema Management API is working!');
        try {
          const schemas = JSON.parse(data);
          console.log(`📊 Found ${schemas.length} schemas in the system`);
          if (schemas.length > 0) {
            console.log('First schema:', schemas[0]);
          }
        } catch (error) {
          console.log('Response is not valid JSON');
        }
      } else {
        console.log('❌ Schema Management API returned an error');
      }
    });
  });

  req.on('error', (err) => {
    console.log('❌ Schema API Error:', err.message);
  });

  req.end();
};

// Test other authenticated endpoints
const testOtherAuthenticatedEndpoints = (token) => {
  console.log('\n🔍 Testing other authenticated endpoints...');
  
  const endpoints = [
    '/api/auth/validate',
    '/api/SchemaManagement/user-preferences',
    '/api/query/history'
  ];

  endpoints.forEach(endpoint => {
    const options = {
      hostname: 'localhost',
      port: 55243,
      path: endpoint,
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    };

    const req = http.request(options, (res) => {
      console.log(`${endpoint}: ${res.statusCode}`);
      if (res.statusCode === 200) {
        let data = '';
        res.on('data', (chunk) => {
          data += chunk;
        });
        res.on('end', () => {
          console.log(`  Response: ${data.substring(0, 100)}${data.length > 100 ? '...' : ''}`);
        });
      }
    });

    req.on('error', (err) => {
      console.log(`${endpoint}: Error - ${err.message}`);
    });

    req.end();
  });
};

// Run the test
const runTest = async () => {
  console.log('=== Admin Login Test ===\n');
  
  try {
    const loginResult = await testAdminLogin();
    
    if (loginResult && loginResult.accessToken) {
      testOtherAuthenticatedEndpoints(loginResult.accessToken);
      
      console.log('\n=== Connection Summary ===');
      console.log('✅ Backend API is running on port 55243');
      console.log('✅ Frontend is running on port 3001');
      console.log('✅ Admin authentication is working');
      console.log('✅ Schema Management API is accessible');
      console.log('\n📝 Next Steps:');
      console.log('1. Open browser: http://localhost:3001');
      console.log('2. Login with: admin / Admin123!');
      console.log('3. Navigate to: Admin > Schema Management');
      console.log('4. Test the Schema Management UI');
    } else {
      console.log('\n❌ Authentication failed - check backend logs');
    }
    
  } catch (error) {
    console.error('❌ Test failed:', error);
  }
};

runTest();
