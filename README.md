# E-Commerce Center — Backend API

> Production-grade e-commerce REST API built with .NET 10, Clean Architecture, and CQRS. Powers both an admin dashboard and a public storefront with Stripe payments, real-time inventory, a rules-based coupon engine, and CJ Dropshipping supplier integration.

---

## 1. Project Overview

E-Commerce Center Backend is a full-featured commerce API designed to handle the hard parts of real e-commerce: **idempotent checkout under concurrent traffic**, **guest-to-authenticated cart migration**, **multi-rule coupon evaluation**, **Stripe payment lifecycle via webhooks**, and **dropshipping supplier automation**.

The system serves two audiences simultaneously:

- **Storefront consumers** — browse catalog, manage carts (as guest or authenticated user), checkout with Stripe, track orders.
- **Admin operators** — manage products/categories/variants, monitor inventory, configure coupons, issue refunds, view revenue dashboards, and import products from CJ Dropshipping.

Built with a strict **Clean Architecture** boundary (API → Application → Domain → Infrastructure) so business logic stays testable and infrastructure stays swappable.

---

## 2. Key Features

### Authentication & Identity

- **JWT access tokens (30 min) + HttpOnly refresh-token cookies (7 days)** — short-lived access tokens reduce blast radius; refresh tokens never touch JavaScript.
- **Google OAuth via Firebase** — server verifies Firebase ID tokens and provisions local accounts on first login.
- **Guest-to-user cart merge** — on login/register, the guest cart (identified by an encrypted session cookie) is seamlessly merged into the authenticated user's cart.
- **Password recovery** — email-based reset flow via SMTP (MailKit) with 24-hour token expiry.

### Catalog & Product Management

- **Hierarchical categories** — self-referencing tree with `ParentId`; supports n-level nesting.
- **Product variants with JSON options** — each variant carries an `OptionsJson` field (e.g., `{"size":"M","color":"Black"}`), its own SKU, pricing, currency, and independent stock tracking.
- **Collision-safe slug generation** — slugs are generated from titles and made unique via a SQL Server sequence (`slug_suffix_seq`) — no polling loops, safe under concurrent inserts.
- **Image management via Azure Blob Storage** — upload, reorder (integer `SortOrder`), and delete product/variant images; SAS URLs generated on upload.
- **CJ Dropshipping integration** — browse CJ's 3-level category tree, search products, view variants, and single/bulk-import into the local catalog (maps `ExternalProductId`, `ExternalSkuId`, `SupplierPrice` for margin tracking).

### Cart System

- **Dual-identity resolution** — carts work for both guests (via encrypted `sessionId` cookie) and authenticated users (via `userId`), with automatic merge on login.
- **Real-time stock validation** — every add/update checks `InventoryItem.OnHand`; quantity capped at 99 per variant.
- **Coupon application on cart** — validates coupon via the evaluator before storing the code; cart query re-evaluates on every read to reflect latest coupon state.
- **Admin cart visibility** — paginated admin endpoint with search/status filtering and abandoned-cart reminder emails.

### Coupon Engine

A dedicated `CouponEvaluator` service acts as the single source of truth for coupon eligibility:

- **Discount types** — percentage (with optional max cap) and fixed amount.
- **Scope targeting** — restrict coupons to specific categories, products, or individual variants; empty scope = applies globally.
- **Usage limits** — global cap (`UsageLimit`) and per-user cap (`PerUserLimit`); guest sessions get a simpler flag-based check.
- **Date windows** — optional `StartsAt` / `ExpiresAt` range.
- **Minimum order amount** — only apply if qualifying subtotal meets threshold.
- **Atomic usage tracking** — `Coupon.UsedCount` incremented + `CouponUsage` record created inside the checkout transaction.

### Checkout & Payments

- **Idempotent order placement** — `POST /api/v1/checkout` requires an `Idempotency-Key` header; the request body is SHA-hashed and the response is cached. Same key + same hash = replay cached response. Same key + different hash = HTTP 409 Conflict. Enforced by a DB unique constraint on `(Key, Route)`.
- **Atomic transaction** — a single `ExecuteInTransactionAsync` wraps: variant/stock validation → order creation (with items, addresses, status history) → coupon usage → Stripe PaymentIntent creation → PaymentAttempt record → outbox messages → idempotency cache. Any failure rolls back everything.
- **Multi-currency & tax** — variants carry their own `CurrencyCode` (ISO 4217); tax rate is looked up by billing country (configurable per-country in `StoreSettings`).
- **Stripe PaymentIntents** — created server-side; `clientSecret` returned to the frontend for client-side confirmation (supports 3DS).
- **Stripe webhook handler** — verifies signature, deduplicates events via `PaymentProviderEvent` unique constraint, then:
  - `payment_intent.succeeded` → marks PaymentAttempt succeeded → transitions Order to Paid → **deducts inventory atomically** → emits outbox events.
  - `payment_intent.payment_failed` → marks attempt failed; order stays PendingPayment for retry.
  - `charge.refunded` / `charge.refund.updated` → updates local Refund status → recalculates order refund state (Refunded vs PartiallyRefunded).
