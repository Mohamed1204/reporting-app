<script setup lang="ts">
// No longer `public: true`: /api/periods needs a token since 4.5, so an
// anonymous visit would 401 the whole page.

// 1 — useFetch: SSR-aware, key derived from the URL
const { data: a } = await useFetch("/api/periods");

// 2 — useAsyncData: SSR-aware, key is yours to invent
const { data: b } = await useAsyncData("periods-manual", () =>
  $fetch("/api/periods")
);

// 3 — $fetch in onMounted: not SSR-aware
const c = ref<{ id: number }[] | null>(null);

// 4 — the 4.3 trap. `$fetch` at the top of setup runs on the server during SSR,
// where the global $fetch carries no cookies, so this 401s on a hard refresh
// while working fine on a client-side navigation. useRequestFetch() returns the
// event-bound $fetch that forwards them — which is what useFetch uses for a
// relative URL, and why (1) never had the problem.
const request = useRequestFetch();
const d = await request("/api/periods");

onMounted(async () => {
  c.value = await $fetch("/api/periods");
});
</script>

<template>
  <h1>lab</h1>
  <ul>
    <li>useFetch: {{ a?.length ?? "null" }}</li>
    <li>useAsyncData: {{ b?.length ?? "null" }}</li>
    <li>$fetch in onMounted: {{ c?.length ?? "null" }}</li>
    <li>useRequestFetch in setup: {{ d?.length ?? "null" }}</li>
  </ul>
</template>
