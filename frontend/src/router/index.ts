import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { usePeriodsStore } from '../stores/periods'
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
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
  ],
})

router.beforeEach((to) => {
  if (to.name !== 'period-review') {
    return true
  }

  const periodsStore = usePeriodsStore(pinia)

  if (!periodsStore.hasSelectedPeriod) {
    return { name: 'home' }
  }

  return true
})

export default router
