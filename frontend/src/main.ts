import './assets/main.css'

import { createApp } from 'vue'

import App from './App.vue'
import router from './router'
import { pinia } from './stores'
import { useAuthStore } from './stores/auth'

const app = createApp(App)

app.use(pinia)

useAuthStore()
  .refresh()
  .finally(() => {
    app.use(router)
    app.mount('#app')
  })
