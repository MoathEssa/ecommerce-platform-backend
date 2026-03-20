<div align="center">

# Hi, I'm Moath Essa 👋

### Backend-Focused Full-Stack Developer · .NET · Clean Architecture · CQRS

I build production-grade APIs and full-stack platforms — from multi-vendor e-commerce systems to government-grade license management — always with Clean Architecture, domain-driven design, and reliability engineering at the core.

[![GitHub](https://img.shields.io/badge/GitHub-MoathEssa-181717?style=flat-square&logo=github)](https://github.com/MoathEssa)

</div>

---

## 🧠 About Me

- 🏗️ I architect backends with **Clean Architecture** (Domain → Application → Infrastructure → API), strict layer boundaries, and the **CQRS + MediatR** pattern — so business logic stays testable and infrastructure stays swappable
- ⚡ I care about the hard reliability problems: idempotent mutations, optimistic concurrency, transactional outbox, atomic multi-step checkouts, and race-condition-safe background workers
- 🌐 Full-stack capable — I pair every backend with a **React 19 + TypeScript** SPA using RTK Query, Radix UI, and Tailwind CSS v4
- 🖥️ I've built desktop applications too — Windows Forms apps with 3-tier architecture
- 🌍 Bilingual-aware: several of my platforms support full **Arabic/English RTL/LTR** switching

---

## 🛠️ Tech Stack

### Backend
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF_Core_9-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![MediatR](https://img.shields.io/badge/MediatR-CQRS-512BD4?style=flat-square)
![FluentValidation](https://img.shields.io/badge/FluentValidation-DC3545?style=flat-square)
![C++](https://img.shields.io/badge/C++-00599C?style=flat-square&logo=cplusplus&logoColor=white)

### Databases
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-47A248?style=flat-square&logo=mongodb&logoColor=white)

### Cloud & Infrastructure
![Azure](https://img.shields.io/badge/Azure_Blob_Storage-0078D4?style=flat-square&logo=microsoftazure&logoColor=white)
![Azure Key Vault](https://img.shields.io/badge/Azure_Key_Vault-0078D4?style=flat-square&logo=microsoftazure&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white)
![Firebase](https://img.shields.io/badge/Firebase_Admin_SDK-FFCA28?style=flat-square&logo=firebase&logoColor=black)

### Payments & Third-Party
![Stripe](https://img.shields.io/badge/Stripe-635BFF?style=flat-square&logo=stripe&logoColor=white)

### Frontend
![React](https://img.shields.io/badge/React_19-61DAFB?style=flat-square&logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white)
![Redux Toolkit](https://img.shields.io/badge/Redux_Toolkit-764ABC?style=flat-square&logo=redux&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS_v4-06B6D4?style=flat-square&logo=tailwindcss&logoColor=white)
![Radix UI](https://img.shields.io/badge/Radix_UI-161618?style=flat-square&logo=radixui&logoColor=white)
![Vite](https://img.shields.io/badge/Vite_7-646CFF?style=flat-square&logo=vite&logoColor=white)

---

## 🚀 Featured Projects

### 🛒 E-Commerce Platform
> **Multi-vendor dropshipping platform** — full-stack from payments to supplier automation

A production-grade platform that lets store operators import products from **CJ Dropshipping**, manage inventory, run coupon campaigns, and process **Stripe payments** — all without holding physical stock.

**Backend highlights:**
- Idempotent checkout with SHA-hashed `Idempotency-Key` and DB-level unique constraint
- Atomic checkout transaction: stock validation → order creation → coupon usage → Stripe PaymentIntent → outbox events
- Stripe webhook handler with deduplication, inventory deduction, and partial refund tracking
- Rules-based coupon engine (scope targeting, usage limits, date windows, min-order threshold)
- Transactional outbox + 4 background workers (outbox publisher, cart cleanup, idempotency cleanup, supplier token refresh)
- Guest-to-authenticated cart merge with encrypted session cookies
- CJ Dropshipping OAuth, product import, and freight estimation

**Frontend highlights:**
- 3-step checkout with Stripe `PaymentElement` (3DS-capable)
- Silent session rehydration + automatic 401-retry via cookie-based refresh
- Generic `DataTableV2` with visual AND/OR query builder used across all admin pages
- Drag-and-drop image reordering via dnd-kit
- Real-time revenue dashboard with Recharts area/pie charts

| | |
|---|---|
| **Backend** | .NET 10 · ASP.NET Core · EF Core 9 · SQL Server · MediatR · FluentValidation · Stripe.net · Firebase · Azure Blob · MailKit |
| **Frontend** | React 19 · TypeScript · RTK Query · Radix UI · Tailwind CSS v4 · react-hook-form · zod · Recharts · dnd-kit |

🔗 [Backend Repo](https://github.com/MoathEssa/ecommerce-platform-backend) · [Frontend Repo](https://github.com/MoathEssa/ecommerce-platform-front-end)

---

### 🏥 Family Rehabilitation Center
> **Case management + Qualtrics-style survey engine** — dual-database architecture

A platform for a family rehabilitation center that combines **client intake workflows** with a full survey lifecycle — visual survey editing, branching logic, session-based runner, scoring engine, and analytics export.

**Backend highlights:**
- Dual-database design: **SQL Server** for relational client/auth data, **MongoDB** for polymorphic survey structures and analytics
- Survey editor supporting **19+ question types** via a single PATCH `/operations` endpoint with a polymorphic dispatcher
- Immutable `SurveyVersion` snapshots — editing a live survey never affects in-progress respondents
- JsonLogic-based branching with a runtime `ConditionEvaluator` and `FlowEngine`
- Cursor-based streaming CSV export for large response datasets
- **Firebase phone OTP (2FA)** with JWT access tokens (15 min) + HttpOnly refresh cookies
- Auto-scanned strategy pattern for question rendering, scoring, and piped-text resolution
- Bilingual (Arabic/English) with `Accept-Language` header detection

**Frontend highlights:**
- Full Arabic/English RTL/LTR layout switching — not just translated strings, but fully mirrored layouts
- Localized Zod validation errors using `zod-i18n-map`
- Feature-scoped translation files (no monolithic i18n files)

| | |
|---|---|
| **Backend** | .NET 10 · ASP.NET Core · EF Core 10 · SQL Server · MongoDB · MediatR · FluentValidation · Firebase · Azure Blob · xUnit + Moq |
| **Frontend** | React 19 · TypeScript · RTK Query · shadcn/ui · Tailwind CSS v4 · i18next · Recharts |

🔗 [Backend Repo](https://github.com/MoathEssa/rehabilitation-center-back-end) · [Frontend Repo](https://github.com/MoathEssa/rehabilitation-center-front-end)

---

### 🪪 Driving License Management Center
> **Government-grade driving license authority system** — available as both a REST API and a desktop app

Digitizes the complete lifecycle of a driving license authority: citizen registration, 3-stage test pipelines (vision → written → practical), license issuance, renewals, detentions, releases, and international permits.

**Backend highlights:**
- Complex domain rules enforced in the application layer (not the DB): cannot issue without passing all tests, cannot renew a detained license, cannot issue international without an active local
- 16 repository implementations + Unit of Work pattern for transactional boundaries
- SQL Server views as keyless EF Core entities for optimized multi-table read queries
- Invitation-based user onboarding with email token flow

**Desktop (WinForms) highlights:**
- Clean 3-tier architecture: Presentation (WinForms + Guna.UI2) → Business Logic → Data Access (ADO.NET)
- 7 application types each with their own lifecycle and fee rules
- SHA-256 password hashing, Windows Event Log integration, "Remember Me" via Windows Registry
- Real-time dashboard with live statistics

**Frontend highlights:**
- Full Arabic/English RTL/LTR switching with per-language Zod validation messages
- Auth interceptor with **mutex lock** preventing duplicate token refresh on concurrent 401s
- Cross-tab logout via `BroadcastChannel` API
- Centralized RTK middleware for toast notifications keyed to HTTP status codes

| | |
|---|---|
| **Backend** | .NET 10 · ASP.NET Core · EF Core · SQL Server · MediatR · FluentValidation · Azure Blob · JWT · MailKit |
| **Desktop** | C# · .NET Framework 4.8 · WinForms · Guna.UI2 · ADO.NET · SQL Server |
| **Frontend** | React 19 · TypeScript · RTK Query · shadcn/ui · Tailwind CSS v4 · i18next (AR/EN) · Recharts |

🔗 [Backend Repo](https://github.com/MoathEssa/driving-license-management-back-end) · [Desktop Repo](https://github.com/MoathEssa/driving-license-management-winforms) · [Frontend Repo](https://github.com/MoathEssa/driving-license-management-front-end)

---

### ⚽ SoccerPro — Tournament Management System
> **University soccer tournament platform** — deployed to Azure with Key Vault secrets and Docker

A backend for managing the full lifecycle of competitive soccer: tournaments, team rosters, match scheduling, live result recording (shots, cards, substitutions), and player transfers.

**Highlights:**
- Deployed to **Azure Container Apps** with **Azure Key Vault** (zero secrets in source code, managed identity)
- Dockerized with Linux container support
- Hybrid data access: **EF Core** for Identity/schema, **raw ADO.NET + stored procedures** for performance-critical tournament operations
- Table-valued parameters (TVPs) for batch-inserting match results (shots, cards, substitutions) in a single transactional stored procedure call
- SQL Server views (`PlayerView`, `MatchView`, `TopScorerPlayerView`, `PlayerViolationView`) for precomputed read models
- Six roles: Admin, Coach, Player, Staff, Guest, Manager
- Player transfer workflow with request/approval lifecycle

| | |
|---|---|
| **Stack** | .NET 9 · ASP.NET Core · EF Core 9 + ADO.NET · SQL Server · MediatR · FluentValidation · AutoMapper · Azure Key Vault · Docker · Swagger |

🔗 [Backend Repo](https://github.com/MoathEssa/soccer-pro-back-end)

---

### 🏦 Bank System (C++ Console)
> **OOP console banking application** — file-based persistence, interactive menu navigation

A console-based banking system built with object-oriented C++ featuring client account management, deposits/withdrawals, balance queries, and file-based persistence.

| | |
|---|---|
| **Stack** | C++20 · OOP · File I/O · Visual Studio |

🔗 [Repo](https://github.com/MoathEssa/bank-system-cpp)

---

## 📊 GitHub Stats

<div align="center">

![Moath's GitHub Stats](https://github-readme-stats.vercel.app/api?username=MoathEssa&show_icons=true&theme=tokyonight&hide_border=true&count_private=true)

![Top Languages](https://github-readme-stats.vercel.app/api/top-langs/?username=MoathEssa&layout=compact&theme=tokyonight&hide_border=true&langs_count=6)

</div>

---

## 📌 Recurring Patterns Across My Work

| Pattern | Purpose |
|---|---|
| **Clean Architecture (4 layers)** | Keeps domain logic independent; infrastructure swappable |
| **CQRS via MediatR** | Decouples controllers from business logic; enables pipeline behaviors |
| **Result\<T\> over exceptions** | Typed error codes mapped to HTTP status — no exception-driven control flow |
| **FluentValidation pipeline behavior** | Requests are validated before reaching handlers — zero boilerplate in controllers |
| **Unit of Work + transactions** | Multi-step operations (checkout, webhooks) are atomic or fully rolled back |
| **Transactional outbox** | Domain events published reliably without distributed transaction complexity |
| **Optimistic concurrency** | EF Core `rowversion` prevents silent overwrites on concurrent stock/record updates |
| **Idempotency keys** | Mutation endpoints are replay-safe under network retries |
| **RTK Query + tag invalidation** | Frontend cache stays consistent without manual refetch logic |
| **Auth interceptor with mutex** | Single token refresh shared across concurrent 401 responses |

---

<div align="center">

*Always building systems that are correct before they are fast, and reliable before they are clever.*

</div>
