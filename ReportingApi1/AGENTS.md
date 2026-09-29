# Project context and working guidance

## User and learning goals

- has about five years of frontend/backend experience, with frontend as his strongest area.
- He is learning applied AI engineering through this existing project. Focus on C# now; Python can come later.
- Learning the concepts matters, including embeddings, vector databases, retrieval, evaluations, and tool calling, even if the first feature does not need them.
- Explain new concepts with concrete examples from this app. For learning questions, present a manageable next step and explain why it exists. For implementation requests, complete the requested scope and explain what changed and how it was verified.
- Distinguish code changes, database changes, and starting/stopping processes when reporting results.
- The detailed curriculum and progress log are in [AI-ENGINEERING-LEARNING-PATH.md](AI-ENGINEERING-LEARNING-PATH.md). Read relevant sections when continuing the learning work.

## Application and active frontend

- This is a learning project for an OSS VAT reporting portal, not a real tax filing system.
- Backend: this directory, ASP.NET Core / C# (.NET 10), EF Core, SQL Server, JWT authentication, FluentValidation, and Serilog.
- Active frontend: `../frontend`, regular Vue 3 + TypeScript, Pinia, Vue Router, and Vite.
- `../frontend-nuxt` is a separate implementation. Mohammed explicitly said he is using regular Vue. Investigate and change the regular Vue app unless the current task says otherwise.
- Backend tests: `../ReportingApi1.Tests`. Frontend unit tests use Vitest.
- Some README descriptions are stale. Verify implementation before treating a documented gap as current.

## Local development

- Start the backend from this directory with `dotnet run --launch-profile https`.
- The HTTPS profile listens on `https://localhost:7033` and `http://localhost:5247`.
- Start the frontend from `../frontend` with `npm run dev`; normally it serves `http://localhost:5173`.
- Vue's Vite proxy forwards `/api` to `https://localhost:7033`. The HTTP-only backend profile does not serve that target.
- Both frontend and backend processes must be running. Check reachability before diagnosing credentials when login fails.
- The current Vue login code labels unsuccessful HTTP responses as invalid credentials, including server failures. Check the actual response status.
- Local SQL Server: `localhost\SQLEXPRESS`; development database: `VatReportingDb`. Confirm effective configuration when troubleshooting.
- Relevant checks: `dotnet build`, `dotnet test ../ReportingApi1.Tests/ReportingApi1.Tests.csproj`, and, from `../frontend`, `npm run type-check` / `npm run test:unit:ci`. Choose checks appropriate to the change.
- Do not store API keys, passwords, or authentication tokens in project context documents.

## Domain facts

- `ReportingPeriods` defines date windows shared across companies. `PeriodStatus` is Open / Closed / Locked.
- `VatReports` links a company to a reporting period. Its `Status` is Draft / Submitted / Approved / Rejected. `SubmittedAt` records submission time; use `Status` for current state.
- `SalesEntries` holds a report's individual sales, linked through `VatReportId`.
- The Vue period cards are built from VAT reports. Adding only a reporting period will not create a company-specific card; the company also needs its VAT report.
- `VatReportService` calls `IVatCalculator`. Keep arithmetic and reporting rules in C#.
- `VatRateRepository` currently uses illustrative rates; they are not authoritative tax guidance.
- Enforce company access in backend operations, including any operations later exposed as AI tools.

## First planned AI feature

- User pastes sales/invoice text, AI suggests form values, user reviews and corrects them, and the existing save workflow persists the validated entries.
- Keep manual entry available. Extraction itself should produce a draft without saving or submitting a report.
- Use a separate nullable draft DTO so missing facts do not silently become default enum values or `false`.
- Do not invent missing buyer type, country, date, category, or VAT-number validity. C# validates the reviewed values and calculates VAT.
- Start with text input and structured extraction. PDF/images, embeddings, vector databases, RAG, tools, and guided workflows follow the curriculum.
- Frontend implemented on 2026-09-28: `../frontend/src/components/SalesTextAssistant.vue` previews extraction results and applies them to the existing report form after review. It is hidden for submitted/approved reports.
- `../frontend/src/services/salesExtraction.ts` calls the planned `POST /api/SalesEntries/extract` with `{ text }`. The endpoint/model integration is not implemented yet, and the user's AI provider/setup is still unspecified.
- Read `../frontend/AI-TEXT-ENTRY.md` for the agreed response contract and frontend behavior before implementing the C# endpoint. The example button supplies text only; there is no fake AI fallback.

## Dated local data context

- On 2026-09-28, Acme Corp was company ID 1. Its Q1 and Q2 2025 reports were Submitted.
- A new open period was created for July 1-September 30, 2025: period ID 3, with empty Draft VAT report ID 5 for Acme Corp.
- These are observations of local test data, not constants to hardcode. Query current data before further changes.

## Maintaining context

- Keep this file concise and update it when the user changes durable preferences or project decisions.
- Track learning milestones and the next exercise in the learning path's progress log. Mark dated observations as such and verify them before relying on them.
- This file is in `ReportingApi1`. Sessions started in a sibling frontend directory may need it explicitly referenced because it is not their ancestor guidance file.