- **Admin refunds** — admin creates a refund request; Stripe API called; result tracked with separate `Refund` entity and `AuditLog`.

### Inventory

- **Optimistic concurrency** — `InventoryItem.RowVersion` mapped to SQL Server's `rowversion` column; EF Core automatically detects mid-air collisions on stock updates.
- **Adjustment history** — every stock change is recorded as an `InventoryAdjustment` (delta, reason, actor) for full auditability.
- **Inventory deduction at payment confirmation** — stock is reduced inside the webhook handler's transaction, not at checkout creation, preventing phantom holds.

### Dashboard & Analytics

- **KPI cards** — total revenue, order count, unique customers, average order value — each with previous-period comparison for trend calculation.
- **Revenue chart** — daily breakdown powered by Stripe charge data within a configurable window (1–365 days).
- **Order status breakdown** — grouped counts across all lifecycle states.
- **Top 5 products** — ranked by revenue from successful orders.
- **Inventory alerts** — low-stock (≤ 10 units) and out-of-stock variant counts with actionable detail.

### Background Workers

- **OutboxPublisherWorker** — polls every 5 s; batch-processes 50 messages with `UPDLOCK, READPAST` hints for concurrency safety; dead-letters after 10 failed attempts.
- **CartCleanupWorker** — runs daily at 04:00 UTC; batch-deletes guest carts inactive > 7 days (cascade-deletes items).
- **SupplierTokenRefreshWorker** — runs every 6 hours; proactively refreshes CJ Dropshipping OAuth tokens expiring within 48 hours.
- **IdempotencyCleanupWorker** — runs daily at 03:00 UTC; purges expired idempotency keys in batches.

### Reliability Primitives

- **Transactional outbox** — domain events written to `OutboxMessages` inside the same transaction as the business operation; published asynchronously by the background worker.
- **Idempotency keys** — checkout + any future mutation can be made idempotent by requiring a client-supplied key + body hash.
- **Audit logging** — `AuditLog` entity captures actor, action, entity type, before/after JSON snapshots.

---

## 3. Tech Stack

| Layer          | Technology                    | Why                                                                                 |
| -------------- | ----------------------------- | ----------------------------------------------------------------------------------- |
| **Runtime**    | .NET 10 / ASP.NET Core        | Modern minimal hosting, strong DI, high-throughput Kestrel pipeline                 |
| **CQRS**       | MediatR 12                    | Decouples controllers from business logic; enables cross-cutting pipeline behaviors |
| **Validation** | FluentValidation 11           | Declarative, testable rules injected as a MediatR pipeline behavior                 |
| **ORM**        | EF Core 9 + SQL Server        | Relational consistency for orders/payments/inventory; code-first migrations         |
| **Identity**   | ASP.NET Identity + JWT Bearer | Role-based authorization, password policies, token providers                        |
| **Payments**   | Stripe.net 50                 | PaymentIntents API, refunds, charge reporting, signed webhook verification          |
| **Auth**       | Firebase Admin SDK            | Server-side Google ID token verification for OAuth flows                            |
| **Email**      | MailKit                       | SMTP delivery for password recovery and cart reminder emails                        |
| **Storage**    | Azure.Storage.Blobs           | Product image hosting with SAS URL generation                                       |
| **Supplier**   | CJ Dropshipping API           | Product/category catalog browsing, variant import, freight calculation              |

---

## 4. Architecture & Design

```
┌──────────────────────────────────────────────────────┐
│                    API Layer                          │
│  Controllers · Middleware · CORS · Cookie Handling    │
├──────────────────────────────────────────────────────┤
│               Application Layer                      │
│  Features/* (Commands/Queries/Handlers/Validators)   │
│  Result<T> Pattern · ApiResponse Wrapper             │
│  MediatR Pipeline · Business Rule Codes              │
├──────────────────────────────────────────────────────┤
│                 Domain Layer                          │
│  Entities · Enums · Value Objects                     │
│  (Zero dependencies on infrastructure)               │
├──────────────────────────────────────────────────────┤
│             Infrastructure Layer                      │
│  EF Core · Repositories · Stripe · Firebase · SMTP   │
│  Azure Blob · CJ Dropshipping · Background Workers   │
└──────────────────────────────────────────────────────┘
```

**Key design decisions:**

- **Result pattern over exceptions** — every handler returns `Result<T>` with semantic `BusinessRuleCode` enums (e.g., `InsufficientStock`, `CouponExpired`, `CurrencyMismatch`). The base `AppController.HandleResult` maps these to appropriate HTTP status codes (400/401/403/404/409/422/500), keeping controllers thin.
- **Unit of Work with explicit transactions** — `IEfUnitOfWork.ExecuteInTransactionAsync()` wraps multi-step operations (checkout, webhook processing) so partial failures always roll back.
- **Generic repository + specialized repositories** — `GenericRepository<T>` handles standard CRUD; domain-specific repositories (e.g., `CartRepository`, `ProductVariantRepository`) add optimized queries with projections.
- **Centralized exception middleware** — catches `ValidationException`, `DbUpdateConcurrencyException`, `DbUpdateException` (duplicate key, FK violations), and all unhandled exceptions — returning consistent `ApiResponse<T>` JSON.
- **DB sequences over GUIDs/polling** — slug suffixes and SKU numbers use SQL Server sequences (`slug_suffix_seq`, `variant_sku_seq`) for contention-free, sequential generation.

