# BI Reporting Copilot v2 - Modern Frontend

A clean, modern implementation of the BI Reporting Copilot frontend following the technical specification exactly.

## 🚀 Features

### Chat Interface Application
- **Modern Conversational UI** - Natural language query interface
- **Real-time Streaming** - Live query execution with Socket.IO
- **SQL Editor** - Monaco Editor with syntax highlighting and autocomplete
- **Interactive Visualizations** - Recharts-based charts and graphs
- **Query History** - Searchable history with favorites

### Admin Dashboard Application
- **Business Metadata Management** - Table and column metadata editing
- **System Configuration** - AI provider settings and system config
- **User Management** - User roles and permissions
- **Analytics & Monitoring** - System performance and usage analytics
- **Schema Management** - Database schema exploration and management

## 🛠 Technology Stack

- **Framework**: React 18+ with TypeScript
- **State Management**: Redux Toolkit + RTK Query (as per spec)
- **UI Framework**: Ant Design v5
- **Real-time**: Socket.IO client
- **Charts**: Recharts + D3.js
- **Code Editor**: Monaco Editor
- **Build Tool**: Vite (for better performance)
- **Authentication**: JWT with refresh tokens + MFA support

## 📁 Project Structure

```
frontend-v2/
├── src/
│   ├── apps/                    # Separate applications
│   │   ├── chat/               # Chat Interface Application
│   │   │   ├── pages/
│   │   │   └── components/
│   │   └── admin/              # Admin Dashboard Application
│   │       ├── pages/
│   │       └── components/
│   ├── shared/                 # Shared components and utilities
│   │   ├── components/         # Reusable UI components
│   │   │   └── core/          # Core design system components
│   │   ├── services/          # API services and utilities
│   │   ├── store/             # Redux store and API slices
│   │   │   └── api/           # RTK Query API slices
│   │   ├── hooks/             # Custom React hooks
│   │   ├── theme/             # Theme configuration
│   │   └── pages/             # Shared pages (login, etc.)
│   ├── App.tsx                # Main application router
│   └── main.tsx               # Application entry point
├── package.json
├── vite.config.ts
└── tsconfig.json
```

## 🎨 Design System

The frontend includes a comprehensive design system with:
- **Design Tokens** - Centralized colors, spacing, typography
- **Component Library** - Reusable UI components
- **Theme Support** - Light/dark mode with Ant Design integration
- **TypeScript Support** - Full type safety throughout

## 🔧 Development

### Prerequisites
- Node.js 18+
- npm 9+

### Installation
```bash
cd frontend-v2
npm install
```

### Development Server
```bash
npm run dev
```

### Build for Production
```bash
npm run build
```

### Testing
```bash
npm run test
```

### Linting & Formatting
```bash
npm run lint
npm run format
```

## 🔌 API Integration

The frontend integrates with the backend through:
- **RTK Query** - Automatic caching, background updates, optimistic updates
- **Socket.IO** - Real-time query streaming and progress updates
- **JWT Authentication** - Secure API access with automatic token refresh

### API Slices
- `authApi` - Authentication and user management
- `queryApi` - Query execution and history
- `businessApi` - Business metadata management
- `semanticApi` - Enhanced semantic layer
- `adminApi` - System configuration and analytics

## 🚦 Routing

### Chat Application (`/chat/*`)
- `/chat` - Main chat interface
- `/chat/history` - Query history
- `/chat/results/:queryId` - Query results

### Admin Application (`/admin/*`)
- `/admin` - Admin dashboard
- `/admin/business-metadata` - Business metadata management
- `/admin/system-config` - System configuration
- `/admin/users` - User management
- `/admin/analytics` - Analytics and monitoring

## 🔐 Authentication

- **JWT-based** authentication with refresh tokens
- **MFA support** - TOTP and SMS authentication
- **Role-based access** - Admin routes protected by role checks
- **Automatic token refresh** - Seamless session management

## 📊 Real-time Features

- **Streaming queries** - Live query execution progress
- **Socket.IO integration** - Real-time updates and notifications
- **Progress tracking** - Visual feedback during query execution

## 🎯 Key Components

### Core Components
- **Button** - Comprehensive button system with variants
- **Chart** - Unified chart component with multiple types
- **SqlEditor** - Monaco-based SQL editor with autocomplete
- **Layout** - Application and page layout components

### Shared Services
- **socketService** - Socket.IO connection management
- **API services** - RTK Query-based API integration

## 🔄 Migration from v1

This is a clean implementation that:
- ✅ **Reuses proven components** from the current frontend
- ✅ **Follows the technical specification** exactly
- ✅ **Implements modern patterns** (Redux Toolkit, RTK Query)
- ✅ **Adds missing features** (Socket.IO, Monaco Editor, MFA)
- ✅ **Maintains compatibility** with existing backend APIs

## 📝 Next Steps

1. **Install dependencies** and start development server
2. **Implement remaining pages** based on business requirements
3. **Add comprehensive testing** for all components
4. **Integrate with backend** and test API connections
5. **Deploy and configure** production environment

## 🤝 Contributing

This frontend follows modern React best practices:
- **TypeScript** for type safety
- **Component composition** over inheritance
- **Custom hooks** for reusable logic
- **RTK Query** for efficient data fetching
- **Consistent naming** and file organization
