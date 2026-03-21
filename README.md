# ServicePulseMonitor

A REST API for monitoring the health and status of microservices. Services register themselves, submit health check results, and consumers query health summaries, historical checks, and service metadata. A background collector automatically polls registered services and broadcasts real-time status changes to connected dashboard clients via SignalR.

## Tech Stack

- **Runtime:** .NET 8 / ASP.NET Core
- **Language:** C# 12
- **Database:** PostgreSQL 16 (via Npgsql + EF Core)
- **Real-time:** ASP.NET Core SignalR
- **Documentation:** Swagger / OpenAPI (Swashbuckle)
- **Testing:** NUnit, Moq 4.18.4, EF Core InMemory, WebApplicationFactory

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for the local PostgreSQL instance)

## Getting Started

### 1. Start the database

```bash
docker compose up -d
```

This starts PostgreSQL 16 on port **5433** with the credentials from `.env`.

### 2. Configure the connection string

The connection string is stored in user secrets (not committed). Initialize secrets and set the value:

```bash
cd ServicePulseMonitor
dotnet user-secrets set "ConnectionStrings:PostgreSQL" \
  "Host=localhost;Port=5433;Database=servicepulse_monitor;Username=servicepulse_admin;Password=Dev_Password_2026!"
```

### 3. Apply migrations

```bash
dotnet ef database update
```

### 4. Run the API

```bash
dotnet run
```

The API starts at `https://localhost:7xxx` / `http://localhost:5xxx`. Swagger UI is available at `/swagger`.

On first run in **Development** mode, the database is seeded with three sample services, two users, sample health checks, and a service dependency graph. Migrations are also applied automatically in Development.

## Background Health Collector

`HealthCollectorService` is a hosted background service that runs on a configurable interval. It:

1. Polls every registered service's `{baseUrl}/health` endpoint.
2. Records a `HealthCheck` entry (status, response time, HTTP details).
3. On any status change, creates an `Alert` record and resolves prior open alerts on recovery.
4. Broadcasts two SignalR events to all connected dashboard clients: `ServiceStatusChanged` and (on degradation) `AlertGenerated`.

Configure polling behavior in `appsettings.json`:

```json
"HealthCollector": {
  "IntervalSeconds": 30,
  "TimeoutSeconds": 10
}
```

## Real-time Dashboard (SignalR)

Connect to `/hubs/health` using the SignalR client. The server pushes the following messages — clients do not send messages to this hub.

| Event | Payload Fields | When fired |
|-------|---------------|------------|
| `ServiceRegistered` | `serviceId`, `serviceName`, `baseUrl`, `registeredAt` | A new service is registered via `POST /api/services` |
| `ServiceStatusChanged` | `serviceId`, `serviceName`, `status`, `responseTimeMs`, `timestamp` | Background collector detects a status change |
| `AlertGenerated` | `serviceId`, `serviceName`, `alertType`, `message`, `triggeredAt` | Background collector detects a degradation/unhealthy transition |

CORS is pre-configured to allow connections from `http://localhost:3000` (the dashboard origin).

## API Reference

### Services

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/services` | Register a new service |
| `GET` | `/api/services` | List all services (paginated) |
| `GET` | `/api/services/{id}` | Get a service by ID |
| `PUT` | `/api/services/{id}` | Update service metadata |
| `DELETE` | `/api/services/{id}` | Delete a service |
| `GET` | `/api/services/search?q={query}` | Search services by name |
| `GET` | `/api/services/{id}/health` | Get health summary for a service |

Pagination parameters: `pageNumber` (default 1), `pageSize` (1–100, default 20).

### Health Checks

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/services/{serviceId}/healthchecks` | Submit a health check result |
| `GET` | `/api/healthchecks/{id}` | Get a health check by ID |
| `GET` | `/api/services/{serviceId}/healthchecks` | Recent checks for a service (limit 1–100) |
| `GET` | `/api/services/{serviceId}/healthchecks/latest` | Latest check for a service |
| `GET` | `/api/healthchecks/status/{status}` | Filter checks by status (limit 1–200) |
| `GET` | `/api/healthchecks` | All health checks (paginated) |

