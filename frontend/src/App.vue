<script setup lang="ts">
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'

const router = useRouter()
const authStore = useAuthStore()

async function handleLogout() {
  await authStore.logout()
  router.push('/auth')
}
</script>

<template>
  <div class="app-shell">
    <nav>
      <RouterLink to="/">Open Periods</RouterLink>
      <span v-if="authStore.isLoggedIn" class="user-info">
        {{ authStore.companyName || '—' }}
        <span class="role-badge" :class="{ admin: authStore.role === 'Admin' }">
          {{ authStore.role }}
        </span>
        <span class="user-name">({{ authStore.userName }})</span>
      </span>
      <button v-if="authStore.isLoggedIn" @click="handleLogout">Logout</button>
    </nav>
    <RouterView />
  </div>
</template>

<style scoped>
.app-shell {
  display: grid;
  gap: 1.5rem;
}

nav {
  display: flex;
  gap: 0.6rem;
  flex-wrap: wrap;
  align-items: center;
}

nav a {
  padding: 0.45rem 0.8rem;
  border-radius: 999px;
  border: 1px solid #d7dfe6;
  color: #204057;
}

nav a.router-link-exact-active {
  color: #0b4b36;
  border-color: #13795b;
  background: #edfbf5;
}

.user-info {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  color: #204057;
}

.role-badge {
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  background: #eef2f6;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #41607a;
}

.role-badge.admin {
  background: #ffe6e0;
  color: #a93023;
}

.user-name {
  font-size: 0.85rem;
  color: #6b7d90;
}
</style>
