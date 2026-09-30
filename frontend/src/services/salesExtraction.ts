import { apiFetch } from '../composables/useApiFetch'
import type { BuyerType, ProductCategory } from '../stores/periods'

export const MAX_EXTRACTION_TEXT_LENGTH = 10_000

export interface SalesEntryDraft {
  buyerCountry: string | null
  amount: number | null
  currency: string | null
  buyerType: BuyerType | null
  productCategory: ProductCategory | null
  saleDate: string | null
}

const categories: ProductCategory[] = [
  'Standard',
  'Food',
  'Books',
  'Medicine',
  'FinancialServices',
  'Education',
]

// Treat API output as untrusted data, even when the model uses a JSON schema.
export function parseSalesEntryDraft(value: unknown): SalesEntryDraft {
  const invalid = () =>
    new Error('The suggestions could not be read. Please try again or enter the sale manually.')
  if (!value || typeof value !== 'object' || Array.isArray(value)) throw invalid()
  const fields = value as Record<string, unknown>
  function optionalText(key: string): string | null {
    const field = fields[key]
    if (field === undefined || field === null || field === '') return null
    if (typeof field !== 'string') throw invalid()
    return field.trim() || null
  }

  const buyerCountry = optionalText('buyerCountry')?.toUpperCase() ?? null
  const currency = optionalText('currency')?.toUpperCase() ?? null
  const buyerType = optionalText('buyerType')
  const productCategory = optionalText('productCategory')
  const saleDate = optionalText('saleDate')
  const amount = fields.amount ?? null

  if (buyerCountry && !/^[A-Z]{2}$/.test(buyerCountry)) throw invalid()
  if (currency && !/^[A-Z]{3}$/.test(currency)) throw invalid()
  if (buyerType !== null && buyerType !== 'B2B' && buyerType !== 'B2C') throw invalid()
  if (productCategory !== null && !categories.includes(productCategory as ProductCategory))
    throw invalid()
  if (
    amount !== null &&
    (typeof amount !== 'number' ||
      !Number.isFinite(amount) ||
      amount <= 0 ||
      amount > 9999999999.99)
  )
    throw invalid()
  if (saleDate) {
    const date = new Date(`${saleDate}T00:00:00Z`)
    if (
      !/^\d{4}-\d{2}-\d{2}$/.test(saleDate) ||
      Number.isNaN(date.getTime()) ||
      date.toISOString().slice(0, 10) !== saleDate
    )
      throw invalid()
  }
  if (
    [buyerCountry, amount, buyerType, productCategory, saleDate].every((field) => field === null)
  ) {
    throw new Error(
      'No sales details were found. Add more information or fill in the form manually.',
    )
  }

  return {
    buyerCountry,
    amount: amount as number | null,
    currency,
    buyerType,
    productCategory: productCategory as ProductCategory | null,
    saleDate,
  }
}

export async function extractSalesEntry(
  text: string,
  signal: AbortSignal,
): Promise<SalesEntryDraft> {
  const response = await apiFetch('/api/SalesEntries/extract', {
    method: 'POST',
    body: JSON.stringify({ text }),
    signal,
  })
  if (!response.ok) {
    const messages: Record<number, string> = {
      400: 'This text could not be processed. Check the details and try again.',
      401: 'Your session has expired. Sign in again to continue.',
      403: 'You do not have access to this feature.',
      404: 'AI suggestions are not available yet. You can still fill in the form manually.',
      405: 'AI suggestions are not available yet. You can still fill in the form manually.',
      413: 'There is too much text. Try one sale at a time.',
      429: 'Too many requests. Wait a moment and try again.',
      501: 'AI suggestions are not available yet. You can still fill in the form manually.',
    }
    throw new Error(
      messages[response.status] ??
        'The suggestion service is unavailable. Try again later or enter the sale manually.',
    )
  }
  let data: unknown
  try {
    data = await response.json()
  } catch {
    throw new Error(
      'The suggestions could not be read. Please try again or enter the sale manually.',
    )
  }
  return parseSalesEntryDraft(data)
}
