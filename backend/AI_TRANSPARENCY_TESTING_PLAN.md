# 🧪 AI Transparency Foundation - Backend Testing Plan
## BI Reporting Copilot - API Testing & Validation Strategy

### 📋 Executive Summary

This document outlines the comprehensive testing strategy for the newly implemented AI Transparency Foundation backend APIs. We have implemented 3 new controllers with 15+ endpoints and need to validate their functionality before frontend integration.

---

## 🎯 **TESTING OBJECTIVES**

### **Primary Goals**
1. ✅ Verify all API endpoints are accessible and return expected responses
2. ✅ Validate request/response DTOs are properly serialized
3. ✅ Test streaming endpoints with real-time data flow
4. ✅ Ensure proper error handling and validation
5. ✅ Confirm service registration and dependency injection

### **Secondary Goals**
1. ✅ Performance testing for streaming endpoints
2. ✅ Load testing for transparency analysis
3. ✅ Integration testing with existing services
4. ✅ Security and authorization validation

---

## 🔧 **TESTING APPROACH**

### **1. Manual API Testing with Swagger**
- Use Swagger UI at `http://localhost:55244/swagger/index.html`
- Test each endpoint individually
- Validate request/response schemas
- Check error handling scenarios

### **2. Automated Testing with Postman/Insomnia**
- Create comprehensive API test collection
- Test positive and negative scenarios
- Validate data types and structures
- Performance benchmarking

### **3. Integration Testing**
- Test with existing business context services
- Validate transparency service integration
- Check audit logging functionality
- Verify caching behavior

### **4. Streaming Testing**
- Test Server-Sent Events functionality
- Validate real-time data flow
- Check connection handling and cleanup
- Test cancellation scenarios

---

## 📊 **TEST SCENARIOS BY CONTROLLER**

### **🔍 TransparencyController Tests**

#### **Test 1: Analyze Prompt Construction**
```http
POST /api/transparency/analyze
Content-Type: application/json

{
  "userQuery": "Show me sales data for last quarter",
  "userId": "test-user-123",
  "context": {
    "domain": "sales",
    "timeframe": "quarterly"
  },
  "includeAlternatives": true,
  "includeOptimizations": true
}
```

**Expected Response:**
```json
{
  "traceId": "trace-guid-here",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:30:02Z",
  "userQuestion": "Show me sales data for last quarter",
  "intentType": "Analytical",
  "steps": [
    {
      "stepNumber": 1,
      "stepName": "Context Analysis",
      "description": "Analyzed intent: Analytical",
      "confidence": 0.92,
      "tokensAdded": 50
    }
  ],
  "overallConfidence": 0.85,
  "totalTokens": 250
}
```

#### **Test 2: Get Transparency Trace**
```http
GET /api/transparency/trace/{traceId}
```

#### **Test 3: Get Confidence Breakdown**
```http
GET /api/transparency/confidence/{analysisId}
```

#### **Test 4: Get Alternative Options**
```http
GET /api/transparency/alternatives/{traceId}
```

#### **Test 5: Get Optimization Suggestions**
```http
POST /api/transparency/optimize
Content-Type: application/json

{
  "userQuery": "Show me sales data for last quarter",
  "userId": "test-user-123",
  "optimizationTypes": ["token", "accuracy", "performance"]
}
```

#### **Test 6: Get Transparency Metrics**
```http
GET /api/transparency/metrics?userId=test-user-123&days=7
```

### **🌊 AIStreamingController Tests**

#### **Test 1: Start Streaming Session**
```http
POST /api/aistreaming/start
Content-Type: application/json

{
  "query": "Analyze sales trends for Q4 2023",
  "userId": "test-user-123",
  "options": {
    "enableProgressUpdates": true,
    "enableDetailedMetrics": true,
    "updateIntervalMs": 500
  },
  "enableTransparency": true,
  "enableCostMonitoring": true
}
```

**Expected Response:**
```json
{
  "sessionId": "session-guid-here",
  "status": "started"
}
```

#### **Test 2: Stream Query Processing**
```http
GET /api/aistreaming/query/{sessionId}
Accept: text/event-stream
```

**Expected SSE Stream:**
```
data: {"sessionId":"session-123","type":"progress","data":{"phase":"parsing","progress":0.2,"currentStep":"Parsing natural language query","confidence":0.7},"timestamp":"2024-01-15T10:30:00Z","sequenceNumber":1}

data: {"sessionId":"session-123","type":"progress","data":{"phase":"understanding","progress":0.4,"currentStep":"Understanding business intent","confidence":0.8},"timestamp":"2024-01-15T10:30:01Z","sequenceNumber":2}

data: {"sessionId":"session-123","type":"complete","data":{"message":"Query processing completed successfully","results":{"rowCount":42,"executionTime":"1.2s"}},"timestamp":"2024-01-15T10:30:05Z","sequenceNumber":6}
```

