# Metadata Enhancement API Testing Guide

## Overview
This guide provides test cases for the updated metadata enhancement endpoints that now match the quick reference guide specifications.

## Updated Endpoints

### 1. Enhanced Metadata Endpoint
**URL:** `POST /api/Tuning/enhance-metadata`

**New Request Format:**
```json
{
  "entityType": "BusinessColumnInfo",
  "mode": "EmptyFieldsOnly", 
  "batchSize": 50,
  "previewOnly": false
}
```

**Response Format:**
```json
{
  "success": true,
  "message": "Enhanced 500 fields across 50 records",
  "columnsProcessed": 50,
  "fieldsEnhanced": 500,
  "processingTime": "00:00:00.0619395"
}
```

### 2. Enhancement Status Endpoint (NEW)
**URL:** `GET /api/Tuning/enhancement-status`

**Response Format:**
```json
{
  "businessColumnInfo": {
    "totalRecords": 333,
    "enhancedRecords": 333,
    "coveragePercentage": 100.0
  },
  "businessTableInfo": {
    "totalRecords": 8,
    "enhancedRecords": 8,
    "coveragePercentage": 100.0
  },
  "businessGlossary": {
    "totalRecords": 28,
    "enhancedRecords": 28,
    "coveragePercentage": 100.0
  }
}
```

## Test Cases

### Test 1: BusinessColumnInfo Enhancement
```bash
curl -X POST "https://localhost:55244/api/Tuning/enhance-metadata" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "entityType": "BusinessColumnInfo",
    "mode": "EmptyFieldsOnly",
    "batchSize": 25,
    "previewOnly": false
  }'
```

### Test 2: BusinessTableInfo Enhancement
```bash
curl -X POST "https://localhost:55244/api/Tuning/enhance-metadata" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "entityType": "BusinessTableInfo",
    "mode": "EmptyFieldsOnly",
    "batchSize": 10,
    "previewOnly": false
  }'
```

### Test 3: BusinessGlossary Enhancement
```bash
curl -X POST "https://localhost:55244/api/Tuning/enhance-metadata" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "entityType": "BusinessGlossary",
    "mode": "EmptyFieldsOnly",
    "batchSize": 15,
    "previewOnly": false
  }'
```

### Test 4: Preview Mode
```bash
curl -X POST "https://localhost:55244/api/Tuning/enhance-metadata" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "entityType": "BusinessColumnInfo",
    "mode": "EmptyFieldsOnly",
    "batchSize": 5,
    "previewOnly": true
  }'
```

### Test 5: Enhancement Status
```bash
curl -X GET "https://localhost:55244/api/Tuning/enhancement-status" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test 6: All Fields Mode
```bash
curl -X POST "https://localhost:55244/api/Tuning/enhance-metadata" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "entityType": "BusinessColumnInfo",
    "mode": "AllFields",
    "batchSize": 10,
    "previewOnly": true
  }'
```

## Expected Behaviors

### Entity Type Processing
- **BusinessColumnInfo**: Should process columns with empty semantic fields
- **BusinessTableInfo**: Should process tables with empty semantic descriptions
- **BusinessGlossary**: Should process glossary terms with empty contextual variations

### Mode Processing
- **EmptyFieldsOnly**: Only fills empty fields, preserves existing data
- **AllFields**: Improves all fields including existing ones

### Preview Mode
- **previewOnly: true**: Shows what would be changed without saving
- **previewOnly: false**: Actually saves changes to database

## Validation Checklist

- [ ] EntityType parameter is properly recognized
- [ ] Mode parameter controls enhancement behavior
- [ ] BatchSize parameter limits processing
- [ ] PreviewOnly parameter prevents database changes
- [ ] Enhancement status endpoint returns accurate counts
- [ ] Response format matches quick reference guide
- [ ] Error handling works for invalid entity types
- [ ] Authentication is properly enforced

## Implementation Changes Made

1. **Updated MetadataEnhancementRequest**: Added EntityType enum and parameter
2. **Modified Enhancement Logic**: Now processes based on entityType instead of all entities
3. **Added Enhancement Status**: New endpoint with coverage statistics
4. **Updated Response Format**: Matches quick reference guide specifications
5. **Simplified Modes**: Removed 'Selective' mode, kept 'EmptyFieldsOnly' and 'AllFields'

## Notes

- The implementation maintains backward compatibility with legacy properties
- All endpoints use the existing TuningController authentication
- Status endpoint provides real-time coverage statistics
- Entity-specific processing improves performance and control
