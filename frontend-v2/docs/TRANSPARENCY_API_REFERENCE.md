# 📚 Transparency API Reference

## 🌐 **Base URL**
```
http://localhost:55244/api/transparency
```

## 🔐 **Authentication**
All endpoints require JWT Bearer token:
```
Authorization: Bearer <your-jwt-token>
```

---

## 📋 **Core Analysis Endpoints**

### **1. Get Confidence Breakdown**
```http
GET /api/transparency/confidence/{analysisId}
```

**Response:**
```json
{
  "analysisId": "trace-123",
  "overallConfidence": 0.85,
  "factorBreakdown": {
    "Intent Classification": 0.92,
    "Domain Understanding": 0.78
  },
  "confidenceFactors": [
    {
      "factorName": "Intent Classification",
      "score": 0.92,
      "weight": 0.4,
      "description": "Confidence in understanding user intent"
    }
  ],
  "timestamp": "2024-12-23T10:30:00Z"
}
```

### **2. Get Alternative Options**
```http
GET /api/transparency/alternatives/{traceId}
```

**Response:**
```json
[
  {
    "optionId": "alt-001",
    "type": "Template",
    "description": "Use analytical template instead of reporting template",
    "score": 0.82,
    "rationale": "Better suited for complex analytical queries",
    "estimatedImprovement": 15.0
  }
]
```

### **3. Get Optimization Suggestions**
```http
POST /api/transparency/optimize
Content-Type: application/json

{
  "userQuery": "Show me sales data for Q3 2024",
  "traceId": "trace-123",
  "priority": "Balanced"
}
```

**Response:**
```json
[
  {
    "suggestionId": "opt-001",
    "type": "Token Optimization",
    "title": "Reduce context verbosity",
    "description": "Remove redundant schema information to save tokens",
    "priority": "Medium",
    "estimatedTokenSaving": 150,
    "estimatedPerformanceGain": 12.0,
    "implementation": "Filter out less relevant table columns"
  }
]
```

---

## 📊 **Metrics & Analytics Endpoints**

### **4. Get Transparency Metrics**
```http
GET /api/transparency/metrics?userId={userId}&days={days}
```

**Response:**
```json
{
  "totalAnalyses": 156,
  "averageConfidence": 0.84,
  "confidenceDistribution": {
    "High (>0.8)": 98,
    "Medium (0.6-0.8)": 45,
    "Low (<0.6)": 13
  },
  "topIntentTypes": {
    "Analytical": 67,
    "Reporting": 45
  },
  "optimizationImpact": {
    "Token Savings": 23.5,
    "Performance Improvement": 18.2
  },
  "timeRange": {
    "from": "2024-12-16T00:00:00Z",
    "to": "2024-12-23T00:00:00Z"
  }
}
```

### **5. Get Dashboard Metrics**
```http
GET /api/transparency/metrics/dashboard?days={days}
```

**Response:**
```json
{
  "totalTraces": 156,
  "averageConfidence": 0.84,
  "topOptimizations": [
    {
      "suggestionId": "opt-001",
      "type": "Token Optimization",
      "title": "Reduce schema verbosity",
      "priority": "High",
      "estimatedTokenSaving": 200,
      "estimatedPerformanceGain": 18.5
    }
  ],
  "confidenceTrends": [
    {
      "date": "2024-12-23",
      "confidence": 0.85,
      "traceCount": 12
    }
  ],
  "usageByModel": [
    {
      "model": "gpt-4",
      "count": 120,
      "averageConfidence": 0.87,
      "totalTokens": 45000
    }
  ]
}
```

### **6. Get Confidence Trends**
```http
GET /api/transparency/analytics/confidence-trends?userId={userId}&days={days}
```

### **7. Get Token Usage Analytics**
```http
GET /api/transparency/analytics/token-usage?userId={userId}&days={days}
```

### **8. Get Performance Metrics**
```http
GET /api/transparency/analytics/performance?days={days}
```

---

## 🗂️ **Traces & History Endpoints**

### **9. Get Recent Traces**
```http
GET /api/transparency/traces/recent?userId={userId}&limit={limit}
```

