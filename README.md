# MicroservicesDemo

**Author:** Clayton Humberto  
**Repository:** https://github.com/claytonhumberto/MicroservicesDemo

A hands-on .NET 10 microservices laboratory built to support a series of articles on distributed systems. Each phase of the project maps directly to a technical concept — the code you see here is the running example behind the articles.

---

## Architecture

```
                 ┌─────────────────┐
                 │     Client      │
                 └────────┬────────┘
                          │ :5100
                          ▼
                 ┌─────────────────┐
                 │   API Gateway   │  YARP
                 └────────┬────────┘
                          │
             ┌────────────┼────────────┐
             ▼            ▼            ▼
       ┌──────────┐ ┌──────────┐ ┌──────────┐
       │ Catalog  │ │  Orders  │ │ Payments │
       │  :5101   │ │  :5102   │ │  :5103   │
       └────┬─────┘ └────┬─────┘ └────┬─────┘
            │             │             │
            ▼             ▼             ▼
       CatalogDb      OrdersDb     PaymentsDb
      (Postgres)     (Postgres)   (Postgres)
                          │
                          │ Events
                          ▼
                  ┌───────────────┐
                  │   RabbitMQ    │  :15672 (UI)
                  └───────┬───────┘
                          │
                          ▼
                  ┌───────────────┐
                  │ Notifications │
                  │    Worker     │
                  └───────────────┘

                  ┌───────────────┐
                  │ FakePayment   │  Configurable: success / fail / timeout / slow
                  │   Provider    │  :5104
                  └───────────────┘
```

---

## Tech Stack

- **.NET 10** / ASP.NET Core 10
- **YARP** — API Gateway (reverse proxy by Microsoft)
- **Entity Framework Core 10** + **PostgreSQL** (Npgsql)
- **RabbitMQ** — async messaging
- **MassTransit** — messaging abstraction (Phase 4)
- **Docker** + **Docker Compose** — local environment
- **Scalar** — API documentation

---

## Project Structure

```
MicroservicesDemo/
├── src/
│   ├── Gateway/
│   │   └── Gateway.Api/              # YARP reverse proxy
│   ├── Services/
│   │   ├── Catalog/                  # Products & Categories — Phase 1 ✅
│   │   │   ├── Catalog.Api/
│   │   │   ├── Catalog.Application/
│   │   │   ├── Catalog.Domain/
│   │   │   └── Catalog.Infrastructure/
│   │   ├── Orders/                   # Order management — Phase 2
│   │   ├── Payments/                 # Payment processing — Phase 3
│   │   │   └── Payments.FakeProvider/ # Configurable fake payment provider
│   │   └── Notifications/            # Event-driven notifications — Phase 4
│   └── BuildingBlocks/
│       ├── BuildingBlocks.Contracts/ # Shared event contracts (DTOs only)
│       ├── BuildingBlocks.Messaging/ # IEventBus abstraction
│       └── BuildingBlocks.Observability/ # Logging, correlation, health
├── infrastructure/
│   └── docker-compose.yml
└── README.md
```

---

## Running Locally

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Start infrastructure only (databases + RabbitMQ)

```bash
cd infrastructure
docker compose --profile infra up -d
```

### Run Catalog Service locally (after infra is up)

```bash
dotnet run --project src/Services/Catalog/Catalog.Api
```

API available at: `http://localhost:5101`  
API Docs (Scalar): `http://localhost:5101/scalar/v1`

### Start everything with Docker

```bash
cd infrastructure
docker compose --profile full up --build
```

| Service | URL |
|---|---|
| Gateway | http://localhost:5100 |
| Catalog API | http://localhost:5101 |
| Orders API | http://localhost:5102 |
| Payments API | http://localhost:5103 |
| Fake Payment Provider | http://localhost:5104 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

---

## Article Series

| # | Topic | Status |
|---|---|---|
| 14 | Microservices in .NET: When Should You Use Them? | Architecture presented here |
| 15 | Bounded Contexts: Where Does a Microservice Start and End? | |
| 16 | Database per Service: Why Sharing a Database Changes Everything | |
| 17 | REST vs Messaging: Synchronous vs Asynchronous Communication | |
| 18 | RabbitMQ in .NET: Building Asynchronous Communication | |
| 19 | Retries, Timeouts and Circuit Breakers in .NET | |
| 20 | Idempotency: What Happens When a Message Arrives Twice? | |
| 21 | Eventual Consistency: When Data Doesn't Update at the Same Time | |
| 22 | Observability: Finding a Request Across Multiple Services | |
| 23 | Authentication Between Microservices | |
| 24 | The Outbox Pattern in .NET | |

---

## Implementation Phases

