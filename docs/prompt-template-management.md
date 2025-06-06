# Prompt Template Management

This document describes the new Prompt Template Management feature added to the AI Tuning interface.

## Overview

The Prompt Template Management feature allows administrators to create, edit, version, and manage AI prompt templates through a user-friendly interface instead of running SQL scripts manually.

## Features

### 1. Template Management
- **Create Templates**: Create new prompt templates with custom content and parameters
- **Edit Templates**: Modify existing template content, descriptions, and settings
- **Version Control**: Create new versions of templates while maintaining history
- **Activate/Deactivate**: Control which template version is active for each template name

### 2. Template Testing
- **Test Interface**: Test templates with sample data before deployment
- **Variable Replacement**: See how placeholders are replaced with actual values
- **Performance Metrics**: View token count and processing time

### 3. Template Versioning
- **Version History**: View all versions of a template with creation dates and authors
- **Version Comparison**: Compare different versions (future enhancement)
- **Rollback Capability**: Easily activate previous versions

### 4. Usage Analytics
- **Usage Statistics**: Track how often templates are used
- **Success Rates**: Monitor template performance over time
- **Performance Metrics**: View processing times and token counts

## User Interface

### Main Dashboard
The Prompt Templates tab in the AI Tuning interface provides:
- Overview statistics (total templates, versions, active templates)
- Template list with version information
- Quick actions for common operations

### Template Editor
- Rich text editor for template content
- JSON editor for parameters
- Validation for template syntax
- Preview functionality

### Version Management
- Expandable rows showing all versions
- Version-specific actions (activate, edit, delete)
- Visual indicators for active versions

## API Endpoints

### Template CRUD Operations
- `GET /api/tuning/prompt-templates` - List all templates
- `GET /api/tuning/prompt-templates/{id}` - Get specific template
- `POST /api/tuning/prompt-templates` - Create new template
- `PUT /api/tuning/prompt-templates/{id}` - Update template
- `DELETE /api/tuning/prompt-templates/{id}` - Delete template

### Template Management
- `POST /api/tuning/prompt-templates/{id}/activate` - Activate template version
- `POST /api/tuning/prompt-templates/{id}/deactivate` - Deactivate template version
- `POST /api/tuning/prompt-templates/{id}/test` - Test template with sample data

## Database Schema

### PromptTemplates Table
```sql
CREATE TABLE [dbo].[PromptTemplates] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Version] NVARCHAR(20) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedBy] NVARCHAR(256) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedDate] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(256) NULL,
    [SuccessRate] DECIMAL(5,2) NULL,
    [UsageCount] INT NOT NULL DEFAULT 0,
    [Parameters] NVARCHAR(MAX) NULL,
    UNIQUE (Name, Version),
    INDEX IX_PromptTemplates_Active (IsActive, Name)
);
```

## Template Structure

### Template Placeholders
Templates support the following standard placeholders:
- `{schema}` - Database schema information
- `{question}` - User's natural language question
- `{context}` - Additional context information
- `{business_rules}` - Business rules and constraints
- `{examples}` - Example queries

### Custom Parameters
Templates can include custom parameters defined in JSON format:
```json
{
  "temperature": 0.7,
  "max_tokens": 1000,
  "model": "gpt-4"
}
```

## Usage Examples

### Creating a New Template
1. Navigate to AI Tuning → Prompt Templates
2. Click "Create Template"
3. Fill in template details:
   - Name: `sql_generation`
   - Version: `2.3`
   - Content: Your prompt template with placeholders
   - Description: Brief description of changes
4. Set parameters if needed
5. Save the template

### Testing a Template
1. Find the template in the list
2. Click the test button (bug icon)
3. Provide test data:
   - Question: Sample natural language query
   - Schema: Sample schema information
   - Context: Additional context
4. Review the processed prompt and metrics

### Version Management
1. Expand a template row to see all versions
2. Use version-specific actions:
   - Activate: Make this version active
   - Edit: Modify this version
   - Delete: Remove this version
3. Only one version per template name can be active

## Migration from SQL Scripts

The new interface replaces the need for SQL scripts like `UpdatePromptTemplateWithPlayerData.sql`. Instead of running scripts:

1. Use the UI to create/update templates
2. Test changes before activation
3. Maintain version history automatically
4. Track usage and performance metrics

## Security and Permissions

- Only users with Admin role can access prompt template management
- All changes are logged with user information
- Template content is validated before saving
- Audit trail maintained for all operations

## Future Enhancements

- Template import/export functionality
- Advanced template validation
- Template performance analytics dashboard
- Automated template optimization suggestions
- Template sharing between environments
