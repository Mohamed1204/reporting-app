import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export interface OpenPeriod {
  id: number
  reportId: number
  name: string
  startDate: string
  endDate: string
  reportStatus: number
}

const REPORT_STATUS_LABELS = ['Draft', 'Submitted', 'Approved', 'Rejected']

export function reportStatusLabel(status: number): string {
  return REPORT_STATUS_LABELS[status] ?? 'Unknown'
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

export interface VatReportListItem {
  id: number
  companyId: number
  companyName: string
  reportingPeriodId: number
  startDate: string
  endDate: string
  submittedAt: string
  status: number
  totalAmount: number
  totalVat: number
  rowVersion: string
}

export type BuyerType = 'B2B' | 'B2C'

export type ProductCategory =
  | 'Standard'
  | 'Food'
  | 'Books'
  | 'Medicine'
  | 'FinancialServices'
  | 'Education'

export interface VatBreakdown {
  vatAmount: number
  vatRate: number
  scheme: number
}

export interface VatReportSalesEntry {
  id: number
  buyerCountry: string
  amount: number
  breakdown?: VatBreakdown
  buyerType?: BuyerType
  productCategory?: ProductCategory
  buyerHasValidVatNumber?: boolean
  saleDate?: string
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

export const mapVatReportsToOpenPeriods = (
  reports: Array<VatReport | VatReportListItem>,
): OpenPeriod[] =>
  reports.map((report) => ({
    id: report.reportingPeriodId,
    reportId: report.id,
    name: `Period ${report.reportingPeriodId}`,
    startDate: report.startDate,
    endDate: report.endDate,
    reportStatus: report.status,
  }))

export const usePeriodsStore = defineStore('periods', () => {
  const selectedPeriodId = ref<number | null>(null)
  const selectedReportId = ref<number | null>(null)

  const hasSelectedPeriod = computed(() => selectedPeriodId.value !== null)

  const selectPeriod = (periodId: number, reportId: number) => {
    selectedPeriodId.value = periodId
    selectedReportId.value = reportId
  }

  const clearSelection = () => {
    selectedPeriodId.value = null
    selectedReportId.value = null
  }

  return {
    selectedPeriodId,
    selectedReportId,
    hasSelectedPeriod,
    selectPeriod,
    clearSelection,
  }
})