- **Phase 1** ✅ — Skeleton, Gateway (YARP), Catalog Service (full CRUD + PostgreSQL)
- **Phase 2** — Orders Service + synchronous HTTP communication (Catalog → Orders)
- **Phase 3** — Payments Service + Fake Provider + resilience (Polly: retry, circuit breaker)
- **Phase 4** — RabbitMQ events + Notifications Worker + Outbox Pattern
- **Phase 5** — Observability (OpenTelemetry, correlation ID, structured logging)
- **Phase 6** — Authentication between services (JWT)

---

## What Was Built — Phase 1

### Why this structure?

One of the most common mistakes when "moving to microservices" is keeping a shared database. Services that share a database are not truly independent — a schema change in one breaks another, deployments become coupled, and teams step on each other's work. This project enforces **Database per Service** from day one: three separate PostgreSQL instances, one per business domain.

### Solution layout

The solution contains **19 projects** organized into four areas:

**Gateway** — a single YARP reverse proxy that receives all client requests and routes them to the correct service based on the URL path. The client never calls a service directly. This is the only entry point.

**Services** — each service is a fully independent vertical slice with its own four layers following Domain-Driven Design:
- `Domain` — entities, business rules, repository interfaces. No framework dependencies.
- `Application` — use cases, DTOs, service orchestration. Knows only the Domain.
- `Infrastructure` — EF Core, PostgreSQL, repository implementations. The only layer that touches the database.
- `Api` — ASP.NET Core controllers, DI wiring, HTTP concerns.

**BuildingBlocks** — three small libraries with a strict rule: they may only contain shared infrastructure contracts, never domain logic. Services do not share domain models with each other.

**Payments.FakeProvider** — a minimal ASP.NET Core app that simulates an external payment gateway. Its behavior is configurable via query string: `?behavior=success`, `fail`, `timeout`, or `slow`. This will be the foundation for the resilience articles (retries, circuit breaker).

### Catalog Service — what is implemented

The Catalog Service is the only fully implemented service in Phase 1. It demonstrates the complete DDD layered structure that every other service will follow.

**Domain layer**

`Product` is the aggregate root. It owns its own state — all properties use `private set`, and state changes go through explicit methods (`Create`, `Update`, `Deactivate`, `ReduceStock`). Business rules live here, not in the controller or service:

```csharp
public void ReduceStock(int quantity)
{
    if (!HasStock(quantity))
        throw new InvalidOperationException(
            $"Insufficient stock for '{Name}'. Available: {StockQuantity}, Requested: {quantity}.");

    StockQuantity -= quantity;
    UpdatedAt = DateTime.UtcNow;
}
```

`IProductRepository` is defined in the Domain layer. The Application layer depends on this interface, never on EF Core or PostgreSQL.

**Infrastructure layer**

`ProductRepository` implements `IProductRepository` using EF Core and PostgreSQL. Pagination is done at the database level with `Skip/Take` — no row is loaded into memory before filtering. The repository is the only class in the entire solution that knows about `CatalogDbContext`.

**Application layer**

`ProductService` orchestrates use cases by calling the repository through its interface. It maps domain entities to DTOs for the API layer. No SQL, no HTTP, no EF Core here.

**API layer**

`ProductsController` handles HTTP concerns only: status codes, route parameters, response mapping. All business logic stays in the service. On startup, the database migration runs automatically and 20 products across 4 categories are seeded.

**Endpoints**

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/products` | Paginated product list (filter by category) |
| `GET` | `/api/products/{id}` | Single product by ID |
| `GET` | `/api/products/categories` | Distinct category list |
| `POST` | `/api/products` | Create a product |
| `PUT` | `/api/products/{id}` | Update a product |
| `DELETE` | `/api/products/{id}` | Soft-delete (sets IsActive = false) |
| `GET` | `/health` | Health check |

### YARP Gateway

The gateway reads its routing table from `appsettings.json` — no code changes are needed to add or change a route. In Phase 1 it handles three routes:

```
/api/products/** → catalog-api:8080
/api/orders/**   → orders-api:8080
/api/payments/** → payments-api:8080
```

Later phases will add authentication at the gateway edge (rate limiting, JWT validation) without touching the downstream services.

### Docker Compose profiles

The compose file uses profiles so you only start what you need:

```bash
# Just the infrastructure (3 PostgreSQL instances + RabbitMQ)
docker compose --profile infra up -d

# Infrastructure + Catalog (useful while building Phase 2)
docker compose --profile catalog up -d

# Everything
docker compose --profile full up --build
```

### Orders, Payments and Notifications — stubs

These services exist as buildable, deployable projects with a `/health` endpoint. They return a JSON response explaining which phase will implement them. This is intentional: the goal of Phase 1 is to prove the skeleton compiles, the Docker network works, and the gateway routes correctly — before writing any business logic in the other services.
