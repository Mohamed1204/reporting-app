# Reporting App — DK OSS Intake Portal

A Denmark-based OSS (One Stop Shop) intake portal. Models the SKAT-side workflow: EU-registered companies log in, declare cross-border B2C sales per destination country, and the system calculates the destination-country VAT each filer owes.

This is a learning project for production-shape ASP.NET Core / Vue patterns — not a real tax filing system.

## Stack

| Layer      | Technology                                                        |
|------------|-------------------------------------------------------------------|
| Backend    | ASP.NET Core (.NET 10), EF Core                                    |
| Database   | SQL Server Express (`VatReportingDb` on `localhost\SQLEXPRESS`)    |
| Auth       | JWT bearer (HS256), refresh-token cookie                           |
| Validation | FluentValidation                                                   |
| Logging    | Serilog                                                            |
| Frontend   | Vue 3 + TypeScript, Pinia, Vue Router, Vite, Vitest                |

## Repository Layout

```
reporting-app/
├── ReportingApi1/                  # Backend
│   ├── Controllers/                # API endpoints
│   ├── Services/                   # Business logic, VAT engine
│   ├── Repositories/               # Swappable data sources (rates)
│   ├── Entities/                   # EF entity models
│   ├── DTOs/                       # Request/response shapes
│   ├── Data/                       # DbContext
│   ├── Migrations/                 # EF migrations
│   ├── Validation/                 # FluentValidation validators + CountryCodes
│   ├── Infrastructure/             # JWT config, global exception handler, dev seed
│   └── Program.cs                  # DI registration, middleware pipeline
├── ReportingApi1.Tests/            # Backend test project (xUnit)
└── frontend/                       # Vue 3 app
    └── src/
        ├── views/
        ├── components/
        ├── stores/
        └── router/
```

## Domain Model

### Company (Filer)
- An EU-registered business that submits OSS reports
- Properties: `Name`, `Country`, `RowVersion`

### User
- A login attached to a Company
- `UserRole`: `Admin` (tax authority side) or `User` (company-side filer)

### ReportingPeriod
- An OSS filing window (typically a quarter)
- Properties: `StartDate`, `EndDate`, `Status`

### SalesEntry
- One cross-border B2C sale inside a VAT report
- Key fields: `BuyerCountry` (destination, ISO-3166 alpha-2), `Amount`, `Currency`, `ProductCategory`, `SaleDate`, `BuyerType`, `BuyerHasValidVatNumber`, `SellerCountry`
- Scoped to OSS-applicable sales only — domestic, B2B-with-VAT-number, non-EU, and exempt-category sales are filtered out at the validation boundary

### VatReport
- One filer's submission for one period
- Aggregates SalesEntries; one report per `(Company, ReportingPeriod)` (unique index)
- Lifecycle: `Draft → Submitted → ...` (Approved/Rejected to be wired)

### Product
- Items with categories that drive VAT rate selection
- Reduced-rate eligible: Food, Books, Medicine
- Exempt (out of OSS scope, rejected at validation): FinancialServices, Education

## VAT Calculation Engine

The heart of the system. **This part of the codebase is in active refactor — what's described below is the target shape, not all of it exists yet.** See *Current state* below for what's actually built.

### Goal: a pure domain function

```
TaxBreakdown Calculate(VatCalculationRequest req, VatCalculationContext ctx)
```

- **Pure** — synchronous, deterministic, no I/O, no `DbContext`, no `DateTime.UtcNow`
- **Unit-testable in isolation** — no host bootstrap, no DI container, no in-memory DB
- Same `(req, ctx)` always returns the same `TaxBreakdown`

### Why pure?

VAT rules are testable, frozen, deterministic logic. Coupling them to EF or HTTP makes every test slow and every async-cascade painful. Keeping the engine pure means rule changes touch only the engine, and rate-source changes touch only the repository.

### Architecture (target)

```
Controllers
   ↓
Services/VatReportService              ← orchestration: loads sales, fetches rates,
   │                                      calls engine, persists, returns DTO
   │
   ├─→ Repositories/IVatRateRepository ← swappable: EF today, SKAT API later
   │      returns: RateBook (immutable rate snapshot)
   │
   └─→ Services/IVatCalculator         ← pure engine
          (req, ctx) → TaxBreakdown
```

### Concepts

- **`VatCalculationRequest`** — small immutable record with what the engine needs (BuyerCountry, Category, Net, SaleDate). Decoupled from the EF `SalesEntry` entity so the engine doesn't depend on storage shape.
- **`VatCalculationContext`** — carries the rate snapshot (and later: `AsOf` date)
- **`RateBook`** — frozen snapshot of effective-dated rate rows. `GetRate(country, category, date)` does a category fallback: reduced row if a category-specific row exists for the date, otherwise the country's standard row.
- **`TaxBreakdown`** — return shape: Net, Rate, VatAmount, Gross, Scheme, RateAppliedFromCountry, Explanation
- **`VatScheme`** — enum (currently just `Oss`; legacy zero-rated/reverse-charge/exempt schemes are filtered out at validation)
- **`VatRate`** — value object so percentage-vs-multiplier ambiguity becomes a compile error (deferred until ambiguity bites)

