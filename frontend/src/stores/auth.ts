import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))

  const isLoggedIn = computed(() => !!token.value)

  function login(newToken: string) {
    token.value = newToken
    localStorage.setItem('token', newToken)
    localStorage.setItem('isLoggedIn', 'true')
  }

  function logout() {
    token.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('isLoggedIn')
  }

  return { token, isLoggedIn, login, logout }
})
