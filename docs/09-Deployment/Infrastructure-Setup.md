# Infrastructure Setup - Virtual Queue Management System

**Document Version:** 1.0  
**Date:** January 15, 2024  
**Author:** DevOps Lead  
**Status:** Draft  
**Phase:** 9 - Deployment  
**Priority:** 🔴 Critical  

---

## Executive Summary

This document provides comprehensive infrastructure setup guidelines for the Virtual Queue Management System. It covers cloud infrastructure provisioning, container orchestration, networking, security, monitoring, and disaster recovery to ensure robust and scalable system infrastructure.

## Infrastructure Overview

### **Infrastructure Objectives**

#### **Primary Objectives**
- **Scalability**: Design infrastructure for horizontal and vertical scaling
- **Reliability**: Ensure high availability and fault tolerance
- **Security**: Implement comprehensive security measures
- **Performance**: Optimize infrastructure for performance
- **Cost Optimization**: Balance performance with cost efficiency

#### **Infrastructure Benefits**
- **High Availability**: Minimize downtime and service interruptions
- **Scalability**: Handle varying loads efficiently
- **Security**: Protect against threats and vulnerabilities
- **Performance**: Deliver optimal user experience
- **Operational Efficiency**: Simplify operations and maintenance

### **Infrastructure Architecture**

#### **Architecture Components**
- **Application Layer**: Web API, Background Workers, SignalR Hub
- **Data Layer**: PostgreSQL Database, Redis Cache
- **Infrastructure Layer**: Load Balancer, CDN, Monitoring
- **Security Layer**: Firewall, SSL/TLS, Authentication
- **Network Layer**: VPC, Subnets, Security Groups

#### **Infrastructure Tiers**
```yaml
infrastructure_tiers:
  web_tier:
    components: ["Load Balancer", "Web Servers", "CDN"]
    scaling: "Auto-scaling groups"
    availability: "Multi-AZ deployment"
    security: "WAF, SSL/TLS"
  
  application_tier:
    components: ["API Servers", "Background Workers", "SignalR Hub"]
    scaling: "Container orchestration"
    availability: "Multi-instance deployment"
    security: "Application security"
  
  data_tier:
    components: ["PostgreSQL", "Redis", "File Storage"]
    scaling: "Read replicas, clustering"
    availability: "Multi-AZ, backups"
    security: "Encryption at rest"
  
  monitoring_tier:
    components: ["Prometheus", "Grafana", "AlertManager"]
    scaling: "Centralized monitoring"
    availability: "High availability setup"
    security: "Secure monitoring"
```

## Cloud Infrastructure

### **Azure Infrastructure**

#### **Azure Resource Group Setup**
```bash
#!/bin/bash
# azure-infrastructure-setup.sh

# Create resource group
az group create --name virtualqueue-rg --location eastus

# Create virtual network
az network vnet create \
  --resource-group virtualqueue-rg \
  --name virtualqueue-vnet \
  --address-prefix 10.0.0.0/16 \
  --subnet-name web-subnet \
  --subnet-prefix 10.0.1.0/24

# Create additional subnets
az network vnet subnet create \
  --resource-group virtualqueue-rg \
  --vnet-name virtualqueue-vnet \
  --name app-subnet \
  --address-prefix 10.0.2.0/24

az network vnet subnet create \
  --resource-group virtualqueue-rg \
  --vnet-name virtualqueue-vnet \
  --name data-subnet \
  --address-prefix 10.0.3.0/24

# Create network security groups
az network nsg create \
  --resource-group virtualqueue-rg \
  --name web-nsg

az network nsg create \
  --resource-group virtualqueue-rg \
  --name app-nsg

az network nsg create \
  --resource-group virtualqueue-rg \
  --name data-nsg

# Configure NSG rules
az network nsg rule create \
  --resource-group virtualqueue-rg \
  --nsg-name web-nsg \
  --name AllowHTTPS \
  --priority 100 \
  --source-address-prefixes '*' \
  --source-port-ranges '*' \
  --destination-address-prefixes '*' \
  --destination-port-ranges 443 \
  --access Allow \
  --protocol Tcp

az network nsg rule create \
  --resource-group virtualqueue-rg \
  --nsg-name web-nsg \
  --name AllowHTTP \
  --priority 110 \
  --source-address-prefixes '*' \
  --source-port-ranges '*' \
  --destination-address-prefixes '*' \
  --destination-port-ranges 80 \
  --access Allow \
  --protocol Tcp
```

