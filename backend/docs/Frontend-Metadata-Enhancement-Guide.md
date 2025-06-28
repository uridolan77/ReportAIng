# Frontend Guide: Metadata Enhancement Module

## Overview

The Metadata Enhancement Module provides AI-powered semantic enrichment for business metadata tables. This guide covers the new tuning endpoints that frontend developers can use to build user interfaces for metadata management.

## API Endpoints

### 1. Enhance Metadata Endpoint

**Endpoint:** `POST /api/Tuning/enhance-metadata`

**Purpose:** Enhances business metadata with AI-generated semantic information

#### Request Body

```json
{
  "entityType": "BusinessColumnInfo | BusinessTableInfo | BusinessGlossary",
  "mode": "EmptyFieldsOnly | AllFields",
  "batchSize": 50,
  "previewOnly": false
}
```

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `entityType` | string | Yes | Type of entity to enhance: `BusinessColumnInfo`, `BusinessTableInfo`, or `BusinessGlossary` |
| `mode` | string | Yes | Enhancement mode: `EmptyFieldsOnly` (only fill empty fields) or `AllFields` (improve all fields) |
| `batchSize` | integer | No | Number of records to process (default: 10, max: 100) |
| `previewOnly` | boolean | No | If true, shows what would be enhanced without saving (default: false) |

#### Response

```json
{
  "success": true,
  "message": "Enhanced 500 fields across 50 records",
  "columnsProcessed": 50,
  "tablesProcessed": 0,
  "glossaryTermsProcessed": 0,
  "fieldsEnhanced": 500,
  "totalCost": 0,
  "processingTime": "00:00:00.0619395",
  "warnings": {},
  "errors": {},
  "fieldEnhancementCounts": {}
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Whether the operation completed successfully |
| `message` | string | Human-readable summary of the operation |
| `columnsProcessed` | integer | Number of column records processed |
| `tablesProcessed` | integer | Number of table records processed |
| `glossaryTermsProcessed` | integer | Number of glossary terms processed |
| `fieldsEnhanced` | integer | Total number of individual fields enhanced |
| `totalCost` | decimal | Cost of AI processing (currently 0 for mock service) |
| `processingTime` | string | Time taken to complete the operation |
| `warnings` | object | Any warnings encountered during processing |
| `errors` | object | Any errors encountered during processing |

### 2. Get Enhancement Status

**Endpoint:** `GET /api/Tuning/enhancement-status`

**Purpose:** Get current status of metadata enhancement coverage

#### Response

```json
{
  "businessColumnInfo": {
    "totalRecords": 333,
    "enhancedRecords": 333,
    "coveragePercentage": 100.0,
    "lastEnhanced": "2024-06-28T11:57:10Z"
  },
  "businessTableInfo": {
    "totalRecords": 8,
    "enhancedRecords": 8,
    "coveragePercentage": 100.0,
    "lastEnhanced": "2024-06-28T11:57:10Z"
  },
  "businessGlossary": {
    "totalRecords": 28,
    "enhancedRecords": 28,
    "coveragePercentage": 100.0,
    "lastEnhanced": "2024-06-28T11:57:10Z"
  }
}
```

## Frontend Implementation Guide

### 1. Configuration Interface

Create a configuration form with the following components:

#### Entity Type Selection
```html
<select name="entityType" required>
  <option value="BusinessColumnInfo">Column Metadata</option>
  <option value="BusinessTableInfo">Table Metadata</option>
  <option value="BusinessGlossary">Business Glossary</option>
</select>
```

#### Enhancement Mode Selection
```html
<div class="radio-group">
  <input type="radio" name="mode" value="EmptyFieldsOnly" checked>
  <label>Fill Empty Fields Only (Recommended)</label>
  
  <input type="radio" name="mode" value="AllFields">
  <label>Enhance All Fields (Improve existing content)</label>
