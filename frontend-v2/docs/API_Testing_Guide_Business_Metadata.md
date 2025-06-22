# API Testing Guide - Business Metadata Management

## 🎯 Overview
This guide provides practical examples for testing the enhanced Business Metadata Management APIs. All endpoints are live and ready for testing.

## 🔗 Base URL
```
http://localhost:55244/api/business-metadata
```

## 🔐 Authentication
All endpoints require authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## 📋 API Testing Examples

### 1. Get Business Tables with Pagination and Filtering

#### **GET** `/tables`
```bash
# Basic request
curl -X GET "http://localhost:55244/api/business-metadata/tables" \
  -H "Authorization: Bearer <token>"

# With pagination and filtering
curl -X GET "http://localhost:55244/api/business-metadata/tables?page=1&pageSize=10&search=customer&schema=dbo&domain=Sales" \
  -H "Authorization: Bearer <token>"
```

#### Expected Response:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "tableName": "Customers",
      "schemaName": "dbo",
      "businessPurpose": "Store customer information",
      "businessContext": "Central customer data repository",
      "primaryUseCase": "Customer management and analytics",
      "commonQueryPatterns": ["SELECT * FROM Customers WHERE CustomerID = ?"],
      "businessRules": "Customer ID must be unique",
      "domainClassification": "Sales",
      "naturalLanguageAliases": ["clients", "buyers", "customers"],
      "businessProcesses": ["Customer Onboarding", "Sales Process"],
      "analyticalUseCases": ["Customer Segmentation", "Churn Analysis"],
      "reportingCategories": ["Sales Reports", "Customer Analytics"],
      "vectorSearchKeywords": ["customer", "client", "buyer"],
      "businessGlossaryTerms": ["Customer", "Client"],
      "llmContextHints": ["This table contains customer demographic data"],
      "queryComplexityHints": ["Simple lookups", "Join with Orders table"],
      "isActive": true,
      "createdDate": "2024-01-15T10:30:00Z",
      "updatedDate": "2024-01-20T14:45:00Z",
      "createdBy": "admin",
      "updatedBy": "admin",
      "columns": [
        {
          "id": 1,
          "columnName": "CustomerID",
          "dataType": "int",
          "businessMeaning": "Unique identifier for customer",
          "dataExamples": ["1001", "1002", "1003"],
          "isKeyColumn": true,
          "isPII": false,
          "businessRules": "Auto-generated, unique",
          "validationRules": ["NOT NULL", "UNIQUE"],
          "semanticTags": ["identifier", "primary_key"],
          "llmContextHints": ["Use for customer lookups"]
        }
      ]
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 25,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "filters": {
    "search": "customer",
    "schema": "dbo",
    "domain": "Sales"
  }
}
```

### 2. Get Specific Business Table

#### **GET** `/tables/{id}`
```bash
curl -X GET "http://localhost:55244/api/business-metadata/tables/1" \
  -H "Authorization: Bearer <token>"
```

### 3. Create New Business Table

#### **POST** `/tables`
```bash
curl -X POST "http://localhost:55244/api/business-metadata/tables" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "Products",
    "schemaName": "dbo",
    "businessPurpose": "Store product catalog information",
    "businessContext": "Central product repository for e-commerce",
    "primaryUseCase": "Product management and catalog display",
    "commonQueryPatterns": [
      "SELECT * FROM Products WHERE CategoryID = ?",
      "SELECT * FROM Products WHERE Price BETWEEN ? AND ?"
    ],
    "businessRules": "Product code must be unique, price must be positive",
    "domainClassification": "Inventory",
    "naturalLanguageAliases": ["items", "merchandise", "goods"],
    "businessProcesses": ["Inventory Management", "Catalog Management"],
    "analyticalUseCases": ["Sales Analysis", "Inventory Optimization"],
    "reportingCategories": ["Product Reports", "Inventory Reports"],
    "vectorSearchKeywords": ["product", "item", "catalog", "inventory"],
    "businessGlossaryTerms": ["Product", "SKU", "Inventory"],
    "llmContextHints": ["Contains product details including pricing and categories"],
    "queryComplexityHints": ["Join with Categories table", "Price range queries common"]
  }'
```

### 4. Update Business Table

#### **PUT** `/tables/{id}`
```bash
curl -X PUT "http://localhost:55244/api/business-metadata/tables/1" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "businessPurpose": "Updated: Store comprehensive customer information",
    "businessContext": "Enhanced customer data repository with analytics",
    "primaryUseCase": "Customer management, analytics, and personalization",
    "commonQueryPatterns": [
      "SELECT * FROM Customers WHERE CustomerID = ?",
      "SELECT * FROM Customers WHERE Region = ? AND Status = ?"
    ],
    "businessRules": "Customer ID must be unique, email must be valid format",
    "domainClassification": "Sales",
    "naturalLanguageAliases": ["clients", "buyers", "customers", "patrons"],
    "businessProcesses": ["Customer Onboarding", "Sales Process", "Customer Support"],
    "analyticalUseCases": ["Customer Segmentation", "Churn Analysis", "Lifetime Value"],
    "reportingCategories": ["Sales Reports", "Customer Analytics", "Marketing Reports"],
    "vectorSearchKeywords": ["customer", "client", "buyer", "patron"],
    "businessGlossaryTerms": ["Customer", "Client", "Buyer"],
    "llmContextHints": ["Contains customer demographic and behavioral data"],
    "queryComplexityHints": ["Simple lookups", "Join with Orders and Transactions"],
    "isActive": true
  }'
