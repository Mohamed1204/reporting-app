import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { usePeriodsStore } from '../stores/periods'
import { useAuthStore } from '../stores/auth'
import { pinia } from '../stores'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/periods/review',
      name: 'period-review',
      component: () => import('../views/PeriodReviewView.vue'),
    },
    {
      path: '/auth',
      name: 'auth',
      component: () => import('../views/LoginRegisterView.vue'),
    },
  ],
})

router.beforeEach((to) => {
  if (to.name === 'auth') return true

  const authStore = useAuthStore(pinia)
  if (!authStore.isLoggedIn) {
    return { name: 'auth' }
  }

  if (to.name === 'period-review') {
    const periodsStore = usePeriodsStore(pinia)
    if (!periodsStore.hasSelectedPeriod) {
      return { name: 'home' }
    }
  }
})

export default router
