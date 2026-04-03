# HrSaas — Multi-Tenant SaaS HR Management System

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com)
[![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue)](docs/architecture.md)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

> A **production-grade, multi-tenant SaaS HR management platform** built with .NET 9, demonstrating evolutionary architecture from Modular Monolith to Microservices.

---

## 🏗️ Architecture at a Glance

```
┌──────────────────────────────────────────────────────────────┐
│                    HrSaas.Api (Host)                          │
├──────────┬──────────┬──────────┬──────────┬──────────────────┤
│ Identity │  Tenant  │ Employee │  Leave   │     Billing      │
│  Module  │  Module  │  Module  │  Module  │     Module       │
├──────────┴──────────┴──────────┴──────────┴──────────────────┤
│         BuildingBlocks: SharedKernel | EventBus | TenantSdk  │
└──────────────────────────────────────────────────────────────┘
         PostgreSQL        RabbitMQ        Redis
```

**Stack**: .NET 9 · C# 13 · EF Core 9 · MediatR 12 · MassTransit 8 · YARP · PostgreSQL · Docker

---

## 📁 Project Structure

```
multi-tenant-saas/
├── .github/
│   └── copilot-instructions.md     ← GitHub Copilot coding rules
├── docs/
│   ├── architecture.md             ← Full system design
│   ├── skills.md                   ← Developer onboarding guide
│   └── ADR/                        ← Architecture Decision Records
│       ├── ADR-001-modular-monolith.md
│       ├── ADR-002-multi-tenancy.md
│       ├── ADR-003-cqrs-mediatr.md
│       ├── ADR-004-event-driven.md
│       └── ADR-005-microservice-extraction.md
├── src/
│   ├── Api/HrSaas.Api/             ← Presentation layer (controllers, middleware)
│   ├── Modules/
│   │   ├── Identity/               ← Auth, JWT, users
│   │   ├── Tenant/                 ← Company management
│   │   ├── Employee/               ← HR employee CRUD (full sample)
│   │   ├── Leave/                  ← Leave workflow
│   │   └── Billing/                ← Subscriptions, invoicing
│   └── BuildingBlocks/
│       ├── SharedKernel/           ← BaseEntity, Result<T>, CQRS interfaces
│       ├── TenantSdk/              ← TenantMiddleware, ITenantService
│       └── EventBus/               ← IEventBus, MassTransit impl
├── scripts/db/init.sql             ← DB schema initialization
├── docker-compose.yml
└── README.md
```

---

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run with Docker Compose

```bash
# Clone the repository
git clone https://github.com/your-org/hrsaas.git
cd hrsaas

# Start all services (Postgres, RabbitMQ, Redis, Seq)
docker-compose up -d

# Run database migrations
dotnet ef database update \
  --project src/Modules/Employee/HrSaas.Modules.Employee \
  --startup-project src/Api/HrSaas.Api \
  --context EmployeeDbContext

# Start the API
dotnet run --project src/Api/HrSaas.Api
```

### Service URLs

| Service | URL | Purpose |
|---------|-----|---------|
| **HrSaas API** | http://localhost:8080 | Main application |
| **OpenAPI Docs** | http://localhost:8080/openapi/v1.json | API documentation |
| **RabbitMQ UI** | http://localhost:15672 | Message broker (guest/guest) |
| **Seq Logs** | http://localhost:8090 | Structured logging |

---

## 🔑 Key Concepts

### Multi-Tenancy

Every database table has a `tenant_id` column. EF Core Global Query Filters automatically scope all queries to the current tenant — no manual filtering needed in queries.

```csharp
// TenantMiddleware extracts from JWT → sets TenantContext
// EF Core filter applies: WHERE tenant_id = @currentTenantId AND is_deleted = false
```

### CQRS with MediatR

```csharp
// Command (mutates state)
var result = await mediator.Send(new CreateEmployeeCommand(tenantId, "Alice", "Engineering", "SWE", "alice@acme.com"));

// Query (reads only)
var result = await mediator.Send(new GetEmployeeByIdQuery(employeeId));
```

### Event-Driven (Domain → Integration Events)

```
Employee.Create() → raises EmployeeCreatedEvent (in-process)
    ↓ domain event handler
publishes EmployeeCreatedIntegrationEvent → RabbitMQ → Billing consumer
```

---

## 📡 API Endpoints

### Authentication
```
POST /api/v1/auth/register    Register new user
POST /api/v1/auth/login       Login and get JWT
```

### Employees (requires Auth + TenantId)
```
GET    /api/v1/employees              List all employees (paginated)
GET    /api/v1/employees/{id}         Get employee by ID
POST   /api/v1/employees              Create employee [Admin, Manager]
PUT    /api/v1/employees/{id}         Update employee [Admin, Manager]
DELETE /api/v1/employees/{id}         Delete employee [Admin]
```

### Leave
```
POST   /api/v1/leaves                 Apply for leave
GET    /api/v1/leaves                 List leave requests
PUT    /api/v1/leaves/{id}/approve    Approve [Manager, Admin]
PUT    /api/v1/leaves/{id}/reject     Reject [Manager, Admin]
```

---

## 🔐 JWT Token Structure

```json
{
  "sub": "user-guid",
  "email": "alice@acmecorp.com",
  "tenant_id": "tenant-guid",
  "role": "Admin",
  "exp": 1712016000
}
```

---

## 🧪 Running Tests

```bash
# All tests
dotnet test

# Domain unit tests
dotnet test tests/HrSaas.Domain.Tests

# Application unit tests
dotnet test tests/HrSaas.Application.Tests

# Integration tests (requires Docker)
dotnet test tests/HrSaas.Infrastructure.Tests
```

---

## 📖 Documentation

| Document | Description |
|----------|-------------|
| [Architecture Overview](docs/architecture.md) | Full system design, diagrams, data flow |
| [Skills Guide](docs/skills.md) | Developer onboarding, skill levels, patterns |
| [ADR-001: Modular Monolith](docs/ADR/ADR-001-modular-monolith.md) | Why modular monolith |
| [ADR-002: Multi-Tenancy](docs/ADR/ADR-002-multi-tenancy.md) | Shared DB strategy |
| [ADR-003: CQRS](docs/ADR/ADR-003-cqrs-mediatr.md) | CQRS with MediatR |
| [ADR-004: Events](docs/ADR/ADR-004-event-driven.md) | Event-driven architecture |
| [ADR-005: Microservices](docs/ADR/ADR-005-microservice-extraction.md) | Extraction strategy |

---

## 🗺️ Roadmap

### Phase 1 — Modular Monolith (current)
- [x] Multi-tenancy with EF Core query filters
- [x] JWT auth with tenant claim
- [x] CQRS with MediatR + FluentValidation
- [x] Employee module (full sample)
- [x] Leave workflow
- [ ] Billing module (complete)
- [ ] Integration tests

### Phase 2 — Service Extraction
- [ ] Extract Identity to standalone service
- [ ] Extract Billing to standalone service
- [ ] YARP gateway configuration
- [ ] OpenTelemetry distributed tracing
- [ ] Kubernetes deployment manifests

### Phase 3 — Scale
- [ ] Redis rate limiting per tenant
- [ ] Database-per-tenant for enterprise plans
- [ ] Horizontal scaling with k8s HPA

---

## 🤝 Contributing

1. Read [docs/skills.md](docs/skills.md) for architecture and coding conventions
2. Read [.github/copilot-instructions.md](.github/copilot-instructions.md) for AI assistant rules
3. Follow the ADRs for design decisions
4. All commands/queries must have validators
5. All aggregate roots must extend `BaseEntity`
6. All tests must validate tenant isolation
