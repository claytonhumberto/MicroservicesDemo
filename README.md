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
- **Phase 2** ✅ — Orders Service + synchronous HTTP communication (Catalog → Orders)
- **Phase 3** ✅ — Payments Service + Fake Provider + resilience (retry, circuit breaker, timeout)
- **Phase 4** ✅ — RabbitMQ events + MassTransit + Notifications Worker
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

---

## What Was Built — Phase 2

### The core concept: synchronous service-to-service communication

Phase 2 introduces the Orders Service and demonstrates the most common pattern for direct service communication: **synchronous HTTP calls**. When a client places an order, the Orders Service needs to know the product's name and current price before it can create the order. It gets this information by calling the Catalog Service over HTTP in real time.

This is the simplest form of inter-service communication — and also the most fragile. If Catalog is down, Orders cannot create new orders. Phase 3 will introduce resilience patterns (retries, circuit breaker, timeouts) to make this less brittle.

### The product snapshot problem

A subtle but important design decision: what happens to an order when a product's price changes later?

The Orders Service stores a **snapshot** of the product name and price at the time the order was placed. Even if the Catalog Service later updates the price, the existing order retains the original value. This is captured in `OrderItem`:

```csharp
public class OrderItem
{
    public string ProductName { get; private set; } // snapshot — captured at order time
    public decimal UnitPrice { get; private set; }  // snapshot — not linked to Catalog
    public decimal Subtotal => UnitPrice * Quantity; // computed, not stored in the DB
    ...
}
```

This is the correct approach for e-commerce systems. An order is a historical record, not a live view of the catalog.

### Service boundary enforcement

The Orders Service does **not** import any class from the Catalog Service. There is no shared `Product` class, no shared database, no direct table join. The only coupling is the HTTP call at order creation time. After that, the data is Orders' own.

`ICatalogClient` (defined in the Application layer) and `CatalogHttpClient` (implemented in Infrastructure) follow the same Dependency Inversion principle used by `IProductRepository` in Phase 1:

```csharp
// Application layer — knows only the interface
public interface ICatalogClient
{
    Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
}

// Infrastructure layer — knows HTTP
public class CatalogHttpClient(HttpClient httpClient) : ICatalogClient
{
    public async Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/api/products/{productId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken);
    }
}
```

### Resilience built in from day one

The HTTP client is registered with `AddStandardResilienceHandler`, which adds retry, circuit breaker, and timeout policies using `Microsoft.Extensions.Http.Resilience`:

```csharp
services.AddHttpClient<ICatalogClient, CatalogHttpClient>(client =>
{
    client.BaseAddress = new Uri(catalogUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromMilliseconds(500);
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
});
```

Phase 3 will tune these policies and introduce the Fake Payment Provider's configurable failures to show these patterns in action.

### Order lifecycle

Orders have four statuses and the transitions are enforced in the domain:

```
Pending → Confirmed → Paid
Pending → Cancelled
Confirmed → Cancelled
```

Each transition is a method on the `Order` aggregate root (`Confirm()`, `Cancel()`, `MarkAsPaid()`). If you try to confirm an order that has no items, or cancel an order that is already paid, the domain throws `InvalidOperationException` — the controller maps this to HTTP 422.

### Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/orders` | Paginated order list |
| `GET` | `/api/orders/{id}` | Order by ID (includes items) |
| `POST` | `/api/orders` | Create an order (calls Catalog for each product) |
| `POST` | `/api/orders/{id}/confirm` | Transition Pending → Confirmed |
| `POST` | `/api/orders/{id}/cancel` | Transition to Cancelled |
| `GET` | `/health` | Health check |

### Running Phase 2 locally

Start the infrastructure and both services:

```bash
cd infrastructure
docker compose --profile infra up -d
```

Then run each service in a separate terminal:

```bash
dotnet run --project src/Services/Catalog/Catalog.Api
dotnet run --project src/Services/Orders/Orders.Api
```

Or start everything with Docker:

```bash
cd infrastructure
docker compose --profile full up --build
```

Orders API docs (Scalar): `http://localhost:5102/scalar/v1`

---

## What Was Built — Phase 3

### The resilience problem

Synchronous HTTP communication (Phase 2) has a hidden fragility: if the downstream service is slow or unavailable, every caller blocks, threads accumulate, and the failure cascades through the entire system. A single slow third-party payment provider can bring down the service that calls it.

Phase 3 introduces the Payments Service and makes resilience explicit — not just in code, but as a **demo you can trigger on purpose** using the Fake Payment Provider.

### The Fake Payment Provider — a controlled chaos engine

The FakeProvider (port 5104) accepts a `?behavior=` query parameter that changes its response:

| Behavior | What happens |
|---|---|
| `success` (default) | Returns 200 with a transaction ID after ~200 ms |
| `fail` | Returns 422 — payment declined by provider |
| `timeout` | Waits 31 seconds, then returns 504 — longer than any retry window |
| `slow` | Waits 3 seconds — triggers the per-attempt timeout and retry |

In the Payments API, you control this via the request body's `ProviderBehavior` field. This lets you demonstrate every resilience scenario without touching infrastructure.

### Resilience policies — layered and named

The Payments Service registers the HTTP client with `AddStandardResilienceHandler` and configures each policy explicitly:

```csharp
.AddStandardResilienceHandler(options =>
{
    // Total budget for the entire operation including all retries: 15 s.
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);

    // Per-attempt timeout: give up on a single try after 5 s.
    // With "slow" behavior (3 s delay), this allows the first attempt to complete.
    // With "timeout" behavior (31 s delay), this fires and triggers a retry.
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);

    // Retry: up to 3 more attempts, with exponential back-off + jitter.
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromMilliseconds(500);
    options.Retry.UseJitter = true;

    // Circuit breaker: if 50%+ of calls fail in a 30 s window (min 5 calls),
    // stop calling the provider for 10 s. Callers get an immediate failure
    // instead of waiting for each attempt to time out.
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.MinimumThroughput = 5;
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);
});
```

