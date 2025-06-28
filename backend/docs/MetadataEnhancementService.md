# Metadata Enhancement Service

## Overview

The MetadataEnhancementService is a simplified service designed to enhance existing business metadata by populating empty semantic fields in the database. This service provides a foundation for improving the semantic layer of the BI Reporting Copilot system.

## Features

- **Empty Field Population**: Automatically fills empty semantic fields with appropriate default values
- **Batch Processing**: Processes records in configurable batches for performance
- **Preview Mode**: Allows previewing changes without committing them to the database
- **Multiple Entity Support**: Enhances BusinessColumnInfo, BusinessTableInfo, and BusinessGlossary entities
- **Comprehensive Logging**: Detailed logging for monitoring and debugging

## API Endpoints

### 1. Enhance Metadata
```http
POST /api/MetadataEnhancement/enhance
```

**Request Body:**
```json
{
  "mode": "EmptyFieldsOnly",
  "targetTables": ["tbl_Daily_actions", "tbl_Currencies"],
  "targetFields": ["SemanticContext", "AnalyticalContext"],
  "batchSize": 50,
  "qualityThreshold": 0.8,
  "previewOnly": false,
  "costBudget": 10.00
}
```

**Response:**
```json
{
  "success": true,
  "message": "Enhanced 156 fields across 78 records",
  "columnsProcessed": 45,
  "tablesProcessed": 12,
  "glossaryTermsProcessed": 21,
  "fieldsEnhanced": 156,
  "totalCost": 0.00,
  "processingTime": "00:00:15.234",
  "warnings": [],
  "errors": [],
  "fieldEnhancementCounts": {
    "SemanticContext": 45,
    "AnalyticalContext": 45,
    "SemanticRelevanceScore": 45
  }
}
```

### 2. Preview Enhancement
```http
POST /api/MetadataEnhancement/preview
```

Same request format as enhance, but automatically sets `previewOnly: true`.

### 3. Get Enhancement Status
```http
GET /api/MetadataEnhancement/status
```

**Response:**
```json
{
  "serviceStatus": "Available",
  "lastEnhancement": "2024-01-15T10:30:00Z",
  "totalEnhancements": 156,
  "availableModes": ["EmptyFieldsOnly", "Enhancement", "Selective"],
  "supportedEntities": ["BusinessColumnInfo", "BusinessTableInfo", "BusinessGlossary"]
}
```

### 4. Get Enhancement Modes
```http
GET /api/MetadataEnhancement/modes
```

**Response:**
```json
{
  "availableModes": [
    {
      "mode": "EmptyFieldsOnly",
      "description": "Only populate empty semantic fields (recommended for initial setup)",
      "isDefault": true,
      "riskLevel": "Low"
    },
    {
      "mode": "Enhancement",
      "description": "Improve existing content in addition to filling empty fields",
      "isDefault": false,
      "riskLevel": "Medium"
    },
    {
      "mode": "Selective",
      "description": "Target specific fields or tables for enhancement",
      "isDefault": false,
      "riskLevel": "Low"
    }
  ],
  "recommendedMode": "EmptyFieldsOnly",
  "defaultBatchSize": 50
}
```

## Enhancement Modes

### EmptyFieldsOnly (Recommended)
- Only populates fields that are currently empty or null
- Safest option with minimal risk of overwriting existing data
- Ideal for initial setup and filling gaps in metadata

### Enhancement
- Improves existing content in addition to filling empty fields
- Higher risk but potentially better quality results
- Use with caution on production data

### Selective
- Allows targeting specific tables or fields for enhancement
- Provides fine-grained control over the enhancement process
- Good for focused improvements

## Enhanced Fields

### BusinessColumnInfo
- `SemanticContext`: Contextual meaning within business domain
- `AnalyticalContext`: Context for analytical usage
- `SemanticRelevanceScore`: Business relevance score (0.0-1.0)

### BusinessTableInfo
- `SemanticDescription`: Rich semantic description for LLM understanding
- `LLMContextHints`: Hints for LLM processing (JSON array)

### BusinessGlossary
- `ContextualVariations`: Context-dependent variations
- `QueryPatterns`: Common query patterns using the term

## Usage Examples

### Basic Enhancement
```bash
curl -X POST "https://localhost:7001/api/MetadataEnhancement/enhance" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "mode": "EmptyFieldsOnly",
    "batchSize": 25
  }'
```

### Preview Changes
```bash
curl -X POST "https://localhost:7001/api/MetadataEnhancement/preview" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "mode": "EmptyFieldsOnly",
    "targetTables": ["tbl_Daily_actions"]
  }'
```

### Selective Enhancement
```bash
curl -X POST "https://localhost:7001/api/MetadataEnhancement/enhance" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "mode": "Selective",
    "targetFields": ["SemanticContext", "AnalyticalContext"],
    "batchSize": 10
  }'
```

## Configuration

The service is automatically registered in the DI container and requires:
- `BICopilotContext`: Database context for accessing metadata tables
- `ILogger<MetadataEnhancementService>`: Logging service

## Current Implementation

This is a **simplified demonstration version** that:
- Populates basic semantic fields with default values
- Provides the infrastructure for future LLM-powered enhancements
- Focuses on safety and reliability over advanced AI features

## Future Enhancements

The service is designed to be extended with:
- **LLM Integration**: Use AI models to generate intelligent semantic content
- **Vector Embeddings**: Generate embeddings for semantic search
- **Quality Scoring**: Advanced algorithms for content quality assessment
- **Batch Optimization**: Intelligent batching based on content complexity
- **Cost Management**: Integration with LLM cost tracking and budgeting

## Error Handling

The service includes comprehensive error handling:
- Database connection issues
- Invalid request parameters
- Processing timeouts
- Batch size limitations

All errors are logged and returned in a structured format for easy debugging.

## Security

- Requires authentication (Bearer token)
- Validates user permissions
- Logs all enhancement activities for audit trails
- Supports preview mode to prevent accidental changes

## Performance Considerations

- Default batch size: 50 records
- Configurable batch sizes for different workloads
- Async processing for better responsiveness
- Comprehensive logging for performance monitoring

## Monitoring

The service provides detailed metrics:
- Processing time per batch
- Number of fields enhanced
- Success/failure rates
- Cost tracking (for future LLM integration)
