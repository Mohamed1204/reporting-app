<script setup lang="ts">
import { computed } from 'vue'
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiFetch } from '../composables/useApiFetch'
import { storeToRefs } from 'pinia'
import {
  VAT_REPORTS_ENDPOINT,
  mapVatReportsToOpenPeriods,
  usePeriodsStore,
  type VatReport,
  type PagedResult,
} from '../stores/periods'

const router = useRouter()
const periodsStore = usePeriodsStore()
const { selectedPeriodId } = storeToRefs(periodsStore)

onMounted(() => {
  periodsStore.clearSelection()
})

const { data, error, isFetching } = useApiFetch<PagedResult<VatReport>>(VAT_REPORTS_ENDPOINT)

const periods = computed(() => mapVatReportsToOpenPeriods(data.value?.items ?? []))

const selectedPeriod = computed(() =>
  periods.value.find((period) => period.id === selectedPeriodId.value),
)

const formatDate = (value: string) =>
  new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(new Date(value))

const onSelectPeriod = async (id: number) => {
  periodsStore.selectPeriod(id)
  await router.push({ name: 'period-review' })
}
</script>

<template>
  <main class="periods-page">
    <section class="hero">
      <h1>Open Periods</h1>
      <p>Select a period to continue.</p>
    </section>

    <p v-if="isFetching" class="state">Loading open periods...</p>
    <p v-else-if="error" class="state error">Failed to load periods. Try again later.</p>

    <section v-else class="cards-grid">
      <button
        v-for="period in periods"
        :key="period.id"
        class="period-card"
        :class="{ selected: selectedPeriodId === period.id }"
        type="button"
        @click="onSelectPeriod(period.id)"
      >
        <span class="status" :class="period.status">
          {{ period.status === 'closing_soon' ? 'Closing Soon' : 'Open' }}
        </span>
        <h2>{{ period.name }}</h2>
        <p>{{ formatDate(period.startDate) }} - {{ formatDate(period.endDate) }}</p>
      </button>
    </section>

    <p v-if="selectedPeriod" class="picked">
      Selected period: <strong>{{ selectedPeriod.name }}</strong>
    </p>
  </main>
</template>

<style scoped>
.periods-page {
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
  cursor: pointer;
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease,
    border-color 0.2s ease;
}

.period-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 22px rgba(19, 43, 63, 0.08);
}

.period-card.selected {
  border-color: #13795b;
  box-shadow: 0 0 0 2px rgba(19, 121, 91, 0.18);
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

.picked {
  margin: 0;
  font-size: 0.98rem;
  color: #102e22;
}
</style>
