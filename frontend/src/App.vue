<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app-shell">
    <nav>
      <RouterLink to="/">Open Periods</RouterLink>
      <RouterLink to="/about">About</RouterLink>
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
</style>
