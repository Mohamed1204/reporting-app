<script setup lang="ts">
const session = useCookie<SessionUser | null>('session')
const isLoggingOut = ref(false)

async function logout() {
  isLoggingOut.value = true
  try {
    await $fetch('/api/auth/logout', { method: 'POST' })
  } finally {
    // The handler already sent the expiring Set-Cookie headers; this re-reads
    // them so `session` here — and in the route middleware — goes null.
    refreshCookie('session')
    isLoggingOut.value = false
    await navigateTo('/auth')
  }
}
</script>

<template>
  <div class="shell">
    <nav>
      <NuxtLink to="/">Home</NuxtLink>
      <NuxtLink to="/periods/review">Review</NuxtLink>
      <NuxtLink to="/periods/1">Period 1</NuxtLink>
      <NuxtLink to="/periods/2">Period 2</NuxtLink>

      <span class="spacer" />

      <template v-if="session">
        <span class="who">{{ session.userName }} · {{ session.companyName }}</span>
        <button :disabled="isLoggingOut" @click="logout">Log out</button>
      </template>
      <NuxtLink v-else to="/auth">Log in</NuxtLink>
    </nav>

    <main>
      <slot />
    </main>
  </div>
</template>

<style scoped>
.shell {
  font-family: system-ui, sans-serif;
  max-width: 60rem;
  margin: 0 auto;
  padding: 1rem;
}

nav {
  display: flex;
  gap: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid #ddd;
}

nav a {
  text-decoration: none;
  color: #444;
}

nav a.router-link-active {
  font-weight: 700;
  color: #00dc82;
}

.spacer {
  flex: 1;
}

.who {
  color: #666;
  font-size: 0.875rem;
}

nav button {
  font: inherit;
  font-size: 0.875rem;
  padding: 0.15rem 0.6rem;
  border: 1px solid #ccc;
  border-radius: 0.25rem;
  background: #fff;
  cursor: pointer;
}

nav button:disabled {
  opacity: 0.5;
  cursor: default;
}

main {
  padding-top: 1rem;
}
</style>
