# Meridian API

A financial operations platform built with **Clean Architecture**, **CQRS/MediatR**, **Domain Events**, and the **Transactional Outbox Pattern** on ASP.NET Core 10 + PostgreSQL.

Built as a demonstration of tech-lead-grade architectural patterns in C# .NET.

## Architecture

```
src/
  Meridian.Domain/          Domain entities, value objects, domain events, repository interfaces
  Meridian.Application/     CQRS commands/queries (MediatR), validators (FluentValidation), pipeline behaviours
  Meridian.Infrastructure/  EF Core + Npgsql, repositories, outbox processor, account number generator
  Meridian.API/             ASP.NET Core controllers, exception middleware, DI wiring
tests/
  Meridian.UnitTests/       Domain logic + application handler tests (Moq, FluentAssertions) — 32 tests
  Meridian.IntegrationTests/ Full HTTP tests against a real Postgres container (Testcontainers) — 9 tests
```

**Dependency rule**: Domain → none. Application → Domain. Infrastructure → Application. API → Application + Infrastructure. Tests reference their layer.

## Domain Design

Two aggregate roots with full lifecycle state machines:

**Account**: `Open → Active ↔ Frozen → Closed`
- `Credit` / `Debit` enforce business rules (active status, non-negative balance)
- Closure blocked on non-zero balance
- Each operation raises a typed domain event (`AccountOpenedEvent`, `AccountCreditedEvent`, …)

**Transaction**: `Pending → Completed | Failed`, `Completed → Reversed`
- Initiated with amount/currency/reference validation
- State transitions are irreversible (attempted transitions throw `DomainException`)
- Domain events: `TransactionInitiatedEvent`, `TransactionCompletedEvent`, `TransactionReversedEvent`

## CQRS with MediatR

**Commands** (write side):
- `OpenAccountCommand` — opens a new account, returns account number
- `TransferFundsCommand` — atomic debit/credit with idempotency on `reference`
- `ReverseTransactionCommand` — reverses a completed transfer, restores both balances
- `FreezeAccountCommand` / `UnfreezeAccountCommand`

**Queries** (read side):
- `GetAccountQuery` — account with balance and status
- `GetTransactionHistoryQuery` — paginated transaction history for an account

**Pipeline behaviours** (cross-cutting):
- `LoggingBehavior` — structured request/response logging via Serilog
- `ValidationBehavior` — FluentValidation before every handler; throws `ValidationException` on failure (mapped to 422 by middleware)

## Transactional Outbox Pattern

`MeridianDbContext.SaveChangesAsync` intercepts domain events raised by aggregates, serialises them as `OutboxMessage` rows, and clears the aggregate's event list — all in the same database transaction as the business write. `OutboxProcessor` (a `BackgroundService`) polls every 5 seconds, deserialises each message by its assembly-qualified type name, and publishes it via MediatR. This guarantees at-least-once delivery and decouples event subscribers from the command path.

## Idempotency

`TransferFundsCommand` checks for an existing `Transaction` with the same `reference` before executing. A completed transfer returns the original result immediately with no second debit. A `transactions.reference` unique index at the DB level prevents races between concurrent requests with the same reference.

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Clean Architecture layers | Enforces the dependency rule; domain has zero framework dependencies |
| CQRS via MediatR | Separates reads and writes; pipeline behaviours compose cleanly |
| Outbox over direct publish | Domain events survive a process crash; no distributed transaction needed |
| Repository + UoW | Keeps application handlers testable with in-memory fakes (no EF Core in unit tests) |
| FluentValidation + pipeline | Validation lives with the command, not in controllers |
| Testcontainers for integration tests | Tests run against real Postgres; no sqlite/in-memory surprises |

## Running Locally

```bash
# Start Postgres
docker compose up postgres -d --wait

# Run migrations + start API
dotnet run --project src/Meridian.API

# API is at http://localhost:5000
# OpenAPI at http://localhost:5000/openapi/v1.json
```

## Tests

```bash
# Unit tests (32, no containers required)
dotnet test tests/Meridian.UnitTests

# Integration tests (9, requires Docker — Testcontainers starts Postgres automatically)
dotnet test tests/Meridian.IntegrationTests
```

## Real Bugs Found During Development

1. **`RequestHandlerDelegate<T>` signature change in MediatR 12** — the delegate no longer accepts `CancellationToken` as an argument (the token is captured by the handler's own scope). Calling `next(ct)` fails at compile time; correct usage is `next()`. Found and fixed before the first test run.

2. **EF Core discovering `DomainEvents` as a navigation property** — `AggregateRoot.DomainEvents` returns `IReadOnlyList<DomainEvent>` and EF Core's model builder interpreted this as a collection navigation to the abstract `DomainEvent` type, throwing at migration time. Fixed by `[NotMapped]` on the property and `modelBuilder.Ignore<DomainEvent>()` in `OnModelCreating`.

3. **Design-time migration tooling can't resolve `IPublisher`** — the `MeridianDbContext` constructor injects `IPublisher` for domain event dispatch, which the EF Core design-time host can't provide. Fixed by adding a `IDesignTimeDbContextFactory<MeridianDbContext>` implementation with an inline no-op publisher.

## API Endpoints

```
POST   /api/accounts                          Open an account
GET    /api/accounts/{id}                     Get account with balance
POST   /api/accounts/{id}/freeze              Freeze account
POST   /api/accounts/{id}/unfreeze            Unfreeze account
GET    /api/accounts/{id}/transactions        Transaction history (paginated)

POST   /api/transactions/transfer             Transfer funds (idempotent on reference)
POST   /api/transactions/{id}/reverse         Reverse a completed transaction
```
