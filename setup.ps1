# AI-Powered BI Reporting Copilot - Setup Script
# This script sets up the development environment for the BI Reporting Copilot

param(
    [switch]$SkipDocker,
    [switch]$SkipDotnet,
    [switch]$SkipNode,
    [switch]$Production,
    [string]$OpenAIKey = ""
)

Write-Host "🚀 Setting up AI-Powered BI Reporting Copilot..." -ForegroundColor Green

# Check prerequisites
function Test-Prerequisites {
    Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow
    
    $missing = @()
    
    if (-not $SkipDocker) {
        if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
            $missing += "Docker"
        }
        if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
            $missing += "Docker Compose"
        }
    }
    
    if (-not $SkipDotnet) {
        if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
            $missing += ".NET 8 SDK"
        }
    }
    
    if (-not $SkipNode) {
        if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
            $missing += "Node.js"
        }
        if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
            $missing += "npm"
        }
    }
    
    if ($missing.Count -gt 0) {
        Write-Host "❌ Missing prerequisites: $($missing -join ', ')" -ForegroundColor Red
        Write-Host "Please install the missing prerequisites and run the script again." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ All prerequisites are installed." -ForegroundColor Green
}

# Setup environment variables
function Set-EnvironmentVariables {
    Write-Host "🔧 Setting up environment variables..." -ForegroundColor Yellow
    
    if ($OpenAIKey) {
        [Environment]::SetEnvironmentVariable("OPENAI_API_KEY", $OpenAIKey, "User")
        Write-Host "✅ OpenAI API key set." -ForegroundColor Green
    } else {
        Write-Host "⚠️  OpenAI API key not provided. You'll need to set it manually." -ForegroundColor Yellow
        Write-Host "   Set the OPENAI_API_KEY environment variable or update appsettings.json" -ForegroundColor Yellow
    }
    
    # Generate a random JWT secret for development
    $jwtSecret = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString()))
    [Environment]::SetEnvironmentVariable("JWT_SECRET", $jwtSecret, "User")
    Write-Host "✅ JWT secret generated and set." -ForegroundColor Green
}

# Setup backend
function Set-Backend {
    if ($SkipDotnet) {
        Write-Host "⏭️  Skipping .NET backend setup." -ForegroundColor Yellow
        return
    }
    
    Write-Host "🔨 Setting up .NET backend..." -ForegroundColor Yellow
    
    Push-Location "backend"
    
    try {
        # Restore packages
        Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Cyan
        dotnet restore
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to restore NuGet packages"
        }
        
        # Build the solution
        Write-Host "🔨 Building the solution..." -ForegroundColor Cyan
        dotnet build --configuration Debug
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build the solution"
        }
        
        Write-Host "✅ Backend setup completed successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Backend setup failed: $_" -ForegroundColor Red
        exit 1
    }
    finally {
        Pop-Location
    }
}

# Setup frontend
function Set-Frontend {
    if ($SkipNode) {
        Write-Host "⏭️  Skipping Node.js frontend setup." -ForegroundColor Yellow
        return
    }
    
    Write-Host "⚛️  Setting up React frontend..." -ForegroundColor Yellow
    
    Push-Location "frontend"
    
    try {
        # Install npm packages
        Write-Host "📦 Installing npm packages..." -ForegroundColor Cyan
        npm install
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to install npm packages"
        }
        
        # Run type checking
        Write-Host "🔍 Running TypeScript type checking..." -ForegroundColor Cyan
        npm run type-check
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "⚠️  TypeScript type checking found issues, but continuing..." -ForegroundColor Yellow
        }
        
        Write-Host "✅ Frontend setup completed successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Frontend setup failed: $_" -ForegroundColor Red
        exit 1
    }
    finally {
        Pop-Location
    }
}

