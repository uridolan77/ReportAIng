# Deployment and Infrastructure Requirements

## Overview
This document outlines the comprehensive deployment strategy and infrastructure requirements for the AI-Powered BI Reporting Copilot system, designed for enterprise-scale deployment with high availability, security, and performance.

## Infrastructure Architecture

### Production Environment Architecture
```
┌─────────────────────────────────────────────────────────────────┐
│                        Azure Cloud Environment                  │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐    ┌─────────────────┐    ┌──────────────┐ │
│  │   Azure CDN     │    │  Application    │    │   Azure SQL  │ │
│  │   + WAF         │    │   Gateway       │    │   Database   │ │
│  └─────────────────┘    └─────────────────┘    └──────────────┘ │
│           │                       │                      │      │
│  ┌─────────────────┐    ┌─────────────────┐    ┌──────────────┐ │
│  │   React SPA     │    │  AKS Cluster    │    │   Redis      │ │
│  │   (Static Web   │    │  - API Pods     │    │   Cache      │ │
│  │    App)         │    │  - SignalR Hubs │    │   Cluster    │ │
│  └─────────────────┘    └─────────────────┘    └──────────────┘ │
│                                 │                               │
│  ┌─────────────────┐    ┌─────────────────┐    ┌──────────────┐ │
│  │   Azure         │    │   Azure         │    │   Azure      │ │
│  │   OpenAI        │    │   Key Vault     │    │   Monitor    │ │
│  │   Service       │    │                 │    │   + Logs     │ │
│  └─────────────────┘    └─────────────────┘    └──────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## 1. Container Strategy

### Docker Configuration

#### Backend API Dockerfile
```dockerfile
# Multi-stage build for optimized production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BIReportingCopilot.API/BIReportingCopilot.API.csproj", "BIReportingCopilot.API/"]
COPY ["BIReportingCopilot.Core/BIReportingCopilot.Core.csproj", "BIReportingCopilot.Core/"]
COPY ["BIReportingCopilot.Infrastructure/BIReportingCopilot.Infrastructure.csproj", "BIReportingCopilot.Infrastructure/"]

RUN dotnet restore "BIReportingCopilot.API/BIReportingCopilot.API.csproj"
COPY . .
WORKDIR "/src/BIReportingCopilot.API"
RUN dotnet build "BIReportingCopilot.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BIReportingCopilot.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "BIReportingCopilot.API.dll"]
```

#### Frontend Dockerfile
```dockerfile
# Build stage
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production

COPY . .
RUN npm run build

# Production stage
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

# Security headers and optimizations
RUN apk add --no-cache curl
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/ || exit 1

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Kubernetes Deployment

#### API Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bi-copilot-api
  namespace: bi-copilot
spec:
  replicas: 3
  selector:
    matchLabels:
      app: bi-copilot-api
  template:
    metadata:
      labels:
        app: bi-copilot-api
    spec:
      containers:
      - name: api
        image: bicopilot.azurecr.io/bi-copilot-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-connection
              key: connection-string
        - name: OpenAI__ApiKey
          valueFrom:
            secretKeyRef:
              name: openai-secret
              key: api-key
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: bi-copilot-api-service
  namespace: bi-copilot
spec:
  selector:
    app: bi-copilot-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP
```

#### Horizontal Pod Autoscaler
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: bi-copilot-api-hpa
  namespace: bi-copilot
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: bi-copilot-api
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

## 2. Database Infrastructure

### Azure SQL Database Configuration
```sql
-- Production database configuration
CREATE DATABASE [BIReportingCopilot]
(
    EDITION = 'Premium',
    SERVICE_OBJECTIVE = 'P2',
    MAXSIZE = 500GB
);

-- Enable advanced security features
ALTER DATABASE [BIReportingCopilot] 
SET QUERY_STORE = ON;

-- Configure backup retention
ALTER DATABASE [BIReportingCopilot]
SET BACKUP_RETENTION_PERIOD = 35; -- 35 days
```

### Database Connection Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:bi-copilot-sql.database.windows.net,1433;Initial Catalog=BIReportingCopilot;Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "ReadOnlyConnection": "Server=tcp:bi-copilot-sql-readonly.database.windows.net,1433;Initial Catalog=BIReportingCopilot;Persist Security Info=False;User ID={readonly_username};Password={readonly_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;ApplicationIntent=ReadOnly;"
  }
}
```

## 3. Caching Strategy

### Redis Configuration
```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: redis-cluster
  namespace: bi-copilot
spec:
  serviceName: redis-cluster
  replicas: 3
  selector:
    matchLabels:
      app: redis-cluster
  template:
    metadata:
      labels:
        app: redis-cluster
    spec:
      containers:
      - name: redis
        image: redis:7-alpine
        ports:
        - containerPort: 6379
        command:
        - redis-server
        - /etc/redis/redis.conf
        volumeMounts:
        - name: redis-config
          mountPath: /etc/redis
        - name: redis-data
          mountPath: /data
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
      volumes:
      - name: redis-config
        configMap:
          name: redis-config
  volumeClaimTemplates:
  - metadata:
      name: redis-data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 10Gi
```

