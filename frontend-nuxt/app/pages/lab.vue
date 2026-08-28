<script setup lang="ts">
// 1 — useFetch: SSR-aware, key derived from the URL
const { data: a } = await useFetch("/api/periods");

// 2 — useAsyncData: SSR-aware, key is yours to invent
const { data: b } = await useAsyncData("periods-manual", () =>
  $fetch("/api/periods")
);

// 3 — $fetch in onMounted: not SSR-aware
const c = ref<{ id: number }[] | null>(null);

const d = await $fetch('/api/periods')
onMounted(async () => {
  c.value = await $fetch("/api/periods");
});
</script>

<template>
  <h1>lab</h1>
  <ul>
    <li>useFetch: {{ a?.length ?? "null" }}</li>
    <li>useAsyncData: {{ b?.length ?? "null" }}</li>
    <li>$fetch in onMounted: {{ d?.length ?? "null" }}</li>
  </ul>
</template>
