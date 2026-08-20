<script setup lang="ts">
const route = useRoute();
console.log("setup ran for id:", route.params.id);
// Tzolis   K.H   Madueke

//        Øde
//   MLS      Bruno.G

// Cala  Maghales  Mosq   White


//strongest
// Eze      K.H       Saka
//
//         Øde
//     Buno.G  Rice
//
// Cala  Maghl Saliba  Timber        
</script>

<template>
  <h1>Period {{ route.params.id }}</h1>
  <NuxtLink to="/periods/5?tab=sales">Sales</NuxtLink>
<NuxtLink to="/periods/5?tab=vat">VAT</NuxtLink>
</template>