#### **Test 3: Stream Insight Generation**
```http
GET /api/aistreaming/insights/{queryId}
Accept: text/event-stream
```

#### **Test 4: Stream Analytics Updates**
```http
GET /api/aistreaming/analytics/{userId}
Accept: text/event-stream
```

#### **Test 5: Cancel Streaming Session**
```http
POST /api/aistreaming/cancel/{sessionId}
```

#### **Test 6: Get Session Status**
```http
GET /api/aistreaming/status/{sessionId}
```

### **🤖 IntelligentAgentsController Tests**

#### **Test 1: Orchestrate Tasks**
```http
POST /api/intelligentagents/orchestrate
Content-Type: application/json

{
  "taskType": "query_processing",
  "userId": "test-user-123",
  "parameters": {
    "query": "Show me top performing products",
    "complexity": "medium"
  },
  "requiredAgents": ["query-understanding", "schema-navigation", "sql-generation"],
  "priority": "Normal"
}
```

**Expected Response:**
```json
{
  "orchestrationId": "orch-guid-here",
  "taskType": "query_processing",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:30:03Z",
  "totalDuration": "00:00:03",
  "status": "completed",
  "agentResults": [
    {
      "agentId": "query-understanding",
      "taskType": "intent_analysis",
      "status": "completed",
      "result": {"intent": "analytical", "confidence": 0.92},
      "processingTime": "00:00:00.150"
    }
  ]
}
```

#### **Test 2: Get Agent Capabilities**
```http
GET /api/intelligentagents/capabilities
```

#### **Test 3: Navigate Schema**
```http
POST /api/intelligentagents/schema/navigate
Content-Type: application/json

{
  "query": "Show me sales data",
  "userId": "test-user-123",
  "maxTables": 10,
  "includeRelationships": true,
  "navigationStrategy": "relevance"
}
```

#### **Test 4: Understand Query**
```http
POST /api/intelligentagents/query/understand
Content-Type: application/json

{
  "query": "What were the top selling products last month?",
  "userId": "test-user-123",
  "includeEntities": true,
  "includeIntent": true,
  "language": "en"
}
```

#### **Test 5: Get Communication Logs**
```http
GET /api/intelligentagents/communication/logs?agentId=query-understanding&limit=50
```

---

## 🧪 **AUTOMATED TEST SCRIPTS**

### **PowerShell Test Script**
```powershell
# File: test-transparency-apis.ps1
$baseUrl = "http://localhost:55244/api"

# Test 1: Analyze Prompt Construction
Write-Host "Testing Transparency API..." -ForegroundColor Green

$analyzeRequest = @{
    userQuery = "Show me sales data for last quarter"
    userId = "test-user-123"
    includeAlternatives = $true
    includeOptimizations = $true
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "$baseUrl/transparency/analyze" -Method POST -Body $analyzeRequest -ContentType "application/json"
Write-Host "Analyze Response: $($response | ConvertTo-Json -Depth 3)"

# Test 2: Start Streaming Session
Write-Host "Testing Streaming API..." -ForegroundColor Green

$streamRequest = @{
    query = "Analyze sales trends"
    userId = "test-user-123"
    options = @{
        enableProgressUpdates = $true
        updateIntervalMs = 500
    }
} | ConvertTo-Json

$streamResponse = Invoke-RestMethod -Uri "$baseUrl/aistreaming/start" -Method POST -Body $streamRequest -ContentType "application/json"
Write-Host "Stream Session: $($streamResponse.sessionId)"

# Test 3: Get Agent Capabilities
Write-Host "Testing Agents API..." -ForegroundColor Green

$capabilities = Invoke-RestMethod -Uri "$baseUrl/intelligentagents/capabilities" -Method GET
Write-Host "Agent Capabilities: $($capabilities.Count) agents found"

Write-Host "All tests completed!" -ForegroundColor Green
```

### **cURL Test Script**
```bash
#!/bin/bash
# File: test-transparency-apis.sh

BASE_URL="http://localhost:55244/api"

echo "🔍 Testing Transparency API..."

# Test Analyze Prompt Construction
curl -X POST "$BASE_URL/transparency/analyze" \
  -H "Content-Type: application/json" \
  -d '{
    "userQuery": "Show me sales data for last quarter",
    "userId": "test-user-123",
    "includeAlternatives": true,
    "includeOptimizations": true
  }' | jq '.'

echo "🌊 Testing Streaming API..."

# Test Start Streaming Session
SESSION_RESPONSE=$(curl -X POST "$BASE_URL/aistreaming/start" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Analyze sales trends",
    "userId": "test-user-123",
    "options": {
      "enableProgressUpdates": true,
      "updateIntervalMs": 500
    }
  }')

SESSION_ID=$(echo $SESSION_RESPONSE | jq -r '.sessionId')
echo "Session ID: $SESSION_ID"

echo "🤖 Testing Agents API..."

# Test Get Agent Capabilities
curl -X GET "$BASE_URL/intelligentagents/capabilities" | jq '.'

echo "✅ All tests completed!"
```

