<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { useFetch } from '@vueuse/core'
import { storeToRefs } from 'pinia'
import {
  VAT_REPORTS_ENDPOINT,
  mapVatReportsToOpenPeriods,
  usePeriodsStore,
  type VatReportSalesEntry,
  type VatReport,
} from '../stores/periods'

const SAVE_VAT_REPORT_ENDPOINT = new URL('/api/VatReports/save', VAT_REPORTS_ENDPOINT).toString()
const SUBMIT_VAT_REPORT_ENDPOINT = new URL('/api/VatReports', VAT_REPORTS_ENDPOINT).toString()

interface SaveVatReportRequest {
  companyId: number
  reportingPeriodId: number
  status: number
  salesEntries: Array<{
    country: string
    amount: number
    vatRate: number
  }>
  rowVersion: string
}

const periodsStore = usePeriodsStore()
const { selectedPeriodId } = storeToRefs(periodsStore)

const { data, error, isFetching } = useFetch(VAT_REPORTS_ENDPOINT).json<VatReport[]>()

const periods = computed(() => mapVatReportsToOpenPeriods(data.value ?? []))

const selectedReport = computed(() => {
  if (selectedPeriodId.value === null) {
    return null
  }

  return (
    (data.value ?? []).find((report) => report.reportingPeriodId === selectedPeriodId.value) ?? null
  )
})

const salesEntries = ref<VatReportSalesEntry[]>([])

watch(
  selectedReport,
  (report) => {
    salesEntries.value = report?.salesEntries.map((entry) => ({ ...entry })) ?? []
  },
  { immediate: true },
)

const newCountry = ref('')
const newAmount = ref<number | null>(null)
const newVatRate = ref<number | null>(null)
const formError = ref('')

const countryOptions = [
  { label: 'Austria', value: 'AT' },
  { label: 'Belgium', value: 'BE' },
  { label: 'Bulgaria', value: 'BG' },
  { label: 'Croatia', value: 'HR' },
  { label: 'Cyprus', value: 'CY' },
  { label: 'Czech Republic', value: 'CZ' },
  { label: 'Denmark', value: 'DK' },
  { label: 'Estonia', value: 'EE' },
  { label: 'Finland', value: 'FI' },
  { label: 'France', value: 'FR' },
  { label: 'Germany', value: 'DE' },
  { label: 'Greece', value: 'GR' },
  { label: 'Hungary', value: 'HU' },
  { label: 'Ireland', value: 'IE' },
  { label: 'Italy', value: 'IT' },
  { label: 'Latvia', value: 'LV' },
  { label: 'Lithuania', value: 'LT' },
  { label: 'Luxembourg', value: 'LU' },
  { label: 'Malta', value: 'MT' },
  { label: 'Netherlands', value: 'NL' },
  { label: 'Poland', value: 'PL' },
  { label: 'Portugal', value: 'PT' },
  { label: 'Romania', value: 'RO' },
  { label: 'Slovakia', value: 'SK' },
  { label: 'Slovenia', value: 'SI' },
  { label: 'Spain', value: 'ES' },
  { label: 'Sweden', value: 'SE' },
  { label: 'Iceland', value: 'IS' },
  { label: 'Liechtenstein', value: 'LI' },
  { label: 'Norway', value: 'NO' },
  { label: 'Switzerland', value: 'CH' },
  { label: 'United Kingdom', value: 'GB' },
]

const countryNameToCode = Object.fromEntries(
  countryOptions.map((option) => [option.label, option.value]),
)
const countryValueSet = new Set(countryOptions.map((option) => option.value))

const toCountryCode = (country: string) => {
  const normalizedCountry = country.trim().toUpperCase()

  if (countryValueSet.has(normalizedCountry)) {
    return normalizedCountry
  }

  return countryNameToCode[country.trim()] ?? normalizedCountry
}

const nextSalesEntryId = computed(() => {
  if (salesEntries.value.length === 0) {
    return 1
  }

  return Math.max(...salesEntries.value.map((entry) => entry.id)) + 1
})

const totalSalesAmount = computed(() =>
  salesEntries.value.reduce((sum, entry) => sum + entry.amount, 0),
)

const totalVatAmount = computed(() =>
  salesEntries.value.reduce((sum, entry) => sum + entry.vatAmount, 0),
)