```

### 5. Advanced Search

#### **POST** `/tables/search`
```bash
curl -X POST "http://localhost:55244/api/business-metadata/tables/search" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "searchQuery": "customer sales analytics",
    "schemas": ["dbo", "sales"],
    "domains": ["Sales", "Marketing"],
    "tags": ["analytics", "reporting"],
    "includeColumns": true,
    "includeGlossaryTerms": true,
    "maxResults": 20,
    "minRelevanceScore": 0.3
  }'
```

### 6. Bulk Operations

#### **POST** `/tables/bulk`
```bash
# Bulk activate tables
curl -X POST "http://localhost:55244/api/business-metadata/tables/bulk" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "tableIds": [1, 2, 3],
    "operation": "Activate"
  }'

# Bulk delete tables
curl -X POST "http://localhost:55244/api/business-metadata/tables/bulk" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "tableIds": [4, 5],
    "operation": "Delete"
  }'
```

#### Expected Response:
```json
{
  "success": true,
  "message": "Bulk operation completed: 3 successful, 0 errors",
  "summary": {
    "operation": "Activate",
    "totalProcessed": 3,
    "successful": 3,
    "errors": 0
  },
  "results": [
    {
      "tableId": 1,
      "success": true,
      "message": "Activated successfully"
    },
    {
      "tableId": 2,
      "success": true,
      "message": "Activated successfully"
    },
    {
      "tableId": 3,
      "success": true,
      "message": "Activated successfully"
    }
  ]
}
```

### 7. Validate Business Table

#### **POST** `/tables/validate`
```bash
curl -X POST "http://localhost:55244/api/business-metadata/tables/validate" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "tableId": 1,
    "validateBusinessRules": true,
    "validateDataQuality": true,
    "validateRelationships": true
  }'
```

#### Expected Response:
```json
{
  "success": true,
  "data": {
    "isValid": false,
    "issues": [
      {
        "type": "MissingBusinessPurpose",
        "message": "Business purpose is required",
        "severity": "Error",
        "field": "BusinessPurpose"
      }
    ],
    "warnings": [
      {
        "type": "MissingBusinessContext",
        "message": "Business context is recommended for better AI query generation",
        "field": "BusinessContext"
      }
    ],
    "suggestions": [
      {
        "type": "MissingDomainClassification",
        "message": "Domain classification helps with query optimization",
        "recommendedAction": "Add domain classification (e.g., Sales, Finance, HR)"
      }
    ],
    "validatedAt": "2024-01-20T15:30:00Z"
  }
}
```

### 8. Get Statistics

#### **GET** `/statistics`
```bash
curl -X GET "http://localhost:55244/api/business-metadata/statistics" \
  -H "Authorization: Bearer <token>"
```

#### Expected Response:
```json
{
  "success": true,
  "data": {
    "totalTables": 25,
    "populatedTables": 20,
    "tablesWithAIMetadata": 15,
    "tablesWithRuleBasedMetadata": 18,
    "totalColumns": 150,
    "populatedColumns": 120,
    "totalGlossaryTerms": 45,
    "lastPopulationRun": "2024-01-20T10:00:00Z",
    "tablesByDomain": {
      "Sales": 8,
      "Finance": 5,
      "HR": 3,
      "Operations": 4,
      "Marketing": 5
    },
    "tablesBySchema": {
      "dbo": 15,
      "sales": 5,
      "finance": 3,
      "hr": 2
    },
    "mostActiveUsers": ["admin", "john.doe", "jane.smith"],
    "averageMetadataCompleteness": 78.5
  }
}
```

### 9. Delete Business Table

#### **DELETE** `/tables/{id}`
```bash
curl -X DELETE "http://localhost:55244/api/business-metadata/tables/1" \
  -H "Authorization: Bearer <token>"
```

#### Expected Response:
```json
{
  "success": true,
  "message": "Business table deleted successfully"
}
```

## 🧪 Testing Scenarios

### Scenario 1: Complete CRUD Workflow
1. **Create** a new business table
2. **Read** the created table
3. **Update** the table metadata
4. **Validate** the updated table
5. **Delete** the table

### Scenario 2: Search and Filter Workflow
1. **Get all tables** with pagination
2. **Filter by schema** and domain
3. **Search** with text query
4. **Advanced search** with multiple criteria

### Scenario 3: Bulk Operations Workflow
1. **Create multiple tables**
2. **Select tables** for bulk operation
3. **Perform bulk activation**
4. **Verify results**

### Scenario 4: Validation Workflow
1. **Create table** with incomplete metadata
2. **Validate** the table
3. **Review issues** and warnings
4. **Update** based on suggestions
5. **Re-validate** to confirm fixes

## 🔍 Error Handling Examples

### 400 Bad Request
```json
{
  "error": "Validation failed",
  "details": "TableName is required"
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized access"
}
```

### 404 Not Found
```json
{
  "error": "Business table with ID 999 not found"
}
```

### 500 Internal Server Error
```json
{
  "error": "Failed to retrieve business tables",
  "details": "Database connection timeout"
}
```

## 📊 Performance Testing

### Load Testing Recommendations
- **Concurrent users**: Test with 10-50 concurrent users
- **Large datasets**: Test pagination with 1000+ tables
- **Bulk operations**: Test with 100+ tables in bulk
- **Search performance**: Test complex search queries
- **Response times**: Ensure < 2 seconds for most operations

## ✅ Integration Checklist

- [ ] Authentication working with JWT tokens
- [ ] All CRUD operations functional
- [ ] Pagination working correctly
- [ ] Filtering and search operational
- [ ] Bulk operations processing correctly
- [ ] Validation providing meaningful feedback
- [ ] Statistics displaying accurate data
- [ ] Error handling working as expected
- [ ] Performance meeting requirements
- [ ] API responses matching documentation

## 🚀 Ready for Frontend Integration

All APIs are tested, documented, and ready for frontend integration. The backend is stable and handles all the specified use cases effectively.
