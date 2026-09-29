# Cloud-Native Microservices

A cloud-native microservices application built with **.NET 9**, **ASP.NET Core Web API**, **PostgreSQL**, **Entity Framework Core**, **RabbitMQ**, **JWT Authentication**, and **Docker**.

The project demonstrates how independent backend services can communicate through both REST APIs and asynchronous event-driven messaging while running together using Docker Compose.

---

## Architecture

The application currently consists of three microservices:

```text
                        ┌─────────────────┐
                        │     Client      │
                        └────────┬────────┘
                                 │
                    ┌────────────┴────────────┐
                    │                         │
                    ▼                         ▼
            ┌───────────────┐         ┌───────────────┐
            │  UserService  │         │ OrderService  │
            │               │         │               │
            │ Authentication│         │ Order APIs    │
            │ User APIs     │         │ PostgreSQL    │
            └───────┬───────┘         └───────┬───────┘
                    │                         │
                    ▼                         ▼
             ┌────────────┐            ┌────────────┐
             │ PostgreSQL │            │  RabbitMQ  │
             └────────────┘            └──────┬─────┘
                                              │
                                              ▼
                                    ┌───────────────────┐
                                    │NotificationService│
                                    │                   │
                                    │ Event Consumer    │
                                    └───────────────────┘
```

The services are designed to remain independently deployable while sharing common event contracts through the `Shared` project.

---

## Services

### UserService

Responsible for user-related operations and authentication.

Main responsibilities:

- User management
- Password hashing using BCrypt
- JWT token authentication
- PostgreSQL persistence
- Entity Framework Core
- Swagger/OpenAPI documentation
- Protected API endpoints

---

### OrderService

Responsible for handling order-related operations.

Main responsibilities:

- Order management
- PostgreSQL persistence
- Entity Framework Core
- JWT authentication
- Publishing events to RabbitMQ
- Swagger/OpenAPI documentation

When an order-related event occurs, the service can publish a message to RabbitMQ so other services can react without creating tight service-to-service dependencies.

---

### NotificationService

Responsible for processing asynchronous application events.

Main responsibilities:

- RabbitMQ message consumption
- Background event processing
- Order-created event handling
- Independent notification processing

The service uses an ASP.NET Core hosted background service to continuously listen for RabbitMQ events.

---

### Shared

Contains contracts and models that can be reused across multiple microservices.

This helps services agree on event structures without duplicating event definitions.

Example:

```text
OrderService
     │
     │ OrderCreatedEvent
     ▼
   RabbitMQ
     │
     ▼
NotificationService
```

---

# Technology Stack

| Technology | Purpose |
|---|---|
| .NET 9 | Application runtime |
| ASP.NET Core Web API | REST API framework |
| C# | Backend development |
| Entity Framework Core | ORM and database access |
| PostgreSQL | Relational database |
| Npgsql | PostgreSQL EF Core provider |
| JWT | API authentication |
| BCrypt | Password hashing |
| RabbitMQ | Asynchronous message broker |
| Docker | Application containerization |
| Docker Compose | Multi-container orchestration |
| Swagger / OpenAPI | API documentation |

---

# Project Structure

```text
cloud-native-microservices/
│
├── UserService/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Migrations/
│   ├── Program.cs
│   ├── Dockerfile
│   └── UserService.csproj
│
├── OrderService/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── Migrations/
│   ├── Program.cs
│   ├── Dockerfile
│   └── OrderService.csproj
│
├── NotificationService/
│   ├── Consumers/
│   ├── Controllers/
│   ├── Program.cs
│   ├── Dockerfile
│   └── NotificationService.csproj
│
├── Shared/
│   └── Shared.csproj
│
├── cloud-native-microservices.sln
├── docker-compose.yml
├── .gitignore
└── README.md
```

---

# Key Concepts Demonstrated

This project demonstrates several important microservices and cloud-native concepts.

### Microservice Separation

Each service has a separate responsibility.

For example:

- `UserService` handles identity and authentication.
- `OrderService` handles orders.

This avoids building a single tightly coupled application.

### Event-Driven Communication

RabbitMQ is used to communicate asynchronously between services.

For example:

```text
Order created
      │
      ▼
OrderService
      │
      │ Publish Event
      ▼
RabbitMQ
      │
      │ Consume Event
      ▼
NotificationService
```

The `OrderService` does not need to directly call the `NotificationService`.

### Database Persistence

