export interface ReportingPeriod {
  id: number
  startDate: string
  endDate: string
  status: number
}

export const periods: ReportingPeriod[] = [
  { id: 1, startDate: '2025-10-01T00:00:00', endDate: '2025-12-31T00:00:00', status: 1 },
  { id: 2, startDate: '2026-01-01T00:00:00', endDate: '2026-03-31T00:00:00', status: 0 },
  { id: 3, startDate: '2026-04-01T00:00:00', endDate: '2026-06-30T00:00:00', status: 0 }
]
