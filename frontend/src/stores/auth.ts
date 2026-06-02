import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const role = ref<string | null>(null)
  const userName = ref<string | null>(null)
  const companyName = ref<string | null>(null)
  const isLoggedIn = computed(() => !!token.value)

  function setSession(
    newToken: string,
    newRole: string,
    newUserName: string,
    newCompanyName: string,
  ) {
    token.value = newToken
    role.value = newRole
    userName.value = newUserName
    companyName.value = newCompanyName
  }

  function clearSession() {
    token.value = null
    role.value = null
    userName.value = null
    companyName.value = null
  }

  async function login(userNameInput: string, password: string): Promise<boolean> {
    const res = await fetch('/api/Auth/login', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ UserName: userNameInput, Password: password }),
    })
    if (!res.ok) return false
    const data = await res.json()
    setSession(data.token, data.role, data.userName, data.companyName)
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
      setSession(data.token, data.role, data.userName, data.companyName)
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

  return { token, role, userName, companyName, isLoggedIn, login, logout, refresh }
})
