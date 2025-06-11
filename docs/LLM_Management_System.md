# LLM Management System

## Overview

The LLM Management System provides comprehensive control over AI providers, models, usage tracking, cost monitoring, and performance analytics. This system replaces the hardcoded AI provider selection with a dynamic, configurable approach that allows administrators to manage multiple providers and models through a user-friendly interface.

## Key Features

### 🔌 **Provider Management**
- Configure multiple LLM providers (OpenAI, Azure OpenAI, etc.)
- Secure API key management with masking
- Real-time connection testing and health monitoring
- Enable/disable providers with default selection
- Provider-specific settings and configurations

### 🤖 **Model Configuration**
- Configure model parameters (temperature, max tokens, etc.)
- Assign models to specific use cases (SQL generation, insights, etc.)
- Cost per token tracking for accurate billing
- Model capability management and optimization
- Default model selection per use case

### 📊 **Usage Analytics**
- Comprehensive request/response logging
- Token usage tracking and analytics
- Performance metrics (response time, success rate)
- User-specific usage tracking
- Export functionality for data analysis

### 💰 **Cost Management**
- Real-time cost tracking per provider/model
- Monthly cost summaries and projections
- Cost alerts and spending limits
- Detailed cost breakdown by request type
- Budget management and optimization

### ⚡ **Performance Monitoring**
- Response time metrics and analysis
- Success/failure rate tracking
- Error analysis and categorization
- Cache hit rate monitoring
- Performance optimization recommendations

## Database Schema

The system uses three main tables:

### LLMProviderConfigs
Stores provider configurations including API keys, endpoints, and settings.

### LLMModelConfigs
Stores model configurations with parameters, use cases, and cost information.

### LLMUsageLogs
Comprehensive usage tracking with tokens, costs, performance metrics, and metadata.

## Usage Guide

### 1. **Initial Setup**

1. **Database Setup**: Run the provided SQL script to create the necessary tables
2. **Access Management**: Navigate to `/admin/llm` (admin access required)
3. **Configure Providers**: Add your API keys and test connections
4. **Enable Providers**: Toggle providers to enabled status

### 2. **Provider Configuration**

1. Go to **Provider Settings** tab
2. Click **Add Provider** or edit existing ones
3. Configure:
   - Provider name and type
   - API key (securely stored)
   - Endpoint URL (if custom)
   - Organization ID (if applicable)
4. **Test Connection** to verify setup
5. **Enable** the provider for use

### 3. **Model Configuration**

1. Go to **Model Configuration** tab
2. Configure model parameters:
   - Temperature (creativity level)
   - Max tokens (response length)
   - Use case assignment (SQL, Insights, etc.)
   - Cost per token for tracking
3. Set **default models** for each use case

### 4. **Query Submission with LLM Selection**

Users can now select specific providers and models when submitting queries:

1. **Automatic Selection**: System uses default provider/model if none specified
2. **Manual Selection**: Users can choose specific provider/model via LLM Selector
3. **Use Case Optimization**: System automatically selects best model for the task

### 5. **Monitoring and Analytics**

1. **Dashboard**: Real-time overview of system health and usage
2. **Usage Analytics**: Detailed request history and performance metrics
3. **Cost Monitoring**: Track spending and set up alerts
4. **Performance Analysis**: Monitor response times and success rates

## API Integration

### Backend Changes

The system introduces several new components:

- **ILLMManagementService**: Core service for provider/model management
- **LLMAwareAIService**: Enhanced AI service with provider selection
- **ConfigurableProviderWrapper**: Runtime provider configuration
- **LLMManagementController**: RESTful API for all operations

### Frontend Integration

- **LLMSelector**: Component for provider/model selection
- **LLMStatusWidget**: System status and quick actions
- **LLMManagementPage**: Comprehensive admin interface
- **Enhanced Query Input**: Integrated LLM selection

## Configuration Flow

```
User Query → LLM Selector → Provider Selection → Model Selection → AI Service → Usage Logging
```

1. User submits query with optional provider/model selection
2. System determines best provider/model if not specified
3. Query is processed using selected configuration
4. Usage is logged for analytics and cost tracking
5. Results are returned with performance metrics

## Security Features

- **API Key Masking**: Sensitive data is never displayed in full
- **Admin-Only Access**: LLM management requires admin privileges
- **Secure Storage**: API keys are encrypted in the database
- **Audit Logging**: All operations are logged for compliance

## Performance Optimizations

- **Caching**: Provider configurations are cached for performance
- **Connection Pooling**: Efficient connection management
- **Async Operations**: Non-blocking request processing
- **Health Monitoring**: Automatic failover to healthy providers

## Cost Optimization

- **Token Tracking**: Accurate token counting for cost calculation
- **Model Selection**: Choose cost-effective models for different tasks
- **Usage Limits**: Set spending limits and alerts
- **Analytics**: Identify cost optimization opportunities

## Testing

Use the **LLM Test Page** (`/admin/llm-test`) to:

- Test provider/model selection
- Verify system integration
- Monitor real-time performance
- Validate cost tracking

## Troubleshooting

### Common Issues

1. **Provider Not Working**: Check API key and endpoint configuration
2. **High Costs**: Review model selection and usage patterns
3. **Slow Responses**: Monitor provider health and performance metrics
4. **Failed Requests**: Check error logs and provider status

### Health Checks

The system provides comprehensive health monitoring:

- Provider connectivity tests
- Model availability checks
- Performance metric tracking
- Error rate monitoring

## Future Enhancements

- **Advanced Analytics**: Machine learning for usage prediction
- **Auto-Scaling**: Dynamic provider selection based on load
- **Cost Optimization**: AI-powered cost reduction recommendations
- **Multi-Region**: Geographic provider distribution
- **Custom Models**: Support for fine-tuned and custom models

## Support

For issues or questions:

1. Check the **Dashboard** for system status
2. Review **Usage Analytics** for insights
3. Test connections in **Provider Settings**
4. Use the **LLM Test Page** for debugging
5. Contact system administrators for advanced support

---

The LLM Management System provides enterprise-grade control over AI operations with comprehensive monitoring, cost management, and performance optimization capabilities.
