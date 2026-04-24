import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export interface OpenPeriod {
  id: number
  name: string
  startDate: string
  endDate: string
  status: 'open' | 'closing_soon'
}

export interface VatReport {
  id: number
  companyId: number
  companyName: string
  reportingPeriodId: number
  startDate: string
  endDate: string
  submittedAt: string
  status: number
  salesEntries: VatReportSalesEntry[]
  totalAmount: number
  totalVat: number
  rowVersion: string
}

export interface VatReportSalesEntry {
  id: number
  country: string
  amount: number
  vatRate: number
  vatAmount: number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export const API_BASE = 'https://localhost:7033'

export type VatReportSortField = 'submittedAt' | 'companyName' | 'status' | 'startDate'
export type SortDirection = 'asc' | 'desc'

export interface VatReportsQuery {
  companyId?: number
  page?: number
  pageSize?: number
  sortBy?: VatReportSortField
  sortDir?: SortDirection
}

export const buildVatReportsEndpoint = (q: VatReportsQuery = {}): string => {
  const params = new URLSearchParams()
  if (q.companyId !== undefined) params.set('companyId', String(q.companyId))
  params.set('page', String(q.page ?? 1))
  params.set('pageSize', String(q.pageSize ?? 50))
  if (q.sortBy) params.set('sortBy', q.sortBy)
  if (q.sortDir) params.set('sortDir', q.sortDir)
  return `${API_BASE}/api/VatReports?${params.toString()}`
}

export const VAT_REPORTS_ENDPOINT = buildVatReportsEndpoint({
  companyId: 1,
  sortBy: 'startDate',
  sortDir: 'asc',
})

const mapVatReportStatusToPeriodStatus = (status: number): OpenPeriod['status'] => {
  // Treat draft/in-progress as open; all other states as non-open in this UI.
  return status === 0 ? 'open' : 'closing_soon'
}

export const mapVatReportsToOpenPeriods = (reports: VatReport[]): OpenPeriod[] =>
  reports.map((report) => ({
    id: report.reportingPeriodId,
    name: `Period ${report.reportingPeriodId}`,
    startDate: report.startDate,
    endDate: report.endDate,
    status: mapVatReportStatusToPeriodStatus(report.status),
  }))

export const usePeriodsStore = defineStore('periods', () => {
  const selectedPeriodId = ref<number | null>(null)

  const hasSelectedPeriod = computed(() => selectedPeriodId.value !== null)

  const selectPeriod = (id: number) => {
    selectedPeriodId.value = id
  }

  const clearSelection = () => {
    selectedPeriodId.value = null
  }

  return {
    selectedPeriodId,
    hasSelectedPeriod,
    selectPeriod,
    clearSelection,
  }
})