# Setup Docker environment
function Set-DockerEnvironment {
    if ($SkipDocker) {
        Write-Host "⏭️  Skipping Docker setup." -ForegroundColor Yellow
        return
    }
    
    Write-Host "🐳 Setting up Docker environment..." -ForegroundColor Yellow
    
    try {
        # Create necessary directories
        $directories = @("logs", "database/init", "redis", "nginx", "monitoring")
        foreach ($dir in $directories) {
            if (-not (Test-Path $dir)) {
                New-Item -ItemType Directory -Path $dir -Force | Out-Null
                Write-Host "📁 Created directory: $dir" -ForegroundColor Cyan
            }
        }
        
        # Create Redis configuration
        $redisConfig = @"
# Redis configuration for BI Copilot
bind 0.0.0.0
port 6379
timeout 0
tcp-keepalive 300
daemonize no
supervised no
pidfile /var/run/redis_6379.pid
loglevel notice
logfile ""
databases 16
save 900 1
save 300 10
save 60 10000
stop-writes-on-bgsave-error yes
rdbcompression yes
rdbchecksum yes
dbfilename dump.rdb
dir ./
maxmemory 256mb
maxmemory-policy allkeys-lru
"@
        
        $redisConfig | Out-File -FilePath "redis/redis.conf" -Encoding UTF8
        Write-Host "📝 Created Redis configuration." -ForegroundColor Cyan
        
        # Pull Docker images
        Write-Host "📥 Pulling Docker images..." -ForegroundColor Cyan
        docker-compose pull
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to pull Docker images"
        }
        
        Write-Host "✅ Docker environment setup completed." -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Docker setup failed: $_" -ForegroundColor Red
        exit 1
    }
}

# Create development database
function Initialize-Database {
    Write-Host "🗄️  Initializing development database..." -ForegroundColor Yellow
    
    try {
        # Start SQL Server container
        Write-Host "🚀 Starting SQL Server container..." -ForegroundColor Cyan
        docker-compose up -d sqlserver
        
        # Wait for SQL Server to be ready
        Write-Host "⏳ Waiting for SQL Server to be ready..." -ForegroundColor Cyan
        $timeout = 60
        $elapsed = 0
        
        do {
            Start-Sleep -Seconds 5
            $elapsed += 5
            $result = docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" 2>$null
        } while (-not $result -and $elapsed -lt $timeout)
        
        if ($elapsed -ge $timeout) {
            throw "SQL Server failed to start within $timeout seconds"
        }
        
        Write-Host "✅ SQL Server is ready." -ForegroundColor Green
        
        # Run database migrations
        Push-Location "backend"
        Write-Host "🔄 Running database migrations..." -ForegroundColor Cyan
        $env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=BIReportingCopilot_Dev;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
        dotnet ef database update --project BIReportingCopilot.Infrastructure --startup-project BIReportingCopilot.API
        Pop-Location
        
        Write-Host "✅ Database initialized successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Database initialization failed: $_" -ForegroundColor Red
        Write-Host "You may need to run database migrations manually." -ForegroundColor Yellow
    }
}

# Main execution
function Main {
    Write-Host @"
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║    🤖 AI-Powered BI Reporting Copilot Setup                 ║
║                                                              ║
║    This script will set up your development environment     ║
║    for the BI Reporting Copilot application.                ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan
    
    Test-Prerequisites
    Set-EnvironmentVariables
    Set-DockerEnvironment
    Set-Backend
    Set-Frontend
    
    if (-not $SkipDocker) {
        Initialize-Database
    }
    
    Write-Host @"

🎉 Setup completed successfully!

Next steps:
1. Set your OpenAI API key in appsettings.Development.json or environment variable
2. Start the development environment:
   docker-compose up -d

3. Access the application:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

4. For development:
   - Backend: cd backend && dotnet run --project BIReportingCopilot.API
   - Frontend: cd frontend && npm start

Happy coding! 🚀
"@ -ForegroundColor Green
}

# Run the main function
Main
