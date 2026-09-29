import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount, type VueWrapper } from '@vue/test-utils'
import { ref } from 'vue'
import { createPinia, setActivePinia } from 'pinia'
import SalesTextAssistant from '../SalesTextAssistant.vue'
import PeriodReviewView from '../../views/PeriodReviewView.vue'
import { usePeriodsStore } from '../../stores/periods'

const api = vi.hoisted(() => ({ apiFetch: vi.fn(), useApiFetch: vi.fn() }))
vi.mock('../../composables/useApiFetch', () => api)

const completeDraft = {
  buyerCountry: 'DE',
  amount: 200,
  currency: 'EUR',
  buyerType: 'B2C',
  productCategory: 'Books',
  saleDate: '2025-07-15',
}
const props = {
  startDate: '2025-07-01',
  endDate: '2025-09-30',
  hasFormValues: false,
  countryOptions: [{ label: 'Germany', value: 'DE' }],
}
const wrappers: VueWrapper[] = []
function createPanel(overrides = {}) {
  const wrapper = mount(SalesTextAssistant, { props: { ...props, ...overrides } })
  wrappers.push(wrapper)
  return wrapper
}
type TestSurface = Pick<VueWrapper, 'get' | 'findAll'>
function button(wrapper: TestSurface, text: string) {
  const match = wrapper.findAll('button').find((b) => b.text() === text)
  if (!match) throw new Error(`Button not found: ${text}`)
  return match
}
async function requestSuggestions(wrapper: TestSurface, data: unknown = completeDraft) {
  api.apiFetch.mockResolvedValueOnce(new Response(JSON.stringify(data), { status: 200 }))
  await wrapper.get('textarea').setValue('Sold books in Germany for EUR 200 on 2025-07-15.')
  await wrapper.get('form').trigger('submit')
  await flushPromises()
}

beforeEach(() => {
  vi.resetAllMocks()
  setActivePinia(createPinia())
})
afterEach(() => {
  wrappers.splice(0).forEach((wrapper) => wrapper.unmount())
})

describe('text-to-sales review', () => {
  it('does not request suggestions for blank input', async () => {
    const wrapper = createPanel()
    await wrapper.get('textarea').setValue('   ')
    await wrapper.get('form').trigger('submit')
    expect(api.apiFetch).not.toHaveBeenCalled()
  })

  it('previews results and only applies them after an explicit action', async () => {
    const wrapper = createPanel()
    await requestSuggestions(wrapper)
    expect(wrapper.text()).toContain('Review suggestions')
    expect(wrapper.text()).toContain('Germany')
    expect(wrapper.emitted('apply')).toBeUndefined()
    expect(api.apiFetch).toHaveBeenCalledWith(
      '/api/SalesEntries/extract',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ text: 'Sold books in Germany for EUR 200 on 2025-07-15.' }),
        signal: expect.any(AbortSignal),
      }),
    )
    await button(wrapper, 'Use suggestions').trigger('click')
    expect(wrapper.emitted('apply')?.[0]).toEqual([completeDraft])
    expect(api.apiFetch).toHaveBeenCalledTimes(1)
  })

  it('requires a currency confirmation when EUR is not established', async () => {
    const wrapper = createPanel()
    await requestSuggestions(wrapper, { amount: 200 })
    expect(wrapper.text()).toContain('Not found')
    expect(button(wrapper, 'Use suggestions').attributes('disabled')).toBeDefined()
    await wrapper.get('input[name="confirmEuro"]').setValue(true)
    await button(wrapper, 'Use suggestions').trigger('click')
    expect(wrapper.emitted('apply')?.[0]?.[0]).toMatchObject({
      amount: 200,
      currency: 'EUR',
      buyerType: null,
      saleDate: null,
    })
  })

  it('blocks applying a foreign currency instead of silently treating it as EUR', async () => {
    const wrapper = createPanel()
    await requestSuggestions(wrapper, { ...completeDraft, currency: 'USD' })
    expect(wrapper.text()).toContain('accepts EUR only')
    expect(button(wrapper, 'Use suggestions').attributes('disabled')).toBeDefined()
    expect(wrapper.emitted('apply')).toBeUndefined()
  })

  it('warns about dates outside the selected period', async () => {
    const wrapper = createPanel()
    await requestSuggestions(wrapper, { ...completeDraft, saleDate: '2026-07-15' })
    expect(wrapper.text()).toContain('outside this reporting period')
  })

  it('keeps source text and explains when the backend is not implemented', async () => {
    const wrapper = createPanel()
    api.apiFetch.mockResolvedValueOnce(new Response('', { status: 404 }))
    await wrapper.get('textarea').setValue('My invoice text')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(wrapper.get('[role="alert"]').text()).toContain('not available yet')
    expect((wrapper.get('textarea').element as HTMLTextAreaElement).value).toBe('My invoice text')
    expect(wrapper.emitted('apply')).toBeUndefined()
  })

  it('cancels an old request when text changes and ignores its late response', async () => {
    const wrapper = createPanel()
    let resolve!: (value: Response) => void
    api.apiFetch.mockImplementationOnce(
      () =>
        new Promise<Response>((done) => {
          resolve = done
        }),
    )
    await wrapper.get('textarea').setValue('Original text')
    await wrapper.get('form').trigger('submit')
    const signal = api.apiFetch.mock.calls[0]?.[1].signal as AbortSignal
    await wrapper.get('textarea').setValue('Corrected text')
    expect(signal.aborted).toBe(true)
    resolve(new Response(JSON.stringify(completeDraft)))
    await flushPromises()
    expect(wrapper.text()).not.toContain('Review suggestions')
    expect(wrapper.emitted('apply')).toBeUndefined()
  })
})

