# Text-to-sales frontend

The regular Vue report review page includes `SalesTextAssistant.vue` for editable reports. It requests suggestions, previews them, and copies them into the existing entry form only after user review. It never creates a sales entry, saves a draft, or submits a report automatically.

## Backend contract to implement next

`POST /api/SalesEntries/extract` through the existing authenticated `apiFetch` helper.

Request:

```json
{ "text": "Sold books to a consumer in Germany on 2025-07-15. Net amount EUR 200." }
```

Successful response (`200`, JSON object; enum values are strings):

```json
{
  "buyerCountry": "DE",
  "amount": 200,
  "currency": "EUR",
  "buyerType": "B2C",
  "productCategory": "Books",
  "saleDate": "2025-07-15"
}
```

All fields may be `null` when unknown. Amount means the amount before VAT. Dates use `YYYY-MM-DD`. Use the existing product-category and buyer-type names. Do not infer VAT-number validity; that remains a manual decision.

The C# endpoint and model integration are not included in this frontend change. A missing endpoint produces a clear unavailable message; the example button only inserts sample text and does not simulate AI extraction.

## Behavior

- Input limit: 10,000 characters. The future backend must enforce its own limit too.
- Requests time out after 30 seconds; users may cancel or edit the text. Late responses from canceled requests are ignored.
- Response shape is checked at runtime before displaying values.
- Unknown fields remain blank. An unknown currency requires the user to confirm EUR; foreign currency cannot be applied because the existing form has no currency conversion.
- Replacing existing form values requires an explicit checkbox; missing suggestions clear old values to prevent accidentally mixing two sales.
- Dates outside the selected period are flagged. The existing entry form enforces the period's date range when adding the sale.
- Submitted and approved reports do not show the assistant.
- All calculations and persistence remain in the existing report workflow.

## Verification

From `frontend`:

```sh
npm run type-check
npm run test:unit:ci
npm run build-only
```

Tests use mocked API responses to exercise review, explicit application, missing values, unsupported currencies, cancellation, unavailable service, and read-only reports. These tests do not verify a real model or the future extraction endpoint.
