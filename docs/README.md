# AI-Powered BI Reporting Copilot

An enterprise-grade AI-powered business intelligence reporting system that enables users to query data using natural language and receive intelligent insights with visualizations.

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or use Docker container)
- OpenAI API Key

### Automated Setup

Run the setup script to automatically configure your development environment:

```powershell
# Windows PowerShell
.\setup.ps1 -OpenAIKey "your-openai-api-key-here"

# Or skip certain components
.\setup.ps1 -SkipDocker -OpenAIKey "your-key"
```

### Manual Setup

1. **Clone and Setup Environment**
   ```bash
   git clone <repository-url>
   cd ReportAIng
   ```

2. **Backend Setup**
   ```bash
   cd backend
   dotnet restore
   dotnet build
   ```

3. **Frontend Setup**
   ```bash
   cd frontend
   npm install
   npm run type-check
   ```

4. **Start with Docker**
   ```bash
   docker-compose up -d
   ```

5. **Access the Application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger Documentation: http://localhost:5000/swagger

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   React SPA     │◄──►│   C# Web API     │◄──►│   SQL Server    │
│   + TypeScript  │    │   + SignalR      │    │   (Read-Only)   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         │              ┌────────▼────────┐              │
         │              │  AI Services    │              │
         │              │  - OpenAI/Azure │              │
         │              │  - Prompt Mgmt  │              │
         │              │  - Query Cache  │              │
         │              └─────────────────┘              │
         │                       │                       │
    ┌────▼────┐         ┌────────▼────────┐    ┌─────────▼─────────┐
    │ Auth    │         │  Monitoring     │    │   Schema Cache    │
    │ Service │         │  & Logging      │    │   & Metadata      │
    └─────────┘         └─────────────────┘    └───────────────────┘
```

## 📁 Project Structure

```
ReportAIng/
├── backend/                          # .NET 8 Web API
│   ├── BIReportingCopilot.API/      # API Controllers & Configuration
│   ├── BIReportingCopilot.Core/     # Domain Models & Interfaces
│   ├── BIReportingCopilot.Infrastructure/ # Data Access & Services
│   └── BIReportingCopilot.Tests/    # Unit & Integration Tests
├── frontend/                         # React + TypeScript SPA
│   ├── src/
│   │   ├── components/              # React Components
│   │   ├── hooks/                   # Custom React Hooks
│   │   ├── services/                # API Services
│   │   ├── stores/                  # State Management (Zustand)
│   │   ├── types/                   # TypeScript Type Definitions
│   │   └── utils/                   # Utility Functions
│   └── public/                      # Static Assets
├── database/                        # Database Scripts & Migrations
├── docs/                           # Documentation
├── infrastructure/                 # Deployment & Infrastructure
└── tests/                         # End-to-End Tests
```

## 🔧 Configuration

### Backend Configuration

Update `backend/BIReportingCopilot.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BIReportingCopilot_Dev;Trusted_Connection=true;",
    "BIDatabase": "your-bi-database-connection-string"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4"
  }
}
```

### Frontend Configuration

Create `frontend/.env.local`:

```env
REACT_APP_API_URL=http://localhost:5000
REACT_APP_WEBSOCKET_URL=ws://localhost:5000
```

## 🚀 Development

### Running Locally

**Backend:**
```bash
cd backend
dotnet run --project BIReportingCopilot.API
```

**Frontend:**
```bash
cd frontend
npm start
```

**Database:**
```bash
# Start SQL Server and Redis
docker-compose up -d sqlserver redis

# Run migrations
cd backend
dotnet ef database update --project BIReportingCopilot.Infrastructure --startup-project BIReportingCopilot.API
```

### Running with Docker

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

### End-to-End Tests
```bash
# Install Playwright
npm install -g @playwright/test

# Run E2E tests
npx playwright test
```

## 📊 Features

### Core Features
- ✅ Natural language to SQL query conversion
- ✅ Real-time query execution with progress updates
- ✅ Automatic data visualization
- ✅ Query history and favorites
- ✅ Export capabilities (CSV, Excel, PDF)
- ✅ Role-based access control
- ✅ Comprehensive audit logging

### AI Features
- ✅ GPT-4 powered query generation
- ✅ Confidence scoring for generated queries
- ✅ Query optimization suggestions
- ✅ Intelligent chart type selection
- ✅ Natural language insights generation

### Enterprise Features
- ✅ Multi-tenant architecture
- ✅ Advanced security and compliance
- ✅ Performance monitoring and alerting
- ✅ Horizontal scaling support
- ✅ Disaster recovery capabilities

## 🔐 Security

### Authentication & Authorization
- JWT-based authentication
- Role-based access control (RBAC)
- Azure AD integration support
- Session management

### Data Security
- SQL injection prevention
- Input validation and sanitization
- Data masking for sensitive information
- Comprehensive audit logging

### Infrastructure Security
- HTTPS enforcement
- Rate limiting
- CORS configuration
- Security headers

## 📈 Monitoring & Observability

### Logging
- Structured logging with Serilog
- Application Insights integration
- Centralized log aggregation

### Metrics
- Performance metrics collection
- Query execution monitoring
- User activity tracking
- System health monitoring

### Alerting
- Real-time error notifications
- Performance threshold alerts
- Security event monitoring

## 🚀 Deployment

### Development
```bash
docker-compose up -d
```

### Staging/Production
```bash
# Build production images
docker-compose -f docker-compose.prod.yml build

# Deploy to Kubernetes
kubectl apply -f infrastructure/k8s/
```

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `OPENAI_API_KEY` | OpenAI API key for AI services | Yes |
| `JWT_SECRET` | Secret key for JWT token signing | Yes |
| `ConnectionStrings__DefaultConnection` | Primary database connection | Yes |
| `ConnectionStrings__BIDatabase` | BI data source connection | Yes |
| `ConnectionStrings__Redis` | Redis cache connection | No |

## 📚 API Documentation

### Core Endpoints

- `POST /api/query/natural-language` - Execute natural language queries
- `GET /api/query/history` - Retrieve query history
- `POST /api/query/feedback` - Submit query feedback
- `GET /api/schema/tables` - Get database schema information
- `GET /api/health` - Health check endpoint

### WebSocket Endpoints

- `/hubs/query-status` - Real-time query execution updates

Full API documentation is available at `/swagger` when running the application.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow the existing code style and conventions
- Write comprehensive tests for new features
- Update documentation for any API changes
- Ensure all tests pass before submitting PR

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:

- 📧 Email: support@company.com
- 📖 Documentation: [docs/](docs/)
- 🐛 Issues: [GitHub Issues](https://github.com/your-org/bi-reporting-copilot/issues)

## 🗺️ Roadmap

### Phase 1 (Current) - Foundation
- [x] Core query processing
- [x] Basic UI implementation
- [x] Authentication system
- [x] Database integration

### Phase 2 - Enhanced Features
- [ ] Advanced visualizations
- [ ] Slack/Teams integration
- [ ] Advanced analytics
- [ ] Performance optimization

### Phase 3 - Enterprise Features
- [ ] Multi-tenant support
- [ ] Advanced security features
- [ ] Compliance reporting
- [ ] Advanced monitoring

---

**Built with ❤️ by the BI Copilot Team**
