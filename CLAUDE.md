# Reporting App

A VAT reporting web application. Companies submit sales data per reporting period; the system generates VAT reports per country. (test edit)

## Stack

| Layer    | Technology                              |
|----------|-----------------------------------------|
| Backend  | ASP.NET Core (C#), Entity Framework Core |
| Database | SQL Server Express (`VatReportingDb`)   |
| Frontend | Vue 3, TypeScript, Pinia, Vue Router    |
| Build    | Vite, Vitest, vue-tsc                   |

## Project Structure

```
reporting-app/
├── ReportingApi1/          # ASP.NET Core backend
│   ├── Controllers/        # API endpoints
│   ├── Services/           # Business logic (interfaces + implementations)
│   ├── Entities/           # EF Core entity models
│   ├── DTOs/               # Request/response data shapes
│   ├── Data/               # DbContext (VatReportingContext)
│   ├── Migrations/         # EF Core migrations
│   └── Validation/         # Custom validation attributes (e.g. country codes)
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
Runs on `https://localhost:7xxx` (see launchSettings.json). Swagger UI available at `/swagger`.

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

- **Controllers** are thin — they validate input and delegate to a service
- **Services** contain all business logic and are registered as `Scoped`
- Every service has an interface (`IXxxService`) injected via DI
- DTOs are separate from Entities — never expose entities directly from the API
- CORS is configured to allow `http://localhost:5173` and `http://localhost:3000`

## Auth

- Auth is basic (username + hashed password stored in DB)
- Login returns `{ id, userName, companyId }` — no JWT yet
- TODO: replace with JWT token auth before production

## Database

- SQL Server Express, local instance `localhost\SQLEXPRESS`
- Database name: `VatReportingDb`
- Connection string is in `appsettings.json`

## Key Domain Concepts

- **Company** — the registered entity submitting reports
- **ReportingPeriod** — a time window (e.g. Q1 2026) during which sales are entered
- **SalesEntry** — a line of sales data tied to a period and product
- **VatReport** — aggregated VAT per country, generated from sales entries
- **Product** — items sold, seeded via migration

## Do Not

- Expose EF entities directly from API responses — always use DTOs
- Use `.Result` or `.Wait()` on async calls — always use `await`
- Add unnecessary comments or docstrings to unchanged code
- Create new files unless strictly necessary
