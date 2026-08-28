<script setup lang="ts">
const { data: periods, error } = await useFetch("/api/periods");

const statusLabels = ["Open", "Closed", "Locked"];
</script>

<template>
  <h1>Reporting periods</h1>

  <p v-if="error" class="error">Could not load periods: {{ error.message }}</p>

  <ul v-else>
    <li v-for="p in periods ?? []" :key="p.id">
      {{ p.startDate.slice(0, 10) }} → {{ p.endDate.slice(0, 10) }}
      <span class="status">{{ statusLabels[p.status] }}</span>
    </li>
  </ul>
</template>

<style scoped>
.error {
  color: #b00;
}

.status {
  color: #666;
  font-size: 0.875rem;
}
</style>
