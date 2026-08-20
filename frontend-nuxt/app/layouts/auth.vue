<template>
  <div class="auth-shell">
    <div class="card">
      <slot />
      <NuxtLink to="/" class="back">Back to home</NuxtLink>
    </div>
  </div>
</template>

<style scoped>
.auth-shell {
  font-family: system-ui, sans-serif;
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #f6f6f6;
}

.card {
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 0.5rem;
  padding: 2rem;
  min-width: 20rem;
}

.back {
  display: inline-block;
  margin-top: 1.5rem;
  font-size: 0.875rem;
  color: #666;
}
</style>
