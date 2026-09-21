<script setup lang="ts">
// Function form rather than the '%s · OSS Reporting' string, which renders a
// dangling separator on any page that sets no title of its own.
useHead({
  titleTemplate: (title) => (title ? `${title} · OSS Reporting` : 'OSS Reporting')
})
</script>

<template>
  <div>
    <NuxtRouteAnnouncer />
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>