const isReportSubmitted = ref(false)
const isSavingDraft = ref(false)
const isSubmittingReport = ref(false)
const actionMessage = ref('')
const actionMessageType = ref<'info' | 'success'>('info')

const displayedPeriods = computed(() => {
  if (selectedPeriodId.value === null) {
    return []
  }

  return periods.value.filter((period) => period.id === selectedPeriodId.value)
})

const formatDate = (value: string) =>
  new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(new Date(value))

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'EUR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)

const clearForm = () => {
  newCountry.value = ''
  newAmount.value = null
  newVatRate.value = null
}

const onSubmitSalesEntry = () => {
  const country = toCountryCode(newCountry.value)
  const amount = newAmount.value
  const vatRate = newVatRate.value

  if (!country) {
    formError.value = 'Country is required.'
    return
  }

  if (amount === null || !Number.isFinite(amount) || amount <= 0) {
    formError.value = 'Amount must be greater than 0.'
    return
  }

  if (vatRate === null || !Number.isFinite(vatRate) || vatRate < 0) {
    formError.value = 'VAT rate must be 0 or greater.'
    return
  }

  const vatAmount = Number(((amount * vatRate) / 100).toFixed(4))

  salesEntries.value = [
    ...salesEntries.value,
    {
      id: nextSalesEntryId.value,
      country,
      amount,
      vatRate,
      vatAmount,
    },
  ]

  isReportSubmitted.value = false
  formError.value = ''
  actionMessage.value = ''
  clearForm()
}

const onDeleteSalesEntry = (entryId: number) => {
  salesEntries.value = salesEntries.value.filter((entry) => entry.id !== entryId)
  isReportSubmitted.value = false
  actionMessageType.value = 'info'
  actionMessage.value = `Sales entry #${entryId} deleted.`
}

const onSaveDraft = async () => {
  if (selectedPeriodId.value === null || isSavingDraft.value) {
    return
  }

  const payload: SaveVatReportRequest = {
    companyId: selectedReport.value?.companyId ?? 1,
    reportingPeriodId: selectedPeriodId.value,
    status: 0,
    salesEntries: salesEntries.value.map((entry) => ({
      country: toCountryCode(entry.country),
      amount: entry.amount,
      vatRate: entry.vatRate,
    })),
    rowVersion: selectedReport.value?.rowVersion ?? '',
  }

  isSavingDraft.value = true

  try {
    const response = await fetch(SAVE_VAT_REPORT_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      throw new Error(`Save failed with status ${response.status}`)
    }

    const savedReport = (await response.json()) as VatReport
    const currentReports = data.value ?? []
    const reportIndex = currentReports.findIndex((report) => report.id === savedReport.id)

    if (reportIndex >= 0) {
      currentReports[reportIndex] = savedReport
    } else {
      currentReports.push(savedReport)
    }

    data.value = [...currentReports]
    salesEntries.value = savedReport.salesEntries.map((entry) => ({ ...entry }))

    actionMessageType.value = 'success'
    actionMessage.value = `Draft saved with ${salesEntries.value.length} sales entr${
      salesEntries.value.length === 1 ? 'y' : 'ies'
    }.`
  } catch {
    actionMessageType.value = 'info'
    actionMessage.value = 'Could not save draft. Please try again.'
  } finally {
    isSavingDraft.value = false
  }
}

const onSubmitReport = async () => {
  if (salesEntries.value.length === 0 || isReportSubmitted.value || isSubmittingReport.value) {
    return
  }

  const payload: SaveVatReportRequest = {
    companyId: selectedReport.value?.companyId ?? 1,
    reportingPeriodId: selectedPeriodId.value!,
    status: 1,
    salesEntries: salesEntries.value.map((entry) => ({
      country: toCountryCode(entry.country),
      amount: entry.amount,
      vatRate: entry.vatRate,
    })),
    rowVersion: selectedReport.value?.rowVersion ?? '',
  }

  isSubmittingReport.value = true

  try {
    const response = await fetch(SUBMIT_VAT_REPORT_ENDPOINT, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      throw new Error(`Submit failed with status ${response.status}`)
    }

    if (response.status !== 204) {
      const submittedReport = (await response.json()) as VatReport
      const currentReports = data.value ?? []
      const reportIndex = currentReports.findIndex((report) => report.id === submittedReport.id)

      if (reportIndex >= 0) {
        currentReports[reportIndex] = submittedReport
      } else {
        currentReports.push(submittedReport)
      }

      data.value = [...currentReports]
      salesEntries.value = submittedReport.salesEntries.map((entry) => ({ ...entry }))
    }

    isReportSubmitted.value = true
    actionMessageType.value = 'success'
    actionMessage.value = `Report submitted. Total sales ${formatCurrency(totalSalesAmount.value)}, total VAT ${formatCurrency(totalVatAmount.value)}.`
  } catch {
    actionMessageType.value = 'info'
    actionMessage.value = 'Could not submit report. Please try again.'
  } finally {
    isSubmittingReport.value = false
  }
}
</script>

