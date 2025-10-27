# Virtual Queue Management System

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13+-blue.svg)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-6.0+-red.svg)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-Supported-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A comprehensive, enterprise-grade Virtual Queue Management System built with .NET 8, Domain-Driven Design (DDD), CQRS, and Clean Architecture. This system provides complete queue management capabilities with multi-tenancy, real-time features, advanced analytics, and mobile support.

## 🚀 Features

### Core Features
- **Multi-tenant Architecture**: Isolated environments for different organizations
- **Real-time Queue Management**: Live updates and notifications
- **User Management**: Complete user lifecycle with roles and permissions
- **Advanced Analytics**: Comprehensive reporting and insights
- **Mobile Support**: Native mobile apps for iOS and Android
- **Queue Templates**: Pre-configured queue setups for quick deployment
- **Queue Merging**: Advanced queue operations and load balancing

### Enterprise Features
- **Authentication & Authorization**: JWT tokens, 2FA, role-based access
- **Rate Limiting**: Redis-based rate limiting and throttling
- **Audit Logging**: Comprehensive action tracking and compliance
- **Backup & Restore**: Automated backup and disaster recovery
- **Template Management**: Customizable templates for notifications
- **Webhook Integration**: Event-driven integration with external systems
- **Alert Management**: Configurable alerts and notifications
- **Data Retention**: Automated data lifecycle management

### Advanced Features
- **Queue Load Balancing**: Intelligent load distribution
- **Business Rules Engine**: Dynamic rule definition and execution
- **Performance Profiling**: Application performance monitoring
- **Third-party Integration**: Generic integration framework
- **Admin Dashboard**: System-wide monitoring and management
- **Mobile API**: Optimized mobile experience

## 🏗️ Architecture

### Technology Stack
- **.NET 8**: Latest .NET framework with C# 12
- **Entity Framework Core 8**: PostgreSQL data access
- **Redis**: Caching and real-time features
- **SignalR**: Real-time communication
- **MediatR**: CQRS implementation
- **FluentValidation**: Request validation
- **AutoMapper**: Object mapping
- **Serilog**: Structured logging
- **Prometheus**: Metrics collection
- **Grafana**: Monitoring dashboards

### Architecture Patterns
- **Domain-Driven Design (DDD)**: Business logic encapsulation
- **CQRS**: Command Query Responsibility Segregation
- **Clean Architecture**: Separation of concerns
- **Repository Pattern**: Data access abstraction
- **Service Provider Pattern**: Dependency injection

## 📁 Project Structure

```
VirtualQueue/
├── src/
│   ├── VirtualQueue.Domain/           # Domain layer (entities, events, value objects)
│   ├── VirtualQueue.Application/       # Application layer (commands, queries, services)
│   ├── VirtualQueue.Infrastructure/   # Infrastructure layer (data access, external services)
│   ├── VirtualQueue.Api/             # API layer (controllers, middleware)
│   └── VirtualQueue.Worker/          # Background worker service
├── tests/
│   ├── VirtualQueue.UnitTests/       # Unit tests
│   └── VirtualQueue.IntegrationTests/ # Integration tests
├── docs/                             # Documentation
├── docker-compose.yml               # Docker Compose configuration
└── README.md                        # This file
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL 13+
- Redis 6.0+
- Git

### 1. Clone the Repository
```bash
git clone https://github.com/your-org/virtual-queue-management.git
cd virtual-queue-management
```

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Configure Database
```sql
CREATE DATABASE virtualqueue;
CREATE USER virtualqueue_user WITH PASSWORD 'yourpassword';
GRANT ALL PRIVILEGES ON DATABASE virtualqueue TO virtualqueue_user;
```

### 4. Update Configuration
Edit `src/VirtualQueue.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=virtualqueue;Username=virtualqueue_user;Password=yourpassword",
    "Redis": "localhost:6379"
  }
}
```

### 5. Run Database Migrations
```bash
dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api
```

### 6. Start the Application
```bash
# Start the API
dotnet run --project src/VirtualQueue.Api

# Start the Worker (optional)
dotnet run --project src/VirtualQueue.Worker
```

### 7. Access the Application
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000
- **Health Check**: http://localhost:5000/healthz

## 🐳 Docker Deployment

### Using Docker Compose
```bash
# Clone the repository
git clone https://github.com/your-org/virtual-queue-management.git
cd virtual-queue-management

# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Services Included
- **API**: Virtual Queue API service
- **Worker**: Background worker service
- **PostgreSQL**: Database server
- **Redis**: Cache and real-time features
- **Grafana**: Monitoring dashboards

## 📚 Documentation

### Complete Documentation
- **[Quick Start Guide](docs/QUICK_START.md)**: Get started in 5 minutes
- **[API Documentation](docs/API_DOCUMENTATION.md)**: Complete API reference
- **[User Guide](docs/USER_GUIDE.md)**: End-user documentation
- **[Admin Guide](docs/ADMIN_GUIDE.md)**: Administrative documentation
- **[Setup Guide](docs/SETUP_GUIDE.md)**: Production deployment guide

### API Endpoints

#### Core API
- **Tenants**: `/api/v1/tenants` - Tenant management
- **Queues**: `/api/v1/tenants/{tenantId}/queues` - Queue management
- **User Sessions**: `/api/v1/tenants/{tenantId}/queues/{queueId}/usersessions` - User session management
- **Analytics**: `/api/v1/tenants/{tenantId}/analytics` - Analytics and reporting

#### Authentication
- **Login**: `/api/v1/auth/login` - User authentication
- **Register**: `/api/v1/auth/register` - User registration
- **Refresh Token**: `/api/v1/auth/refresh` - Token refresh
- **2FA**: `/api/v1/auth/2fa` - Two-factor authentication

#### Mobile API
- **Queue Status**: `/api/v1/mobile/queues/{queueId}/status` - Mobile queue status
- **User Sessions**: `/api/v1/mobile/users/{userId}/sessions` - Mobile user sessions
- **Notifications**: `/api/v1/mobile/notifications` - Mobile notifications
- **Location**: `/api/v1/mobile/location` - Location services

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Database=virtualqueue;Username=virtualqueue_user;Password=yourpassword"

# Redis
ConnectionStrings__Redis="localhost:6379"

# JWT Settings
JwtSettings__SecretKey="your-super-secret-jwt-key"
JwtSettings__Issuer="VirtualQueue"
JwtSettings__Audience="VirtualQueueUsers"
JwtSettings__ExpiryMinutes="60"

# Email Settings
EmailSettings__SmtpServer="smtp.gmail.com"
EmailSettings__SmtpPort="587"
EmailSettings__Username="your-email@gmail.com"
EmailSettings__Password="your-app-password"
```

### Security Configuration
- **JWT Authentication**: Secure API authentication
- **Rate Limiting**: Redis-based rate limiting
- **CORS**: Configurable cross-origin resource sharing
- **HTTPS**: SSL/TLS encryption
- **Audit Logging**: Comprehensive action tracking

## 📊 Monitoring

### Health Checks
- **API Health**: `/healthz` - API service health
- **Database Health**: Database connectivity
- **Redis Health**: Cache service health
- **System Health**: Overall system status

### Metrics
- **Prometheus**: Metrics collection at `/metrics`
- **Grafana**: Monitoring dashboards at http://localhost:3000
- **Custom Metrics**: Application-specific metrics
- **Performance Monitoring**: Response times and throughput

### Logging
- **Structured Logging**: JSON-formatted logs
- **Log Levels**: Configurable log levels
- **Log Aggregation**: Centralized log collection
- **Audit Trails**: User action tracking

## 🧪 Testing

### Unit Tests
```bash
dotnet test tests/VirtualQueue.UnitTests
```

### Integration Tests
```bash
dotnet test tests/VirtualQueue.IntegrationTests
```

### Test Coverage
- **Domain Logic**: Business rule testing
- **API Endpoints**: HTTP endpoint testing
- **Database Operations**: Data access testing
- **External Services**: Integration testing

## 🔒 Security

### Authentication Methods
- **JWT Tokens**: Secure API authentication
- **API Keys**: Programmatic access
- **Two-Factor Authentication**: Enhanced security
- **Single Sign-On**: SSO integration

### Security Features
- **Rate Limiting**: Request throttling
- **Input Validation**: Request data validation
- **SQL Injection Prevention**: Parameterized queries
- **XSS Protection**: Cross-site scripting prevention
- **CSRF Protection**: Cross-site request forgery prevention

## 🚀 Performance

### Performance Features
- **Redis Caching**: High-performance caching
- **Connection Pooling**: Database connection optimization
- **Async Operations**: Non-blocking I/O
- **Load Balancing**: Traffic distribution
- **Auto-scaling**: Automatic resource scaling

### Optimization
- **Database Indexing**: Query performance optimization
- **Query Optimization**: Efficient database queries
- **Memory Management**: Optimized memory usage
- **Response Compression**: Reduced payload sizes

## 🌐 Integration

### Supported Integrations
- **Email**: SMTP email notifications
- **SMS**: SMS notifications
- **Webhooks**: Event-driven integrations
- **Third-party APIs**: External service integration
- **Mobile Apps**: iOS and Android support

### API Integration
- **REST API**: Complete REST API
- **WebSocket**: Real-time communication
- **GraphQL**: Flexible data querying
- **gRPC**: High-performance RPC

## 📱 Mobile Support

### Mobile Features
- **Native Apps**: iOS and Android apps
- **Push Notifications**: Real-time mobile notifications
- **Offline Support**: Offline queue management
- **Location Services**: GPS-based features
- **Mobile API**: Optimized mobile endpoints

### Mobile API
- **Queue Status**: Real-time queue information
- **User Sessions**: Mobile user session management
- **Notifications**: Push notification management
- **Location**: GPS location services
- **Sync**: Data synchronization

## 🔧 Development

### Development Setup
```bash
# Clone repository
git clone https://github.com/your-org/virtual-queue-management.git
cd virtual-queue-management