</div>
```

#### Batch Size Configuration
```html
<input type="number" name="batchSize" min="1" max="100" value="50">
<small>Number of records to process at once (1-100)</small>
```

#### Preview Mode Toggle
```html
<input type="checkbox" name="previewOnly">
<label>Preview Mode (Don't save changes)</label>
```

### 2. Progress Tracking

Since enhancement can process large datasets, implement progress tracking:

```javascript
async function enhanceMetadata(config) {
  const totalRecords = await getTotalRecords(config.entityType);
  let processedRecords = 0;
  
  while (processedRecords < totalRecords) {
    const response = await fetch('/api/Tuning/enhance-metadata', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(config)
    });
    
    const result = await response.json();
    
    if (result.success) {
      processedRecords += result[getProcessedField(config.entityType)];
      updateProgressBar(processedRecords, totalRecords);
      
      // If no records were processed, we're done
      if (result[getProcessedField(config.entityType)] === 0) {
        break;
      }
    } else {
      handleError(result);
      break;
    }
  }
}
```

### 3. Status Dashboard

Create a dashboard showing enhancement coverage:

```javascript
async function loadEnhancementStatus() {
  const response = await fetch('/api/Tuning/enhancement-status');
  const status = await response.json();
  
  // Update UI with coverage percentages
  updateCoverageChart(status);
}
```

### 4. Error Handling

Implement proper error handling for common scenarios:

```javascript
function handleEnhancementError(error) {
  if (error.status === 400) {
    showError('Invalid configuration. Please check your settings.');
  } else if (error.status === 500) {
    showError('Server error during enhancement. Please try again later.');
  } else {
    showError('An unexpected error occurred.');
  }
}
```

## UI/UX Recommendations

### 1. Enhancement Wizard

Create a step-by-step wizard:

1. **Step 1:** Select entity type and view current status
2. **Step 2:** Configure enhancement settings
3. **Step 3:** Preview changes (if preview mode enabled)
4. **Step 4:** Execute enhancement with progress tracking
5. **Step 5:** Show results and updated status

### 2. Status Indicators

Use visual indicators for enhancement status:

- 🟢 **100% Enhanced** - All records have semantic metadata
- 🟡 **Partially Enhanced** - Some records need enhancement
- 🔴 **Not Enhanced** - No semantic metadata available

### 3. Batch Processing UI

For large datasets, provide batch processing controls:

- **Batch Size Slider:** Allow users to adjust processing batch size
- **Pause/Resume:** Enable pausing long-running operations
- **Progress Bar:** Show real-time progress with ETA
- **Cancel Option:** Allow users to stop processing

### 4. Preview Mode

When preview mode is enabled:

- Show a sample of what would be enhanced
- Display before/after comparisons
- Provide "Apply Changes" button to execute actual enhancement

## Example Implementation

### React Component Example

```jsx
import React, { useState, useEffect } from 'react';

