# Reporting App — User Guide

A web application for companies to report their VAT (Value Added Tax) obligations. Companies submit their sales data per reporting period, and the system produces VAT reports broken down by country.

---

## Who uses this app

Two kinds of users log into the platform:

| Role | Typical person | What they do |
|------|----------------|--------------|
| **User** | An employee of a company that owes VAT | Fills in their company's VAT reports and submits them |
| **Admin** | Platform operator / tax authority staff | Sets up report slots for companies, reviews submissions, manages data across all companies |

Every user belongs to exactly one company. Admins aren't tied to a single company — they can act across all tenants.

---

## Core concepts

A few things you'll see throughout the app:

- **Company** — an organization registered in the system (e.g. "Acme Denmark ApS"). Every user belongs to one.
- **Reporting Period** — a fixed time window that companies must report on. Typically quarterly (e.g. *Q1 2026: Jan 1 – Mar 31*).
- **Sales Entry** — one line of a company's sales data inside a report. Each entry records:
  - **Country** — where the sale happened (determines which country's VAT rules apply)
  - **Amount** — the sale value
  - **VAT rate** — the applicable percentage (e.g. 25%)
  - (The app computes the VAT amount automatically from Amount × Rate.)
- **VAT Report** — a package of sales entries for one company and one reporting period. This is the main artifact the app produces.
- **Report Status** — the report moves through a few states as it's worked on and submitted. See the lifecycle below.

---

## How a VAT report flows through the system

Every report moves through this lifecycle:

```
   [Draft] ──submit──▶ [Submitted] ──approve──▶ [Approved]
                            │
                        reject
                            ▼
                       [Rejected] ──resubmit──▶ [Submitted]
```

1. **Draft** — The report has been created but isn't final yet. Company users can keep editing the sales entries.
2. **Submitted** — The company has locked in their data and handed it over. The submission timestamp is recorded.
3. **Approved** — The submission has been accepted. No more changes.
4. **Rejected** — Something was wrong. The report goes back to a drafty state, and the company can fix it and resubmit.

**Rule:** Only reports in *Draft* or *Rejected* state can be submitted. Already-submitted or approved reports cannot be re-submitted without being rejected first.

---

## What company users can do

When you log in as a regular user, you can only work with your own company's reports.

### View your reports
- See a list of all VAT reports belonging to your company.
- Filter by status (e.g. show me only the ones still in Draft).
- Open a specific report to see its sales entries and totals.

### Fill in a draft report (**Save**)
- Open an existing Draft report and fill in your sales entries (country, amount, VAT rate per line).
- Save your work as often as you want — the report stays in Draft until you choose to submit.
- You can't save changes to a report that's already Submitted.

### Submit the report (**Submit**)
- Once your sales entries are final, submit the report.
- The report's status flips to Submitted and the submission time is recorded.
- After this, you can't edit it anymore unless an admin rejects it.

### Register and log in
- Create an account (must be linked to an existing company).
- Log in with username + password — you get a token that authenticates you for everything else.

### What company users **cannot** do
- Create new report slots from scratch — admins set those up.
- Delete reports.
- See other companies' data — ever.

---

## What admins can do

Admins have everything company users can do, plus:

### Provision reports
- **Create** a new VAT report slot for any company for any reporting period. This is the "opening" step — the company's users can then fill the report in.

### Manage any report
- **Update** the structure of any report (for any company) — typically for corrections or cleanup.
- **Delete** any report if it was created in error.

### View across tenants
- See VAT reports **across all companies**, not just one — useful for auditing and oversight.
- Optionally filter by a specific company when they want to narrow down.

### Act on behalf of any company
- Admins can also Save and Submit reports for any company — not just their own. Useful when an admin is helping a company that's struggling with the UI, or filing retroactively.

### Manage reporting periods
- Create new reporting periods (e.g. opening "Q2 2026" when the quarter arrives).
- Delete reporting periods if they're no longer needed.

---

## Supporting resources

The app also exposes some shared reference data:

- **Products** — the catalog of items that can be sold. Currently read-only in the API (seeded from the database).
- **Reporting periods** — browsable by all authenticated users so they know which period a report belongs to.

---

## Typical user journey

**A company filing their Q1 2026 VAT report:**

1. An admin has already created a Draft report for *Company Acme × Q1 2026*.
2. Alice (an Acme employee) logs in.
3. She lands on her company's report list and opens the Q1 2026 report.
4. She fills in sales entries — one per country where Acme did business in Q1.
5. She saves her progress a few times as she double-checks numbers.
6. Once she's confident the numbers are right, she submits the report.
7. The report flips to Submitted. Alice can still view it but can no longer edit.
8. An admin reviews it. They either approve it (done) or reject it (Alice fixes and resubmits).

---

## What this app is not (yet)

Worth being explicit about the edges:

- No email notifications — users don't get pinged when a report is rejected or approved.
- No file uploads — sales data has to be entered line-by-line; there's no CSV import.
- No in-app reporting deadlines / reminders — companies are expected to know their own deadlines.
- No multi-user collaboration indicators — two users from the same company saving the same report at the same time may overwrite each other's work (though the app does detect version conflicts on submit).