PostgreSQL is used as the relational database.

Entity Framework Core provides database access and migration support.

### Authentication

JWT bearer authentication is configured for protected APIs.

The application validates:

- JWT issuer
- JWT audience
- JWT expiration
- JWT signing key

### Containerization

Each microservice can run inside a Docker container.

Docker Compose is used to start the complete local application stack.

---

# Docker Compose Architecture

The Docker Compose environment contains:

```text
PostgreSQL
RabbitMQ
UserService
OrderService
NotificationService
```

Default ports:

| Component | Port |
|---|---:|
| PostgreSQL | `5432` |
| RabbitMQ | `5672` |
| RabbitMQ Management UI | `15672` |
| UserService | `5001` |
| OrderService | `5002` |
| NotificationService | `5003` |

---

# Getting Started

## Prerequisites

Install the following tools:

- .NET 9 SDK
- Docker
- Docker Compose
- Git

If you plan to run the services outside Docker, PostgreSQL and RabbitMQ must also be available locally.

---

## Clone the Repository

```bash
git clone https://github.com/siri-chandanak/cloud-native-microservices.git

cd cloud-native-microservices
```

---

# Run Using Docker Compose

The easiest way to run the application is with Docker Compose.

```bash
docker compose up --build
```

This starts:

```text
PostgreSQL
RabbitMQ
UserService
OrderService
NotificationService
```

Run the containers in detached mode:

```bash
docker compose up --build -d
```

Check running containers:

```bash
docker compose ps
```

Stop the application:

```bash
docker compose down
```

To also delete the PostgreSQL Docker volume:

```bash
docker compose down -v
```

---

# Service URLs

After starting Docker Compose:

### UserService

```text
http://localhost:5001
```

### OrderService

```text
http://localhost:5002
```

### NotificationService

```text
http://localhost:5003
```

---

# Swagger API Documentation

ASP.NET Core Swagger support is enabled for the API services in development mode.

Example Swagger URLs when running locally:

```text
http://localhost:5001/swagger
```

and

```text
http://localhost:5002/swagger
```

Swagger can be used to explore and test the APIs.

For authenticated endpoints, generate a JWT token and use:

```text
Authorization: Bearer <your-token>
```

---

# PostgreSQL

Docker Compose runs PostgreSQL using:

```text
postgres:16
```

Default Docker Compose configuration:

```text
Database: microservicesdb
Username: postgres
Password: postgres
Port: 5432
```

The application connects to PostgreSQL from inside Docker using:

```text
Host=postgres
Port=5432
Database=microservicesdb
Username=postgres
Password=postgres
```

> The default credentials are intended only for local development. Production environments should use secrets or environment-specific configuration.

---

# Entity Framework Core

Both `UserService` and `OrderService` use Entity Framework Core with PostgreSQL.

Database migrations are automatically applied when the services start.

Conceptually:

```csharp
db.Database.Migrate();
```

This ensures pending migrations are applied to the configured PostgreSQL database during startup.

---

# RabbitMQ

RabbitMQ provides asynchronous communication between microservices.

Docker Compose exposes:

```text
AMQP:
localhost:5672

Management UI:
http://localhost:15672
```

The basic messaging flow is:

```text
OrderService
    │
    │ Publish OrderCreatedEvent
    ▼
RabbitMQ
    │
    │ Consume
    ▼
NotificationService
```

This approach allows services to communicate without requiring synchronous HTTP calls for every action.

---

# RabbitMQ Publisher

`OrderService` registers a RabbitMQ publisher as an application service.

```text
OrderService
      │
      ▼
RabbitMqPublisher
      │
      ▼
RabbitMQ
```

This publisher is responsible for sending events generated by order operations.

---

# RabbitMQ Consumer

`NotificationService` registers an `OrderCreatedConsumer` as a hosted service.

```text
RabbitMQ
    │
    ▼
OrderCreatedConsumer
    │
    ▼
NotificationService
```

Because the consumer runs as a background service, it can continuously process events while the API is running.

---

# Authentication Flow

JWT authentication is used by the protected services.

Typical flow:

```text
User
 │
 │ Login
 ▼
UserService
 │
 │ Generate JWT
 ▼
Client
 │
 │ Authorization: Bearer <token>
 ▼
Protected API
```

The protected service validates the JWT before allowing the request.

---

# Password Security

Passwords should never be stored as plain text.

`UserService` includes BCrypt support for secure password hashing.

Typical flow:

