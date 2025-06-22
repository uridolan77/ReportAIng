# Chat Pages Documentation

## Overview
This directory contains all the main page components for the chat application, providing different interfaces and views for AI-powered conversations.

## Pages

### 1. EnhancedChatPage.tsx
**Route:** `/chat/enhanced`

**Description:** 
Advanced chat interface with comprehensive AI transparency features and real-time monitoring.

**Features:**
- **Mode Toggle:** Switch between Normal and Advanced modes
- **Real-time Transparency:** Live tracking of AI decision-making processes
- **Business Context Analysis:** Automatic analysis of business metadata and relationships
- **Performance Metrics:** Query performance monitoring and optimization suggestions
- **AI Analytics:** Deep insights into AI confidence and reasoning
- **Dashboard Metrics:** Overview of transparency traces and confidence levels

**Components Used:**
- `ComprehensiveChatInterface` - Main chat interface with transparency
- `PageLayout` - Standard page wrapper
- Transparency API integration for metrics

**Navigation:**
- Added to sidebar as "Enhanced Chat" with eye icon
- Accessible via main navigation menu

### 2. ChatInterface.tsx (Legacy)
**Route:** `/chat`

**Description:**
Standard chat interface page wrapper.

**Features:**
- Basic chat functionality
- Query history access
- Standard message display

### 3. QueryHistory.tsx
**Route:** `/chat/history`

**Description:**
View and manage chat query history.

### 4. QueryResults.tsx
**Route:** `/chat/results/:queryId?`

**Description:**
Display and analyze query results.

## Usage Examples

### Enhanced Chat Page
```typescript
// Navigate to enhanced chat
navigate('/chat/enhanced')

// With conversation ID
navigate('/chat/enhanced/conv-123')
```

### Mode Configuration
The Enhanced Chat Page supports two modes:

**Normal Mode:**
- Standard chat interface
- Basic transparency indicators
- Simple confidence display

**Advanced Mode:**
- Real-time transparency tracking
- Business context analysis
- Performance metrics monitoring
- AI analytics dashboard
- Comprehensive insights panel

## Integration Points

### Transparency API
The Enhanced Chat Page integrates with:
- `useGetTransparencyDashboardMetricsQuery` - Dashboard metrics
- Real-time transparency tracking
- Confidence analysis
- Performance monitoring

### Navigation
- Added to main sidebar navigation
- Proper route highlighting
- Breadcrumb support

## Development Notes

### Adding New Features
1. Extend the `ComprehensiveChatInterface` component
2. Add new transparency API endpoints
3. Update mode-specific feature toggles

### Customization
The page supports various customization options:
- Mode switching (Normal/Advanced)
- Feature toggles for transparency components
- Responsive design for different screen sizes

## Future Enhancements

### Planned Features
- Settings modal for transparency preferences
- Export functionality for chat sessions
- Advanced filtering and search
- Custom dashboard widgets
- Integration with business glossary

### Performance Optimizations
- Lazy loading of transparency components
- Efficient real-time data polling
- Optimized rendering for large chat histories