#### **Azure App Service Setup**
```bash
# Create App Service Plan
az appservice plan create \
  --resource-group virtualqueue-rg \
  --name virtualqueue-plan \
  --sku P1V2 \
  --is-linux

# Create App Service for API
az webapp create \
  --resource-group virtualqueue-rg \
  --plan virtualqueue-plan \
  --name virtualqueue-api \
  --runtime "DOTNET|8.0"

# Create App Service for Worker
az webapp create \
  --resource-group virtualqueue-rg \
  --plan virtualqueue-plan \
  --name virtualqueue-worker \
  --runtime "DOTNET|8.0"

# Configure App Service settings
az webapp config appsettings set \
  --resource-group virtualqueue-rg \
  --name virtualqueue-api \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Host=virtualqueue-db.postgres.database.azure.com;Database=virtualqueue;Username=postgres;Password=$DB_PASSWORD" \
    Redis__ConnectionString="virtualqueue-redis.redis.cache.windows.net:6380,ssl=true,password=$REDIS_PASSWORD"

# Enable Always On
az webapp config set \
  --resource-group virtualqueue-rg \
  --name virtualqueue-api \
  --always-on true

# Configure auto-scaling
az monitor autoscale create \
  --resource-group virtualqueue-rg \
  --resource virtualqueue-api \
  --resource-type Microsoft.Web/sites \
  --name virtualqueue-autoscale \
  --min-count 2 \
  --max-count 10 \
  --count 2
```

#### **Azure Database Setup**
```bash
# Create PostgreSQL Flexible Server
az postgres flexible-server create \
  --resource-group virtualqueue-rg \
  --name virtualqueue-db \
  --admin-user postgres \
  --admin-password $DB_PASSWORD \
  --sku-name Standard_D2s_v3 \
  --tier GeneralPurpose \
  --storage-size 128 \
  --version 15 \
  --location eastus \
  --public-access 0.0.0.0-255.255.255.255

# Create database
az postgres flexible-server db create \
  --resource-group virtualqueue-rg \
  --server-name virtualqueue-db \
  --database-name virtualqueue

# Configure firewall rules
az postgres flexible-server firewall-rule create \
  --resource-group virtualqueue-rg \
  --name virtualqueue-db \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Enable backup
az postgres flexible-server parameter set \
  --resource-group virtualqueue-rg \
  --server-name virtualqueue-db \
  --name backup_retention_days \
  --value 7
```

#### **Azure Redis Setup**
```bash
# Create Redis Cache
az redis create \
  --resource-group virtualqueue-rg \
  --name virtualqueue-redis \
  --location eastus \
  --sku Premium \
  --vm-size P1 \
  --enable-non-ssl-port false

# Configure Redis settings
az redis update \
  --resource-group virtualqueue-rg \
  --name virtualqueue-redis \
  --set "redisConfiguration.maxmemory-policy=allkeys-lru"

# Enable clustering
az redis update \
  --resource-group virtualqueue-rg \
  --name virtualqueue-redis \
  --set "redisConfiguration.cluster-enabled=true"
```

### **AWS Infrastructure**