function createReview(status = 0) {
  usePeriodsStore().selectPeriod(3, 5)
  api.useApiFetch.mockReturnValue({
    data: ref({
      id: 5,
      companyId: 1,
      reportingPeriodId: 3,
      startDate: '2025-07-01',
      endDate: '2025-09-30',
      status,
      salesEntries: [],
      rowVersion: 'version',
    }),
    error: ref(null),
    isFetching: ref(false),
  })
  const wrapper = mount(PeriodReviewView, { global: { stubs: { RouterLink: true } } })
  wrappers.push(wrapper)
  return wrapper
}

describe('report form integration', () => {
  it('asks before replacing manual values, clears unknown fields, and does not add or save a sale', async () => {
    const wrapper = createReview()
    await wrapper.get('select[name="country"]').setValue('FR')
    await wrapper.get('select[name="buyerType"]').setValue('B2C')
    await wrapper.get('input[name="saleDate"]').setValue('2025-07-10')
    const assistant = wrapper.getComponent(SalesTextAssistant)
    await requestSuggestions(assistant, { buyerCountry: 'DE', amount: 200, currency: 'EUR' })
    expect(button(assistant, 'Use suggestions').attributes('disabled')).toBeDefined()
    await assistant.get('input[name="confirmReplace"]').setValue(true)
    await button(assistant, 'Use suggestions').trigger('click')
    expect((wrapper.get('select[name="country"]').element as HTMLSelectElement).value).toBe('DE')
    expect((wrapper.get('input[name="amount"]').element as HTMLInputElement).value).toBe('200')
    expect((wrapper.get('select[name="buyerType"]').element as HTMLSelectElement).value).toBe('')
    expect((wrapper.get('input[name="saleDate"]').element as HTMLInputElement).value).toBe('')
    expect(wrapper.text()).toContain('No sales entries yet')
    expect(api.apiFetch).toHaveBeenCalledTimes(1)
  })

  it.each([1, 2])('hides extraction for a locked report with status %i', (status) => {
    expect(createReview(status).findComponent(SalesTextAssistant).exists()).toBe(false)
  })
})