### Current state

- `Services/VatCalculationEngine.cs` exists but doesn't compile — references `TaxBreakdown`, `VatScheme`, `VatCalculationContext` which haven't been built yet
- `Repositories/VatRateRepository.cs` is a stub returning `0.5m` for any input
- The engine takes the EF `SalesEntry` entity directly (target: `VatCalculationRequest`)
- Whether the repository is injected into the engine vs the orchestrator is undecided
- `VatReportService` still computes VAT inline as `Amount * VatRate / 100` at lines 64 and 255

### Out of OSS scope (rejected at validation)

- Domestic sales (`BuyerCountry == SellerCountry`)
- B2B with valid VAT number (reverse charge — handled by buyer)
- Non-EU destinations (zero-rated export)
- Exempt categories (FinancialServices, Education)

These don't reach the engine. The engine assumes a valid OSS sale and computes the destination-country rate.

## Concurrency

All entities have `RowVersion` (SQL Server `rowversion`). EF maps it as a concurrency token:
- First write wins
- Conflicting writes raise `DbUpdateConcurrencyException`, surfaced as HTTP 409 by `Infrastructure/GlobalExceptionHandler`
- Frontend reads `RowVersion` from GET responses and includes it in PUTs

## Auth

- JWT bearer (HS256); settings under `JwtSettings` in `appsettings.json`
- `[Authorize]` applied globally via a filter in `Program.cs` — actions opt out with `[AllowAnonymous]`
- Two roles: `Admin` (tax authority side) and `User` (company-side filer)
- Login issues an access token; refresh token sent as HttpOnly cookie via `POST /api/auth/refresh`
- Logout clears the refresh cookie

## Validation

- FluentValidation, auto-registered from the `ReportingApi1` assembly
- Validators in `Validation/` (e.g. `CreateVatReportValidator`)
- Country codes validated via `[ValidCountryCode]` against `Validation/CountryCodes` (EU-27 + EEA + GB/CH)

## Logging

- Serilog, configured via the `Serilog` section of `appsettings.json`
- `app.UseSerilogRequestLogging()` logs every HTTP request

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server Express (`localhost\SQLEXPRESS`)

### Backend (from `ReportingApi1/`)

```bash
dotnet restore
dotnet ef database update          # apply migrations
dotnet run                          # see launchSettings.json for ports
```

Swagger UI at `/swagger`. Includes a `Bearer` security scheme — paste a JWT to call protected endpoints.

### Frontend (from `frontend/`)

```bash
npm install
npm run dev                         # http://localhost:5173
npm run type-check                  # vue-tsc
npm run test:unit                   # Vitest
```

### Migrations

```bash
dotnet ef migrations add <Name>
dotnet ef database update
```

## API Endpoints

### Auth (`/api/auth`)
- `POST /login` — exchange credentials for an access token + refresh cookie
- `POST /register` — create a new user/company
- `POST /refresh` — exchange refresh cookie for a new access token
- `POST /logout` — clear refresh cookie

### Companies (`/api/companies`)
- `GET /` — list (filter with `?name=`)
- `GET /{id}` — get one
- `POST /` — create
- `PUT /{id}` — update
- `DELETE /{id}` — delete

### Reporting Periods (`/api/reportingperiods`)
- `GET /` — list
- `GET /{id}` — get one
- `POST /` — create
- `DELETE /{id}` — delete

### VAT Reports (`/api/vatreports`)
- `GET /` — paged list
- `GET /{id}` — get one with sales entries
- `POST /` — create *(Admin only)*
- `PUT /` — update *(Admin only)*
- `DELETE /{id}` — delete *(Admin only)*
- `POST /save` — filer saves a draft
- `POST /submit` — filer submits the report

### Sales Entries (`/api/salesentries`)
- `DELETE /{id}` — remove one entry from a report

### Products (`/api/products`)
- `GET /` — list
- `GET /{id}` — get one

## Status / Known Gaps

- VAT calculation engine is mid-refactor — see *Current state* above
- Effective-dated rate table not in DB yet; the rate repository is a stub
- Filer-side summary view not built
- Tax-authority redistribution view (per-destination-country totals across filers) not built
- Filing lifecycle states beyond Draft/Submitted not enforced
- `SalesEntry.productCategory` casing inconsistent with C# convention (should be `ProductCategory`)
- Unique index on `(VatReportId, BuyerCountry)` will reject legal data once category/buyer-type vary — needs to be widened or removed

## Frontend Integration

CORS allows:
- `http://localhost:5173` (Vite default)
- `http://localhost:3000` (Vue CLI default)

Both with credentials (`AllowCredentials`) so the refresh cookie round-trips.