Valid status values: `Healthy`, `Degraded`, `Unhealthy`.

### Dependencies

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/dependencies/report` | Record a detected dependency between two services (idempotent) |
| `GET` | `/api/dependencies` | Get the full dependency graph as a flat list of edges |

The `POST /api/dependencies/report` endpoint is intended to be called by a client-side `DependencyDetectionHandler`. It resolves the target service by matching `BaseUrl` prefix or service name.

### System

| Route | Description |
|-------|-------------|
| `/health` | ASP.NET Core health endpoint |
| `/hubs/health` | SignalR hub for real-time dashboard updates |
| `/swagger` | Swagger UI |

### Example Requests

See `ServicePulseMonitor/ServicePulseMonitor.http` for a full set of ready-to-run HTTP requests covering all endpoints.

**Register a service:**
```json
POST /api/services
{
  "serviceName": "payment-service",
  "baseUrl": "https://payments.internal",
  "description": "Handles all payment processing"
}
```

**Submit a health check:**
```json
POST /api/services/1/healthchecks
{
  "status": "Healthy",
  "responseTimeMs": 42,
  "details": { "db": "connected", "cache": "hit-rate-0.98" }
}
```

**Report a dependency:**
```json
POST /api/dependencies/report
{
  "serviceName": "order-service",
  "dependsOnUrl": "https://payments.internal",
  "dependsOnServiceName": "payment-service"
}
```

## Project Structure

```
ServicePulseMonitor/
├── Controllers/                 # ServicesController, HealthChecksController, DependenciesController
├── Data/
│   ├── Configurations/          # EF Core IEntityTypeConfiguration<T> classes
│   ├── DTOs/                    # Request/response records
│   ├── Migrations/              # EF Core migration files
│   ├── Models/                  # Domain entities (Service, HealthCheck, Alert, etc.)
│   └── Seed/                    # DataSeeder for development
├── Features/
│   ├── Services/                # IRegistrationService + RegistrationService
│   └── HealthChecks/            # IHealthCheckService + HealthCheckService
├── Hubs/                        # HealthHub (SignalR)
├── Options/                     # HealthCollectorOptions
├── Services/                    # HealthCollectorService (background worker)
├── Common/                      # PagedResult<T>
└── Program.cs

ServicePulseMonitor.Tests/
├── Controllers/                 # ServicesController, DependenciesController tests
├── Features/                    # Unit tests for service layer
├── Hubs/                        # HealthHub tests
├── Integration/                 # End-to-end tests via WebApplicationFactory
└── Services/                    # HealthCollectorService tests
```

## Running Tests

```bash
dotnet test
```

The test suite includes unit tests for services, controllers (with mocked dependencies), the SignalR hub, the background health collector, and integration tests that run against an in-memory database.

## Data Model Overview

| Entity | Key Fields |
|--------|-----------|
| `Service` | `ServiceId`, `ServiceName` (unique), `BaseUrl`, `CurrentStatus`, `LastSeenAt` |
| `HealthCheck` | `HealthCheckId`, `ServiceId`, `Status`, `ResponseTimeMs`, `CheckedAt`, `Details` (JSONB) |
| `ServiceDependency` | `ServiceId` → `DependsOnServiceId`, `DiscoveredAt` |
| `AlertRule` | `RuleId`, `ServiceId`, `RuleType`, `Threshold` |
| `Alert` | `AlertId`, `ServiceId`, `AlertType`, `Message`, `IsAcknowledged`, `IsResolved`, `ResolvedAt` |
| `User` | `UserGuid`, `Username` (unique), `PasswordHash`, `AccessLevel` |
| `NotificationConfig` | Per-service notification channel configuration |

`Service.CurrentStatus` tracks the last known status and is updated by the background collector on every status change. Health check `Details` and `Alert.Message` are stored as JSONB.
