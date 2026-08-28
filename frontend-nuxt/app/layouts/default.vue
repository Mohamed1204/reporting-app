<template>
  <div class="shell">
    <nav>
      <NuxtLink to="/">Home</NuxtLink>
      <NuxtLink to="/auth">Auth</NuxtLink>
      <NuxtLink to="/periods/review">Review</NuxtLink>
      <NuxtLink to="/periods/2">Period 2</NuxtLink>
      <NuxtLink to="/periods/3">Period 3</NuxtLink>
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

main {
  padding-top: 1rem;
}
</style>
