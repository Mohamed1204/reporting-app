<script setup lang="ts">
const route = useRoute();
console.log("setup ran for id:", route.params.id);
// Tzolis   K.H   Madueke

//        Øde
//   MLS      Bruno.G

// Cala  Maghales  Mosq   White


//strongest
// Alvares    K.H        Saka
// Tzolis     Gyok       Madueke
//           Jesus      Downman

//           Øde
//           Eze

//       Buno.G   Rice
//       MLS      Zubi
//       Merino     
//
//Cala   Maghl Saliba   Timber
//Hincap Mosq  Konsa    White 
const { data: period, error } = await useFetch(
  () => `/api/periods/${route.params.id}`
);

const statusLabels = ["Open", "Closed", "Locked"];

// query as view state: derived, never fetched
const tab = computed(() => route.query.tab ?? "sales");
</script>

<template>
  <h1>Period {{ route.params.id }}</h1>

  <p v-if="error" class="error">
    {{ error.statusCode }} — {{ error.statusMessage }}
  </p>

  <dl v-else-if="period">
    <dt>Start</dt>
    <dd>{{ period.startDate.slice(0, 10) }}</dd>
    <dt>End</dt>
    <dd>{{ period.endDate.slice(0, 10) }}</dd>
    <dt>Status</dt>
    <dd>{{ statusLabels[period.status] }}</dd>
  </dl>

  <nav class="tabs">
    <NuxtLink :to="`/periods/${route.params.id}?tab=sales`">Sales</NuxtLink>
    <NuxtLink :to="`/periods/${route.params.id}?tab=vat`">VAT</NuxtLink>
  </nav>

  <p class="tab">active tab: {{ tab }}</p>
</template>

<style scoped>
.error {
  color: #b00;
}

dl {
  display: grid;
  grid-template-columns: 5rem 1fr;
  gap: 0.25rem 1rem;
  margin: 1rem 0;
}

dt {
  color: #666;
}

.tabs {
  display: flex;
  gap: 1rem;
}

.tab {
  color: #666;
  font-size: 0.875rem;
}
</style>
