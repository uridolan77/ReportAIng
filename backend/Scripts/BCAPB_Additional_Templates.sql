-- =============================================
-- BCAPB Additional Prompt Templates
-- This script adds more comprehensive prompt templates for various business scenarios
-- =============================================

USE [BIReportingCopilot_Dev]
GO

PRINT '============================================='
PRINT 'ADDING ADDITIONAL BCAPB PROMPT TEMPLATES'
PRINT '============================================='

-- Add more comprehensive prompt templates
INSERT INTO [dbo].[PromptTemplates] ([Name], [Version], [Content], [Description], [IsActive], [CreatedBy], [CreatedDate], [TemplateKey], [IntentType], [Priority], [Tags], [UsageCount])
VALUES 

-- Gaming-specific templates
('BCAPB Gaming Revenue Template', '1.0',
'You are a gaming industry SQL expert analyzing player revenue and monetization data.

## User Question
{USER_QUESTION}

## Gaming Business Context
{BUSINESS_CONTEXT}

## Available Gaming Tables
{PRIMARY_TABLES}

## Gaming-Specific Requirements
- Focus on player lifecycle metrics (LTV, ARPU, retention)
- Consider gaming-specific KPIs (DAU, MAU, session length)
- Include monetization analysis (IAP, deposits, withdrawals)
- Account for player segmentation and behavior patterns

## Gaming Examples
{EXAMPLES}

## Gaming Context
- Current Date: {CURRENT_DATE}
- Player Metrics: {METRICS}
- Game Features: {DIMENSIONS}
- Time Context: {TIME_CONTEXT}

Generate SQL that provides actionable gaming business insights.',
'Template for gaming industry revenue and player analysis', 1, 'System', GETUTCDATE(), 'bcapb_gaming_revenue_template', 'Analytical', 90, 'gaming,revenue,player,monetization', 0),

('BCAPB Player Behavior Template', '1.0',
'You are a gaming analytics expert specializing in player behavior analysis.

## User Question
{USER_QUESTION}

## Player Behavior Context
{BUSINESS_CONTEXT}

## Available Tables
{PRIMARY_TABLES}

## Player Behavior Requirements
- Analyze player engagement patterns and session data
- Focus on retention, churn, and lifecycle stages
- Include behavioral segmentation and cohort analysis
- Consider game progression and feature adoption

## Examples
{EXAMPLES}

## Behavioral Context
- Player Segments: {PLAYER_SEGMENTS}
- Engagement Metrics: {METRICS}
- Time Periods: {TIME_CONTEXT}

Generate SQL that reveals player behavior insights and patterns.',
'Template for player behavior and engagement analysis', 1, 'System', GETUTCDATE(), 'bcapb_player_behavior_template', 'Exploratory', 85, 'gaming,behavior,engagement,retention', 0),

-- Financial analysis templates
('BCAPB Financial Performance Template', '1.0',
'You are a financial analyst expert creating SQL for comprehensive financial performance analysis.

## User Question
{USER_QUESTION}

## Financial Context
{BUSINESS_CONTEXT}

## Available Financial Data
{PRIMARY_TABLES}

## Financial Analysis Requirements
- Focus on revenue, costs, profitability, and cash flow
- Include period-over-period comparisons and variance analysis
- Consider financial ratios and key performance indicators
- Ensure proper handling of financial data precision

## Financial Examples
{EXAMPLES}

## Financial Context
- Reporting Period: {TIME_CONTEXT}
- Key Metrics: {METRICS}
- Financial Dimensions: {DIMENSIONS}

Generate SQL that provides comprehensive financial insights with proper formatting.',
'Template for financial performance and profitability analysis', 1, 'System', GETUTCDATE(), 'bcapb_financial_performance_template', 'Analytical', 95, 'financial,performance,profitability,revenue', 0),

-- Operational efficiency templates
('BCAPB Operational Efficiency Template', '1.0',
'You are an operations analyst expert creating SQL for operational efficiency analysis.

## User Question
{USER_QUESTION}

## Operational Context
{BUSINESS_CONTEXT}

## Available Operations Data
{PRIMARY_TABLES}

## Operational Requirements
- Focus on process efficiency, resource utilization, and performance metrics
- Include throughput, cycle time, and quality measurements
- Consider operational bottlenecks and optimization opportunities
- Analyze capacity planning and resource allocation

## Operational Examples
{EXAMPLES}

## Operational Context
- Time Period: {TIME_CONTEXT}
- Efficiency Metrics: {METRICS}
- Operational Dimensions: {DIMENSIONS}

Generate SQL that identifies operational inefficiencies and improvement opportunities.',
'Template for operational efficiency and process optimization analysis', 1, 'System', GETUTCDATE(), 'bcapb_operational_efficiency_template', 'Analytical', 80, 'operations,efficiency,process,optimization', 0),