#### **AWS CloudFormation Template**
```yaml
# aws-infrastructure.yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Virtual Queue Management System Infrastructure'

Parameters:
  Environment:
    Type: String
    Default: production
    AllowedValues: [development, staging, production]
  
  InstanceType:
    Type: String
    Default: t3.medium
    AllowedValues: [t3.small, t3.medium, t3.large, t3.xlarge]

Resources:
  VPC:
    Type: AWS::EC2::VPC
    Properties:
      CidrBlock: 10.0.0.0/16
      EnableDnsHostnames: true
      EnableDnsSupport: true
      Tags:
        - Key: Name
          Value: VirtualQueue-VPC

  PublicSubnet1:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      CidrBlock: 10.0.1.0/24
      AvailabilityZone: !Select [0, !GetAZs '']
      MapPublicIpOnLaunch: true
      Tags:
        - Key: Name
          Value: VirtualQueue-Public-Subnet-1

  PublicSubnet2:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      CidrBlock: 10.0.2.0/24
      AvailabilityZone: !Select [1, !GetAZs '']
      MapPublicIpOnLaunch: true
      Tags:
        - Key: Name
          Value: VirtualQueue-Public-Subnet-2

  PrivateSubnet1:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      CidrBlock: 10.0.3.0/24
      AvailabilityZone: !Select [0, !GetAZs '']
      Tags:
        - Key: Name
          Value: VirtualQueue-Private-Subnet-1

  PrivateSubnet2:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      CidrBlock: 10.0.4.0/24
      AvailabilityZone: !Select [1, !GetAZs '']
      Tags:
        - Key: Name
          Value: VirtualQueue-Private-Subnet-2

  InternetGateway:
    Type: AWS::EC2::InternetGateway
    Properties:
      Tags:
        - Key: Name
          Value: VirtualQueue-IGW

  InternetGatewayAttachment:
    Type: AWS::EC2::VPCGatewayAttachment
    Properties:
      InternetGatewayId: !Ref InternetGateway
      VpcId: !Ref VPC

  PublicRouteTable:
    Type: AWS::EC2::RouteTable
    Properties:
      VpcId: !Ref VPC
      Tags:
        - Key: Name
          Value: VirtualQueue-Public-RouteTable

  DefaultPublicRoute:
    Type: AWS::EC2::Route
    DependsOn: InternetGatewayAttachment
    Properties:
      RouteTableId: !Ref PublicRouteTable
      DestinationCidrBlock: 0.0.0.0/0
      GatewayId: !Ref InternetGateway

  PublicSubnet1RouteTableAssociation:
    Type: AWS::EC2::SubnetRouteTableAssociation
    Properties:
      RouteTableId: !Ref PublicRouteTable
      SubnetId: !Ref PublicSubnet1

  PublicSubnet2RouteTableAssociation:
    Type: AWS::EC2::SubnetRouteTableAssociation
    Properties:
      RouteTableId: !Ref PublicRouteTable
      SubnetId: !Ref PublicSubnet2

  ECSCluster:
    Type: AWS::ECS::Cluster
    Properties:
      ClusterName: VirtualQueue-Cluster
      CapacityProviders:
        - FARGATE
        - FARGATE_SPOT
      DefaultCapacityProviderStrategy:
        - CapacityProvider: FARGATE
          Weight: 1

  ECSClusterCapacityProvider:
    Type: AWS::ECS::ClusterCapacityProviderAssociations
    Properties:
      Cluster: !Ref ECSCluster
      CapacityProviders:
        - FARGATE
        - FARGATE_SPOT
      DefaultCapacityProviderStrategy:
        - CapacityProvider: FARGATE
          Weight: 1

  DatabaseSubnetGroup:
    Type: AWS::RDS::DBSubnetGroup
    Properties:
      DBSubnetGroupDescription: Subnet group for VirtualQueue database
      SubnetIds:
        - !Ref PrivateSubnet1
        - !Ref PrivateSubnet2
      Tags:
        - Key: Name
          Value: VirtualQueue-DB-SubnetGroup

  Database:
    Type: AWS::RDS::DBInstance
    Properties:
      DBInstanceIdentifier: virtualqueue-db
      DBInstanceClass: db.t3.medium
      Engine: postgres
      EngineVersion: '15.4'
      MasterUsername: postgres
      MasterUserPassword: !Ref DatabasePassword
      AllocatedStorage: 100
      StorageType: gp2
      DBSubnetGroupName: !Ref DatabaseSubnetGroup
      VPCSecurityGroups:
        - !Ref DatabaseSecurityGroup
      BackupRetentionPeriod: 7
      MultiAZ: true
      StorageEncrypted: true
      Tags:
        - Key: Name
          Value: VirtualQueue-Database

  DatabaseSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Security group for VirtualQueue database
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 5432
          ToPort: 5432
          SourceSecurityGroupId: !Ref ApplicationSecurityGroup
      Tags:
        - Key: Name
          Value: VirtualQueue-DB-SecurityGroup

  ApplicationSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Security group for VirtualQueue application
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 80
          ToPort: 80
          SourceSecurityGroupId: !Ref LoadBalancerSecurityGroup
        - IpProtocol: tcp
          FromPort: 443
          ToPort: 443
          SourceSecurityGroupId: !Ref LoadBalancerSecurityGroup
      Tags:
        - Key: Name
          Value: VirtualQueue-App-SecurityGroup

  LoadBalancerSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Security group for VirtualQueue load balancer
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 80
          ToPort: 80
          CidrIp: 0.0.0.0/0
        - IpProtocol: tcp
          FromPort: 443
          ToPort: 443
          CidrIp: 0.0.0.0/0
      Tags:
        - Key: Name
          Value: VirtualQueue-LB-SecurityGroup

Outputs:
  VPCId:
    Description: VPC ID
    Value: !Ref VPC
    Export:
      Name: VirtualQueue-VPC-ID

  PublicSubnet1Id:
    Description: Public Subnet 1 ID
    Value: !Ref PublicSubnet1
    Export:
      Name: VirtualQueue-Public-Subnet-1-ID

  PublicSubnet2Id:
    Description: Public Subnet 2 ID
    Value: !Ref PublicSubnet2
    Export:
      Name: VirtualQueue-Public-Subnet-2-ID

  PrivateSubnet1Id:
    Description: Private Subnet 1 ID
    Value: !Ref PrivateSubnet1
    Export:
      Name: VirtualQueue-Private-Subnet-1-ID

  PrivateSubnet2Id:
    Description: Private Subnet 2 ID
    Value: !Ref PrivateSubnet2
    Export:
      Name: VirtualQueue-Private-Subnet-2-ID

  ECSClusterName:
    Description: ECS Cluster Name
    Value: !Ref ECSCluster
    Export:
      Name: VirtualQueue-ECS-Cluster-Name

  DatabaseEndpoint:
    Description: Database Endpoint
    Value: !GetAtt Database.Endpoint.Address
    Export:
      Name: VirtualQueue-Database-Endpoint
```

