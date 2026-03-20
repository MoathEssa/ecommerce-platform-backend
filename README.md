### 1. Project Overview
- **E-Commerce Center Backend** is a production-style REST API that powers both an **admin dashboard** and a **public storefront** (catalog browsing, cart, checkout, payments).
- It solves the “hard parts” of e-commerce that tend to break in real deployments: **auth with refresh tokens**, **idempotent checkout**, **inventory consistency**, **coupon rules**, and **payment lifecycle via Stripe webhooks**.
- Built for teams that care about maintainability: the codebase is structured as **API → Application → Domain → Infrastructure** so business logic stays testable and infrastructure stays swappable.

### 2. Key Features
- **JWT access tokens + HttpOnly refresh-token cookies**: short-lived access tokens for APIs, refresh token stored server-side + cookie-based rotation to reduce XSS exposure.
- **CQRS with MediatR + FluentValidation pipeline**: commands/queries live in `Application/Features/*`, with validation applied consistently via a MediatR behavior.
- **Idempotent checkout**: `POST /api/v1/checkout` requires an `Idempotency-Key` header; the request body is hashed and cached responses are replayed safely to prevent duplicate orders on retries.
- **Atomic order placement**: checkout runs inside a transaction (order, items, addresses, status history, coupon usage, payment attempt) with stock checks done as a bulk read to avoid N+1 patterns.
- **Outbox pattern foundation**: domain events (e.g., `OrderPlaced`, `PaymentIntentCreated`) are written to `OutboxMessages` inside the same transaction; a background worker can publish/process them later.
- **Stripe integration**: creates PaymentIntents, supports refunds, and validates signed Stripe webhook payloads (raw body + `Stripe-Signature`).
- **Supplier + shipping integration hooks**: CJ Dropshipping API client wiring + freight calculation endpoint to support real carrier/price options.
- **Media storage abstraction**: product images are served as static files from `wwwroot` and also support Azure Blob storage via `IImageStorageService`.

### 3. Tech Stack
- **Backend**
  - **.NET 10 / ASP.NET Core**: modern hosting model, strong DI and middleware, high throughput.
  - **MediatR + FluentValidation**: CQRS separation and centralized validation without controller bloat.
- **Database**
  - **SQL Server + EF Core 9**: relational consistency for orders/payments/inventory; migrations in `Infrastructure/Migrations`.
  - **ASP.NET Identity (EF stores)**: robust user management, token providers, and role-based authorization.
- **Integrations**
  - **Stripe (Stripe.net)**: PaymentIntents + refunds + charge reporting.
  - **Firebase Admin SDK**: verifies Google sign-in tokens server-side.
  - **MailKit**: SMTP for password recovery flows.
  - **Azure.Storage.Blobs**: store product images outside the web host when needed.
- **DevOps / Ops**
  - **Transactions + idempotency + outbox**: reliability primitives that scale with real traffic patterns (retries, webhook duplication, eventual consistency).

### 4. Architecture & Design
- **Layered “Clean Architecture” boundaries**
  - **API**: thin controllers, cookie handling, CORS policy, and exception middleware.
  - **Application**: business use-cases in `Features/*` (commands/queries), plus cross-cutting behaviors (validation), result types, and settings.
  - **Domain**: entities and enums (Orders, Payments, Inventory, Coupons, Reliability).
  - **Infrastructure**: EF Core DbContext + repositories + external integrations (Stripe, Firebase, SMTP, Blob).
- **CQRS + Result pattern**
  - Controllers delegate to MediatR; responses are normalized through an API response wrapper + a central `HandleResult` mapper.
- **Reliability by default**
  - **Checkout idempotency** is enforced by a DB constraint on `(Key, Route)` and validated via `RequestHash` to prevent replay with modified payloads.
  - **Outbox** decouples “order transaction” from “side effects” (email, analytics, supplier sync) while keeping correctness.
- **Security posture**
  - Password policies via ASP.NET Identity.
  - JWT bearer validation with strict expiry (`ClockSkew = 0`).
  - Admin endpoints protected with role checks (e.g., payments reporting/refunds).
  - Refresh token stored in **HttpOnly cookie**, sent cross-origin only when CORS is configured with credentials.

### 5. Challenges & Solutions
- **Preventing duplicate orders on flaky networks**
  - Checkout is designed around **idempotency keys + cached responses**, so retries (browser refresh, timeouts, mobile networks) don’t create multiple orders.
- **Avoiding N+1 queries while validating cart items**
  - Variants and inventory are bulk-loaded and stored in maps; validations run in-memory to keep DB round-trips predictable.
- **Keeping payment state consistent with third-party callbacks**
  - Webhooks are treated as an external event stream: the API reads the raw payload for signature verification and routes processing through a dedicated command.
- **Separating domain logic from integrations**
  - Stripe/Firebase/SMTP/Blob are behind interfaces in Application; Infrastructure owns the concrete implementations.

### 6. Performance & Optimization (if applicable)
- **Bulk data access** for checkout validations (variants, inventory) to avoid per-item DB calls.
- **Cursor-based charge listing support** (server + client patterns) to keep admin payment browsing scalable.
- **Short-lived JWTs** and cookie-based refresh reduce the blast radius of leaked access tokens.

### 7. How to Run the Project
1) **Prerequisites**
   - .NET SDK **10.0**
   - SQL Server (localdb or Docker)

2) **Configure settings**
   - Set connection string in `src/ECommerceCenter.API/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ECommerceCenter;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

   - Secrets should be stored via **User Secrets** (the API project has a `UserSecretsId`) or environment variables. Typical settings you’ll need:

   ```json
   {
     "JwtSettings": {
       "SecretKey": "<32+ chars>",
       "Issuer": "ECommerceCenter",
       "Audience": "ECommerceCenterClient"
     },
     "Stripe": {
       "PublishableKey": "pk_...",
       "SecretKey": "sk_...",
       "WebhookSecret": "whsec_..."
     },
     "BlobStorage": {
       "AccountName": "...",
       "AccountKey": "..."
     },
     "FirebaseAuth": {
       "ServiceAccountJson": "{...json...}"
     },
     "CjDropshipping": {
       "ApiKey": "..."
     }
   }
   ```

3) **Apply migrations**
   - From the backend folder:

   ```bash
   dotnet tool restore
   dotnet ef database update --project src/ECommerceCenter.Infrastructure --startup-project src/ECommerceCenter.API
   ```

4) **Run the API**

   ```bash
   dotnet run --project src/ECommerceCenter.API
   ```

   - Default dev URL is `http://localhost:5247` (see `launchSettings.json`).

### 8. Future Improvements
- **Turn on Outbox workers** and route events to real side effects (email, supplier sync, analytics) or a message broker.
- Add **rate limiting** and **structured audit trails** around sensitive admin operations.
- Introduce **read models** for dashboard-heavy queries (pre-aggregations, materialized views) while keeping transactional writes clean.
- Add **integration tests** for webhook flows and idempotency race conditions.
- Add **observability**: OpenTelemetry tracing + correlation IDs from API → outbox → background workers.
