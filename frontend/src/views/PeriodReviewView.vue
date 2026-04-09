<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useFetch } from '@vueuse/core'
import { storeToRefs } from 'pinia'
import {
  VAT_REPORTS_ENDPOINT,
  mapVatReportsToOpenPeriods,
  usePeriodsStore,
  type VatReport,
} from '../stores/periods'

const periodsStore = usePeriodsStore()
const { selectedPeriodId } = storeToRefs(periodsStore)

const { data, error, isFetching } = useFetch(VAT_REPORTS_ENDPOINT).json<VatReport[]>()

const periods = computed(() => mapVatReportsToOpenPeriods(data.value ?? []))

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
</style>