## Container Orchestration

### **Docker Configuration**

#### **Dockerfile for API**
```dockerfile
# Dockerfile.api
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/VirtualQueue.Api/VirtualQueue.Api.csproj", "src/VirtualQueue.Api/"]
COPY ["src/VirtualQueue.Application/VirtualQueue.Application.csproj", "src/VirtualQueue.Application/"]
COPY ["src/VirtualQueue.Domain/VirtualQueue.Domain.csproj", "src/VirtualQueue.Domain/"]
COPY ["src/VirtualQueue.Infrastructure/VirtualQueue.Infrastructure.csproj", "src/VirtualQueue.Infrastructure/"]
RUN dotnet restore "src/VirtualQueue.Api/VirtualQueue.Api.csproj"
COPY . .
WORKDIR "/src/src/VirtualQueue.Api"
RUN dotnet build "VirtualQueue.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VirtualQueue.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualQueue.Api.dll"]
```

#### **Dockerfile for Worker**
```dockerfile
# Dockerfile.worker
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/VirtualQueue.Worker/VirtualQueue.Worker.csproj", "src/VirtualQueue.Worker/"]
COPY ["src/VirtualQueue.Application/VirtualQueue.Application.csproj", "src/VirtualQueue.Application/"]
COPY ["src/VirtualQueue.Domain/VirtualQueue.Domain.csproj", "src/VirtualQueue.Domain/"]
COPY ["src/VirtualQueue.Infrastructure/VirtualQueue.Infrastructure.csproj", "src/VirtualQueue.Infrastructure/"]
RUN dotnet restore "src/VirtualQueue.Worker/VirtualQueue.Worker.csproj"
COPY . .
WORKDIR "/src/src/VirtualQueue.Worker"
RUN dotnet build "VirtualQueue.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VirtualQueue.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualQueue.Worker.dll"]
```

#### **Docker Compose**
```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile.api
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=virtualqueue;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis
    networks:
      - virtualqueue-network

  worker:
    build:
      context: .
      dockerfile: Dockerfile.worker
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=virtualqueue;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis
    networks:
      - virtualqueue-network

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_DB=virtualqueue
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - virtualqueue-network

  redis:
    image: redis:7
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - virtualqueue-network

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - api
    networks:
      - virtualqueue-network

volumes:
  postgres_data:
  redis_data:

networks:
  virtualqueue-network:
    driver: bridge
```