---

## 📈 **PERFORMANCE TESTING**

### **Load Testing Scenarios**

#### **Scenario 1: Transparency Analysis Load**
- **Concurrent Users**: 10, 50, 100
- **Test Duration**: 5 minutes
- **Endpoint**: `/api/transparency/analyze`
- **Expected Response Time**: < 2 seconds
- **Success Rate**: > 95%

#### **Scenario 2: Streaming Connections**
- **Concurrent Streams**: 5, 20, 50
- **Test Duration**: 10 minutes
- **Endpoint**: `/api/aistreaming/query/{sessionId}`
- **Expected Latency**: < 500ms
- **Connection Stability**: > 98%

#### **Scenario 3: Agent Orchestration**
- **Concurrent Requests**: 5, 15, 30
- **Test Duration**: 3 minutes
- **Endpoint**: `/api/intelligentagents/orchestrate`
- **Expected Response Time**: < 5 seconds
- **Success Rate**: > 90%

---

## 🔒 **SECURITY TESTING**

### **Authentication & Authorization**
1. ✅ Test endpoints without authentication
2. ✅ Test with invalid user tokens
3. ✅ Test with expired tokens
4. ✅ Validate user context isolation

### **Input Validation**
1. ✅ Test with malformed JSON
2. ✅ Test with SQL injection attempts
3. ✅ Test with XSS payloads
4. ✅ Test with oversized requests

### **Rate Limiting**
1. ✅ Test rapid successive requests
2. ✅ Validate rate limiting responses
3. ✅ Test rate limit recovery

---

## 📋 **TEST EXECUTION CHECKLIST**

### **Pre-Testing Setup**
- [ ] Ensure backend is running on port 55244
- [ ] Verify database connection is working
- [ ] Check Swagger UI is accessible
- [ ] Confirm all services are registered in DI

### **Manual Testing (Swagger UI)**
- [ ] Test all TransparencyController endpoints
- [ ] Test all AIStreamingController endpoints
- [ ] Test all IntelligentAgentsController endpoints
- [ ] Verify error responses for invalid inputs
- [ ] Check response schemas match DTOs

### **Automated Testing**
- [ ] Run PowerShell test script
- [ ] Run cURL test script
- [ ] Execute Postman collection
- [ ] Validate all responses

### **Streaming Testing**
- [ ] Test SSE connection establishment
- [ ] Verify real-time data flow
- [ ] Test connection cleanup
- [ ] Test cancellation scenarios

### **Integration Testing**
- [ ] Test with existing business context services
- [ ] Verify audit logging is working
- [ ] Check transparency service integration
- [ ] Validate caching behavior

### **Performance Testing**
- [ ] Run load tests for each controller
- [ ] Monitor memory usage during streaming
- [ ] Check response times under load
- [ ] Validate connection limits

---

## 🎯 **SUCCESS CRITERIA**

### **Functional Requirements**
- ✅ All endpoints return HTTP 200 for valid requests
- ✅ All DTOs serialize/deserialize correctly
- ✅ Streaming endpoints maintain stable connections
- ✅ Error handling returns appropriate HTTP status codes
- ✅ Audit logging captures all API calls

### **Performance Requirements**
- ✅ Transparency analysis completes within 2 seconds
- ✅ Streaming latency remains under 500ms
- ✅ Agent orchestration completes within 5 seconds
- ✅ System handles 50 concurrent users
- ✅ Memory usage remains stable during extended testing

### **Quality Requirements**
- ✅ No memory leaks during streaming tests
- ✅ Proper cleanup of resources
- ✅ Consistent response formats
- ✅ Comprehensive error messages
- ✅ Security validations pass

---

## 🚀 **NEXT STEPS**

1. **Execute Manual Testing** - Use Swagger UI to test all endpoints
2. **Run Automated Scripts** - Execute PowerShell and cURL tests
3. **Performance Validation** - Run load tests for critical endpoints
4. **Integration Verification** - Test with existing services
5. **Documentation Update** - Document any issues found and resolutions

Once testing is complete and all endpoints are validated, we can proceed with confidence to frontend integration and full system testing.

---

*This testing plan ensures the AI Transparency Foundation backend is robust, performant, and ready for production use.*