### Failure recording — no re-throw

A key design decision in `PaymentService`: when the provider is unreachable or the circuit breaker opens, the exception is caught and the payment is recorded as `Failed` in the database — it is **not** re-thrown to the controller. This means:

1. The API always returns a response (201 Created) with the payment record.
2. The `Status` field in the response tells the client what happened.
3. The payment ID is preserved, enabling retry workflows in future phases.

```csharp
try
{
    result = await providerClient.ChargeAsync(chargeRequest, cancellationToken);
}
catch (Exception ex)
{
    payment.Fail($"Provider communication error: {ex.Message}");
    await paymentRepository.UpdateAsync(payment, cancellationToken);
    return MapToDto(payment); // returns 201 with Status = Failed
}
```

### Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/payments` | Paginated payment list |
| `GET` | `/api/payments/{id}` | Payment by ID |
| `GET` | `/api/payments/order/{orderId}` | All payments for a given order |
| `POST` | `/api/payments` | Process a payment (calls FakeProvider) |
| `GET` | `/health` | Health check |

### Demonstrating the circuit breaker

Start everything and run 5 consecutive `POST /api/payments` requests with `"providerBehavior": "timeout"`. After 5 failures, the circuit opens and subsequent calls return immediately with `Status: Failed` and a message like `"circuit breaker is open"` — without waiting for the timeout.

Payments API docs (Scalar): `http://localhost:5103/scalar/v1`

---

## What Was Built — Phase 4

### The problem with synchronous communication everywhere

In Phase 2 and 3, all communication is synchronous: the caller waits for the callee to respond. This works well for reads and for operations where the caller needs the result immediately. But for notifications — "send the customer an email after their payment is approved" — the caller does not need to wait. If the notification system is slow or down, the payment should still succeed.

This is the fundamental motivation for asynchronous messaging.

### Event-driven architecture with RabbitMQ and MassTransit

Phase 4 introduces two integration events published to RabbitMQ:

| Event | Publisher | Consumer |
|---|---|---|
| `OrderConfirmed` | Orders Service | Notifications Worker |
| `PaymentApproved` | Payments Service | Notifications Worker |

Events are defined in `BuildingBlocks.Contracts` — the one shared library that crosses service boundaries. Each event is a simple `record` with no behavior, only data:

```csharp
public record PaymentApproved(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string TransactionId,
    string CustomerEmail,
    DateTime ApprovedAt);
```

### MassTransit as the messaging abstraction

MassTransit sits between the application code and RabbitMQ. Publishers call `IPublishEndpoint.Publish<T>()` — they never touch AMQP or queue names directly. MassTransit handles exchange binding, serialization, retry on publish failure, and consumer queue naming automatically.

`BuildingBlocks.Messaging` provides a single registration method used by all services:

```csharp
// For services that only publish (Orders, Payments)
services.AddMassTransitPublisher(configuration);

// For services that also consume (Notifications.Worker)
services.AddMassTransitWithConsumers(configuration, x =>
{
    x.AddConsumer<PaymentApprovedConsumer>();
    x.AddConsumer<OrderConfirmedConsumer>();
});
```

`cfg.ConfigureEndpoints(ctx)` in the consumer registration tells MassTransit to auto-create queues named after the consumer class. This is the convention-over-configuration approach — no hand-coded queue names.

### Publishing inside the domain flow

`PaymentService.ProcessAsync` publishes `PaymentApproved` immediately after the payment is saved as approved:

```csharp
payment.Approve(result.TransactionId!);
await paymentRepository.UpdateAsync(payment, cancellationToken);

await publishEndpoint.Publish(new PaymentApproved(...), cancellationToken);
```

`OrderService.ConfirmAsync` publishes `OrderConfirmed` after the order is saved:

```csharp
order.Confirm();
await orderRepository.UpdateAsync(order, cancellationToken);

await publishEndpoint.Publish(new OrderConfirmed(...), cancellationToken);
```

Note the ordering: persist first, then publish. This is correct but not fully safe — if the process crashes between the two lines, the event is lost. **Phase 5 will address this with the Outbox Pattern**, which makes event publishing atomic with the database transaction.

### The Notifications Worker

The worker has no HTTP endpoints. It is a `Microsoft.Extensions.Hosting` worker that connects to RabbitMQ on startup and receives messages via MassTransit consumers:

```csharp
public class PaymentApprovedConsumer(ILogger<PaymentApprovedConsumer> logger) : IConsumer<PaymentApproved>
{
    public Task Consume(ConsumeContext<PaymentApproved> context)
    {
        var evt = context.Message;
        logger.LogInformation("[NOTIFICATION] Payment approved — OrderId: {OrderId} | Customer: {CustomerEmail} ...", ...);
        return Task.CompletedTask;
    }
}
```

In production this would call SendGrid, Firebase Cloud Messaging, or a push notification service. The consumer is kept simple here to isolate the messaging concept from the notification delivery mechanism — each article covers one thing at a time.

### Observing the events

With the full stack running (`docker compose --profile full up --build`):

1. Create an order: `POST /api/orders`
2. Confirm the order: `POST /api/orders/{id}/confirm` → Notifications Worker logs `[NOTIFICATION] Order confirmed`
3. Process a payment: `POST /api/payments` with `"providerBehavior": "success"` → Notifications Worker logs `[NOTIFICATION] Payment approved`
4. Open the RabbitMQ management UI at `http://localhost:15672` (guest/guest) to see the exchanges and queues MassTransit created automatically.