# Install dependencies
dotnet restore

# Run database migrations
dotnet ef database update --project src/VirtualQueue.Infrastructure --startup-project src/VirtualQueue.Api

# Start development server
dotnet run --project src/VirtualQueue.Api
```

### Development Tools
- **Visual Studio 2022**: Full IDE support
- **VS Code**: Lightweight development
- **JetBrains Rider**: Cross-platform IDE
- **Postman**: API testing
- **DBeaver**: Database management

### Code Quality
- **Code Analysis**: Static code analysis
- **Unit Testing**: Comprehensive test coverage
- **Integration Testing**: End-to-end testing
- **Code Coverage**: Test coverage reporting
- **Code Review**: Peer code review process

## 📈 Roadmap

### Upcoming Features
- **Machine Learning**: AI-powered queue optimization
- **Advanced Analytics**: Predictive analytics
- **Multi-language Support**: Internationalization
- **Advanced Security**: Enhanced security features
- **Performance Optimization**: Further performance improvements

### Planned Integrations
- **CRM Systems**: Customer relationship management
- **ERP Systems**: Enterprise resource planning
- **Payment Gateways**: Payment processing
- **Social Media**: Social media integration
- **IoT Devices**: Internet of Things support

## 🤝 Contributing

### How to Contribute
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new features
5. Submit a pull request

### Development Guidelines
- Follow C# coding standards
- Write comprehensive tests
- Document new features
- Update documentation
- Follow semantic versioning

### Code of Conduct
- Be respectful and inclusive
- Follow community guidelines
- Report issues appropriately
- Help others learn and grow
- Maintain professional behavior

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

### Getting Help
- **Documentation**: Comprehensive guides and references
- **GitHub Issues**: Bug reports and feature requests
- **Discord Community**: Real-time community support
- **Stack Overflow**: Technical questions and answers
- **Email Support**: support@virtualqueue.com

### Professional Support
- **Enterprise Support**: Dedicated support for enterprise customers
- **Consulting Services**: Professional implementation services
- **Training Programs**: Comprehensive training and certification
- **Custom Development**: Custom feature development

## 🙏 Acknowledgments

- **.NET Community**: For the excellent .NET ecosystem
- **Open Source Contributors**: For their valuable contributions
- **Beta Testers**: For their feedback and testing
- **Community Members**: For their support and engagement

---

**🎉 Thank you for choosing the Virtual Queue Management System!**

For more information, visit our [documentation](docs/) or join our [community](https://discord.gg/virtualqueue).

**Built with ❤️ using .NET 8, DDD, CQRS, and Clean Architecture**