---

## 5. Challenges & Solutions

| Challenge                                             | Solution                                                                                                                                                                                                 |
| ----------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Duplicate orders from network retries**             | Idempotency keys with body-hash validation and DB unique constraint on `(Key, Route)`. Cached responses replayed on retry; mismatched bodies return 409.                                                 |
| **Inventory overselling under concurrency**           | Optimistic concurrency via SQL Server `rowversion` on `InventoryItem`. Stock deducted atomically inside the webhook transaction (not at checkout creation), preventing phantom holds.                    |
| **Guest-to-authenticated cart continuity**            | Encrypted session cookie identifies guest carts. On login/register, guest items are merged by variant ID (quantities summed, capped at 99). Session cookie cleared post-merge.                           |
| **Coupon evaluation complexity**                      | Single-responsibility `CouponEvaluator` service handles all rule combinations (scope, limits, dates, minimums) in a defined pipeline. Used identically at cart-read time and checkout-commit time.       |
| **Webhook event ordering & duplication**              | `PaymentProviderEvent` table with unique constraint on `(Provider, EventId)` ensures each Stripe event is processed exactly once. Idempotent status transitions (e.g., already-Paid orders are skipped). |
| **N+1 queries in checkout validation**                | Variants and inventory items are bulk-loaded into dictionaries by ID; all validations run in-memory against the maps. Inventory items sorted by VariantId before locking to prevent deadlocks.           |
| **Slug collisions under concurrent product creation** | DB sequence (`slug_suffix_seq`) generates unique numeric suffixes in a single atomic call — no retry loops or optimistic checks needed.                                                                  |

---

## 6. Performance & Optimization

- **Bulk data access** — checkout loads all variants and inventory items in two queries (not per-item), reducing round-trips from O(n) to O(1).
- **Sorted locking order** — inventory items are processed in `VariantId` order within transactions to prevent deadlock cycles.
- **Cursor-based pagination** — Stripe charge listing uses `startingAfter` cursors for O(1) page navigation regardless of dataset size.
- **Projection queries** — cart and catalog list queries project only needed columns via EF Core `.Select()`, avoiding full-entity materialization.
- **Batch background processing** — outbox publisher processes 50 messages per cycle with `UPDLOCK, READPAST` hints; cleanup workers use `ExecuteDeleteAsync` for set-based batch deletes.
- **Strict JWT expiry** — `ClockSkew = TimeSpan.Zero` eliminates the default 5-minute grace period, keeping the token validity window tight.

---

## 7. How to Run the Project

### Prerequisites

- .NET SDK **10.0**
- SQL Server (LocalDB, Docker, or remote instance)

### 1. Clone & configure

```bash
git clone https://github.com/MoathEssa/ecommerce-platform-backend.git
cd ecommerce-platform-backend
```

Set the connection string in `src/ECommerceCenter.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ECommerceCenter;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Store secrets via **User Secrets** or environment variables:

| Key                               | Description                                 |
| --------------------------------- | ------------------------------------------- |
| `JwtSettings:SecretKey`           | ≥ 32-character signing key                  |
| `Stripe:SecretKey`                | Stripe secret key (`sk_...`)                |
| `Stripe:WebhookSecret`            | Stripe webhook signing secret (`whsec_...`) |
| `BlobStorage:AccountName`         | Azure Storage account name                  |
| `BlobStorage:AccountKey`          | Azure Storage account key                   |
| `FirebaseAuth:ServiceAccountJson` | Firebase service account JSON               |
| `CjDropshipping:ApiKey`           | CJ Dropshipping API key                     |
| `SmtpSettings:*`                  | SMTP host, port, username, password         |

### 2. Apply migrations

```bash
dotnet ef database update \
  --project src/ECommerceCenter.Infrastructure \
  --startup-project src/ECommerceCenter.API
```

### 3. Run

```bash
dotnet run --project src/ECommerceCenter.API
```

API available at `http://localhost:5247` (see `Properties/launchSettings.json`).

---

## 8. Future Improvements

- **Event-driven side effects** — connect outbox publisher to a real message broker (RabbitMQ / Azure Service Bus) to decouple email, analytics, and supplier sync.
- **Rate limiting** — apply per-IP and per-user rate limits on checkout and auth endpoints.
- **Read models for dashboard** — pre-aggregate revenue/order metrics into materialized views to eliminate real-time Stripe API calls.
- **Integration test suite** — cover webhook flows, idempotency race conditions, and coupon edge cases with testcontainers.
- **OpenTelemetry observability** — distributed tracing with correlation IDs from API → outbox → background workers.
- **Multi-tenant support** — extend `StoreSettings` to support multiple storefronts with independent catalogs and configurations.
