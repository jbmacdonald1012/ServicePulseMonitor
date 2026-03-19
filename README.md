# ServicePulseMonitor

A REST API for monitoring the health and status of microservices. Services register themselves, submit health check results, and consumers query health summaries, historical checks, and service metadata.

## Tech Stack

- **Runtime:** .NET 8 / ASP.NET Core
- **Language:** C# 12
- **Database:** PostgreSQL 16 (via Npgsql + EF Core)
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

On first run in **Development** mode, the database is seeded with three sample services, two users, sample health checks, and a service dependency graph.

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

Pagination parameters: `pageNumber` (default 1), `pageSize` (1–100, default 10).

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

### System

| Route | Description |
|-------|-------------|
| `/health` | ASP.NET Core health endpoint |
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

## Project Structure

```
ServicePulseMonitor/
├── Controllers/                 # ServicesController, HealthChecksController
├── Data/
│   ├── Configurations/          # EF Core IEntityTypeConfiguration<T> classes
│   ├── DTOs/                    # Request/response records (CreateServiceDto, etc.)
│   ├── Migrations/              # EF Core migration files
│   ├── Models/                  # Domain entities (Service, HealthCheck, Alert, etc.)
│   └── Seed/                    # DataSeeder for development
├── Features/
│   ├── Services/                # IRegistrationService + RegistrationService
│   └── HealthChecks/            # IHealthCheckService + HealthCheckService
├── Common/                      # PagedResult<T>
└── Program.cs

ServicePulseMonitor.Tests/
├── Controllers/                 # Controller tests (mocked services)
├── Features/                    # Unit tests for service layer
└── Integration/                 # End-to-end tests via WebApplicationFactory
```

## Running Tests

```bash
dotnet test
```

The test suite includes unit tests, controller tests (with mocked dependencies), and integration tests that run against an in-memory database.

## Data Model Overview

| Entity | Key Fields |
|--------|-----------|
| `Service` | `ServiceId`, `ServiceName` (unique), `BaseUrl`, `LastSeenAt` |
| `HealthCheck` | `HealthCheckId`, `ServiceId`, `Status`, `ResponseTimeMs`, `CheckedAt`, `Details` (JSONB) |
| `ServiceDependency` | `ServiceId` → `DependsOnServiceId` |
| `AlertRule` | `RuleId`, `ServiceId`, `RuleType`, `Threshold` |
| `Alert` | `AlertId`, `ServiceId`, `AlertType`, `IsAcknowledged`, `IsResolved` |
| `User` | `UserGuid`, `Username` (unique), `PasswordHash`, `AccessLevel` |

Health check `Details` is stored as JSONB, so any key/value metadata can be attached to a check.