## 4. Security Configuration

### Network Security
```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: bi-copilot-network-policy
  namespace: bi-copilot
spec:
  podSelector:
    matchLabels:
      app: bi-copilot-api
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 80
  egress:
  - to: []
    ports:
    - protocol: TCP
      port: 443  # HTTPS outbound
    - protocol: TCP
      port: 1433 # SQL Server
    - protocol: TCP
      port: 6379 # Redis
```

### SSL/TLS Configuration
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: bi-copilot-ingress
  namespace: bi-copilot
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - bi-copilot.company.com
    secretName: bi-copilot-tls
  rules:
  - host: bi-copilot.company.com
    http:
      paths:
      - path: /api
        pathType: Prefix
        backend:
          service:
            name: bi-copilot-api-service
            port:
              number: 80
      - path: /
        pathType: Prefix
        backend:
          service:
            name: bi-copilot-frontend-service
            port:
              number: 80
```

## 5. Monitoring and Observability

### Application Insights Configuration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = Configuration.GetConnectionString("ApplicationInsights");
        options.EnableAdaptiveSampling = true;
        options.EnableQuickPulseMetricStream = true;
    });

    services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
}
```

### Prometheus Metrics
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-config
  namespace: monitoring
data:
  prometheus.yml: |
    global:
      scrape_interval: 15s
    scrape_configs:
    - job_name: 'bi-copilot-api'
      kubernetes_sd_configs:
      - role: pod
      relabel_configs:
      - source_labels: [__meta_kubernetes_pod_label_app]
        action: keep
        regex: bi-copilot-api
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
```

## 6. CI/CD Pipeline

### Azure DevOps Pipeline
```yaml
trigger:
  branches:
    include:
    - main
    - develop

variables:
  buildConfiguration: 'Release'
  containerRegistry: 'bicopilot.azurecr.io'

stages:
- stage: Build
  jobs:
  - job: BuildAndTest
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: 'sdk'
        version: '8.0.x'
    
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: '**/*.csproj'
    
    - task: DotNetCoreCLI@2
      displayName: 'Build application'
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: '**/*Tests.csproj'
        arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage"'
    
    - task: Docker@2
      displayName: 'Build and push API image'
      inputs:
        containerRegistry: '$(containerRegistry)'
        repository: 'bi-copilot-api'
        command: 'buildAndPush'
        Dockerfile: '**/Dockerfile'
        tags: |
          $(Build.BuildId)
          latest

- stage: Deploy
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: DeployToProduction
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: KubernetesManifest@0
            displayName: 'Deploy to AKS'
            inputs:
              action: 'deploy'
              kubernetesServiceConnection: 'aks-connection'
              namespace: 'bi-copilot'
              manifests: |
                k8s/deployment.yaml
                k8s/service.yaml
                k8s/ingress.yaml
```

## 7. Disaster Recovery and Backup

### Backup Strategy
- **Database**: Automated daily backups with 35-day retention
- **Application State**: Redis persistence with daily snapshots
- **Configuration**: Git-based infrastructure as code
- **Secrets**: Azure Key Vault with geo-replication

### Recovery Procedures
```bash
#!/bin/bash
# Disaster recovery script

# 1. Restore database from backup
az sql db restore \
  --dest-name BIReportingCopilot-restored \
  --edition Premium \
  --service-objective P2 \
  --resource-group bi-copilot-rg \
  --server bi-copilot-sql \
  --source-database BIReportingCopilot \
  --time "2024-01-15T10:00:00Z"

# 2. Update connection strings
kubectl patch secret db-connection \
  -n bi-copilot \
  --type='json' \
  -p='[{"op": "replace", "path": "/data/connection-string", "value":"'$(echo -n $NEW_CONNECTION_STRING | base64)'"}]'

# 3. Restart application pods
kubectl rollout restart deployment/bi-copilot-api -n bi-copilot
```

## 8. Performance Requirements

### Minimum Infrastructure Specifications

#### Production Environment
- **AKS Cluster**: 3 nodes, Standard_D4s_v3 (4 vCPU, 16 GB RAM)
- **Database**: Azure SQL Premium P2 (250 DTU, 500 GB)
- **Cache**: Azure Cache for Redis Premium P1 (6 GB)
- **Storage**: Premium SSD with 1000 IOPS minimum

#### Staging Environment
- **AKS Cluster**: 2 nodes, Standard_D2s_v3 (2 vCPU, 8 GB RAM)
- **Database**: Azure SQL Standard S2 (50 DTU, 250 GB)
- **Cache**: Azure Cache for Redis Standard C1 (1 GB)

### Performance Targets
- **API Response Time**: <500ms for 95th percentile
- **Query Execution**: <2 seconds for simple queries, <10 seconds for complex queries
- **Concurrent Users**: Support 100+ concurrent users
- **Availability**: 99.9% uptime SLA
- **Scalability**: Auto-scale from 3 to 10 pods based on load

This infrastructure setup provides enterprise-grade deployment capabilities with high availability, security, and performance for the AI-Powered BI Reporting Copilot system.
