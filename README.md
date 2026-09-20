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
