# Process Flow Viewer Component

A comprehensive React component for visualizing and monitoring the complete AI chat processing pipeline with real-time updates and detailed transparency information.

## Overview

The Process Flow Viewer provides users with complete visibility into how their natural language queries are processed by the AI system, from initial authentication through final response assembly. It shows each step of the pipeline with timing information, detailed logs, and transparency data about AI model usage.

## Features

### 🔄 Real-time Process Tracking
- Live status updates for each processing step
- Progress indicators and completion percentages
- Real-time log streaming from backend services
- WebSocket integration for instant updates

### 👁️ Complete AI Transparency
- AI model information (GPT-4, temperature, etc.)
- Token usage breakdown (prompt, completion, total)
- Confidence scores for each processing step
- Cost tracking and budget monitoring

### 📊 Detailed Step Breakdown
- Hierarchical step visualization with sub-steps
- Input/output data for each processing stage
- Execution timing and performance metrics
- Error handling and retry mechanisms

### 🎯 Interactive Interface
- Expandable step details with syntax highlighting
- Collapsible sections for logs and metadata
- Copy functionality for SQL and data
- Export capabilities for process reports

## Component Architecture

### Core Components

#### `ProcessFlowViewer`
Main component that renders the process flow in a drawer interface.

```typescript
interface ProcessFlowViewerProps {
  visible: boolean;
  onClose: () => void;
  processData?: ProcessFlowData;
  realTimeUpdates?: boolean;
}
```

#### `ProcessStepCard`
Individual step component with expandable details and sub-steps.

#### `useProcessFlow` Hook
Custom hook for managing process flow state and real-time updates.

```typescript
const {
  processData,
  isVisible,
  showProcessFlow,
  hideProcessFlow,
  updateStep,
  setTransparencyData
} = useProcessFlow({ enableRealTime: true });
```

## Data Structure

### ProcessFlowData
```typescript
interface ProcessFlowData {
  sessionId: string;
  query: string;
  userId: string;
  startTime: string;
  endTime?: string;
  totalDuration?: number;
  status: 'running' | 'completed' | 'error';
  steps: ProcessStep[];
  transparency?: {
    promptTokens?: number;
    completionTokens?: number;
    totalTokens?: number;
    model?: string;
    temperature?: number;
    confidence?: number;
  };
}
```

### ProcessStep
```typescript
interface ProcessStep {
  id: string;
  name: string;
  description: string;
  status: 'pending' | 'running' | 'completed' | 'error';
  startTime?: string;
  endTime?: string;
  duration?: number;
  details?: {
    input?: any;
    output?: any;
    logs?: string[];
    metadata?: Record<string, any>;
  };
  subSteps?: ProcessStep[];
}
```

## Processing Steps

The system tracks these main processing steps:

### 1. Authentication & Authorization
- JWT token validation
- User permission verification
- Rate limiting checks

### 2. Request Validation
- Parameter validation
- Query structure verification
- Option validation

### 3. Semantic Analysis
- **Intent Detection**: Determining query type and business intent
- **Entity Extraction**: Identifying tables, columns, and business entities
- Natural language processing
- Business context analysis

### 4. Schema Retrieval
- Database schema metadata fetching
- Table relationship analysis
- Column constraint validation

### 5. Prompt Construction
- **Context Gathering**: Collecting schema, examples, and business rules
- **Prompt Assembly**: Assembling final prompt for AI model
- Template loading and customization
- Business rule injection

### 6. AI SQL Generation
- **OpenAI API Call**: Sending request to GPT-4 model
- **Response Parsing**: Parsing and validating AI response
- Model processing and optimization
- Confidence scoring

### 7. SQL Validation
- Syntax validation
- Semantic analysis
- Security checks
- Performance analysis

### 8. SQL Execution
- **Query Execution**: Running SQL query on database
- **Result Processing**: Processing and formatting query results
- Connection management
- Data transformation

### 9. Response Assembly
- Final response structure creation
- Metadata attachment
- Visualization data preparation
- Alternative query suggestions

## Usage Examples