```text
Password
   │
   ▼
BCrypt Hash
   │
   ▼
Database
```

Authentication compares the submitted password with the stored hash rather than storing or retrieving the original password.

---

# Run Services Without Docker

Restore project dependencies:

```bash
dotnet restore
```

Build the complete solution:

```bash
dotnet build
```

Run `UserService`:

```bash
dotnet run --project UserService
```

Run `OrderService`:

```bash
dotnet run --project OrderService
```

Run `NotificationService`:

```bash
dotnet run --project NotificationService
```

When running the services outside Docker, make sure PostgreSQL and RabbitMQ are running and that the application configuration points to the correct hosts and ports.

---

# Build the Solution

```bash
dotnet build cloud-native-microservices.sln
```

A successful build confirms that all projects and shared dependencies compile correctly.

---

# Docker Commands

Build all services:

```bash
docker compose build
```

Start all containers:

```bash
docker compose up
```

Start and rebuild:

```bash
docker compose up --build
```

View logs:

```bash
docker compose logs
```

View logs for a specific service:

```bash
docker compose logs order-service
```

Follow logs:

```bash
docker compose logs -f
```

Stop containers:

```bash
docker compose down
```

---

# Development Workflow

A typical development workflow is:

```text
1. Create or update an API endpoint
        ↓
2. Update models / database schema
        ↓
3. Create EF Core migration if required
        ↓
4. Build and test locally
        ↓
5. Build Docker images
        ↓
6. Start services using Docker Compose
        ↓
7. Test REST APIs through Swagger
        ↓
8. Validate RabbitMQ event flow
```

---

# Event-Driven Workflow Example

An example order-processing workflow:

```text
Client
   │
   │ Create Order
   ▼
OrderService
   │
   ├── Save order to PostgreSQL
   │
   └── Publish OrderCreatedEvent
                    │
                    ▼
                RabbitMQ
                    │
                    ▼
           NotificationService
                    │
                    ▼
            Process notification
```

This architecture keeps order processing and notification processing loosely coupled.

---

# Current Features

- ASP.NET Core microservices
- User management
- JWT authentication
- BCrypt password hashing
- Order management
- PostgreSQL persistence
- Entity Framework Core
- Automatic database migrations
- RabbitMQ event publishing
- RabbitMQ background consumer
- Shared event contracts
- Swagger/OpenAPI
- Docker containers
- Docker Compose orchestration

---

# Future Improvements

This repository is a solid learning implementation of a microservices architecture, but several additions would make it closer to a production cloud-native system.

Possible next steps:

- Kubernetes deployment manifests
- Helm charts
- API Gateway
- Service discovery
- Health checks
- Distributed tracing
- OpenTelemetry
- Prometheus metrics
- Grafana dashboards
- Centralized logging
- Retry policies
- Dead-letter queues
- RabbitMQ message durability
- Outbox pattern
- Circuit breakers
- API versioning
- Refresh tokens
- Role-based authorization
- Secrets management
- Separate databases per microservice
- CI/CD using GitHub Actions
- Automated unit tests
- Integration tests
- Kubernetes readiness probes
- Kubernetes liveness probes
- Horizontal Pod Autoscaling
- Infrastructure as Code

---

# Production Considerations

The current Docker Compose configuration is intended for local development.

For production environments:

- Do not hardcode database credentials.
- Store secrets using a secret-management solution.
- Use separate databases or schemas where appropriate for service isolation.
- Enable TLS.
- Configure RabbitMQ durable exchanges and queues.
- Add retry and dead-letter handling.
- Add health/readiness endpoints.
- Add centralized observability.
- Add automated CI/CD pipelines.
- Deploy services through an orchestration platform such as Kubernetes.

---

# Learning Objectives

This project demonstrates practical experience with:

- Microservices architecture
- ASP.NET Core Web APIs
- REST API development
- JWT authentication
- Password security
- Entity Framework Core
- PostgreSQL
- Database migrations
- Event-driven architecture
- RabbitMQ
- Producer/consumer messaging
- Background services
- Docker
- Docker Compose
- Service isolation
- Shared event contracts
- Cloud-native application concepts

---

# Repository

```text
https://github.com/siri-chandanak/cloud-native-microservices
```

---

## License

This project is intended for learning, experimentation, and portfolio demonstration.

---

Built with **C# · .NET 9 · ASP.NET Core · PostgreSQL · RabbitMQ · Docker**
