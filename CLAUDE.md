# Reporting App

A Denmark-based OSS (One Stop Shop) intake portal. EU-registered companies submit cross-border B2C sales per destination country, and the system calculates the destination-country VAT each filer owes. Models the SKAT-side OSS workflow.

## Stack

| Layer      | Technology                                        |
|------------|---------------------------------------------------|
| Backend    | ASP.NET Core (.NET 10), Entity Framework Core      |
| Database   | SQL Server Express (`VatReportingDb`)              |
| Auth       | JWT bearer (HS256) + refresh-token cookie          |
| Validation | FluentValidation                                   |
| Logging    | Serilog                                            |
| Frontend   | Vue 3, TypeScript, Pinia, Vue Router               |
| Build      | Vite, Vitest, vue-tsc                              |

## Project Structure

```
reporting-app/
├── ReportingApi1/          # ASP.NET Core backend
│   ├── Controllers/        # API endpoints (thin)
│   ├── Services/           # Business logic + VAT calculation engine
│   ├── Repositories/       # Data access for swappable sources (e.g. VAT rates)
│   ├── Entities/           # EF Core entity models
│   ├── DTOs/               # Request/response shapes
│   ├── Data/               # DbContext (VatReportingContext)
│   ├── Migrations/         # EF Core migrations
│   ├── Validation/         # FluentValidation validators + country codes
│   └── Infrastructure/     # JWT settings, global exception handler, dev seed
└── frontend/               # Vue 3 frontend
    └── src/
        ├── views/          # Page-level components
        ├── components/     # Reusable UI components
        ├── stores/         # Pinia stores
        └── router/         # Vue Router config
```

## Running the App

**Backend** (from `ReportingApi1/`):
```bash
dotnet run
```
Runs on `https://localhost:7xxx` (see `launchSettings.json`). Swagger UI at `/swagger`.

**Frontend** (from `frontend/`):
```bash
npm run dev
```
Runs on `http://localhost:5173`.

**Type check frontend:**
```bash
npm run type-check
```

**Run frontend tests:**
```bash
npm run test:unit
```

**Add EF migration** (from `ReportingApi1/`):
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Architecture Conventions

- **Controllers** are thin — they delegate to FluentValidation for input shape and to a service for behavior
- **Services** contain business logic and orchestration; registered as `Scoped`
- **Repositories** abstract data sources we expect to swap later (the VAT rate table is the obvious case — could come from EF today, SKAT's API tomorrow). Registered as `Scoped`. Naming: `IXxxRepository` for the interface; impls prefixed by source (e.g. `EfVatRateRepository`).
- Every service has an interface (`IXxxService`) injected via DI
- DTOs are separate from Entities — never expose entities directly from the API
- The VAT calculation engine aims to be a pure domain function — see README for details
- CORS is configured to allow `http://localhost:5173` and `http://localhost:3000`

## Auth

- JWT bearer (HS256), settings under `JwtSettings` in `appsettings.json`
- `[Authorize]` is applied globally via a filter in `Program.cs` — actions opt out with `[AllowAnonymous]`
- Two roles: `Admin` (tax authority side) and `User` (company-side filer) — enum `UserRole` in `Entities/User.cs`
- Login (`POST /api/auth/login`) issues an access token; refresh token is set as an HttpOnly cookie via `POST /api/auth/refresh`
- Logout clears the refresh cookie

## Database

- SQL Server Express, local instance `localhost\SQLEXPRESS`
- Database name: `VatReportingDb`
- Connection string in `appsettings.json`
- All entities use `RowVersion` for optimistic concurrency

## Key Domain Concepts

- **Company** — an EU-registered business filing through the DK OSS portal (the "filer")
- **User** — a login attached to a Company; has a `UserRole` (Admin or User)
- **ReportingPeriod** — an OSS filing window (typically a quarter)
- **SalesEntry** — one cross-border B2C sale: destination country, amount, product category, sale date
- **VatReport** — one filer's submission for one period; aggregates SalesEntries; one report per (Company, Period)
- **Product** — items with categories that drive VAT rate selection (Food/Books/Medicine eligible for reduced rates)
- **VAT Calculation Engine** — pure domain function that takes a sale plus a rate snapshot and returns a `TaxBreakdown` (in progress; see README)
- **VAT Rate Repository** — abstracted rate-table lookup so the source can change later without touching the engine

OSS scope only: domestic sales, B2B with valid VAT number, non-EU buyers, and exempt categories are out of scope and rejected at the validation boundary — they don't reach the engine.

## Do Not

- Expose EF entities directly from API responses — always use DTOs
- Use `.Result` or `.Wait()` on async calls — always use `await`
- Add unnecessary comments or docstrings to unchanged code
- Create new files unless strictly necessary

## Working With Me

- This codebase is a learning exercise. I want to gain experience that transfers to real jobs.
- **Do not write or edit code unless I explicitly ask you to.** Default to guidance only — explain the approach, point to the relevant lines, describe the shape of the fix. I'll write the code myself. If you think a code change is warranted, ask first.
- When recommending approaches, **default to what is standard in production codebases** (the mainstream industry norm), since that's what I'll encounter in real .NET / Vue jobs.
- **But if a non-norm pattern is clearly better for the situation, recommend it** — just be explicit that it's not the mainstream choice and explain why it wins here. I'd rather learn when to break from the norm than blindly follow it.
- Call out *why* something is the norm (e.g. "built into ASP.NET Core," "what most job postings assume you know") so I build intuition, not just rules.