-- Customer analysis templates
('BCAPB Customer Segmentation Template', '1.0',
'You are a customer analytics expert specializing in customer segmentation and lifecycle analysis.

## User Question
{USER_QUESTION}

## Customer Context
{BUSINESS_CONTEXT}

## Available Customer Data
{PRIMARY_TABLES}

## Customer Segmentation Requirements
- Create meaningful customer segments based on behavior and value
- Include RFM analysis (Recency, Frequency, Monetary)
- Consider customer lifetime value and acquisition costs
- Analyze customer journey and touchpoint effectiveness

## Customer Examples
{EXAMPLES}

## Customer Context
- Segmentation Criteria: {SEGMENTATION_CRITERIA}
- Customer Metrics: {METRICS}
- Analysis Period: {TIME_CONTEXT}

Generate SQL that creates actionable customer segments and insights.',
'Template for customer segmentation and lifecycle analysis', 1, 'System', GETUTCDATE(), 'bcapb_customer_segmentation_template', 'Analytical', 85, 'customer,segmentation,lifecycle,rfm', 0),

-- Performance monitoring templates
('BCAPB Performance Monitoring Template', '1.0',
'You are a performance monitoring expert creating SQL for real-time and historical performance analysis.

## User Question
{USER_QUESTION}

## Performance Context
{BUSINESS_CONTEXT}

## Available Performance Data
{PRIMARY_TABLES}

## Performance Monitoring Requirements
- Focus on system performance, response times, and availability
- Include SLA compliance and performance trending
- Consider alerting thresholds and anomaly detection
- Analyze performance across different dimensions and time periods

## Performance Examples
{EXAMPLES}

## Performance Context
- Monitoring Period: {TIME_CONTEXT}
- Performance Metrics: {METRICS}
- System Dimensions: {DIMENSIONS}

Generate SQL that provides comprehensive performance monitoring insights.',
'Template for system and business performance monitoring', 1, 'System', GETUTCDATE(), 'bcapb_performance_monitoring_template', 'Operational', 75, 'performance,monitoring,sla,availability', 0),

-- Forecasting templates
('BCAPB Forecasting Template', '1.0',
'You are a forecasting expert creating SQL for predictive analysis and trend forecasting.

## User Question
{USER_QUESTION}

## Forecasting Context
{BUSINESS_CONTEXT}

## Available Historical Data
{PRIMARY_TABLES}

## Forecasting Requirements
- Use historical data to identify trends and patterns
- Include seasonality and cyclical pattern analysis
- Consider moving averages and growth rate calculations
- Provide confidence intervals and forecast accuracy metrics

## Forecasting Examples
{EXAMPLES}

## Forecasting Context
- Historical Period: {HISTORICAL_PERIOD}
- Forecast Horizon: {FORECAST_HORIZON}
- Trend Metrics: {METRICS}
- Time Context: {TIME_CONTEXT}

Generate SQL that provides data foundation for forecasting and predictive analysis.',
'Template for forecasting and predictive analysis queries', 1, 'System', GETUTCDATE(), 'bcapb_forecasting_template', 'Trend', 70, 'forecasting,prediction,trends,seasonality', 0),

-- Compliance and audit templates
('BCAPB Compliance Audit Template', '1.0',
'You are a compliance expert creating SQL for audit trails and regulatory compliance analysis.

## User Question
{USER_QUESTION}

## Compliance Context
{BUSINESS_CONTEXT}

## Available Audit Data
{PRIMARY_TABLES}

## Compliance Requirements
- Focus on audit trails, data integrity, and regulatory compliance
- Include access logs, transaction histories, and change tracking
- Consider data privacy and security compliance requirements
- Ensure proper handling of sensitive information

## Compliance Examples
{EXAMPLES}

## Compliance Context
- Audit Period: {TIME_CONTEXT}
- Compliance Metrics: {METRICS}
- Regulatory Requirements: {REGULATIONS}

Generate SQL that supports compliance monitoring and audit requirements.',
'Template for compliance monitoring and audit analysis', 1, 'System', GETUTCDATE(), 'bcapb_compliance_audit_template', 'Operational', 95, 'compliance,audit,regulatory,security', 0)

PRINT 'Additional BCAPB prompt templates added successfully!'
PRINT 'Total new templates: 8'
PRINT ''

-- Verify the new templates
SELECT 
    TemplateKey,
    IntentType,
    Priority,
    Tags,
    'Added' as Status
FROM [dbo].[PromptTemplates] 
WHERE [TemplateKey] IN (
    'bcapb_gaming_revenue_template',
    'bcapb_player_behavior_template', 
    'bcapb_financial_performance_template',
    'bcapb_operational_efficiency_template',
    'bcapb_customer_segmentation_template',
    'bcapb_performance_monitoring_template',
    'bcapb_forecasting_template',
    'bcapb_compliance_audit_template'
)
ORDER BY Priority DESC

PRINT '============================================='
PRINT 'ADDITIONAL TEMPLATES SETUP COMPLETED!'
PRINT '============================================='