### **Kubernetes Configuration**

#### **Kubernetes Deployment**
```yaml
# k8s-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: virtualqueue-api
  labels:
    app: virtualqueue-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: virtualqueue-api
  template:
    metadata:
      labels:
        app: virtualqueue-api
    spec:
      containers:
      - name: api
        image: virtualqueue/api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: virtualqueue-secrets
              key: database-connection
        - name: Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: virtualqueue-secrets
              key: redis-connection
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
apiVersion: apps/v1
kind: Deployment
metadata:
  name: virtualqueue-worker
  labels:
    app: virtualqueue-worker
spec:
  replicas: 2
  selector:
    matchLabels:
      app: virtualqueue-worker
  template:
    metadata:
      labels:
        app: virtualqueue-worker
    spec:
      containers:
      - name: worker
        image: virtualqueue/worker:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: virtualqueue-secrets
              key: database-connection
        - name: Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: virtualqueue-secrets
              key: redis-connection
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"

---
apiVersion: v1
kind: Service
metadata:
  name: virtualqueue-api-service
spec:
  selector:
    app: virtualqueue-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer

---
apiVersion: v1
kind: Secret
metadata:
  name: virtualqueue-secrets
type: Opaque
data:
  database-connection: <base64-encoded-connection-string>
  redis-connection: <base64-encoded-redis-connection-string>
```

## Security Configuration

### **Network Security**

#### **Security Groups Configuration**
```bash
# Create security groups for different tiers
aws ec2 create-security-group \
  --group-name virtualqueue-web-sg \
  --description "Security group for web tier" \
  --vpc-id vpc-12345678

aws ec2 create-security-group \
  --group-name virtualqueue-app-sg \
  --description "Security group for application tier" \
  --vpc-id vpc-12345678

aws ec2 create-security-group \
  --group-name virtualqueue-data-sg \
  --description "Security group for data tier" \
  --vpc-id vpc-12345678

# Configure security group rules
aws ec2 authorize-security-group-ingress \
  --group-id sg-web123456 \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0

aws ec2 authorize-security-group-ingress \
  --group-id sg-web123456 \
  --protocol tcp \
  --port 443 \
  --cidr 0.0.0.0/0

aws ec2 authorize-security-group-ingress \
  --group-id sg-app123456 \
  --protocol tcp \
  --port 80 \
  --source-group sg-web123456

aws ec2 authorize-security-group-ingress \
  --group-id sg-app123456 \
  --protocol tcp \
  --port 443 \
  --source-group sg-web123456

aws ec2 authorize-security-group-ingress \
  --group-id sg-data123456 \
  --protocol tcp \
  --port 5432 \
  --source-group sg-app123456

aws ec2 authorize-security-group-ingress \
  --group-id sg-data123456 \
  --protocol tcp \
  --port 6379 \
  --source-group sg-app123456
```

#### **SSL/TLS Configuration**
```bash
# Request SSL certificate
aws acm request-certificate \
  --domain-name virtualqueue.example.com \
  --subject-alternative-names "*.virtualqueue.example.com" \
  --validation-method DNS

# Configure SSL certificate in load balancer
aws elbv2 create-listener \
  --load-balancer-arn arn:aws:elasticloadbalancing:us-east-1:123456789012:loadbalancer/app/virtualqueue-lb/1234567890123456 \
  --protocol HTTPS \
  --port 443 \
  --certificates CertificateArn=arn:aws:acm:us-east-1:123456789012:certificate/12345678-1234-1234-1234-123456789012 \
  --default-actions Type=forward,TargetGroupArn=arn:aws:elasticloadbalancing:us-east-1:123456789012:targetgroup/virtualqueue-tg/1234567890123456
```

### **Identity and Access Management**

