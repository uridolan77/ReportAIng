# Metadata Enhancement - Quick Reference

## API Endpoints

### Enhance Metadata
```
POST /api/Tuning/enhance-metadata
```

**Request:**
```json
{
  "entityType": "BusinessColumnInfo",
  "mode": "EmptyFieldsOnly", 
  "batchSize": 50,
  "previewOnly": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Enhanced 500 fields across 50 records",
  "columnsProcessed": 50,
  "fieldsEnhanced": 500,
  "processingTime": "00:00:00.0619395"
}
```

### Get Status
```
GET /api/Tuning/enhancement-status
```

**Response:**
```json
{
  "businessColumnInfo": {
    "totalRecords": 333,
    "enhancedRecords": 333,
    "coveragePercentage": 100.0
  }
}
```

## Entity Types

| Type | Description | Typical Count |
|------|-------------|---------------|
| `BusinessColumnInfo` | Column metadata | 300+ records |
| `BusinessTableInfo` | Table metadata | 5-10 records |
| `BusinessGlossary` | Business terms | 20-50 records |

## Enhancement Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| `EmptyFieldsOnly` | Fill only empty fields | Initial setup, new data |
| `AllFields` | Improve all fields | Quality improvement |

## Configuration Guidelines

### Batch Sizes
- **Small datasets** (< 50 records): Use batch size 10-25
- **Medium datasets** (50-200 records): Use batch size 25-50  
- **Large datasets** (200+ records): Use batch size 50-100

### Processing Strategy
1. Start with `BusinessGlossary` (smallest dataset)
2. Process `BusinessTableInfo` (medium dataset)
3. Process `BusinessColumnInfo` in batches (largest dataset)

## JavaScript Examples

### Basic Enhancement
```javascript
const config = {
  entityType: 'BusinessColumnInfo',
  mode: 'EmptyFieldsOnly',
  batchSize: 50,
  previewOnly: false
};

const response = await fetch('/api/Tuning/enhance-metadata', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(config)
});

const result = await response.json();
console.log(`Enhanced ${result.fieldsEnhanced} fields`);
```

### Batch Processing Loop
```javascript
async function enhanceAllRecords(entityType) {
  let totalProcessed = 0;
  
  while (true) {
    const result = await enhanceMetadata({
      entityType,
      mode: 'EmptyFieldsOnly',
      batchSize: 50
    });
    
    if (!result.success || result.columnsProcessed === 0) {
      break;
    }
    
    totalProcessed += result.columnsProcessed;
    console.log(`Processed ${totalProcessed} records so far...`);
  }
  
  return totalProcessed;
}
```

### Progress Tracking
```javascript
async function enhanceWithProgress(entityType, onProgress) {
  const status = await getEnhancementStatus();
  const total = status[entityType].totalRecords;
  let processed = 0;
  
  while (processed < total) {
    const result = await enhanceMetadata({
      entityType,
      mode: 'EmptyFieldsOnly',
      batchSize: 25
    });
    
    if (result.success) {
      processed += result.columnsProcessed;
      onProgress(processed, total);
      
      if (result.columnsProcessed === 0) break;
    }
  }
}
```

## UI Components

### Configuration Form
```html
<form id="enhancement-form">
  <select name="entityType" required>
    <option value="BusinessColumnInfo">Column Metadata</option>
    <option value="BusinessTableInfo">Table Metadata</option>
    <option value="BusinessGlossary">Business Glossary</option>
  </select>
  
  <fieldset>
    <legend>Enhancement Mode</legend>
    <input type="radio" name="mode" value="EmptyFieldsOnly" checked>
    <label>Fill Empty Fields Only</label>
    
    <input type="radio" name="mode" value="AllFields">
    <label>Enhance All Fields</label>
  </fieldset>
  
  <label>
    Batch Size: 
    <input type="range" name="batchSize" min="1" max="100" value="50">
    <span class="batch-size-display">50</span>
  </label>
  
  <label>
    <input type="checkbox" name="previewOnly">
    Preview Mode (don't save changes)
  </label>
  
  <button type="submit">Start Enhancement</button>
</form>
```

### Progress Indicator
```html
<div class="progress-container" style="display: none;">
  <div class="progress-bar">
    <div class="progress-fill" style="width: 0%"></div>
  </div>
  <div class="progress-text">0 / 0 records processed</div>
  <div class="progress-eta">Estimated time remaining: --</div>
</div>
```

### Status Dashboard
```html
<div class="status-dashboard">
  <div class="status-card">
    <h3>Column Metadata</h3>
    <div class="coverage-circle" data-percentage="100">
      <span class="percentage">100%</span>
    </div>
    <p>333 / 333 enhanced</p>
  </div>
  
  <div class="status-card">
    <h3>Table Metadata</h3>
    <div class="coverage-circle" data-percentage="100">
      <span class="percentage">100%</span>
    </div>
    <p>8 / 8 enhanced</p>
  </div>
  
  <div class="status-card">
    <h3>Business Glossary</h3>
    <div class="coverage-circle" data-percentage="100">
      <span class="percentage">100%</span>
    </div>
    <p>28 / 28 enhanced</p>
  </div>
</div>
```

## Error Handling

### Common Error Responses
```javascript
// 400 Bad Request
{
  "success": false,
  "message": "Invalid entity type specified",
  "errors": { "entityType": "Must be BusinessColumnInfo, BusinessTableInfo, or BusinessGlossary" }
}

// 500 Internal Server Error  
{
  "success": false,
  "message": "Enhancement service temporarily unavailable",
  "errors": { "service": "AI enhancement service is down" }
}
```

### Error Handling Pattern
```javascript
async function handleEnhancement(config) {
  try {
    const response = await fetch('/api/Tuning/enhance-metadata', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(config)
    });
    
    const result = await response.json();
    
    if (!response.ok) {
      throw new Error(result.message || 'Enhancement failed');
    }
    
    return result;
  } catch (error) {
    console.error('Enhancement error:', error);
    showErrorMessage(error.message);
    throw error;
  }
}
```

## CSS Styling Examples

### Progress Bar
```css
.progress-container {
  margin: 20px 0;
}

.progress-bar {
  width: 100%;
  height: 20px;
  background-color: #f0f0f0;
  border-radius: 10px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #4CAF50, #45a049);
  transition: width 0.3s ease;
}
```

### Status Cards
```css
.status-dashboard {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin: 20px 0;
}

.status-card {
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  text-align: center;
}

.coverage-circle {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  border: 4px solid #4CAF50;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 10px auto;
}
```

## Testing Checklist

- [ ] Test with small batch sizes (1-5 records)
- [ ] Test preview mode functionality
- [ ] Test all entity types
- [ ] Test error scenarios (invalid config, network issues)
- [ ] Test progress tracking with large datasets
- [ ] Verify enhancement quality in preview mode
- [ ] Test cancellation/interruption handling
- [ ] Verify status dashboard updates correctly

## Performance Tips

1. **Use appropriate batch sizes** based on dataset size
2. **Implement progress tracking** for better UX
3. **Add cancellation support** for long operations
4. **Cache status data** to reduce API calls
5. **Debounce user inputs** in configuration forms
6. **Show loading states** during processing
7. **Implement retry logic** for failed requests