**Response:**
```json
[
  {
    "traceId": "trace-123",
    "userId": "user@example.com",
    "userQuestion": "Show me sales data for Q3 2024",
    "intentType": "Analytical",
    "overallConfidence": 0.85,
    "totalTokens": 2500,
    "success": true,
    "createdAt": "2024-12-23T10:30:00Z"
  }
]
```

### **10. Get Trace Detail**
```http
GET /api/transparency/traces/{traceId}/detail
```

**Response:**
```json
{
  "traceId": "trace-123",
  "userId": "user@example.com",
  "userQuestion": "Show me sales data for Q3 2024",
  "intentType": "Analytical",
  "overallConfidence": 0.85,
  "totalTokens": 2500,
  "success": true,
  "createdAt": "2024-12-23T10:30:00Z",
  "steps": [
    {
      "id": "step-001",
      "stepName": "Intent Analysis",
      "stepOrder": 1,
      "startTime": "2024-12-23T10:30:00Z",
      "endTime": "2024-12-23T10:30:05Z",
      "success": true,
      "tokensAdded": 150,
      "confidence": 0.92,
      "content": "Analyzed user intent as analytical query",
      "details": {}
    }
  ],
  "finalPrompt": "Based on the user's analytical query...",
  "metadata": {}
}
```

---

## ⚙️ **Settings & Configuration Endpoints**

### **11. Get Transparency Settings**
```http
GET /api/transparency/settings
```

**Response:**
```json
{
  "enableDetailedLogging": true,
  "confidenceThreshold": 0.7,
  "retentionDays": 30,
  "enableOptimizationSuggestions": true
}
```

### **12. Update Transparency Settings**
```http
PUT /api/transparency/settings
Content-Type: application/json

{
  "enableDetailedLogging": true,
  "confidenceThreshold": 0.8,
  "retentionDays": 45,
  "enableOptimizationSuggestions": true
}
```

---

## 📤 **Export Endpoints**

### **13. Export Transparency Data**
```http
POST /api/transparency/export
Content-Type: application/json

{
  "format": "json",
  "startDate": "2024-12-16T00:00:00Z",
  "endDate": "2024-12-23T00:00:00Z",
  "userId": "user@example.com"
}
```

**Response:** Binary file download (JSON/CSV/Excel)

---

## 🔄 **Real-time SignalR Hub**

### **Hub URL**
```
/hubs/transparency
```

### **Client Methods (Incoming Events)**
```typescript
// Subscribe to trace updates
connection.on('TransparencyUpdate', (update) => {
  console.log('Transparency update:', update);
});

// Step completion notifications
connection.on('StepCompleted', (data) => {
  console.log('Step completed:', data.step);
});

// Real-time confidence changes
connection.on('ConfidenceUpdate', (data) => {
  console.log('Confidence updated:', data.confidence);
});

// Trace completion
connection.on('TraceCompleted', (data) => {
  console.log('Trace completed:', data.trace);
});

// Error notifications
connection.on('TransparencyError', (data) => {
  console.error('Transparency error:', data.error);
});
```

### **Server Methods (Outgoing Events)**
```typescript
// Subscribe to specific trace updates
await connection.invoke('SubscribeToTrace', traceId);

// Unsubscribe from trace updates
await connection.invoke('UnsubscribeFromTrace', traceId);

// Get current status
await connection.invoke('GetTransparencyStatus');
```

---

## 🧪 **Testing Endpoints**

### **Quick Test Script**
```bash
# Get auth token
curl -X POST http://localhost:55244/api/test/auth/token \
  -H "Content-Type: application/json" \
  -d '{"userId":"test@example.com","userName":"Test User"}'

# Test transparency metrics (replace TOKEN with actual token)
curl -X GET "http://localhost:55244/api/transparency/metrics?days=7" \
  -H "Authorization: Bearer TOKEN"

# Test transparency settings
curl -X GET "http://localhost:55244/api/transparency/settings" \
  -H "Authorization: Bearer TOKEN"
```

---

## ✅ **Status: All Endpoints Live**

- ✅ **15+ Endpoints** - All implemented and tested
- ✅ **Real Data** - No mock data, all database-driven
- ✅ **Authentication** - JWT tokens working
- ✅ **Real-time** - SignalR hub functional
- ✅ **Error Handling** - Comprehensive error responses
- ✅ **Documentation** - Swagger UI available

**Ready for frontend integration!** 🚀
