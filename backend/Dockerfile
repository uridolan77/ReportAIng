# Multi-stage build for optimized production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["BIReportingCopilot.API/BIReportingCopilot.API.csproj", "BIReportingCopilot.API/"]
COPY ["BIReportingCopilot.Core/BIReportingCopilot.Core.csproj", "BIReportingCopilot.Core/"]
COPY ["BIReportingCopilot.Infrastructure/BIReportingCopilot.Infrastructure.csproj", "BIReportingCopilot.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "BIReportingCopilot.API/BIReportingCopilot.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/BIReportingCopilot.API"
RUN dotnet build "BIReportingCopilot.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BIReportingCopilot.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "BIReportingCopilot.API.dll"]
