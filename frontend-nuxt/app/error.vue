<script setup lang="ts">
import type { NuxtError } from '#app'

const props = defineProps<{ error: NuxtError }>()

// error.vue replaces the entire app — NuxtLayout and NuxtPage included — so it
// renders its own shell and cannot rely on anything from the default layout.
const isNotFound = computed(() => props.error.statusCode === 404)

const heading = computed(() => (isNotFound.value ? 'Page not found' : 'Something broke'))

const detail = computed(() => {
  if (isNotFound.value) return 'That page does not exist, or it moved.'

  // statusMessage is ours (thrown by a server handler); `message` can carry an
  // internal stack trace, so it is only shown in dev.
  return props.error.statusMessage || 'The page could not be rendered.'
})

useSeoMeta({ title: heading, robots: 'noindex' })

const isDev = import.meta.dev

// clearError resets the error state before navigating; a plain navigateTo would
// leave this component mounted over the new page.
function goHome() {
  return clearError({ redirect: '/' })
}
</script>

<template>
  <div class="shell">
    <p class="code">{{ error.statusCode }}</p>
    <h1>{{ heading }}</h1>
    <p class="detail">{{ detail }}</p>

    <pre v-if="isDev && error.message" class="trace">{{ error.message }}</pre>

    <button @click="goHome">Back to periods</button>
  </div>
</template>

<style scoped>
.shell {
  font-family: system-ui, sans-serif;
  max-width: 32rem;
  margin: 6rem auto;
  padding: 0 1rem;
  text-align: center;
}

.code {
  font-size: 3rem;
  font-weight: 700;
  color: #00dc82;
  margin: 0;
}

h1 {
  margin: 0.25rem 0 0.5rem;
}

.detail {
  color: #666;
}

.trace {
  text-align: left;
  overflow-x: auto;
  background: #f6f6f6;
  border-radius: 0.25rem;
  padding: 0.75rem;
  font-size: 0.8125rem;
}

button {
  font: inherit;
  margin-top: 1rem;
  padding: 0.4rem 1rem;
  border: 1px solid #ccc;
  border-radius: 0.25rem;
  background: #fff;
  cursor: pointer;
}
</style>
