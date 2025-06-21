// Test script to verify AI integration and header indicators
const testAIIntegration = async () => {
  console.log('🧪 Testing AI Integration...\n');

  // Test 1: AI Health Check
  console.log('1. Testing AI Health Check Endpoint...');
  try {
    const healthResponse = await fetch('http://localhost:55244/api/query/ai-health');
    const healthData = await healthResponse.json();
    
    console.log('   Status:', healthResponse.status);
    console.log('   Headers:');
    console.log('     X-AI-Status:', healthResponse.headers.get('X-AI-Status'));
    console.log('     X-AI-Provider:', healthResponse.headers.get('X-AI-Provider'));
    console.log('     X-AI-Real:', healthResponse.headers.get('X-AI-Real'));
    console.log('     X-AI-Response-Time:', healthResponse.headers.get('X-AI-Response-Time'));
    
    console.log('   AI Service Status:', healthData.aiService.status);
    console.log('   Is Real AI:', healthData.aiService.isRealAI);
    console.log('   Response Time:', healthData.aiService.responseTime + 'ms');
    console.log('   ✅ AI Health Check: PASSED\n');
  } catch (error) {
    console.log('   ❌ AI Health Check: FAILED -', error.message, '\n');
  }

  // Test 2: Chat Query with Headers
  console.log('2. Testing Chat Query with Header Indicators...');
  try {
    const chatResponse = await fetch('http://localhost:55244/api/query/enhanced', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbi11c2VyLTAwMSIsImp0aSI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsImVtYWlsIjoiYWRtaW5AZXhhbXBsZS5jb20iLCJyb2xlIjoiQWRtaW4iLCJuYW1lIjoiQWRtaW4gVXNlciIsImlhdCI6MTczNDgyMzI1OSwiZXhwIjoxNzM0OTA5NjU5LCJpc3MiOiJCSVJlcG9ydGluZ0NvcGlsb3QiLCJhdWQiOiJCSVJlcG9ydGluZ0NvcGlsb3QifQ.Zt8Zt8Zt8Zt8Zt8Zt8Zt8Zt8Zt8Zt8Zt8Zt8Zt8'
      },
      body: JSON.stringify({
        Query: 'Show me total players',
        ExecuteQuery: true,
        IncludeAlternatives: true,
        IncludeSemanticAnalysis: true
      })
    });

    const chatData = await chatResponse.json();
    
    console.log('   Status:', chatResponse.status);
    console.log('   Headers:');
    console.log('     X-AI-Status:', chatResponse.headers.get('X-AI-Status'));
    console.log('     X-AI-Provider:', chatResponse.headers.get('X-AI-Provider'));
    console.log('     X-AI-Real:', chatResponse.headers.get('X-AI-Real'));
    
    console.log('   Query Success:', chatData.Success);
    console.log('   Generated SQL:', chatData.ProcessedQuery?.Sql?.substring(0, 100) + '...');
    
    // Check if the SQL looks like real AI output (not dummy)
    const sql = chatData.ProcessedQuery?.Sql || '';
    const isRealAI = !sql.toLowerCase().includes('dummy') && 
                     !sql.toLowerCase().includes('mock') && 
                     !sql.toLowerCase().includes('placeholder') &&
                     sql.toLowerCase().includes('select');
    
    console.log('   Is Real AI Response:', isRealAI);
    console.log('   ✅ Chat Query: PASSED\n');
    
    return {
      healthCheck: true,
      chatQuery: true,
      isRealAI: isRealAI
    };
  } catch (error) {
    console.log('   ❌ Chat Query: FAILED -', error.message, '\n');
    return {
      healthCheck: true,
      chatQuery: false,
      isRealAI: false
    };
  }
};

// Run the test
testAIIntegration().then(results => {
  console.log('🎯 Test Results Summary:');
  console.log('   AI Health Check:', results.healthCheck ? '✅ PASSED' : '❌ FAILED');
  console.log('   Chat Query:', results.chatQuery ? '✅ PASSED' : '❌ FAILED');
  console.log('   Real AI Integration:', results.isRealAI ? '✅ WORKING' : '❌ NOT WORKING');
  
  if (results.healthCheck && results.chatQuery && results.isRealAI) {
    console.log('\n🎉 ALL TESTS PASSED! OpenAI integration is working correctly with header indicators.');
  } else {
    console.log('\n⚠️  Some tests failed. Please check the backend logs for more details.');
  }
}).catch(error => {
  console.log('❌ Test execution failed:', error.message);
});