#### **IAM Roles and Policies**
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "rds:DescribeDBInstances",
        "rds:DescribeDBClusters",
        "rds:DescribeDBSubnetGroups",
        "rds:DescribeDBParameterGroups",
        "rds:DescribeDBParameters"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "elasticache:DescribeCacheClusters",
        "elasticache:DescribeReplicationGroups",
        "elasticache:DescribeCacheSubnetGroups",
        "elasticache:DescribeCacheParameterGroups",
        "elasticache:DescribeCacheParameters"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:DescribeLogGroups",
        "logs:DescribeLogStreams"
      ],
      "Resource": "*"
    }
  ]
}
```

## Monitoring and Logging

### **Monitoring Setup**

#### **Prometheus Configuration**
```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "rules/*.yml"

scrape_configs:
  - job_name: 'virtualqueue-api'
    static_configs:
      - targets: ['virtualqueue-api:80']
    metrics_path: /metrics
    scrape_interval: 5s

  - job_name: 'virtualqueue-worker'
    static_configs:
      - targets: ['virtualqueue-worker:80']
    metrics_path: /metrics
    scrape_interval: 5s

  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres-exporter:9187']

  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093
```

#### **Grafana Dashboard**
```json
{
  "dashboard": {
    "title": "Virtual Queue System Dashboard",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{endpoint}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      },
      {
        "title": "Error Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total{status=~\"5..\"}[5m])",
            "legendFormat": "5xx errors"
          }
        ]
      },
      {
        "title": "Active Connections",
        "type": "singlestat",
        "targets": [
          {
            "expr": "active_connections",
            "legendFormat": "Active Connections"
          }
        ]
      }
    ]
  }
}
```

## Disaster Recovery

### **Backup Strategy**

#### **Database Backup**
```bash
#!/bin/bash
# database-backup.sh

# Create backup directory
mkdir -p /backups/$(date +%Y%m%d)

# Create database backup
pg_dump -h $DB_HOST -U $DB_USER -d $DB_NAME > /backups/$(date +%Y%m%d)/virtualqueue_$(date +%Y%m%d_%H%M%S).sql

# Compress backup
gzip /backups/$(date +%Y%m%d)/virtualqueue_$(date +%Y%m%d_%H%M%S).sql

# Upload to S3
aws s3 cp /backups/$(date +%Y%m%d)/virtualqueue_$(date +%Y%m%d_%H%M%S).sql.gz s3://virtualqueue-backups/database/

# Clean up old backups (keep 30 days)
find /backups -type d -mtime +30 -exec rm -rf {} \;
```

#### **Redis Backup**
```bash
#!/bin/bash
# redis-backup.sh

# Create Redis backup
redis-cli --rdb /backups/$(date +%Y%m%d)/redis_$(date +%Y%m%d_%H%M%S).rdb

# Compress backup
gzip /backups/$(date +%Y%m%d)/redis_$(date +%Y%m%d_%H%M%S).rdb

# Upload to S3
aws s3 cp /backups/$(date +%Y%m%d)/redis_$(date +%Y%m%d_%H%M%S).rdb.gz s3://virtualqueue-backups/redis/

# Clean up old backups (keep 30 days)
find /backups -type f -name "redis_*.rdb.gz" -mtime +30 -delete
```

### **Recovery Procedures**

#### **Database Recovery**
```bash
#!/bin/bash
# database-recovery.sh

# Download backup from S3
aws s3 cp s3://virtualqueue-backups/database/virtualqueue_20240115_120000.sql.gz /tmp/

# Decompress backup
gunzip /tmp/virtualqueue_20240115_120000.sql.gz

# Restore database
psql -h $DB_HOST -U $DB_USER -d $DB_NAME < /tmp/virtualqueue_20240115_120000.sql

# Verify restoration
psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c "SELECT COUNT(*) FROM tenants;"
```

## Approval and Sign-off

### **Infrastructure Setup Approval**
- **DevOps Lead**: [Name] - [Date]
- **Security Lead**: [Name] - [Date]
- **Development Lead**: [Name] - [Date]
- **Management**: [Name] - [Date]

### **Document Control**
- **Version**: 1.0
- **Last Updated**: January 15, 2024
- **Next Review**: February 15, 2024
- **Distribution**: DevOps Team, Security Team, Development Team, Management

---

**Document Status**: Draft  
**Next Phase**: CI/CD Pipeline  
**Dependencies**: Infrastructure provisioning, security configuration, monitoring setup