const MetadataEnhancementPanel = () => {
  const [config, setConfig] = useState({
    entityType: 'BusinessColumnInfo',
    mode: 'EmptyFieldsOnly',
    batchSize: 50,
    previewOnly: false
  });
  
  const [status, setStatus] = useState(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    loadStatus();
  }, []);

  const loadStatus = async () => {
    const response = await fetch('/api/Tuning/enhancement-status');
    setStatus(await response.json());
  };

  const handleEnhance = async () => {
    setIsProcessing(true);
    setProgress(0);
    
    try {
      const response = await fetch('/api/Tuning/enhance-metadata', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(config)
      });
      
      const result = await response.json();
      
      if (result.success) {
        setProgress(100);
        await loadStatus(); // Refresh status
        showSuccess(result.message);
      } else {
        showError('Enhancement failed');
      }
    } catch (error) {
      showError('Network error');
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="metadata-enhancement-panel">
      {/* Configuration form */}
      {/* Progress indicator */}
      {/* Status dashboard */}
      {/* Action buttons */}
    </div>
  );
};
```

## Security Considerations

- Ensure proper authentication for tuning endpoints
- Validate user permissions for metadata modification
- Implement rate limiting for enhancement operations
- Log all enhancement activities for audit purposes

## Performance Notes

- Enhancement operations are CPU-intensive
- Large batch sizes may impact server performance
- Consider running enhancements during off-peak hours
- Monitor server resources during bulk operations

## Testing

Test the enhancement functionality with:

1. **Small batches** (1-5 records) for quick validation
2. **Preview mode** to verify enhancement quality
3. **Different entity types** to ensure proper handling
4. **Error scenarios** (invalid configurations, network issues)
5. **Large datasets** to test performance and progress tracking

## Advanced Features

### 1. Bulk Enhancement Operations

For processing all entity types at once:

```javascript
async function enhanceAllMetadata() {
  const entityTypes = ['BusinessColumnInfo', 'BusinessTableInfo', 'BusinessGlossary'];
  const results = [];

  for (const entityType of entityTypes) {
    const result = await enhanceEntityType(entityType);
    results.push({ entityType, result });
  }

  return results;
}
```

### 2. Scheduled Enhancement

Implement scheduled enhancement for regular metadata updates:

```javascript
const scheduleConfig = {
  frequency: 'weekly', // daily, weekly, monthly
  entityTypes: ['BusinessColumnInfo'],
  mode: 'EmptyFieldsOnly',
  batchSize: 25
};
```

### 3. Enhancement Quality Metrics

Track enhancement quality over time:

```javascript
const qualityMetrics = {
  fieldsEnhanced: 1500,
  averageConfidenceScore: 0.85,
  userFeedbackScore: 4.2,
  lastQualityReview: '2024-06-28T12:00:00Z'
};
```

## Troubleshooting

### Common Issues

1. **"No records to enhance" message**
   - All records already have enhanced metadata
   - Try using "AllFields" mode to improve existing content

2. **Slow processing**
   - Reduce batch size (try 10-25 records)
   - Check server resources
   - Consider processing during off-peak hours

3. **Enhancement quality concerns**
   - Use preview mode to review changes before applying
   - Provide feedback through the quality review interface
   - Consider manual review for critical business terms

4. **Timeout errors**
   - Reduce batch size
   - Implement retry logic with exponential backoff
   - Check network connectivity

### Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| 400 | Invalid request parameters | Check entity type and mode values |
| 429 | Rate limit exceeded | Wait and retry with smaller batches |
| 500 | Server error | Contact system administrator |
| 503 | Service unavailable | AI service may be down, try later |

## Best Practices

1. **Start Small**: Begin with small batches to test configuration
2. **Use Preview**: Always preview changes for critical business data
3. **Monitor Progress**: Implement proper progress tracking for user experience
4. **Handle Errors**: Provide clear error messages and recovery options
5. **Regular Updates**: Schedule regular enhancement runs for new data
6. **Quality Review**: Implement feedback mechanisms for enhancement quality

## Integration Examples

### Vue.js Implementation

```vue
<template>
  <div class="enhancement-panel">
    <form @submit.prevent="startEnhancement">
      <select v-model="config.entityType">
        <option value="BusinessColumnInfo">Columns</option>
        <option value="BusinessTableInfo">Tables</option>
        <option value="BusinessGlossary">Glossary</option>
      </select>

      <input type="range" v-model="config.batchSize" min="1" max="100">
      <span>{{ config.batchSize }} records</span>

      <button type="submit" :disabled="isProcessing">
        {{ isProcessing ? 'Processing...' : 'Enhance Metadata' }}
      </button>
    </form>

    <div v-if="isProcessing" class="progress">
      <div class="progress-bar" :style="{ width: progress + '%' }"></div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      config: {
        entityType: 'BusinessColumnInfo',
        mode: 'EmptyFieldsOnly',
        batchSize: 50,
        previewOnly: false
      },
      isProcessing: false,
      progress: 0
    };
  },
  methods: {
    async startEnhancement() {
      this.isProcessing = true;
      // Implementation here
    }
  }
};
</script>
```

### Angular Service

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MetadataEnhancementService {
  private baseUrl = '/api/Tuning';

  constructor(private http: HttpClient) {}

  enhanceMetadata(config: EnhancementConfig): Observable<EnhancementResult> {
    return this.http.post<EnhancementResult>(`${this.baseUrl}/enhance-metadata`, config);
  }

  getEnhancementStatus(): Observable<EnhancementStatus> {
    return this.http.get<EnhancementStatus>(`${this.baseUrl}/enhancement-status`);
  }
}
```

## Conclusion

The Metadata Enhancement Module provides powerful AI-driven capabilities for enriching business metadata. By following this guide, frontend developers can create intuitive interfaces that allow users to easily configure and execute metadata enhancements, improving the overall quality and usability of the BI Reporting Copilot system.