### Basic Usage
```typescript
import { ProcessFlowViewer } from './ProcessFlowViewer';
import { useProcessFlow } from '../hooks/useProcessFlow';

const ChatComponent = () => {
  const { processData, isVisible, showProcessFlow, hideProcessFlow } = 
    useProcessFlow({ enableRealTime: true });

  const handleSendMessage = async (query: string) => {
    const sessionId = `session-${Date.now()}`;
    showProcessFlow(sessionId, { query, userId: 'current-user' });
    
    // Execute query...
  };

  return (
    <>
      {/* Chat interface */}
      <ProcessFlowViewer
        visible={isVisible}
        onClose={hideProcessFlow}
        processData={processData}
        realTimeUpdates={true}
      />
    </>
  );
};
```

### Manual Process Flow Creation
```typescript
const demoProcessData: ProcessFlowData = {
  sessionId: 'demo-session',
  query: 'Show me top 10 players by deposits',
  userId: 'demo-user',
  startTime: new Date().toISOString(),
  status: 'completed',
  steps: [
    {
      id: 'auth',
      name: 'Authentication',
      description: 'Validating user credentials',
      status: 'completed',
      duration: 150,
      details: {
        logs: ['JWT token validated', 'Permissions verified'],
        metadata: { endpoint: '/api/query/enhanced' }
      }
    }
    // ... more steps
  ],
  transparency: {
    model: 'gpt-4',
    totalTokens: 1250,
    confidence: 0.95
  }
};
```

### Integration with Message Actions
```typescript
const MessageActions = ({ message, onShowProcessFlow }) => {
  return (
    <Button
      icon={<EyeOutlined />}
      onClick={() => onShowProcessFlow(message.id)}
      title="View AI process flow"
    />
  );
};
```

## Real-time Updates

The component supports real-time updates via WebSocket connections:

```typescript
// Socket event handlers
socketService.on('process:step:update', (data) => {
  updateStep(data.stepId, { status: data.status, ...data.details });
});

socketService.on('process:step:log', (data) => {
  // Add log entry to specific step
});

socketService.on('process:complete', (data) => {
  // Mark process as complete with final metrics
});
```

## Styling and Theming

The component uses Ant Design components with custom styling:

- **Step Status Colors**: Green (completed), Blue (running), Red (error), Gray (pending)
- **Progress Indicators**: Circular progress for running steps
- **Syntax Highlighting**: Code blocks with tomorrow theme
- **Responsive Design**: Adapts to different screen sizes

## Performance Considerations

- **Virtualization**: Large step lists are virtualized for performance
- **Lazy Loading**: Step details are loaded on demand
- **Memory Management**: Old process data is cleaned up automatically
- **WebSocket Optimization**: Efficient event handling and cleanup

## Demo Component

The `ProcessFlowDemo` component provides interactive examples:

```typescript
import { ProcessFlowDemo } from './ProcessFlowDemo';

// Shows three demo scenarios:
// 1. Simple Data Query
// 2. Complex Analytics Query  
// 3. Semantic Analysis Heavy Query
```

## Backend Integration

The component expects these backend events:

```typescript
// Step status updates
POST /api/process/step/update
{
  sessionId: string,
  stepId: string,
  status: 'running' | 'completed' | 'error',
  details?: any
}

// Log entries
POST /api/process/step/log
{
  sessionId: string,
  stepId: string,
  log: string,
  timestamp: string
}

// Process completion
POST /api/process/complete
{
  sessionId: string,
  status: 'completed' | 'error',
  totalDuration: number,
  transparency: TransparencyData
}
```

## Future Enhancements

- **Process Comparison**: Compare processing flows between different queries
- **Performance Analytics**: Historical performance trends and optimization suggestions
- **Custom Step Definitions**: Allow custom processing steps for different query types
- **Export Functionality**: Export process flows as PDF reports or JSON data
- **Process Templates**: Save and reuse common processing patterns
- **Collaborative Features**: Share process flows with team members
- **Integration Monitoring**: Monitor external API calls and dependencies

## Troubleshooting

### Common Issues

1. **Process Flow Not Showing**: Ensure WebSocket connection is established
2. **Real-time Updates Missing**: Check socket event handlers are properly registered
3. **Performance Issues**: Enable virtualization for large step lists
4. **Memory Leaks**: Ensure proper cleanup of socket listeners

### Debug Mode

Enable debug logging:
```typescript
const { processData } = useProcessFlow({ 
  enableRealTime: true,
  debug: true 
});
```

This comprehensive Process Flow Viewer provides complete transparency into the AI processing pipeline, helping users understand how their queries are transformed into results while maintaining trust through detailed visibility into every step of the process.
