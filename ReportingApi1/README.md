# VAT Reporting System - Backend API

A .NET 8 Web API for managing VAT reports, built as a practice project for learning EF Core, clean architecture, and concurrency handling.

## 🏗️ Architecture

```
ReportingApi1/
├── Entities/               # Domain models (DB entities)
│   ├── Company.cs
│   ├── ReportingPeriod.cs
│   ├── VatReport.cs
│   └── SalesEntry.cs
├── Data/                   # DbContext
│   └── VatReportingContext.cs
├── DTOs/                   # Data Transfer Objects
│   ├── CompanyDto.cs
│   ├── ReportingPeriodDto.cs
│   ├── VatReportDto.cs
│   └── SalesEntryDto.cs
├── Services/               # Business logic
│   ├── CompanyService.cs
│   ├── ReportingPeriodService.cs
│   └── VatReportService.cs
└── Controllers/            # API endpoints
    ├── CompaniesController.cs
    ├── ReportingPeriodsController.cs
    └── VatReportsController.cs
```

## 🎯 Domain Model

### Company
- Has many VAT reports
- Properties: Name, Country

### ReportingPeriod
- Represents a time period (e.g., Q1 2026)
- Properties: StartDate, EndDate, Status (Open/Closed/Locked)

### VatReport
- Belongs to one Company and one ReportingPeriod
- Contains multiple SalesEntries
- Properties: SubmittedAt, Status (Draft/Submitted/Approved/Rejected)
- **Constraint**: One report per company per period (unique index)

### SalesEntry
- Belongs to one VatReport
- Properties: Country, Amount, VatRate
- **VatAmount is calculated** (not stored): `Amount * VatRate / 100`

## 🔐 Concurrency Handling

All entities have `RowVersion` timestamp for optimistic concurrency control:
- SQL Server uses `rowversion` type
- Updates that conflict return 409 Conflict with helpful error message
- Prevents silent data overwrites when multiple users edit the same record

## 📚 Key Features Implemented

✅ **EF Core**
- Entity relationships (1-to-many, foreign keys)
- Fluent API configuration
- Migrations
- Optimistic concurrency with RowVersion

✅ **Clean Architecture**
- Separation of concerns: Entities → Services → Controllers
- DTOs separate from entities
- Interface-based services for testability

✅ **Calculated Fields**
- `VatAmount` calculated in DTOs
- `TotalAmount` and `TotalVat` aggregated in service layer
- Not stored in database (follows DRY principle)

✅ **API Best Practices**
- RESTful endpoints
- Proper HTTP status codes
- Error handling with meaningful messages
- CORS configured for frontend

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server Express (localhost\SQLEXPRESS)

### Setup

1. **Restore packages**
   ```bash
   dotnet restore
   ```

2. **Database is already created**, but if you need to recreate:
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```

3. **Run the API**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI**
   - HTTP: http://localhost:5247/swagger
   - HTTPS: https://localhost:7033/swagger

## 🧪 API Endpoints

### Companies
- `GET /api/companies` - Get all companies
- `GET /api/companies/{id}` - Get company by ID
- `POST /api/companies` - Create new company
- `PUT /api/companies/{id}` - Update company
- `DELETE /api/companies/{id}` - Delete company

### Reporting Periods
- `GET /api/reportingperiods` - Get all periods
- `GET /api/reportingperiods/{id}` - Get period by ID
- `POST /api/reportingperiods` - Create new period
- `DELETE /api/reportingperiods/{id}` - Delete period

### VAT Reports
- `GET /api/vatreports` - Get all reports
- `GET /api/vatreports/{id}` - Get report by ID
- `GET /api/vatreports/company/{companyId}` - Get reports by company
- `POST /api/vatreports` - Create new report
- `PUT /api/vatreports/{id}` - Update report
- `DELETE /api/vatreports/{id}` - Delete report

## 📝 Example: Testing Concurrency

1. Create a company via POST `/api/companies`:
   ```json
   {
     "name": "Acme Corp",
     "country": "Netherlands"
   }
   ```

2. Get the company (note the RowVersion in response)

3. Try updating from two different clients with the same RowVersion
   - First update succeeds
   - Second update returns 409 Conflict

## 🎓 Learning Points Covered

- [x] EF Core entity configuration
- [x] Database relationships and constraints
- [x] Migrations workflow
- [x] Service layer pattern
- [x] DTOs vs Entities separation
- [x] Optimistic concurrency handling
- [x] Calculated properties (not persisted)
- [x] RESTful API design
- [x] Error handling and status codes
- [x] CORS configuration

## 🔗 Frontend Integration

CORS is configured to accept requests from:
- `http://localhost:5173` (Vite default)
- `http://localhost:3000` (Vue CLI default)

## 📦 NuGet Packages

- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.11)
- `Microsoft.EntityFrameworkCore.Tools` (8.0.11)
- `Microsoft.EntityFrameworkCore.Design` (8.0.11)

## 🗃️ Database Schema

Tables created:
- `Companies` (Id, Name, Country, RowVersion)
- `ReportingPeriods` (Id, StartDate, EndDate, Status, RowVersion)
- `VatReports` (Id, CompanyId, ReportingPeriodId, SubmittedAt, Status, RowVersion)
- `SalesEntries` (Id, VatReportId, Country, Amount, VatRate, RowVersion)

Indexes:
- `Companies.Name`
- `ReportingPeriods.(StartDate, EndDate)`
- `VatReports.(CompanyId, ReportingPeriodId)` - UNIQUE

---

**Built with .NET 8 as a learning project** 🚀