<template>
  <main class="review-page">
    <section class="hero">
      <h1>Period Review</h1>
      <p>Your selected period is highlighted below.</p>
    </section>

    <p v-if="isFetching" class="state">Loading open periods...</p>
    <p v-else-if="error" class="state error">Failed to load periods. Try again later.</p>

    <section v-else-if="displayedPeriods.length > 0" class="cards-grid">
      <article
        v-for="period in displayedPeriods"
        :key="period.id"
        class="period-card"
        :class="{ selected: true }"
      >
        <span class="status" :class="period.status">
          {{ period.status === 'closing_soon' ? 'Closing Soon' : 'Open' }}
        </span>
        <h2>{{ period.name }}</h2>
        <p>{{ formatDate(period.startDate) }} - {{ formatDate(period.endDate) }}</p>
      </article>
    </section>

    <section v-if="selectedPeriodId !== null" class="sales-section">
      <h2>Sales Entries</h2>
      <p class="section-subtitle">Submit sales entries and review existing ones for this period.</p>

      <form class="sales-form" @submit.prevent="onSubmitSalesEntry">
        <label>
          Country
          <select v-model="newCountry" name="country" required>
            <option disabled value="">Select a country</option>
            <option v-for="country in countryOptions" :key="country.value" :value="country.value">
              {{ country.label }} ({{ country.value }})
            </option>
          </select>
        </label>

        <label>
          Amount (EUR)
          <input
            v-model.number="newAmount"
            type="number"
            name="amount"
            min="0.01"
            step="0.01"
            required
          />
        </label>

        <label>
          VAT Rate (%)
          <input
            v-model.number="newVatRate"
            type="number"
            name="vatRate"
            min="0"
            step="0.01"
            required
          />
        </label>

        <button type="submit">Submit Sales Entry</button>
      </form>

      <p v-if="formError" class="state error">{{ formError }}</p>

      <p v-if="salesEntries.length === 0" class="state">No sales entries yet for this period.</p>

      <div v-else class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Country</th>
              <th>Amount</th>
              <th>VAT Rate</th>
              <th>VAT Amount</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="entry in salesEntries" :key="entry.id">
              <td>{{ entry.id }}</td>
              <td>{{ entry.country }}</td>
              <td>{{ formatCurrency(entry.amount) }}</td>
              <td>{{ entry.vatRate.toFixed(2) }}%</td>
              <td>{{ formatCurrency(entry.vatAmount) }}</td>
              <td>
                <button type="button" class="row-delete-btn" @click="onDeleteSalesEntry(entry.id)">
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <section class="report-actions" aria-label="report-actions">
        <div class="actions-copy">
          <h3>Finalize this report</h3>
          <p>
            Save a draft while you are still editing, then submit once all sales entries are ready.
          </p>
        </div>

        <div class="actions-buttons">
          <button
            type="button"
            class="btn secondary"
            :disabled="isSavingDraft"
            @click="onSaveDraft"
          >
            {{ isSavingDraft ? 'Saving Draft...' : 'Save Draft' }}
          </button>
          <button
            type="button"
            class="btn primary"
            :disabled="
              salesEntries.length === 0 || isReportSubmitted || isSavingDraft || isSubmittingReport
            "
            @click="onSubmitReport"
          >
            {{
              isSubmittingReport
                ? 'Submitting...'
                : isReportSubmitted
                  ? 'Report Submitted'
                  : 'Submit Report'
            }}
          </button>
        </div>

        <p v-if="salesEntries.length === 0" class="state">
          Add at least one sales entry to submit.
        </p>
        <p v-if="actionMessage" class="state" :class="{ success: actionMessageType === 'success' }">
          {{ actionMessage }}
        </p>
      </section>
    </section>

    <p v-if="selectedPeriodId === null" class="hint">
      No period selected yet. <RouterLink to="/">Go back and choose one</RouterLink>.
    </p>
  </main>
