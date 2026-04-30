import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const role = ref<string | null>(null)
  const isLoggedIn = computed(() => !!token.value)

  function setSession(newToken: string, newRole: string) {
    token.value = newToken
    role.value = newRole
  }

  function clearSession() {
    token.value = null
    role.value = null
  }

  async function login(userName: string, password: string): Promise<boolean> {
    const res = await fetch('/api/Auth/login', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ UserName: userName, Password: password }),
    })
    if (!res.ok) return false
    const data = await res.json()
    setSession(data.token, data.role)
    return true
  }

  async function refresh(): Promise<boolean> {
    try {
      const res = await fetch('/api/Auth/refresh', {
        method: 'POST',
        credentials: 'include',
      })
      if (!res.ok) {
        clearSession()
        return false
      }
      const data = await res.json()
      setSession(data.token, data.role)
      return true
    } catch {
      clearSession()
      return false
    }
  }

  async function logout() {
    try {
      await fetch('/api/Auth/logout', {
        method: 'POST',
        credentials: 'include',
      })
    } catch {
      /* best-effort */
    }
    clearSession()
  }

  return { token, role, isLoggedIn, login, logout, refresh }
})
