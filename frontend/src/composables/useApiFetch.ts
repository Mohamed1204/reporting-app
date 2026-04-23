import { useFetch } from '@vueuse/core'
import type { UseFetchOptions } from '@vueuse/core'
import { useAuthStore } from '../stores/auth'
import router from '../router'

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.exp * 1000 < Date.now()
  } catch {
    return true
  }
}

export function useApiFetch<T = any>(url: string, options: UseFetchOptions = {}) {
  return useFetch(url, {
    ...options,
    beforeFetch({ options: fetchOptions }) {
      const token = localStorage.getItem('token')
      if (token) {
        fetchOptions.headers = {
          ...fetchOptions.headers,
          Authorization: `Bearer ${token}`,
        }
      }
      return { options: fetchOptions }
    },
  }).json<T>()
}

export async function apiFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const token = localStorage.getItem('token')
  const res = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers as Record<string, string> | undefined),
    },
  })

  if (res.status === 401) {
    useAuthStore().logout()
    router.push({ name: 'auth' })
  }

  return res
}