</template>

<style scoped>
.review-page {
  display: grid;
  gap: 1rem;
}

.hero h1 {
  margin: 0;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}

.hero p {
  margin: 0.35rem 0 0;
  color: #51616f;
}

.state {
  margin: 0;
  color: #3b4b58;
}

.state.error {
  color: #b3292d;
}

.state.success {
  color: #156e52;
}

.cards-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.period-card {
  border: 1px solid #d5dde5;
  border-radius: 14px;
  background: linear-gradient(180deg, #ffffff 0%, #f4f7fa 100%);
  text-align: left;
  padding: 1rem;
}

.period-card.selected {
  border-color: #13795b;
  box-shadow: 0 0 0 2px rgba(19, 121, 91, 0.18);
  background: linear-gradient(180deg, #f8fffb 0%, #edf8f3 100%);
}

.period-card h2 {
  margin: 0.55rem 0 0.35rem;
  font-size: 1.1rem;
  color: #12202b;
}

.period-card p {
  margin: 0;
  color: #4d5b67;
}

.status {
  display: inline-block;
  font-size: 0.75rem;
  padding: 0.2rem 0.55rem;
  border-radius: 999px;
  border: 1px solid transparent;
}

.status.open {
  background: #ebfff7;
  color: #156e52;
  border-color: #c6f1e2;
}

.status.closing_soon {
  background: #fff4eb;
  color: #9a4d0b;
  border-color: #ffd8b7;
}

.hint {
  margin: 0;
  color: #465766;
}

.sales-section {
  margin-top: 0.5rem;
  padding: 1rem;
  border: 1px solid #d5dde5;
  border-radius: 14px;
  background: #ffffff;
}

.sales-section h2 {
  margin: 0;
}

.section-subtitle {
  margin: 0.35rem 0 0.9rem;
  color: #51616f;
}

.sales-form {
  display: grid;
  gap: 0.75rem;
  grid-template-columns: repeat(auto-fit, minmax(190px, 1fr));
  align-items: end;
  margin-bottom: 0.9rem;
}

.sales-form label {
  display: grid;
  gap: 0.35rem;
  color: #203342;
  font-weight: 500;
}

.sales-form input,
.sales-form select {
  border: 1px solid #c7d2dd;
  border-radius: 10px;
  padding: 0.55rem 0.7rem;
  font: inherit;
  background: #ffffff;
}

.sales-form button {
  border: none;
  border-radius: 10px;
  background: #13795b;
  color: #ffffff;
  font: inherit;
  font-weight: 600;
  padding: 0.65rem 0.85rem;
  cursor: pointer;
}

.sales-form button:hover {
  background: #0f654b;
}

.report-actions {
  margin-top: 1rem;
  border-top: 1px solid #e3eaf0;
  padding-top: 1rem;
  display: grid;
  gap: 0.75rem;
}

.actions-copy h3 {
  margin: 0;
  color: #12202b;
}

.actions-copy p {
  margin: 0.35rem 0 0;
  color: #51616f;
}

.actions-buttons {
  display: flex;
  gap: 0.6rem;
  flex-wrap: wrap;
}

.btn {
  border: none;
  border-radius: 10px;
  font: inherit;
  font-weight: 600;
  padding: 0.62rem 0.95rem;
  cursor: pointer;
}

.btn.primary {
  background: #0f766e;
  color: #ffffff;
}

.btn.primary:hover:enabled {
  background: #0c645d;
}

.btn.secondary {
  background: #eef2f6;
  color: #1f3444;
  border: 1px solid #d7e0e8;
}

.btn.secondary:hover {
  background: #e4ebf1;
}

.btn:disabled {
  opacity: 0.58;
  cursor: not-allowed;
}

.table-wrap {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  text-align: left;
  padding: 0.6rem 0.5rem;
  border-bottom: 1px solid #e3eaf0;
  white-space: nowrap;
}

th {
  color: #304452;
  font-weight: 600;
}

.row-delete-btn {
  border: 1px solid #efc4c4;
  border-radius: 8px;
  background: #fff4f4;
  color: #b3292d;
  font: inherit;
  font-size: 0.9rem;
  font-weight: 600;
  padding: 0.35rem 0.55rem;
  cursor: pointer;
}

.row-delete-btn:hover {
  background: #feeaea;
}
</style>
