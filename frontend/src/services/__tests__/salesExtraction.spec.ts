import { describe, expect, it, vi } from 'vitest'
vi.mock('../../composables/useApiFetch', () => ({ apiFetch: vi.fn() }))
import { parseSalesEntryDraft } from '../salesExtraction'

describe('sales extraction response validation', () => {
  it('preserves unknown values instead of inventing form defaults', () => {
    expect(parseSalesEntryDraft({ buyerCountry: ' de ', amount: 200 })).toEqual({
      buyerCountry: 'DE',
      amount: 200,
      currency: null,
      buyerType: null,
      productCategory: null,
      saleDate: null,
    })
  })

  it.each([
    null,
    [],
    {},
    { amount: '200' },
    { amount: -2 },
    { amount: Infinity },
    { buyerType: 'Individual' },
    { productCategory: 'Unknown' },
    { saleDate: '2025-02-30' },
    { saleDate: '15/07/2025' },
    { buyerCountry: '<script>' },
  ])('rejects unusable model output: %j', (value) => {
    expect(() => parseSalesEntryDraft(value)).toThrow()
  })
})
