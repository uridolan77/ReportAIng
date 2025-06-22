# 🚀 BI Reporting Copilot Frontend - Setup & Run Guide

## 📋 Prerequisites

### Required Software
- **Node.js**: Version 18.0.0 or higher
- **npm**: Version 9.0.0 or higher (comes with Node.js)
- **Git**: For version control

### Check Your Environment
```bash
node --version    # Should be >= 18.0.0
npm --version     # Should be >= 9.0.0
```

## 🛠️ Installation & Setup

### 1. Navigate to Frontend Directory
```bash
cd C:\dev\ReportAIng\frontend-v2
```

### 2. Install Dependencies
```bash
# Install all dependencies (this may take 2-3 minutes)
npm install

# If you encounter any issues, try:
npm install --legacy-peer-deps
```

### 3. Add Missing Dependencies (if needed)
```bash
# Export capabilities
npm install xlsx jspdf jspdf-autotable

# Additional type definitions
npm install --save-dev @types/xlsx
```

## 🚀 Running the Application

### Development Mode
```bash
# Start the development server
npm run dev

# The application will be available at:
# http://localhost:3001
```

### Production Build
```bash
# Build for production
npm run build

# Preview production build
npm run preview
```

## 🔧 Development Tools

### Code Quality
```bash
# Type checking
npm run type-check

# Linting
npm run lint
npm run lint:fix

# Code formatting
npm run format
```

### Testing
```bash
# Run tests
npm run test

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage
```

### Storybook (Component Documentation)
```bash
# Start Storybook
npm run storybook

# Available at: http://localhost:6006
```

## 🌐 Application URLs

Once running, you can access:

- **Main Application**: http://localhost:3001
- **Chat Interface**: http://localhost:3001/chat
- **Admin Dashboard**: http://localhost:3001/admin
- **Cost Management**: http://localhost:3001/admin/cost-management
- **Performance Monitoring**: http://localhost:3001/admin/performance
- **User Management**: http://localhost:3001/admin/users
- **Business Metadata**: http://localhost:3001/admin/business-metadata

## 🔌 Backend Integration

The frontend is configured to connect to:
- **API Server**: http://localhost:5000/api
- **WebSocket Hub**: http://localhost:5000/hub

### Backend Requirements
Make sure your backend server is running on port 5000 with the following endpoints:
- Authentication: `/api/auth/*`
- Query Processing: `/api/query/*`
- Cost Management: `/api/cost-management/*`
- Performance: `/api/performance/*`
- Business Metadata: `/api/business/*`
- Admin: `/api/admin/*`

## 🎯 Key Features Available

### 1. Chat Interface
- AI-powered query assistance
- Real-time streaming responses
- Query cost tracking
- History management

### 2. Admin Dashboard
- System monitoring
- User management
- Cost analytics
- Performance metrics

### 3. Cost Management
- Real-time cost tracking
- Budget management
- Cost optimization recommendations
- ROI analysis

### 4. Performance Monitoring
- System performance analysis
- Bottleneck detection
- Auto-tuning capabilities
- Benchmark tracking

### 5. Advanced Features
- Monaco SQL Editor with syntax highlighting
- Virtual scrolling for large datasets
- Multi-format export (CSV, Excel, PDF)
- D3.js advanced visualizations

## 🐛 Troubleshooting

### Common Issues

#### Port Already in Use
```bash
# If port 3001 is busy, change it in vite.config.ts
# Or kill the process using the port
netstat -ano | findstr :3001
taskkill /PID <PID_NUMBER> /F
```

#### Module Not Found Errors
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

#### TypeScript Errors
```bash
# Check for type errors
npm run type-check

# If Monaco Editor types are missing:
npm install --save-dev @types/monaco-editor
```

#### Build Errors
```bash
# Clear Vite cache
rm -rf node_modules/.vite
npm run dev
```

### Performance Issues
- Ensure you have at least 4GB RAM available
- Close unnecessary applications
- Use Chrome/Edge for best performance

## 📁 Project Structure

```
frontend-v2/
├── src/
│   ├── apps/
│   │   ├── chat/           # Chat interface
│   │   └── admin/          # Admin dashboard
│   ├── shared/
│   │   ├── components/     # Reusable components
│   │   ├── hooks/          # Custom hooks
│   │   ├── store/          # Redux store & APIs
│   │   ├── types/          # TypeScript types
│   │   └── utils/          # Utility functions
│   ├── App.tsx             # Main app component
│   └── main.tsx            # Entry point
├── docs/                   # Documentation
├── package.json            # Dependencies
└── vite.config.ts          # Vite configuration
```

## 🎉 Success Indicators

When everything is working correctly, you should see:

1. **Console Output**: No TypeScript errors
2. **Browser**: Application loads without errors
3. **Network Tab**: API calls to localhost:5000
4. **Features**: All components render properly

## 📞 Support

If you encounter issues:
1. Check the browser console for errors
2. Verify backend server is running
3. Ensure all dependencies are installed
4. Check network connectivity to backend

The application is now ready to run with all 100% of features implemented!
